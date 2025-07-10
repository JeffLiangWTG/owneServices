namespace Enterprise.DocumentEngine.Areas.Testing
{
	abstract class TreeDataAreaAbstractTest : AreaAbstractTest
	{
		public void TestUpdateRowRangesWithIndex()
		{
			var treeArea = GetNewAreaToTest() as TreeDataArea;

			InitializeRowRangesWithIndex(treeArea);
			treeArea.UpdateRowRangesWithIndex(5, 10);
			Assert(treeArea.DataRowIndexWithRowRanges[0].Start == 1 && treeArea.DataRowIndexWithRowRanges[0].End == 20);
			Assert(treeArea.DataRowIndexWithRowRanges[1].Start == 21 && treeArea.DataRowIndexWithRowRanges[1].End == 30);
			Assert(treeArea.DataRowIndexWithRowRanges[2].Start == 31 && treeArea.DataRowIndexWithRowRanges[2].End == 40);

			InitializeRowRangesWithIndex(treeArea);
			treeArea.UpdateRowRangesWithIndex(11, 5);
			Assert(treeArea.DataRowIndexWithRowRanges[0].Start == 1 && treeArea.DataRowIndexWithRowRanges[0].End == 10);
			Assert(treeArea.DataRowIndexWithRowRanges[1].Start == 11 && treeArea.DataRowIndexWithRowRanges[1].End == 25);
			Assert(treeArea.DataRowIndexWithRowRanges[2].Start == 26 && treeArea.DataRowIndexWithRowRanges[2].End == 35);

			InitializeRowRangesWithIndex(treeArea);
			treeArea.UpdateRowRangesWithIndex(20, 5);
			Assert(treeArea.DataRowIndexWithRowRanges[0].Start == 1 && treeArea.DataRowIndexWithRowRanges[0].End == 10);
			Assert(treeArea.DataRowIndexWithRowRanges[1].Start == 11 && treeArea.DataRowIndexWithRowRanges[1].End == 25);
			Assert(treeArea.DataRowIndexWithRowRanges[2].Start == 26 && treeArea.DataRowIndexWithRowRanges[2].End == 35);
		}

		public void TestGetRowIndex()
		{
			var treeArea = GetNewAreaToTest() as TreeDataArea;
			InitializeRowRangesWithIndex(treeArea);
			AssertEquals(0, treeArea.GetRowIndex(2));
			AssertEquals(1, treeArea.GetRowIndex(12));
			AssertEquals(2, treeArea.GetRowIndex(22));
			AssertEquals(-1, treeArea.GetRowIndex(220));
		}

		void InitializeRowRangesWithIndex(TreeDataArea area)
		{
			area.DataRowIndexWithRowRanges.Clear();
			area.DataRowIndexWithRowRanges[0] = new RowRange(1, 10);
			area.DataRowIndexWithRowRanges[1] = new RowRange(11, 20);
			area.DataRowIndexWithRowRanges[2] = new RowRange(21, 30);
		}
	}
}
