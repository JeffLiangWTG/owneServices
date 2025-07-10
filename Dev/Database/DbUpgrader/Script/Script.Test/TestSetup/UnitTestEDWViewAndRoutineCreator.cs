using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script.Test.TestSetup
{
	class UnitTestEDWViewAndRoutineCreator : EdwViewAndRoutineCreator
	{
		public static UnitTestEDWViewAndRoutineCreator Test(IUpgradeManager manager, DbConnection upgConnection, string mainDbName)
		{
			string edwDatabaseName = mainDbName + Db.EdwDatabaseSuffix;
			return new UnitTestEDWViewAndRoutineCreator(manager, upgConnection, edwDatabaseName);
		}

		public UnitTestEDWViewAndRoutineCreator(IUpgradeManager manager, DbConnection upgConnection, string mainDbName)
			: base(manager, upgConnection, mainDbName)
		{
		}

		protected override void AddBiScriptsToCollection(DbRoutineScriptCollection scriptCollection, IEnumerable<IDbScript> biScriptCollection)
		{
			var testScriptCollection = new DbRoutineScriptCollection
			{
				new DbScript("VW_Test_View_EDW_01", "CREATE VIEW VW_Test_View_EDW_01 AS SELECT 0 Col1", "VIEW")
			};

			foreach (IDbScript biScript in testScriptCollection)
			{
				scriptCollection.Add(biScript);
			}
		}
	}
}
