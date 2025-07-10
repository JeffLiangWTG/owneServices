using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class USReferenceDbUpgraderForTesting : USReferenceDbUpgrader
	{
		public USReferenceDbUpgraderForTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public void DoDataUpgrade_Exposed(DbConnection conn, int currentVersion, int latestVersion)
		{
			latestVersionOverride = latestVersion;
			base.DoDataUpgrade(conn, currentVersion);
		}

		public override int LatestVersion
		{
			get { return (latestVersionOverride == null) ? base.LatestVersion : latestVersionOverride.Value; }
		}
		public int? latestVersionOverride;

		public void CopyPrevisoinVersion(int versionBeforeUpgrade, string previousRefDbName, string currentRefDbName)
		{
			base.CopyDataFromVersionBeforeUpgradeCore(versionBeforeUpgrade, previousRefDbName, currentRefDbName);
		}
	}
}
