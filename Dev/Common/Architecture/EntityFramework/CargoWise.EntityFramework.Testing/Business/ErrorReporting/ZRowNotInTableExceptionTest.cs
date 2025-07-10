using System.Data;
using CargoWise.Data;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZRowNotInTableExceptionTest : TestCaseWithFactory
	{
		public void TestRowNotInTableExceptionMessage()
		{
			var table = new DataTable("TestTable");
			table.PrimaryKey = new DataColumn[] { table.Columns.Add("TestColumn") };

			var row = table.Rows.Add(1);
			table.Rows.Add(2);

			row.AcceptChanges();

			var rowException = new RowNotInTableException();
			var exception = new ZRowNotInTableException(rowException, row, Db.Connection);

			Assert("Debug message should contain information on the current row column data", exception.Message.Contains("TestColumn = 1"));

			row["TestColumn"] = 20;

			exception = new ZRowNotInTableException(rowException, row, Db.Connection);
			Assert("Debug message should contain information on the current row column data", exception.Message.Contains("TestColumn = 20"));
			Assert("Debug message should still contain information on the row column data", exception.ExtraDebugInfo.Contains("TestColumn = 1"));

			row.Delete();

			exception = new ZRowNotInTableException(rowException, row, Db.Connection);
			Assert("Debug message should still contain information on the row column data", exception.ExtraDebugInfo.Contains("TestColumn = 1"));
			Assert("Message should include error table", exception.Message.Contains("TableName=TestTable"));
		}
	}
}
