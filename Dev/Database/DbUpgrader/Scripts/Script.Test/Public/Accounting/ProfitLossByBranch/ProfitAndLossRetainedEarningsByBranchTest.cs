using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLossByBranch;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ProfitLossByBranch
{
	[TestedType(typeof(ProfitAndLossRetainedEarningsByBranch))]
	class ProfitAndLossRetainedEarningsByBranchTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);
			helper.SetRegistryPLAppropriationAccount(helper.GLAccountPK1);

			var sqlText = $"SELECT * FROM ProfitAndLossRetainedEarningsByBranch(202001,'{TestDbHelper.DefaultCompanyPK}','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(100m, result.Rows[0]["CurrentPeriod"]);
			AssertEquals(100m, result.Rows[0]["YearToPeriod"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(2500m, result.Rows[0]["CurrentPeriod"]);
			AssertEquals(2500m, result.Rows[0]["YearToPeriod"]);
		}
	}
}

