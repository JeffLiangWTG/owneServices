using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class ExternalRequestTypeUpgradeTaskTest : TransactionedTestCase
	{
		public void TestDataTables()
		{
			task.Run();

			var tempFile = new ExternalRequestTypeDataFileForTest();
			var data = tempFile.LoadDataFromDatabase();

			AssertEquals("Table Count", 1, data.Tables.Count);
			AssertEquals(ExternalRequestTypeSchema.Constants.TableName, data.Tables[0].TableName);
			AssertNotEquals(0, data.Tables[0].Rows.Count);
		}

		public void TestWhenGenericExternalRequestTypeIsCreated()
		{
			var addDefaultRequestType = $@"
INSERT INTO dbo.ExternalRequestType (RQT_PK, RQT_Code, RQT_Description, RQT_JobType, RQT_IsSystem, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES ('007e8627-31fd-470c-a604-8d440213031b', 'GEN', 'Not Generic', 'ALL', 1, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP');
INSERT INTO dbo.ExternalRequestType (RQT_PK, RQT_Code, RQT_Description, RQT_JobType, RQT_SystemCreateTimeUtc, RQT_SystemCreateUser, RQT_SystemLastEditTimeUtc, RQT_SystemLastEditUser)
VALUES (NEWID(), 'AAA', 'AAA Type', 'ALL', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

			Db.Connection.ExecuteNonQuery(addDefaultRequestType);

			string assertSql = "SELECT COUNT(1) FROM dbo.ExternalRequestType WHERE RQT_Code = 'GEN'";
			AssertEquals("Has Existing ExternalRequestType", 1, Db.Connection.ExecuteScalar(assertSql));

			task.Run();

			assertSql = "SELECT COUNT(1) FROM dbo.ExternalRequestType WHERE RQT_Code = 'GEN' and RQT_Description = 'Generic'";
			AssertEquals("UpgradeTask should update existing value", 1, Db.Connection.ExecuteScalar(assertSql));

			assertSql = "SELECT COUNT(1) FROM dbo.ExternalRequestType WHERE RQT_Code = 'AAA'";
			AssertEquals("UpgradeTask should not update other value", 1, Db.Connection.ExecuteScalar(assertSql));
		}

		protected override void SetUp()
		{
			base.SetUp();

			DataHelpers.ClearTable(ExternalRequestTypeSchema.Constants.TableName);

			task = new ExternalRequestTypeUpgradeTask();
		}

		EmbeddedUpgradeTask task;
	}
}