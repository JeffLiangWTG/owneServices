using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(UCC5ImportInvoiceHeaderValidationDecider))]
	sealed class UCC5ImportInvoiceHeaderValidationDeciderTest : EU.Business.Declaration.Testing.InvoiceHeaderValidationDeciderTest<UCC5ImportInvoiceHeaderValidationDecider>
	{
		public void TestIRuleCD8051ForJZ_ValuationCodeDeciderActive()
		{
			var header = Factory.New<JobComInvoiceHeader>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "V1";
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = header.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			instrction.CEI_Style = "H1";
			IRuleCD8051ForJZ_ValuationCodeDecider validationDecider = new UCC5ImportInvoiceHeaderValidationDecider();
			AssertEquals("IRuleCD8051ForJZ_ValuationCodeDecider.IsActive", true, validationDecider.IsActive(header));

			instrction.CEI_Style = "H2";
			AssertEquals("IRuleCD8051ForJZ_ValuationCodeDecider.IsActive", false, validationDecider.IsActive(header));

			instrction.CEI_Style = "H5";
			AssertEquals("IRuleCD8051ForJZ_ValuationCodeDecider.IsActive", true, validationDecider.IsActive(header));
		}

		protected override bool ExpectedIsRuleC0002Active => false;

		protected override bool ExpectedIsRuleC0624Active => false;

		protected override bool ExpectedIsRuleC0627Active => false;

		protected override bool ExpectedIsRuleC0729Active => false;

		protected override bool ExpectedIsRuleC0728Active => false;

		protected override bool ExpectedIsRuleR0012Active => false;

		protected override bool ExpectedIsRuleC0738Active => false;
	}
}
