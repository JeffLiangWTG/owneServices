using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Testing
{
	sealed class StrategyForTest : SharedAvailabilityGroupRefDbPreparationStrategy
	{
		public StrategyForTest(IUpgradeContext context, AdminConnection upgConnection, string mainDbName, int versionToUpgradeTo)
			: base(context, upgConnection, mainDbName, "ORDWP4-CP1AS1", RefDbTypeEnum.Customs, "ZZ", versionToUpgradeTo)
		{
		}

		public StrategyForTest(IUpgradeContext context, AdminConnection upgConnection, string mainDbName, RefDbTypeEnum refDbType, string refDbCountry, int versionToUpgradeTo)
			: base(context, upgConnection, mainDbName, "ORDWP4-CP1AS1", refDbType, refDbCountry, versionToUpgradeTo)
		{
		}

		public string VersionSharedRefDbPrefix_Exposed => VersionSharedRefDbPrefix;
		public string BaseDatabaseForUpgrade_Exposed => BaseDatabaseForUpgrade;
		public int VersionFromDatabase => ((IRefDbPreparationStrategy)this).GetVersionFromDatabase();

		public void DoCreateDatabase_Exposed(AdminConnection upgConnection)
		{
			DoCreateDatabase(upgConnection);
		}

		public void GetPhysicalDatabaseSettingsFromMasterDB_Exposed(AdminConnection adminConnection, out string dataPath, out string logPath)
		{
			GetPhysicalDatabaseSettingsFromMasterDB(out dataPath, out logPath, adminConnection);
		}

		public void GetPhysicalDatabaseSettingsFromPreviousRefDB_Exposed(AdminConnection adminConnection, out string dataPath, out string logPath)
		{
			GetPhysicalDatabaseSettingsFromPreviousRefDB(out dataPath, out logPath, adminConnection);
		}

		public void GetPhysicalDatabaseSettingsFromFirstRefDB_Exposed(AdminConnection adminConnection, out string dataPath, out string logPath)
		{
			GetPhysicalDatabaseSettingsFromFirstRefDB(out dataPath, out logPath, adminConnection);
		}

		public void GetPhysicalDatabaseSettingsFromMainDB_Exposed(AdminConnection adminConnection, out string dataPath, out string logPath)
		{
			GetPhysicalDatabaseSettingsFromMainDB(out dataPath, out logPath, adminConnection);
		}
	}
}
