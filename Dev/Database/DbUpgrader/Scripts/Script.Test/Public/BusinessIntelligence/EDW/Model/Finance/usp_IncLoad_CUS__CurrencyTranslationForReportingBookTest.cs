using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__CurrencyTranslationForReportingBook))]
	internal class usp_IncLoad_CUS__CurrencyTranslationForReportingBookTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals("Columns", 13, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, 1, 1, 1, "CNY", "JNL", "BUY", "P&L", null, 1);
				AssertRowValues(resultTable, 1, 1, 2, 1, 1, "CNY", "JNL", "PER", "BSH", null, 1);
				AssertRowValues(resultTable, 2, 2, 3, 1, 1, "USD", "JNL", "BUY", "P&L", null, 1);
				AssertRowValues(resultTable, 2, 2, 3, 2, 1, "USD", "JNL", "BUY", "P&L", null, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int reportingBookKey, int alternateChartKey, int currencyTranslationKey, int companyKey, int? isReciprocal, string currency, string currencyTranslationLevel, string exRateType, string accountType, int? alternateGLAccountKey, int count)
		{
			var selectqry = string.Format("AccReportingBookKey = {0} AND AlternateChartKey = {1} AND AlternateChartCurrencyTranslationKey = {2} AND CompanyKey = {3} AND IsReciprocal {4} AND Currency = '{5}' AND CurrencyTranslationLevel = '{6}' AND ExRateType = '{7}' AND AccountType = '{8}' AND AlternateGLAccountKey {9}",
				reportingBookKey,
				alternateChartKey,
				currencyTranslationKey,
				companyKey,
				isReciprocal == null ? "IS NULL" : "= " + isReciprocal,
				currency,
				currencyTranslationLevel,
				exRateType,
				accountType,
				alternateGLAccountKey == null ? "IS NULL" : "= " + alternateGLAccountKey
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalAccReportingBook()
		{
			TestHelper.InsertReportingBook(1, -1, "RUY", currency: "");
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[BAS__AccReportingBook] set RX_NKCurrency = '' where AccReportingBookKey = 1
			DELETE FROM [{0}].[Finance].[BAS__AccReportingBook] where AccReportingBookKey = 2

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'BAS__AccReportingBook', 3, null),
			('Finance', 'BAS__AccReportingBook', 1, null),
			('Finance', 'BAS__AccReportingBook', 2, newid()),
			('Finance', 'BAS__AccReportingBook', 1, newid())
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals(0, resultTable.Select("AccReportingBookKey = 2").Length);
			AssertEquals(2, resultTable.Select("AccReportingBookKey = 1").Length);
			AssertEquals(2, resultTable.Select("AccReportingBookKey = 3").Length);
			AssertEquals(4, resultTable.Select("AlternateChartKey = 1 and Currency ='CNY'").Length);
		}

		public void TestIncrementalCompany()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].Organization.BAS__Company set LocalCurrency = 'USD', IsReciprocal = 0 where CompanyKey = 1
			UPDATE [{0}].Finance.BAS__AccReportingBook set RX_NKCurrency = '' where AccReportingBookKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Organization', 'BAS__Company', 1, newid()),
			('Finance', 'BAS__AccReportingBook', 1, newid()),
			('Organization', 'BAS__Company', 1, null),
			('Finance', 'BAS__AccReportingBook', 1, null)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals(2, resultTable.Select("AccReportingBookKey = 1 AND Currency = 'USD' AND CompanyKey in (1) AND ExRateType in ('PER', 'BUY') AND IsReciprocal = 0").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(6, transformedRows.Rows.Count);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalCurrencyTranslation()
		{
			TestHelper.InsertAlternateChartCurrencyTranslation(1, -1, "BSH", "TGL", "PER");
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].Finance.BAS__AlternateChartCurrencyTranslation set AccountType = 'UUU' where AlternateChartCurrencyTranslationKey = 1

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'BAS__AlternateChartCurrencyTranslation', 1, newid()),
			('Finance', 'BAS__AlternateChartCurrencyTranslation', 1, null),
			('Finance', 'BAS__AlternateChartCurrencyTranslation', 4, null)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 5, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("AccountType = 'UUU' and AlternateChartCurrencyTranslationKey = 1").Length);
			AssertEquals(1, resultTable.Select("CurrencyTranslationLevel = 'TGL' and AlternateChartCurrencyTranslationKey = 4").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(3, transformedRows.Rows.Count);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(1, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalCurrency()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].Finance.BAS__Currency set SubUnitRatio = 10 where CurrencyKey = 2

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue, RefValue1)
			VALUES
			('Finance', 'BAS__Currency', 2, newid(), 10000)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 4, resultTable.Rows.Count);
			AssertEquals(2, resultTable.Select("Currency = 'CNY' and CurrencyKey = 2 AND SubUnitRatio = 10").Length);
			AssertEquals(2, resultTable.Select("Currency = 'USD' and CurrencyKey = 3 AND SubUnitRatio = 1000").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(4, transformedRows.Rows.Count);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NOT NULL").Length);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__CurrencyTranslationForReportingBook]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__CurrencyTranslationForReportingBook'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE FROM {0}.Organization.BAS__Company;",
				ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			var companyPK = TestHelper.InsertCompany("CNY", "DCN");
			TestHelper.InsertCompany("AUD", "DUU");
			TestHelper.InsertCurrency("AUD", 100);
			TestHelper.InsertCurrency("CNY", 1000);
			TestHelper.InsertCurrency("USD", 1000);
			TestHelper.InsertALternateChart("CCC", companyID: companyPK);
			TestHelper.InsertALternateChart("EEE", isGlobal: 1, companyID: Guid.Empty);
			TestHelper.InsertAlternateChartCurrencyTranslation(1, -1);
			TestHelper.InsertAlternateChartCurrencyTranslation(1, -1, "BSH", rateType: "PER");
			TestHelper.InsertAlternateChartCurrencyTranslation(2, -1);
			TestHelper.InsertReportingBook(1, 1);
			TestHelper.InsertReportingBook(2, -1, currency: "USD");
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__CurrencyTranslationForReportingBook'",
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

