using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbSnapshotTest : TransactionedTestCase
	{
		public void TestSnapshot()
		{
			var snap1 = DbSnapshot.TakeSnapshot("snap1", TestConnection);
			var snap2 = DbSnapshot.TakeSnapshot("snap2", TestConnection);

			AssertEquals("difference between snap1 and snap2", "", snap2.Diff(snap1, new List<string>()));

			using (var command = TestConnection.Command("INSERT dbo.StmData (SD_PK, SD_Name) VALUES ('5F8E002E-62D9-42A8-A6AA-20C797967BFB', 'TestSnapshot')"))
			{
				command.ExecuteNonQuery();
			}

			var snap3 = DbSnapshot.TakeSnapshot("snap3", TestConnection);
			AssertEquals("difference between snap2 and snap3",
				string.Format(
					"Table: StmData, snap3: {0}, snap2: {1}, Diff: 1\r\n",
					snap3.TableRecordCount["StmData"].RowCount, snap2.TableRecordCount["StmData"].RowCount),
				snap3.Diff(snap2, new List<string>()));
			AssertEquals("difference between snap2 and snap3 ignoring StmData", "", snap3.Diff(snap2, new List<string> { "StmData" }));
		}

		public void TestGetTableUsageInfoWithBigTable()
		{
			var tableUsage = DbSnapshot.GetTableUsageInfo(TestConnection, @"
				INSERT INTO #SnapshotResult(TableName, Rows, Reserved, Data, IndexSize, Unused)
					Values('TestTableX', '3111222333        ', '3222333444 KB     ', '3333444555 KB     ', '3444555666 KB     ', '3555666777 KB     ')

				");

			Assert("Check debug info correctly added", tableUsage.ContainsKey("TestTableX"));

			var testTableXUsage = tableUsage["TestTableX"];

			AssertEquals(3111222333, testTableXUsage.RowCount);
			AssertEquals(3222333444, testTableXUsage.ReservedKB);
			AssertEquals(3333444555, testTableXUsage.DataKB);
			AssertEquals(3444555666, testTableXUsage.IndexSizeKB);
			AssertEquals(3555666777, testTableXUsage.UnusedKB);
		}
	}
}
