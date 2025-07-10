using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLossReportPeriodAnalysis))]
	class ProfitAndLossReportPeriodAnalysisTest : DbCreateScriptTest
	{
		public void TestReportTypeTBSAndBshAccountCRAndClosingBalanceIsZero()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "CR", "DR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-500.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'TBS', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeBSHAndBshAccountCRAndClosingBalanceIsZero()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "CR", "DR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-500.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, ''");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rows.Length);
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeTBSAndBshAccountCRAndClosingBalanceIsNegative()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "CR", "DR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-600.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'TBS', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeBSHAndBshAccountCRAndClosingBalanceIsNegative()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "CR", "DR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-600.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rows.Length);
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeTBSAndBshAccountCRAndClosingBalanceIsPositive()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "CR", "DR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-400.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'TBS', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("Account Number should be ALT account", "9110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 2", "Bank 2", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be ALT", "ALT", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("BSH account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeBSHAndBshAccountCRAndClosingBalanceIsPositive()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "CR", "DR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-400.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rows.Length);
			AssertEquals("Account Number should be ALT account", "9110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 2", "Bank 2", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be ALT", "ALT", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("BSH account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeTBSAndBshAccountDRAndClosingBalanceIsZero()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-500.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'TBS', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeBSHAndBshAccountDRAndClosingBalanceIsZero()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-500.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rows.Length);
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeTBSAndBshAccountDRAndClosingBalanceIsNegative()
		{
			CreateAccountPeriods();

			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-600.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'TBS', NULL, ''");
			DataRow[] rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("Account Number should be ALT account", "9110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 2", "Bank 2", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be ALT", "ALT", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("BSH account should not appear in data table", 0, rows.Length);
		}

		public void TestSummedFieldsExceedMoneyRangeWithNoException()
		{
			CreateAccountPeriods();

			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("922337203685477.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("12345.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, ''");
			var row = result.Select("AccountNumber = '7110.10.10'").First();

			AssertEquals("Period 1 should equal", 922337203697822.0000M, row["P1"]);
		}

		public void TestReportTypeBSHAndBshAccountDRAndClosingBalanceIsNegative()
		{
			CreateAccountPeriods();

			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-600.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, ''");
			DataRow[] altAccountRow = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, altAccountRow.Length);
			AssertEquals("Account Number should be ALT account", "9110.10.10", altAccountRow[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 2", "Bank 2", altAccountRow[0]["AccountName"].ToString());
			AssertEquals("Account Type should be ALT", "ALT", altAccountRow[0]["AccountType"].ToString());
			AssertEquals("Period 1 should equal", "5000.0000", altAccountRow[0]["P1"].ToString());
			AssertEquals("Period 2 should equal", "5000.0000", altAccountRow[0]["P2"].ToString());
			AssertEquals("Period 3 should equal", "5000.0000", altAccountRow[0]["P3"].ToString());
			AssertEquals("Period 4 should equal", "5000.0000", altAccountRow[0]["P4"].ToString());
			AssertEquals("Period 5 should equal", "5600.0000", altAccountRow[0]["P5"].ToString());
			AssertEquals("Period 6 should equal", "100.0000", altAccountRow[0]["P6"].ToString());
			AssertEquals("Period 7 should equal", "100.0000", altAccountRow[0]["P7"].ToString());
			AssertEquals("Period 8 should equal", "100.0000", altAccountRow[0]["P8"].ToString());
			AssertEquals("Period 9 should equal", "100.0000", altAccountRow[0]["P9"].ToString());
			AssertEquals("Period 10 should equal", "100.0000", altAccountRow[0]["P10"].ToString());
			AssertEquals("Period 11 should equal", "100.0000", altAccountRow[0]["P11"].ToString());
			AssertEquals("Closing balance should equal", "100.0000", altAccountRow[0]["P12"].ToString());
			DataRow[] bshAccountRow = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("BSH account should not appear in data table", 0, bshAccountRow.Length);
			DataRow[] totalAssetsRow = result.Select("AccountNumber = '7999.00.00'");
			AssertEquals("Total Assets Period 1 should equal", "0.0000", totalAssetsRow[0]["P1"].ToString());
			AssertEquals("Total Assets Period 2 should equal", "0.0000", totalAssetsRow[0]["P2"].ToString());
			AssertEquals("Total Assets Period 3 should equal", "0.0000", totalAssetsRow[0]["P3"].ToString());
			AssertEquals("Total Assets Period 4 should equal", "0.0000", totalAssetsRow[0]["P4"].ToString());
			AssertEquals("Total Assets Period 5 should equal", "0.0000", totalAssetsRow[0]["P5"].ToString());
			AssertEquals("Total Assets Period 6 should equal", "0.0000", totalAssetsRow[0]["P6"].ToString());
			AssertEquals("Total Assets Period 7 should equal", "0.0000", totalAssetsRow[0]["P7"].ToString());
			AssertEquals("Total Assets Period 8 should equal", "0.0000", totalAssetsRow[0]["P8"].ToString());
			AssertEquals("Total Assets Period 9 should equal", "0.0000", totalAssetsRow[0]["P9"].ToString());
			AssertEquals("Total Assets Period 10 should equal", "0.0000", totalAssetsRow[0]["P10"].ToString());
			AssertEquals("Total Assets Period 11 should equal", "0.0000", totalAssetsRow[0]["P11"].ToString());
			AssertEquals("Total Assets Period 12 should equal", "0.0000", totalAssetsRow[0]["P12"].ToString());
			DataRow[] totalLiabilitiesRow = result.Select("AccountNumber = '9799.00.00'");
			AssertEquals("Total Liabilities Period 1 should equal", "5000.0000", totalLiabilitiesRow[0]["P1"].ToString());
			AssertEquals("Total Liabilities Period 2 should equal", "5000.0000", totalLiabilitiesRow[0]["P2"].ToString());
			AssertEquals("Total Liabilities Period 3 should equal", "5000.0000", totalLiabilitiesRow[0]["P3"].ToString());
			AssertEquals("Total Liabilities Period 4 should equal", "5000.0000", totalLiabilitiesRow[0]["P4"].ToString());
			AssertEquals("Total Liabilities Period 5 should equal", "5600.0000", totalLiabilitiesRow[0]["P5"].ToString());
			AssertEquals("Total Liabilities Period 6 should equal", "100.0000", totalLiabilitiesRow[0]["P6"].ToString());
			AssertEquals("Total Liabilities Period 7 should equal", "100.0000", totalLiabilitiesRow[0]["P7"].ToString());
			AssertEquals("Total Liabilities Period 8 should equal", "100.0000", totalLiabilitiesRow[0]["P8"].ToString());
			AssertEquals("Total Liabilities Period 9 should equal", "100.0000", totalLiabilitiesRow[0]["P9"].ToString());
			AssertEquals("Total Liabilities Period 10 should equal", "100.0000", totalLiabilitiesRow[0]["P10"].ToString());
			AssertEquals("Total Liabilities Period 11 should equal", "100.0000", totalLiabilitiesRow[0]["P11"].ToString());
			AssertEquals("Total Liabilities Period 12 should equal", "100.0000", totalLiabilitiesRow[0]["P12"].ToString());
		}

		public void TestTransactionCategoryGroup()
		{
			CreateAccountPeriods();

			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-600.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "AAA");
			CreateAccountGLAggregate("-700.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "BBB");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "CCC");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', 'AAA', ''");

			DataRow[] altAccountRow = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, altAccountRow.Length);
			AssertEquals("Account Number should be ALT account", "9110.10.10", altAccountRow[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 2", "Bank 2", altAccountRow[0]["AccountName"].ToString());
			AssertEquals("Account Type should be ALT", "ALT", altAccountRow[0]["AccountType"].ToString());
			AssertEquals("Period 1 should equal", "5000.0000", altAccountRow[0]["P1"].ToString());
			AssertEquals("Period 2 should equal", "5000.0000", altAccountRow[0]["P2"].ToString());
			AssertEquals("Period 3 should equal", "5000.0000", altAccountRow[0]["P3"].ToString());
			AssertEquals("Period 4 should equal", "5000.0000", altAccountRow[0]["P4"].ToString());
			AssertEquals("Period 5 should equal", "5600.0000", altAccountRow[0]["P5"].ToString());
			AssertEquals("Period 6 should equal", "5600.0000", altAccountRow[0]["P6"].ToString());
			AssertEquals("Period 7 should equal", "5600.0000", altAccountRow[0]["P7"].ToString());
			AssertEquals("Period 8 should equal", "5600.0000", altAccountRow[0]["P8"].ToString());
			AssertEquals("Period 9 should equal", "5600.0000", altAccountRow[0]["P9"].ToString());
			AssertEquals("Period 10 should equal", "5600.0000", altAccountRow[0]["P10"].ToString());
			AssertEquals("Period 11 should equal", "5600.0000", altAccountRow[0]["P11"].ToString());
			AssertEquals("Closing balance should equal", "5600.0000", altAccountRow[0]["P12"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', 'AAA,BBB,CCC', ''");
			altAccountRow = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, altAccountRow.Length);
			AssertEquals("Account Number should be ALT account", "9110.10.10", altAccountRow[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 2", "Bank 2", altAccountRow[0]["AccountName"].ToString());
			AssertEquals("Account Type should be ALT", "ALT", altAccountRow[0]["AccountType"].ToString());
			AssertEquals("Period 1 should equal", "5000.0000", altAccountRow[0]["P1"].ToString());
			AssertEquals("Period 2 should equal", "5000.0000", altAccountRow[0]["P2"].ToString());
			AssertEquals("Period 3 should equal", "5000.0000", altAccountRow[0]["P3"].ToString());
			AssertEquals("Period 4 should equal", "5000.0000", altAccountRow[0]["P4"].ToString());
			AssertEquals("Period 5 should equal", "6300.0000", altAccountRow[0]["P5"].ToString());
			AssertEquals("Period 6 should equal", "800.0000", altAccountRow[0]["P6"].ToString());
			AssertEquals("Period 7 should equal", "800.0000", altAccountRow[0]["P7"].ToString());
			AssertEquals("Period 8 should equal", "800.0000", altAccountRow[0]["P8"].ToString());
			AssertEquals("Period 9 should equal", "800.0000", altAccountRow[0]["P9"].ToString());
			AssertEquals("Period 10 should equal", "800.0000", altAccountRow[0]["P10"].ToString());
			AssertEquals("Period 11 should equal", "800.0000", altAccountRow[0]["P11"].ToString());
			AssertEquals("Closing balance should equal", "800.0000", altAccountRow[0]["P12"].ToString());
		}

		public void TestContainUnits()
		{
			CreateAccountPeriods();
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK = TestHelper.InsertGLHeader("2201.02.01", "Test Account 1", "P&L", "");
			var accountPK1 = TestHelper.InsertGLHeader("2202.02.01", "Test Account KWH", "P&L", "KWH");
			var accountPK2 = TestHelper.InsertGLHeader("2203.02.01", "Test Account KWH", "P&L", "UNT");

			TestHelper.CreateGLAggregate(testDBHelper, "", 5, 100, 201201, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 6, 100, 201201, accountPK1, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			TestHelper.CreateGLAggregate(testDBHelper, "", 7, 100, 201201, accountPK2, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC ProfitAndLossReportPeriodAnalysis 201201, '{TestDbHelper.DefaultCompanyPK}', '', '', 'N', '', '', 'TBS', NULL, '' ");
			AssertEquals("Result should have 6 rows", 6, result.Rows.Count);

			DataRow rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2201") select row).FirstOrDefault();
			AssertEquals("Units field", "", rowSelected["Units"].ToString());
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2202") select row).FirstOrDefault();
			AssertEquals("Units field", "KWH", rowSelected["Units"].ToString());
			rowSelected = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith("2203") select row).FirstOrDefault();
			AssertEquals("Units field", "UNT", rowSelected["Units"].ToString());
		}

		public void TestContainNewColumnsWhichUseForCalculateTotalAmount()
		{
			CreateAccountPeriods();
			var testDBHelper = new TestDbHelper(Db.Connection);
			var accountPK = TestHelper.InsertGLHeader("8201.02.01", "Test Account 1", "NTE", "");
			var accountPK1 = TestHelper.InsertGLHeader("8202.02.01", "Test Account KWH", "P&L", "KWH");

			TestHelper.CreateGLAggregate(testDBHelper, "", 2, 100, 201201, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			CreateGLAggregateForFullYear(testDBHelper, 2013, 1, 12, accountPK);
			CreateGLAggregateForFullYear(testDBHelper, 2013, 1, 12, accountPK1);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC ProfitAndLossReportPeriodAnalysis 201312, '{TestDbHelper.DefaultCompanyPK}', '', '', 'N', '', '', 'TBS', NULL, '' ");
			AssertResult("8201.02.01");
			AssertResult("8202.02.01");

			void AssertResult(string accountNum)
			{
				DataRow rowdata = (from DataRow row in result.Rows where row["AccountNumber"].ToString().StartsWith(accountNum) select row).FirstOrDefault();
				var isNoteAccount = (rowdata["AccountType"]?.ToString() ?? "") == "NTE";
				if (isNoteAccount)
				{
					AssertEquals(200m, rowdata["P0"]);
					AssertEquals(0m, rowdata["P0ForCalculateTotal"]);
				}
				else
				{
					AssertEquals(DBNull.Value, rowdata["P0"]);
					AssertEquals(DBNull.Value, rowdata["P0ForCalculateTotal"]);
				}

				for (int i = 1; i <= 12; i++)
				{
					AssertEquals((decimal)(i * 100), rowdata[$"P{i.ToString()}"]);
					AssertEquals(isNoteAccount ? 0m : i * 100, rowdata[$"P{i.ToString()}ForCalculateTotal"]);
				}
			}
		}

		public void TestReportTypeTBSAndBshAccountDRAndClosingBalanceIsPositive()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-400.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'TBS', NULL, '' ");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypeBSHAndBshAccountDRAndClosingBalanceIsPositive()
		{
			CreateAccountPeriods();
			CreateAccountGLHeaders("3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1", "363958D7-A610-4015-97FD-A6C15E701BD8", "DR", "CR");
			CreateAccountGLAggregate("-5000.0000", "201201", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("-400.0000", "201305", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");
			CreateAccountGLAggregate("5500.0000", "201306", "3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', 'N', '', '', 'BSH', NULL, ''");
			DataRow[] rows = result.Select("AccountNumber = '7110.10.10'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rows.Length);
			AssertEquals("Account Number should be BSH account", "7110.10.10", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be Bank 1", "Bank 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be BSH", "BSH", rows[0]["AccountType"].ToString());
			rows = result.Select("AccountNumber = '9110.10.10'");
			AssertEquals("ALT account should not appear in data table", 0, rows.Length);
		}

		public void TestReportTypePNLWithDepartmentDissection()
		{
			CreateAccountPeriods();
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) " +
								   "VALUES ('3c1076ee-33be-4645-9cfe-b45dcb7526e7', '1330.20.30', 'PNL Account 1', 'P&L', 'CR', 1, NULL, 0, 'TS')");
			CreateAccountGLAggregate("100.0000", "201305", "3c1076ee-33be-4645-9cfe-b45dcb7526e7");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '', 'ALLACCT', '', 'PNL', '', '', 'Department'");
			DataRow[] rows = result.Select("AccountNumber = '1330.20.30' AND AdditionalDissection = 'BRN'");
			AssertEquals("Should only find 1 account that matches criteria", 1, rows.Length);
			AssertEquals("Account Number should be", "1330.20.30", rows[0]["AccountNumber"].ToString());
			AssertEquals("Account Name should be", "PNL Account 1", rows[0]["AccountName"].ToString());
			AssertEquals("Account Type should be", "P&L", rows[0]["AccountType"].ToString());
			// check totals
			rows = result.Select("AccountNumber = '1900.00.00'");
			AssertEquals("Account Name should be", "GROSS PROFIT FROM OPERATION", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '2999.00.00'");
			AssertEquals("Account Name should be", "TOTAL INCOME", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '4499.00.00'");
			AssertEquals("Account Name should be", "GROSS OPERATING PROFIT", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '4599.00.00'");
			AssertEquals("Account Name should be", "OPERATING PROFIT BEFORE TAX", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '4699.00.00'");
			AssertEquals("Account Name should be", "OPERATING PROFIT AFTER TAX", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '4799.00.00'");
			AssertEquals("Account Name should be", "TOTAL AVAILABLE FOR APPROPRIATION", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '4899.00.00'");
			AssertEquals("Account Name should be", "TOTAL YTD UNAPPROPRIATED PROFITS", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
			rows = result.Select("AccountNumber = '4999.00.00'");
			AssertEquals("Account Name should be", "RETAINED PROFITS", rows[0]["AccountName"].ToString());
			AssertEquals("Total should be", "-100.0000", rows[0]["P5"].ToString());
		}

		public void TestProfitAndLossWithBranchAndDepartmentLoadAPAccounts()
		{
			CreateAccountPeriods();
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) " +
								   "VALUES ('3c1076ee-33be-4645-9cfe-b45dcb7526e7', '1330.20.30', 'PNL Account 1', 'P&L', 'CR', 1, NULL, 0, 'AP')");
			CreateAccountGLAggregate("100.0000", "201305", "3c1076ee-33be-4645-9cfe-b45dcb7526e7");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '', 'ALLACCT', '', 'PNL', '', '', ''");
			DataRow[] rows = result.Select("AccountNumber = '1330.20.30'");
			AssertEquals("Should have 1 row", 1, rows.Length);
			DataTable result2 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '', 'ALLACCT', '', 'PNL', '', '', 'Department'");
			DataRow[] rows2 = result2.Select("AccountNumber = '1330.20.30'");
			AssertEquals("Should have 2 rows", 2, rows2.Length);
			DataTable result3 = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201312, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '', 'ALLACCT', '', 'PNL', '', '', 'Branch'");
			DataRow[] rows3 = result3.Select("AccountNumber = '1330.20.30'");
			AssertEquals("Should have 2 rows", 2, rows3.Length);
		}

		void CreateAccountPeriods()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201201', 2012, '2012-01-01 00:00:00.000','2012-01-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201202', 2012, '2012-02-01 00:00:00.000','2012-02-29 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201203', 2012, '2012-03-01 00:00:00.000','2012-03-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201204', 2012, '2012-04-01 00:00:00.000','2012-04-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201205', 2012, '2012-05-01 00:00:00.000','2012-05-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201206', 2012, '2012-06-01 00:00:00.000','2012-06-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201207', 2012, '2012-07-01 00:00:00.000','2012-07-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201208', 2012, '2012-08-01 00:00:00.000','2012-08-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201209', 2012, '2012-09-01 00:00:00.000','2012-09-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201210', 2012, '2012-10-01 00:00:00.000','2012-10-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201211', 2012, '2012-11-01 00:00:00.000','2012-11-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201212', 2012, '2012-12-01 00:00:00.000','2012-12-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201301', 2013, '2013-01-01 00:00:00.000','2013-01-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201302', 2013, '2013-02-01 00:00:00.000','2013-02-28 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201303', 2013, '2013-03-01 00:00:00.000','2013-03-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201304', 2013, '2013-04-01 00:00:00.000','2013-04-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201305', 2013, '2013-05-01 00:00:00.000','2013-05-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201306', 2013, '2013-06-01 00:00:00.000','2013-06-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201307', 2013, '2013-07-01 00:00:00.000','2013-07-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201308', 2013, '2013-08-01 00:00:00.000','2013-08-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201309', 2013, '2013-09-01 00:00:00.000','2013-09-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201310', 2013, '2013-10-01 00:00:00.000','2013-10-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201311', 2013, '2013-11-01 00:00:00.000','2013-11-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) VALUES (NEWID(), '201312', 2013, '2013-12-01 00:00:00.000','2013-12-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		void CreateAccountGLHeaders(string accountPK, string alternateAccountPK, string bshAccountDebitCreditType, string altAccountDebitCreditType)
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column, AG_SystemCreateTimeUtc, AG_SystemCreateUser, AG_SystemLastEditTimeUtc, AG_SystemLastEditUser) VALUES ('" + alternateAccountPK + "', '9110.10.10', 'Bank 2', 'ALT', '" + altAccountDebitCreditType + "', 1, NULL, 0, 'AS', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column, AG_AG_AlternateNum, AG_SystemCreateTimeUtc, AG_SystemCreateUser, AG_SystemLastEditTimeUtc, AG_SystemLastEditUser) VALUES ('" + accountPK + "', '7110.10.10', 'Bank 1', 'BSH', '" + bshAccountDebitCreditType + "',1, NULL, 0, 'AS', '" + alternateAccountPK + "', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		void CreateAccountGLAggregate(string amount, string period, string accountPK, string transactionCategory = "")
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) VALUES (newid()," + amount + "," + period + ",'" + accountPK + "', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '" + transactionCategory + "')");
		}

		void CreateGLAggregateForFullYear(TestDbHelper testDBHelper, int year, int startMonth, int endMonth, Guid accountPK)
		{
			for (int i = startMonth; i <= endMonth; i++)
			{
				TestHelper.CreateGLAggregate(testDBHelper, "", i, 100, year * 100 + i, accountPK, testDBHelper.DefaultDepartmentPK, testDBHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			}
		}

		public void TestOpeningBalance()
		{
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_ArchiveCommenced, AM_EndDate, AM_GC_Company, AM_IsGeneralLedgerClosed, AM_IsSubLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_Period, AM_StartDate, AM_Year)
VALUES ('8d161f3a-ca7a-4858-a757-c7b8e8399200', 0, '2008-12-31 23:59:00.000', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 0, 0, 0, 200812, '2008-12-01 00:00:00.000', 2008)

INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_ArchiveCommenced, AM_EndDate, AM_GC_Company, AM_IsGeneralLedgerClosed, AM_IsSubLedgerClosed, AM_IsSubledgerClosedForAdjustments, AM_Period, AM_StartDate, AM_Year)
VALUES ('5db63c0a-53aa-456e-b975-81041cfe71ed', 0, '2009-01-31 23:59:00.000', '878d7aca-ffc3-49fc-9710-969ca0c0f2ac', 0, 0, 0, 200901, '2009-01-01 00:00:00.000', 2009)");
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 100.0000, 200812, '176E946D-3483-4B2B-AA32-E348CBD94DFD', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 200901, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '','', '', '', 'TBS', NULL, '' ");

			AssertEquals("Field Account Number", "4900.00.00", result.Rows[0][0].ToString());
			AssertEquals("Field P0", 100M, decimal.Parse(result.Rows[0][7].ToString()));
		}

		public void TestProfitAndLossReportHideHeaderAndTotalForZeroBalanceAccountIfIncludeZeroBalanceIsFalse()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201001', 2010, '2010-01-01 00:00:00.000','2010-01-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201002', 2010, '2010-02-01 00:00:00.000','2010-02-28 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201003', 2010, '2010-03-01 00:00:00.000','2010-03-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201004', 2010, '2010-04-01 00:00:00.000','2010-04-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201005', 2010, '2010-05-01 00:00:00.000','2010-05-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201006', 2010, '2010-06-01 00:00:00.000','2010-06-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201007', 2010, '2010-07-01 00:00:00.000','2010-07-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201008', 2010, '2010-08-01 00:00:00.000','2010-08-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201009', 2010, '2010-09-01 00:00:00.000','2010-09-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201010', 2010, '2010-10-01 00:00:00.000','2010-10-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201011', 2010, '2010-11-01 00:00:00.000','2010-11-30 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), '201012', 2010, '2010-12-01 00:00:00.000','2010-12-31 23:59:00.000','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.AccChargeCode");
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.AccGLHeader");

			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1', '1999.99.99', 'Total 1', 'TTL','DR',0, NULL, 3, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('363958D7-A610-4015-97FD-A6C15E701BD8', '1000.00.00', 'Header 1', 'HDR','DR',0, '3EF0B396-A27F-4F0F-8BF3-A1D6E6A019C1', 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('AB450091-10A8-4782-A993-29751C9478C8', '1899.99.99', 'Total 2', 'TTL','DR',0, NULL, 2, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('426F0D98-F6A0-4341-B8A0-3A98C9DDBF96', '1100.00.00', 'Header 2', 'HDR','DR',0, 'AB450091-10A8-4782-A993-29751C9478C8', 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('B5BEC451-FFFF-43BA-AC69-614DD80AC43D', '1299.99.99', 'Total 2.1', 'TTL','DR',0, NULL, 1, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('69042485-DFD4-43BD-8EF1-07DC45F64236', '1200.00.00', 'Header 2.1', 'HDR','DR',0, 'B5BEC451-FFFF-43BA-AC69-614DD80AC43D', 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('06262709-2659-4FBD-ADD3-D0295D80D5AB', '1699.99.99', 'Total 2.2', 'TTL','DR',0, NULL, 1, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('98796AB4-08B2-4C21-BD6D-13C73DD6BB66', '1300.00.00', 'Header 2.2', 'HDR','DR',0, '06262709-2659-4FBD-ADD3-D0295D80D5AB', 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('17059159-5852-468A-8AE5-3E1C82EB80E4', '1201.00.00', 'Header 2.1 P&L Account', 'P&L','DR',0, NULL, 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('32AB75DC-FA3C-4451-8BD8-279AE69FABC4', '1301.00.00', 'Header 2.2 P&L Account', 'P&L','DR',0, NULL, 0, 'TS')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column) VALUES ('65BBEAF1-B62C-4837-B45B-B069FB0A5ECB', '9999.99.99', 'control account', 'BSH','DR',1, NULL, 0, 'LI')");

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -100.0000, 201009, '65BBEAF1-B62C-4837-B45B-B069FB0A5ECB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 100.0000, 201009, '65BBEAF1-B62C-4837-B45B-B069FB0A5ECB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), -100.0000, 201009, '17059159-5852-468A-8AE5-3E1C82EB80E4', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')

INSERT INTO dbo.AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory) 
VALUES (newid(), 100.0000, 201009, '65BBEAF1-B62C-4837-B45B-B069FB0A5ECB', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '')
");

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "EXEC ProfitAndLossReportPeriodAnalysis 201010, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '', '', 'ALLACCT', '', 'PNL', NULL, '' ");
			DataRow rowHeader22 = (from DataRow row in result.Rows where row["AccountName"].ToString().Equals("Header 2.2") select row).FirstOrDefault();
			AssertNull("Should not have row with GL Acount Name Header 2.2 because we hide header for zero balance account if InclZeroBal <> 'Y'", rowHeader22);
			DataRow rowTotal22 = (from DataRow row in result.Rows where row["AccountName"].ToString().Equals("Total 2.2") select row).FirstOrDefault();
			AssertNull("Should not have row with GL Acount Name Total 2.2 because we hide Total for zero balance account if InclZeroBal <> 'Y'", rowTotal22);
			AssertEquals("Result should have 7 rows", 7, result.Rows.Count);
		}
	}
}

