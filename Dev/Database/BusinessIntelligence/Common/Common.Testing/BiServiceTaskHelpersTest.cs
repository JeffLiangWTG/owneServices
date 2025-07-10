using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace CargoWise.Bi.Common.Testing
{
	class BiServiceTaskHelpersTest : TestCase
	{
		public void TestIsBusinessIntelligenceEnabled()
		{
			var requirementMessage = BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();
			AssertEquals("IsBusinessIntelligenceEnabled() should return empty string.", string.Empty, requirementMessage);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestIsBusinessIntelligenceEnabled_OnlyAuditDatabaseIsUpdated()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				SetBiDatabaseVersion(mainDbConnection, Db.EdwDatabaseName, "0.0");
				var requirementMessage = BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();
				AssertEquals("IsBusinessIntelligenceEnabled() should return empty string.", string.Empty, requirementMessage);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestIsBusinessIntelligenceEnabled_OnlyEdwDatabaseIsUpdated()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				SetBiDatabaseVersion(mainDbConnection, Db.AuditDatabaseName, "0.0");
				var requirementMessage = BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();
				AssertEquals("IsBusinessIntelligenceEnabled() should return empty string.", string.Empty, requirementMessage);
			}
		}

		[UseSnapshotProtection]
		public void TestIsEdwEnabledHandlesException()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			using (mainDbConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
			{
				BiServers.SaveEdwServer(mainDbConnection, "bing bong");
				var requirementMessage = BiServiceTaskHelpers.IsEdwEnabled();
				AssertContains("SaveEdwServer() should return the exception message.", "An exception was thrown when trying to connect to the DataWarehouse server:", requirementMessage);
			}
		}

		[UseSnapshotProtection]
		public void TestIsAuditEnabledHandlesException()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			using (mainDbConnection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
			{
				BiServers.SaveAuditServer(mainDbConnection, "bing bong");
				var requirementMessage = BiServiceTaskHelpers.IsAuditEnabled();
				AssertContains("SaveAuditServer() should return the exception message.", "An exception was thrown when trying to connect to the Audit server:", requirementMessage);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestIsBusinessIntelligenceEnabled_NoBiDatabasesUpdated()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				SetBiDatabaseVersion(mainDbConnection, Db.AuditDatabaseName, "0.0");
				SetBiDatabaseVersion(mainDbConnection, Db.EdwDatabaseName, "0.0");
				var requirementMessage = BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();
				CombineAssertions("IsBusinessIntelligenceEnabled() should return requirement message.", () =>
				{
					AssertContains($"[{Db.AuditDatabaseName}] is not updated. Expected version: {SchemaVersion.Application}, Actual version: 0.0", requirementMessage);
					AssertContains($"[{Db.EdwDatabaseName}] is not updated. Expected version: {SchemaVersion.Application}, Actual version: 0.0", requirementMessage);
				});
			}
		}

		public void TestIsAuditEnabled()
		{
			var requirementMessage = BiServiceTaskHelpers.IsAuditEnabled();
			AssertEquals("IsAuditEnabled() should return empty string.", string.Empty, requirementMessage);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestIsAuditEnabled_AuditIsNotUpdated()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				SetBiDatabaseVersion(mainDbConnection, Db.AuditDatabaseName, "0.0");
				var requirementMessage = BiServiceTaskHelpers.IsAuditEnabled();
				AssertEquals("IsAuditEnabled() should return requirement message.", $"[{Db.AuditDatabaseName}] is not updated. Expected version: {SchemaVersion.Application}, Actual version: 0.0", requirementMessage);
			}
		}

		public void TestIsEdwEnabled()
		{
			var requirementMessage = BiServiceTaskHelpers.IsEdwEnabled();
			AssertEquals("IsEdwEnabled() should return empty string.", string.Empty, requirementMessage);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.EDW })]
		public void TestIsEdwEnabled_AuditIsNotUpdated()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				SetBiDatabaseVersion(mainDbConnection, Db.EdwDatabaseName, "0.0");
				var requirementMessage = BiServiceTaskHelpers.IsEdwEnabled();
				AssertEquals("IsEdwEnabled() should return requirement message.", $"[{Db.EdwDatabaseName}] is not updated. Expected version: {SchemaVersion.Application}, Actual version: 0.0", requirementMessage);
			}
		}

		public void TestIsAnalysisServerSet()
		{
			AssertEquals("IsEdwEnabled() should return empty string.", string.Empty, BiServiceTaskHelpers.IsAnalysisServerSet());
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestIsBusinessIntelligenceEnabledWithMainDbSchemaVersion()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(SchemaVersion.Application.Major, mainDbConnection);
				DbRegistry.DatabaseMinorSchemaVersion.SaveValue(SchemaVersion.Application.Minor, mainDbConnection);

				var databaseMajorSchemaVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(mainDbConnection);
				var databaseMinorSchemaVersion = DbRegistry.DatabaseMinorSchemaVersion.LoadValue(mainDbConnection);

				SetBiDatabaseVersion(mainDbConnection, Db.EdwDatabaseName, "1.2");
				SetBiDatabaseVersion(mainDbConnection, Db.AuditDatabaseName, "1.2");

				var requirementMessage = BiServiceTaskHelpers.IsBusinessIntelligenceEnabled();
				AssertContains($"[{Db.AuditDatabaseName}] is not updated. Expected version: {databaseMajorSchemaVersion}.{databaseMinorSchemaVersion}, Actual version: 1.2", requirementMessage);
				AssertContains($"[{Db.EdwDatabaseName}] is not updated. Expected version: {databaseMajorSchemaVersion}.{databaseMinorSchemaVersion}, Actual version: 1.2", requirementMessage);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		[ExpectNoExceptions]
		public void TestIsAuditEnableWithoutLoginPermission()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				var loginName = DataProtection.RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);
				using (((ICurrentDbControl)adminConnection).UseDatabase(Db.AuditDatabaseName))
				{
					SqlSecurityUtils.Login.Drop(adminConnection, loginName);
				}
				AssertNoExceptionThrown(() => BiServiceTaskHelpers.IsAuditEnabled());
			}
		}

		public void TestCheckAuditEnabledForHostedServiceRequirementsWithoutAuditServer()
		{
			using (BiServers.TemporarilySetAuditServerToNull())
			{
				AssertEquals("Audit server should be empty", "No Audit server set in the registry.", BiServiceTaskHelpers.CheckAuditEnabledForHostedServiceRequirements());
			}
		}

		public void TestCheckEdwEnabledForHostedServiceRequirementsWithoutEdwServer()
		{
			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				AssertEquals("EDW server should be empty", "No DataWarehouse server set in the registry.", BiServiceTaskHelpers.CheckEdwEnabledForHostedServiceRequirements());
			}
		}

		#region Implementation

		void SetBiDatabaseVersion(DbConnection connection, string dbName, string version)
		{
			DataUtils.SaveDbExtendedProperty(connection, BiConstants.MainDbSchemaVersionExtPtyName, version, dbName);
		}

		#endregion
	}
}
