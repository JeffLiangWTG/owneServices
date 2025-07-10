using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class RefAirlineUpgradeTaskTest : TransactionedTestCase
	{
		public void TestUpdateMatchingRows()
		{
			string sql = "UPDATE dbo.RefAirline SET RM_AirlineName2 = '~AirlineName2' WHERE RM_PK = '80bfcac5-979f-4dae-8b1e-c1c76e572748'";
			Db.Connection.ExecuteNonQuery(sql);

			sql = "SELECT RM_AirlineName2 FROM dbo.RefAirline WHERE RM_PK = '80bfcac5-979f-4dae-8b1e-c1c76e572748'";
			string name2 = Db.Connection.ExecuteScalar(sql).ToString();
			AssertEquals("[PRE-CONDITION]", "~AirlineName2", name2);

			RefAirlineUpgradeTask task = new RefAirlineUpgradeTask();
			task.Run();

			sql = "SELECT RM_AirlineName2 FROM dbo.RefAirline WHERE RM_PK = '80bfcac5-979f-4dae-8b1e-c1c76e572748'";
			name2 = Db.Connection.ExecuteScalar(sql).ToString();
			AssertEquals("Value after DataUpgrade", "", name2);
		}

		public void TestDoNotDeleteUnmatchingRows()
		{
			string insertSql = "INSERT dbo.RefAirline (RM_PK, RM_AirlineName1, RM_AirlineName2) VALUES ('1E607E4A-8E5A-4DCE-8D2A-114F2F3F793C', '~Airline 1', '~UnmatchingRow')";
			Db.Connection.ExecuteNonQuery(insertSql);

			RefAirlineUpgradeTask task = new RefAirlineUpgradeTask();
			task.Run();

			DataFile tempFile = new EmbeddedDataFile("", "RefAirline");
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);

			DataRow[] unmatchingRows = data.Tables["RefAirline"].Select("RM_AirlineName2 = '~UnmatchingRow'");
			AssertEquals("Unmatching Airlines should NOT have been deleted", 1, unmatchingRows.Length);
			AssertEquals("Unmatching Airline Name", "~Airline 1", unmatchingRows[0]["RM_AirlineName1"]);
		}

		public void TestIsRequired()
		{
			RefAirlineUpgradeTask task = new RefAirlineUpgradeTask();
			task.ResourceFile.VersionInDatabase = 0;
			AssertEquals("initial database", true, task.IsRequired);
			task.ResourceFile.VersionInDatabase = 1;
			AssertEquals("updating database", false, task.IsRequired);
		}
	}
}
