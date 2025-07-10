#if !WINZOR
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CargoWise.Main.Screenshot
{
	public static class ScreenshotHelper
	{
		[DllImport("user32.dll")]
		static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);

		public static Bitmap CaptureControlWithPrintWindow(Control ctrl)
		{
			var bmp = new Bitmap(ctrl.Width, ctrl.Height);
			using (var g = Graphics.FromImage(bmp))
			{
				var hdc = g.GetHdc();
				PrintWindow(ctrl.Handle, hdc, 0);
				g.ReleaseHdc(hdc);
			}
			return bmp;
		}

		public static Bitmap CaptureWpfElement(FrameworkElement element)
		{
			double dpi = 96;
			var rtb = new RenderTargetBitmap(
				(int)element.ActualWidth,
				(int)element.ActualHeight,
				dpi, dpi,
				PixelFormats.Pbgra32);

			var dv = new DrawingVisual();
			using (var ctx = dv.RenderOpen())
			{
				ctx.DrawRectangle(new VisualBrush(element), null, new Rect(new System.Windows.Size(element.ActualWidth, element.ActualHeight)));
			}
			rtb.Render(dv);

			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(rtb));

			using var ms = new MemoryStream();
			encoder.Save(ms);
			return new Bitmap(ms);
		}

		public static ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
		{
			using (var memory = new MemoryStream())
			{
				bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
				memory.Position = 0;

				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();
				bitmapImage.Freeze();

				return bitmapImage;
			}
		}
	}
}
#endif
