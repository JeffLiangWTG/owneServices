using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLossReportPeriodAnalysisReportingBook))]
	class ProfitAndLossReportPeriodAnalysisReportingBookTest : BiCreateScriptTest
	{
		public void TestColumnsOfProfitAndLossReportPeriodAnalysis()
		{
			var whiteList = new List<string>
			{
				"AccountNumber",
				"AccountName",
				"AccountType",
				"Units",
				"PrintSequence",
				"AccountNumberForSequence",
				"AdditionalDissection",
				"ParentAccountNumber"
			};
			var msgToHint = "Please confirm the columns of ProfitAndLossReportPeriodAnalysisReportingBook";

			var result = Execute(ReportingBookPK, 202301, CompanyPK);
			Assert(msgToHint, result.Columns.Count >= whiteList.Count);
			foreach (var column in whiteList)
			{
				Assert($@"{msgToHint} Should contain column '{column}'", result.Columns.Contains(column));
			}
		}

		public void TestLastProcessedDate()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK);
			AssertEquals("Should have 1 Rows ", 3, result.Rows.Count);

			result = Execute(ReportingBookPK2, 202305, CompanyPK);
			AssertEquals("Should have 0 Rows ", 0, result.Rows.Count);
		}

		public void TestTBSReport()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "TBS", inclZeroBal: "Y");
			AssertEquals("Should have data Rows ", 11, result.Rows.Count);
			AssertEquals("P&L account", 1, result.Select("AccountPK = 2 and P0 is null and P1 = 1 and P1ForCalculateTotal = 1 and P5 = 1 and P12 is null").Length);
			AssertEquals("BSH account", 1, result.Select("AccountPK = 21 and P0 = 3 and P1 = 1 and P1ForCalculateTotal = 1 and P5 = 1 and P12 is null").Length);
			AssertEquals("NTE account before BSH start account", 1, result.Select("AccountPK = 22 and P0 is null and P5 = 1 and P1ForCalculateTotal = 0").Length);
			AssertEquals("NTE account after BSH start account", 1, result.Select("AccountPK = 23 and P0 =2 and P0ForCalculateTotal =0 and P5 = 1 and P1ForCalculateTotal = 0").Length);
			AssertEquals("PL appropriation account", 1, result.Select("AccountPK = 19 and P0 =2").Length);
		}

		public void TestPNLReport()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "PNL");
			AssertEquals("Should have data Rows ", 3, result.Rows.Count);
			AssertEquals("P&L account", 1, result.Select("AccountPK = 2 and P1 = -1 and P1ForCalculateTotal is null and P5 = -1 and P12 is null").Length);
			AssertEquals("NTE account before BSH start account", 1, result.Select("AccountPK = 22 and P5 = -1 and P1ForCalculateTotal is null").Length);
			AssertEquals("HDR account", 1, result.Select("AccountPK = 15 and P1 is null").Length);
		}

		public void TestBSHReport()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "BSH");
			AssertEquals("Should have data Rows ", 2, result.Rows.Count);
			AssertEquals("P&L account", 1, result.Select("AccountPK = 23 and P1 = 2 and P5 = 3 and P12 is null").Length);
			AssertEquals("NTE account before BSH start account", 1, result.Select("AccountPK = 17 and P1 = 6 and P2 = 7 and P3 = 8").Length);
		}

		public void TestBranchFilter()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "TBS", branchList: "SYN");
			AssertEquals("Should have data Rows ", 5, result.Rows.Count);
			AssertEquals("PL appropriation account", 1, result.Select("AccountPK = 19 and P0 = 1").Length);
			AssertEquals("BSH account", 1, result.Select("AccountPK = 21 and P0 = 1 and P1 = 1 and P1ForCalculateTotal = 1 and P5 = 1 and P12 is null").Length);
		}

		public void TestDepartmentFilter()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "TBS", departmentList: "AU1");
			AssertEquals("Should have data Rows ", 5, result.Rows.Count);
			AssertEquals("PL appropriation account", 1, result.Select("AccountPK = 19 and P0 = 1").Length);
			AssertEquals("BSH account", 1, result.Select("AccountPK = 21 and P0 = 3 and P1 is null and P2 = 1 and P5 = 1 and P12 is null").Length);
		}

		public void TestAdditionalDissectionFilter()
		{
			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "PNL", additionalDissectio: "Branch");
			AssertEquals("Should have data Rows ", 6, result.Rows.Count);
			AssertEquals("SYN branch", 1, result.Select("AccountPK = 22 and AdditionalDissection = 'SYN' and P5 = -1").Length);
			AssertEquals("Empty branch", 1, result.Select("AccountPK = 22 and AdditionalDissection = '' and P5 = -1").Length);
			AssertEquals("Empty branch", 1, result.Select("AccountPK = 2 and AdditionalDissection = '' and P1 = -1 and P2 = -1").Length);
			AssertEquals("SYC branch", 1, result.Select("AccountPK = 2 and AdditionalDissection = 'SYC' and P1 is null and P2 = -1").Length);

			result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "PNL", additionalDissectio: "Department");
			AssertEquals("AU1 department", 1, result.Select("AccountPK = 22 and AdditionalDissection = 'AU1' and P5 = -1").Length);
			AssertEquals("Empty department", 1, result.Select("AccountPK = 22 and AdditionalDissection = '' and P5 = -1").Length);
			AssertEquals("Empty department", 1, result.Select("AccountPK = 2 and AdditionalDissection = '' and P1 = -1 and P2 = -1").Length);
			AssertEquals("TW1 branch", 1, result.Select("AccountPK = 2 and AdditionalDissection = 'TW1' and P1 =-1 and P2 is null").Length);
		}

		public void TestEmptyReportOrder()
		{
			var sql = string.Format(CultureInfo.InvariantCulture, $@"
				Update {ScriptDbName}.Finance.BAS__AlternateChart set ReportOrder = ''
			");
			TestConnection.ExecuteNonQuery(sql);

			var result = Execute(ReportingBookPK, 202305, CompanyPK, reportType: "PNL", additionalDissectio: "Branch");
			AssertEquals("Should have data", 6, result.Rows.Count);
		}

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

			CompanyPK = TestHelper.InsertCompany("CNY", "DYY", countryCode: "YY");
			CompanyPK2 = TestHelper.InsertCompany("CNY", "DXX", countryCode: "XX");

			sqlText = string.Format(CultureInfo.InvariantCulture, $@"
				DELETE FROM [{ScriptDbName}].[Geography].[BAS__Country];
				DELETE FROM [{ScriptDbName}].[Finance].[BAS__GLAccount];

				INSERT [{ScriptDbName}].[Finance].[GRP__ReportingBookGeneralLedgerAggregateData]
				(BranchKey, CompanyKey, DepartmentKey, GLAccountKey, GLAmountLocalCredit, GLAmountLocalDebit, GLAmountLocalBalance, PostPeriod,
TransactionCategory, ReportingBookCode, ReportingBookKey, CompanyOfPeriodKey, Attribute_TIC, Attribute_SPR, Attribute_ORG, Attribute_OCG, Attribute_LFE, Attribute_LFO, OriginalAttribute_TIC, OriginalAttribute_SPR, OriginalAttribute_ORG,
OriginalAttribute_OCG, OriginalAttribute_LFE, OriginalAttribute_LFO, AlternateGLAccount, AlternateGLAccountKey, AccountTypeCode, AlternateChartKey, PeriodManagementKey, TranslatedCredit, TranslatedDebit, TranslatedBalance, TranslatedCurrency, CurrencyTranslationLevel, TranslatedRateType, SubUnitRatio, IsReciprocal, LocalCurrency)
				VALUES
				(1, 1, 1, 2, 3, 4, 1, 202305, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 2, 3, 4, 1, 202301, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 1, 2, 3, 4, 1, 202302, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 2, 3, 4, 1, 202303, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202304, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 1, 2, 3, 4, 1, 202205, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 2, 3, 4, 1, 202206, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202307, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 8, 3, 4, 1, 202305, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 8, 3, 4, 1, 202301, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 1, 8, 3, 4, 1, 202302, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 8, 3, 4, 1, 202303, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 8, 3, 4, 1, 202304, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 1, 8, 3, 4, 1, 202205, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 8, 3, 4, 1, 202206, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 1, 8, 3, 4, 1, 202107, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 2, 3, 4, 1, 202308, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 21, 'BSH', 1, 1, 1, 1, 1, 'CNY', 'BAL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 2, 3, 4, 1, 202309, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989',  2, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(2, 2, 1, 2, 2, 3, 1, 202405, '', 'RRW', 2, 1, 'STI', 'SPR', 'INTT2', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', '899988', 12, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 3, 2, 3, 3, 4, 1, 202405, '', 'RRR', 2, 1, 'STI', 'SPR', 'INTT3', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899987',  1, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 4, 3, 4, 1, 202605, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899986',  2, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(2, 1, 2, 5, 3, 4, 1, 202306, '', 'RRR', 1, 1, 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT4', 'TPY', 'WEU', 'LOC', '899985',  1, 'P&L', 2, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 2, 5, 30, 40, 10, 202306, '', 'RRR', 1, 1, '', '', '', '', '', '', '', '', 'INTT4', 'TPY', 'WEU', 'LOC', '899985', 13, 'P&L', 1, 1, 1, 1, 10, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 1, 3, 4, 1, 202306, '', 'RRB', 1, 1, 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', 'STI', 'SPR', 'INTT1', 'TPY', 'WEU', 'LOC', '899989', 1, 'P&L', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 6,  3, 4, 1, 202305, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899949', 22, 'NTE', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 6,  3, 4, 1, 202205, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899949', 22, 'NTE', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 6,  3, 4, 1, 202105, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899949', 22, 'NTE', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(3, 1, 1, 10, 3, 4, 1, 202305, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899949', 23, 'NTE', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 10, 3, 4, 1, 202205, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899949', 23, 'NTE', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY'),
				(1, 1, 1, 10, 3, 4, 1, 202105, '', 'RRB', 1, 1, '', '', '', '', '', '', '', '', '', '', '', '', '899949', 23, 'NTE', 1, 1, 1, 1, 1, 'CNY', 'JNL', 'SEL', 1000, 1, 'CNY')

				INSERT [{ScriptDbName}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1'),
						(2, newid(), 'TW1');
				INSERT [{ScriptDbName}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [CompanyKey], [BranchCode], [OrganizationKey], [CompanyID])
					VALUES
						(1, newid(), 1, 'SYN', 1, '{CompanyPK}'),
						(2, newid(), 2, 'DTW', 2, '{CompanyPK2}'),
						(3, newid(), 1, 'SYC', 1, '{CompanyPK}');

				INSERT [{ScriptDbName}].[Finance].[CUS__AlternateGLAccountData]
				(AlternateGLAccountDataKey, AccountNumberForTotals, DestinationAccountNo, AccountType, AlternateChartCode, AlternateChartKey, GLAccountKey, Attribute_ORG, Attribute_LFO, Attribute_LFE, Attribute_TIC, ATtribute_SPR, Attribute_OCG, CompanyKey, AlternateGLAccountKey, AccountTypeCode, DebitCreditCode, ReportSection, ParentGLAccountNo)
				Values
				(1,   '12.34.5',  '12.34.5', 'BSH','AA1', 1, 1, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1,  1, 'P&L', 'CR', 'TS', ''),
				(2,   '12.34.6',  '12.34.6', 'BSH','AA2', 1, 2, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1,  2, 'P&L', 'DR', 'TS', ''),
				(3,   '12.34.7',  '12.34.7', 'P&L','AA3', 1, 3,   'XXX', 'FOR', 'WEU', 'ETI', 'SPR', 'TPY', 1,  3, 'P&L', 'DR', 'TS', ''),
				(4,  '12.34.72', '12.34.72', 'BSH','AA4', 1, 4,   'YYY', 'FOR', 'OEU', 'STI', 'SPS', 'INT', 1,  4, 'BSH', 'DR', 'LI', ''),
				(5,   '12.34.8',  '12.34.8', 'BSH','AA5', 1, 5,   'XXX', 'LOC', 'LOC', 'ETI', 'SPR', 'TPY', 1,  5, 'BSH', 'DR', 'LI', ''),
				(6,   '12.34.9',  '12.34.9', 'BSH','AA6', 1, 1,   'YYY', 'LOC', 'LOC', 'STI', 'SPR', 'TPY', 1,  6, 'BSH', 'DR', 'LI', ''),
				(7,   '22.34.5',  '22.34.5', 'BSH','BA1', 2, 1,   'XXX', 'LOC', 'LOC', 'ETI', 'SPS', 'TPY', 2,  7, 'P&L', 'DR', 'TS', ''),
				(8,   '22.34.6',  '22.34.6', 'BSH','BA2', 2, 2,   'YYY', 'LOC', 'LOC', 'STI', 'SPR', 'INT', 2,  8, 'P&L', 'DR', 'TS', ''),
				(9,   '22.34.7',  '22.34.7', 'P&L','BA3', 2, 3,   'XXX', 'FOR', 'WEU', 'ETI', 'SPR', 'INT', 2,  9, 'P&L', 'DR', 'TS', ''),
				(10,  '22.34.1',  '22.34.1', 'BSH','BA4', 2, 4,   'YYY', 'FOR', 'OEU', 'ETI', 'SPS', 'INT', 2, 10, 'BSH', 'DR', 'LI', ''),
				(11,  '22.34.8',  '22.34.8', 'BSH','BA5', 2, 5,   'XXX', 'LOC', 'LOC',  null,  null, 'TPY', 2, 11, 'P&L', 'DR', 'TS', ''),
				(12,  '22.34.9',  '22.34.9', 'BSH','BA6', 2, 1,   'YYY', 'LOC', 'LOC',  null,  null, 'TPY', 2, 12, 'P&L', 'DR', 'TS', ''),
				(13, '22.34.55', '22.34.55', 'BSH','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 13, 'BSH', 'DR', 'LI', ''),
				(14, '22.34.10', '22.34.10', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 14, 'CHT', 'DR', 'TS', ''),
				(15, '22.34.01', '22.34.01', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 15, 'HDR', 'DR', 'TS', ''),
				(16, '22.34.70', '22.34.70', 'CHT','BA6', 2, 1,      '',    '',    '',    '',    '',    '', 1, 16, 'HDR', 'DR', 'LI', ''),
				(17,  '22.34.9',  '22.34.9', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 17, 'RUP', 'DR', 'OE', ''),
				(18,  '22.34.9',  '22.34.9', 'CHT','BA6', 1, 1,      '',    '',    '',    '',    '',    '', 1, 18, 'GRP', 'DR', 'OE', ''),
				(19,  '22.34.9',  '22.34.9', 'BSH','BA6', 1, 7,      '',    '',    '',    '',    '',    '', 1, 19, 'BSH', 'DR', 'OE', ''),
				(20, '22.34.91', '22.34.91', 'BSH','BA7', 2, 7,      '',    '',    '',    '',    '',    '', 2, 20, 'BSH', 'DR', 'OE', ''),
				(21, '22.34.94', '22.34.94', 'BSH','BA8', 1, 8, 'INTT1', 'LOC', 'WEU', 'STI', 'SPR', 'TPY', 1, 21, 'BSH', 'DR', 'OE', ''),
				(22, '12.34.31', '12.34.31', 'NTE','BA9', 1,  6,     '',    '',    '',    '',    '',    '', 1, 22, 'NTE', 'DR', 'TS','1'),
				(23, '32.34.31', '32.34.31', 'NTE','BA9', 1, 10,     '',    '',    '',    '',    '',    '', 1, 23, 'NTE', 'DR', 'OE','4')
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
			TestHelper.InsertBASAccount(9, "GL_BS_ACCOUNT_START");
			TestHelper.InsertGLAccount("12345");
			TestHelper.InsertGLAccount("123456");
			TestHelper.InsertGLAccount("1234567");
			TestHelper.InsertGLAccount("12345678");
			TestHelper.InsertGLAccount("123456789");
			TestHelper.InsertGLAccount("12345661", "NTE");
			TestHelper.InsertGLAccount("12345681", "BSH");
			TestHelper.InsertGLAccount("12345691", "BSH");
			TestHelper.InsertGLAccount("12345662", "BSH");
			TestHelper.InsertGLAccount("12345663", "NTE");

			ReportingBookPK = TestHelper.InsertReportingBook(1, 1);
			ReportingBookPK2 = TestHelper.InsertReportingBook(2, 2, "YYY");
			TestHelper.InsertPeriodForInputYear(2022);
			TestHelper.InsertPeriodForInputYear(2023);
			TestHelper.InsertPeriodForInputYear(2024);
			TestHelper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2020, 2, 1), 1);
			TestHelper.InsertCurrency("USD");
			TestHelper.InsertCurrency("CNY");

			var alternateGLAccountPK1 = TestHelper.InsertAlternateGLAccount("20.11.00", "HDR", alternateGLAccountKey: 15);
			var alternateGLAccountPK2 = TestHelper.InsertAlternateGLAccount("22.34.10", "HDR", 2, alternateGLAccountKey: 16);

			TestHelper.InsertALternateChart("CHL", secondAccountID: alternateGLAccountPK1, companyID: CompanyPK);
			TestHelper.InsertALternateChart("CHG", reportOrder: "BTP", secondAccountID: alternateGLAccountPK2, companyID: CompanyPK2);
		}

		DataTable Execute(Guid reportingBookPK, int period, Guid companyPK, string departmentList = "", string branchList = "", string inclZeroBal = "", string reportType = "PNL", string branchMgtCode = "", string additionalDissectio = "")
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.AppendLine($"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]");
			sqlBuilder.Append(
				$@"@ReportingBookPK = '{reportingBookPK}'");
			sqlBuilder.Append($",@Period = {period}");
			sqlBuilder.Append(
				$@",@CompanyPK = '{companyPK}'");
			sqlBuilder.Append(
				$@",@DepartmentList = ").Append($"'{departmentList}'");
			sqlBuilder.Append(
				$@",@BranchList = ").Append($"'{branchList}'");
			sqlBuilder.Append(
				$@",@InclZeroBal = ").Append($"'{inclZeroBal}'");
			sqlBuilder.Append(
				$@",@ReportType = ").Append($"'{reportType}'");
			sqlBuilder.Append(
				$@",@BranchManagementCode = ").Append($"'{branchMgtCode}'");
			sqlBuilder.Append(
				$@",@AdditionalDissection = ").Append($"'{additionalDissectio}'");

			var sqlText = string.Format(CultureInfo.InvariantCulture, sqlBuilder.ToString());

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName => Db.EdwDatabaseName;
		Guid CompanyPK, CompanyPK2, ReportingBookPK, ReportingBookPK2;
		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
