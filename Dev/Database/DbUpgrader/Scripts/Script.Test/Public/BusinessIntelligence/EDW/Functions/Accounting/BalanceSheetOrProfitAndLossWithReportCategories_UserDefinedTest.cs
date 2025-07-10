using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(BalanceSheetOrProfitAndLossWithReportCategories_UserDefined))]
	class BalanceSheetOrProfitAndLossWithReportCategories_UserDefinedTest : BiCreateScriptTest
	{
		public void TestBalanceSheetOrProfitAndLoss_UserDefinedReportType()
		{
			var results = RunScript(200912, "BSS", "BSS");
			AssertEquals("Should not return row", 0, results.Rows.Count);

			results = RunScript(200912, "BSH", "BBB");
			AssertEquals("Should not return row", 0, results.Rows.Count);

			TestConnection.ExecuteNonQuery($@"
				INSERT [{ScriptDbName}].[Finance].[BAS__StmDataDate]
					([StmDataDateKey], [StmDataDateID], [Name], [CompanyID], [Value], [CompanyKey])
				VALUES
					(1, newid(), 'JournalEntriesLastProcessedDate', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '2009-01-01 00:00:00.000', 1)");

			results = RunScript(200912, "BSH", "BBB");
			var row = results.Rows.Find("200912");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", 300m, row["D01"]);
			AssertEquals("TOTAL D02", 300m, row["D02"]);
			AssertEquals("TOTAL D03", 300m, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(201001, "BSH", "BBB");
			row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", 300m, row["D01"]);
			AssertEquals("TOTAL D02", 1400m, row["D02"]);
			AssertEquals("TOTAL D03", 1400m, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13"]);
			AssertEquals("TOTAL D01", 300m, row["D01_YED"]);
			AssertEquals("TOTAL D02", 300m, row["D02_YED"]);
			AssertEquals("TOTAL D03", 300m, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(201012, "BSH", "BBB");
			row = results.Rows.Find("201012");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", 3300m, row["D01"]);
			AssertEquals("TOTAL D02", 4400m, row["D02"]);
			AssertEquals("TOTAL D03", 4400m, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13"]);
			AssertEquals("TOTAL D01", 300m, row["D01_YED"]);
			AssertEquals("TOTAL D02", 300m, row["D02_YED"]);
			AssertEquals("TOTAL D03", 300m, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(200912, "PNL", "AAA");
			row = results.Rows.Find("200912");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", -302m, row["D11"]);
			AssertEquals("TOTAL D12", -301m, row["D12"]);
			AssertEquals("TOTAL D13", 301m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", DBNull.Value, row["D12_YED"]);
			AssertEquals("TOTAL D13", DBNull.Value, row["D13_YED"]);

			results = RunScript(201001, "PNL", "AAA");
			row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", -1101m, row["D12"]);
			AssertEquals("TOTAL D13", 1101m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", -302m, row["D11_YED"]);
			AssertEquals("TOTAL D12", -301m, row["D12_YED"]);
			AssertEquals("TOTAL D13", 301m, row["D13_YED"]);

			results = RunScript(201012, "PNL", "AAA");
			row = results.Rows.Find("201012");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", -3002m, row["D11"]);
			AssertEquals("TOTAL D12", -4102m, row["D12"]);
			AssertEquals("TOTAL D13", 4102m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", -302m, row["D11_YED"]);
			AssertEquals("TOTAL D12", -301m, row["D12_YED"]);
			AssertEquals("TOTAL D13", 301m, row["D13_YED"]);

			results = RunScript(201001, "PLM", "AAA");
			row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11"]);
			AssertEquals("TOTAL D12", -1101m, row["D12"]);
			AssertEquals("TOTAL D13", 1101m, row["D13"]);
			AssertEquals("TOTAL D01", DBNull.Value, row["D01_YED"]);
			AssertEquals("TOTAL D02", DBNull.Value, row["D02_YED"]);
			AssertEquals("TOTAL D03", DBNull.Value, row["D03_YED"]);
			AssertEquals("TOTAL D11", DBNull.Value, row["D11_YED"]);
			AssertEquals("TOTAL D12", -1101m, row["D12_YED"]);
			AssertEquals("TOTAL D13", 1101m, row["D13_YED"]);
		}

		public void TestLastProcessedDate()
		{
			Helper.InsertBASAggregate(3, 201001, 10M);
			Helper.InsertBASAggregate(4, 201002, 10M);
			var results = RunScript(201001, "PLM", "AAA");
			var row = results.Rows.Find("201001");
			AssertNotNull("Should find row", row);
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
		}

		void PrepareData()
		{
			TestConnection.ExecuteNonQuery($@"INSERT [{ScriptDbName}].[Organization].[BAS__Company]([CompanyKey], [CompanyID], [LocalCurrency], [CompanyCode], [IsGSTCashBasis], [IsGSTRegistered] )VALUES(1, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'CNY', 'DCN', 1, 1);
				INSERT[{ScriptDbName}].[Organization].[BAS__Branch]([BranchKey], [BranchID], [CompanyKey], [BranchCode])VALUES(1, newid(), 1, 'CBH')
				INSERT[{ScriptDbName}].[Organization].[BAS__Department]([DepartmentKey], [DepartmentID], [Code])VALUES(1, newid(), 'CN1')");

			for (var i = 0; i < 12; i++)
			{
				Helper.InsertPeriodManagement(3 * i + 1, 2008, i + 1);
				Helper.InsertPeriodManagement(3 * i + 2, 2009, i + 1);
				Helper.InsertPeriodManagement(3 * i + 3, 2010, i + 1);
			}

			Helper.InsertGLAccount(1, "1100.03.95", "P&L", "DR");
			Helper.InsertGLAccount(2, "1100.03.96", "P&L", "DR");

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 1, gLAccountKey: 1, localAccountNumber: "61000.95", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "P&L", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, gLAccountKey: 1, localAccountNumber: "61000.95", language: "ZH-CN", countryOfCompliance: "CN", reportType: "AAA", reportCategory: "D11", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 3, gLAccountKey: 2, localAccountNumber: "61000.96", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "P&L", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 4, gLAccountKey: 2, localAccountNumber: "61000.96", language: "ZH-CN", countryOfCompliance: "CN", reportType: "AAA", reportCategory: "D11", debitCredit: "DR");

			Helper.InsertAccountDescriptorPivot(1, 1, 1);
			Helper.InsertAccountDescriptorPivot(2, 1, 2);
			Helper.InsertAccountDescriptorPivot(3, 2, 3);
			Helper.InsertAccountDescriptorPivot(4, 2, 4);

			Helper.InsertGLAggregate(1001m, 201011, 1, "", 1001m);
			Helper.InsertGLAggregate(2001m, 201011, 2, "", 2001m);

			Helper.InsertGLAccount(3, "1100.03.97", "P&L", "DR");
			Helper.InsertGLAccount(4, "1100.03.98", "P&L", "CR");

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 5, gLAccountKey: 3, localAccountNumber: "61000.97", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "P&L", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 6, gLAccountKey: 3, localAccountNumber: "61000.97", language: "ZH-CN", countryOfCompliance: "CN", reportType: "AAA", reportCategory: "D12", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 7, gLAccountKey: 4, localAccountNumber: "61000.96", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "P&L", debitCredit: "CR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 8, gLAccountKey: 4, localAccountNumber: "61000.96", language: "ZH-CN", countryOfCompliance: "CN", reportType: "AAA", reportCategory: "D13", debitCredit: "CR");

			Helper.InsertAccountDescriptorPivot(5, 3, 5);
			Helper.InsertAccountDescriptorPivot(6, 3, 6);
			Helper.InsertAccountDescriptorPivot(7, 4, 7);
			Helper.InsertAccountDescriptorPivot(8, 4, 8);

			Helper.InsertGLAggregate(3001m, 201011, 3, "", 3001m);
			Helper.InsertGLAggregate(-3001m, 201011, 4, "", -3001m);
			Helper.InsertGLAggregate(1101m, 201001, 3, "", 1101m);
			Helper.InsertGLAggregate(-1101m, 201001, 4, "", -1101m);

			Helper.InsertGLAggregate(101m, 200911, 1, "", 101m);
			Helper.InsertGLAggregate(201m, 200911, 2, "", 201m);
			Helper.InsertGLAggregate(301m, 200911, 3, "", 301m);
			Helper.InsertGLAggregate(-301m, 200911, 4, "", -301m);

			Helper.InsertGLAccount(5, "5300.03.95", "BSH", "DR");
			Helper.InsertGLAccount(6, "5300.03.96", "BSH", "DR");

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 9, gLAccountKey: 5, localAccountNumber: "53000.95", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "BSH", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 10, gLAccountKey: 5, localAccountNumber: "53000.95", language: "ZH-CN", countryOfCompliance: "CN", reportType: "BBB", reportCategory: "D01", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 11, gLAccountKey: 6, localAccountNumber: "53000.96", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "BSH", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 12, gLAccountKey: 6, localAccountNumber: "53000.96", language: "ZH-CN", countryOfCompliance: "CN", reportType: "BBB", reportCategory: "D01", debitCredit: "DR");

			Helper.InsertAccountDescriptorPivot(9, 5, 9);
			Helper.InsertAccountDescriptorPivot(10, 5, 10);
			Helper.InsertAccountDescriptorPivot(11, 6, 11);
			Helper.InsertAccountDescriptorPivot(12, 6, 12);

			Helper.InsertGLAggregate(1000m, 201012, 5, "", 1000m);
			Helper.InsertGLAggregate(2000m, 201012, 6, "", 2000m);

			Helper.InsertGLAccount(7, "5300.03.97", "BSH", "DR");
			Helper.InsertGLAccount(8, "5300.03.98", "BSH", "CR");

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 13, gLAccountKey: 7, localAccountNumber: "53000.97", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "BSH", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 14, gLAccountKey: 7, localAccountNumber: "53000.97", language: "ZH-CN", countryOfCompliance: "CN", reportType: "BBB", reportCategory: "D02", debitCredit: "DR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 15, gLAccountKey: 8, localAccountNumber: "53000.98", language: "ZH-CN", countryOfCompliance: "CN", reportType: "COA", reportCategory: "BSH", debitCredit: "CR");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 16, gLAccountKey: 8, localAccountNumber: "53000.98", language: "ZH-CN", countryOfCompliance: "CN", reportType: "BBB", reportCategory: "D03", debitCredit: "CR");

			Helper.InsertAccountDescriptorPivot(13, 7, 13);
			Helper.InsertAccountDescriptorPivot(14, 7, 14);
			Helper.InsertAccountDescriptorPivot(15, 8, 15);
			Helper.InsertAccountDescriptorPivot(16, 8, 16);

			Helper.InsertGLAggregate(3000m, 201012, 7, "", 3000m);
			Helper.InsertGLAggregate(-3000m, 201012, 8, "", -3000m);
			Helper.InsertGLAggregate(1100m, 201001, 7, "", 1100m);
			Helper.InsertGLAggregate(-1100m, 201001, 8, "", -1100m);

			Helper.InsertGLAggregate(100m, 200912, 5, "", 100m);
			Helper.InsertGLAggregate(200m, 200912, 6, "", 200m);
			Helper.InsertGLAggregate(300m, 200912, 7, "", 300m);
			Helper.InsertGLAggregate(-300m, 200912, 8, "", -300m);
		}

		DataTable RunScript(int period, string repotType, string accountType)
		{
			var table = DataUtils.GetDataTableFromQuery(Db.Connection,
									string.Format($@"EXEC [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}] '{repotType}','{accountType}',{period},'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC','','CBH','D01, D02, D03, D11, D12, D13, D01_YED, D02_YED, D03_YED, D11_YED, D12_YED, D13_YED','ZH-CN','CN'")
								);

			table.PrimaryKey = new[] { table.Columns["Period"] };

			return table;
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
