using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ChinaReportsBreakdownByCategoriesTest : ScriptTest
	{
		[TestDate(2017, 02, 14)]
		public void TestErrorReportType()
		{
			PrepareTestData();

			DataTable results = RunScript(200912, "BSS");
			AssertEquals("Should not return row", 0, results.Rows.Count);
		}

		[TestDate(2017, 02, 14)]
		public void TestChinaBalanceSheetBreakdownByCategories()
		{
			PrepareTestData();

			DataTable results = RunScript(200912, "BSH");

			var headers = new[] { "CurrentAmount", "LastYearEndAmount", "LocalAccountNumber", "AccountDescription", "Category", "AccountNumber", "AccountName", "CategoryDescription" };
			var lines = new[]
			{
				new object[] { 100.0000m, 0.0000m, "53000.95", "", "D01", "5300.03.95", "TEST BSH 1", "货币资金" },
				new object[] { 200.0000m, 0.0000m, "53000.96", "", "D01", "5300.03.96", "TEST BSH 2", "货币资金" },
				new object[] { 300.0000m, 0.0000m, "53000.97", "", "D02", "5300.03.97", "TEST BSH 3", "△交易性金融资产" },
				new object[] { 300.0000m, 0.0000m, "53000.98", "", "D03", "5300.03.98", "TEST BSH 4", "＃短期投资" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaBalanceSheetBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201001, "BSH");
			lines = new[]
			{
				new object[] { 100.0000m, 100.0000m, "53000.95", "", "D01", "5300.03.95", "TEST BSH 1", "货币资金" },
				new object[] { 200.0000m, 200.0000m, "53000.96", "", "D01", "5300.03.96", "TEST BSH 2", "货币资金" },
				new object[] { 1400.0000m, 300.0000m, "53000.97", "", "D02", "5300.03.97", "TEST BSH 3", "△交易性金融资产" },
				new object[] { 1400.0000m, 300.0000m, "53000.98", "", "D03", "5300.03.98", "TEST BSH 4", "＃短期投资" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaBalanceSheetBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201012, "BSH");
			lines = new[]
			{
				new object[] { 1100.0000m, 100.0000m, "53000.95", "", "D01", "5300.03.95", "TEST BSH 1", "货币资金" },
				new object[] { 2200.0000m, 200.0000m, "53000.96", "", "D01", "5300.03.96", "TEST BSH 2", "货币资金" },
				new object[] { 4400.0000m, 300.0000m, "53000.97", "", "D02", "5300.03.97", "TEST BSH 3", "△交易性金融资产" },
				new object[] { 4400.0000m, 300.0000m, "53000.98", "", "D03", "5300.03.98", "TEST BSH 4", "＃短期投资" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaBalanceSheetBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });
		}

		[TestDate(2017, 02, 14)]
		public void TestChinaProfitAndLossMonthlyBreakdownByCategories()
		{
			PrepareTestData();

			DataTable results = RunScript(200912, "PLM");

			var headers = new[] { "CurrentAmount", "LastYearEndAmount", "LocalAccountNumber", "AccountDescription", "Category", "AccountNumber", "AccountName", "CategoryDescription" };
			var lines = new[]
			{
				new object[] { 0.0000m, -101.0000m, "61000.95", "", "D14", "1100.03.95", "TEST PNL 1", "营业费用" },
				new object[] { 0.0000m, -201.0000m, "61000.96", "", "D14", "1100.03.96", "TEST PNL 2", "营业费用" },
				new object[] { 0.0000m, -301.0000m, "63000.97", "", "D15", "1200.03.97", "TEST PNL 3", "管理费用" },
				new object[] { 0.0000m, 301.0000m, "53000.98", "", "D16", "1200.03.98", "TEST PNL 4", "财务费用" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAndLossMonthlyBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201001, "PLM");
			lines = new[]
			{
				new object[] { -1101.0000m, -1101.0000m, "63000.97", "", "D15", "1200.03.97", "TEST PNL 3", "管理费用" },
				new object[] { 1101.0000m, 1101.0000m, "53000.98", "", "D16", "1200.03.98", "TEST PNL 4", "财务费用" },
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAndLossMonthlyBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201012, "PLM");
			lines = new[]
			{
				new object[] { 0.0000m, -1001.0000m, "61000.95", "", "D14", "1100.03.95", "TEST PNL 1", "营业费用" },
				new object[] { 0.0000m, -2001.0000m, "61000.96", "", "D14", "1100.03.96", "TEST PNL 2", "营业费用" },
				new object[] { 0.0000m, -4102.0000m, "63000.97", "", "D15", "1200.03.97", "TEST PNL 3", "管理费用" },
				new object[] { 0.0000m, 4102.0000m, "53000.98", "", "D16", "1200.03.98", "TEST PNL 4", "财务费用" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAndLossMonthlyBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });
		}

		[TestDate(2017, 02, 14)]
		public void TestChinaProfitAndLossYearlyBreakdownByCategories()
		{
			PrepareTestData();

			DataTable results = RunScript(200912, "PNL");

			var headers = new[] { "CurrentAmount", "LastYearEndAmount", "LocalAccountNumber", "AccountDescription", "Category", "AccountNumber", "AccountName", "CategoryDescription" };
			var lines = new[]
			{
				new object[] { -101.0000m, 0.0000m, "61000.95", "", "D11", "1100.03.95", "TEST PNL 1", "研究与开发费" },
				new object[] { -201.0000m, 0.0000m, "61000.96", "", "D11", "1100.03.96", "TEST PNL 2", "研究与开发费" },
				new object[] { -301.0000m, 0.0000m, "63000.97", "", "D12", "1200.03.97", "TEST PNL 3", "财务费用" },
				new object[] { 301.0000m, 0.0000m, "53000.98", "", "D13", "1200.03.98", "TEST PNL 4", "其中：利息支出" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAndLossYearlyBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201001, "PNL");
			lines = new[]
			{
				new object[] { 0.0000m, -101.0000m, "61000.95", "", "D11", "1100.03.95", "TEST PNL 1", "研究与开发费" },
				new object[] { 0.0000m, -201.0000m, "61000.96", "", "D11", "1100.03.96", "TEST PNL 2", "研究与开发费" },
				new object[] { -1101.0000m, -301.0000m, "63000.97", "", "D12", "1200.03.97", "TEST PNL 3", "财务费用" },
				new object[] { 1101.0000m, 301.0000m, "53000.98", "", "D13", "1200.03.98", "TEST PNL 4", "其中：利息支出" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAndLossYearlyBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201012, "PNL");
			lines = new[]
			{
				new object[] { -1001.0000m, -101.0000m, "61000.95", "", "D11", "1100.03.95", "TEST PNL 1", "研究与开发费" },
				new object[] { -2001.0000m, -201.0000m, "61000.96", "", "D11", "1100.03.96", "TEST PNL 2", "研究与开发费" },
				new object[] { -4102.0000m, -301.0000m, "63000.97", "", "D12", "1200.03.97", "TEST PNL 3", "财务费用" },
				new object[] { 4102.0000m, 301.0000m, "53000.98", "", "D13", "1200.03.98", "TEST PNL 4", "其中：利息支出" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAndLossYearlyBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });
		}

		[TestDate(2017, 02, 14)]
		public void TestChinaProfitAppropriationBreakdownByCategories()
		{
			PrepareTestData();

			DataTable results = RunScript(200912, "PLA");

			var headers = new[] { "CurrentAmount", "LastYearEndAmount", "LocalAccountNumber", "AccountDescription", "Category", "AccountNumber", "AccountName", "CategoryDescription" };
			var lines = new[]
			{
				new object[] { -301.0000m, 0.0000m, "63000.97", "", "A03", "1200.03.97", "TEST PNL 3", "利润分配--盈余公积转入" },
				new object[] { 301.0000m, 0.0000m, "53000.98", "", "A04", "1200.03.98", "TEST PNL 4", "利润分配--提取法定盈余公积" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAppropriationBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201001, "PLA");
			lines = new[]
			{
				new object[] { -101.0000m, 0.0000m, "61000.95", "", "A02", "1100.03.95", "TEST PNL 1", "未分配利润" },
				new object[] { -201.0000m, 0.0000m, "61000.96", "", "A02", "1100.03.96", "TEST PNL 2", "未分配利润" },
				new object[] { -1101.0000m, -301.0000m, "63000.97", "", "A03", "1200.03.97", "TEST PNL 3", "利润分配--盈余公积转入" },
				new object[] { 1101.0000m, 301.0000m, "53000.98", "", "A04", "1200.03.98", "TEST PNL 4", "利润分配--提取法定盈余公积" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAppropriationBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });

			results = RunScript(201012, "PLA");
			lines = new[]
			{
				new object[] { -101.0000m, 0.0000m, "61000.95", "", "A02", "1100.03.95", "TEST PNL 1", "未分配利润" },
				new object[] { -201.0000m, 0.0000m, "61000.96", "", "A02", "1100.03.96", "TEST PNL 2", "未分配利润" },
				new object[] { -4102.0000m, -301.0000m, "63000.97", "", "A03", "1200.03.97", "TEST PNL 3", "利润分配--盈余公积转入" },
				new object[] { 4102.0000m, 301.0000m, "53000.98", "", "A04", "1200.03.98", "TEST PNL 4", "利润分配--提取法定盈余公积" }
			};
			AssertDataTableAllRowsByKeyColumns("ChinaProfitAppropriationBreakdownByCategories", results, headers, lines, new[] { "Category", "AccountNumber" });
		}

		void PrepareTestData()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(2008);
			testHelper.PostPeriodsForEntireYear(2009);
			testHelper.PostPeriodsForEntireYear(2010);

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			//BSH
			AccGLHeader glHeader1BSH = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "TEST BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2BSH = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "TEST BSH 2", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader3BSH = testObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "TEST BSH 3", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4BSH = testObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "TEST BSH 4", "BSH", Constants.DebitCredit.Credit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1BSH, "53000.95", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1BSH, "BSH", "D01");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2BSH, "53000.96", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2BSH, "BSH", "D01");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader3BSH, "53000.97", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader3BSH, "BSH", "D02");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader4BSH, "53000.98", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit), glHeader4BSH, "BSH", "D03");

			testObjectCreator.CreateAccGLAggregate(100m, 200912, glHeader1BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(200m, 200912, glHeader2BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(300m, 200912, glHeader3BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-300m, 200912, glHeader4BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1100m, 201001, glHeader3BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1100m, 201001, glHeader4BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1000m, 201012, glHeader1BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(2000m, 201012, glHeader2BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(3000m, 201012, glHeader3BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-3000m, 201012, glHeader4BSH.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			//PNL & PLM & PLA
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "TEST PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "TEST PNL 2", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "TEST PNL 3", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4PNL = testObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "TEST PNL 4", "P&L", Constants.DebitCredit.Credit);

			AccGLAccountDescriptor accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader1PNL, "P&L", "D11");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader1PNL, "PLM", "D14");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader1PNL, "PLA", "A02");

			accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "61000.96", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader2PNL, "P&L", "D11");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader2PNL, "PLM", "D14");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader2PNL, "PLA", "A02");

			accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader3PNL, "63000.97", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader3PNL, "P&L", "D12");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader3PNL, "PLM", "D15");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader3PNL, "PLA", "A03");

			accGLAccountDescriptor = testObjectCreator.CreateAccountDescriptor(glHeader4PNL, "53000.98", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader4PNL, "P&L", "D13");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader4PNL, "PLM", "D16");
			testObjectCreator.CreateGLDescriptorPivot(accGLAccountDescriptor, glHeader4PNL, "PLA", "A04");

			testObjectCreator.CreateAccGLAggregate(101m, 200911, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(201m, 200911, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(301m, 200911, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-301m, 200911, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1101m, 201001, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1101m, 201001, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(1001m, 201011, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(2001m, 201011, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(3001m, 201011, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-3001m, 201011, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();
		}

		DataTable RunScript(int period, string reportType)
		{
			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format(@"EXEC ChinaCommonReportsBreakdownByCategories '{0}', {1}, '{2}', '{3}', '','{4}', 'ZH-CN', 'CN'",
													reportType,
													period,
													GlbCompany.CurrentCompany.PK,
													GetReportCategoriesFromCode(reportType),
													GlbBranch.CurrentBranch.GB_Code
													)
								);

			return table;
		}

		string GetReportCategoriesFromCode(string reportType)
		{
			string result = string.Empty;

			switch (reportType)
			{
				case "BSH":
					result = "D01, D02, D03, D04, D05, D06, D07, D08, D09, D10, D11, D12, D13, D14, D20, D21, D22, D23, D24, D25, D26, D27, D28, D30, D32, D33, D34, D35, D36, D37, D38, D39, D40, D41, D42, D43, D44, D45, D46, H01, H02, H03, H04, H05, H06, H07, H08, H09, H10, H11, H12, H13, H14, H15, H16, H30, H31, H32, H33, H34, H35, H36, H37, H38, H41, H42, H43, H44, H45, H46, H47, H48, H49, H50, H51, H52, H53, H54, H55, H57, H59, D01_YED, D02_YED, D03_YED, D04_YED, D05_YED, D06_YED, D07_YED, D08_YED, D09_YED, D10_YED, D11_YED, D12_YED, D13_YED, D14_YED, D20_YED, D21_YED, D22_YED, D23_YED, D24_YED, D25_YED, D26_YED, D27_YED, D28_YED, D30_YED, D32_YED, D33_YED, D34_YED, D35_YED, D36_YED, D37_YED, D38_YED, D39_YED, D40_YED, D41_YED, D42_YED, D43_YED, D44_YED, D45_YED, D46_YED, H01_YED, H02_YED, H03_YED, H04_YED, H05_YED, H06_YED, H07_YED, H08_YED, H09_YED, H10_YED, H11_YED, H12_YED, H13_YED, H14_YED, H15_YED, H16_YED, H30_YED, H31_YED, H32_YED, H33_YED, H34_YED, H35_YED, H36_YED, H37_YED, H38_YED, H41_YED, H42_YED, H43_YED, H44_YED, H45_YED, H46_YED, H47_YED, H48_YED, H49_YED, H50_YED, H51_YED, H52_YED, H53_YED, H54_YED, H55_YED, H57_YED, H59_YED";
					break;

				case "PLM":
					result = "D01, D04, D05, D11, D14, D15, D16, D19, D22, D23, D25, D28, D29, D30, D01_YED, D04_YED, D05_YED, D11_YED, D14_YED, D15_YED, D16_YED, D19_YED, D22_YED, D23_YED, D25_YED, D28_YED, D29_YED, D30_YED";
					break;

				case "PNL":
					result = "D01, D02, D03, D04, D05, D06, D07, D08, D09, D10, D11, D12, D13, D14, D20, D21, D22, D23, D24, D25, D26, D27, D28, D30, D32, D33, D34, D35, D36, D37, D38, D39, D40, D41, D42, D43, D44, D45, D46, H01, H02, H03, H04, H05, H06, H07, H08, H09, H10, H11, H12, H13, H14, H15, H16, H30, H31, H32, H33, H34, H35, H36, H37, H38, H41, H42, H43, H44, H45, H46, H47, H48, H49, H50, H51, H52, H53, H54, H55, H57, H59, D01_YED, D02_YED, D03_YED, D04_YED, D05_YED, D06_YED, D07_YED, D08_YED, D09_YED, D10_YED, D11_YED, D12_YED, D13_YED, D14_YED, D20_YED, D21_YED, D22_YED, D23_YED, D24_YED, D25_YED, D26_YED, D27_YED, D28_YED, D30_YED, D32_YED, D33_YED, D34_YED, D35_YED, D36_YED, D37_YED, D38_YED, D39_YED, D40_YED, D41_YED, D42_YED, D43_YED, D44_YED, D45_YED, D46_YED, H01_YED, H02_YED, H03_YED, H04_YED, H05_YED, H06_YED, H07_YED, H08_YED, H09_YED, H10_YED, H11_YED, H12_YED, H13_YED, H14_YED, H15_YED, H16_YED, H30_YED, H31_YED, H32_YED, H33_YED, H34_YED, H35_YED, H36_YED, H37_YED, H38_YED, H41_YED, H42_YED, H43_YED, H44_YED, H45_YED, H46_YED, H47_YED, H48_YED, H49_YED, H50_YED, H51_YED, H52_YED, H53_YED, H54_YED, H55_YED, H57_YED, H59_YED";
					break;

				case "PLA":
					result = "A02, A03, A04, A05, A06, A07, A08, A09, A10, A11, A12, A13, A02_YED, A03_YED, A04_YED, A05_YED, A06_YED, A07_YED, A08_YED, A09_YED, A10_YED, A11_YED, A12_YED, A13_YED";
					break;
			}

			return result;
		}

		#region Implementation

		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			base.SetUp();
		}
		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
			base.TearDown();
		}

		#endregion
	}
}
