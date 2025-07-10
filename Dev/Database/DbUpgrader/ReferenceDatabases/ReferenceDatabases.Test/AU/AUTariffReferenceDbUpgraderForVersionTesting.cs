using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.AU
{
	sealed class AUTariffReferenceDbUpgraderForVersionTesting : ReferenceDbUpgraderForVersionTesting
	{
		public AUTariffReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName
		{
			get { return "AU Tariff"; }
		}

		public override string CountryCode
		{
			get { return "AU"; }
		}

		public int GetActualLatestVersion(DbConnection conn)
		{
			return new AUTariffReferenceDbUpgrader(upgradeContext, conn, logger).LatestVersion;
		}
	}
}
