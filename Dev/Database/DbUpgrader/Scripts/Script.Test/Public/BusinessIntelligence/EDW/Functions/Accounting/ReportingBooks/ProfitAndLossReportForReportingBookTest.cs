using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLossReportForReportingBook))]
	class ProfitAndLossReportForReportingBookTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestReportTypeTBS_CheckLastProcessedDate()
		{
			var result = Execute(ReportingBookID1, 202308, CompanyPK1);
			AssertEquals("Has data if LastProcessedDate is earlier than first startDate", 8, result.Rows.Count);

			result = Execute(ReportingBookID2, 202308, CompanyPK2);
			AssertEquals("No data if LastProcessedDate is null", 0, result.Rows.Count);

			TestHelper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2022, 01, 01), 2);

			result = Execute(ReportingBookID2, 202308, CompanyPK2);
			AssertEquals("No data if startDate is null", 0, result.Rows.Count);

			TestHelper.InsertPeriodForInputYear(2023, 2);
			result = Execute(ReportingBookID2, 202308, CompanyPK2);
			AssertEquals("Has data if LastProcessedDate is earlier than first startDate", 7, result.Rows.Count);
		}

		public void TestBranchFilter()
		{
			var result = Execute(ReportingBookID1, 202307, CompanyPK1, branchList: "SYN", inclZeroBal: "");
			AssertEquals("Has data if BranchCode is right", 1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 1 and PeriodLastYear is null and ClosingStandard = -1 and Attribute_SPR = 'SPR-Sales/Purchases Return' and Attribute_TIC ='STI-Standard Tax IDs'").Length);

			result = Execute(ReportingBookID1, 202307, CompanyPK1, branchList: "SYB", inclZeroBal: "");
			AssertEquals("No data if BranchCode is wrong", 0, result.Rows.Count);
		}

		public void TestDepartmentFilter()
		{
			var result = Execute(ReportingBookID1, 202308, CompanyPK1, departmentList: "AU1", inclZeroBal: "");
			AssertEquals("Has data if departmentCode is right", 1, result.Rows.Count);

			result = Execute(ReportingBookID1, 202305, CompanyPK1, departmentList: "TW5", inclZeroBal: "");
			AssertEquals("No data if departmentCode is wrong", 0, result.Rows.Count);
		}

		public void TestPresentationJournalsFilter()
		{
			var result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "");
			AssertEquals("Has data if PresentationJournals of Reporting Book is empty", 1, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 1 and AccountNumber = '12.34.5' and YearToPeriod = 1 and GLAccountKey = 1 and Attribute_ORG = 'INTT1' and GLAccountDesc ='12345-desc'").Length);

			var sql = $@"UPDATE [{ScriptDbName}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'EET'";
			TestConnection.ExecuteNonQuery(sql);
			result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "");
			AssertEquals("Two data if PresentationJournals of Reporting Book is set to EET", 2, result.Rows.Count);
		}

		public void TestPeriodFilter()
		{
			var result = Execute(ReportingBookID1, 202307, CompanyPK1, inclZeroBal: "");
			AssertEquals("Has data if currentPeriod/yearPeriod != 0", 1, result.Rows.Count);

			result = Execute(ReportingBookID1, 202301, CompanyPK1, inclZeroBal: "");
			AssertEquals("No data if currentPeriod = 0", 0, result.Rows.Count);
		}

		public void TestPeriodGap()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 1, 2, 3, 4, 1, 100001, '',    'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, -1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202405, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202408, '',    'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202308, '',    'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202405, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202305, '',    'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202005, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				UPDATE [{ScriptDbName}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'EET'
			");

			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202408, CompanyPK1, inclZeroBal: "");
			AssertEquals("Has data if currentPeriod/yearPeriod != 0", 3, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 2 and AccountNumber = '12.34.6' and AccountNumberForTotals = '12.34.6' and AccountType = 'P&L' and YearToPeriodPresentationJournals = 1 and YearToPeriod = 1 and LastYearToPeriodStandard = 1 and LastYearToPeriodPresentationJournals = 1 and LastyearToPeriod = 2 and TotalLastYearStandard = 1 and TotalLastYearPresentationJournals = 1 and TotalLastYear = 2 and ClosingPresentationJournals = -1 and ClosingStandard is null").Length);
			AssertEquals(1, result.Select("AccountPK = 6 and AccountNumber = '12.34.9' and AccountNumberForTotals = '12.34.9' and AccountType = 'BSH' and CurrentPeriodStandard = 1 and CurrentPeriod = 1 and YearToPeriodPresentationJournals = 1 and YearToPeriod = 4 and LastYearToPeriodStandard = 1 and LastYearToPeriodPresentationJournals is null and LastyearToPeriod = 1 and TotalLastYearStandard = 1 and TotalLastYearPresentationJournals is null and TotalLastYear = 1 and ClosingPresentationJournals = -1 and ClosingStandard =-3 and CLosingCredit = 4 and CurrentCredit = 1 and CurrentStandard = -1 and ClosingStandardForCalculateTotal = -3").Length);
			AssertEquals(1, result.Select("AccountPK = 19 AND YearToPeriod = 4 AND YearToPeriodPresentationJournals = 2").Length);

			result = Execute(ReportingBookID1, 202305, CompanyPK1, inclZeroBal: "");
			AssertEquals(1, result.Select("AccountPK = 2 and CurrentPeriodPresentationJournals = 1 and CurrentPeriod = 2 and YearToPeriodPresentationJournals = 1 and YearToPeriod = 2 and LastYearToPeriodStandard is null and TotalLastYear is null and CurrentCredit = 2 and CurrentStandard = -1 and ClosingPresentationJournals = -1 and ClosingStandard = -1").Length);

			result = Execute(ReportingBookID1, 202405, CompanyPK1, inclZeroBal: "");
			AssertEquals(1, result.Select("AccountPK = 2 and PeriodLastYearStandard = 1 and PeriodLastYearPresentationJournals = 1 and PeriodLastYear = 2").Length);
		}

		public void TestSeparateNumberingFalse()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 40, 10, 202306, 'CAT', 'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '',    'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202406, 'CAT', 'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202403, '',    'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202406, 'CAT', 'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT5', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 4, 30, 40, 10, 202306, '',    'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT5', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				UPDATE [{ScriptDbName}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'CAT'
			");

			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202406, CompanyPK1, inclZeroBal: "");
			AssertEquals("Has data if currentPeriod/yearPeriod != 0", 3, result.Rows.Count);
			AssertEquals(0, result.Select("AccountPK = 1").Length);
			AssertEquals(1, result.Select("AccountPK = 13 and CurrentPeriodPresentationJournals =10 and CurrentPeriod = 10 and YearToPeriodPresentationJournals = 10 and YearToPeriod = 20 and PeriodLastYear = 30 and  LastYearToPeriodStandard = 10 and TotalLastYear = 30 and GLAccountDesc = '12345678-desc,123456789-desc' and Attribute_ORG = 'INTT4' and Attribute_OCG = 'TPY' and Attribute_LFE = 'WEU-Within EU'").Length);
			AssertEquals(1, result.Select("AccountPK = 13 and CurrentPeriodPresentationJournals =10 and CurrentPeriod = 10 and YearToPeriodPresentationJournals = 10 and YearToPeriod = 10 and PeriodLastYear = 10 and  LastYearToPeriodStandard = 10 and TotalLastYear = 10 and GLAccountDesc = '12345678-desc,123456789-desc' and Attribute_ORG = 'INTT5' and Attribute_OCG = 'TPY' and Attribute_LFE = 'WEU-Within EU'").Length);
		}

		public void TestPNLReport()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 40, 10, 202308, 'CAT', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '',    'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202406, 'CAT', 'RRR', 1, 1, 'ETI', 'SPR', 'XXX', 'TPY', 'WEU', 'FOR', 'ETI', 'SPR', 'XXX', 'TPY', 'WEU', 'FOR', '899985', 3, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202403, '',    'RRR', 1, 1, 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', '899985', 4, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202406, 'CAT', 'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT5', 'TPY', 'WEU', 'LOC', '899985', 5, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '',    'RRR', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899985', 6, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				UPDATE [{ScriptDbName}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'CAT'
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202406, CompanyPK1, inclZeroBal: "", reportType: "PNL");
			AssertEquals("No data if ReportSection is not in ('OV', 'AP', 'TS')", 0, result.Rows.Count);

			sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'OV' where AlternateGLAccountKey = 1;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'AP' where AlternateGLAccountKey = 2;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'TS' where AlternateGLAccountKey = 3;
			");
			TestConnection.ExecuteNonQuery(sqlText);
			result = Execute(ReportingBookID1, 202406, CompanyPK1, inclZeroBal: "Y", reportType: "PNL");
			AssertEquals("Has data if ReportSection is in ('OV', 'AP', 'TS')", 3, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 1 and CurrentPeriodPresentationJournals is null and YearToPeriod is null and PeriodLastYear = -1 and LastYearToPeriod = -1 and TotalLastYear = -11 and GLAccountDesc = '12345-desc,123456789-desc'").Length);
			AssertEquals(1, result.Select("AccountPK = 2 and CurrentPeriodPresentationJournals is null and PeriodLastYear = -10 and YearToPeriodStandard is null and LastYearToPeriod = -10 and GLAccountDesc = '123456789-desc'").Length);
			AssertEquals(1, result.Select("AccountPK = 3 and CurrentPeriodPresentationJournals = -10 and YearToPeriod = -10 and GLAccountDesc = '123456789-desc'").Length);

			result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL");
			AssertEquals("Has data if ReportSection is in ('OV', 'AP', 'TS')", 2, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 1 and CurrentPeriodPresentationJournals = -10 and YearToPeriod = -11 and PeriodLastYear is null and LastYearToPeriod is null and TotalLastYear is null and GLAccountDesc = '12345-desc,123456789-desc'").Length);
			AssertEquals(1, result.Select("AccountPK = 2 and CurrentPeriodPresentationJournals is null and PeriodLastYear is null and YearToPeriodStandard = -10 and LastYearToPeriod is null and GLAccountDesc = '123456789-desc'").Length);

			sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET DebitCreditCode = 'DR'");
			TestConnection.ExecuteNonQuery(sqlText);
			result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL", nature: "Y");
			AssertEquals(1, result.Select("AccountPK = 1 and CurrentPeriodPresentationJournals = 10 and YearToPeriod = 11 and PeriodLastYear is null and LastYearToPeriod is null").Length);
		}

		public void TestTTLAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 40, 10, 202308, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '', 'RRR', 1, 1, 'ETI', 'SPR', 'XXX', 'TPY', 'WEU', 'FOR', 'ETI', 'SPR', 'XXX', 'TPY', 'WEU', 'FOR', '899985', 3, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel)
				Values
				(100, '12.34.53', '12.34.53', 'BSH','AA1', 1, 1, '', '', '', '', '', '', 1, 100, 'TTL', 'OV', 2),
				(101, '12.34.702', '12.34.702', 'BSH','AA1', 1, 1, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1, 101, 'TTL', 'TS', 2),
				(102, '12.34.704', '12.34.704', 'BSH','AA1', 1, 1, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1, 102, 'TTL', 'TS', 1),
				(103, '12.34.705', '12.34.705', 'BSH','AA1', 1, 1, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1, 103, 'TTL', 'TS', 3);

				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'OV', TotalLevel = 1 where AlternateGLAccountKey = 1;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'AP', TotalLevel = 1 where AlternateGLAccountKey = 2;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'TS', TotalLevel = 1 where AlternateGLAccountKey = 3;
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202306, CompanyPK1, inclZeroBal: "", reportType: "PNL");
			AssertEquals("No data if ReportSection is not in ('OV', 'AP', 'TS')", 7, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 100 and CurrentPeriodStandard = -1 and CurrentPeriodPresentationJournals = 0 and GLAccountDesc is null").Length);
			AssertEquals(1, result.Select("AccountPK = 101 and CurrentPeriodStandard = -20 and CurrentPeriodPresentationJournals = 0 and GLAccountDesc is null").Length);
			AssertEquals(1, result.Select("AccountPK = 102 and CurrentPeriodStandard = 0 and CurrentPeriodPresentationJournals = 0 and GLAccountDesc is null").Length);
			AssertEquals(1, result.Select("AccountPK = 103 and CurrentPeriodStandard = -21 and CurrentPeriodPresentationJournals = 0 and YearToPeriod = -21").Length);
		}

		public void TestCLNAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 40, 10, 202306, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '', 'RRR', 1, 1, 'ETI', 'SPR', 'XXX', 'TPY', 'WEU', 'FOR', 'ETI', 'SPR', 'XXX', 'TPY', 'WEU', 'FOR', '899985', 3, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel)
				Values
				(100, '12.34.53', '12.34.53', 'BSH','AA1', 1, 1, '', '', '', '', '', '', 1, 100, 'CLN', 'OV', 2)

				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'OV', TotalLevel = 1, ConsolidationAccountKey = 100 where AlternateGLAccountKey = 1;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'AP', TotalLevel = 1, ConsolidationAccountKey = 100 where AlternateGLAccountKey = 2;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'TS', TotalLevel = 1, ConsolidationAccountKey = 100 where AlternateGLAccountKey = 3;
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202306, CompanyPK1, inclZeroBal: "", reportType: "PNL");
			AssertEquals("CLN Account is here", 4, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 100 and CurrentPeriod = -31").Length);
			AssertEquals(1, result.Select("AccountPK = 1 and CurrentPeriod = -11").Length);
			AssertEquals(1, result.Select("AccountPK = 2 and CurrentPeriod = -10").Length);
			AssertEquals(1, result.Select("AccountPK = 3 and CurrentPeriod = -10").Length);
		}

		public void TestALTAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 40,  5, 202403, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202403, '', 'RRR', 1, 1, 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', '899985', 4, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202403, '', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 0, -3, 202403, '', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel, DebitCreditCode)
				Values
				(100, '1002301', '12.34.53', 'BSH','AA1', 1, 1, '', '', '', '', '', '', 1, 100, 'ALT', 'OV', 2, 'DR'),
				(101, '1002330', '12.34.702', 'BSH','AA1', 1, 1, '', '', '', '', '', '', 1, 101, 'ALT', 'TS', 2, 'CR')

				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'OV', TotalLevel = 1, DebitCreditCode = 'CR', AlternateAccountKey = 100 where AlternateGLAccountKey = 1;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'AP', TotalLevel = 1, DebitCreditCode = 'CR', AlternateAccountKey = 100 where AlternateGLAccountKey = 4;
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'TS', TotalLevel = 1, DebitCreditCode = 'DR', AlternateAccountKey = 101 where AlternateGLAccountKey = 6;
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "TBS");
			AssertEquals("ALT only get amount from BSH with right DR/CR value", 4, result.Rows.Count);
			AssertEquals(1, result.Select("AccountPK = 100 and CurrentPeriodStandard = -10 and CurrentPeriodPresentationJournals is null and CurrentPeriod = -10").Length);
			AssertEquals(1, result.Select("AccountPK = 101 and CurrentPeriodStandard = 2 and CurrentPeriodPresentationJournals is null and CurrentPeriod = 2").Length);
			AssertEquals(1, result.Select("AccountPK = 1 and CurrentPeriodStandard = -5").Length);
			AssertEquals(1, result.Select("AccountPK = 19 and YearToPeriod = 1").Length);
		}

		public void TestNTEAccount()
		{
			TestHelper.InsertGLAccount("12345671", "HDR");
			TestHelper.InsertGLAccount("12345661", "NTE");
			TestHelper.InsertGLAccount("12345681", "NTE");
			TestHelper.InsertBASAccount(8, "GL_BS_ACCOUNT_START");
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2,  9, 30, 40,  5, 202403, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 100, 'NTE', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2,  9, 30, 40, 10, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 100, 'NTE', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 10, 3, 4,    1, 202303, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 101, 'NTE', 1, 1, 1, 1,  1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 10, 3, 0,   -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 101, 'NTE', 1, 1, 1, 1,  1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel, ParentGLAccountNo)
				Values
				(100, '1002301', '12.34.530', 'BSH','AA1', 1,  9, '', '', '', '', '', '', 1, 100, 'NTE', 'OV', 2, '12345631'),
				(101, '1002330', '12.34.802', 'BSH','AA1', 1, 10, '', '', '', '', '', '', 1, 101, 'NTE', 'TS', 2, '12345681')
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "TBS");
			AssertEquals("Two NTE and PL Appropriation Account", 3, result.Rows.Count);
			AssertEquals("NTE actions like BSH if NTE > BSHStartAccount", 1, result.Select("AccountPK = 101 and YearToPeriod = 2 and CurrentPeriod = 3").Length);
			AssertEquals("NTE actions like P&L if NTE < BSHStartAccount", 1, result.Select("AccountPK = 100 and YearToPeriod = -5 and CurrentPeriod = -5").Length);
		}

		public void TestPLAppropriationAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 7, 30, 40, 5, 202403, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 100, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 1, 30, 40, 5, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 1, 30, 40, 5, 202401, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 6, 30, 40, 10, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 101, 'NTE', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 3, 0, -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 13, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				DELETE FROM [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] WHERE GLAccountKey = 7
				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel, ParentGLAccountNo)
				Values
				(100, '1002301', '12.34.53', 'BSH','AA1', 1, 7, '', '', '', '', '', '', 1, 100, 'BSH', 'OV', 2, '12345631'),
				(101, '1002330', '12.34.802', 'BSH','AA1', 1, 6, '', '', '', '', '', '', 1, 101, 'NTE', 'TS', 2, '12345681')
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "TBS");
			AssertEquals("Two data", 2, result.Rows.Count);
			AssertEquals("Normal P&L Account", 1, result.Select("AccountPK = 13 and YearToPeriod = 2 and CurrentPeriod = -3").Length);
			AssertEquals("PLAppropriationAccount", 1, result.Select("AccountPK = 100 and YearToPeriod = -11 and CurrentPeriod = -5").Length);
		}

		public void TestRUPAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 35,  5, 202401, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 35,  5, 202403, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202403, '', 'RRR', 1, 1, 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', '899985', 4, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202401, '', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 0, -3, 202403, '', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 6, 30, 40, 10, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 98, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 3, 0, -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 98, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 0, -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 102, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel, ParentGLAccountNo)
				Values
				(98, '12.24', '12.24', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 98, 'P&L', 'OV', 2, '12345631'),
				(99, '12.24', '12.24', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 99, 'HDR', 'OV', 2, '12345631'),
				(100, '12.34', '12.34', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 100, 'RUP', 'OV', 2, '12345631'),
				(102, '12.34.91', '12.34.91', 'BSH','AA1', 1, 2, '', '', '', '', '', '', 1, 102, 'BSH', 'LI', 2, '12345631'),
				(101, '12.44.802', '12.44.802', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 101, 'RUP', 'LI', 2, '12345681')
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] set ReportSection = 'OV' WHERE AlternateGLAccountKey IN (1,2,3)
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] set ReportSection = 'LI' WHERE AlternateGLAccountKey IN (4,5,6)
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "PNL");
			AssertEquals("RUP in PNL Report", 3, result.Rows.Count);
			AssertEquals("RUP Account", 1, result.Select("AccountPK = 100 and YearToPeriod = -10 and CurrentPeriod = -5 and AccountType = 'RUP'").Length);
			AssertEquals("No sub accounts", 0, result.Select("AccountPK = 1").Length);
			AssertEquals("Normal Account", 1, result.Select("AccountPK = 98 and YearToPeriod = 3 and CurrentPeriod = 3 and PeriodLastYear = -10").Length);
			AssertEquals("HDR Account", 1, result.Select("AccountPK = 99 and YearToPeriod is null and CurrentPeriod is null").Length);
		}

		public void TestGRPAccount()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 5, 30, 35, 5, 202401, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 35, 5, 202403, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202403, '', 'RRR', 1, 1, 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', 'STI', 'SPS', 'YYY', 'INT', 'OEU', 'FOR', '899985', 4, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202401, '', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 0, -3, 202403, '', 'RRB', 1, 1, 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR', 'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 6, 30, 40, 10, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 98, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 3, 0, -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 98, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 6, 30, 40, 10, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985', 99, 'BSH', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 3, 0, -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 99, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 0, -3, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 102, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel, ParentGLAccountNo)
				Values
				(98, '32.2411', '32.2411', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 98, 'BSH', 'LI', 2, '12345631'),
				(99, '32.242', '32.242', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 99, 'BSH', 'LI', 2, '12345631'),
				(100, '32.24', '32.24', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 100, 'RUP', 'LI', 2, '12345631'),
				(102, '12.34.91', '12.34.91', 'BSH','AA1', 1, 2, '', '', '', '', '', '', 1, 102, 'BSH', 'OV', 2, '12345631'),
				(101, '32.241', '32.241', 'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 101, 'RUP', 'LI', 2, '12345681')
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] set ReportSection = 'OV' WHERE AlternateGLAccountKey IN (1,2,3)
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] set ReportSection = 'LI' WHERE AlternateGLAccountKey IN (4,5,6)
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "BSH");
			AssertEquals("RUP in BSH Report", 5, result.Rows.Count);
			AssertEquals("RUP Account", 1, result.Select("AccountPK = 100 and YearToPeriod = 14 and CurrentPeriod = -6 and AccountType = 'RUP' and PeriodLastYearStandard = 20").Length);
			AssertEquals("Normal Account", 1, result.Select("AccountPK = 4 and YearToPeriod = 10 and CurrentPeriod = 10 and PeriodLastYear is null").Length);
			AssertEquals("Normal Account", 1, result.Select("AccountPK = 6 and YearToPeriod = -2 and CurrentPeriod = -3 and PeriodLastYear is null").Length);
			AssertEquals("HDR/RUP Account with zero Amount", 2, result.Select("AccountPK in (15,17) and CurrentPeriod is null").Length);
			AssertEquals("Amount has been summed into RUP Account", 0, result.Select("AccountPK in (99,98)").Length);
		}

		public void TestTranslatedAmount()
		{
			TestHelper.InsertBASAccount(4, "GL_BS_ACCOUNT_START");
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 2, 1, 0, 20, 20, 202401, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 1, 1, 10, 'USD', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 1, 0, 20, 20, 202403, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 1, 1, 10, 'USD', 'BAL', 'SEL', 1000, 0, 'CNY'),
				(1, 1, 2, 4, 0, 50, 50, 202403, '', 'RRR', 1, 1, 'STI', 'SPS',   'YYY', 'INT', 'OEU', 'FOR', 'STI', 'SPS',   'YYY', 'INT', 'OEU', 'FOR', '899985', 4, 'BSH', 1, 1, 1, 1, 10, 'USD', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 0,  5,  5, 202401, '', 'RRB', 1, 1, 'STI', 'SPR',   'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR',   'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1,  1, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 0,  5,  5, 202403, '', 'RRB', 1, 1, 'STI', 'SPR',   'YYY', 'TPY', 'LOC', 'LOC', 'STI', 'SPR',   'YYY', 'TPY', 'LOC', 'LOC', '899989', 6, 'BSH', 1, 1, 1, 1,  1, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 1, 0, 10, 10, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985',  98, 'NTE', 1, 1, 1, 1, 10, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 0,  1,  1, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989',  98, 'NTE', 1, 1, 1, 1,  1, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 2, 0, 50, 50, 202303, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899985',  99, 'BSH', 1, 1, 1, 1, 10, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 0,  5,  5, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989',  99, 'BSH', 1, 1, 1, 1,  1, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 0,  5,  5, 202403, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899989', 102, 'BSH', 1, 1, 1, 1,  1, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel, ParentGLAccountNo)
				Values
				(98,  '32.2411',  '32.2411',  'BSH','AA1', 1,    1, '', '', '', '', '', '', 1,  98, 'NTE', 'LI', 2, '12345631'),
				(99,  '32.242',   '32.242',   'BSH','AA1', 1,    2, '', '', '', '', '', '', 1,  99, 'BSH', 'LI', 2, '12345631'),
				(100, '32.24',    '32.24',    'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 100, 'RUP', 'LI', 2, '12345631'),
				(102, '12.34.91', '12.34.91', 'BSH','AA1', 1,    5, '', '', '', '', '', '', 1, 102, 'BSH', 'OV', 2, '12345631'),
				(101, '32.241',   '32.241',   'BSH','AA1', 1, null, '', '', '', '', '', '', 1, 101, 'RUP', 'LI', 2, '12345681')
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] set ReportSection = 'OV' WHERE AlternateGLAccountKey IN (1,2,3)
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] set ReportSection = 'LI' WHERE AlternateGLAccountKey IN (4,5,6)

				INSERT [{ScriptDbName}].Finance.CUS__ExchangeRatesforReportingbook
				(ExchangeRatesforReportingbookKey, CompanyKey, StartDate, EndDate, ExRateType, Currency, SellRate)
				VALUES
				(1, 1, '2022-02-01', '2024-12-01', 'SEL', 'USD', 2),
				(2, 1, '2022-02-01', '2024-12-01', 'BUY', 'USD', 5)

				INSERT [{ScriptDbName}].Finance.CUS__CurrencyTranslationForReportingBook
				(CurrencyTranslationForReportingBookKey, CompanyKey, AccReportingBookKey, AccountType, ExRateType, CurrencyTranslationLevel, AlternateGLAccountKey)
				VALUES
				(1, 1, 1, 'P&L', 'SEL', 'BAL', null),
				(2, 1, 1,    '', 'SEL', 'BAL', 1),
				(3, 1, 1,    '', 'BUY', 'BAL', 4),
				(4, 1, 1, 'BSH', 'BUY', 'BAL', null)

				UPDATE [{ScriptDbName}].Finance.BAS__AccReportingBook SET RX_NKCurrency = 'USD'
			");
			TestConnection.ExecuteNonQuery(sqlText);

			var result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "BSH");
			AssertEquals("Total rows", 6, result.Rows.Count);
			AssertEquals("HDR Account", 1, result.Select("AccountPK = 15 and CurrentPeriod is null and AccountType = 'HDR'").Length);
			AssertEquals("RUP Account", 1, result.Select("AccountPK = 100 and YearToPeriod = 11 and CurrentPeriod = 1 and AccountType = 'RUP' and PeriodLastYearStandard = 10 and GLAccountKey is null").Length);
			AssertEquals("BSH Account", 1, result.Select("AccountPK = 4 and YearToPeriod = 10 and CurrentPeriod = 10 and LocalCurrentPeriod = 50 and PeriodLastYear is null and TranslatedCurrency = 'USD'").Length);
			AssertEquals("BSH Account", 1, result.Select("AccountPK = 6 and YearToPeriod = 2 and LocalYearToPeriod = 10 and CurrentPeriod = 1 and LocalCurrentPeriod = 5 and PeriodLastYear is null and TranslatedCurrency = 'USD'").Length);
			AssertEquals("NTE Account", 1, result.Select("AccountPK = 98 and YearToPeriod = 11 and LocalYearToPeriod = 11 and CurrentPeriod = 1 and LocalCurrentPeriod = 1 and PeriodLastYear =10 and TranslatedCurrency = 'USD'").Length);

			result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "PNL");
			AssertEquals("P&L Account", 1, result.Select("CurrentPeriod = -10 and LocalCurrentPeriod = -20 and YearToPeriod = -20 and LocalYearToPeriod = -40 and CurrentDebit is null and CurrentCredit is null and TranslatedCurrency = 'USD'").Length);

			result = Execute(ReportingBookID1, 202403, CompanyPK1, inclZeroBal: "", reportType: "TBS");
			AssertEquals("P&L Account", 1, result.Select("AccountPK = 1 and CurrentPeriod = -10 and LocalCurrentPeriod = -20 and YearToPeriod = -20 and LocalYearToPeriod = -40 and TotalLastYear = 0.5 and CurrentDebit =10 and LocalCurrentDebit = 20 and CurrentCredit is null and LocalCurrentCredit is null and ClosingDebit = 20 and LocalClosingDebit = 40 and ClosingDebitForCalculateTotal = 20 and LocalClosingDebitForCalculateTotal = 40 and ClosingCreditForCalculateTotal is null").Length);
			AssertEquals("BSH Account", 1, result.Select("AccountPK = 4 and CurrentPeriod = -10 and LocalCurrentPeriod = -50 and YearToPeriod = -10 and LocalYearToPeriod = -50 and CurrentDebit =10 and CurrentCredit is null and LocalCurrentDebit = 50").Length);
			AssertEquals("NTE Account", 1, result.Select("AccountPK = 98 and CurrentDebit =1 and CurrentCredit is null and LocalCurrentDebit = 1 and CurrentCreditForCalculateTotal = 0").Length);
			AssertEquals("TranslatedCurrency", 7, result.Select("TranslatedCurrency = 'USD'").Length);
		}

		#region TSGroupBy

		public void TestTsGroupByBranch()
		{
			PrepareTsGroupByData();
			var result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL", tsGroupBy: "Branch");
			AssertEquals("Two branch", 2, result.Rows.Count);
			AssertEquals("First branch SYN", 1, result.Select("BranchCode = 'SYN' and CurrentPeriod = 16 and CurrentPeriodStandard = 8 and YearToPeriod = -4 and YearToPeriodStandard = -2 and PeriodLastYear = 16 and PeriodLastYearStandard = 8 and LastYearToPeriod = -4 and YearToPeriodDebit = -3 and YearToPeriodCredit = -1 and PeriodLastYearDebit = 16 and LastYearToPeriodDebit = -4").Length);
			AssertEquals("Second branch SYC", 1, result.Select("BranchCode = 'SYC' and CurrentPeriod = -10 and LastYearToPeriodCredit = 8 and TotalLastYearCredit = 8 and YearToPeriodCredit = 4 and CurrentPeriodCredit = -10 and TotalLastYearDebit is null").Length);

			result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL", tsGroupBy: "Branch", branchManagementCode: "IUR");
			AssertEquals("No data if branch management code is wrong", 0, result.Rows.Count);

			var sql = $@"UPDATE [{ScriptDbName}].Organization.BAS__Branch SET AccountingGroupCode = 'IUR'";
			TestConnection.ExecuteNonQuery(sql);
			result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL", tsGroupBy: "Branch", branchManagementCode: "IUR");
			Assert("Has data if branch management code is right", result.Rows.Count > 0);
		}

		public void TestTsGroupByDepartment()
		{
			PrepareTsGroupByData();
			var result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL", tsGroupBy: "Department");
			AssertEquals("Two department", 2, result.Rows.Count);
			AssertEquals("First department AU1", 1, result.Select("DepartmentCode = 'AU1' and CurrentPeriod = 16 and CurrentPeriodStandard = 8 and YearToPeriod = -4 and YearToPeriodStandard = -2 and PeriodLastYear = 16 and PeriodLastYearStandard = 8 and LastYearToPeriod = -4 and YearToPeriodDebit = -3 and YearToPeriodCredit = -1 and PeriodLastYearDebit = 16 and LastYearToPeriodDebit = -4 and TotalLastYearCredit is null").Length);
			AssertEquals("Second department TW1", 1, result.Select("DepartmentCode = 'TW1' and CurrentPeriod = -10 and LastYearToPeriodCredit = 8 and TotalLastYearCredit = 8 and YearToPeriodCredit = 4 and CurrentPeriodCredit = -10 and TotalLastYearDebit is null and PeriodLastYearCredit = -10").Length);
		}

		public void TestTsGroupByMgt()
		{
			PrepareTsGroupByData();
			var result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "", reportType: "PNL", tsGroupBy: "Mgt");
			AssertEquals("Two department", 2, result.Rows.Count);
			AssertEquals("First branchCode IU1", 1, result.Select("BranchCode = 'IU1' and CurrentPeriod = 16 and CurrentPeriodStandard = 8 and YearToPeriod = -4 and YearToPeriodStandard = -2 and PeriodLastYear = 16 and PeriodLastYearStandard = 8 and LastYearToPeriod = -4 and YearToPeriodDebit = -3 and YearToPeriodCredit = -1 and PeriodLastYearDebit = 16 and LastYearToPeriodDebit = -4 and TotalLastYearCredit is null").Length);
			AssertEquals("Second branchCode IU2", 1, result.Select("BranchCode = 'IU2' and CurrentPeriod = -10 and LastYearToPeriodCredit = 8 and TotalLastYearCredit = 8 and YearToPeriodCredit = 4 and CurrentPeriodCredit = -10 and TotalLastYearDebit is null and PeriodLastYearCredit = -10").Length);
		}

		public void TestTTLAccountWithGroupBy()
		{
			PrepareTsGroupByData();
			var sql = $@"INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, Attribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, ReportSection, TotalLevel)
				Values
				(200, '12.34.63', '12.34.53', 'TTL', 'AA2', 2, 1, '', '', '', '', '', '', 1, 200, 'TTL', 'OV', 2),
				(201, '12.34.63', '12.34.53', 'HDR', 'AA3', 3, 1, '', '', '', '', '', '', 1, 201, 'HDR', 'OV', 2),
				(202, '12.34.63', '12.34.53', 'TTL', 'AA4', 4, 1, '', '', '', '', '', '', 1, 202, 'TTL', 'OV', 2)
				";
			TestConnection.ExecuteNonQuery(sql);

			AssertGroupByResult("Branch");
			AssertGroupByResult("Mgt");
			AssertGroupByResult("Department");

			void AssertGroupByResult(string groupBy)
			{
				var result = Execute(ReportingBookID1, 202308, CompanyPK1, inclZeroBal: "Y", reportType: "PNL", tsGroupBy: groupBy, splitDebitCredit: "N");
				AssertEquals("Three accounts not in Alternate chart 1", 0, result.Select("AccountPK in (200, 201, 202)").Length);
				AssertEquals("Accounts in Alternate Chart 1", 5, result.Rows.Count);
			}
		}

		void PrepareTsGroupByData()
		{
			var sql = $@"UPDATE [{ScriptDbName}].[Finance].[BAS__AccReportingBook] SET IncludePresentationJournals = 'EET'
				UPDATE [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData] SET ReportSection = 'TS'
				UPDATE [{ScriptDbName} ].[Organization].[BAS__Branch] SET AccountingGroupCode = 'IU1' WHERE BranchCode = 'SYN'
				UPDATE [{ScriptDbName} ].[Organization].[BAS__Branch] SET AccountingGroupCode = 'IU2' WHERE BranchCode = 'SYC'
				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(3, 1, 2, 5, 3, 4, 1, 202308,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 6, 1,  5, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 5, 2, 202306,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 8, 8, 1, -7, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 6, 3, 202306,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 0, 9,  9, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 7, 4, 202308,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 9, 1, -8, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 4, 1, 202208,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 6, 1,  5, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 5, 2, 202206,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 8, 8, 1, -7, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 6, 3, 202206,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 0, 9,  9, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 7, 4, 202208,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 9, 1, -8, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 4, 1, 202308, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 6, 1,  5, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 5, 2, 202306, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 8, 8, 1, -7, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 6, 3, 202306, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 0, 9,  9, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 7, 4, 202308, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 9, 1, -8, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 4, 1, 202208, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 6, 1,  5, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 5, 2, 202206, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 8, 8, 1, -7, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 6, 3, 202206, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 0, 9,  9, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 7, 4, 202208, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 9, 1, -8, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 4, 1, 202201,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 6, 1,  5, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 5, 2, 202201,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 8, 8, 1, -7, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 6, 3, 202201,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 0, 9,  9, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 7, 4, 202201,	 '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 9, 1, -8, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 4, 1, 202202, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 1, 6, 1,  5, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 2, 5, 3, 5, 2, 202202, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 1, 8, 8, 1, -7, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 6, 3, 202202, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 0, 9,  9, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 5, 3, 7, 4, 202202, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899985', 2, 'P&L', 1, 1, 9, 1, -8, 'USD', 'JNL', 'SEL', 1000, 1, 'CNY')
				";
			TestConnection.ExecuteNonQuery(sql);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
		}

		void PrepareData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Company];
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Branch];
				DELETE FROM [{ScriptDbName}].[Organization].[BAS__Department];
				DELETE FROM [{ScriptDbName}].Finance.BAS__ALternateGLAccount;
				DELETE FROM [{ScriptDbName}].Finance.BAS__AlternateChart;
			");
			TestConnection.ExecuteNonQuery(sqlText);

			CompanyPK1 = TestHelper.InsertCompany("CNY", "DYY", countryCode: "YY");
			CompanyPK2 = TestHelper.InsertCompany("CNY", "DXX", countryCode: "XX");

			sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[Geography].[BAS__Country];
				DELETE FROM [{ScriptDbName}].[Finance].[BAS__GLAccount];

				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 1, 2, 3, 4, 1, 202305, 'EET', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(2, 2, 1, 2, 2, 3, 1, 202405, 'EEY', 'RRW', 2, 1, 'STI', 'SPR', 'INTT2', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', '899988', 12, 'P&L', 2,1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 3, 1, 3, 3, 4, 1, 202405, 'EET', 'RRR', 2, 1, 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899987', 1, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 4, 3, 4, 1, 202605, 'EET', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899986', 2, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(2, 1, 2, 5, 3, 4, 1, 202306, 'CAT', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 1, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, 'CAT', 'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 3, 4, 1, 202306, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 1, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1'),
						(2, newid(), 'TW1');
				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey], [CompanyID])
					VALUES
						(1, newid(), 1, 'SYN', 1, '{CompanyPK1}'),
						(2, newid(), 2, 'DTW', 2, '{CompanyPK2}'),
						(3, newid(), 1, 'SYC', 1, '{CompanyPK1}');

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, ATtribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, DebitCreditCode)
				Values
				(1,  '12.34.5',  '12.34.5', 'BSH','AA1', 1, 1, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1,  1, 'P&L', 'CR'),
				(2,  '12.34.6',  '12.34.6', 'BSH','AA2', 1, 2, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1,  2, 'P&L', 'DR'),
				(3,  '12.34.7',  '12.34.7', 'P&L','AA3', 1, 3,   'XXX', 'FOR', 'WEU', 'ETI', 'SPR', 'TPY', 1,  3, 'P&L', 'DR'),
				(4, '12.34.72', '12.34.72', 'BSH','AA4', 1, 4,   'YYY', 'FOR', 'OEU', 'STI', 'SPS', 'INT', 1,  4, 'BSH', 'DR'),
				(5,  '12.34.8',  '12.34.8', 'BSH','AA5', 1, 5,   'XXX', 'LOC', 'LOC', 'ETI', 'SPR', 'TPY', 1,  5, 'BSH', 'DR'),
				(6,  '12.34.9',  '12.34.9', 'BSH','AA6', 1, 1,   'YYY', 'LOC', 'LOC', 'STI', 'SPR', 'TPY', 1,  6, 'BSH', 'DR'),
				(7,  '22.34.5',  '22.34.5', 'BSH','BA1', 2, 1,   'XXX', 'LOC', 'LOC', 'ETI', 'SPS', 'TPY', 2,  7, 'P&L', 'DR'),
				(8,  '22.34.6',  '22.34.6', 'BSH','BA2', 2, 2,   'YYY', 'LOC', 'LOC', 'STI', 'SPR', 'INT', 2,  8, 'P&L', 'DR'),
				(9,  '22.34.7',  '22.34.7', 'P&L','BA3', 2, 3,   'XXX', 'FOR', 'WEU', 'ETI', 'SPR', 'INT', 2,  9, 'P&L', 'DR'),
				(10, '22.34.1',  '22.34.1', 'BSH','BA4', 2, 4,   'YYY', 'FOR', 'OEU', 'ETI', 'SPS', 'INT', 2, 10, 'BSH', 'DR'),
				(11, '22.34.8',  '22.34.8', 'BSH','BA5', 2, 5,   'XXX', 'LOC', 'LOC',  null,  null, 'TPY', 2, 11, 'P&L', 'DR'),
				(12, '22.34.9',  '22.34.9', 'BSH','BA6', 2, 1,   'YYY', 'LOC', 'LOC',  null,  null, 'TPY', 2, 12, 'P&L', 'DR'),
				(13,'22.34.55', '22.34.55', 'BSH','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 13, 'P&L', 'DR'),
				(14,'22.34.10', '22.34.10', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 14, 'CHT', 'DR'),
				(15,'12.34.71', '12.34.71', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 15, 'HDR', 'DR'),
				(16,'22.34.70', '22.34.70', 'CHT','BA6', 2, 1,      '',    '',    '',    '',    '',    '', 1, 16, 'HDR', 'DR'),
				(17, '22.34.9',  '22.34.9', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 17, 'RUP', 'DR'),
				(18, '22.34.9',  '22.34.9', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 18, 'GRP', 'DR'),
				(19, '22.34.9',  '22.34.9', 'BSH','BA6', 1, 7,      '',    '',    '',    '',    '',    '', 1, 19, 'BSH', 'DR'),
				(20,'22.34.91', '22.34.91', 'BSH','BA7', 2, 7,      '',    '',    '',    '',    '',    '', 2, 20, 'BSH', 'DR')
				"
			);
			TestConnection.ExecuteNonQuery(sqlText);

			TestHelper.InsertCountry("XX");
			TestHelper.InsertCountry("YY", economicGrouping: "UUU");

			TestHelper.InsertBASAccount(1, "GL_GST_INPUT_ACCOUNT");
			TestHelper.InsertBASAccount(2, "GL_GST_OUTPUT_ACCOUNT");
			TestHelper.InsertBASAccount(3, "GL_PENDING_GST_INPUT_ACCOUNT");
			TestHelper.InsertBASAccount(4, "GL_PENDING_GST_OUTPUT_ACCOUNT");
			TestHelper.InsertBASAccount(7, "GL_PL_APPROPRIATION_ACCOUNT");
			TestHelper.InsertGLAccount("12345");
			TestHelper.InsertGLAccount("123456");
			TestHelper.InsertGLAccount("1234567");
			TestHelper.InsertGLAccount("12345678");
			TestHelper.InsertGLAccount("123456789");
			TestHelper.InsertGLAccount("12345661", "NTE");
			TestHelper.InsertGLAccount("12345681", "BSH");

			ReportingBookID1 = TestHelper.InsertReportingBook(1, 1);
			ReportingBookID2 = TestHelper.InsertReportingBook(2, 2, "YYY");
			TestHelper.InsertPeriodForInputYear(2022);
			TestHelper.InsertPeriodForInputYear(2023);
			TestHelper.InsertPeriodForInputYear(2024);
			TestHelper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2020, 2, 1), 1);
			TestHelper.InsertCurrency("USD");

			var alternateGLAccountPK1 = TestHelper.InsertAlternateGLAccount("20.11.00", "HDR", alternateGLAccountKey: 15);
			var alternateGLAccountPK2 = TestHelper.InsertAlternateGLAccount("22.34.10", "HDR", 2, alternateGLAccountKey: 16);

			TestHelper.InsertALternateChart("CHL", secondAccountID: alternateGLAccountPK1, companyID: CompanyPK1);
			TestHelper.InsertALternateChart("CHG", reportOrder: "BTP", secondAccountID: alternateGLAccountPK2, companyID: CompanyPK2);
		}

		DataTable Execute(Guid reportingBookPK, int period, Guid companyPK, string departmentList = "", string branchList = "", string inclZeroBal = "Y", string reportType = "TBS", string tsGroupBy = "", string nature = "", string branchManagementCode = "", string splitDebitCredit = "Y")
		{
			var sqlText = $@"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]
						'{reportingBookPK}',{period}, '{companyPK}', '{departmentList}', '{branchList}', '{inclZeroBal}', '{reportType}', '{tsGroupBy}', '{nature}', '{branchManagementCode}', '{splitDebitCredit}'";

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		Guid CompanyPK1, CompanyPK2, ReportingBookID1, ReportingBookID2;

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
