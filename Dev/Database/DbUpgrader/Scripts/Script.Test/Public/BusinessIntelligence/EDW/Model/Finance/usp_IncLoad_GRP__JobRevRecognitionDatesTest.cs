using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__JobRevRecognitionDates))]
	internal class usp_IncLoad_GRP__JobRevRecognitionDatesTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			HashSet<string> columns = GetColumns();
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
			var columns = new HashSet<string>();

			columns.Add("JH");
			columns.Add("DateList");

			return columns;
		}

		void AssertRowValues(DataTable resultTable, int? jH, string dateList)
		{
			var selectqry = string.Format("JobHeaderKey {0} AND DateList {1}",
				jH == null ? "IS NULL" : "=" + jH,
				dateList == null ? "IS NULL" : "= '" + dateList + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals(1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Finance].[GRP__JobRevRecognitionDates]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		string GetIncLoadSQLText()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [IncrementalLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__JobRevRecognitionDates'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			string iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				@"
				INSERT [{0}].[Finance].[BAS__RevenueRecognition]
					([JobHeaderKey], [RecognitionDate], [RecognitionType], [RevenueRecognitionKey],
					[RevenueRecognitionID])
					VALUES
						(1, '2023-08-10','TY1', 1, newid()),
						(2, '2023-08-11','TY2', 2, newid()),
						(1, '2023-08-11','TY1', 3, newid())

				INSERT INTO [{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, RefValue1)
					SELECT 'Finance', 'BAS__RevenueRecognition', RevenueRecognitionKey, JobHeaderKey
					FROM [{0}].[Finance].[BAS__RevenueRecognition];
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void Execute()
		{
			string incLoadSQLText = GetIncLoadSQLText();
			string sqlText1 = "USE " + ScriptDbName + " " + incLoadSQLText;
			string sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
