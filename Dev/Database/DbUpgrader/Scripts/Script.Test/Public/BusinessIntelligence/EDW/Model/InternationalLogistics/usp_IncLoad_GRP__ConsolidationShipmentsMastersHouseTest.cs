using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__ConsolidationShipmentsMastersHouse))]
	internal class usp_IncLoad_GRP__ConsolidationShipmentsMastersHouseTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareInitialLoadData();
			ExecuteIniLoad();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 0, 2, 2);
			});

			LoadNewData();
			ExecuteIncLoad();

			resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 0, 4, 8);
			});
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

		void PrepareInitialLoadData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot](ConsolidationKey, ConsolidationID, ShipmentKey, ConsolidationShipmentPivotID, ConsolidationShipmentPivotKey )
															VALUES (1, null, 1, NEWID(), 1),
																   (1, NEWID(), 2, NEWID(),2 ),
																   (1, NEWID(), 2, NEWID(),3 )

				INSERT [{0}].[InternationalLogistics].[BAS__Shipment](ShipmentKey,ShipmentID, IsCancelled, ColoadMasterShipmentKey,ShipmentType )
															VALUES (1 ,NEWID(), 0, 2, 'STD'),
																   (2 ,NEWID(), 0, 1, 'ASM'),
																   (3 ,NEWID(), 0, 1, 'HLS')
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void LoadNewData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot](ConsolidationKey, ConsolidationID, ShipmentKey, ConsolidationShipmentPivotID, ConsolidationShipmentPivotKey )
															VALUES (1, null, 1, NEWID(), 1)

				INSERT [{0}].[InternationalLogistics].[BAS__Shipment](ShipmentKey,ShipmentID, IsCancelled, ColoadMasterShipmentKey,ShipmentType )
															VALUES (1 ,NEWID(), 0, 2, 'STD')

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
					SELECT 'InternationalLogistics', 'BAS__Shipment', ShipmentKey
					FROM [{0}].[InternationalLogistics].[BAS__Shipment]

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
					SELECT 'InternationalLogistics', 'BAS__ConsolidationShipmentPivot', ConsolidationShipmentPivotKey
					FROM [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot]
				",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[GRP__ConsolidationShipmentsMastersHouse]",
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
					SELECT [IncrementalLoadQuery] FROM [{0}].[biadmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'GRP__ConsolidationShipmentsMastersHouse'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			return incLoadSQLText;
		}

		string GetIniLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [InitialLoadQuery] FROM [{0}].[biadmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'GRP__ConsolidationShipmentsMastersHouse'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void ExecuteIniLoad()
		{
			var iniLoadSQLText = GetIniLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}

		void ExecuteIncLoad()
		{
			var incLoadSQLText = GetIncLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + incLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
