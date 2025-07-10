using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests
{
	class ProfitAndLossReportTest : ScriptTest
	{
		public void TestOnlyCalculationsForPeriodAnalysis()
		{
			var plAccountPK = TestObjectCreator.InsertGLHeader("2222.00.00", "Test PL Account", "P&L", "", "DR", false, "AP");

			TestObjectCreator.CreateTestPeriodsForEntireYear(2021);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);
			TestObjectCreator.InsertAmountsFor(plAccountPK, 202101, 202312);
			Factory.Save();

			var sqlTemplate = @"
				DECLARE @p1 AS Int = 202206
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = ''		-- PresentationCategory
				DECLARE @p10 AS NVarChar(100) = null
				DECLARE @p11 AS varchar(4000) = '{0}'  -- OnlyCalculationsForPeriodAnalysis
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', '{1}', '', '', '', @p7, '', @p8, @p9, @p10, 'Y', '', @p11";

			// Control case
			var resultPnl = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, "", "PNL"));
			var pnlRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is empty, all figures should be calculated.", () =>
			{
				AssertRowEquals(pnlRow, "CurrentPeriod", 59m);
				AssertRowEquals(pnlRow, "YearToPeriod", 280m);
				AssertRowEquals(pnlRow, "PeriodLastYear", 11m);
				AssertRowEquals(pnlRow, "LastYearToPeriod", 32m);
				AssertRowEquals(pnlRow, "TotalLastYear", 164m);
			});

			// Report Types
			var resultPnlCurrentPeriod = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":CurrentPeriod:", "PNL"));
			var pnlCurrentPeriodRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is CurrentPeriod, only CurrentPeriod figures should be calculated", () =>
			{
				AssertRowEquals(pnlCurrentPeriodRow, "CurrentPeriod", 59m);
				AssertColumnIsNullOrZero(resultPnlCurrentPeriod, "YearToPeriod");
				AssertColumnIsNullOrZero(resultPnlCurrentPeriod, "PeriodLastYear");
				AssertColumnIsNullOrZero(resultPnlCurrentPeriod, "LastYearToPeriod");
				AssertColumnIsNullOrZero(resultPnlCurrentPeriod, "TotalLastYear");
			});

			var resultBshCurrentPeriod = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":CurrentPeriod:", "BSH"));
			var bshCurrentPeriodRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is CurrentPeriod, only CurrentPeriod figures should be calculated", () =>
			{
				AssertRowEquals(bshCurrentPeriodRow, "CurrentPeriod", 59m);
				AssertColumnIsNullOrZero(resultBshCurrentPeriod, "YearToPeriod");
				AssertColumnIsNullOrZero(resultBshCurrentPeriod, "PeriodLastYear");
				AssertColumnIsNullOrZero(resultBshCurrentPeriod, "LastYearToPeriod");
				AssertColumnIsNullOrZero(resultBshCurrentPeriod, "TotalLastYear");
			});

			var resultTbsCurrentPeriod = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":CurrentPeriod:", "TBS"));
			var tbsCurrentPeriodRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is CurrentPeriod, only CurrentPeriod figures should be calculated", () =>
			{
				AssertRowEquals(tbsCurrentPeriodRow, "CurrentPeriod", 59m);
				AssertColumnIsNullOrZero(resultTbsCurrentPeriod, "YearToPeriod");
				AssertColumnIsNullOrZero(resultTbsCurrentPeriod, "PeriodLastYear");
				AssertColumnIsNullOrZero(resultTbsCurrentPeriod, "LastYearToPeriod");
				AssertColumnIsNullOrZero(resultTbsCurrentPeriod, "TotalLastYear");
			});

			// Filter combinations (note that CurrentPeriod is tested above)
			var resultYearToPeriod = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":YearToPeriod:", "PNL"));
			var yearToPeriodRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is PeriodLastYear, only PeriodLastYear figures should be calculated", () =>
			{
				AssertColumnIsNullOrZero(resultYearToPeriod, "CurrentPeriod");
				AssertRowEquals(yearToPeriodRow, "YearToPeriod", 280m);
				AssertColumnIsNullOrZero(resultYearToPeriod, "PeriodLastYear");
				AssertColumnIsNullOrZero(resultYearToPeriod, "LastYearToPeriod");
				AssertColumnIsNullOrZero(resultYearToPeriod, "TotalLastYear");
			});

			var resultPeriodLastYear = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":PeriodLastYear:", "PNL"));
			var periodLastYearRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is PeriodLastYear, only PeriodLastYear figures should be calculated", () =>
			{
				AssertColumnIsNullOrZero(resultPeriodLastYear, "CurrentPeriod");
				AssertColumnIsNullOrZero(resultPeriodLastYear, "YearToPeriod");
				AssertRowEquals(periodLastYearRow, "PeriodLastYear", 11m);
				AssertColumnIsNullOrZero(resultPeriodLastYear, "LastYearToPeriod");
				AssertColumnIsNullOrZero(resultPeriodLastYear, "TotalLastYear");
			});

			var resultLastYearToPeriod = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":LastYearToPeriod:", "PNL"));
			var lastYearToPeriodRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is PeriodLastYear, only PeriodLastYear figures should be calculated", () =>
			{
				AssertColumnIsNullOrZero(resultLastYearToPeriod, "CurrentPeriod");
				AssertColumnIsNullOrZero(resultLastYearToPeriod, "YearToPeriod");
				AssertColumnIsNullOrZero(resultLastYearToPeriod, "PeriodLastYear");
				AssertRowEquals(lastYearToPeriodRow, "LastYearToPeriod", 32m);
				AssertColumnIsNullOrZero(resultLastYearToPeriod, "TotalLastYear");
			});

			var resultTotalLastYear = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":TotalLastYear:", "PNL"));
			var totalLastYearRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is TotalLastYear, only TotalLastYear figures should be calculated", () =>
			{
				AssertColumnIsNullOrZero(resultTotalLastYear, "CurrentPeriod");
				AssertColumnIsNullOrZero(resultTotalLastYear, "YearToPeriod");
				AssertColumnIsNullOrZero(resultTotalLastYear, "PeriodLastYear");
				AssertColumnIsNullOrZero(resultTotalLastYear, "LastYearToPeriod");
				AssertRowEquals(totalLastYearRow, "TotalLastYear", 164m);
			});

			var resultMulti = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":TotalLastYear:PeriodLastYear:LastYearToPeriod:", "PNL"));
			var multiRow = resultPnl.GetRowByAccountCode("2222.00.00");
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis has multiple values, figures for specified columns only should be calculated", () =>
			{
				AssertColumnIsNullOrZero(resultMulti, "CurrentPeriod");
				AssertColumnIsNullOrZero(resultMulti, "YearToPeriod");
				AssertRowEquals(multiRow, "PeriodLastYear", 11m);
				AssertRowEquals(multiRow, "LastYearToPeriod", 32m);
				AssertRowEquals(multiRow, "TotalLastYear", 164m);
			});

			var resultUnknown = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlTemplate, ":ThisIsAnUnknownKey:", "PNL"));
			CombineAssertions("When OnlyCalculationsForPeriodAnalysis is unknown, no figures should be calculated", () =>
			{
				AssertColumnIsNullOrZero(resultUnknown, "CurrentPeriod");
				AssertColumnIsNullOrZero(resultUnknown, "YearToPeriod");
				AssertColumnIsNullOrZero(resultUnknown, "PeriodLastYear");
				AssertColumnIsNullOrZero(resultUnknown, "LastYearToPeriod");
				AssertColumnIsNullOrZero(resultUnknown, "TotalLastYear");
			});

			void AssertColumnIsNullOrZero(DataTable table, string baseColumnName)
			{
				foreach (DataRow row in table.Rows)
				{
					AssertEquals(baseColumnName, 0m, row.IsNull(baseColumnName) ? 0m : row[baseColumnName]);
					AssertEquals(baseColumnName + "Standard", 0m, row.IsNull(baseColumnName + "Standard") ? 0m : row[baseColumnName + "Standard"]);
				}
			}
			void AssertRowEquals(DataRow row, string baseColumnName, decimal amount)
			{
				AssertEquals(baseColumnName, amount, row.IsNull(baseColumnName) ? 0m : row[baseColumnName]);
				AssertEquals(baseColumnName + "Standard", amount, row.IsNull(baseColumnName + "Standard") ? 0m : row[baseColumnName + "Standard"]);
			}
		}

		public void TestAllSummedFieldsExceedMoneyRangeWithNoException()
		{
			CreateManagementPeriod();

			TestConnection.ExecuteNonQuery(@"
-- Current year not PresentationJournals
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 922337203685477.0000, 200912, 'D37A2C56-C09A-44F7-A503-986C8F87F623', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 12345.0000, 200912, 'D37A2C56-C09A-44F7-A503-986C8F87F623', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

-- Last year not PresentationJournals
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 922337203685477.0000, 200812, 'D37A2C56-C09A-44F7-A503-986C8F87F623', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 12345.0000, 200812, 'D37A2C56-C09A-44F7-A503-986C8F87F623', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

-- Current year PresentationJournals
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 922337203685477.0000, 200912, '35443F5A-6781-4273-B800-9DC5AF08ADEB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'TST')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 12345.0000, 200912, '35443F5A-6781-4273-B800-9DC5AF08ADEB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'TST')

-- Last year PresentationJournals
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 922337203685477.0000, 200812, '35443F5A-6781-4273-B800-9DC5AF08ADEB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'TST')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 12345.0000, 200812, '35443F5A-6781-4273-B800-9DC5AF08ADEB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'TST')


