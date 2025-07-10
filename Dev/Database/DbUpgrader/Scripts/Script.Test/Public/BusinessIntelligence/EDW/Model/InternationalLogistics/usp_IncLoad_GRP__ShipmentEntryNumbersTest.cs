using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__ShipmentEntryNumbers))]
	internal class usp_IncLoad_GRP__ShipmentEntryNumbersTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, new Guid("7878DF80-2340-4C6A-83B4-006D7F465737"), "CAN ACGYPAYL9");
				AssertRowValues(resultTable, new Guid("48C4AE51-49D0-4871-9BE9-002B67688592"), "XLV A12345678");
			});
		}

		void AssertRowValues(DataTable resultTable, Guid? js, string entryNumbers)
		{
			var selectqry = string.Format("JS {0} AND EntryNumbers {1}",
				js == null ? "IS NULL" : "= '" + js + "'",
				entryNumbers == null ? "IS NULL" : "= '" + entryNumbers + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[CUS__ShipmentParentIDs](JS, ShipmentParentKey, ParentID)
															VALUES('7878DF80-2340-4C6A-83B4-006D7F465737', 1, '7878DF80-2340-4C6A-83B4-006D7F465737'),
															('48C4AE51-49D0-4871-9BE9-002B67688592', 2, '48C4AE51-49D0-4871-9BE9-002B67688592')

				INSERT [{0}].[Customs].[BAS__EntryNumber](EntryNumberID, EntryNumberKey, EntryType, EntryNum, ParentID, Category, ParentTable)
															VALUES('32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 1, 'CAN', 'ACGYPAYL9', '7878DF80-2340-4C6A-83B4-006D7F465737', 'CUS', 'JobShipment'),
															('257123E1-589D-4926-A8FF-5A186317F57F', 2, 'OAG', 'S03661641', '7878DF80-2340-4C6A-83B4-006D7F465737', 'OTH', 'JobShipment'),
															('CB7B4B03-5072-4D4D-B045-67E248D6C773', 3, 'XLV', 'A12345678', '48C4AE51-49D0-4871-9BE9-002B67688592', 'CUS', 'JobShipment')

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue)
					SELECT 'InternationalLogistics', 'CUS__ShipmentParentIDs', ShipmentParentKey
					FROM [{0}].[InternationalLogistics].[CUS__ShipmentParentIDs]",
				ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[GRP__ShipmentEntryNumbers]",
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
					"SELECT [IncrementalLoadQuery] FROM [{0}].[biadmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'GRP__ShipmentEntryNumbers'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
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
