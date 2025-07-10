using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.StmData
{
	sealed class StmDataUpgradeTaskTest : TransactionedTestCase
	{
		public void TestRun()
		{
			// Prepare test data
			Guid existingStmDataPk = new Guid("9eea9b9f-ed54-418d-be24-dbc203bbb57e");
			Guid linkedGuid = new Guid();
			string existingStmDataName = "SOME_RANDOM_ACCOUNT";

			string query = String.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES (@PK, @SD_Name, @SD_GuidValue)",
				StmDataSchema.Constants.TableName,
				StmDataSchema.PK.Name,
				StmDataSchema.SD_Name.Name,
				StmDataSchema.SD_GuidValue.Name);

			AssertEquals("[INITIAL DATA] existingStmDataPk is not yet in DB", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, StmDataSchema.PK, existingStmDataPk));

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, existingStmDataPk);
				command.AddParameter("@SD_Name", SqlDbType.VarChar, existingStmDataName);
				command.AddParameter("@SD_GuidValue", SqlDbType.UniqueIdentifier, linkedGuid);
				command.ExecuteNonQuery();
			}

			AssertEquals("[BEFORE UPGRADE] existingStmDataPk is now in DB", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, StmDataSchema.PK, existingStmDataPk));
			AssertEquals("[BEFORE UPGRADE] existingStmData's name", existingStmDataName, BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, StmDataSchema.PK, existingStmDataPk, StmDataSchema.SD_Name));
			AssertEquals("[BEFORE UPGRADE] existingStmData's guid value", linkedGuid, BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, StmDataSchema.PK, existingStmDataPk, StmDataSchema.SD_GuidValue));

			StmDataUpgradeTask task = new StmDataUpgradeTask();
			task.Run();

			AssertEquals("[AFTER UPGRADE] existingStmDataPk is still in DB", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, StmDataSchema.PK, existingStmDataPk));
			AssertEquals("[AFTER UPGRADE] existingStmData's name stays the same", existingStmDataName, BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, StmDataSchema.PK, existingStmDataPk, StmDataSchema.SD_Name));
			AssertEquals("[AFTER UPGRADE] existingStmData's guid value stays the same", linkedGuid, BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, StmDataSchema.PK, existingStmDataPk, StmDataSchema.SD_GuidValue));
		}
	}
}
