using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportInvoiceHeaderValidationDecider))]
	sealed class UCC6ImportInvoiceHeaderValidationDeciderTest : InvoiceHeaderValidationDeciderTest<UCC6ImportInvoiceHeaderValidationDecider>
	{
		protected override bool ExpectedIsRuleR0012Active => true;

		protected override bool ExpectedIsRuleC0002Active => true;

		protected override bool ExpectedIsRuleC0624Active => true;

		protected override bool ExpectedIsRuleC0627Active => true;

		protected override bool ExpectedIsRuleC0728Active => true;

		protected override bool ExpectedIsRuleC0729Active => true;

		protected override bool ExpectedIsRuleC0738Active => true;
	}
}
