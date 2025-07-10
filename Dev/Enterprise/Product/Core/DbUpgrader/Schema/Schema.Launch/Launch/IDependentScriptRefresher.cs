using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	public interface IDependentScriptRefresher
	{
		void RefreshDependentScripts(DbConnection connection, string tableDb, string tableSchemaName, string tableName, IUpgradeManager manager);
	}
}
