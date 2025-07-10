using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GeneralLedgerTranslatedData))]
	internal class usp_IncLoad_CUS__GeneralLedgerTranslatedDataTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 5, resultTable.Rows.Count);
			AssertEquals("Columns", 45, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, 1, 1, 1, 1, 1, 1, "BUY", 20, 0.05m, 1);
				AssertRowValues(resultTable, 2, 1, 2, 1, null, 1, 4, 2, "SEL", null, null, 1);
				AssertRowValues(resultTable, 2, 1, 3, 1, null, 1, null, null, null, null, null, 1);
				AssertRowValues(resultTable, 2, 1, 4, 1, -1, 1, -1, null, null, 1, 1, 1);
				AssertRowValues(resultTable, 4, 1, 5, 1, 5, 1, 6, 4, "SEL", 10, 0.1m, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int reportingBookKey, int alternateChartKey, int generalLedgerDataKey, int companyKey, int? exchangeRateKey, int? periodManagementKey, int? currencyTranslationKey, int? alternateGLAccountKey, string rateType, decimal? exchangeRate, decimal? translatedAmount, int count)
		{
			var sql = string.Format("ReportingBookKey = {0} AND AlternateChartKey = {1} AND GeneralLedgerDataKey = {2} AND CompanyKey = {3} AND ExchangeRatesForReportingBookKey {4} AND PeriodManagementKey {5} AND CurrencyTranslationForReportingBookKey {6} AND AlternateGLAccountKey {7} AND TranslatedRateType {8} AND TranslatedExchangeRate {9} AND TranslatedAmount {10}",
				reportingBookKey,
				alternateChartKey,
				generalLedgerDataKey,
				companyKey,
				exchangeRateKey == null ? "IS NULL" : "= " + exchangeRateKey,
				periodManagementKey == null ? "IS NULL" : "= " + periodManagementKey,
				currencyTranslationKey == null ? "IS NULL" : "= " + currencyTranslationKey,
				alternateGLAccountKey == null ? "IS NULL" : "= " + alternateGLAccountKey,
				rateType == null ? "IS NULL" : "= '" + rateType + "'",
				exchangeRate == null ? "IS NULL" : "= " + exchangeRate,
				translatedAmount == null ? "IS NULL" : "= " + translatedAmount
				);

			var rows = resultTable.Select(sql);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalCurrencyTranslation()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			INSERT INTO {0}.Finance.CUS__AlternateGLAccountData (AlternateGLAccountDataKey, GLAccountKey, AlternateChartKey, CompanyKey, DestinationAccountNo, StatisticalUnits, OriginalAccount, AccountTypeCode, AlternateGLAccountKey, Description, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC)
			Values
			(30, 1, 1, 1, '9074', 'KG', '4562', 'BSH', 30, 'desc', 'OR3', 'INT', 'WEU', 'LOC', '', '')

			UPDATE [{0}].[Finance].[CUS__CurrencyTranslationForReportingBook] SET AccountType = 'BSH' WHERE CurrencyTranslationForReportingBookKey = 4

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue, RefValue1, RefValue2, RefValue3, RefValue4)
			VALUES
			('Finance', 'CUS__CurrencyTranslationForReportingBook', 4, newID(), null, null, null, null),
			('Finance', 'CUS__AlternateGLAccountData', 30, newID(), null, null, null, null),
			('Finance', 'CUS__CurrencyTranslationForReportingBook', 4, newID(), 2, 'P&L', 1, null)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 5, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("CurrencyTranslationForReportingBookKey = 4 AND TranslatedRateType = 'SEL'").Length);
		}

		public void TestIncrementalExchangeRate()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[CUS__ExchangeRatesforReportingbook] SET SellRate = 5 WHERE ExchangeRatesforReportingbookKey = 1
			UPDATE [{0}].[Finance].[CUS__ExchangeRatesforReportingbook] SET StartDate = '2021-09-08' WHERE ExchangeRatesforReportingbookKey = 2

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'CUS__ExchangeRatesforReportingbook', 1, newID()),
			('Finance', 'CUS__ExchangeRatesforReportingbook', 2, newID())
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 5, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("TranslatedAmount = 0.2 AND ExchangeRatesForReportingBookKey = 1").Length);
			AssertEquals(1, resultTable.Select("TranslatedAmount = 10 AND ExchangeRatesForReportingBookKey = 2").Length);
		}

		public void TestIncrementalAlternateGLAccountData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[CUS__AlternateGLAccountData] SET DestinationAccountNo = '888',AccountTypeCode ='NTE' WHERE AlternateGLAccountDataKey = 1

			INSERT INTO {0}.Finance.CUS__AlternateGLAccountData (AlternateGLAccountDataKey, GLAccountKey, AlternateChartKey, CompanyKey, DestinationAccountNo, StatisticalUnits, OriginalAccount, AccountTypeCode, AlternateGLAccountKey, Description, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC)
			Values
			(30, 1, 1, 1, '9074', 'KG', '4562', 'BSH', 30, 'desc', 'OR3', 'INT', 'WEU', 'LOC', '', ''),
			(31, 1, 1, 1, '9174', 'KG', '4512', 'NTE', 31, 'desc', 'OR3', 'INT', 'WEU', 'LOT', '', '')

			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'CUS__AlternateGLAccountData', 1, newID()),
			('Finance', 'CUS__AlternateGLAccountData', 31, newID()),
			('Finance', 'CUS__AlternateGLAccountData', 30, newID())
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 5, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("AlternateGLAccount = '888' AND AlternateGLAccountKey = 1 AND TranslatedRateType IS NULL AND AccountTypeCode = 'NTE'").Length);
			AssertEquals(1, resultTable.Select("AlternateGLAccount = '9074' AND AlternateGLAccountKey = 30 AND CurrencyTranslationForReportingBookKey IS NULL").Length);
			AssertEquals(1, resultTable.Select("AlternateGLAccount = '9174' AND AlternateGLAccountKey = 31 AND CurrencyTranslationForReportingBookKey = -1").Length);
			AssertEquals(0, resultTable.Select("AlternateGLAccountKey IS NULL").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(6, transformedRows.Rows.Count);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NULL").Length);
			AssertEquals(3, transformedRows.Select("RefValue1 IS NOT NULL").Length);
		}

		public void TestIncrementalGeneralLedgerDataAttribute()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			DELETE FROM [{0}].biadmin.TransformedRow;
			UPDATE [{0}].[Finance].[CUS__GeneralLedgerDataAttribute] SET Attribute_ORG = 'OR4' WHERE GeneralLedgerDataAttributeKey = 1

			INSERT INTO [{0}].[Finance].[CUS__GeneralLedgerDataAttribute]
				(GeneralLedgerDataAttributeKey, GeneralLedgerDataKey, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, GLAccountKey, AlternateChartKey, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG,
				BranchKey, CompanyKey, CompanyOfPeriodKey, DepartmentKey, LocalAmount, LocalCurrency, OrganizationKey, PeriodManagementKey, PostDate, PostPeriodForReportingBook, TranslatedCurrency, ReportingBookKey)
				VALUES
					(10, 10, 'O11', 'INT', 'WEU', 'LOC', '', '', 1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 1, 1, 1, 1, 1, 'CNY', 1, 1, '2023-09-08', 202309, 'USD', 1)
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, PKValue)
			VALUES
			('Finance', 'CUS__GeneralLedgerDataAttribute', 1, newID()),
			('Finance', 'CUS__GeneralLedgerDataAttribute', 10, newID())
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 6, resultTable.Rows.Count);
			AssertEquals(1, resultTable.Select("GeneralLedgerDataAttributeKey = 1 AND Attribute_ORG = 'OR4'").Length);
			AssertEquals(1, resultTable.Select("GeneralLedgerDataAttributeKey = 10 AND Attribute_ORG = 'O11'").Length);

			var transformedRows = SelectTransformedRows();
			AssertEquals(3, transformedRows.Rows.Count);
			AssertEquals(2, transformedRows.Select("RefValue1 IS NULL").Length);
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		DataTable SelectTransformedRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].biadmin.TransformedRow WHERE SchemaName = 'Finance' AND TableName = 'CUS__GeneralLedgerTranslatedData'", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO {0}.Finance.CUS__AlternateGLAccountData (AlternateGLAccountDataKey, GLAccountKey, AlternateChartKey, CompanyKey, DestinationAccountNo, StatisticalUnits, OriginalAccount, AccountTypeCode, AlternateGLAccountKey, Description, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC)
				Values
				(1, 1, 1, 1, '9876', 'KG', '4567', 'BSH', 1, 'desc', 'OR1', 'INT', 'WEU', 'LOC', '', ''),
				(2, 1, 1, 1, '9875', 'KG', '4561', 'P&L', 2, 'desc', 'OR2', 'INT', 'WEU', 'LOC', '', ''),
				(3, 2, 2, 2, '9874', 'KG', '4562', 'BSH', 3, 'desc', '', '', '', '', '', ''),
				(4, 3, 1, 1, '9873', 'KG', '4563', 'BSH', 4, 'desc', 'OR1', 'INT', 'WEU', 'LOC', '', '')

				INSERT [{0}].[Finance].[CUS__GeneralLedgerDataAttribute]
					(GeneralLedgerDataAttributeKey, GeneralLedgerDataKey, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, Attribute_SPR, Attribute_TIC, GLAccountKey, AlternateChartKey, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, OriginalAttribute_SPR, OriginalAttribute_TIC, OriginalAttribute_ORG,
					BranchKey, CompanyKey, CompanyOfPeriodKey, DepartmentKey, LocalAmount, LocalCurrency, OrganizationKey, PeriodManagementKey, PostDate, PostPeriodForReportingBook, TranslatedCurrency, ReportingBookKey)
				VALUES
					(1, 1, 'OR1', 'INT', 'WEU', 'LOC', '', '', 1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 1, 1, 1, 1, 1, 'CNY', 1, 1, '2023-09-08', 202309, 'USD', 1),
					(2, 2, 'OR2', 'INT', 'WEU', 'LOC', '', '', 1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 1, 1, 1, 1, 1, 'CNY', 1, 1, '2022-08-08', 202309, 'USD', 2),
					(3, 3, 'OR3', 'INT', 'WEU', 'LOC', '', '', 1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 1, 1, 1, 1, 1, 'CNY', 1, 1, '2022-08-08', 202309, 'USD', 2),
					(4, 4, 'OR3', 'INT', 'WEU', 'LOT', '', '', 1, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 1, 1, 1, 1, 1, 'CNY', 1, 1, '2022-08-08', 202309, 'CNY', 2),
					(5, 5, 'OR1', 'INT', 'WEU', 'LOC', '', '', 3, 1, 'INT', 'WEU', 'LOC', 'SPS', 'ETI', 'OR1', 1, 1, 1, 1, 1, 'CNY', 1, 1, '2023-09-08', 202309, 'USD', 4)

				INSERT [{0}].[Finance].[CUS__CurrencyTranslationForReportingBook]
					(CurrencyTranslationForReportingBookKey, CompanyKey, AccReportingBookKey, AlternateChartKey, IsReciprocal, CurrencyTranslationLevel, ExRateType, Currency, AccountType, AlternateGLAccountKey, SubUnitRatio)
				VALUES
					(1, 1, 1, 1, 1, 'JNL', 'BUY', 'CNY', 'BSH', null, 100),
					(2, 1, 1, 1, 1, 'JNL', 'SEL', 'CNY', 'P&L', null, 100),
					(3, 1, 2, 1, 1, 'CNL', 'SEL', 'CNY',    '',    1, 100),
					(4, 1, 2, 1, 0, 'JNL', 'SEL', 'CNY', 'P&L', null, 100),
					(5, 2, 3, 2, 1, 'CNL', 'SEL', 'CNY', 'BSH', null, 100),
					(6, 1, 4, 1, 1, 'JNL', 'SEL', 'USD',    '',    4, 100),
					(7, 1, 4, 1, 1, 'JNL', 'BUY', 'USD', 'BSH', null, 100)

				INSERT [{0}].[Finance].[CUS__ExchangeRatesforReportingbook]
					(ExchangeRatesForReportingBookKey, CompanyKey, SellRate, StartDate, EndDate, ExRateType, Currency)
				VALUES
					(1, 1, 20, '2023-01-01', '2023-11-01', 'BUY', 'CNY'),
					(2, 1, 10, '2023-01-01', '2023-11-01', 'SEL', 'CNY'),
					(3, 2,  2, '2023-01-01', '2023-11-01', 'BUY', 'CNY'),
					(4, 2,  5, '2023-01-01', '2023-11-01', 'SEL', 'CNY'),
					(5, 1, 10, '2023-01-01', '2023-11-01', 'SEL', 'USD')
				",
				ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertPeriodForInputYear(2023);
			TestHelper.InsertPeriodForInputYear(2024, 2);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__GeneralLedgerTranslatedData'",
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

