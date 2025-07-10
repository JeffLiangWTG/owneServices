using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__ExchangeRatesForReportingBook))]
	internal class usp_IncLoad_CUS__ExchangeRatesForReportingBookTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 12, resultTable.Rows.Count);
			AssertEquals("Columns", 8, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 3, 1, "CNY", 1, "PER", "2024-01-01", "2024-01-05", 3.0m, 1);
				AssertRowValues(resultTable, 6, 2, "CNY", 13, "PER", "2024-01-01", "2024-01-03", 3.0m, 1);
				AssertRowValues(resultTable, 5, 2, "CNY", null, "SEL", "2022-01-03", "2022-01-03", 3.0m, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int exchangeRateKey, int companyKey, string currency, int? periodManagementKey, string exRateType, string startDate, string endDate, decimal sellRate, int count)
		{
			var selectqry = string.Format("ExchangeRatesForReportingBookKey = {0} AND CompanyKey = {1} AND Currency = '{2}' AND ExRateType = '{3}' AND StartDate = '{4}' AND EndDate = '{5}' AND SellRate = {6} AND PeriodManagementKey {7}",
				exchangeRateKey,
				companyKey,
				currency,
				exRateType,
				startDate,
				endDate,
				sellRate,
				periodManagementKey == null ? "IS NULL" : "= " + periodManagementKey
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalPeriodManagement()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			INSERT [{0}].[Finance].[BAS__PeriodManagement]
			([PeriodManagementKey], [PeriodManagementID], [CompanyKey], [StartDate], [EndDate], [Period])
			VALUES
			(30, newid(), 1, '2022-01-01', '2023-10-30', 202310)
			UPDATE [{0}].[Finance].[BAS__PeriodManagement] set StartDate = '2023-09-30' where PeriodManagementKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'BAS__PeriodManagement', 1, newID()),
			('Finance', 'BAS__PeriodManagement', 1, null),
			('Finance', 'BAS__PeriodManagement', 30, null)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 12, resultTable.Rows.Count);
			AssertEquals(2, resultTable.Select("PeriodManagementKey = 1 AND StartDate = '2023-09-30'").Length);
			AssertEquals(8, resultTable.Select("PeriodManagementKey IS NULL").Length);
		}

		public void TestEmptyPeriodManagement()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			DELETE FROM [{0}].[Finance].[BAS__PeriodManagement]
			
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'BAS__PeriodManagement', 1, null),
			('Finance', 'BAS__PeriodManagement', 1, newID()),
			('Finance', 'BAS__PeriodManagement', 13, null),
			('Finance', 'BAS__PeriodManagement', 13, newID())
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 12, resultTable.Rows.Count);
			AssertEquals(4, resultTable.Select("PeriodManagementKey IS NULL AND ExRateType ='PER'").Length);
		}

		public void TestIncrementalExchangeRate()
		{
			TestHelper.InsertExchangeRate(1, null, "2024-03-10", "2024-03-12", 2.10m, "PER");
			TestHelper.InsertExchangeRate(1, null, "2024-04-10", "2024-04-12", 2.20m, "PER");
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].Finance.BAS__ExchangeRate set SellRate = 3.4 where ExchangeRateKey = 3

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'BAS__ExchangeRate', 3, newid()),
			('Finance', 'BAS__ExchangeRate', 3, null),
			('Finance', 'BAS__ExchangeRate', 13, null),
			('Finance', 'BAS__ExchangeRate', 14, null)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 14, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("ExchangeRatesForReportingBookKey = 13 and SellRate = 2.1").Length);
			AssertEquals(1, resultTable.Select("ExchangeRatesForReportingBookKey = 14 and SellRate = 2.2").Length);
			AssertEquals(1, resultTable.Select("ExchangeRatesForReportingBookKey = 3 and SellRate = 3.4").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(1, transformedRows.Select("RefValue1 IS NOT NULL").Length);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__ExchangeRatesForReportingBook]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__ExchangeRatesForReportingBook'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM {0}.Organization.BAS__Company;
				DELETE FROM {0}.Finance.BAS__PeriodManagement;
				DELETE FROM {0}.Finance.BAS__ExchangeRate;",
				ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertPeriodForInputYear(2024, 1);
			TestHelper.InsertPeriodForInputYear(2024, 2);

			TestHelper.InsertExchangeRate(1, null, "2022-01-03", "2022-01-03", 2.0m);
			TestHelper.InsertExchangeRate(1, null, "2022-01-03", "2022-01-03", 3.0m, rateType: "SEL");
			TestHelper.InsertExchangeRate(1, null, "2024-01-03", "2024-01-05", 3.0m, rateType: "PER");
			TestHelper.InsertExchangeRate(2, null, "2022-01-03", "2022-01-03", 2.0m);
			TestHelper.InsertExchangeRate(2, null, "2022-01-03", "2022-01-03", 3.0m, rateType: "SEL");
			TestHelper.InsertExchangeRate(2, null, "2024-01-03", "2024-01-03", 3.0m, rateType: "PER");
			TestHelper.InsertExchangeRate(1, null, "2022-01-03", "2022-01-03", 2.0m, currency: "USD");
			TestHelper.InsertExchangeRate(1, null, "2022-01-03", "2022-01-03", 3.0m, rateType: "SEL", currency: "USD");
			TestHelper.InsertExchangeRate(1, null, "2024-01-03", "2024-01-03", 3.0m, rateType: "PER", currency: "USD");
			TestHelper.InsertExchangeRate(2, null, "2022-01-03", "2022-01-03", 2.0m, currency: "USD");
			TestHelper.InsertExchangeRate(2, null, "2022-01-03", "2022-01-03", 3.0m, rateType: "SEL", currency: "USD");
			TestHelper.InsertExchangeRate(2, null, "2024-01-05", "2024-01-13", 3.0m, rateType: "PER", currency: "USD");
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__ExchangeRatesForReportingBook'",
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

