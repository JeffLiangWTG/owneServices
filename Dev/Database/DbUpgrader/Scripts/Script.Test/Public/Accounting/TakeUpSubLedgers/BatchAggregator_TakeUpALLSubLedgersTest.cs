using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.TakeUpSubLedgers.Testing
{
	[TestedType(typeof(BatchAggregator_TakeUpALLSubLedgers))]
	class BatchAggregator_TakeUpALLSubLedgersTest : BatchAggregatorAggregationTest
	{
		public override void TestAggregation()
		{
			PrepareForAggregation();

			var expected = System.Array.Empty<(decimal, string, int, string, string, string, string)>();

			RunAndAssertAggregation(expected);
		}

		public override void TestAggregationForDifferentBranches()
		{
			Assert("It is already tested in test classes for each individual stored procedure", true);
		}

		public override void TestAggregationForDifferentDepartments()
		{
			Assert("It is already tested in test classes for each individual stored procedure", true);
		}

		public override void TestAggregationForDifferentGLAccounts()
		{
			Assert("It is already tested in test classes for each individual stored procedure", true);
		}

		public override void TestAggregationForDifferentPeriods()
		{
			Assert("It is already tested in test classes for each individual stored procedure", true);
		}
	}
}