--CurrentCredit
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -922337203685477.0000, 200912, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -12345.0000, 200912, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

--Current year Budget
DECLARE @AU_PK UNIQUEIDENTIFIER
SET @AU_PK = NEWID()
INSERT INTO dbo.AccGLBudget (AU_PK, AU_Year, AU_Opening, AU_Closing, AU_AG, AU_GB, AU_GE, AU_AllocationType, AU_AllocationValue, AU_AllocationIncrement)
VALUES (@AU_PK, 2009, 0.0, 0.00, 'D37A2C56-C09A-44F7-A503-986C8F87F623', '27A55065-AC88-4EC3-8BED-E575E79172CB', '86BB1C22-0865-4685-996E-D56CBD136491', 'PRD', 0.0, 0.0)

INSERT INTO dbo.AccGLBudgetLines (AD_PK, AD_Period, AD_Percent, AD_Amount, AD_AU)
VALUES (NEWID(), 200912, 0.50, 922337203685477, @AU_PK)
INSERT INTO dbo.AccGLBudgetLines (AD_PK, AD_Period, AD_Percent, AD_Amount, AD_AU)
VALUES (NEWID(), 200912, 0.50, 12345.00, @AU_PK)

--Last year Budget
SET @AU_PK = NEWID()
INSERT INTO dbo.AccGLBudget (AU_PK, AU_Year, AU_Opening, AU_Closing, AU_AG, AU_GB, AU_GE, AU_AllocationType, AU_AllocationValue, AU_AllocationIncrement)
VALUES (@AU_PK, 2008, 0.0, 0.00, 'D37A2C56-C09A-44F7-A503-986C8F87F623', '27A55065-AC88-4EC3-8BED-E575E79172CB', '86BB1C22-0865-4685-996E-D56CBD136491', 'PRD', 0.0, 0.0)

