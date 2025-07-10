using System.IO;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	public class StmEventDataFileTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataFileExists()
		{
			var dataFile = new StmEventDataFile();
			Assert(File.Exists(dataFile.FileFullPath));
		}

		public void TestStmEventDataFile()
		{
			DataHelpers.ClearTable("StmEvent");

			string insertSql = @"
				INSERT dbo.StmEvent (SE_PK, SE_Code, SE_Desc, SE_SystemCreateTimeUtc, SE_SystemCreateUser, SE_SystemLastEditTimeUtc, SE_SystemLastEditUser) VALUES ('07C017B7-0817-4714-8285-3F3B165F9DC3', '000', 'Test Event 000', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
				INSERT dbo.StmEvent (SE_PK, SE_Code, SE_Desc, SE_SystemCreateTimeUtc, SE_SystemCreateUser, SE_SystemLastEditTimeUtc, SE_SystemLastEditUser) VALUES ('183EF542-B544-4121-8686-6EF7755E918C', '111', 'Test Event 111', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
			Db.Connection.ExecuteNonQuery(insertSql);

			StmEventDataFile file = new StmEventDataFile();
			var data = file.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals("StmEvent row count", 2, data.Tables["StmEvent"].Rows.Count);
			AssertEquals("Code - Row 0", "000", data.Tables["StmEvent"].Rows[0]["SE_Code"].ToString());
			AssertEquals("Code - Row 1", "111", data.Tables["StmEvent"].Rows[1]["SE_Code"].ToString());
		}
	}
}
