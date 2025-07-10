using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	[HttpContextEnabledTest]
	[UseSnapshotProtection]
	class ZGlobalUpgradeTest : TestCase
	{
		public void TestCheckDBVersionMatches_AfterDatabaseUpgraded_HandleDatabaseUpgradedExceptions()
		{
			var globalMock = new Mock<ZGlobal>() { CallBase = true };
			var httpApplication = globalMock.Object;

			using (var adminConnection = Db.NewAdminConnection())
			{
				try
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => TryOpenNewExtraConnection());

					// Arrange
					BumpUpSchemaVersion(adminConnection);
					adminConnection.ResetLockout();
					AssertExceptionThrown<DatabaseUpgradedException>(() => TryOpenNewExtraConnection());

					// Act
					// Assert
					AssertNoExceptionThrown(() => _ = httpApplication.DBVersionMatches);
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}

		public void TestCheckDBVersionMatches_DuringDatabaseUpgradeInProgress_ThrowsDatabaseUpgradeInProgressExceptions()
		{
			// Arrange
			var globalMock = new Mock<ZGlobal>() { CallBase = true };
			var httpApplication = globalMock.Object;
			using (var adminConnection = Db.NewAdminConnection())
			{
				AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
				DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
				AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => TryOpenNewExtraConnection());

				try
				{
					// Act
					// Assert
					AssertExceptionThrown<DatabaseUpgradeInProgressException>(() => _ = httpApplication.DBVersionMatches);
				}
				finally
				{
					adminConnection.ResetLockout();
				}
			}
		}

		void TryOpenNewExtraConnection()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnsureIsOpen();
			}
		}

		void BumpUpSchemaVersion(DbConnection connection)
		{
			const string sql = @"
UPDATE dbo.StmData SET
SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), (SELECT CONVERT(int, CONVERT(nvarchar(max), SD_BinaryValue))) + 1))
WHERE 1 = 1
AND SD_Name = 'DATABASE_SCHEMA_VERSION'
AND SD_Owner is NULL
AND SD_DepartmentGuid is NULL
";
			connection.ExecuteNonQuery(sql);
		}
	}
}
