using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(RawPLBSHALTAccDepYeartoPeriod))]
	class RawPLBSHALTAccDepYeartoPeriodTest : DbCreateScriptTest
	{
		public void TestTransactionCategoryGroup()
		{
			var helper = new TestDbHelper(TestConnection);

			var categotyGroups = TestHelper.CreateGLAggregate(helper, "A", 26, 100, 202001, helper.GLAccountPK1, helper.DefaultDepartmentPK, helper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK);

			var sqlText = $"SELECT * FROM RawPLBSHALTAccDepYeartoPeriod(202001,'{TestDbHelper.DefaultCompanyPK}','','','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have 2 row", 2, result.Rows.Count);

			var row = FirstOrDefault(result, "GE_PK1", DBNull.Value);
			AssertEquals(DBNull.Value, row["AG_PK"]);
			AssertEquals(0m, row["AmountYearToPeriodRevenue"]);
			AssertEquals(0m, row["AmountYearToPeriodCost"]);
			AssertEquals(DBNull.Value, row["AccNum"]);

			row = FirstOrDefault(result, "GE_PK1", helper.DefaultDepartmentPK);
			AssertEquals(helper.GLAccountPK1, row["AG_PK"]);
			AssertEquals(0m, row["AmountYearToPeriodRevenue"]);
			AssertEquals(-100m, row["AmountYearToPeriodCost"]);
			AssertEquals("111.222.01", row["AccNum"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, categotyGroups));
			AssertEquals("Result should have 2 row", 2, result.Rows.Count);

			row = FirstOrDefault(result, "GE_PK1", DBNull.Value);
			AssertEquals(DBNull.Value, row["AG_PK"]);
			AssertEquals(0m, row["AmountYearToPeriodRevenue"]);
			AssertEquals(0m, row["AmountYearToPeriodCost"]);
			AssertEquals(DBNull.Value, row["AccNum"]);

			row = FirstOrDefault(result, "GE_PK1", helper.DefaultDepartmentPK);
			AssertEquals(helper.GLAccountPK1, row["AG_PK"]);
			AssertEquals(0m, row["AmountYearToPeriodRevenue"]);
			AssertEquals(-2500m, row["AmountYearToPeriodCost"]);
			AssertEquals("111.222.01", row["AccNum"]);
		}
	}
}

