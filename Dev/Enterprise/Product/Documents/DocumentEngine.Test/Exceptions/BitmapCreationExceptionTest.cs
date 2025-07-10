using System;
using System.Drawing.Imaging;
using CargoWise.BrandManager;

namespace Enterprise.DocumentEngine.Exceptions.Testing
{
	sealed class BitmapCreationExceptionTest : ExceptionTestCase<BitmapCreationException>
	{
		public void TestConstructor()
		{
			BitmapCreationException ex = new BitmapCreationException(new ArgumentException("Test Message 1"), 1000, 2000, 300, PixelFormat.Format24bppRgb);
			AssertEquals("BitmapCreationException.Width", 1000, ex.Width);
			AssertEquals("BitmapCreationException.Height", 2000, ex.Height);
			AssertEquals("BitmapCreationException.Resolution", 300, (int)ex.Resolution);
			AssertEquals("BitmapCreationException.PixFmt", PixelFormat.Format24bppRgb, ex.PixFmt);
			AssertNotNull("InnerException should not be null", ex.InnerException);
			Assert("InnerException should be an ArgumentException", ex.InnerException is ArgumentException);
			AssertEquals("Test Message 1", "Test Message 1", ex.InnerException.Message);
		}

		public void TestMessage()
		{
			BitmapCreationException ex = new BitmapCreationException(new ArgumentException(), 1000, 2000, 300, PixelFormat.Format24bppRgb);
			AssertEquals(@$"Bitmap creation failed. 
Your system might be low on memory or system resources, please close all other tasks, exit {BrandingFactory.Instance.ProductName}, come back in and try again.
Parameters are: 
  Width = 1000
  Height = 2000
  Resolution = 300
  Pixel Format = Format24bppRgb", ex.Message);
		}

		public void TestIsDimensionOutsideRange()
		{
			BitmapCreationException ex = new BitmapCreationException(new ArgumentException(), 10000, 10000, 300, PixelFormat.Format24bppRgb);
			AssertEquals("Dimension is out of range", true, ex.IsDimensionOutsideRange);

			ex = new BitmapCreationException(new ArgumentException(), 0, 0, 300, PixelFormat.Format24bppRgb);
			AssertEquals("Dimension is out of range", true, ex.IsDimensionOutsideRange);

			ex = new BitmapCreationException(new ArgumentException(), 1000, 1000, 300, PixelFormat.Format24bppRgb);
			AssertEquals("Dimension is inside range", false, ex.IsDimensionOutsideRange);
		}

		protected override BitmapCreationException GetNewExceptionToTest(string message)
		{
			return new BitmapCreationException(new Exception(), 1, 1, 1.45f, PixelFormat.Alpha);
		}
	}
}
