using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(UCC6ImportInvoiceHeaderValidationDecider))]
	sealed class UCC6ImportInvoiceHeaderValidationDeciderTest : InvoiceHeaderValidationDeciderTest<UCC6ImportInvoiceHeaderValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => false;

		protected override bool ExpectedIsRuleC0624Active => true;

		protected override bool ExpectedIsRuleC0627Active => true;

		protected override bool ExpectedIsRuleC0729Active => false;

		protected override bool ExpectedIsRuleC0728Active => true;

		protected override bool ExpectedIsRuleR0012Active => false;

		protected override bool ExpectedIsRuleC0738Active => false;

		protected override bool ExpectedIsRuleTNAT_078Active => true;

		protected override bool ExpectedIsRuleNAT_240Active => true;

		protected override bool ExpectedIsRuleNAT_154Active => true;

		protected override bool ExpectedIsRuleNAT_237Active => true;

		public void TestIsRuleC0624Active_FR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var validationDecider = new UCC6ImportInvoiceHeaderValidationDecider(invoice);
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0624 should apply when entry instruction is not simplified", validationDecider.IsRuleC0624Active);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0624 should not apply when entry instruction is simplified", !validationDecider.IsRuleC0624Active);
		}

		public void TestIsRuleC0627Active_FR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var validationDecider = new UCC6ImportInvoiceHeaderValidationDecider(invoice);
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0627 should apply when entry instruction is not simplified", validationDecider.IsRuleC0627Active);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0627 should not apply when entry instruction is simplified", !validationDecider.IsRuleC0627Active);
		}
	}
}
