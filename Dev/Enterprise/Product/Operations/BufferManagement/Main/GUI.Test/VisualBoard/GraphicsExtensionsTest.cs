using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class GraphicsExtensionsTest : TestCase
	{
		public void TestGenerateExceptionWithExtendedMessage()
		{
			var innerEx = new ArgumentException("Some inner exception message") { Source = "System.Drawing" };
			var image = new Bitmap(200, 100, PixelFormat.Format32bppPArgb);
			var destRect = new Rectangle(1, 2, 3, 4);

			var exceptionThrown = false;
			try
			{
				GraphicsExtensions.GenerateExceptionWithExtendedMessage(innerEx, image, destRect, 5, 6, 7, 8, GraphicsUnit.Pixel, new ImageAttributes());
			}
			catch (ArgumentException ex) when (ex.Source.Contains("System.Drawing"))
			{
				exceptionThrown = true;
				AssertEquals("System.Drawing", ex.Source);
				AssertEquals(@"Cannot draw image on the graphics object.
image.IsDisposed: False,
image.Width: 200,
image.Height: 100,
destRect: {X=1,Y=2,Width=3,Height=4},
srcX: 5,
srcY: 6,
srcWidth: 7,
srcHeight: 8,
srcUnit: Pixel,
imageAttr: System.Drawing.Imaging.ImageAttributes", ex.Message);
				AssertEquals("Should carry the inner exception just in case", innerEx, ex.InnerException);
			}
			Assert("Exception should throw", exceptionThrown);
		}
	}
}
