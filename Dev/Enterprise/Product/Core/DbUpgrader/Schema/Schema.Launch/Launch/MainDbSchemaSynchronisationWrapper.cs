using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Upgrades the main CW1 (Odyssey) database schema during a software upgrade.
	/// </summary>
	public class MainDbSchemaSynchronisationWrapper : SchemaSynchronisationWrapper
	{
		public MainDbSchemaSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
			: base(manager, dbToUpgrade, upgConnection)
		{
		}

		public void RefreshDependentScripts(IDependentScriptRefresher dependentScriptRefresher)
		{
			var modifiedTables = SchemaSynchroniser.DistinctModifiedTablesIgnoringCase.ToList();
			if (modifiedTables.Count != 0)
			{
				Manager.StartTask($"Refreshing dependent scripts of {modifiedTables.Count} table(s)");
				foreach (var table in modifiedTables)
				{
					dependentScriptRefresher.RefreshDependentScripts(UpgConnection, DbBeingUpgraded, table.SchemaName, table.TableName, Manager);
				}
			}
		}
	}
}
