using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class ZStringArrayDataSourceTest : TestCase
	{
		public void TestGetRowsFromIndexs()
		{
			var collection = new ZString[] { "a", "b", "c", "d", "e" };
			var arrayDataSource = new ZStringArrayDataSource("Children", collection);
			AssertEquals(5, arrayDataSource.RowCount);
			var newSource = arrayDataSource.GetRowsFromIndexes(new int[] { 1, 3, 5 });
			AssertEquals(3, newSource.RowCount);
		}

		public void TestRowCount()
		{
			var collection = new ZString[] { "1", "2", "3", "4", "5" };
			var children = new ZStringArrayDataSource("Children", collection);
			AssertEquals(5, children.RowCount);
		}

		public void TestSplit()
		{
			var collection = new ZString[] { "1", "2", "3", "4", "5" };
			var ds1 = new ZStringArrayDataSource("Children", collection);
			var ds2 = ds1.Split(1);
			AssertEquals(1, ds1.RowCount);
			AssertEquals(4, ds2.RowCount);

			IDataRowSource ds3 = null;
			AssertNoExceptionThrown("No OverflowException is thrown out", () => ds3 = ds2.Split(6));
			AssertEquals(4, ds2.RowCount);
			AssertEquals(0, ds3.RowCount);
		}

		public void TestGetFirstNRows()
		{
			var collection = new ZString[] { "1", "2", "3", "4", "5" };
			var ds1 = new ZStringArrayDataSource("Children", collection);
			var ds2 = ds1.GetFirstNRows(1);
			AssertEquals(5, ds1.RowCount);
			AssertEquals(1, ds2.RowCount);
		}

		public void TestGetFirstNRowsWithMoreRowsThanInDataSource()
		{
			var collection = new ZString[] { "1", "2", "3", "4", "5" };
			var ds1 = new ZStringArrayDataSource("Children", collection);
			var ds2 = ds1.GetFirstNRows(10);
			AssertEquals(5, ds1.RowCount);
			AssertEquals(10, ds2.RowCount);
		}
	}
}
