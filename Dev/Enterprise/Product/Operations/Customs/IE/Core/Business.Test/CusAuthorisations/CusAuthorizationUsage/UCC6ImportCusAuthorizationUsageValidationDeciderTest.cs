using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UCC6ImportCusAuthorizationUsageValidationDecider))]
	sealed class UCC6ImportCusAuthorizationUsageValidationDeciderTest : TestCaseWithFactory
	{
		public void TestIsRuleR0010Active()
		{
			var validationDecider = new UCC6ImportCusAuthorizationUsageValidationDecider();
			Assert(validationDecider.IsRuleR0010Active);
		}

		public void TestIsRuleBR2039Active()
		{
			var validationDecider = new UCC6ImportCusAuthorizationUsageValidationDecider();
			Assert(validationDecider.IsRuleBR2039Active);
		}

		public void TestIsRuleR0675Active()
		{
			var validationDecider = new UCC6ImportCusAuthorizationUsageValidationDecider();
			AssertEquals(false, validationDecider.IsRuleR0675Active);
		}
	}
}
