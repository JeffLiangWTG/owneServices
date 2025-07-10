using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(UCC6ImportInvoiceHeaderValidationDecider))]
sealed class UCC6ImportInvoiceHeaderValidationDeciderTest : InvoiceHeaderValidationDeciderTest<UCC6ImportInvoiceHeaderValidationDecider>
{
	protected override bool ExpectedIsRuleC0002Active => false;

	protected override bool ExpectedIsRuleC0624Active => false;

	protected override bool ExpectedIsRuleC0627Active => false;

	protected override bool ExpectedIsRuleC0729Active => false;

	protected override bool ExpectedIsRuleC0728Active => false;

	protected override bool ExpectedIsRuleR0012Active => false;

	protected override bool ExpectedIsRuleC0738Active => false;
}
