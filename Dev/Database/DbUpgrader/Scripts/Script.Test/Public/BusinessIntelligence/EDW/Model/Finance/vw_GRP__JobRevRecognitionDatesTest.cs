using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__JobRevRecognitionDates))]
	internal class vw_GRP__JobRevRecognitionDatesTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, 1, "TY1 10-Aug-23, TY1 11-Aug-23");
				AssertRowValues(resultTable, 2, "TY2 11-Aug-23");
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"JH",
				"DateList"
			};

			return columns;
		}

		void AssertRowValues(DataTable resultTable, int? jH, string dateList)
		{
			var selectqry = string.Format("JH {0} AND DateList {1}",
			jH == null ? "IS NULL" : "=" + jH,
			dateList == null ? "IS NULL" : "= '" + dateList + "'");

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[vw_GRP__JobRevRecognitionDates]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__RevenueRecognition]
					([JobHeaderKey], [RecognitionDate], [RecognitionType], [RevenueRecognitionKey],
					[RevenueRecognitionID])
					VALUES
						(1, '2023-08-10','TY1', 1, newid()),
						(2, '2023-08-11','TY2', 2, newid()),
						(1, '2023-08-11','TY1', 3, newid())",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
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
