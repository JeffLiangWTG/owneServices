using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ProfitAndLossByBranch : ScriptTest
	{
		[TestDate(2017, 01, 01)]
		public void TestProfitLossByBranch()
		{
			#region Set up

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddYears(-1)); //for previous year

			var branch1 = TestObjectCreator.CreateBranch("PER", "Perth", GlbCompany.CurrentCompany, GlbCompany.CurrentCompany.OrgProxy);
			var branch2 = TestObjectCreator.CreateBranch("HOB", "Hobart", GlbCompany.CurrentCompany, GlbCompany.CurrentCompany.OrgProxy);

			var department1 = TestObjectCreator.CreateDepartment("DAA", "Department A");
			var department2 = TestObjectCreator.CreateDepartment("DBB", "Department B");

			var profitLossCreditAccount = TestObjectCreator.CreateGLHeader();
			profitLossCreditAccount.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			profitLossCreditAccount.AG_DebitCredit = Core.Constants.DebitCredit.Credit; //revenue
			var profitLossDebitAccount = TestObjectCreator.CreateGLHeader();
			profitLossDebitAccount.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			profitLossDebitAccount.AG_DebitCredit = Core.Constants.DebitCredit.Debit; //cost			

			Factory.Save();

			#endregion

			#region Create AccGLAggregate Records

			var lastPeriodInPreviousYear = PeriodCalculator.GetLastPeriodForYear(ZDateTime.Today.Year - 1);
			var firstPeriodInCurrentYear = PeriodCalculator.GetFirstPeriodForYear(ZDateTime.Today.Year);
			var secondPeriodInCurrentYear = PeriodCalculator.GetNextPeriod(firstPeriodInCurrentYear);
			var plAppropriationAccount = (Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.Value;
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();

			//Create AccGLAggregate records for last period in previous year
			TestObjectCreator.CreateAccGLAggregate(5669.66m, lastPeriodInPreviousYear, profitLossCreditAccount.PK, branch1.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-4554.64m, lastPeriodInPreviousYear, profitLossDebitAccount.PK, branch1.PK, companyPK, department1.PK, "");

			TestObjectCreator.CreateAccGLAggregate(-2015.25m, lastPeriodInPreviousYear, profitLossCreditAccount.PK, branch1.PK, companyPK, department2.PK, "");
			TestObjectCreator.CreateAccGLAggregate(5457.66m, lastPeriodInPreviousYear, profitLossDebitAccount.PK, branch1.PK, companyPK, department2.PK, "");

			TestObjectCreator.CreateAccGLAggregate(7856.99m, lastPeriodInPreviousYear, profitLossCreditAccount.PK, branch2.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-4885.87m, lastPeriodInPreviousYear, profitLossDebitAccount.PK, branch2.PK, companyPK, department1.PK, "");

			TestObjectCreator.CreateAccGLAggregate(-2348.23m, lastPeriodInPreviousYear, profitLossCreditAccount.PK, branch2.PK, companyPK, department2.PK, "");
			TestObjectCreator.CreateAccGLAggregate(9328.99m, lastPeriodInPreviousYear, profitLossDebitAccount.PK, branch2.PK, companyPK, department2.PK, "");

			//Create AccGLAggregate records for first period
			TestObjectCreator.CreateAccGLAggregate(120.78m, firstPeriodInCurrentYear, profitLossCreditAccount.PK, branch1.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-679.96m, firstPeriodInCurrentYear, profitLossDebitAccount.PK, branch1.PK, companyPK, department1.PK, "");

			TestObjectCreator.CreateAccGLAggregate(-456.55m, firstPeriodInCurrentYear, profitLossCreditAccount.PK, branch1.PK, companyPK, department2.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-264.58m, firstPeriodInCurrentYear, profitLossDebitAccount.PK, branch1.PK, companyPK, department2.PK, "");

			TestObjectCreator.CreateAccGLAggregate(561.33m, firstPeriodInCurrentYear, profitLossCreditAccount.PK, branch2.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-567.54m, firstPeriodInCurrentYear, profitLossDebitAccount.PK, branch2.PK, companyPK, department1.PK, "");

			TestObjectCreator.CreateAccGLAggregate(-564.78m, firstPeriodInCurrentYear, profitLossCreditAccount.PK, branch2.PK, companyPK, department2.PK, "");
			TestObjectCreator.CreateAccGLAggregate(445.22m, firstPeriodInCurrentYear, profitLossDebitAccount.PK, branch2.PK, companyPK, department2.PK, "");

			//Create AccGLAggregate records for second period
			TestObjectCreator.CreateAccGLAggregate(-476.59m, secondPeriodInCurrentYear, profitLossCreditAccount.PK, branch1.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(495.68m, secondPeriodInCurrentYear, profitLossDebitAccount.PK, branch1.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-87987.44m, secondPeriodInCurrentYear, plAppropriationAccount, branch1.PK, companyPK, department1.PK, "");

			TestObjectCreator.CreateAccGLAggregate(256.39m, secondPeriodInCurrentYear, profitLossCreditAccount.PK, branch1.PK, companyPK, department2.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-167.52m, secondPeriodInCurrentYear, profitLossDebitAccount.PK, branch1.PK, companyPK, department2.PK, "");

			TestObjectCreator.CreateAccGLAggregate(-156.42m, secondPeriodInCurrentYear, profitLossCreditAccount.PK, branch2.PK, companyPK, department1.PK, "");
			TestObjectCreator.CreateAccGLAggregate(268.56m, secondPeriodInCurrentYear, profitLossDebitAccount.PK, branch2.PK, companyPK, department1.PK, "");

			TestObjectCreator.CreateAccGLAggregate(-198.21m, secondPeriodInCurrentYear, profitLossCreditAccount.PK, branch2.PK, companyPK, department2.PK, "");
			TestObjectCreator.CreateAccGLAggregate(-459.26m, secondPeriodInCurrentYear, profitLossDebitAccount.PK, branch2.PK, companyPK, department2.PK, "");

			Factory.Save();

			#endregion

			var profitLossTableHeaders = new[] { "CurrentPeriodRevenue", "CurrentPeriodCost", "CurrentPeriodNetRevenue", "YearToPeriodRevenue", "YearToPeriodCost", "YearToPeriodNetRevenue", "BranchCode", "BranchDesc" };
			var retainedEarningsTableHeaders = new[] { "CurrentPeriod", "YearToPeriod" };

			#region first period deaprtment 1

			var profitLossResult = RunScriptReport_ProfitAndLossByBranch(firstPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department1.GE_Code, "");
			var lines = new object[][]
						{
						new object[] { -120.78m, 679.96m, 559.18m, -120.78m, 679.96m, 559.18m, branch1.GB_Code, branch1.GB_BranchName },
						new object[] { -561.33m, 567.54m, 6.21m, -561.33m, 567.54m, 6.21m, branch2.GB_Code, branch2.GB_BranchName },
						};
			AssertDataTableAllRowsByKeyColumns("ProfitAndLossByBranch_FirstPeriod_Department1", profitLossResult, profitLossTableHeaders, lines, profitLossTableHeaders);

			var retainedEarningsResult = RunScript_ProfitAndLossRetainedEarningsByBranch(firstPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department1.GE_Code, "");
			lines = new object[][]
						{
						new object[] { DBNull.Value, -4086.14m },
						};
			AssertDataTableAllRowsByKeyColumns("RetainedEarningsByBranch_FirstPeriod_Department1", retainedEarningsResult, retainedEarningsTableHeaders, lines, retainedEarningsTableHeaders);

			#endregion

			#region first period deaprtment 2

			profitLossResult = RunScriptReport_ProfitAndLossByBranch(firstPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department2.GE_Code, "");
			lines = new object[][]
						{
						new object[] { 456.55m, 264.58m, 721.13m, 456.55m, 264.58m, 721.13m, branch1.GB_Code, branch1.GB_BranchName },
						new object[] { 564.78m, -445.22m, 119.56m, 564.78m, -445.22m, 119.56m, branch2.GB_Code, branch2.GB_BranchName },
						};
			AssertDataTableAllRowsByKeyColumns("ProfitAndLossByBranch_FirstPeriod_Department2", profitLossResult, profitLossTableHeaders, lines, profitLossTableHeaders);

			retainedEarningsResult = RunScript_ProfitAndLossRetainedEarningsByBranch(firstPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department2.GE_Code, "");
			lines = new object[][]
						{
						new object[] { DBNull.Value, -10423.17m },
						};
			AssertDataTableAllRowsByKeyColumns("RetainedEarningsByBranch_FirstPeriod_Department2", retainedEarningsResult, retainedEarningsTableHeaders, lines, retainedEarningsTableHeaders);

			#endregion

			#region first period all departments

			profitLossResult = RunScriptReport_ProfitAndLossByBranch(firstPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), "", "");
			lines = new object[][]
						{
						new object[] { 335.77m, 944.54m, 1280.31m, 335.77m, 944.54m, 1280.31m, branch1.GB_Code, branch1.GB_BranchName },
						new object[] { 3.45m, 122.32m, 125.77m, 3.45m, 122.32m, 125.77m, branch2.GB_Code, branch2.GB_BranchName },
						};
			AssertDataTableAllRowsByKeyColumns("ProfitAndLossByBranch_FirstPeriod_AllDepartments", profitLossResult, profitLossTableHeaders, lines, profitLossTableHeaders);

			retainedEarningsResult = RunScript_ProfitAndLossRetainedEarningsByBranch(firstPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), "", "");
			lines = new object[][]
						{
						new object[] { DBNull.Value, -14509.31m },
						};
			AssertDataTableAllRowsByKeyColumns("RetainedEarningsByBranch_FirstPeriod_AllDepartments", retainedEarningsResult, retainedEarningsTableHeaders, lines, retainedEarningsTableHeaders);

			#endregion

			#region second period deaprtment 1

			profitLossResult = RunScriptReport_ProfitAndLossByBranch(secondPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department1.GE_Code, "");
			lines = new object[][]
						{
						new object[] { 476.59m, -495.68m, -19.09m, 355.81m, 184.28m, 540.09m, branch1.GB_Code, branch1.GB_BranchName },
						new object[] { 156.42m, -268.56m, -112.14m, -404.91m, 298.98m, -105.93m, branch2.GB_Code, branch2.GB_BranchName },
						};
			AssertDataTableAllRowsByKeyColumns("ProfitAndLossByBranch_SecondPeriod_Department1", profitLossResult, profitLossTableHeaders, lines, profitLossTableHeaders);

			retainedEarningsResult = RunScript_ProfitAndLossRetainedEarningsByBranch(secondPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department1.GE_Code, "");
			lines = new object[][]
									{
						new object[] { 87987.44m, 83901.3m },
									};
			AssertDataTableAllRowsByKeyColumns("RetainedEarningsByBranch_SecondPeriod_Department1", retainedEarningsResult, retainedEarningsTableHeaders, lines, retainedEarningsTableHeaders);

			#endregion

			#region second period deaprtment 2

			profitLossResult = RunScriptReport_ProfitAndLossByBranch(secondPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department2.GE_Code, "");
			lines = new object[][]
						{
						new object[] { -256.39m, 167.52m, -88.87m, 200.16m, 432.1m, 632.26m, branch1.GB_Code, branch1.GB_BranchName },
						new object[] { 198.21m, 459.26m, 657.47m, 762.99m, 14.04m, 777.03m, branch2.GB_Code, branch2.GB_BranchName },
						};
			AssertDataTableAllRowsByKeyColumns("ProfitAndLossByBranch_SecondPeriod_Department2", profitLossResult, profitLossTableHeaders, lines, profitLossTableHeaders);

			retainedEarningsResult = RunScript_ProfitAndLossRetainedEarningsByBranch(secondPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), department2.GE_Code, "");
			lines = new object[][]
									{
						new object[] { DBNull.Value, -10423.17m },
									};
			AssertDataTableAllRowsByKeyColumns("RetainedEarningsByBranch_SecondPeriod_Department2", retainedEarningsResult, retainedEarningsTableHeaders, lines, retainedEarningsTableHeaders);

			#endregion

			#region second period all departments

			profitLossResult = RunScriptReport_ProfitAndLossByBranch(secondPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), ZString.Empty, "");
			lines = new object[][]
						{
						new object[] { 220.2m, -328.16m, -107.96m, 555.97m, 616.38m, 1172.35m, branch1.GB_Code, branch1.GB_BranchName },
						new object[] { 354.63m, 190.7m, 545.33m, 358.08m, 313.02m, 671.1m, branch2.GB_Code, branch2.GB_BranchName },
						};
			AssertDataTableAllRowsByKeyColumns("ProfitAndLossByBranch_SecondPeriod_AllDepartments", profitLossResult, profitLossTableHeaders, lines, profitLossTableHeaders);

			retainedEarningsResult = RunScript_ProfitAndLossRetainedEarningsByBranch(secondPeriodInCurrentYear, GlbCompany.CurrentCompany.PK.ToString(), "", "");
			lines = new object[][]
									{
						new object[] { 87987.44m, 73478.13m },
									};
			AssertDataTableAllRowsByKeyColumns("RetainedEarningsByBranch_SecondPeriod_AllDepartments", retainedEarningsResult, retainedEarningsTableHeaders, lines, retainedEarningsTableHeaders);

			#endregion
		}

		DataTable RunScriptReport_ProfitAndLossByBranch(ZInt period, ZString companyPK, ZString departmentCodes, ZString transactionCategory)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format("SELECT * FROM Report_ProfitAndLossByBranch ('{0}', '{1}', '{2}', '{3}')",
			period, companyPK, departmentCodes, transactionCategory));
		}

		DataTable RunScript_ProfitAndLossRetainedEarningsByBranch(ZInt period, ZString companyPK, ZString departmentCodes, ZString transactionCategory)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format("SELECT * FROM ProfitAndLossRetainedEarningsByBranch ('{0}', '{1}', '{2}', '{3}')",
			period, companyPK, departmentCodes, transactionCategory));
		}
	}
}
