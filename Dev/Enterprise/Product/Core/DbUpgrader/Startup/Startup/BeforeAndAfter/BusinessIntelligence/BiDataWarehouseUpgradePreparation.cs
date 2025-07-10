using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Startup
{
	class BiDataWarehouseUpgradePreparation : BusinessIntelligenceUpgradePreparation
	{
		public BiDataWarehouseUpgradePreparation(AdminConnection mainDbConnection, AdminConnection biConnection, IVersionChangeInfo upgVersionInfo)
			: base(mainDbConnection, biConnection, upgVersionInfo)
		{
		}

		protected override string BiDatabaseName => mainDbName + Db.EdwDatabaseSuffix;
		protected override string BiDatabaseType => "EDW";

		protected override bool MustRecreateBiDatabase
		{
			get
			{
				return
					upgVersionInfo != null
					&& upgVersionInfo.IsRequired_Schema
					&& upgVersionInfo.DbReferenceVersion_Schema.CompareTo(new VersionLabel(7346, 0)) < 0
				;
			}
		}
	}
}
