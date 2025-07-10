using System;
//using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol.Testing
{
	[TestedType(typeof(ViewFirstLastConsolTransport))]
	class ViewFirstLastConsolTransportTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, "Test 1:", "CON", new Guid("07BD3D66-4D8E-43E3-BCB2-000150406877"));
				AssertRowValues(resultTable, "Test 2:", "CON", new Guid("D7B7900D-CF86-4282-98FA-0006FED2A289"));
			});
		}

		void AssertRowValues(DataTable resultTable, string testName, string parentType, Guid? jk)
		{
			var selectqry = string.Format("ParentType {0} AND JK {1}",
				parentType == null ? "IS NULL" : "='" + parentType + "'",
				jk == null ? "IS NULL" : "='" + jk + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals(testName, 1, rows.Length);
		}
		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[dbo].[ViewFirstLastConsolTransport]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport]
					([ConsolAndShipmentTransportKey], [ConsolAndShipmentTransportID], [LegOrder], [ParentGUID], [ParentType],[LoadPort],[ETA],[ETD],[DischargePort],[ATA],[ATD],[VoyageFlight], [JobSailingKey] )
					VALUES
						(1, CAST('07BD3D66-4D8E-43E3-BCB2-000150406877' AS uniqueidentifier), 1,  CAST('07BD3D66-4D8E-43E3-BCB2-000150406877' AS uniqueidentifier), 'CON', 'SGSIN', NULL, '2011-06-03', 'AUSYD', NULL, NULL, 'QF05', NULL),
						(2, CAST('D7B7900D-CF86-4282-98FA-0006FED2A289' AS uniqueidentifier), 1,  CAST('D7B7900D-CF86-4282-98FA-0006FED2A289' AS uniqueidentifier), 'CON', 'AUSYD', '2020-06-03', '2020-05-30', 'AUBNE', NULL, NULL, 'CDEO000068', NULL);

				", ScriptDbName
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

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
