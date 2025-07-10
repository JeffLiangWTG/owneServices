using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__ReportingBookAndPeriodInfo))]
	internal class usp_IncLoad_CUS__ReportingBookAndPeriodInfoTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals("Columns", 10, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, 1, 1, "R1", "CNY", "2023-01-01", 1);
				AssertRowValues(resultTable, 1, 2, 1, 2, "R2", "CNY", "2024-01-01", 1);
				AssertRowValues(resultTable, 2, 3, 1, 2, "R3", "CNY", "2024-01-01", 1);
				AssertRowValues(resultTable, 2, 3, 2, 2, "R3", "CNT", "2024-01-01", 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int alternateChartKey, int reportingBookKey, int companyKey, int companyOfPeriodKey, string reportingBookCode, string currency, string startDate, int count)
		{
			var sql = string.Format("AlternateChartKey = {0} AND ReportingBookKey = {1} AND CompanyKey = {2} AND CompanyOfPeriodKey = {3} AND ReportingBookCode = '{4}' AND RX_NKCurrency = '{5}' AND StartDate = '{6}'",
				alternateChartKey,
				reportingBookKey,
				companyKey,
				companyOfPeriodKey,
				reportingBookCode,
				currency,
				startDate
			);

			var rows = resultTable.Select(sql);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalLoadReportingBook()
		{
			TestHelper.InsertReportingBook(2, 2, "R3", currency: "USD");
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[BAS__AccReportingBook] set RX_NKCurrency = '', ReportingBookCode = 'KJH' where AccReportingBookKey = 1
			DELETE FROM [{0}].[Finance].[BAS__AccReportingBook] where AccReportingBookKey = 2

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue)
			VALUES
			('Finance', 'BAS__AccReportingBook', 4),
			('Finance', 'BAS__AccReportingBook', 1),
			('Finance', 'BAS__AccReportingBook', 2)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 5, resultTable.Rows.Count);
			AssertEquals(0, resultTable.Select("ReportingBookKey = 2").Length);
			AssertEquals(1, resultTable.Select("ReportingBookKey = 1 AND ReportingBookCode = 'KJH'").Length);
			AssertEquals(2, resultTable.Select("ReportingBookKey = 4").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(5, transformedRows.Rows.Count);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalPeriodManagement()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			INSERT [{0}].[Finance].[BAS__PeriodManagement]
			([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate], [Period])
			VALUES
			(30, newid(), 2, '2022-01-01', '2023-10-30', 202310)
			UPDATE [{0}].[Finance].[BAS__PeriodManagement] set StartDate = '2023-09-30' where PeriodManagementKey = 3
			DELETE FROM [{0}].[Finance].[BAS__PeriodManagement] WHERE PeriodManagementKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1, RefValue3)
			VALUES
			('Finance', 'BAS__PeriodManagement', 3, 1, '2023-03-01'),
			('Finance', 'BAS__PeriodManagement', 1, 1, '2023-01-01'),
			('Finance', 'BAS__PeriodManagement', 30, null, null)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("CompanyKey = 1 AND StartDate = '2023-02-01'").Length);
			AssertEquals(3, resultTable.Select("CompanyOfPeriodKey = 2 AND StartDate = '2022-01-01'").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(8, transformedRows.Rows.Count);
			AssertEquals(4, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(4, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalCompany()
		{
			TestHelper.InsertCompany("CNR", "DWU");
			TestHelper.InsertPeriodForInputYear(2021, 3);
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			
			UPDATE [{0}].[Organization].[BAS__Company] set CompanyCode = 'we' where CompanyKey = 1
			DELETE FROM [{0}].[Organization].[BAS__Company] WHERE CompanyKey = 2

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue, RefValue2, RefValue1)
			VALUES
			('Organization', 'BAS__Company', 1, newID(), 'CND', null),
			('Organization', 'BAS__Company', 2, newID(), '3', null),
			('Organization', 'BAS__Company', 3, null, null, null),
			('Finance', 'BAS__PeriodManagement', 25, null, null, 3),
			('Finance', 'BAS__AccReportingBook', 3, null, null, 3)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("CompanyKey = 1 AND StartDate = '2023-01-01'").Length);
			AssertEquals(3, resultTable.Select("CompanyOfPeriodKey = 2 AND StartDate = '2024-01-01'").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(8, transformedRows.Rows.Count);
			AssertEquals(4, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(4, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
			ExecuteTableLoad("InitialLoadQuery");
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__ReportingBookAndPeriodInfo]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__ReportingBookAndPeriodInfo'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var companyPK = TestHelper.InsertCompany("CNY", "DCN");
			TestHelper.InsertCompany("CNT", "DCU");
			TestHelper.InsertALternateChart("A1", companyID: companyPK);
			TestHelper.InsertALternateChart("A2");
			TestHelper.InsertReportingBook(1, 1, "R1");
			TestHelper.InsertReportingBook(1, 2, "R2");
			TestHelper.InsertReportingBook(2, 2, "R3", currency: "");
			TestHelper.InsertPeriodForInputYear(2023);
			TestHelper.InsertPeriodForInputYear(2024, 2);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__ReportingBookAndPeriodInfo'",
				sqlName, ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
