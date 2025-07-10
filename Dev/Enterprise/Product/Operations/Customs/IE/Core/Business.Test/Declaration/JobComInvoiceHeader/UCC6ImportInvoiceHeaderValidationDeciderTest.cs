using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UCC6ImportInvoiceHeaderValidationDecider))]
	sealed class UCC6ImportInvoiceHeaderValidationDeciderTest : EU.Business.Declaration.Testing.InvoiceHeaderValidationDeciderTest<UCC6ImportInvoiceHeaderValidationDecider>
	{
		public void TestIRuleCD8051ForJZ_ValuationCodeDeciderActive()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			IRuleCD8051ForJZ_ValuationCodeDecider validationDecider = new UCC6ImportInvoiceHeaderValidationDecider();
			AssertEquals("IRuleCD8051ForJZ_ValuationCodeDecider.IsActive", true, validationDecider.IsActive(header));
		}

		protected override bool ExpectedIsRuleC0002Active => true;

		protected override bool ExpectedIsRuleC0624Active => true;

		protected override bool ExpectedIsRuleC0627Active => true;

		protected override bool ExpectedIsRuleC0729Active => true;

		protected override bool ExpectedIsRuleC0728Active => true;

		protected override bool ExpectedIsRuleR0012Active => true;

		protected override bool ExpectedIsRuleC0738Active => true;
	}
}
