using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportInvoiceLineValidationDecider))]
	sealed class UCC6ImportInvoiceLineValidationDeciderTest : ImportInvoiceLineValidationDeciderTest<UCC6ImportInvoiceLineValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => true;

		protected override bool ExpectedIsRuleC0627Active => true;

		protected override bool ExpectedIsRuleC0820ActiveForJI_SupplementaryCode1 => true;

		protected override bool ExpectedIsRuleC0820ActiveForJI_Tariff => true;

		protected override bool ExpectedIsRuleCD0111Active => true;

		protected override bool ExpectedIsRuleCD5151ActiveForZG_CountryOfSupply => false;

		protected override bool ExpectedIsRuleCD5161ActiveForJI_CountryOfOrigin => false;

		protected override bool ExpectedIsRuleCD9102Active => true;

		protected override bool ExpectedIsRuleC0699_N01Active => true;

		protected override bool ExpectedIsRuleC0699_N02Active => true;

		protected override bool ExpectedIsRuleC0710ActiveForJI_LinePrice => true;

		protected override bool ExpectedIsRuleC0710ActiveForJI_CountryOfOrigin => false;

		protected override bool ExpectedIsRuleC0710ActiveForZG_CountryOfSupply => false;

		protected override bool ExpectedIsRuleC0710_N01Active => true;

		protected override bool ExpectedIsRuleC0919Active => false;

		protected override bool ExpectedIsRuleR0012Active => true;

		protected override bool ExpectedIsRuleR0222Active => false;

		protected override bool ExpectedIsRuleR0224Active => true;

		protected override bool ExpectedIsRuleC0624Active => true;

		protected override bool ExpectedIsRuleC0936Active => true;

		protected override bool ExpectedIsRuleC0728Active => true;

		protected override bool ExpectedAllowMultipleRequestedProcedure => true;

		protected override bool ExpectedIsRuleC0834_N02Active => true;

		protected override bool ExpectedIsRuleNAT_105Active => true;

		protected override bool ExpectedIsRuleNAT_235Active => true;

		protected override bool ExpectedIsRuleNAT_240Active => true;

		protected override bool ExpectedIsRuleNAT_254Active => true;

		protected override bool ExpectedIsRuleR0223Active => true;

		protected override bool ExpectedIsRuleNAT_030Active => true;

		protected override bool ExpectedIsRuleNAT_154Active => true;

		protected override bool ExpectedIsRuleNAT_237Active => true;

		public void TestShouldValidateCPCAgainstEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var decider = new UCC6ImportInvoiceLineValidationDecider(invoiceLine);
			AssertEquals(true, decider.ShouldValidateCPCAgainstEntryInstruction);
		}

		public void TestIsRuleC0624Active_FR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var decider = new UCC6ImportInvoiceLineValidationDecider(invoiceLine);
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0624 should apply when entry instruction is not simplified", decider.IsRuleC0624Active);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0624 should not apply when entry instruction is simplified", !decider.IsRuleC0624Active);
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
			var decider = new UCC6ImportInvoiceLineValidationDecider(invoiceLine);
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0627 should apply when entry instruction is not simplified", decider.IsRuleC0627Active);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0627 should not apply when entry instruction is simplified", !decider.IsRuleC0627Active);
		}

		public void TestIsRuleCD0111Active_FR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var decider = new UCC6ImportInvoiceLineValidationDecider(invoiceLine);
			Assert("Prerequisite: HasFreeGoods should be false when there is no SupplementaryCode equal to 0097.", !invoiceLine.HasFreeGoods);
			Assert("Rule CD0111 should apply when Rule NAT_240 is not active or the invoiceLine does not has freeGoods.", decider.IsRuleCD0111Active);

			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			Assert("Prerequisite: HasFreeGoods should be true when SupplementaryCode equal to 0097.", invoiceLine.HasFreeGoods);
			Assert("Rule CD0111 should not apply when Rule NAT_240 is active and the invoiceLine has freeGoods.", !decider.IsRuleCD0111Active);
		}

		public void TestIsRuleC0710ActiveForJI_LinePrice_FR()
		{
			var declaration = Factory.New<JobDeclaration>();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var decider = new UCC6ImportInvoiceLineValidationDecider(invoiceLine);
			Assert("Prerequisite: HasFreeGoods should be false when there is no SupplementaryCode equal to 0097.", !invoiceLine.HasFreeGoods);
			Assert("Rule C0710ActiveForJI_LinePrice should apply when Rule NAT_240 is not active or the invoiceLine does not has freeGoods.", decider.IsRuleC0710ActiveForJI_LinePrice);

			invoiceLine.JI_SupplementaryCode1 = FRConstants.SupplementaryCodes.FreeGoodsSupplementaryCode;
			Assert("Prerequisite: HasFreeGoods should be true when SupplementaryCode equal to 0097.", invoiceLine.HasFreeGoods);
			Assert("Rule C0710ActiveForJI_LinePrice should not apply when Rule NAT_240 is active and the invoiceLine has freeGoods.", !decider.IsRuleC0710ActiveForJI_LinePrice);
		}
	}
}
