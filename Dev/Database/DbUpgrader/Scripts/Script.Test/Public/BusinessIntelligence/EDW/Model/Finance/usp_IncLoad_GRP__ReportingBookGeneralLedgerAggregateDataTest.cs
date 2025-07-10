using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_GRP__ReportingBookGeneralLedgerAggregateData))]
	internal class usp_IncLoad_GRP__ReportingBookGeneralLedgerAggregateDataTest : BiCreateScriptTest
	{
		public void TestInitialLoad()
		{
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 9, resultTable.Rows.Count);
			AssertEquals("Columns", 38, resultTable.Columns.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "JNL", 100001, 1, 1, "SPS", "ETI", "ETI", 5, 0, 1, 2, 1);
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 2, 1, "SPS", "ETI", "ETI", 5, 0, 1, 2, 1);
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 1, 1, "SPS", "ETI", "ETI", 1, 0, 2, 4, 1);
				AssertRowValues(resultTable, 1, 1, "BNL", 202305, 1, 1, "SPS", "ETI", "ETI", 7, 0, 1, 2, 1);
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 1, 1, "SPS", "ETI", "ETI", 2, 0, 1, 2, 2);
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 1, 1, "SPS", "ETI", "ETI", 3, 0, 1, 2, 2);
			});
		}

		void AssertRowValues(DataTable resultTable, int reportingBookKey, int alternateGLAccountKey, string currencyTranslationLevel, int postPeriod, int companykey, int glAccountkey, string attribute_SPR, string attribute_TIC, string originalAttribute_TIC, int branchKey, decimal localCredit = 0m, decimal localDebit = 0m, decimal translatedAmount = 0m, int count = 1)
		{
			var selectqry = string.Format("ReportingBookKey = {0} AND AlternateGLAccountKey = {1} AND CurrencyTranslationLevel = '{2}' AND PostPeriod = {3} AND CompanyKey = {4} AND GLAccountKey = {5} AND Attribute_SPR = '{6}' AND Attribute_TIC = '{7}' AND OriginalAttribute_TIC = '{8}' AND BranchKey = {9} AND GLAmountLocalCredit = {10} AND GLAmountLocalDebit = {11} AND TranslatedBalance = {12}",
				reportingBookKey,
				alternateGLAccountKey,
				currencyTranslationLevel,
				postPeriod,
				companykey,
				glAccountkey,
				attribute_SPR,
				attribute_TIC,
				originalAttribute_TIC,
				branchKey,
				localCredit,
				localDebit,
				translatedAmount
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals($"Rowcount should be {count}", count, rows.Length);
		}

		public void TestIncrementalGeneralLedgerTranslatedData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
			INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
			(GeneralLedgerTranslatedDataKey, BranchKey, CompanyKey, DepartmentKey, GLAccountKey, LocalAmount, TranslatedAmount, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, PostPeriodForReportingBook, TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, IsReciprocal, SubUnitRatio, LocalCurrency)
				VALUES
				( 100, 3, 1, 4, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY')
				
			DELETE FROM [{0}].[Finance].[CUS__GeneralLedgerTranslatedData] WHERE GeneralLedgerTranslatedDataKey = 2
			UPDATE [{0}].[Finance].[CUS__GeneralLedgerTranslatedData] set Attribute_TIC = 'ORG', BranchKey = 4, ReportingBookKey = 3 where GeneralLedgerTranslatedDataKey = 1
			INSERT INTO [{0}].biadmin.TransformedRow(SchemaName, TableName, KeyValue, RefValue1, RefValue2, RefValue3, RefValue4, RefValue5)
			VALUES
			('Finance', 'CUS__GeneralLedgerTranslatedData', 100, null, null, null, null, null),
			('Finance', 'CUS__GeneralLedgerTranslatedData', 2, 1, 1, 1, 2, 1),
			('Finance', 'CUS__GeneralLedgerTranslatedData', 1, null, null, null, null, null),
			('Finance', 'CUS__GeneralLedgerTranslatedData', 1, 1, 1, 1, 1, 1)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			ExecuteTableLoad();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 10, resultTable.Rows.Count);
			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 1, 1, "SPS", "ETI", "ETI", 1, 0, 1, 2, 1);
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 1, 1, "SPS", "ETI", "ETI", 2, 0, 1, 2, 1);
				AssertRowValues(resultTable, 1, 1, "JNL", 202304, 1, 1, "SPS", "ETI", "ETI", 3, 0, 1, 2, 3);
				AssertRowValues(resultTable, 3, 1, "JNL", 202304, 1, 1, "SPS", "ORG", "ETI", 4, 0, 1, 2, 1);
			});
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
			var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT * FROM [{0}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]", ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[CUS__GeneralLedgerTranslatedData]
				(GeneralLedgerTranslatedDataKey, BranchKey, CompanyKey, DepartmentKey, GLAccountKey, LocalAmount, TranslatedAmount, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, PostPeriodForReportingBook, TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG, OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, IsReciprocal, SubUnitRatio, LocalCurrency)
				VALUES
				( 1, 1, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				( 2, 2, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				( 3, 3, 1, 1, 1, 1, 2, 'USD', 'JNL', 'BUY', 202304,    '', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'AUD'),
				( 4, 4, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL',   null, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				( 5, 5, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL', 100001, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				( 6, 6, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304,    '', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'AUD'),
				( 7, 7, 1, 1, 1, 1, 2, 'CNY', 'BNL', 'CUS', 202305, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				(11, 1, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				(21, 2, 1, 1, 1, 1, 2, 'USD', 'JNL', 'CUE', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				(31, 3, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				(41, 4, 1, 1, 1, 1, 2, 'USD', 'JNL', 'SEL',   null, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY'),
				(81, 5, 2, 3, 1, 1, 2, 'USD', 'JNL', 'SEL', 202304, 'CAT', 'RB1', 1, 1, 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', 'ETI', 'SPS', 'ORG', 'INT', 'LOC', 'OEU', '1001', 1, 'P&L', 1, 1, 1, 100, 'CNY')",
				ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void ExecuteTableLoad(string sqlName = "IncrementalLoadQuery")
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'GRP__ReportingBookGeneralLedgerAggregateData'",
				sqlName, ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}
	}
}

