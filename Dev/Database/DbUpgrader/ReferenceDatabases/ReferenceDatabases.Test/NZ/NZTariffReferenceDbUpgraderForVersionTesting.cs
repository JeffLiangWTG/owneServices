using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.NZ.Testing
{
	sealed class NZTariffReferenceDbUpgraderForVersionTesting : ReferenceDbUpgraderForVersionTesting
	{
		public NZTariffReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName
		{
			get { return "NZ Tariff"; }
		}

		public override string CountryCode
		{
			get { return "NZ"; }
		}

		public int GetActualLatestVersion(DbConnection conn)
		{
			return new NZTariffReferenceDbUpgrader(upgradeContext, conn, logger).LatestVersion;
		}

		public override void DoDataUpgrade_Exposed(DbConnection conn, int currentVersion, int latestVersion)
		{
			var upgrader = new NZTariffReferenceDbUpgraderForTesting(upgradeContext, conn, logger);
			upgrader.DoDataUpgrade_Exposed(conn, currentVersion, latestVersion);
		}
	}
}
