using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(ChinaCommonReportsBreakdownByCategories))]
	class ChineseReportsBreakdownByReportCategoriesTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestChinaBalanceSheetBreakdownByCategories()
		{
			var results = RunScript(200912, "BSH");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			var table = createDataTable();

			AddRowIntoTable(table, new[]
			{
				new object[] { 100.0000m, 0.0000m, "53000.95", "", "D01", "5300.03.95", "ACC1", "货币资金" },
				new object[] { 200.0000m, 0.0000m, "53000.96", "", "D01", "5300.03.96", "ACC2", "货币资金" },
				new object[] { 300.0000m, 0.0000m, "53000.97", "", "D02", "5300.03.97", "ACC3", "△交易性金融资产" },
				new object[] { 300.0000m, 0.0000m, "53000.98", "", "D03", "5300.03.98", "ACC4", "＃短期投资" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201001, "BSH");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { 100.0000m, 100.0000m, "53000.95", "", "D01", "5300.03.95", "ACC1", "货币资金" },
				new object[] { 200.0000m, 200.0000m, "53000.96", "", "D01", "5300.03.96", "ACC2", "货币资金" },
				new object[] { 1400.0000m, 300.0000m, "53000.97", "", "D02", "5300.03.97", "ACC3", "△交易性金融资产" },
				new object[] { 1400.0000m, 300.0000m, "53000.98", "", "D03", "5300.03.98", "ACC4", "＃短期投资" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201012, "BSH");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { 1100.0000m, 100.0000m, "53000.95", "", "D01", "5300.03.95", "ACC1", "货币资金" },
				new object[] { 2200.0000m, 200.0000m, "53000.96", "", "D01", "5300.03.96", "ACC2", "货币资金" },
				new object[] { 4400.0000m, 300.0000m, "53000.97", "", "D02", "5300.03.97", "ACC3", "△交易性金融资产" },
				new object[] { 4400.0000m, 300.0000m, "53000.98", "", "D03", "5300.03.98", "ACC4", "＃短期投资" }
			});
			AssertDataTables("it should be same", table, results);
		}

		public void TestChinaProfitAndLossMonthlyBreakdownByCategories()
		{
			var results = RunScript(200912, "PLM");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			var table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { 0.0000m, -101.0000m, "61000.95", "", "D14", "1100.03.95", "ACC5", "营业费用" },
				new object[] { 0.0000m, -201.0000m, "61000.96", "", "D14", "1100.03.96", "ACC6", "营业费用" },
				new object[] { 0.0000m, -301.0000m, "63000.97", "", "D15", "1200.03.97", "ACC7", "管理费用" },
				new object[] { 0.0000m, 301.0000m, "53000.98", "", "D16", "1200.03.98", "ACC8", "财务费用" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201001, "PLM");
			AssertEquals("Result should have two row", 2, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { -1101.0000m, -1101.0000m, "63000.97", "", "D15", "1200.03.97", "ACC7", "管理费用" },
				new object[] { 1101.0000m, 1101.0000m, "53000.98", "", "D16", "1200.03.98", "ACC8", "财务费用" },
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201012, "PLM");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { 0.0000m, -1001.0000m, "61000.95", "", "D14", "1100.03.95", "ACC5", "营业费用" },
				new object[] { 0.0000m, -2001.0000m, "61000.96", "", "D14", "1100.03.96", "ACC6", "营业费用" },
				new object[] { 0.0000m, -4102.0000m, "63000.97", "", "D15", "1200.03.97", "ACC7", "管理费用" },
				new object[] { 0.0000m, 4102.0000m, "53000.98", "", "D16", "1200.03.98", "ACC8", "财务费用" }
			});
			AssertDataTables("it should be same", table, results);
		}

		public void TestChinaProfitAndLossYearlyBreakdownByCategories()
		{
			var results = RunScript(200912, "PNL");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			var table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { -101.0000m, 0.0000m, "61000.95", "", "D11", "1100.03.95", "ACC5", "研究与开发费" },
				new object[] { -201.0000m, 0.0000m, "61000.96", "", "D11", "1100.03.96", "ACC6", "研究与开发费" },
				new object[] { -301.0000m, 0.0000m, "63000.97", "", "D12", "1200.03.97", "ACC7", "财务费用" },
				new object[] { 301.0000m, 0.0000m, "53000.98", "", "D13", "1200.03.98", "ACC8", "其中：利息支出" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201001, "PNL");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { 0.0000m, -101.0000m, "61000.95", "", "D11", "1100.03.95", "ACC5", "研究与开发费" },
				new object[] { 0.0000m, -201.0000m, "61000.96", "", "D11", "1100.03.96", "ACC6", "研究与开发费" },
				new object[] { -1101.0000m, -301.0000m, "63000.97", "", "D12", "1200.03.97", "ACC7", "财务费用" },
				new object[] { 1101.0000m, 301.0000m, "53000.98", "", "D13", "1200.03.98", "ACC8", "其中：利息支出" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201012, "PNL");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { -1001.0000m, -101.0000m, "61000.95", "", "D11", "1100.03.95", "ACC5", "研究与开发费" },
				new object[] { -2001.0000m, -201.0000m, "61000.96", "", "D11", "1100.03.96", "ACC6", "研究与开发费" },
				new object[] { -4102.0000m, -301.0000m, "63000.97", "", "D12", "1200.03.97", "ACC7", "财务费用" },
				new object[] { 4102.0000m, 301.0000m, "53000.98", "", "D13", "1200.03.98", "ACC8", "其中：利息支出" }
			});
			AssertDataTables("it should be same", table, results);
		}

		public void TestChinaProfitAppropriationBreakdownByCategories()
		{
			var results = RunScript(200912, "PLA");
			AssertEquals("Result should have two row", 2, results.Rows.Count);
			var table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { -301.0000m, 0.0000m, "63000.97", "", "A03", "1200.03.97", "ACC7", "利润分配--盈余公积转入" },
				new object[] { 301.0000m, 0.0000m, "53000.98", "", "A04", "1200.03.98", "ACC8", "利润分配--提取法定盈余公积" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201001, "PLA");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { -101.0000m, 0.0000m, "61000.95", "", "A02", "1100.03.95", "ACC5", "未分配利润" },
				new object[] { -201.0000m, 0.0000m, "61000.96", "", "A02", "1100.03.96", "ACC6", "未分配利润" },
				new object[] { -1101.0000m, -301.0000m, "63000.97", "", "A03", "1200.03.97", "ACC7", "利润分配--盈余公积转入" },
				new object[] { 1101.0000m, 301.0000m, "53000.98", "", "A04", "1200.03.98", "ACC8", "利润分配--提取法定盈余公积" }
			});
			AssertDataTables("it should be same", table, results);

			results = RunScript(201012, "PLA");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
			table = createDataTable();
			AddRowIntoTable(table, new[]
			{
				new object[] { -101.0000m, 0.0000m, "61000.95", "", "A02", "1100.03.95", "ACC5", "未分配利润" },
				new object[] { -201.0000m, 0.0000m, "61000.96", "", "A02", "1100.03.96", "ACC6", "未分配利润" },
				new object[] { -4102.0000m, -301.0000m, "63000.97", "", "A03", "1200.03.97", "ACC7", "利润分配--盈余公积转入" },
				new object[] { 4102.0000m, 301.0000m, "53000.98", "", "A04", "1200.03.98", "ACC8", "利润分配--提取法定盈余公积" }
			});
			AssertDataTables("it should be same", table, results);
		}

		public void TestLastProcessedDate()
		{
			Helper.InsertBASGLAggregate(3, 201002, 1);
			Helper.InsertBASGLAggregate(3, 201002, 2);
			Helper.InsertBASGLAggregate(3, 201002, 3);
			Helper.InsertBASGLAggregate(3, 201002, 4);
			Helper.InsertBASGLAggregate(3, 201001, 5);
			Helper.InsertBASGLAggregate(3, 201001, 6);
			Helper.InsertBASGLAggregate(3, 201001, 7);
			Helper.InsertBASGLAggregate(3, 201001, 8);

			Helper.InsertStmData(new System.DateTime(2014, 4, 1));
			var results = RunScript(201001, "PNL");
			AssertEquals("Result should have four row", 4, results.Rows.Count);

			Helper.InsertStmData(new System.DateTime(2010, 12, 1));
			results = RunScript(201001, "PNL");
			AssertEquals("Result should have four row", 4, results.Rows.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTestData();
		}

		void PrepareTestData()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertPeriodForInputYear(2008);
			Helper.InsertPeriodForInputYear(2009);
			Helper.InsertPeriodForInputYear(2010);
			Helper.InsertStmData(new System.DateTime(2008, 1, 1));
			//BSH
			Helper.InsertGLAccount(1, "5300.03.95", "BSH", "DR", "AS");
			Helper.InsertGLAccount(2, "5300.03.96", "BSH", "DR", "AS");
			Helper.InsertGLAccount(3, "5300.03.97", "BSH", "DR", "AS");
			Helper.InsertGLAccount(4, "5300.03.98", "BSH", "CR", "AS");

			Helper.InsertAccountDescriptor(1, "DR", 1, "ZH-CN", "53000.95", "BSH", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(2, "DR", 2, "ZH-CN", "53000.96", "BSH", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(3, "DR", 3, "ZH-CN", "53000.97", "BSH", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(4, "CR", 4, "ZH-CN", "53000.98", "BSH", "COA", countryOfCompliance: "CN");

			Helper.InsertAccountDescriptor(21, "DR", 1, "ZH-CN", "53000.95", "D01", "BSH", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(22, "DR", 2, "ZH-CN", "53000.96", "D01", "BSH", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(23, "DR", 3, "ZH-CN", "53000.97", "D02", "BSH", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(24, "CR", 4, "ZH-CN", "53000.98", "D03", "BSH", countryOfCompliance: "CN");

			Helper.InsertAccountDescriptorPivot(1, 1);
			Helper.InsertAccountDescriptorPivot(2, 2);
			Helper.InsertAccountDescriptorPivot(3, 3);
			Helper.InsertAccountDescriptorPivot(4, 4);

			Helper.InsertAccountDescriptorPivot(21, 1);
			Helper.InsertAccountDescriptorPivot(22, 2);
			Helper.InsertAccountDescriptorPivot(23, 3);
			Helper.InsertAccountDescriptorPivot(24, 4);

			Helper.InsertGLAggregate(0, 200912, 1, gLAmountLocalBalance: 100M);
			Helper.InsertGLAggregate(0, 200912, 2, gLAmountLocalBalance: 200M);
			Helper.InsertGLAggregate(0, 200912, 3, gLAmountLocalBalance: 300M);
			Helper.InsertGLAggregate(0, 200912, 4, gLAmountLocalBalance: -300M);

			Helper.InsertGLAggregate(0, 201001, 3, gLAmountLocalBalance: 1100M);
			Helper.InsertGLAggregate(0, 201001, 4, gLAmountLocalBalance: -1100M);

			Helper.InsertGLAggregate(0, 201012, 1, gLAmountLocalBalance: 1000M);
			Helper.InsertGLAggregate(0, 201012, 2, gLAmountLocalBalance: 2000M);
			Helper.InsertGLAggregate(0, 201012, 3, gLAmountLocalBalance: 3000M);
			Helper.InsertGLAggregate(0, 201012, 4, gLAmountLocalBalance: -3000M);

			//PNL & PLM & PLA
			Helper.InsertGLAccount(5, "1100.03.95", "P&L", "DR", "TS");
			Helper.InsertGLAccount(6, "1100.03.96", "P&L", "DR", "TS");
			Helper.InsertGLAccount(7, "1200.03.97", "P&L", "DR", "TS");
			Helper.InsertGLAccount(8, "1200.03.98", "P&L", "CR", "TS");

			Helper.InsertAccountDescriptor(5, "DR", 5, "ZH-CN", "61000.95", "P&L", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(6, "DR", 6, "ZH-CN", "61000.96", "P&L", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(7, "DR", 7, "ZH-CN", "63000.97", "P&L", "COA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(8, "CR", 8, "ZH-CN", "53000.98", "P&L", "COA", countryOfCompliance: "CN");

			Helper.InsertAccountDescriptor(9, "DR", 5, "ZH-CN", "61000.95", "D11", "P&L", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(10, "DR", 6, "ZH-CN", "61000.96", "D11", "P&L", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(11, "DR", 7, "ZH-CN", "63000.97", "D12", "P&L", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(12, "CR", 8, "ZH-CN", "53000.98", "D13", "P&L", countryOfCompliance: "CN");

			Helper.InsertAccountDescriptor(13, "DR", 5, "ZH-CN", "61000.95", "A02", "PLA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(14, "DR", 6, "ZH-CN", "61000.96", "A02", "PLA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(15, "DR", 7, "ZH-CN", "63000.97", "A03", "PLA", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(16, "CR", 8, "ZH-CN", "53000.98", "A04", "PLA", countryOfCompliance: "CN");

			Helper.InsertAccountDescriptor(17, "DR", 5, "ZH-CN", "61000.95", "D14", "PLM", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(18, "DR", 6, "ZH-CN", "61000.96", "D14", "PLM", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(19, "DR", 7, "ZH-CN", "63000.97", "D15", "PLM", countryOfCompliance: "CN");
			Helper.InsertAccountDescriptor(20, "CR", 8, "ZH-CN", "53000.98", "D16", "PLM", countryOfCompliance: "CN");

			Helper.InsertAccountDescriptorPivot(5, 5);
			Helper.InsertAccountDescriptorPivot(6, 6);
			Helper.InsertAccountDescriptorPivot(7, 7);
			Helper.InsertAccountDescriptorPivot(8, 8);

			Helper.InsertAccountDescriptorPivot(9, 5);
			Helper.InsertAccountDescriptorPivot(10, 6);
			Helper.InsertAccountDescriptorPivot(11, 7);
			Helper.InsertAccountDescriptorPivot(12, 8);

			Helper.InsertAccountDescriptorPivot(13, 5);
			Helper.InsertAccountDescriptorPivot(14, 6);
			Helper.InsertAccountDescriptorPivot(15, 7);
			Helper.InsertAccountDescriptorPivot(16, 8);

			Helper.InsertAccountDescriptorPivot(17, 5);
			Helper.InsertAccountDescriptorPivot(18, 6);
			Helper.InsertAccountDescriptorPivot(19, 7);
			Helper.InsertAccountDescriptorPivot(20, 8);

			Helper.InsertGLAggregate(0, 200911, 5, gLAmountLocalBalance: 101M);
			Helper.InsertGLAggregate(0, 200911, 6, gLAmountLocalBalance: 201M);
			Helper.InsertGLAggregate(0, 200911, 7, gLAmountLocalBalance: 301M);
			Helper.InsertGLAggregate(0, 200911, 8, gLAmountLocalBalance: -301M);

			Helper.InsertGLAggregate(0, 201001, 7, gLAmountLocalBalance: 1101M);
			Helper.InsertGLAggregate(0, 201001, 8, gLAmountLocalBalance: -1101M);

			Helper.InsertGLAggregate(0, 201011, 5, gLAmountLocalBalance: 1001M);
			Helper.InsertGLAggregate(0, 201011, 6, gLAmountLocalBalance: 2001M);
			Helper.InsertGLAggregate(0, 201011, 7, gLAmountLocalBalance: 3001M);
			Helper.InsertGLAggregate(0, 201011, 8, gLAmountLocalBalance: -3001M);
		}

		DataTable RunScript(int period, string reportType)
		{
			var table = DataUtils.GetDataTableFromQuery(Db.Connection,
				$@"EXEC [{ScriptDbName}].[dbo].ChinaCommonReportsBreakdownByCategories '{reportType}', {period}, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '{GetReportCategoriesFromCode(reportType)}', 'AU1', 'CBH', 'ZH-CN', 'CN'");

			return table;
		}

		void AddRowIntoTable(DataTable table, object[] values)
		{
			values.ForEach(value =>
			{
				var row = table.NewRow();
				row.ItemArray = (object[])value;
				table.Rows.Add(row);
			});
		}

		DataTable createDataTable()
		{
			var dataTable = new DataTable();
			dataTable.Columns.Add("CurrentAmount", typeof(decimal));
			dataTable.Columns.Add("LastYearEndAmount", typeof(decimal));
			dataTable.Columns.Add("LocalAccountNumber", typeof(string));
			dataTable.Columns.Add("AccountDescription", typeof(string));
			dataTable.Columns.Add("Category", typeof(string));
			dataTable.Columns.Add("AccountNumber", typeof(string));
			dataTable.Columns.Add("AccountName", typeof(string));
			dataTable.Columns.Add("CategoryDescription", typeof(string));
			return dataTable;
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

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

