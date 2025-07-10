using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLossByBranch;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ProfitLossByBranch
{
	[TestedType(typeof(RawPLBSHALTAccBranchCurrentOrYearToPeriod))]
	class RawPLBSHALTAccBranchCurrentOrYearToPeriodTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			var departmentCode = "CDP";
			var currentDep = helper.InsertDepartment(departmentCode);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, currentDep, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM RawPLBSHALTAccBranchCurrentOrYearToPeriod(202001,'{TestDbHelper.DefaultCompanyPK}','{departmentCode}','','{"{0}"}',0) Period WHERE Period.Branch IS NOT NULL";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("singe category", -100m, result.Rows[0]["Cost"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("maximum group is 25", -2500m, result.Rows[0]["Cost"]);
		}
	}
}

