using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class EmptyDataSourceTest : TestCase
	{
		public void TestGetRowsFromIndexs()
		{
			var children = new EmptyDataSource();
			var newSource = children.GetRowsFromIndexes(new int[3] { 1, 3, 5 });
			AssertNull(newSource);
		}

		public void TestRowCount()
		{
			EmptyDataSource children = new EmptyDataSource();
			AssertEquals(0, children.RowCount);
		}

		public void TestGroupBy()
		{
			EmptyDataSource dS = new EmptyDataSource();
			AssertNull(dS.GroupBy(new string[] { "TestString" }));
		}

		public void TestSplit()
		{
			EmptyDataSource dS = new EmptyDataSource();
			AssertNull(dS.Split(10));
		}
	}
}
