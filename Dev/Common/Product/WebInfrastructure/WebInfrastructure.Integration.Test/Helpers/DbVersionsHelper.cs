using System;
using CargoWise.Data;
using Enterprise.Upgrades;
using static System.FormattableString;

namespace CargoWiseOne.WebInfrastructure.Integration.Test.Helpers
{
	static class DbVersionsHelper
	{
		public static UpgradeInfo QueryCurrentVersion()
		{
			var sqlConnection = ((IDbConnectionInternals)Db.Connection).ADOConnection;
			var upgradeManager = new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database));
			return upgradeManager.QueryCurrentVersion();
		}

		public static Version IncrementPackageVersion(Version oldVersion)
		{
			var newVersion = new Version(oldVersion.Major, oldVersion.Minor + 1, oldVersion.Build, oldVersion.Revision);

			using var disposable = Db.DisableSchemaVersionCheck();
			using var adminConnection = Db.NewAdminConnection();

			return 1 == adminConnection.ExecuteNonQuery(Invariant($@"
UPDATE [StmUpgrade]
SET
	SZ_MinorVersion = {newVersion.Minor}
WHERE 1=1
	AND SZ_Status = 'CUR'
"))
				? newVersion
				: oldVersion;
		}

		public static Version IncrementSchemaMajorVersion()
		{
			return UpdateSchemaMajorVersion(1);
		}

		public static Version DecrementSchemaMajorVersion()
		{
			return UpdateSchemaMajorVersion(-1);
		}

		public static Version LoadDatabaseSchemaVersion()
		{
			using var disposable = Db.DisableSchemaVersionCheck();
			using var adminConnection = Db.NewAdminConnection();

			return new Version(
				DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection),
				DbRegistry.DatabaseMinorSchemaVersion.LoadValue(adminConnection)
			);
		}

		static Version UpdateSchemaMajorVersion(int diff)
		{
			using var disposable = Db.DisableSchemaVersionCheck();
			using var adminConnection = Db.NewAdminConnection();

			var initialMajorVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection);
			var initialMinorVersion = DbRegistry.DatabaseMinorSchemaVersion.LoadValue(adminConnection);

			var updatedMajorVersion = initialMajorVersion + diff;
			DbRegistry.DatabaseMajorSchemaVersion.SaveValue(updatedMajorVersion, adminConnection);

			return new Version(updatedMajorVersion, initialMinorVersion);
		}
	}
}
