using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA
{
	sealed class CATariffReferenceDbUpgraderForVersionTesting : ReferenceDbUpgraderForVersionTesting
	{
		public CATariffReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName
		{
			get { return "CA EdiTariff"; }
		}

		public override string CountryCode
		{
			get { return "CA"; }
		}

		public int GetActualLatestVersion(DbConnection conn)
		{
			return new CATariffReferenceDbUpgrader(upgradeContext, conn, logger).LatestVersion;
		}
	}
}
