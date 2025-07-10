using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__FCLConsolContainers))]
	internal class usp_IncLoad_GRP__FCLConsolContainersTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, new Guid("832C742D-3A1C-4DEE-BEA4-71159F4C5463"), 30001, 9000100.00);
			});
		}

		void AssertRowValues(DataTable resultTable, int? consolKey, Guid? consolPK, int? containerCount, double? totalTEU)
		{
			var selectqry = string.Format("ConsolKey {0} AND ConsolPK {1} AND ContainerCount {2} AND TotalTEU {3}",
				consolKey == null ? "IS NULL" : "= " + consolKey,
				consolPK == null ? "IS NULL" : "= '" + consolPK + "'",
				containerCount == null ? "IS NULL" : "= " + containerCount,
				totalTEU == null ? "IS NULL" : "= " + totalTEU
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__Container](ContainerID,ConsolidationID, ContainerKey, ConsolidationKey, ContainerCount, ReferenceContainerKey)
															VALUES (newid(),'832C742D-3A1C-4DEE-BEA4-71159F4C5463', 1, 1, 1, 1),
																   (newid(),'832C742D-3A1C-4DEE-BEA4-71159F4C5463', 2, 1, 30000, 2)

				INSERT [{0}].[InternationalLogistics].[BAS__ReferenceContainer](ReferenceContainerID, ReferenceContainerKey, TEU)
															VALUES (newid(), 1, 100.00),
																   (newid(), 2, 300.00)

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
					SELECT 'InternationalLogistics', 'BAS__Container', ContainerKey
					FROM [{0}].[InternationalLogistics].[BAS__Container]",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[GRP__FCLConsolContainers]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetIncLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [IncrementalLoadQuery] FROM [{0}].[biadmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'GRP__FCLConsolContainers'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			return incLoadSQLText;
		}

		void Execute()
		{
			var incLoadSQLText = GetIncLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + incLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
