using CargoWise.Data;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	class DependentScriptRefresher : IDependentScriptRefresher
	{
		public void RefreshDependentScripts(DbConnection connection, string tableDb, string tableSchemaName, string tableName, IUpgradeManager manager)
			=> TablePreSynchroniser.RefreshDependentScripts(connection, tableDb, tableSchemaName, tableName, manager);
	}
}
