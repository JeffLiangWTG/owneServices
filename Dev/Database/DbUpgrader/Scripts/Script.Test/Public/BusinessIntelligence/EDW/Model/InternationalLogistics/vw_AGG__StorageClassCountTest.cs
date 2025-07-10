using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_AGG__StorageClassCount))]
	internal class vw_AGG__StorageClassCountTest : BiCreateScriptTest
	{
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
				AssertRowValues(resultTable, new Guid("CE9B8D22-59E2-4E60-857A-00125449E19F"), 7, "20F", 1, "FCL", 1, 3, 3);
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
				{
					"ContainerID"
					, "ContainerKey"
					, "StorageClass"
					, "ContainerCount"
					, "ContainerMode"
					, "TEU"
					, "ContainerReferenceContainerKey"
					, "ReferenceContainerKey"
				};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, Guid? containerid, int? containerkey, string storageclass, int? containercount, string containermode, decimal? teu, int? containerreferencecontainerkey, int? referencecontainerkey)
		{
			var selectqry = string.Format("ContainerID {0} AND ContainerKey {1} AND StorageClass {2} AND ContainerCount {3} AND ContainerMode {4} AND TEU {5} AND ContainerReferenceContainerKey {6} AND ReferenceContainerKey {7}",
				containerid == null ? "IS NULL" : "= '" + containerid + "'"
				, containerkey == null ? "IS NULL" : "=" + containerkey
				, storageclass == null ? "IS NULL" : "= '" + storageclass + "'"
				, containercount == null ? "IS NULL" : "=" + containercount
				, containermode == null ? "IS NULL" : "= '" + containermode + "'"
				, teu == null ? "IS NULL" : "= '" + teu + "'"
				, containerreferencecontainerkey == null ? "IS NULL" : "=" + containerreferencecontainerkey
				, referencecontainerkey == null ? "IS NULL" : "=" + referencecontainerkey
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
					"SELECT *  FROM [{0}].[InternationalLogistics].[vw_AGG__StorageClassCount]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

					INSERT [{0}].[InternationalLogistics].[BAS__Container]([ContainerID], [ContainerKey], [ContainerCount], [ContainerMode], [ReferenceContainerID], [ReferenceContainerKey])
																	VALUES('CE9B8D22-59E2-4E60-857A-00125449E19F', 7, 1, 'FCL', 'AD809CB7-8691-4534-ADEA-38D0C17D4EC1', 3);

					INSERT [{0}].[InternationalLogistics].[BAS__ReferenceContainer]([ReferenceContainerID], [StorageClass], [TEU],[ReferenceContainerKey])
																	VALUES('AD809CB7-8691-4534-ADEA-38D0C17D4EC1', '20F', 1, 3)",
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
