using System.ComponentModel;
using Android.Graphics;
using Android.Graphics.Drawables;
using AButton = Android.Widget.Button;
using ACanvas = Android.Graphics.Canvas;
using GlobalResource = Android.Resource;

namespace Xamarin.Forms.Platform.Android
{
	public class FrameRenderer : VisualElementRenderer<Frame>
	{
		bool _disposed;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && !_disposed)
			{
				Background.Dispose();
				_disposed = true;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null && e.OldElement == null)
				UpdateBackground();
		}

		void UpdateBackground()
		{
			this.SetBackground(new FrameDrawable(Element));
		}

		class FrameDrawable : Drawable
		{
			readonly Frame _frame;

			bool _isDisposed;
			Bitmap _normalBitmap;

			public FrameDrawable(Frame frame)
			{
				_frame = frame;
				frame.PropertyChanged += FrameOnPropertyChanged;
			}

			public override bool IsStateful
			{
				get { return false; }
			}

			public override int Opacity
			{
				get { return 0; }
			}

			public override void Draw(ACanvas canvas)
			{
				int width = Bounds.Width();
				int height = Bounds.Height();

				if (width <= 0 || height <= 0)
				{
					if (_normalBitmap != null)
					{
						_normalBitmap.Dispose();
						_normalBitmap = null;
					}
					return;
				}

				if (_normalBitmap == null || _normalBitmap.Height != height || _normalBitmap.Width != width)
				{
					// If the user changes the orientation of the screen, make sure to detroy reference before
					// reassigning a new bitmap reference.
					if (_normalBitmap != null)
					{
						_normalBitmap.Dispose();
						_normalBitmap = null;
					}

					_normalBitmap = CreateBitmap(false, width, height);
				}
				Bitmap bitmap = _normalBitmap;
				using (var paint = new Paint())
					canvas.DrawBitmap(bitmap, 0, 0, paint);
			}

			public override void SetAlpha(int alpha)
			{
			}

			public override void SetColorFilter(ColorFilter cf)
			{
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !_isDisposed)
				{
					if (_normalBitmap != null)
					{
						_normalBitmap.Dispose();
						_normalBitmap = null;
					}

					_isDisposed = true;
				}

				base.Dispose(disposing);
			}

			protected override bool OnStateChange(int[] state)
			{
				return false;
			}

			Bitmap CreateBitmap(bool pressed, int width, int height)
			{
				Bitmap bitmap;
				using (Bitmap.Config config = Bitmap.Config.Argb8888)
					bitmap = Bitmap.CreateBitmap(width, height, config);

				using (var canvas = new ACanvas(bitmap))
				{
					DrawBackground(canvas, width, height, pressed);
					DrawOutline(canvas, width, height);
				}

				return bitmap;
			}

			void DrawBackground(ACanvas canvas, int width, int height, bool pressed)
			{
				using (var paint = new Paint { AntiAlias = true })
				using (var path = new Path())
				using (Path.Direction direction = Path.Direction.Cw)
				using (Paint.Style style = Paint.Style.Fill)
				using (var rect = new RectF(0, 0, width, height))
				{
					float rx = Forms.Context.ToPixels(5);
					float ry = Forms.Context.ToPixels(5);
					path.AddRoundRect(rect, rx, ry, direction);

					global::Android.Graphics.Color color = _frame.BackgroundColor.ToAndroid();

					paint.SetStyle(style);
					paint.Color = color;

					canvas.DrawPath(path, paint);
				}
			}

			void DrawOutline(ACanvas canvas, int width, int height)
			{
				using (var paint = new Paint { AntiAlias = true })
				using (var path = new Path())
				using (Path.Direction direction = Path.Direction.Cw)
				using (Paint.Style style = Paint.Style.Stroke)
				using (var rect = new RectF(0, 0, width, height))
				{
					float rx = Forms.Context.ToPixels(5);
					float ry = Forms.Context.ToPixels(5);
					path.AddRoundRect(rect, rx, ry, direction);

					paint.StrokeWidth = 1;
					paint.SetStyle(style);
					paint.Color = _frame.OutlineColor.ToAndroid();

					canvas.DrawPath(path, paint);
				}
			}

			void FrameOnPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == Frame.OutlineColorProperty.PropertyName)
				{
					using (var canvas = new ACanvas(_normalBitmap))
					{
						int width = Bounds.Width();
						int height = Bounds.Height();
						canvas.DrawColor(global::Android.Graphics.Color.Black, PorterDuff.Mode.Clear);
						DrawBackground(canvas, width, height, false);
						DrawOutline(canvas, width, height);
					}
					InvalidateSelf();
				}
			}
		}
	}
}