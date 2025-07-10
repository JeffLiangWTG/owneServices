using System.Data;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ReadOnlyDataAdapterTest : TestCase
	{
		public void TestFillDataSetShouldUseStringInterner()
		{
			AssertFillDataSetShouldUseStringInterner(0);
			AssertFillDataSetShouldUseStringInterner(10);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Test only")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Test only")]
		void AssertFillDataSetShouldUseStringInterner(int timeoutSeconds)
		{
			const string Query = @"
SELECT 'A'
UNION ALL
SELECT 'B'
UNION ALL
SELECT 'A'

SELECT 'A'
UNION ALL
SELECT 'B'
UNION ALL
SELECT 'C'
";

			var dataSet = new DataSet();

			using (var command = Db.Connection.Command(Query))
			using (var adapter = command.NewDataAdapter(timeoutSeconds))
			{
				adapter.Fill(dataSet);
			}

			var t1r1 = (string)dataSet.Tables[0].Rows[0][0];
			var t1r2 = (string)dataSet.Tables[0].Rows[1][0];
			var t1r3 = (string)dataSet.Tables[0].Rows[2][0];
			AssertEquals("A", t1r1);
			AssertEquals("B", t1r2);
			AssertEquals("A", t1r3);
			Assert("same context string in same table should have same reference", object.ReferenceEquals(t1r1, t1r3));

			var t2r1 = (string)dataSet.Tables[1].Rows[0][0];
			var t2r2 = (string)dataSet.Tables[1].Rows[1][0];
			var t2r3 = (string)dataSet.Tables[1].Rows[2][0];
			AssertEquals("A", t2r1);
			AssertEquals("B", t2r2);
			AssertEquals("C", t2r3);
			Assert("same context string in diff tables should have same reference", object.ReferenceEquals(t1r1, t2r1));
			Assert("same context string in diff tables should have same reference", object.ReferenceEquals(t1r2, t2r2));

			CombineAssertions("all DataRowState should be Unchanged", () =>
			{
				foreach (DataTable table in dataSet.Tables)
				{
					foreach (DataRow row in table.Rows)
					{
						AssertEquals(DataRowState.Unchanged, row.RowState);
					}
				}
			});
		}

		public void TestFillDoesnotProduceDuplicateRowsAfterDisconnect()
		{
			const string Query = @"
SELECT 'A'
UNION ALL
SELECT 'B'
UNION ALL
SELECT 'C'";

			var table = new DataTable();
			table.RowChanged += TableRowChangedWithDbConnectionDrop;

			using (var command = Db.Connection.Command(Query))
			using (var adapter = command.NewDataAdapter())
			{
				adapter.Fill(table);
			}

			AssertEquals(3, table.Rows.Count);
		}

		void TableRowChangedWithDbConnectionDrop(object sender, DataRowChangeEventArgs e)
		{
			var table = (DataTable)sender;
			if (table.Rows.Count == 2)
			{
				table.RowChanged -= TableRowChangedWithDbConnectionDrop;
				Db.Connection.CloseConnection();
			}
		}
	}
}
