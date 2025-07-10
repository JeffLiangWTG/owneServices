using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class TableColumnTest : TestCase
	{
		public void TestConstructor()
		{
			TableColumn tc = new TableColumn("Table", "Column");
			AssertEquals("Table", tc.TableName);
			AssertEquals("Column", tc.ColumnName);
		}
	}
}
