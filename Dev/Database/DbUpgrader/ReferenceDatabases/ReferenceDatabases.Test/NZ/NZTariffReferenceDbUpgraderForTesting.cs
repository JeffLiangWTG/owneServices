using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.NZ.Testing
{
	sealed class NZTariffReferenceDbUpgraderForTesting : NZTariffReferenceDbUpgrader
	{
		public NZTariffReferenceDbUpgraderForTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
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
	}
}
