using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ProfitAndLossReportPeriodAnalysisTest : ScriptTest
	{
		[TestDate(2011, 06, 20)]
		public void TestColumnsForEachPeriod()
		{
			int baseColumnsCount = 7;
			int expectedPeriodCount = 12;
			var periodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2011, 1, 1));
			AssertEquals("Precondition: Periods.Count", expectedPeriodCount, periodManager.Periods.Count);
			Factory.Save();

			DataTable resultForSettlementGroup = RunScript(false);
			AssertEquals("Columns.Count", baseColumnsCount + expectedPeriodCount * 2, resultForSettlementGroup.Columns.Count);

			resultForSettlementGroup = RunScript(true);
			AssertEquals("Columns.Count", baseColumnsCount + expectedPeriodCount * 4, resultForSettlementGroup.Columns.Count);

			periodManager.Periods.RemoveAndDeleteAll();

			expectedPeriodCount = 52;
			periodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2011, 1, 1), Constants.ACPeriodFormat.Weeks);
			AssertEquals("Precondition: Periods.Count", expectedPeriodCount, periodManager.Periods.Count);
			Factory.Save();

			resultForSettlementGroup = RunScript(false);
			AssertEquals("Columns.Count", baseColumnsCount + expectedPeriodCount * 2, resultForSettlementGroup.Columns.Count);

			resultForSettlementGroup = RunScript(true);
			AssertEquals("Columns.Count", baseColumnsCount + expectedPeriodCount * 4, resultForSettlementGroup.Columns.Count);
		}

		[TestDate(2018, 07, 26)]
		public void TestBranchFilterWithOneHundredSelected()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var periodManager = creator.CreateTestPeriods(new ZDateTime(2018, 1, 1));
			var department = creator.CreateDepartment("D01");
			AccGLHeader glHeader = creator.CreateAccGLHeader("5300.03.95", "TS", "ACCOUNT 1", "P&L", Core.Constants.DebitCredit.Debit);
			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			ZStringBuilder branchList = new ZStringBuilder();
			Func<int, string> genBranchCode = (int i) => { return i.ToString().PadLeft(3, '0'); };
			for (int i = 1; i < 105; i++)
			{
				var branchCode = genBranchCode(i);
				var branch = creator.CreateBranch(branchCode, GlbCompany.CurrentCompany);
				creator.CreateAccGLAggregate(100M, currentPeriod, glHeader.PK, branch.PK, GlbCompany.CurrentCompany.PK, department.PK, "");

				if (i <= 100)
				{
					branchList.Append(branchCode + ", ");
				}
			}

			Factory.Save();

			var result = RunScript(false, branchList: branchList.ToString().TrimEnd(',', ' '), additionalDissection: "Branch");

			for (int i = 1; i < 105; i++)
			{
				var branchCode = genBranchCode(i);
				var rows = result.Select(string.Format("AdditionalDissection = '{0}'", branchCode));
				if (i <= 100)
				{
					AssertEquals($"Should find row with branch {branchCode}", 1, rows.Length);
				}
				else
				{
					AssertEquals($"Should not find row with branch {branchCode}", 0, rows.Length);
				}
			}
		}

		[TestDate(2018, 07, 26)]
		public void TestDepartmentFilterWithOneHundredSelected()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var periodManager = creator.CreateTestPeriods(new ZDateTime(2018, 1, 1));
			var branch = creator.CreateBranch("B01", GlbCompany.CurrentCompany);
			AccGLHeader glHeader = creator.CreateAccGLHeader("5300.03.95", "TS", "ACCOUNT 1", "P&L", Core.Constants.DebitCredit.Debit);
			var currentPeriod = PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			ZStringBuilder departmentList = new ZStringBuilder();
			Func<int, string> gendepartmentCode = (int i) => { return i.ToString().PadLeft(3, '0'); };
			for (int i = 0; i < 105; i++)
			{
				var departmentCode = gendepartmentCode(i);
				var department = creator.CreateDepartment(departmentCode);
				creator.CreateAccGLAggregate(100M, currentPeriod, glHeader.PK, branch.PK, GlbCompany.CurrentCompany.PK, department.PK, "");

				if (i <= 100)
				{
					departmentList.Append(departmentCode + ", ");
				}
			}

			Factory.Save();

			var result = RunScript(false, departmentList: departmentList.ToString().TrimEnd(',', ' '), additionalDissection: "Department");

			for (int i = 0; i < 105; i++)
			{
				var departmentCode = gendepartmentCode(i);
				var rows = result.Select(string.Format("AdditionalDissection = '{0}'", departmentCode));
				if (i <= 100)
				{
					AssertEquals($"Should find row with department {departmentCode}", 1, rows.Length);
				}
				else
				{
					AssertEquals($"Should not find row with department {departmentCode}", 0, rows.Length);
				}
			}
		}

		DataTable RunScript(ZBool includeBudget, string branchList = "", string departmentList = "", string additionalDissection = "")
		{
			string sql = string.Format(@"
							EXEC ProfitAndLossReportPeriodAnalysis
							{0},		--@Period int,
							'{1}',		--@CompanyPK uniqueidentifier,
							'{3}',		--@DepartmentList varchar(8000),
							'{4}',		--@BranchList varchar(8000),
							'',			--@InclZeroBal char(1),
							'ALLACCT',  --@SummaryType char(20),
							'{2}',		--@IncludeBudget char(1),
							'PNL',		--@ReportType char(20),
							NULL,		--@TransactionCategory varchar(100),
							'',			--@BranchManagementCode varchar(3),
							'{5}'		--@AdditionalDissection varchar(10)
						",
						PeriodCalculator.GetPeriodFromDate(ZDateTime.Today),
						GlbCompany.CurrentCompany.PK,
						includeBudget,
						departmentList,
						branchList,
						additionalDissection
						);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}

