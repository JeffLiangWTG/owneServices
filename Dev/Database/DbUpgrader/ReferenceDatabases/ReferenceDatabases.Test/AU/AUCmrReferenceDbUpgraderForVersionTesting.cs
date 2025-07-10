using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.AU
{
	sealed class AUCmrReferenceDbUpgraderForVersionTesting : ReferenceDbUpgraderForVersionTesting
	{
		public AUCmrReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override string ReferenceName
		{
			get { return "AU CMR"; }
		}

		public override string CountryCode
		{
			get { return "AU"; }
		}

		public int GetActualLatestVersion(DbConnection conn)
		{
			return new AUCmrReferenceDbUpgrader(upgradeContext, conn, logger).LatestVersion;
		}
	}
}
