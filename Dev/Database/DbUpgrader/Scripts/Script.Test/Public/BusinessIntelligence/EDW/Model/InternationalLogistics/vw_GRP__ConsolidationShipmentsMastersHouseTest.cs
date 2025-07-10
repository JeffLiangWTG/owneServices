using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__ConsolidationShipmentsMastersHouse))]
	internal class vw_GRP__ConsolidationShipmentsMastersHouseTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, 1, 0, 2, 2);
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"JK",
				"Shipments",
				"Masters",
				"House"
			};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, int? key, int? shipments, int? masters, int? house)
		{
			var selectqry = string.Format("JK {0} AND Shipments {1} AND Masters {2} AND House {3}",
				key == null ? "IS NULL" : "= " + key,
				shipments == null ? "IS NULL" : "= " + shipments,
				masters == null ? "IS NULL" : "= " + masters,
				house == null ? "IS NULL" : "= " + house
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
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_GRP__ConsolidationShipmentsMastersHouse]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot](ConsolidationKey, ConsolidationID, ShipmentKey, ConsolidationShipmentPivotID, ConsolidationShipmentPivotKey)
															VALUES (1, null, 1, NEWID(), 1),
																   (1, NEWID(), 2, NEWID(),2 ),
																   (1, NEWID(), 2, NEWID(),3 )

				INSERT [{0}].[InternationalLogistics].[BAS__Shipment](ShipmentKey,ShipmentID, IsCancelled, ColoadMasterShipmentKey,ShipmentType )
															VALUES (1 ,NEWID(), 0, 2, 'STD'),
																   (2 ,NEWID(), 0, 1, 'ASM'),
																   (3 ,NEWID(), 0, 1, 'HLS')",
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
