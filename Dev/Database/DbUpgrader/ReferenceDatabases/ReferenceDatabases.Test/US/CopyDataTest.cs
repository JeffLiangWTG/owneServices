using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class CopyDataTest : TestCase
	{
		public void TestCopyDataWithoutEmptyCode()
		{
			var upgrader = new USReferenceDbUpgraderForTesting(upgradeContext, testConnection, logger);
			const string testMockOldSharedRefDb = "CW-RefDb-Cmr-US-000093";
			const string testMockNewSharedRefDb = "CW-RefDb-Cmr-US-000102";

			try
			{
				CreateDatabaseAndTable(testMockOldSharedRefDb, 93);
				CreateDatabaseAndTable(testMockNewSharedRefDb, 102);
				InsertData(testMockOldSharedRefDb, "01", "X", "Y", "TestDesc1");
				InsertData(testMockOldSharedRefDb, "", "X2", "Y2", "TestDesc2");
				upgrader.CopyPrevisoinVersion(0, testMockOldSharedRefDb, testMockNewSharedRefDb);

				Assert((int)testConnection.ExecuteScalar(string.Format("SELECT count(*) FROM [{0}].dbo.[USCScheduleB] WHERE UB_Code = '{1}'", testMockNewSharedRefDb, "01")) == 1);
				Assert((int)testConnection.ExecuteScalar(string.Format("SELECT count(*) FROM [{0}].dbo.[USCScheduleB] WHERE UB_Code = '{1}'", testMockNewSharedRefDb, "")) == 0);

				string sqlText = string.Format("Delete from [{0}].dbo.[USCScheduleB]", testMockNewSharedRefDb);
				testConnection.ExecuteNonQuery(sqlText);
				InsertData(testMockNewSharedRefDb, "  ", "X3", "Y3", "TestDesc3");
				AssertNoExceptionThrown(() => upgrader.CopyPrevisoinVersion(0, testMockOldSharedRefDb, testMockNewSharedRefDb));
				Assert((int)testConnection.ExecuteScalar(string.Format("SELECT count(*) FROM [{0}].dbo.[USCScheduleB] WHERE UB_Code = '{1}'", testMockNewSharedRefDb, "01")) == 1);
				Assert((int)testConnection.ExecuteScalar(string.Format("SELECT count(*) FROM [{0}].dbo.[USCScheduleB] WHERE UB_Code = '{1}'", testMockNewSharedRefDb, "")) == 1);
			}
			finally
			{
				using (var adminCnx = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
					AdoTestUtils.DropDbIfExists(adminCnx, testMockNewSharedRefDb, Db.DatabaseName);
				}

				DbCommitTracker.Ignore(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				DbCommitTracker.Ignore(testMockOldSharedRefDb);
				DbCommitTracker.Ignore(testMockNewSharedRefDb);
			}
		}

		void InsertData(string testMockOldSharedRefDb, string code, string unit1, string unit2, string desc)
		{
			var sqlText = string.Format(@"INSERT INTO [{0}].dbo.[USCScheduleB] (UB_PK, UB_Code, UB_Unit1, UB_Unit2, UB_ShortDescription)
							values(newid(), '{1}', '{2}', '{3}', '{4}')", testMockOldSharedRefDb, code, unit1, unit2, desc);
			testConnection.ExecuteNonQuery(sqlText);
		}

		void CreateDatabaseAndTable(string testMockOldSharedRefDb, int version)
		{
			using (var adminCnx = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				AdoTestUtils.DropDbIfExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
				AdoTestUtils.CreateDbIfNotExists(adminCnx, SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb);
				AdoTestUtils.CreateDbIfNotExists(adminCnx, testMockOldSharedRefDb, Db.DatabaseName);
			}

			using (var upgConnection = Db.NewAdminConnection(SharedDatabasePreparationStrategyWithMockMainDbForTesting.TestMockDb))
			{
				var testStrategy = new SharedDatabasePreparationStrategyWithMockMainDbForTesting(upgradeContext, upgConnection, version);
				AssertEquals("RefDbName", testMockOldSharedRefDb, testStrategy.RefDbName);
				UpgCommandRunner.RunCommandOnGivenDb(upgConnection, testMockOldSharedRefDb, USCScheduleBCreationScript);
			}
		}

		const string USCScheduleBCreationScript = @"CREATE TABLE USCScheduleB (UB_PK uniqueidentifier CONSTRAINT PK_USCScheduleB PRIMARY KEY NONCLUSTERED,UB_Code varchar (10) NOT NULL ,UB_Unit1 varchar (3) NULL CONSTRAINT DF_USCScheduleB_UB_StatisticalUnit1 DEFAULT (''),UB_Unit2 varchar (3) NULL CONSTRAINT DF_UB_StatisticalUnit2 DEFAULT (''),UB_ShortDescription varchar (50) NULL CONSTRAINT DF_USCScheduleB_UB_ShortDescription DEFAULT (''))
CREATE UNIQUE INDEX NR_IX__UB_Code ON USCScheduleB(UB_Code)";

		protected override void SetUp()
		{
			testConnection.BeginTransaction();
			logger = new UpgradeTaskWorkflowLoggerTestClass();
			upgradeContext = new Mock<IUpgradeContext>().Object;
			base.SetUp();
		}

		readonly DbConnection testConnection = Db.NewExtraConnectionToMainDb();
		UpgradeTaskWorkflowLoggerTestClass logger;
		IUpgradeContext upgradeContext;
	}
}
