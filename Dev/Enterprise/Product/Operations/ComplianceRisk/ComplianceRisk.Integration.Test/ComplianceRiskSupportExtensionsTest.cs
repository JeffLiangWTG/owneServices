using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Integration.Test
{
	public class ComplianceRiskSupportExtensionsTest : TestCase
	{
		public void TestIsSupportInitialization()
		{
			AssertEquals(false, ComplianceRiskSupport.None.IsSupportInitialization());
			AssertEquals(true, ComplianceRiskSupport.SupportInitialization.IsSupportInitialization());
			AssertEquals(false, ComplianceRiskSupport.SupportSubCompliances.IsSupportInitialization());
			AssertEquals(true, ComplianceRiskSupport.FullySupported.IsSupportInitialization());
		}

		public void TestIsSupportSubCompliances()
		{
			AssertEquals(false, ComplianceRiskSupport.None.IsSupportSubCompliances());
			AssertEquals(false, ComplianceRiskSupport.SupportInitialization.IsSupportSubCompliances());
			AssertEquals(true, ComplianceRiskSupport.SupportSubCompliances.IsSupportSubCompliances());
			AssertEquals(true, ComplianceRiskSupport.FullySupported.IsSupportSubCompliances());
		}
	}
}
