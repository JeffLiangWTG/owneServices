#if !WINZOR
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using CargoWise.Main.Screenshot;
using NUnit.Framework;

namespace CargoWise.Main.Test.Screenshot
{
	public class ScreenshotHelperTest : TestCase
	{
		public void TestWinformsControlScreenshot()
		{
			using var form = new Form();
			form.Text = "Test Window";
			form.BackColor = Color.Green;
			form.Width = 300;
			form.Height = 200;
			form.StartPosition = FormStartPosition.Manual;
			form.Location = new System.Drawing.Point(100, 100);

			form.Show();
			form.Refresh();

			var bmp = ScreenshotHelper.CaptureControlWithPrintWindow(form);

			AssertNotNull(bmp);
			AssertEquals(300, bmp.Width);
			AssertEquals(200, bmp.Height);

			var pixel = bmp.GetPixel(10, 10);
			AssertEquals(Color.FromArgb(255, 159, 186, 214), pixel);

			pixel = bmp.GetPixel(100, 100);
			AssertEquals(Color.FromArgb(255, 0, 128, 0), pixel);

			form.Close();
		}

		[RequiresSTA]
		public void TestWpfElementScreenshot()
		{
			var border = new Border
			{
				Width = 200,
				Height = 200,
				Background = System.Windows.Media.Brushes.Green,
			};

			var window = new Window
			{
				Content = border,
				Width = 200,
				Height = 200,
				WindowStyle = WindowStyle.SingleBorderWindow
			};
			window.Show();
			window.UpdateLayout();
			border.UpdateLayout();

			var bmp = ScreenshotHelper.CaptureWpfElement(border);

			AssertNotNull(bmp);
			AssertEquals(200, bmp.Height);
			AssertEquals(200, bmp.Width);

			var pixel = bmp.GetPixel(100, 100);
			AssertEquals(Color.FromArgb(255, 0, 128, 0), pixel);
		}
	}
}
#endif
