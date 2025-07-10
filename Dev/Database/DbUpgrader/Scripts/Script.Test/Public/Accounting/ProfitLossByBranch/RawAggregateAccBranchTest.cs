using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ProfitLossByBranch;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ProfitLossByBranch
{
	[TestedType(typeof(RawAggregateAccBranch))]
	class RawAggregateAccBranchTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			var departmentCode = "CDP";
			var currentDep = helper.InsertDepartment(departmentCode);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, currentDep, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM RawAggregateAccBranch(202001,'{departmentCode}',202002,'{TestDbHelper.DefaultCompanyPK}','{"{0}"}',NULL)";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(100m, result.Rows[0]["CurrentAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(2500m, result.Rows[0]["CurrentAmount"]);
		}
	}
}

