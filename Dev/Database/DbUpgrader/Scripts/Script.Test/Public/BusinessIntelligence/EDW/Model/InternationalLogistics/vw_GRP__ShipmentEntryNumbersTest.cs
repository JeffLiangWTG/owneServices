using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_GRP__ShipmentEntryNumbers))]
	internal class vw_GRP__ShipmentEntryNumbersTest : BiCreateScriptTest
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
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, new Guid("7878DF80-2340-4C6A-83B4-006D7F465737"), "CAN ACGYPAYL9");
				AssertRowValues(resultTable, new Guid("48C4AE51-49D0-4871-9BE9-002B67688592"), "XLV A12345678");
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"JS",
				"EntryNumbers"
			};

			return columns;
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

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_GRP__ShipmentEntryNumbers]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
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
															('CB7B4B03-5072-4D4D-B045-67E248D6C773', 3, 'XLV', 'A12345678', '48C4AE51-49D0-4871-9BE9-002B67688592', 'CUS', 'JobShipment')",
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
