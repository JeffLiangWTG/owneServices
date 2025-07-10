using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__ConsolShipmentPivot))]
	internal class usp_IncLoad_CUS__ConsolShipmentPivotTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, 1, 1, "CSHADCNO06A900683375", new Guid("CC825DAE-3D44-4BFC-8D72-91A001D82C14"), 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int? consolidationKey, int? consolidationShipmentPivotKey, string jobNumber, Guid? shipmentID, int? shipmentKey)
		{
			var selectqry = string.Format("ConsolidationKey {0} AND ConsolidationShipmentPivotKey  {1} AND JobNumber {2} AND ShipmentID {3} AND ShipmentKey {4}",
				consolidationKey == null ? "IS NULL" : "= '" + consolidationKey + "'",
				consolidationShipmentPivotKey == null ? "IS NULL" : "= '" + consolidationShipmentPivotKey + "'",
				jobNumber == null ? "IS NULL" : "= '" + jobNumber + "'",
				shipmentID == null ? "IS NULL" : "= '" + shipmentID + "'",
				shipmentKey == null ? "IS NULL" : "= '" + shipmentKey + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__Consolidation]
					([ConsolidationKey], [ConsolidationID], [JobNumber])
					VALUES
						(1, 'B302A255-DC49-4056-BBF7-B6E41B172C89', 'CSHADCNO06A900683375'),
						(2, '5F1355DA-82B2-436A-ACF0-665228B6F1F8', 'COEA0000683169');

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot]
					([ConsolidationShipmentPivotKey], [ConsolidationShipmentPivotID], [ShipmentKey], [ShipmentID], [ConsolidationKey], [ConsolidationID])
					VALUES
						(1, newid(), 1, 'CC825DAE-3D44-4BFC-8D72-91A001D82C14', 1, 'B302A255-DC49-4056-BBF7-B6E41B172C89'),
						(2, newid(), 2, '2A440808-3729-4226-8704-FCA0F21E8963', 3, '4A2055DA-82B2-436A-ACF0-665228B6F1F8')

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
					SELECT 'InternationalLogistics', 'BAS__Consolidation', ConsolidationKey 
					FROM [{0}].[InternationalLogistics].[BAS__Consolidation]",

				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[CUS__ConsolShipmentPivot]",
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
					"SELECT [IncrementalLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__ConsolShipmentPivot'",
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
