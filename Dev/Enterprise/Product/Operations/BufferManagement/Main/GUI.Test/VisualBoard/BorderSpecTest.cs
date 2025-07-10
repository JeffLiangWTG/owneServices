using System.Windows.Forms;
using Enterprise.BufferManagement.Business.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BorderSpecTest : BMSTestCaseWithFactory
	{
		public void TestThicknesses_AllSidesSame()
		{
			var border = new BorderSpec("4");

			AssertEquals(4, border.LeftThickness);
			AssertEquals(4, border.TopThickness);
			AssertEquals(4, border.RightThickness);
			AssertEquals(4, border.BottomThickness);
		}

		public void TestThicknesses_AllSidesDifferent()
		{
			var border = new BorderSpec("4 3 2  1  ");

			AssertEquals(4, border.LeftThickness);
			AssertEquals(3, border.TopThickness);
			AssertEquals(2, border.RightThickness);
			AssertEquals(1, border.BottomThickness);
		}

		public void TestThicknesses_AlphanumericSides()
		{
			var border = new BorderSpec("4 3a 2  1  ");

			AssertEquals(4, border.LeftThickness);
			AssertEquals(0, border.TopThickness);
			AssertEquals(2, border.RightThickness);
			AssertEquals(1, border.BottomThickness);
		}

		public void TestThicknesses_TooManySides()
		{
			var border = new BorderSpec("4 3 2  1 1 ");

			AssertEquals(4, border.LeftThickness);
			AssertEquals(3, border.TopThickness);
			AssertEquals(2, border.RightThickness);
			AssertEquals(1, border.BottomThickness);
		}

		public void TestBorderStyle()
		{
			var border = new BorderSpec("0 0 5 4");
			border.SetBorderStyles(ButtonBorderStyle.Dotted);

			AssertEquals(ButtonBorderStyle.None, border.LeftBorderStyle);
			AssertEquals(ButtonBorderStyle.None, border.TopBorderStyle);
			AssertEquals(ButtonBorderStyle.Dotted, border.RightBorderStyle);
			AssertEquals(ButtonBorderStyle.Dotted, border.BottomBorderStyle);
		}
	}
}
