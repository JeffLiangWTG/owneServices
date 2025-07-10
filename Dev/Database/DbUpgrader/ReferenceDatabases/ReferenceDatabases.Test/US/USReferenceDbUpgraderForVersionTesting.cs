using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class USReferenceDbUpgraderForVersionTesting : ReferenceDbUpgraderForVersionTesting
	{
		public USReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName
		{
			get { return "US"; }
		}

		public override string CountryCode
		{
			get { return "US"; }
		}

		public override void DoDataUpgrade_Exposed(DbConnection conn, int currentVersion, int latestVersion)
		{
			new USReferenceDbUpgraderForTesting(upgradeContext, conn, logger).DoDataUpgrade_Exposed(conn, currentVersion, latestVersion);
		}

		public int GetActualLatestVersion(DbConnection conn)
		{
			return new USReferenceDbUpgrader(upgradeContext, conn, logger).LatestVersion;
		}
	}
}
