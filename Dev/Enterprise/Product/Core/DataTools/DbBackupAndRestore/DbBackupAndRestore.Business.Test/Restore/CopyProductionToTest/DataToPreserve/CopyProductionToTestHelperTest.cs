using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class CopyProductionToTestHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestGetStmData()
		{
			var testDbName = "TestDbC9D38634206B8500A9C9B2EEFER53412RT";
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				try
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
					AssertEquals("Database exists?", false, connection.DatabaseExists(testDbName));
					CopyProductionToTestHelper.GetStmData(connection, testDbName, "SD_Name");

					AdoTestUtils.CreateDbIfNotExists(connection, testDbName);
					CopyProductionToTestHelper.GetStmData(connection, testDbName, "SD_Name");

					var sqlScript = $"Create Table {testDbName}.dbo.StmData (i int)";
					using (var cmd = connection.Command(sqlScript))
					{
						cmd.ExecuteNonQuery();
					}
					CopyProductionToTestHelper.GetStmData(connection, testDbName, "SD_Name");

					using (var conn2 = Db.NewAdminConnection(testDbName))
					{
						sqlScript = $@"Drop Table StmData;
								Create Synonym StmData for NonExistingTable";

						using (var cmd = conn2.Command(sqlScript))
						{
							cmd.ExecuteNonQuery();
						}
					}
					CopyProductionToTestHelper.GetStmData(connection, testDbName, "SD_Name");
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(connection, testDbName);
				}
			}
		}
	}
}
