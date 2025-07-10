using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__FCLConsolContainers))]
	internal class vw_GRP__FCLConsolContainersTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = GetColumns();
			TestColumnsAreAsExpected(ScriptToTest.Name, columns);

			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, new Guid("832C742D-3A1C-4DEE-BEA4-71159F4C5463"), 30001, 9000100.00);
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"ConsolKey",
				"ConsolPK",
				"ContainerCount",
				"TotalTEU"
			};

			return columns;
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

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_GRP__FCLConsolContainers]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[InternationalLogistics].[BAS__Container](ContainerID,ConsolidationID, ContainerKey, ConsolidationKey, ContainerCount, ReferenceContainerKey)
															VALUES (newid(),'832C742D-3A1C-4DEE-BEA4-71159F4C5463', 1, 1, 1, 1),
																   (newid(),'832C742D-3A1C-4DEE-BEA4-71159F4C5463', 2, 1, 30000, 2)

				INSERT [{0}].[InternationalLogistics].[BAS__ReferenceContainer](ReferenceContainerID, ReferenceContainerKey, TEU)
															VALUES (newid(), 1, 100.00),
																   (newid(), 2, 300.00)",
			ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
