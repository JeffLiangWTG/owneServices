using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class CostVarianceComparisonOptionTest : TestCase
	{
		public void TestCostVarianceComparisonOption()
		{
			AssertEquals("JOB", Constants.CostVarianceComparisonOption.Job);
			AssertEquals("JCH", Constants.CostVarianceComparisonOption.JobAndChargeCode);
			AssertEquals("JCR", Constants.CostVarianceComparisonOption.JobAndCreditor);
			AssertEquals("CJB", Constants.CostVarianceComparisonOption.ImportedChargeOrJob);
			AssertEquals("CCH", Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode);
			AssertEquals("CJR", Constants.CostVarianceComparisonOption.ImportedChargeOrCreditor);
		}
	}
}
