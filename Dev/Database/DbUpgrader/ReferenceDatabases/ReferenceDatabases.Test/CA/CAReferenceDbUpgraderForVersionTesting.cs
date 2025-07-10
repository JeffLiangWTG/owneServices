using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	sealed class CAReferenceDbUpgraderForVersionTesting : ReferenceDbUpgraderForVersionTesting
	{
		public CAReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName
		{
			get { return "CA"; }
		}

		public override string CountryCode
		{
			get { return "CA"; }
		}

		public int GetActualLatestVersion(DbConnection conn)
		{
			return new CAReferenceDbUpgrader(upgradeContext, conn, logger).LatestVersion;
		}

		public override void DoDataUpgrade_Exposed(DbConnection conn, int currentVersion, int latestVersion)
		{
			new CAReferenceDbUpgraderForTesting(upgradeContext, conn, logger).DoDataUpgrade_Exposed(conn, currentVersion, latestVersion);
		}
	}
}
