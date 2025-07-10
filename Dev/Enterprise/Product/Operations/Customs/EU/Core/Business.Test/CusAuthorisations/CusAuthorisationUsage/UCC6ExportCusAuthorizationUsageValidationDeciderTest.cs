using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(UCC6ExportCusAuthorizationUsageValidationDecider))]
	sealed class UCC6ExportCusAuthorizationUsageValidationDeciderTest : CusAuthorizationUsageValidationDeciderTestBase<UCC6ExportCusAuthorizationUsageValidationDecider>
	{
		protected override bool ExpectedIsRuleR0010Active => false;

		protected override bool ExpectedIsRuleR0675Active => false;
	}
}
