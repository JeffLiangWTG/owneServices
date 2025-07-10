using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	abstract class ReferenceDbUpgraderForVersionTesting : ReferenceDbUpgrader
	{
		public ReferenceDbUpgraderForVersionTesting(IUpgradeContext upgradeContext, DbConnection upgradeConnection, IUpgradeTaskWorkflowLogger logger)
			: base(upgradeContext, upgradeConnection, logger)
		{
		}

		public override int LatestVersion
		{
			get { return (latestVersionOverride == null) ? LatestVersion : latestVersionOverride.Value; }
		}
		public int? latestVersionOverride;

		protected override void DoDataUpgrade(DbConnection conn, int versionBeforeUpgrade)
		{
		}

		public virtual void DoDataUpgrade_Exposed(DbConnection conn, int currentVersion, int latestVersion)
		{
		}

		public override RefDbTypeEnum DatabaseType
		{
			get { return RefDbTypeEnum.Enterprise; }
		}

		protected override void UpgradeDatabase(DbConnection conn, int versionBeforeUpgrade)
		{
			DoDataUpgrade_Exposed(conn, versionBeforeUpgrade, LatestVersion);
			EnsureColumnSchemaRequirements(conn);
			UpdateVersionToLatest(conn);
		}
	}
}
