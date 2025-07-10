using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class ExcelCellRangeTest : TestCase
	{
		public void TestConstruction()
		{
			var range = new ExcelCellRange(1, 2, 3, 4);
			AssertEquals("range.Top", 1, range.Top);
			AssertEquals("range.Left", 2, range.Left);
			AssertEquals("range.Bottom", 3, range.Bottom);
			AssertEquals("range.Right", 4, range.Right);
		}
	}
}
