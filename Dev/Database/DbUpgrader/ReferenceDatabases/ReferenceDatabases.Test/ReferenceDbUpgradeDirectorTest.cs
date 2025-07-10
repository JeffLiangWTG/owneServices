using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class ReferenceDbUpgradeDirectorTest : TestCase
	{
		public void TestCreateSingleRefDatabaseExistOnEDWServer()
		{
			using (var mainConnection = Db.NewAdminConnection())
			{
				mainConnection.BeginTransaction();
				var dwServerName = DbRegistry.BiDataWarehouseServer.LoadValue(mainConnection);
				using (var edwConnection = string.IsNullOrEmpty(dwServerName) ? Db.NewAdminConnection() : Db.NewAdminConnection(dwServerName, Db.SqlMasterDb))
				{
					var director = new ReferenceDbUpgradeDirector(upgradeContext.Object, edwConnection, logger.Object);
					AssertNoExceptionThrown(() => director.CreateSingleRefDatabases(testDbName));
					AssertEquals(true, edwConnection.DatabaseExists(testDbName));
					AdoTestUtils.DropDbIfExists(edwConnection, testDbName, Db.DatabaseName);
				}
			}
		}

		public void TestCreateSingleRefDatabaseOnSecondaryServers()
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();
				var director = new ReferenceDbUpgradeDirector_ForTest(upgradeContext.Object, connection, logger.Object);
				AssertNoExceptionThrown(() => director.CreateSingleRefDatabases(testDbName));
				logger.Verify(x => x.ShowTaskError(It.Is<string>(s => s.Contains("Fail to create SRDB on UnavailableServer_ForTest"))), Times.Once);
			}
		}

		public void TestCreateSingleRefDatabases()
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();
				var director = new ReferenceDbUpgradeDirector(upgradeContext.Object, connection, logger.Object);
				AssertNoExceptionThrown(() => director.CreateSingleRefDatabases(testDbName));
				AssertEquals(true, connection.DatabaseExists(testDbName));
			}
		}

		public void TestUpgradeSingleRefDatabaseIfThrowsOtherException()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var director = new ReferenceDbUpgradeDirectorThrowsOtherExceptionForTest(upgradeContext.Object, connection, logger.Object);
				logger.Invocations.Clear();
				AssertNoExceptionThrown(() => director.UpgradeSRDbWithLiveService(connection, RefDbTableNameResolver.DefaultSingleRefDbName));
				logger.Verify(x => x.ShowInfoMessage($"Warning: Cannot upgrade {RefDbTableNameResolver.SingleRefDatabaseName} this time, it will be retried later by RDU service task."), Times.Once);
				logger.Verify(x => x.ShowTaskError("Please use Help -> Database Administration -> Reference Data option and try again"), Times.Once);
				logger.Verify(x => x.ShowTaskError(It.Is<string>(error => error.Contains("Test Exception"))), Times.Once);
			}
		}

		public void TestUpgradeSingleRefDatabaseIfThrowsSqlException()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var director = new ReferenceDbUpgradeDirectorThrowsSqlExceptionForTest(upgradeContext.Object, connection, logger.Object);
				logger.Invocations.Clear();
				AssertExceptionThrown<SqlException>("Exception Thrown", () => director.UpgradeSRDbWithLiveService(connection, RefDbTableNameResolver.DefaultSingleRefDbName));
			}
		}

		[UseSnapshotProtection]
		public void TestUpgradeSingleRefDatabaseIfIsRunningOnDATIsFalse()
		{
			var originalIsRunningOnDAT = TestingState.IsRunningOnDAT;
			try
			{
				TestingState.IsRunningOnDAT = true;
				using (var connection = Db.NewAdminConnection())
				{
					var director = new ReferenceDbUpgradeDirectorThrowsSqlExceptionForTest(upgradeContext.Object, connection, logger.Object);
					logger.Invocations.Clear();
					AssertNoExceptionThrown(() => director.UpgradeSingleRefDatabase());
					logger.Verify(x => x.ShowInfoMessage($"Warning: Cannot upgrade {RefDbTableNameResolver.SingleRefDatabaseName} this time, it will be retried later by RDU service task."), Times.Never);
					AssertEquals("Y", DataUtils.LoadDbExtendedProperty(connection, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName));
				}
			}
			finally
			{
				TestingState.IsRunningOnDAT = originalIsRunningOnDAT;
			}
		}

		public void TestSRDbIsSimpleModelAfterCreated()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var director = new ReferenceDbUpgradeDirector(upgradeContext.Object, connection, logger.Object);
				director.CreateSingleRefDatabases(testDbName);
				AssertEquals(DbRecoveryModel.Simple, DbRecoveryModelManager.GetActual(connection, testDbName));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			upgradeContext = new Mock<IUpgradeContext>();
			logger = new Mock<IUpgradeTaskWorkflowLogger>();
		}

		protected override void TearDown()
		{
			base.TearDown();

			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, testDbName, Db.DatabaseName);
			}
		}

		Mock<IUpgradeContext> upgradeContext;
		Mock<IUpgradeTaskWorkflowLogger> logger;
		readonly string testDbName = RefDbTableNameResolver.SingleRefDatabaseName + "_Test";
	}
}
