using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(RawAggregateAccDep))]
	class RawAggregateAccDepTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchCode = "CBH";
			var currentBranch = helper.InsertBranch(branchCode, TestDbHelper.DefaultCompanyPK);

			helper.SetRegistryPLAppropriationAccount(helper.GLAccountPK1);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, currentBranch, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM RawAggregateAccDep(202001,'{branchCode}',202002,'{TestDbHelper.DefaultCompanyPK}','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("singe category", 100m, result.Rows[0]["CurrentAmount"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("maximum group is 25", 2500m, result.Rows[0]["CurrentAmount"]);
		}
	}
}

