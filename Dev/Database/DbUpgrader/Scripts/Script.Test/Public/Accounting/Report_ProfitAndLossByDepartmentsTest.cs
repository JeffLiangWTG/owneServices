using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_ProfitAndLossByDepartments))]
	class Report_ProfitAndLossByDepartmentsTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM Report_ProfitAndLossByDepartments(202001,'{TestDbHelper.DefaultCompanyPK}','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(0m, result.Rows[0]["CurrentPeriodRevenue"]);
			AssertEquals(-100m, result.Rows[0]["CurrentPeriodCost"]);
			AssertEquals(0m, result.Rows[0]["YearToPeriodRevenue"]);
			AssertEquals(-100m, result.Rows[0]["YearToPeriodCost"]);
			AssertEquals("ZZD", result.Rows[0]["DepartmentCode"]);
			AssertEquals("", result.Rows[0]["DepartmentDesc"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(0m, result.Rows[0]["CurrentPeriodRevenue"]);
			AssertEquals(-2500m, result.Rows[0]["CurrentPeriodCost"]);
			AssertEquals(0m, result.Rows[0]["YearToPeriodRevenue"]);
			AssertEquals(-2500m, result.Rows[0]["YearToPeriodCost"]);
			AssertEquals("ZZD", result.Rows[0]["DepartmentCode"]);
			AssertEquals("", result.Rows[0]["DepartmentDesc"]);
		}
	}
}