INSERT INTO dbo.AccGLBudgetLines (AD_PK, AD_Period, AD_Percent, AD_Amount, AD_AU)
VALUES (NEWID(), 200812, 0.50, 922337203685477, @AU_PK)
INSERT INTO dbo.AccGLBudgetLines (AD_PK, AD_Period, AD_Percent, AD_Amount, AD_AU)
VALUES (NEWID(), 200812, 0.50, 12345.00, @AU_PK)
");

			#region Columns To Assert

			var periodPresentationJournalsColumns = new List<string>()
			{
				"CurrentPeriodPresentationJournals",
				"YearToPeriodPresentationJournals",
				"PeriodLastYearPresentationJournals",
				"LastYearToPeriodPresentationJournals",
				"TotalLastYearPresentationJournals"
			};
			var periodColumns = new List<string>()
			{
				"CurrentPeriodStandard",
				"CurrentPeriod",
				"YearToPeriodStandard",
				"YearToPeriod",
				"PeriodLastYearStandard",
				"PeriodLastYear",
				"LastYearToPeriodStandard",
				"LastYearToPeriod",
				"TotalLastYearStandard",
				"TotalLastYear",
				"BudgetCurrentPeriod",
				"BudgetYearToPeriod",
				"BudgetLastYearTotal",
				"TotalYearBudget"
			};

			var debitColumns = new List<string>()
			{
				"CurrentDebit",
				"CurrentDebitForCalculateTotal",
				"CurrentStandard",
				"CurrentStandardForCalculateTotal",
				"ClosingDebit",
				"ClosingDebitForCalculateTotal",
				"ClosingStandard",
				"ClosingStandardForCalculateTotal",
			};
			var creditColumns = new List<string>()
			{
				"CurrentCredit",
				"CurrentCreditForCalculateTotal",
				"ClosingCredit",
				"ClosingCreditForCalculateTotal"
			};
			var currentAndClosingPresentationJournalsColumns = new List<string>()
			{
				"CurrentPresentationJournals",
				"CurrentPresentationJournalsForCalculateTotal",
				"ClosingPresentationJournals",
				"ClosingPresentationJournalsForCalculateTotal",
			};

			var currentYearMovementColumns = new List<string>() { "CurrentYearMovement" };

			var creditAndStandardColumns = new List<string>()
			{
				"CurrentPeriodStandard",
				"CurrentPeriod",
				"CurrentCredit",
				"CurrentCreditForCalculateTotal",
				"CurrentStandard",
				"CurrentStandardForCalculateTotal",
				"CurrentYearMovement",
				"CurrentYearMovementCredit",
				"YearToPeriodStandard",
				"YearToPeriod",
				"ClosingCredit",
				"ClosingCreditForCalculateTotal",
				"ClosingStandard",
				"ClosingStandardForCalculateTotal",
			};
			var debitAndPresentationJournalsColumns = new List<string>()
			{
				"CurrentPeriodPresentationJournals",
				"CurrentDebit",
				"CurrentDebitForCalculateTotal",
				"CurrentPresentationJournals",
				"CurrentPresentationJournalsForCalculateTotal",
				"CurrentYearMovementDebit",
				"YearToPeriodPresentationJournals",
				"ClosingDebit",
				"ClosingDebitForCalculateTotal",
				"ClosingPresentationJournals",
				"ClosingPresentationJournalsForCalculateTotal",
			};
			var lastYearAndBudgetColumns = new List<string>()
			{
				"LastYearToPeriodStandard",
				"LastYearToPeriodPresentationJournals",
				"LastYearToPeriodCredit",
				"LastYearToPeriodDebit",
				"TotalLastYearStandard",
				"TotalLastYearPresentationJournals",
				"PeriodLastYearStandard",
				"PeriodLastYearPresentationJournals",
				"BudgetCurrentPeriod",
				"TotalYearBudget"
			};
			var lastYearDoubleColumns = new List<string>()
			{
				"LastYearToPeriod",
				"TotalLastYear",
				"PeriodLastYear",
			};

			#endregion

			var resultPL1 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport 200912, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', 'Y', 'PNL', '', '', '', '0', 'Department', 'N', 'TST', '', ''");

			var row_PresentationJounals = (from DataRow row in resultPL1.Rows where row["AccountPK"].ToString().ToUpper().Equals("35443F5A-6781-4273-B800-9DC5AF08ADEB") select row).FirstOrDefault();
			var row_NotPresentationJounals = (from DataRow row in resultPL1.Rows where row["AccountPK"].ToString().ToUpper().Equals("D37A2C56-C09A-44F7-A503-986C8F87F623") select row).FirstOrDefault();

			AssertAggregation(row_PresentationJounals, periodPresentationJournalsColumns);
			AssertAggregation(row_NotPresentationJounals, periodColumns);

			var resultPL2 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport 200912, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', 'Y', 'TBS', '', '', '', '0', '', 'N', 'TST', '', ''");

			var row_Credit = (from DataRow row in resultPL2.Rows where row["AccountPK"].ToString().ToUpper().Equals("318F6D36-6158-4B3A-8043-B81C996F2F36") select row).FirstOrDefault();
			var row_Debit = (from DataRow row in resultPL2.Rows where row["AccountPK"].ToString().ToUpper().Equals("D37A2C56-C09A-44F7-A503-986C8F87F623") select row).FirstOrDefault();
			var row_CurrentAndClosingPresentationJournals = (from DataRow row in resultPL2.Rows where row["AccountPK"].ToString().ToUpper().Equals("35443F5A-6781-4273-B800-9DC5AF08ADEB") select row).FirstOrDefault();

			AssertAggregation(row_Debit, debitColumns);
			AssertAggregation(row_Credit, creditColumns);
			AssertAggregation(row_CurrentAndClosingPresentationJournals, currentAndClosingPresentationJournalsColumns);

			var resultPL3 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport 200912, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', 'Y', 'PNL', 'ZH-CN', '', '', '0', 'Department', 'N', 'TST', '', ''");
			var row_CurrentYearMovement = (from DataRow row in resultPL3.Rows where row["AccountNumber"].ToString().Equals("3820") select row).FirstOrDefault();

			AssertAggregation(row_CurrentYearMovement, currentYearMovementColumns);

			var resultPLConsol1 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport 200912, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', 'Y', 'PNL', 'ZH-CN', '', '', '0', 'Department', 'N', 'TST', '', ''");
			var row_CreditAndStandard = (from DataRow row in resultPL3.Rows where row["AccountNumber"].ToString().Equals("1010") select row).FirstOrDefault();
			var row_DebitAndPresentationJournals = (from DataRow row in resultPL3.Rows where row["AccountNumber"].ToString().Equals("3820") select row).FirstOrDefault();

			AssertAggregation(row_CreditAndStandard, creditAndStandardColumns);
			AssertAggregation(row_DebitAndPresentationJournals, debitAndPresentationJournalsColumns);

			var resultPLConsol2 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport 200912, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', 'Y', 'PNL', '', '', '', '0', 'Branch', 'N', 'TST', '', 'Y'");
			var row_LastYearAndBudget = resultPLConsol2.Rows[0];

			AssertAggregation(row_LastYearAndBudget, lastYearAndBudgetColumns);
			AssertAggregation(row_LastYearAndBudget, lastYearDoubleColumns, true);

			void AssertAggregation(DataRow row, IEnumerable<string> columnNames, bool isDoubleValue = false)
			{
				var expectedValue = 922337203697822.0000M;
				foreach (var columnName in columnNames)
				{
					AssertEquals($"The value of {columnName} can exceed the range of money type.", isDoubleValue ? expectedValue * 2 : expectedValue, Math.Abs((decimal)row[columnName]));
				}
			}
		}

		public void TestRetainedEarningsBranchFilterAndBranchManagementCodeFilter()
		{
			CreateManagementPeriod();

			// Insert GL Aggregation record for BNE branch and 4900.00.00 account which is a P&L Appropriation Account in the test system
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 100.0000, 200812, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 200.0000, 200912, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 300.0000, 201001, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')
");

			// RunReport without Branch filter
			RunReportAndAssertRetainedEarningLine("", "");
			// Run report for SYD branch
			RunReportAndAssertRetainedEarningLine("SYD", "");
			// Run report for BNE branch
			RunReportAndAssertRetainedEarningLine("BNE", "");

			TestConnection.ExecuteNonQuery("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRS', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GETUTCDATE() WHERE GB_Code = 'SYD' And GB_GC = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'");
			TestConnection.ExecuteNonQuery("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRB', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GETUTCDATE() WHERE GB_Code = 'BNE' And GB_GC = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'");

			// Run report for BRS branch management code
			RunReportAndAssertRetainedEarningLine("", "BRS");
			// Run report for BRB branch management code
			RunReportAndAssertRetainedEarningLine("", "BRB");
		}

		public void TestOptionalColumnsByGroupWithSplitDebitCredit()
		{
			CreateManagementPeriod();

			// Insert GL Aggregation record for BNE branch and 4900.00.00 account which is a P&L Appropriation Account in the test system
			// Insert GL Aggregation record for BNE branch and 1020.20.20 account which is a P&L DR Account in the test system
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 100.0000, 200812, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 110.0000, 200812, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 200.0000, 200901, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 250.0000, 200901, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 300.0000, 201001, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 400.0000, 201001, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 500.0000, 201001, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 1000.0000, 200812, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 1100.0000, 200812, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 2000.0000, 200901, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 2500.0000, 200901, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 3000.0000, 201001, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 4000.0000, 201001, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 5000.0000, 201001, '3ac06f84-e70f-4bf8-aee0-56703d5329e3', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')
");

			AssertOptionalColumnsByGroup("Branch");
			AssertOptionalColumnsByGroup("Department");
			AssertOptionalColumnsByGroup("Mgt");
		}

		void AssertOptionalColumnsByGroup(string groupBy)
		{
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', 'Y', 'PNL', '', '', '', '1', '{groupBy}', 'N', 'AAA,BBB', ''");
			AssertEquals("Result should have 1 rows", 1, result.Rows.Count);
			var row = result.Rows[0];

			AssertEquals("CurrentPeriodDebitStandard field", -3000M, decimal.Parse(row["CurrentPeriodDebitStandard"].ToString()));
			AssertEquals("CurrentPeriodDebitPresentationJournals field", -9000M, decimal.Parse(row["CurrentPeriodDebitPresentationJournals"].ToString()));
			AssertEquals("CurrentPeriodDebit field", -12000M, decimal.Parse(row["CurrentPeriodDebit"].ToString()));

			AssertEquals("YearToPeriodDebitStandard field", -3000M, decimal.Parse(row["YearToPeriodDebitStandard"].ToString()));
			AssertEquals("YearToPeriodDebitPresentationJournals field", -9000M, decimal.Parse(row["YearToPeriodDebitPresentationJournals"].ToString()));
			AssertEquals("YearToPeriodDebit field", -12000M, decimal.Parse(row["YearToPeriodDebit"].ToString()));

			AssertEquals("PeriodLastYearDebitStandard field", -2000M, decimal.Parse(row["PeriodLastYearDebitStandard"].ToString()));
			AssertEquals("PeriodLastYearDebitPresentationJournals field", -2500M, decimal.Parse(row["PeriodLastYearDebitPresentationJournals"].ToString()));
			AssertEquals("PeriodLastYearDebit field", -4500M, decimal.Parse(row["PeriodLastYearDebit"].ToString()));

			AssertEquals("LastYearToPeriodDebitStandard field", -2000M, decimal.Parse(row["LastYearToPeriodDebitStandard"].ToString()));
			AssertEquals("LastYearToPeriodDebitPresentationJournals field", -2500M, decimal.Parse(row["LastYearToPeriodDebitPresentationJournals"].ToString()));
			AssertEquals("LastYearToPeriodDebit field", -4500M, decimal.Parse(row["LastYearToPeriodDebit"].ToString()));

			AssertEquals("TotalLastYearDebitStandard field", -2000M, decimal.Parse(row["TotalLastYearDebitStandard"].ToString()));
			AssertEquals("TotalLastYearDebitPresentationJournals field", -2500M, decimal.Parse(row["TotalLastYearDebitPresentationJournals"].ToString()));
			AssertEquals("TotalLastYearDebit field", -4500M, decimal.Parse(row["TotalLastYearDebit"].ToString()));

			AssertEquals("CurrentPeriodCreditStandard field", -300M, decimal.Parse(row["CurrentPeriodCreditStandard"].ToString()));
			AssertEquals("CurrentPeriodCreditPresentationJournals field", -900M, decimal.Parse(row["CurrentPeriodCreditPresentationJournals"].ToString()));
			AssertEquals("CurrentPeriodCredit field", -1200M, decimal.Parse(row["CurrentPeriodCredit"].ToString()));

			AssertEquals("YearToPeriodCreditStandard field", -3600M, decimal.Parse(row["YearToPeriodCreditStandard"].ToString()));
			AssertEquals("YearToPeriodCreditPresentationJournals field", -4860M, decimal.Parse(row["YearToPeriodCreditPresentationJournals"].ToString()));
			AssertEquals("YearToPeriodCredit field", -8460M, decimal.Parse(row["YearToPeriodCredit"].ToString()));

			AssertEquals("PeriodLastYearCreditStandard field", -200M, decimal.Parse(row["PeriodLastYearCreditStandard"].ToString()));
			AssertEquals("PeriodLastYearCreditPresentationJournals field", -250M, decimal.Parse(row["PeriodLastYearCreditPresentationJournals"].ToString()));
			AssertEquals("PeriodLastYearCredit field", -450M, decimal.Parse(row["PeriodLastYearCredit"].ToString()));

			AssertEquals("LastYearToPeriodCreditStandard field", -1300M, decimal.Parse(row["LastYearToPeriodCreditStandard"].ToString()));
			AssertEquals("LastYearToPeriodCreditPresentationJournals field", -1460M, decimal.Parse(row["LastYearToPeriodCreditPresentationJournals"].ToString()));
			AssertEquals("LastYearToPeriodCredit field", -2760M, decimal.Parse(row["LastYearToPeriodCredit"].ToString()));

			AssertEquals("TotalLastYearCreditStandard field", -1300M, decimal.Parse(row["TotalLastYearCreditStandard"].ToString()));
			AssertEquals("TotalLastYearCreditPresentationJournals field", -1460M, decimal.Parse(row["TotalLastYearCreditPresentationJournals"].ToString()));
			AssertEquals("TotalLastYearCredit field", -2760M, decimal.Parse(row["TotalLastYearCredit"].ToString()));
		}

		public void TestReplaceZeroWithNullForPnL()
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO [AccPeriodManagement] ([AM_PK],[AM_Period],[AM_Year],[AM_StartDate],[AM_EndDate],[AM_IsSubLedgerClosed],[AM_IsGeneralLedgerClosed],[AM_GC_Company],[AM_IsSubledgerClosedForAdjustments],[AM_ArchiveCommenced])VALUES('0DA3A30E-F815-4381-9FD5-7766EC9BB4B7',200909,2009,'Sep  1 2009 12:00:00:000AM','Sep 30 2009 11:59:00:000PM',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',0,0)
				INSERT INTO [AccPeriodManagement] ([AM_PK],[AM_Period],[AM_Year],[AM_StartDate],[AM_EndDate],[AM_IsSubLedgerClosed],[AM_IsGeneralLedgerClosed],[AM_GC_Company],[AM_IsSubledgerClosedForAdjustments],[AM_ArchiveCommenced])VALUES('3162E948-7A5B-4463-9316-A80EDBF74682',201009,2010,'Sep  1 2010 12:00:00:000AM','Sep 30 2010 11:59:00:000PM',0,0,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',0,0)");

			TestConnection.ExecuteNonQuery(@"INSERT INTO [AccGLAggregate] ([AA_PK],[AA_Amount],[AA_Period],[AA_AG],[AA_GB],[AA_GC],[AA_GE],[AA_TransactionCategory])VALUES('14F778B3-6C56-4A92-9027-AC3D9822BC80',0.0000,200909,'318F6D36-6158-4B3A-8043-B81C996F2F36','FDD429D2-648C-4895-8F9F-06E90DED2BE5','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','F5C72696-19AD-4759-879F-89C8532FF238','')");

			TestConnection.ExecuteNonQuery(@"DECLARE @AU_PK UNIQUEIDENTIFIER
				SET @AU_PK = NEWID()
				INSERT INTO dbo.AccGLBudget (AU_PK, AU_Year, AU_Opening, AU_Closing, AU_AG, AU_GB, AU_GE, AU_AllocationType, AU_AllocationValue, AU_AllocationIncrement)
				VALUES (@AU_PK, 2010, 0.0, 0.00, '318F6D36-6158-4B3A-8043-B81C996F2F36', 'FDD429D2-648C-4895-8F9F-06E90DED2BE5', 'F5C72696-19AD-4759-879F-89C8532FF238', 'PRD', 0.0, 0.0)

				INSERT INTO dbo.AccGLBudgetLines (AD_PK, AD_Period, AD_Percent, AD_Amount, AD_AU)
				VALUES (NEWID(), 201009, 0.50, 0.00, @AU_PK)");

			string sql = @"DECLARE
							@Period INT, 
							@CompanyPK UNIQUEIDENTIFIER, 
							@DepartmentList AS VARCHAR(8000), 
							@BranchList AS VARCHAR(8000), 
							@InclZeroBal CHAR(1), 
							@SummaryType CHAR(20), 
							@IncludeBudget CHAR(1), 
							@ReportType CHAR(3), 
							@Language CHAR(3), 
							@MultiLanguageBSHStartAccount VARCHAR(10), 
							@BudgetOnly CHAR(1), 
							@RollUp CHAR(1),
							@TSGroupBy VARCHAR(10),
							@Nature CHAR(1),
							@TransactionCategory VARCHAR(100),
							@BranchManagementCode VARCHAR(3),
							@SplitDebitCredit char(1) = 'Y'
						SET @Period = 201009
						SET @CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
						SET @DepartmentList = N''
						SET @BranchList = N''
						SET @InclZeroBal = N'Y'
						SET @SummaryType = N'ALLACCT'
						SET @IncludeBudget = 'Y'
						SET @ReportType = 'PNL'
						SET @Language = ''
						SET @MultiLanguageBSHStartAccount = ''
						SET @BudgetOnly = ''
						SET @RollUp = '0'
						SET @TSGroupBy = ''
						SET @Nature = ''
						SET @TransactionCategory = NULL
						SET @BranchManagementCode = ''
						EXEC ProfitAndLossReport @Period, @CompanyPK, @DepartmentList, @BranchList, @InclZeroBal, @SummaryType, @IncludeBudget, @ReportType, @Language,
							@MultiLanguageBSHStartAccount, @BudgetOnly, @RollUp, @TSGroupBy, @Nature, @TransactionCategory, @BranchManagementCode";

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			Assert("Result should have rows", result.Rows.Count > 0);

			DataRow row10102000 = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010.20.00") select row).FirstOrDefault();
			DataRow row18000000 = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1800.00.00") select row).FirstOrDefault();

			AssertEquals("BudgetCurrentPeriod should be NULL", System.DBNull.Value, row10102000["BudgetCurrentPeriod"]);
			AssertEquals("BudgetCurrentPeriod should be NULL", System.DBNull.Value, row18000000["BudgetCurrentPeriod"]);
		}

		public void TestNatureProperty()
		{
			CreateManagementPeriod();
			CreateGLAggregate();

			var sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar = ''
				DECLARE @p4 AS NVarChar = ''
				DECLARE @p5 AS NVarChar = 'N'
				DECLARE @p6 AS NVarChar = 'ALLACCT'
				DECLARE @p7 AS NVarChar = '0'		-- RollUp
				DECLARE @p8 AS NVarChar = 'N'       -- Nature
				DECLARE @p9 AS NVarChar = 'Y'		
				DECLARE @p10 AS NVarChar = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			DataRow row1800NaturePropertyUnchecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1800") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);
			AssertEquals("CurrentPeriod field", 300M, decimal.Parse(row1800NaturePropertyUnchecked["CurrentPeriod"].ToString()));
			AssertEquals("YearToPeriod field", 300M, decimal.Parse(row1800NaturePropertyUnchecked["YearToPeriod"].ToString()));
			AssertEquals("LastYearToPeriod field", 200M, decimal.Parse(row1800NaturePropertyUnchecked["TotalLastYear"].ToString()));

			sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar = ''
				DECLARE @p4 AS NVarChar = ''
				DECLARE @p5 AS NVarChar = 'N'
				DECLARE @p6 AS NVarChar = 'ALLACCT'
				DECLARE @p7 AS NVarChar = '0'		-- RollUp
				DECLARE @p8 AS NVarChar = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar = 'Y'		
				DECLARE @p10 AS NVarChar = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			DataRow row1800NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1800") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);
			AssertEquals("CurrentPeriod field", -300M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriod"].ToString()));
			AssertEquals("YearToPeriod field", -300M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriod"].ToString()));
			AssertEquals("LastYearToPeriod field", -200M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYear"].ToString()));
		}

		public void TestOptionalColumnsWithPresentationCategoryGroup()
		{
			CreateManagementPeriod();
			CreateGLAggregateForPresentationCategoryGroup();

			var sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'	-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'      -- Nature
				DECLARE @p9 AS NVarChar(100) = 'AAA'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			DataRow row1800NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1800") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);

			AssertEquals("CurrentPeriod field", -300M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriodStandard"].ToString()));
			AssertEquals("CurrentPeriod field", -440M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriodPresentationJournals"].ToString()));
			AssertEquals("CurrentPeriod field", -740M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriod"].ToString()));

			AssertEquals("YearToPeriod field", -300M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriodStandard"].ToString()));
			AssertEquals("YearToPeriod field", -440M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriodPresentationJournals"].ToString()));
			AssertEquals("YearToPeriod field", -740M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriod"].ToString()));

			AssertEquals("LastYearToPeriod field", -1400M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYearStandard"].ToString()));
			AssertEquals("LastYearToPeriod field", -1440M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYearPresentationJournals"].ToString()));
			AssertEquals("LastYearToPeriod field", -2840M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYear"].ToString()));

			AssertEquals("PeriodLastYear field", -1200M, decimal.Parse(row1800NaturePropertyChecked["PeriodLastYearStandard"].ToString()));
			AssertEquals("PeriodLastYear field", -1220M, decimal.Parse(row1800NaturePropertyChecked["PeriodLastYearPresentationJournals"].ToString()));
			AssertEquals("PeriodLastYear field", -2420M, decimal.Parse(row1800NaturePropertyChecked["PeriodLastYear"].ToString()));

			var row4900NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("4900") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);
			AssertEquals("LastYearToPeriod field", 100M, decimal.Parse(row4900NaturePropertyChecked["LastYearToPeriodStandard"].ToString()));
			AssertEquals("LastYearToPeriod field", 111M, decimal.Parse(row4900NaturePropertyChecked["LastYearToPeriodPresentationJournals"].ToString()));
			AssertEquals("LastYearToPeriod field", 211M, decimal.Parse(row4900NaturePropertyChecked["LastYearToPeriod"].ToString()));

			sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = 'AAA,BBB,CCC'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			row1800NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1800") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);

			AssertEquals("CurrentPeriod field", -300M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriodStandard"].ToString()));
			AssertEquals("CurrentPeriod field", -1650M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriodPresentationJournals"].ToString()));
			AssertEquals("CurrentPeriod field", -1950M, decimal.Parse(row1800NaturePropertyChecked["CurrentPeriod"].ToString()));

			AssertEquals("YearToPeriod field", -300M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriodStandard"].ToString()));
			AssertEquals("YearToPeriod field", -1650M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriodPresentationJournals"].ToString()));
			AssertEquals("YearToPeriod field", -1950M, decimal.Parse(row1800NaturePropertyChecked["YearToPeriod"].ToString()));

			AssertEquals("LastYearToPeriod field", -1400M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYearStandard"].ToString()));
			AssertEquals("LastYearToPeriod field", -1470M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYearPresentationJournals"].ToString()));
			AssertEquals("LastYearToPeriod field", -2870M, decimal.Parse(row1800NaturePropertyChecked["TotalLastYear"].ToString()));

			AssertEquals("PeriodLastYear field", -1200M, decimal.Parse(row1800NaturePropertyChecked["PeriodLastYearStandard"].ToString()));
			AssertEquals("PeriodLastYear field", -1250M, decimal.Parse(row1800NaturePropertyChecked["PeriodLastYearPresentationJournals"].ToString()));
			AssertEquals("PeriodLastYear field", -2450M, decimal.Parse(row1800NaturePropertyChecked["PeriodLastYear"].ToString()));

			row4900NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("4900") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);
			AssertEquals("LastYearToPeriod field", 100M, decimal.Parse(row4900NaturePropertyChecked["LastYearToPeriodStandard"].ToString()));
			AssertEquals("LastYearToPeriod field", 333M, decimal.Parse(row4900NaturePropertyChecked["LastYearToPeriodPresentationJournals"].ToString()));
			AssertEquals("LastYearToPeriod field", 433M, decimal.Parse(row4900NaturePropertyChecked["LastYearToPeriod"].ToString()));
		}

		public void TestOptionalColumnsForCreditWithPresentationCategoryGroup()
		{
			CreateManagementPeriod();
			CreateGLAggregateForPresentationCategoryGroup();

			var sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = 'AAA'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'TBS', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			DataRow row1010NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);

			AssertEquals("CurrentStandard field", -300M, decimal.Parse(row1010NaturePropertyChecked["CurrentStandard"].ToString()));
			AssertEquals("CurrentPresentationJournals field", -440M, decimal.Parse(row1010NaturePropertyChecked["CurrentPresentationJournals"].ToString()));
			AssertEquals("CurrentCredit field", 740M, decimal.Parse(row1010NaturePropertyChecked["CurrentCredit"].ToString()));

			AssertEquals("ClosingStandard field", -300M, decimal.Parse(row1010NaturePropertyChecked["ClosingStandard"].ToString()));
			AssertEquals("ClosingPresentationJournals field", -440M, decimal.Parse(row1010NaturePropertyChecked["ClosingPresentationJournals"].ToString()));
			AssertEquals("ClosingCredit field", 740M, decimal.Parse(row1010NaturePropertyChecked["ClosingCredit"].ToString()));

			sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = 'AAA,BBB,CCC'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'TBS', '', '', '', @p7, '', @p8, @p9, @p10";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			row1010NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);

			AssertEquals("CurrentStandard field", -300M, decimal.Parse(row1010NaturePropertyChecked["CurrentStandard"].ToString()));
			AssertEquals("CurrentPresentationJournals field", -1650M, decimal.Parse(row1010NaturePropertyChecked["CurrentPresentationJournals"].ToString()));
			AssertEquals("CurrentCredit field", 1950M, decimal.Parse(row1010NaturePropertyChecked["CurrentCredit"].ToString()));

			AssertEquals("ClosingStandard field", -300M, decimal.Parse(row1010NaturePropertyChecked["ClosingStandard"].ToString()));
			AssertEquals("ClosingPresentationJournals field", -1650M, decimal.Parse(row1010NaturePropertyChecked["ClosingPresentationJournals"].ToString()));
			AssertEquals("ClosingCredit field", 1950M, decimal.Parse(row1010NaturePropertyChecked["ClosingCredit"].ToString()));
		}

		public void TestOptionalColumnsForDebitWithPresentationCategoryGroup()
		{
			CreateManagementPeriod();
			//318F6D36 - 6158 - 4B3A - 8043 - B81C996F2F36  1010.20.00
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 300.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 440.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 550.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 660.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'CCC')
");

			var sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = 'AAA'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'TBS', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			DataRow row1010NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);

			AssertEquals("CurrentStandard field", 300M, decimal.Parse(row1010NaturePropertyChecked["CurrentStandard"].ToString()));
			AssertEquals("CurrentPresentationJournals field", 440M, decimal.Parse(row1010NaturePropertyChecked["CurrentPresentationJournals"].ToString()));
			AssertEquals("CurrentDebit field", 740M, decimal.Parse(row1010NaturePropertyChecked["CurrentDebit"].ToString()));

			AssertEquals("ClosingStandard field", 300M, decimal.Parse(row1010NaturePropertyChecked["ClosingStandard"].ToString()));
			AssertEquals("ClosingPresentationJournals field", 440M, decimal.Parse(row1010NaturePropertyChecked["ClosingPresentationJournals"].ToString()));
			AssertEquals("ClosingDebit field", 740M, decimal.Parse(row1010NaturePropertyChecked["ClosingDebit"].ToString()));

			sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = 'AAA,BBB,CCC'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'TBS', '', '', '', @p7, '', @p8, @p9, @p10";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			row1010NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);

			AssertEquals("CurrentStandard field", 300M, decimal.Parse(row1010NaturePropertyChecked["CurrentStandard"].ToString()));
			AssertEquals("CurrentPresentationJournals field", 1650M, decimal.Parse(row1010NaturePropertyChecked["CurrentPresentationJournals"].ToString()));
			AssertEquals("CurrentDebit field", 1950M, decimal.Parse(row1010NaturePropertyChecked["CurrentDebit"].ToString()));

			AssertEquals("ClosingStandard field", 300M, decimal.Parse(row1010NaturePropertyChecked["ClosingStandard"].ToString()));
			AssertEquals("ClosingPresentationJournals field", 1650M, decimal.Parse(row1010NaturePropertyChecked["ClosingPresentationJournals"].ToString()));
			AssertEquals("ClosingDebit field", 1950M, decimal.Parse(row1010NaturePropertyChecked["ClosingDebit"].ToString()));
		}

		public void TestContainUnits()
		{
			CreateManagementPeriod();
			var accountPK = TestObjectCreator.InsertGLHeader("2201.02.01", "Test Account 1", "P&L", "");
			var accountPK1 = TestObjectCreator.InsertGLHeader("2202.02.01", "Test Account KWH", "P&L", "KWH");
			var accountPK2 = TestObjectCreator.InsertGLHeader("2203.02.01", "Test Account KWH", "P&L", "UNT");

			TestObjectCreator.CreateGLAggregate("", 5, 100, 201001, accountPK, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 6, 100, 201001, accountPK1, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 7, 100, 201001, accountPK2, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);

			Factory.Save();

			var sql = $@"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '{TestObjectCreator.DefaultCompanyPK}'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = ''		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'TBS', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have 3 rows", 3, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2201") select row).FirstOrDefault();
			AssertEquals("Units field", "", rowSelected["Units"].ToString());
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2202") select row).FirstOrDefault();
			AssertEquals("Units field", "KWH", rowSelected["Units"].ToString());
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2203") select row).FirstOrDefault();
			AssertEquals("Units field", "UNT", rowSelected["Units"].ToString());
		}

		public void TestContainNewColumnsWhichUseForCalculateTotalAmount()
		{
			CreateManagementPeriod();
			var accountPK = TestObjectCreator.InsertGLHeader("2201.02.01", "Test Account 1", "NTE", "");
			var accountPK1 = TestObjectCreator.InsertGLHeader("2202.02.01", "Test Account KWH", "P&L", "KWH");

			TestObjectCreator.CreateGLAggregate("", 5, 100, 201001, accountPK, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 6, 100, 201001, accountPK1, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);

			Factory.Save();

			var sql = $@"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '{TestObjectCreator.DefaultCompanyPK}'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = ''		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'TBS', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2201") select row).FirstOrDefault();
			AssertEquals("CurrentDebitForCalculateTotal field", 0M, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", 0M, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("CurrentStandardForCalculateTotal field", 0M, rowSelected["CurrentStandardForCalculateTotal"]);
			AssertEquals("CurrentPresentationJournalsForCalculateTotal field", 0M, rowSelected["CurrentPresentationJournalsForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", 0M, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", 0M, rowSelected["ClosingCreditForCalculateTotal"]);
			AssertEquals("ClosingStandardForCalculateTotal field", 0M, rowSelected["ClosingStandardForCalculateTotal"]);
			AssertEquals("ClosingPresentationJournalsForCalculateTotal field", 0M, rowSelected["ClosingPresentationJournalsForCalculateTotal"]);

			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2202") select row).FirstOrDefault();
			AssertEquals("CurrentDebitForCalculateTotal field", 600M, rowSelected["CurrentDebitForCalculateTotal"]);
			AssertEquals("CurrentCreditForCalculateTotal field", DBNull.Value, rowSelected["CurrentCreditForCalculateTotal"]);
			AssertEquals("CurrentStandardForCalculateTotal field", 600M, rowSelected["CurrentStandardForCalculateTotal"]);
			AssertEquals("CurrentPresentationJournalsForCalculateTotal field", DBNull.Value, rowSelected["CurrentPresentationJournalsForCalculateTotal"]);
			AssertEquals("ClosingDebitForCalculateTotal field", 600M, rowSelected["ClosingDebitForCalculateTotal"]);
			AssertEquals("ClosingCreditForCalculateTotal field", DBNull.Value, rowSelected["ClosingCreditForCalculateTotal"]);
			AssertEquals("ClosingStandardForCalculateTotal field", 600M, rowSelected["ClosingStandardForCalculateTotal"]);
			AssertEquals("ClosingPresentationJournalsForCalculateTotal field", DBNull.Value, rowSelected["ClosingPresentationJournalsForCalculateTotal"]);
		}

		public void TestHandleYearToPeriodForNoteAccountWithReporttypeBSHAndTBS()
		{
			CreateManagementPeriod();
			var accountPK = TestObjectCreator.InsertGLHeader("6201.02.01", "Test Account 1", "NTE", "", reportSection: "AS");
			var accountPK1 = TestObjectCreator.InsertGLHeader("6202.02.01", "Test Account 2", "NTE", "", reportSection: "AS");

			TestObjectCreator.CreateGLAggregate("", 2, 100, 201001, accountPK, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);
			TestObjectCreator.CreateGLAggregate("", 4, 100, 201001, accountPK1, TestObjectCreator.DefaultDepartmentPK, TestObjectCreator.DefaultBranchPK, TestObjectCreator.DefaultCompanyPK);

			Factory.Save();

			var sql = $@"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '{TestObjectCreator.DefaultCompanyPK}'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = ''		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', '{"{0}"}', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sql, "TBS"));
			AssertEquals("Result should have 2 rows", 2, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("6201") select row).FirstOrDefault();
			AssertEquals("YearToPeriod field", -200M, rowSelected["YearToPeriod"]);
			AssertEquals("YearToPeriodStandard field", -200M, rowSelected["YearToPeriodStandard"]);
			AssertEquals("YearToPeriodPresentationJournals field", DBNull.Value, rowSelected["YearToPeriodPresentationJournals"]);
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("6202") select row).FirstOrDefault();
			AssertEquals("YearToPeriod field", -400M, rowSelected["YearToPeriod"]);
			AssertEquals("YearToPeriodStandard field", -400M, rowSelected["YearToPeriodStandard"]);
			AssertEquals("YearToPeriodPresentationJournals field", DBNull.Value, rowSelected["YearToPeriodPresentationJournals"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sql, "BSH"));

			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("6201") select row).FirstOrDefault();
			AssertEquals("YearToPeriod field", -200M, rowSelected["YearToPeriod"]);
			AssertEquals("YearToPeriodStandard field", -200M, rowSelected["YearToPeriodStandard"]);
			AssertEquals("YearToPeriodPresentationJournals field", DBNull.Value, rowSelected["YearToPeriodPresentationJournals"]);
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("6202") select row).FirstOrDefault();
			AssertEquals("YearToPeriod field", -400M, rowSelected["YearToPeriod"]);
			AssertEquals("YearToPeriodStandard field", -400M, rowSelected["YearToPeriodStandard"]);
			AssertEquals("YearToPeriodPresentationJournals field", DBNull.Value, rowSelected["YearToPeriodPresentationJournals"]);
		}

		public void TestRollUp()
		{
			CreateManagementPeriod();
			CreateGLAggregate();

			var sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar = ''
				DECLARE @p4 AS NVarChar = ''
				DECLARE @p5 AS NVarChar = 'N'
				DECLARE @p6 AS NVarChar  = 'ALLACCT'
				DECLARE @p7 AS NVarChar = '1'		-- RollUp
				DECLARE @p8 AS NVarChar = 'N'       -- Nature
				DECLARE @p9 AS NVarChar = 'Y'		
				DECLARE @p10 AS NVarChar = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			DataRow row1010NaturePropertyUnchecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);
			AssertEquals("YearToPeriod field", 300M, decimal.Parse(row1010NaturePropertyUnchecked["YearToPeriod"].ToString()));
			AssertEquals("CurrentPeriod field", 300M, decimal.Parse(row1010NaturePropertyUnchecked["CurrentPeriod"].ToString()));
			AssertEquals("LastYearToPeriod field", -700M, decimal.Parse(row1010NaturePropertyUnchecked["TotalLastYear"].ToString()));

			sql = @"
				DECLARE @p1 AS Int = 201001
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar = ''
				DECLARE @p4 AS NVarChar = ''
				DECLARE @p5 AS NVarChar = 'N'
				DECLARE @p6 AS NVarChar  = 'ALLACCT'
				DECLARE @p7 AS NVarChar = '1'		-- RollUp
				DECLARE @p8 AS NVarChar = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar = 'Y'		
				DECLARE @p10 AS NVarChar = null
				EXEC ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			// 1010.xx.xx is a CR account, therefore nature doesn't apply
			// However 1010.20.10 is a DR account type, its value is flipped when nature is checked
			// The sum of 1010.20.10 and 1010.10.xx should not be affected by this nature effect      
			DataRow row1010NaturePropertyChecked = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("1010") select row).FirstOrDefault();
			Assert("Result should have rows when line is expected", result.Rows.Count > 0);
			AssertEquals("YearToPeriod field", 300M, decimal.Parse(row1010NaturePropertyChecked["YearToPeriod"].ToString()));
			AssertEquals("CurrentPeriod field", 300M, decimal.Parse(row1010NaturePropertyChecked["CurrentPeriod"].ToString()));
			AssertEquals("LastYearToPeriod field", -700M, decimal.Parse(row1010NaturePropertyChecked["TotalLastYear"].ToString()));
		}

		void CreateManagementPeriod()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_ArchiveCommenced, AM_EndDate, AM_GC_Company, AM_IsGeneralLedgerClosed, AM_IsSubLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_Period, AM_StartDate, AM_Year, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser)
VALUES ('8d161f3a-ca7a-4858-a757-c7b8e8399200', 0, '2008-12-31 23:59:00.000', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 0, 0, 0, 200812, '2008-12-01 00:00:00.000', 2008, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.AccPeriodManagement(AM_PK, AM_ArchiveCommenced, AM_EndDate, AM_GC_Company, AM_IsGeneralLedgerClosed, AM_IsSubLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_Period, AM_StartDate, AM_Year, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser)
VALUES('9EF46E10-7655-4148-B2F0-B16CD1E2F3D9', 0, '2009-01-31 23:59:00.000', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 0, 0, 0, 200901, '2009-01-01 00:00:00.000', 2009, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_ArchiveCommenced, AM_EndDate, AM_GC_Company, AM_IsGeneralLedgerClosed, AM_IsSubLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_Period, AM_StartDate, AM_Year, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser)
VALUES ('2b7a96ee-b5c5-48fb-85df-5b7ac35ac868', 0, '2009-12-31 23:59:00.000', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 0, 0, 0, 200912, '2009-12-01 00:00:00.000', 2009, GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_ArchiveCommenced, AM_EndDate, AM_GC_Company, AM_IsGeneralLedgerClosed, AM_IsSubLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_Period, AM_StartDate, AM_Year, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser)
VALUES ('5db63c0a-53aa-456e-b975-81041cfe71ed', 0, '2010-01-31 23:59:00.000', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 0, 0, 0, 201001, '2010-01-01 00:00:00.000', 2010, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		void CreateGLAggregate()
		{
			//318F6D36 - 6158 - 4B3A - 8043 - B81C996F2F36  1010.20.00
			//B86D02D0 - DD42 - 4CA3 - BA35 - D9030042E567  1010.20.10
			//AD9AFCC2 - 52F8 - 493E - 9D5D - 3AD6300AE291  1010.10.00

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 900.0000, 200912, 'AD9AFCC2-52F8-493E-9D5D-3AD6300AE291', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -100.0000, 200812, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -200.0000, 200912, 'B86D02D0-DD42-4CA3-BA35-D9030042E567', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -300.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')
");
		}

		void CreateGLAggregateForPresentationCategoryGroup()
		{
			//318F6D36 - 6158 - 4B3A - 8043 - B81C996F2F36  1010.20.00
			//B86D02D0 - DD42 - 4CA3 - BA35 - D9030042E567  1010.20.10
			//AD9AFCC2 - 52F8 - 493E - 9D5D - 3AD6300AE291  1010.10.00

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 900.0000, 200912, 'AD9AFCC2-52F8-493E-9D5D-3AD6300AE291', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -100.0000, 200812, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -111.0000, 200812, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -222.0000, 200812, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -1200.0000, 200901, 'B86D02D0-DD42-4CA3-BA35-D9030042E567', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -1220.0000, 200901, 'B86D02D0-DD42-4CA3-BA35-D9030042E567', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -30.0000, 200901, 'B86D02D0-DD42-4CA3-BA35-D9030042E567', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -200.0000, 200912, 'B86D02D0-DD42-4CA3-BA35-D9030042E567', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -220.0000, 200912, 'B86D02D0-DD42-4CA3-BA35-D9030042E567', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -300.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -440.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'AAA')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -550.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'BBB')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -660.0000, 201001, '318F6D36-6158-4B3A-8043-B81C996F2F36', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', 'CCC')
");
		}

		void RunReportAndAssertRetainedEarningLine(string branchFilter, string branchManagementCodeFilter)
		{
			var includeBNEbranchData = (string.IsNullOrEmpty(branchManagementCodeFilter) || branchManagementCodeFilter.ToUpper().Contains("BRB")) && (string.IsNullOrEmpty(branchFilter) || branchFilter.ToUpper().Contains("BNE"));
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '" + branchFilter + "', 'N', '', 'Y', 'PNL', '', '', '', '0', '', 'N', '', '" + branchManagementCodeFilter + "',''");
			AssertAssertRetainedEarningLine(result, includeBNEbranchData, false);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '" + branchFilter + "', 'N', '', 'Y', 'PNL', '', '', '', '0', 'Branch', 'N', '', '" + branchManagementCodeFilter + "',''");
			AssertAssertRetainedEarningLine(result, includeBNEbranchData, false);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '" + branchFilter + "', 'N', '', 'Y', 'PNL', '', '', '', '0', 'Department', 'N', '','" + branchManagementCodeFilter + "', ''");
			AssertAssertRetainedEarningLine(result, includeBNEbranchData, false);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '" + branchFilter + "', 'N', '', 'Y', 'PNL', 'CHS', '', '', '0', '', 'N','', '" + branchManagementCodeFilter + "', ''");
			AssertAssertRetainedEarningLine(result, includeBNEbranchData, true);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '" + branchFilter + "', 'N', '', 'Y', 'PNL', 'CHS', '', '', '0', 'Branch', 'N', '','" + branchManagementCodeFilter + "', ''");
			AssertAssertRetainedEarningLine(result, includeBNEbranchData, true);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReport  201001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '" + branchFilter + "', 'N', '', 'Y', 'PNL', 'CHS', '', '', '0', 'Department', 'N', '','" + branchManagementCodeFilter + "', ''");
			AssertAssertRetainedEarningLine(result, includeBNEbranchData, true);
		}

		void AssertAssertRetainedEarningLine(DataTable result, bool expectLine, bool isCHS)
		{
			Assert("Result should have rows when line is expected", !expectLine || result.Rows.Count > 0);
			DataRow row4900 = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("4900") select row).FirstOrDefault();

			if (expectLine)
			{
				AssertNotNull("There should not be a row with 4900 GL Acount because we filter by other Branch", row4900);
				AssertEquals("CurrentPeriod field", -300M, decimal.Parse(row4900["CurrentPeriod"].ToString()));
				AssertEquals("YearToPeriod field", -600M, decimal.Parse(row4900["YearToPeriod"].ToString()));
				AssertEquals("LastYearToPeriod field", -100M, decimal.Parse(row4900["LastYearToPeriod"].ToString()));
				if (!isCHS)
				{
					AssertEquals("TotalLastYear field", -300M, decimal.Parse(row4900["TotalLastYear"].ToString()));
				}
			}
			else
			{
				AssertNull("Should not be row with 4900.00.00 GL Acount because we filter by other Branch", row4900);
			}
		}

		public void TestTotalLastYearAndLastYearToPeriodWithoutPresentationCategory()
		{
			SetupTotalLastYearAndLastYearToPeriodWithoutPresentationCategory();
			var result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201809);
			var plAppropriationNum = GetPlAppropriationNum();
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, DBNull.Value,
				DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201909);
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, -300M, DBNull.Value, -300M,
				-600M, DBNull.Value, -600M);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(202009);
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, -7500M, DBNull.Value,
				-7500M, -8100M, DBNull.Value, -8100M);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201812);
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, DBNull.Value,
				DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201912);
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, -600M, DBNull.Value, -600M,
				-600M, DBNull.Value, -600M);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(202012);
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, -8100M, DBNull.Value,
				-8100M, -8100M, DBNull.Value, -8100M);
		}

		public void TestTotalLastYearAndLastYearToPeriodWithPresentationCategory()
		{
			SetupTotalLastYearAndLastYearToPeriodWithoutPresentationCategory("ABC");

			var result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201809, "ABC");
			var plAppropriationNum = GetPlAppropriationNum();
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, DBNull.Value,
				DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201909, "ABC");
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, -300M, -300M,
				DBNull.Value, -600M, -600M);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(202009, "ABC");
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, -7500M,
				-7500M, DBNull.Value, -8100M, -8100M);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201812, "ABC");
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, DBNull.Value,
				DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(201912, "ABC");
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, -600M, -600M,
				DBNull.Value, -600M, -600M);
			result = CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(202012, "ABC");
			AssertResultForTotalLastYearAndLastYearToPeriod(plAppropriationNum, result, DBNull.Value, -8100M,
				-8100M, DBNull.Value, -8100M, -8100M);
		}

		void SetupTotalLastYearAndLastYearToPeriodWithoutPresentationCategory(string presentationCategory = "")
		{
			TestConnection.ExecuteNonQuery(@"INSERT INTO dbo.StmData(SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue)
													VALUES
													(NEWID(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'DT', CONVERT(varbinary(max), '0x32003000300030002D00300031002D00300031002000300030003A00300030003A00300030002E00300030003000', 1))");

			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(
				@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM dbo.stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());

			var plAccount = TestObjectCreator.InsertGLHeader("2222.00.00", "Test PL Account", "P&L", "", "DR", false, "AP");
			TestObjectCreator.CreateTestPeriodsForEntireYear(2018);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2019);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

			PrepareTestDataForPeriodAndGLAggregate(plAppropriationAccountPK, plAccount, presentationCategory);
		}

		void AssertResultForTotalLastYearAndLastYearToPeriod(string plAppropriationNum, DataTable result, object lastYearToPeriodS, object lastYearToPeriodP, object lastYearToPeriod, object totalLastYearS, object totalLastYearP, object totalLastYear)
		{
			var rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith(plAppropriationNum) select row).FirstOrDefault();
			AssertResultForLastYearToPeridAndTotalLastYear(rowSelected, lastYearToPeriodS, lastYearToPeriodP, lastYearToPeriod, totalLastYearS, totalLastYearP, totalLastYear);
		}

		void AssertResultForLastYearToPeridAndTotalLastYear(DataRow row, object lastYearToPeriodS, object lastYearToPeriodP, object lastYearToPeriod, object totalLastYearS, object totalLastYearP, object totalLastYear)
		{
			AssertEquals("LastYearToPeriodStandard field", lastYearToPeriodS, row["LastYearToPeriodStandard"]);
			AssertEquals("LastYearToPeriodPresentationJournals field", lastYearToPeriodP, row["LastYearToPeriodPresentationJournals"]);
			AssertEquals("LastYearToPeriod field", lastYearToPeriod, row["LastYearToPeriod"]);
			AssertEquals("TotalLastYearStandard field", totalLastYearS, row["TotalLastYearStandard"]);
			AssertEquals("TotalLastYearPresentationJournals field", totalLastYearP, row["TotalLastYearPresentationJournals"]);
			AssertEquals("TotalLastYear field", totalLastYear, row["TotalLastYear"]);
		}

		DataTable CallProfitAndLossReportForTotalLastYearAndLastYearToPeriod(int period, string presentationCategory = "")
		{
			var sql = @"
				DECLARE @p1 AS Int = {0}
				DECLARE @p2 AS UniqueIdentifier = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
				DECLARE @p3 AS NVarChar(100) = ''
				DECLARE @p4 AS NVarChar(100) = ''
				DECLARE @p5 AS NVarChar(100) = 'N'
				DECLARE @p6 AS NVarChar(100) = 'ALLACCT'
				DECLARE @p7 AS NVarChar(100) = '0'		-- RollUp
				DECLARE @p8 AS NVarChar(100) = 'Y'       -- Nature
				DECLARE @p9 AS NVarChar(100) = '{1}'		
				DECLARE @p10 AS NVarChar(100) = null
				EXEC [{2}].[dbo].ProfitAndLossReport @p1, @p2, @p3, @p4, @p5, @p6,'N', 'PNL', '', '', '', @p7, '', @p8, @p9, @p10";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sql, period, presentationCategory, ScriptDbName));
			return result;
		}

		void PrepareTestDataForPeriodAndGLAggregate(Guid appropriationAccountPK, Guid plAccountPK, string presentationCategory)
		{
			TestObjectCreator.InsertAccGLAggregate(100, 201808, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(200, 201809, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(300, 201810, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(400, 201908, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(500, 201909, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(600, 201910, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(700, 202008, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(800, 202009, appropriationAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(900, 202010, appropriationAccountPK, transactionCategory: presentationCategory);

			TestObjectCreator.InsertAccGLAggregate(1000, 201808, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(2000, 201809, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(3000, 201810, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(4000, 201908, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(5000, 201909, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(6000, 201910, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(7000, 202008, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(8000, 202009, plAccountPK, transactionCategory: presentationCategory);
			TestObjectCreator.InsertAccGLAggregate(9000, 202010, plAccountPK, transactionCategory: presentationCategory);
		}

		#region properties

		protected string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}

		protected string GetPlAppropriationNum()
		{
			var plAppropriationAccountPK = new Guid(TestConnection.ExecuteScalar(@"SELECT CONVERT(UNIQUEIDENTIFIER, CONVERT(NVARCHAR(4000), CONVERT(VARBINARY(8000), SD_BinaryValue)))
			FROM stmdata WHERE sd_name = 'GL_PL_APPROPRIATION_ACCOUNT'").ToString());
			var plAppropriationNum = TestConnection.ExecuteScalar($@"SELECT AG_AccountNum FROM AccGlHeader WHERE AG_PK = '{plAppropriationAccountPK}'").ToString();
			return plAppropriationNum;
		}

		#endregion
	}
}

