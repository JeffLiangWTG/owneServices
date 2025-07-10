using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public sealed class BiRequirementCheckerTest : TransactionedTestCase
	{
		public void TestIsBiDatabaseUpgradeRequired()
		{
			AssertEquals("BI database upgrade should be required for debug mode.", true, BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(TestConnection, TestConnection));
		}

		public void TestIsBiDatabaseUpgradeRequiredWhenOldSqlServerGeneration()
		{
			using (Db.Connection.SetSqlServerVersionForTest("1.00.0000.00"))
			{
				AssertEquals("BI database upgrade should be required for debug mode.", true, BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(TestConnection, TestConnection));
			}
		}

		public void TestIsBiDatabaseUpgradeRequiredWhenNotYetSupportSqlServerGeneration()
		{
			using (Db.Connection.SetSqlServerVersionForTest("99.00.0000.00"))
			{
				AssertEquals("BI database upgrade should be required for debug mode.", true, BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(TestConnection, TestConnection));
			}
		}
	}
}
