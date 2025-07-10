using CargoWise.Data;

namespace Enterprise.DbUpgrader.Startup
{
	class BiDataWarehouseUpgradeConclusion : BusinessIntelligenceUpgradeConclusion
	{
		public BiDataWarehouseUpgradeConclusion(AdminConnection mainDbConnection, AdminConnection biConnection)
			: base(mainDbConnection, biConnection)
		{
		}

		protected override string BiDatabaseType => "EDW";
		protected override string BiDatabaseName => Db.EdwDatabaseName;
	}
}
