using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(UCC6ImportCusAuthorizationUsageValidationDecider))]
	sealed class UCC6ImportCusAuthorizationUsageValidationDeciderTest : CusAuthorizationUsageValidationDeciderTestBase<UCC6ImportCusAuthorizationUsageValidationDecider>
	{
		protected override bool ExpectedIsRuleR0010Active => true;

		protected override bool ExpectedIsRuleR0675Active => false;
	}
}
