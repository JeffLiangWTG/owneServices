using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class RowRangeListTest : TestCase
	{
		public void TestAdd()
		{
			RowRangeList testRowRangeList = new RowRangeList();
			AssertEquals(0, testRowRangeList.Count);
			testRowRangeList.Add(new RowRange(1, 1));
			testRowRangeList.Add(new RowRange(3, 3));
			testRowRangeList.Add(new RowRange(5, 5));
			AssertEquals(3, testRowRangeList.Count);
		}

		public void TestAddMergesRanges()
		{
			RowRangeList testRowRangeList = new RowRangeList();
			AssertEquals(0, testRowRangeList.Count);
			testRowRangeList.Add(new RowRange(1, 1));
			testRowRangeList.Add(new RowRange(2, 2));
			testRowRangeList.Add(new RowRange(3, 3));
			AssertEquals(1, testRowRangeList.Count);
			AssertEquals(1, testRowRangeList[0].Start);
			AssertEquals(3, testRowRangeList[0].End);
		}
	}
}
