using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script
{
	class EdwModelScriptCollectionTest : TestCase
	{
		public void TestEdwModelScriptsConsistentWithConfiguration()
		{
			var modelScripts = CoreEdwScriptIndex.GetModelScripts();

			CombineAssertions(() =>
			{
				//EDW Model Objects (views)
				foreach (var modelView in BiAutomationConfigLoader.Instance.ConfigData.EdwModelViewTableConfig)
				{
					AssertScriptExists(modelScripts, DbRoutineType.SqlViewTypeDesc, modelView.Schema, modelView.Name);
				}

				// Denormalised Views & Initial Load Stored Procedures
				foreach (var denormTable in BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig)
				{
					AssertScriptExists(modelScripts, DbRoutineType.SqlViewTypeDesc, denormTable.Schema, BiConstants.EdwAggregateViewPrefix + denormTable.Name);
					AssertScriptExists(modelScripts, DbRoutineType.SqlProcedureTypeDesc, denormTable.Schema, BiConstants.EdwAggregateInitialLoadProcPrefix + denormTable.Name);
				}
			});
		}

		static void AssertScriptExists(IEnumerable<IDbScript> scriptCollection, string type, string schemaName, string scriptName)
		{
			Assert(string.Format("Collection should contain {0} [{1}].[{2}]", type, schemaName, scriptName),
				scriptCollection.Count(s =>
					s.ObjectType == type
					&& s.SchemaName == schemaName
					&& s.Name == scriptName
					) == 1
			);
		}
	}
}

