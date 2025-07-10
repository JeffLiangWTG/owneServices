
using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ProfitAndLossReportTest : ScriptTest
	{
		[ExpectNoExceptions]
		public void TestLongBranchNameDoesNotCrashReport()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var company = testObjectCreator.CreateNewCompany("TSC", "AU");
			var branch = testObjectCreator.CreateBranch("TSB", "1234567890123456789012345678901234567890", company);

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "TS", "ACCRUAL - JOB COSTING", "BSH", Core.Constants.DebitCredit.Debit);
				var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
				AccGLAggregate aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader1.PK, branch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				GLBudget budget1 = testObjectCreator.CreateAccGLBudget(ZDateTime.Now.Year, 0m, 0m, glHeader1.PK, branch.PK, GlbDepartment.CurrentDepartment.PK, "PRD", 100m, 0m);
				Factory.Save();

				DataTable results = RunScriptWithoutSettingPrimaryKey("PNL", "", "Branch", "", "", "Y", "Y", "N", company.PK);
				DataRow[] rows = results.Select(string.Format("AccountPK = '{0}'", glHeader1.PK.ToString()));
				AssertEquals("duplicate detected!", 1, rows.Length);
			}
		}

		[ExpectNoExceptions]
		public void TestBranchManagementCodeFilter()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "TBS";
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			var glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "TS", "ACCRUAL - JOB COSTING", "BSH", Core.Constants.DebitCredit.Debit);
			var glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "TS", "ACCRUAL - JOB COSTING 2", "BSH", Core.Constants.DebitCredit.Debit);
			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			var aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			aggregate1.AA_GC = GlbCompany.CurrentCompany.PK;
			var aggregate2 = testObjectCreator.CreateAccGLAggregate(2000m, currentPeriod, glHeader2.PK, branch.PK, GlbDepartment.CurrentDepartment.PK, GlbDepartment.CurrentDepartment.PK, "");
			aggregate2.AA_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRE', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = 'BNE' And GB_GC = '{0}'", GlbCompany.CurrentCompany.PK));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'BRT', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = 'TBS' And GB_GC = '{0}'", GlbCompany.CurrentCompany.PK));

			var results = RunScriptWithoutSettingPrimaryKey("PNL", "", "Branch", "", "", "Y", "Y", "Y");
			DataRow[] rows = results.Select(string.Format("BranchCode = '{0}'", GlbBranch.CurrentBranch.GB_Code));
			AssertEquals("duplicate detected!", 1, rows.Length);

			rows = results.Select(string.Format("BranchCode = '{0}'", branch.GB_Code));
			AssertEquals("duplicate detected!", 1, rows.Length);

			results = RunScriptWithoutSettingPrimaryKey("PNL", "", "Branch", "", "BRE", "Y", "Y", "Y");
			rows = results.Select(string.Format("BranchCode = '{0}'", GlbBranch.CurrentBranch.GB_Code));
			AssertEquals("duplicate detected!", 1, rows.Length);

			rows = results.Select(string.Format("BranchCode = '{0}'", branch.GB_Code));
			AssertEquals("duplicate detected!", 0, rows.Length);

			results = RunScriptWithoutSettingPrimaryKey("PNL", "", "Branch", "", "BRT", "Y", "Y", "Y");
			rows = results.Select(string.Format("BranchCode = '{0}'", GlbBranch.CurrentBranch.GB_Code));
			AssertEquals("duplicate detected!", 0, rows.Length);

			rows = results.Select(string.Format("BranchCode = '{0}'", branch.GB_Code));
			AssertEquals("duplicate detected!", 1, rows.Length);
		}

		public void TestNoDuplicateRowsWhenIncludeBudgetAndGroupByDepartmentOrBranch()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "TS", "ACCRUAL - JOB COSTING", "BSH", Core.Constants.DebitCredit.Debit);
			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			AccGLAggregate aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			GLBudget budget1 = testObjectCreator.CreateAccGLBudget(ZDateTime.Now.Year, 0m, 0m, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, "PRD", 100m, 0m);
			Factory.Save();

			DataTable results = RunScriptWithoutSettingPrimaryKey("PNL", "", "Department", "", "", "Y", "Y", "N");
			DataRow[] rows = results.Select(string.Format("AccountPK = '{0}'", glHeader1.PK.ToString()));
			AssertEquals("duplicate detected!", 1, rows.Length);

			results = RunScriptWithoutSettingPrimaryKey("PNL", "", "Branch", "", "", "Y", "Y", "N");
			rows = results.Select(string.Format("AccountPK = '{0}'", glHeader1.PK.ToString()));
			AssertEquals("duplicate detected!", 1, rows.Length);
		}

		public void TestTotal()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "ACCRUAL - JOB COSTING", "BSH", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "WIP CONTROL", "BSH", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "TOTAL ASSETS", "TTL", Core.Constants.DebitCredit.Debit);
			glHeader1.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader2.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader3.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader3.AG_AccountType = "TTL";

			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			AccGLAggregate aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			AccGLAggregate aggregate2 = testObjectCreator.CreateAccGLAggregate(2000m, currentPeriod, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				DataTable results = RunScript();
				DataRow row = results.Rows.Find("5300.03.99");
				AssertNotNull("Should find row", row);
				AssertEquals("TOTAL ASSETS YearToPeriod", 3000m, row["YearToPeriod"]);
			}
		}

		public void TestHeaderTotalNotSuppressedWhenAccountsTotalEqualsZero()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.00", "AS", "HEADER", "HDR", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "ACCOUNT 1", "P&L", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "ACCOUNT 2", "P&L", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "ACCOUNT TOTAL", "TTL", Core.Constants.DebitCredit.Debit);

			glHeader1.AG_AG_HeaderDependsOnTotal = glHeader4.PK;

			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			AccGLAggregate aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			AccGLAggregate aggregate2 = testObjectCreator.CreateAccGLAggregate(-1000m, currentPeriod, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			DataTable results = RunScript();

			DataRow row = results.Rows.Find("5300.03.00");
			AssertNotNull("Should find row 5300.03.00 HEADER", row);

			row = results.Rows.Find("5300.03.95");
			AssertNotNull("Should find row 5300.03.95 ACCOUNT 1", row);

			row = results.Rows.Find("5300.03.96");
			AssertNotNull("Should find row 5300.03.96 ACCOUNT 2", row);

			row = results.Rows.Find("5300.03.99");
			AssertNotNull("Should find row 5300.03.99 ACCOUNT TOTAL", row);
			AssertEquals("TOTAL Should Be 0", 0m, row["CurrentPeriod"]);
		}

		public void TestHeaderTotalNotSuppressedWhenAccountsTotalNotEqualsZero()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.00", "AS", "HEADER", "HDR", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "ACCOUNT 1", "P&L", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "ACCOUNT 2", "P&L", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "ACCOUNT TOTAL", "TTL", Core.Constants.DebitCredit.Debit);

			glHeader1.AG_AG_HeaderDependsOnTotal = glHeader4.PK;

			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			AccGLAggregate aggregate1 = testObjectCreator.CreateAccGLAggregate(1010m, currentPeriod, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			AccGLAggregate aggregate2 = testObjectCreator.CreateAccGLAggregate(-1000m, currentPeriod, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			DataTable results = RunScript();

			DataRow row = results.Rows.Find("5300.03.00");
			AssertNotNull("Should find row 5300.03.00 HEADER", row);

			row = results.Rows.Find("5300.03.95");
			AssertNotNull("Should find row 5300.03.95 ACCOUNT 1", row);

			row = results.Rows.Find("5300.03.96");
			AssertNotNull("Should find row 5300.03.96 ACCOUNT 2", row);

			row = results.Rows.Find("5300.03.99");
			AssertNotNull("Should find row 5300.03.99 ACCOUNT TOTAL", row);
			AssertEquals("TOTAL Should Be 10", 10m, row["CurrentPeriod"]);
		}

		public void TestHeaderAndTotalSuppressedWhenAccountsBalanceZero()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.00", "AS", "HEADER", "HDR", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "ACCOUNT 1", "P&L", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "ACCOUNT 2", "P&L", Core.Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "ACCOUNT TOTAL", "TTL", Core.Constants.DebitCredit.Debit);

			glHeader1.AG_AG_HeaderDependsOnTotal = glHeader4.PK;

			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			AccGLAggregate aggregate1 = testObjectCreator.CreateAccGLAggregate(0m, currentPeriod, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			AccGLAggregate aggregate2 = testObjectCreator.CreateAccGLAggregate(0m, currentPeriod, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			DataTable results = RunScript();

			DataRow row = results.Rows.Find("5300.03.00");
			AssertNull("Should not find row 5300.03.00 HEADER", row);

			row = results.Rows.Find("5300.03.99");
			AssertNull("Should not find row 5300.03.99 ACCOUNT TOTAL", row);
		}

		public void TestGroupByBranchManagementCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "TB1";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "TB2";
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch3.GB_Code = "TB3";
			branch3.GB_GC = GlbCompany.CurrentCompany.PK;

			var glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "TS", "ACCRUAL - JOB COSTING", "BSH", Core.Constants.DebitCredit.Debit);
			var glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "TS", "ACCRUAL - JOB COSTING 2", "BSH", Core.Constants.DebitCredit.Debit);
			var glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.97", "TS", "ACCRUAL - JOB COSTING 3", "BSH", Core.Constants.DebitCredit.Debit);
			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			var aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader1.PK, branch1.PK, GlbDepartment.CurrentDepartment.PK, GlbDepartment.CurrentDepartment.PK, "");
			aggregate1.AA_GC = GlbCompany.CurrentCompany.PK;
			var aggregate2 = testObjectCreator.CreateAccGLAggregate(2000m, currentPeriod, glHeader2.PK, branch2.PK, GlbDepartment.CurrentDepartment.PK, GlbDepartment.CurrentDepartment.PK, "");
			aggregate2.AA_GC = GlbCompany.CurrentCompany.PK;
			var aggregate3 = testObjectCreator.CreateAccGLAggregate(3000m, currentPeriod, glHeader3.PK, branch3.PK, GlbDepartment.CurrentDepartment.PK, GlbDepartment.CurrentDepartment.PK, "");
			aggregate3.AA_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'MG1', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = 'TB1' And GB_GC = '{0}'", GlbCompany.CurrentCompany.PK));
			TestConnection.ExecuteNonQuery(string.Format("UPDATE dbo.GlbBranch SET GB_AccountingGroupCode = 'MG2', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_Code = 'TB2' And GB_GC = '{0}'", GlbCompany.CurrentCompany.PK));

			var table = RunScriptWithoutSettingPrimaryKey("PNL", "", "Mgt", "", "", "Y", "Y", "Y");
			AssertEquals(3, table.Rows.Count);

			DataRow[] rows = table.Select("BranchCode = 'MG1'");
			AssertEquals(1, rows.Length);
			rows = table.Select("BranchCode = 'MG2'");
			AssertEquals(1, rows.Length);

			table = RunScriptWithoutSettingPrimaryKey("PNL", "", "Mgt", "", "MG1", "Y", "Y", "Y");
			AssertEquals(1, table.Rows.Count);
			rows = table.Select("BranchCode = 'MG1'");
			AssertEquals(1, rows.Length);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				table = RunScriptWithoutSettingPrimaryKey("PNL", "", "Mgt", "", "MG2", "Y", "Y", "Y");
				AssertEquals(1, table.Rows.Count);
				rows = table.Select("BranchCode = 'MG2'");
				AssertEquals(1, rows.Length);
			}
		}

		#region TestTransactionCategory

		[TestDate(2011, 05, 20)]
		public void TestTransactionCategoryForReportTypeBSH()
		{
			SetUpTransactionCategoryTestData();

			DataTable results = RunScript("BSH");
			var headers = new[] { "AccountNumber", "CurrentPeriod", "YearToPeriod", "PeriodLastYear", "LastYearToPeriod", "TotalLastYear" };
			var lines = new object[][]
						{
							new object[] {	"5400.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"5899.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"6210.00.00",	100m,	100m,	0m,	0m,	0m	},
							new object[] {	"6999.00.00",	100m,	100m,	0m,	0m,	0m	},
							new object[] {	"7999.00.00",	100m,	100m,	0m,	0m,	0m	},
							new object[] {	"8210.00.00",	70m,	70m,	0m,	0m,	0m	},
							new object[] {	"8999.00.00",	70m,	70m,	0m,	0m,	0m	},
							new object[] {	"9799.00.00",	70m,	70m,	0m,	0m,	0m	},
							new object[] {	"9999.00.00",	-30m,	-30m,	0m,	0m,	0m	}
						};
			AssertDataTableSelectedRows("BSH", results, headers, lines);

			results = RunScript("BSH", "XYZ");
			AssertDataTableSelectedRows("BSH, XYZ", results, headers, lines);

			results = RunScript("BSH", "ABC");
			lines = new object[][]
						{
							new object[] {	"5400.00.00",	30m,	80m,	-50m,	-50m,	-50m	},
							new object[] {	"5899.00.00",	30m,	80m,	-50m,	-50m,	-50m	},
							new object[] {	"6210.00.00",	100m,	150m,	50m,	50m,	50m		},
							new object[] {	"6999.00.00",	100m,	150m,	50m,	50m,	50m		},
							new object[] {	"7999.00.00",	100m,	150m,	50m,	50m,	50m		},
							new object[] {	"8210.00.00",	70m,	70m,	0m,		0m,		0m		},
							new object[] {	"8999.00.00",	70m,	70m,	0m,		0m,		0m		},
							new object[] {	"9799.00.00",	70m,	70m,	0m,		0m,		0m		},
							new object[] {	"9999.00.00",	-30m,	-80m,	50m,	50m,	50m		}
						};
			AssertDataTableSelectedRows("BSH, ABC", results, headers, lines);

			headers = new[] { "CurrentPeriod", "YearToPeriod", "PeriodLastYear", "LastYearToPeriod", "TotalLastYear",
							"CurrentPeriodDebit", "YearToPeriodDebit", "PeriodLastYearDebit", "LastYearToPeriodDebit", "TotalLastYearDebit",
							"CurrentPeriodCredit", "YearToPeriodCredit", "PeriodLastYearCredit", "LastYearToPeriodCredit", "TotalLastYearCredit" };
			results = RunScript("BSH", "", "Branch");
			lines = new object[][]
						{
							new object[] {	0m,	70m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m,	0m,	70m,	0m,	0m,	0m	},
							new object[] {	0m,	100m,	0m,	0m,	0m,	0m,	100m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m	},
						};
			AssertDataTableAllRows("BSH, '', Branch", results, headers, lines);

			results = RunScript("BSH", "XYZ", "Branch");
			AssertDataTableAllRows("BSH, XYZ, Branch", results, headers, lines);

			results = RunScript("BSH", "", "Department");
			AssertDataTableAllRows("BSH, '', Department", results, headers, lines);

			results = RunScript("BSH", "XYZ", "Department");
			AssertDataTableAllRows("BSH, XYZ, Department", results, headers, lines);

			results = RunScript("BSH", "ABC", "Branch");
			lines = new object[][]
						{
							new object[] {	0m,	70m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m,	0m,	70m,	0m,	0m,	0m	},
							new object[] {	0m,	150m,	0m,	0m,	0m,	0m,	150m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m	},
						};
			AssertDataTableAllRows("BSH, ABC, Branch", results, headers, lines);

			results = RunScript("BSH", "ABC", "Department");
			AssertDataTableAllRows("BSH, ABC, Department", results, headers, lines);

			results = RunScript("BSH", "", "", Core.SharedConstants.Languages.ChineseSimplified);
			headers = new[] { "AccountNumber", "LastYearToPeriod", "LastYearToPeriodCredit", "LastYearToPeriodDebit",
											"CurrentPeriod", "CurrentDebit", "CurrentCredit",
											"CurrentYearMovement", "CurrentYearMovementCredit", "CurrentYearMovementDebit",
											"YearToPeriod", "ClosingDebit", "ClosingCredit" };
			lines = new object[][]
						{
							new object[] {	"5899",	0m,	0m,	0m,	30m,	0m,		30m,	0m,		0m,		0m,		30m,	0m,		30m		},
							new object[] {	"6210",	0m,	0m,	0m,	100m,	0m,		100m,	-100m,	0m,		100m,	100m,	0m,		100m	},
							new object[] {	"6999",	0m,	0m,	0m,	100m,	0m,		100m,	0m,		0m,		0m,		100m,	0m,		100m	},
							new object[] {	"7999",	0m,	0m,	0m,	100m,	0m,		100m,	0m,		0m,		0m,		100m,	0m,		100m	},
							new object[] {	"8210",	0m,	0m,	0m,	70m,	0m,		70m,	70m,	70m,	0m,		70m,	0m,		70m		},
							new object[] {	"8999",	0m,	0m,	0m,	70m,	0m,		70m,	0m,		0m,		0m,		70m,	0m,		70m		},
							new object[] {	"9799",	0m,	0m,	0m,	70m,	0m,		70m,	0m,		0m,		0m,		70m,	0m,		70m		},
							new object[] {	"9999",	0m,	0m,	0m,	-30m,	30m,	0m,		0m,		0m,		0m,		-30m,	30m,	0m		}
						};
			AssertDataTableSelectedRows("BSH, '', '', ZH-CN", results, headers, lines);

			results = RunScript("BSH", "XYZ", "", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, XYZ, '', ZH-CN", results, headers, lines);

			results = RunScript("BSH", "ABC", "", Core.SharedConstants.Languages.ChineseSimplified);
			lines = new object[][]
						{
							new object[] {	"5899",	-50m,	0m,		50m,	30m,	0m,		30m,	0m,		0m,		0m,		30m,	0m,		30m		},
							new object[] {	"6210",	50m,	50m,	0m,		100m,	0m,		100m,	-100m,	0m,		100m,	150m,	0m,		150m	},
							new object[] {	"6999",	50m,	50m,	0m,		100m,	0m,		100m,	0m,		0m,		0m,		150m,	0m,		150m	},
							new object[] {	"7999",	50m,	50m,	0m,		100m,	0m,		100m,	0m,		0m,		0m,		150m,	0m,		150m	},
							new object[] {	"8210",	0m,		0m,		0m,		70m,	0m,		70m,	70m,	70m,	0m,		70m,	0m,		70m		},
							new object[] {	"8999",	0m,		0m,		0m,		70m,	0m,		70m,	0m,		0m,		0m,		70m,	0m,		70m		},
							new object[] {	"9799",	0m,		0m,		0m,		70m,	0m,		70m,	0m,		0m,		0m,		70m,	0m,		70m		},
							new object[] {	"9999",	50m,	50m,	0m,		-30m,	30m,	0m,		0m,		0m,		0m,		-80m,	80m,	0m		}
						};
			AssertDataTableSelectedRows("BSH, ABC, '', ZH-CN", results, headers, lines);

			results = RunScript("BSH", "", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			lines = new object[][]
						{
							new object[] {	"5899",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"6210",	0m,	0m,	0m,	0m,	0m,	0m,	-100m,	0m,		100m,	100m,	0m,	100m	},
							new object[] {	"6999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"7999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"8210",	0m,	0m,	0m,	0m,	0m,	0m,	70m,	70m,	0m,		70m,	0m,	70m		},
							new object[] {	"8999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"9799",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"9999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		}
						};
			AssertDataTableSelectedRows("BSH, '', Branch, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, '', Department, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "XYZ", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, XYZ, Branch, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "XYZ", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, XYZ, Department, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "ABC", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			lines = new object[][]
						{
							new object[] {	"5899",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"6210",	0m,	0m,	0m,	0m,	0m,	0m,	-100m,	0m,		100m,	150m,	0m,	150m	},
							new object[] {	"6999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"7999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"8210",	0m,	0m,	0m,	0m,	0m,	0m,	70m,	70m,	0m,		70m,	0m,	70m		},
							new object[] {	"8999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"9799",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		},
							new object[] {	"9999",	0m,	0m,	0m,	0m,	0m,	0m,	0m,		0m,		0m,		0m,		0m,	0m		}
						};
			AssertDataTableSelectedRows("BSH, ABC, Branch, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "ABC", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, ABC, Department, ZH-CN", results, headers, lines);
		}

		[TestDate(2011, 05, 20)]
		public void TestTransactionCategoryForReportTypePNL()
		{
			SetUpTransactionCategoryTestData();

			DataTable results = RunScript("PNL");
			var headers = new[] { "AccountNumber", "CurrentPeriod", "YearToPeriod", "PeriodLastYear", "LastYearToPeriod", "TotalLastYear" };
			var lines = new object[][]
						{
							new object[] {	"1010.10.10",	100m,	100m,	0m,	0m,	0m	},
							new object[] {	"1010.20.10",	-70m,	-70m,	0m,	0m,	0m	},
							new object[] {	"1700.00.00",	100m,	100m,	0m,	0m,	0m	},
							new object[] {	"1800.00.00",	-70m,	-70m,	0m,	0m,	0m	},
							new object[] {	"1900.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"2999.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"4499.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"4599.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"4699.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"4799.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"4899.00.00",	30m,	30m,	0m,	0m,	0m	},
							new object[] {	"4999.00.00",	30m,	30m,	0m,	0m,	0m	}
						};

			AssertDataTableSelectedRows("PNL", results, headers, lines);

			results = RunScript("PNL", "XYZ");
			AssertDataTableSelectedRows("PNL, XYZ", results, headers, lines);

			results = RunScript("PNL", "ABC");
			lines = new object[][]
						{
							new object[] {	"1010.10.10",	100m,	100m,	50m,	50m,	50m	},
							new object[] {	"1010.20.10",	-70m,	-70m,	0m,		0m,		0m	},
							new object[] {	"1700.00.00",	100m,	100m,	50m,	50m,	50m	},
							new object[] {	"1800.00.00",	-70m,	-70m,	0m,		0m,		0m	},
							new object[] {	"1900.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"2999.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"4499.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"4599.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"4699.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"4799.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"4899.00.00",	30m,	30m,	50m,	50m,	50m	},
							new object[] {	"4900.00.00",	0m,		50m,	0m,		0m,		0m	},
							new object[] {	"4999.00.00",	30m,	80m,	50m,	50m,	50m	}
						};
			AssertDataTableSelectedRows("PNL, ABC", results, headers, lines);

			headers = new[] { "CurrentPeriod", "YearToPeriod", "PeriodLastYear", "LastYearToPeriod", "TotalLastYear",
							"CurrentPeriodDebit", "YearToPeriodDebit", "PeriodLastYearDebit", "LastYearToPeriodDebit", "TotalLastYearDebit",
							"CurrentPeriodCredit", "YearToPeriodCredit", "PeriodLastYearCredit", "LastYearToPeriodCredit", "TotalLastYearCredit" };
			results = RunScript("PNL", "", "Branch");
			lines = new object[][]
						{
							new object[] { 30m, 30m, 0m, 0m, 0m, -70m, -70m, 0m, 0m, 0m, 100m, 100m, 0m, 0m, 0m },
						};
			AssertDataTableAllRows("PNL, '', Branch", results, headers, lines);

			results = RunScript("PNL", "XYZ", "Branch");
			AssertDataTableAllRows("PNL, XYZ, Branch", results, headers, lines);

			results = RunScript("PNL", "", "Department");
			AssertDataTableAllRows("PNL, '', Department", results, headers, lines);

			results = RunScript("PNL", "XYZ", "Department");
			AssertDataTableAllRows("PNL, XYZ, Department", results, headers, lines);

			results = RunScript("PNL", "ABC", "Branch");
			lines = new object[][]
						{
							new object[] { 30m, 30m, 50m, 50m, 50m, -70m, -70m, 0m, 0m, 0m, 100m, 100m, 50m, 50m, 50m },
						};
			AssertDataTableAllRows("PNL, ABC, Branch", results, headers, lines);

			results = RunScript("PNL", "ABC", "Department");
			AssertDataTableAllRows("PNL, ABC, Department", results, headers, lines);

			results = RunScript("BSH", "", "", Core.SharedConstants.Languages.ChineseSimplified);
			headers = new[] { "AccountNumber", "LastYearToPeriod", "LastYearToPeriodCredit", "LastYearToPeriodDebit",
											"CurrentPeriod", "CurrentDebit", "CurrentCredit",
											"CurrentYearMovement", "CurrentYearMovementCredit", "CurrentYearMovementDebit",
											"YearToPeriod", "ClosingDebit", "ClosingCredit" };
			lines = new object[][]
						{
							new object[] {	"5899",	0m,	0m,	0m,	  30m,	 0m,	 30m,	   0m,	 0m,	  0m,	 30m,	 0m,	 30m	},
							new object[] {	"6210",	0m,	0m,	0m,	 100m,	 0m,	100m,	-100m,	 0m,	100m,	100m,	 0m,	100m	},
							new object[] {	"6999",	0m,	0m,	0m,	 100m,	 0m,	100m,	   0m,	 0m,	  0m,	100m,	 0m,	100m	},
							new object[] {	"7999",	0m,	0m,	0m,	 100m,	 0m,	100m,	   0m,	 0m,	  0m,	100m,	 0m,	100m	},
							new object[] {	"8210",	0m,	0m,	0m,	  70m,	 0m,	 70m,	  70m,	70m,	  0m,	 70m,	 0m,	 70m	},
							new object[] {	"8999",	0m,	0m,	0m,	  70m,	 0m,	 70m,	   0m,	 0m,	  0m,	 70m,	 0m,	 70m	},
							new object[] {	"9799",	0m,	0m,	0m,	  70m,	 0m,	 70m,	   0m,	 0m,	  0m,	 70m,	 0m,	 70m	},
							new object[] {	"9999",	0m,	0m,	0m,	 -30m,	30m,	  0m,	   0m,	 0m,	  0m,	-30m,	30m,	  0m	}
						};
			AssertDataTableSelectedRows("BSH, '', '', ZH-CN", results, headers, lines);

			results = RunScript("BSH", "XYZ", "", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, XYZ, '', ZH-CN", results, headers, lines);

			results = RunScript("BSH", "ABC", "", Core.SharedConstants.Languages.ChineseSimplified);
			lines = new object[][]
						{
							new object[] {	"5899",	-50m,	 0m,	50m,	 30m,	 0m,	 30m,	   0m,	 0m,	  0m,	 30m,	 0m,	 30m	},
							new object[] {	"6210",	 50m,	50m,	 0m,	100m,	 0m,	100m,	-100m,	 0m,	100m,	150m,	 0m,	150m	},
							new object[] {	"6999",	 50m,	50m,	 0m,	100m,	 0m,	100m,	   0m,	 0m,	  0m,	150m,	 0m,	150m	},
							new object[] {	"7999",	 50m,	50m,	 0m,	100m,	 0m,	100m,	   0m,	 0m,	  0m,	150m,	 0m,	150m	},
							new object[] {	"8210",	  0m,	 0m,	 0m,	 70m,	 0m,	 70m,	  70m,	70m,	  0m,	 70m,	 0m,	 70m	},
							new object[] {	"8999",	  0m,	 0m,	 0m,	 70m,	 0m,	 70m,	   0m,	 0m,	  0m,	 70m,	 0m,	 70m	},
							new object[] {	"9799",	  0m,	 0m,	 0m,	 70m,	 0m,	 70m,	   0m,	 0m,	  0m,	 70m,	 0m,	 70m	},
							new object[] {	"9999",	 50m,	50m,	 0m,	-30m,	30m,	  0m,	   0m,	 0m,	  0m,	-80m,	80m,	  0m	}
						};
			AssertDataTableSelectedRows("BSH, ABC, '', ZH-CN", results, headers, lines);

			results = RunScript("BSH", "", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			lines = new object[][]
						{
							new object[] {	"6210",	0m,	0m,	0m,	0m,	0m,	0m,	-100m,	 0m,	100m,	100m,	0m,	100m	},
							new object[] {	"8210",	0m,	0m,	0m,	0m,	0m,	0m,	  70m,	70m,	  0m,	 70m,	0m,	 70m	}
						};
			AssertDataTableSelectedRows("BSH, '', Branch, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, '', Department, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "XYZ", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, XYZ, Branch, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "XYZ", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, XYZ, Department, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "ABC", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			lines = new object[][]
						{
							new object[] {	"6210",	0m,	0m,	0m,	0m,	0m,	0m,	-100m,	 0m,	100m,	150m,	0m,	150m	},
							new object[] {	"8210",	0m,	0m,	0m,	0m,	0m,	0m,	  70m,	70m,	  0m,	 70m,	0m,	 70m	}
						};
			AssertDataTableSelectedRows("BSH, ABC, Branch, ZH-CN", results, headers, lines);

			results = RunScript("BSH", "ABC", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableSelectedRows("BSH, ABC, Department, ZH-CN", results, headers, lines);
		}

		[TestDate(2011, 05, 20)]
		public void TestTransactionCategoryForReportTypeTBS()
		{
			SetUpTransactionCategoryTestData();

			DataTable results = RunScript("TBS");
			var headers = new[] { "AccountNumber", "CurrentPeriod", "YearToPeriod", "PeriodLastYear", "LastYearToPeriod", "TotalLastYear", "CurrentDebit", "CurrentCredit", "ClosingDebit", "ClosingCredit" };
			var lines = new object[][]
						{
							new object[] {	"1010.10.10",	100m,	100m,	0m,	0m,	0m,	0m,		100m,	0m,		100m	},
							new object[] {	"1010.20.10",	-70m,	-70m,	0m,	0m,	0m,	70m,	0m,		70m,	0m		},
							new object[] {	"6210.00.00",	-100m,	-100m,	0m,	0m,	0m,	100m,	0m,		100m,	0m		},
							new object[] {	"8210.00.00",	70m,	70m,	0m,	0m,	0m,	0m,		70m,	0m,		70m		},
						};
			AssertDataTableAllRows("TBS", results, headers, lines);

			results = RunScript("TBS", "XYZ");
			AssertDataTableAllRows("TBS, XYZ", results, headers, lines);

			results = RunScript("TBS", "ABC");
			lines = new object[][]
						{
							new object[] {	"1010.10.10",	100m,	100m,	50m,	50m,	-50m,	0m,		100m,	0m,		100m	},
							new object[] {	"1010.20.10",	-70m,	-70m,	0m,		0m,		0m,		70m,	0m,		70m,	0m		},
							new object[] {	"4900.00.00",	0m,		50m,	0m,		0m,		0m,		0m,		0m,		0m,		50m		},
							new object[] {	"6210.00.00",	-100m,	-150m,	50m,	50m,	50m,	100m,	0m,		150m,	0m		},
							new object[] {	"8210.00.00",	70m,	70m,	0m,		0m,		0m,		0m,		70m,	0m,		70m		},
						};
			AssertDataTableAllRows("TBS, ABC", results, headers, lines);

			headers = new[] { "CurrentPeriod", "YearToPeriod", "PeriodLastYear", "LastYearToPeriod", "TotalLastYear",
							"CurrentPeriodDebit", "YearToPeriodDebit", "PeriodLastYearDebit", "LastYearToPeriodDebit", "TotalLastYearDebit",
							"CurrentPeriodCredit", "YearToPeriodCredit", "PeriodLastYearCredit", "LastYearToPeriodCredit", "TotalLastYearCredit" };
			results = RunScript("TBS", "", "Branch");
			lines = new object[][]
						{
							new object[] {	0m,	170m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m,	0m,	170m,	0m,	0m,	0m	},
							new object[] {	0m,	-170m,	0m,	0m,	0m,	0m,	-170m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m	}
						};
			AssertDataTableAllRows("TBS, '', Branch", results, headers, lines);

			results = RunScript("TBS", "", "Department");
			AssertDataTableAllRows("TBS, '', Department", results, headers, lines);

			results = RunScript("TBS", "XYZ", "Branch");
			AssertDataTableAllRows("TBS, XYZ, Branch", results, headers, lines);

			results = RunScript("TBS", "XYZ", "Department");
			AssertDataTableAllRows("TBS, XYZ, Department", results, headers, lines);

			results = RunScript("TBS", "ABC", "Branch");
			lines = new object[][]
						{
							new object[] {	0m,	170m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m,	0m,	170m,	0m,	0m,	0m	},
							new object[] {	0m,	-220m,	0m,	0m,	0m,	0m,	-220m,	0m,	0m,	0m,	0m,	0m,		0m,	0m,	0m	}
						};
			AssertDataTableAllRows("TBS, ABC, Branch", results, headers, lines);

			results = RunScript("TBS", "ABC", "Department");
			AssertDataTableAllRows("TBS, ABC, Department", results, headers, lines);

			results = RunScript("TBS", "", "", Core.SharedConstants.Languages.ChineseSimplified);
			headers = Array.Empty<string>();
			lines = Array.Empty<object[]>();
			AssertDataTableAllRows("TBS, '', '', ZH-CN", results, headers, lines);

			results = RunScript("TBS", "XYZ", "", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, XYZ, '', ZH-CN", results, headers, lines);

			results = RunScript("TBS", "ABC", "", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, ABC, '', ZH-CN", results, headers, lines);

			results = RunScript("TBS", "", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, '', Branch, ZH-CN", results, headers, lines);

			results = RunScript("TBS", "", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, '', Department, ZH-CN", results, headers, lines);

			results = RunScript("TBS", "XYZ", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, XYZ, Branch, ZH-CN", results, headers, lines);

			results = RunScript("TBS", "XYZ", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, XYZ, Department, ZH-CN", results, headers, lines);

			results = RunScript("TBS", "ABC", "Branch", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, ABC, Branch, ZH-CN", results, headers, lines);

			results = RunScript("TBS", "ABC", "Department", Core.SharedConstants.Languages.ChineseSimplified);
			AssertDataTableAllRows("TBS, ABC, Department, ZH-CN", results, headers, lines);
		}

		void SetUpTransactionCategoryTestData()
		{
			new PeriodManagement.PeriodManager(Factory).Periods.RemoveAndDeleteAll();
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year - 1, 1, 1));
			TestObjectCreator.CreateTestPeriods(new ZDateTime(ZDateTime.Today.Year, 1, 1));

			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);

			Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "BOND")).Delete();
			Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CLAIM")).Delete();

			var glHeader1 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1010.10.10"));
			var glHeader2 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1010.20.10"));
			var glHeader3 = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6210.00.00"));
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR INV", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.ABIGAS, glHeader1.PK);
			var apInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP INV", TestObjectCreator.AUD, 1M, 70M, 0M, 70M, 0M, TestObjectCreator.AALSHI, glHeader2.PK);
			ZDateTime glJournalPostDate = ZDateTime.Today.AddYears(-1);
			var glJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, ZDateTime.Today, glJournalPostDate, null);
			glJournal.AH_TransactionCategory = "ABC";
			TestObjectCreator.CreateGLJournalLine(glJournal, 50M, DebitCredit.CR, glHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glJournal, 50M, DebitCredit.DR, glHeader3.PK);
			AggregateWrapper aggregator = new AggregateWrapper(glJournal, glJournal);
			BusinessObjectFactory.SaveTogether(Factory, aggregator);

			bool aggregationResult = new BatchAggregator().Aggregate();
			Assert("Precondition: Agregation should be run successfully.", aggregationResult);
			Factory.Save();
		}

		#endregion

		public void TestIndexOnTemporaryTable()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "ACCRUAL - JOB COSTING", "BSH", Core.Constants.DebitCredit.Debit);
			var glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "WIP CONTROL", "BSH", Core.Constants.DebitCredit.Debit);
			var glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "TOTAL ASSETS", "TTL", Core.Constants.DebitCredit.Debit);
			glHeader1.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader2.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader3.AG_DebitCredit = Core.Constants.DebitCredit.Debit;
			glHeader3.AG_AccountType = "TTL";

			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			var aggregate1 = testObjectCreator.CreateAccGLAggregate(1000m, currentPeriod, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			var aggregate2 = testObjectCreator.CreateAccGLAggregate(2000m, currentPeriod, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			Factory.Save();

			var sql = GetFormatedScript(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now));

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				TestConnection.ExecuteReader(sql, cmd => { });

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First();
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First(t => t.Contains("FROM #PL")));

				var indexName = "AccountPKAndBranchPKAndDepartmentPK";
				Assert("Index is hit: ", queryPlanAnalyzer.IndexSeeks.Any(x => x.IndexName == indexName) || queryPlanAnalyzer.IndexScans.Any(x => x.IndexName == indexName));
			}
		}

		DataTable RunScript(string reportType = "BSH", string transactionCategory = "", string tsGroupBy = "", string language = "", string branchManagementCode = "")
		{
			DataTable table = RunScriptWithoutSettingPrimaryKey(reportType, transactionCategory, tsGroupBy, language,  branchManagementCode);
			table.PrimaryKey = new DataColumn[] { table.Columns["AccountNumber"] };
			return table;
		}

		DataTable RunScriptWithoutSettingPrimaryKey(string reportType = "BSH", string transactionCategory = "", string tsGroupBy = "", string language = "", string branchManagementCode = "",
													string includeBudget = "", string includeZeroBal = "", string splitDebitCredit = "Y", ZGuid companyPK = new ZGuid())
		{
			var script = GetFormatedScript(PeriodCalculator.GetPeriodFromDate(ZDateTime.Now), reportType, transactionCategory, tsGroupBy, language, branchManagementCode, includeBudget, includeZeroBal, splitDebitCredit, companyPK.IsValid ? companyPK : GlbCompany.CurrentCompany.PK);
			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection, script);

			return table;
		}

		string GetFormatedScript(int period, string reportType = "BSH", string transactionCategory = "", string tsGroupBy = "", string language = "", string branchManagementCode = "",
													string includeBudget = "", string includeZeroBal = "", string splitDebitCredit = "Y", ZGuid companyPK = new ZGuid())
		{
			return string.Format(@"
DECLARE
	@Period INT, 
	@CompanyPK UNIQUEIDENTIFIER, 
	@DepartmentList AS VARCHAR(8000), 
	@BranchList AS VARCHAR(8000), 
	@InclZeroBal CHAR(1), 
	@SummaryType CHAR(20), 
	@IncludeBudget CHAR(1), 
	@ReportType CHAR(3), 
	@Language CHAR(7), 
	@MultiLanguageBSHStartAccount VARCHAR(10), 
	@BudgetOnly CHAR(1), 
	@RollUp CHAR(1),
	@TSGroupBy VARCHAR(10),
	@Nature CHAR(1),
	@TransactionCategory VARCHAR(100),
	@BranchManagementCode VARCHAR(3),
	@SplitDebitCredit char(1) = 'Y'

SET @Period={0}
SET @CompanyPK='{1}'
SET @DepartmentList = ''
SET @BranchList = ''
SET @InclZeroBal= '{7}'
SET @SummaryType = ''
SET @IncludeBudget = '{6}'
SET @RollUp=N'0'
SET @Language = '{2}'
SET @MultiLanguageBSHStartAccount = ''
SET @BudgetOnly = ''
SET	@TSGroupBy = '{3}'
SET	@Nature = ''
SET @TransactionCategory='{4}'
SET @ReportType = '{5}'
Set @SplitDebitCredit = '{8}'
SET @BranchManagementCode = '{9}'

EXEC ProfitAndLossReport
	@Period,
	@CompanyPK,
	@DepartmentList,
	@BranchList,
	@InclZeroBal,
	@SummaryType,
	@IncludeBudget,
	@ReportType,
	@Language,
	@MultiLanguageBSHStartAccount,
	@BudgetOnly,
	@RollUp,
	@TSGroupBy,
	@Nature,
	@TransactionCategory,
	@BranchManagementCode,
	@SplitDebitCredit
",
			period,
			companyPK.IsValid ? companyPK : GlbCompany.CurrentCompany.PK,
			language,
			tsGroupBy,
			transactionCategory,
			reportType,
			includeBudget,
			includeZeroBal,
			splitDebitCredit,
			branchManagementCode
			);
		}
	}
}


