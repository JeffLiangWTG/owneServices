using System.Drawing;
using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ExcelImageTest : TestCase
	{
		public void TestToString()
		{
			ExcelImage excelImage = new ExcelImage(new Bitmap(1, 2), 10, 20, "Master Piece");
			AssertEquals("excelImage.ToString", @"Master Piece (20x10)", excelImage.ToString());
		}

		public void TestIsAspectRatioLocked()
		{
			ExcelImage excelImage = new ExcelImage(new Bitmap(1, 2), 10, 20, "Piece of Resistance");
			AssertEquals(false, excelImage.IsAspectRatioLocked);

			excelImage = new ExcelImage(new Bitmap(1, 1), 10, 20, "Piece of Resistance", true);
			AssertEquals(true, excelImage.IsAspectRatioLocked);
		}

		public void TestIsDimensionInPixel()
		{
			ExcelImage excelImage = new ExcelImage(new Bitmap(1, 2), 10, 20, "Piece of Resistance");
			AssertEquals(false, excelImage.IsDimensionInPixel);

			excelImage = new ExcelImage(new Bitmap(1, 1), 10, 20, "Piece of Resistance", true, true);
			AssertEquals(true, excelImage.IsDimensionInPixel);
		}
	}
}
