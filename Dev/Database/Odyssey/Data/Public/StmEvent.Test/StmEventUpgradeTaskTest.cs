using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class StmEventUpgradeTaskTest : TransactionedTestCase
	{
		public void TestMixOfInsertUpdateDelete()
		{
			string sqlText = "DELETE dbo.StmEvent WHERE SE_Code = 'ADD'";
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = "UPDATE dbo.StmEvent SET SE_Desc = 'Must change to: Edited a record' WHERE SE_Code = 'EDT'";
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = "INSERT dbo.StmEvent (SE_PK, SE_Code, SE_Desc, SE_SystemCreateTimeUtc, SE_SystemCreateUser, SE_SystemLastEditTimeUtc, SE_SystemLastEditUser) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC3', '000', 'Test Event 000', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(sqlText);
			sqlText = "UPDATE dbo.StmEvent SET SE_Desc = 'Must not be updated' WHERE SE_Code = 'Z00'";
			Db.Connection.ExecuteNonQuery(sqlText);

			var task = new StmEventUpgradeTask(new StmEventDataFile());
			task.Run();

			var tempFile = new StmEventDataFile();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);

			DataRow[] unmatchingRows = data.Tables["StmEvent"].Select("SE_Code = '000'");
			AssertEquals("Unmatching Events should have been deleted", 0, unmatchingRows.Length);

			DataRow[] reInsertedEvents = data.Tables["StmEvent"].Select("SE_Code = 'ADD'");
			AssertEquals("ADD event should have been re-inserted", 1, reInsertedEvents.Length);

			DataRow[] updatedEvents = data.Tables["StmEvent"].Select("SE_Code = 'EDT'");
			AssertEquals("EDT event description should have been changed back", "Edited a record", updatedEvents[0]["SE_Desc"].ToString());

			DataRow[] customizableUpdatedEvents = data.Tables["StmEvent"].Select("SE_Code = 'Z00'");
			AssertEquals("Z00 event description should not have been changed back", "Must not be updated", customizableUpdatedEvents[0]["SE_Desc"].ToString());
		}
	}
}
