using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportEntryInstructionValidationDecider))]
	sealed class UCC6ImportEntryInstructionValidationDeciderTest : EntryInstructionValidationDeciderTest<UCC6ImportEntryInstructionValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => true;
		protected override bool ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult => true;
		protected override bool ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult => true;
		protected override bool ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result => true;
		protected override bool ExpectedIsRuleC0627Active => true;
		protected override bool ExpectedIsRuleC0810_N01Active => true;
		protected override bool ExpectedIsRuleC0834_N02Active => true;
		protected override bool ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult => false;
		protected override bool ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result => true;
		protected override bool ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult => true;
		protected override bool ExpectedIsRuleR0012Active => true;
		protected override bool ExpectedIsRuleNAT_004BisActive => true;
		protected override bool ExpectedIsRuleNAT_130BisActive => true;
		protected override bool ExpectedIsRuleR0933_N03Active => true;
		protected override bool ExpectedIsMaximumEntryLinesAllowedRuleActive => true;
		protected override bool ExpectedIsRuleNAT_030Active => true;

		public void TestIsRuleC0810_N01Active_FR()
		{
			var validationDecider = new UCC6ImportEntryInstructionValidationDecider(entryInstruction);

			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0810_N01 should apply when entry instruction is not simplified", validationDecider.IsRuleC0810_N01Active);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0810_N01 should not apply when entry instruction is simplified", !validationDecider.IsRuleC0810_N01Active);
		}

		public void TestIsRuleC0626ActiveForCEI_OA_Warehouse2_FR()
		{
			var validationDecider = new UCC6ImportEntryInstructionValidationDecider(entryInstruction);

			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0626 should apply when entry instruction is not simplified", validationDecider.IsRuleC0626ActiveForCEI_OA_Warehouse2);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0626 should not apply when entry instruction is simplified", !validationDecider.IsRuleC0626ActiveForCEI_OA_Warehouse2);
		}

		public void TestIsRuleC0627Active_FR()
		{
			var validationDecider = new UCC6ImportEntryInstructionValidationDecider(entryInstruction);

			entryInstruction.CEI_Style = "H1";
			entryInstruction.CEI_SubStyle = "A";
			Assert("Prerequisite: entry instruction is not simplified.", !entryInstruction.IsSimplified);
			Assert("Rule C0627 should apply when entry instruction is not simplified", validationDecider.IsRuleC0627Active);

			entryInstruction.CEI_Style = "I1";
			entryInstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryInstruction.IsSimplified);
			Assert("Rule C0627 should not apply when entry instruction is simplified", !validationDecider.IsRuleC0627Active);
		}

		public void TestIsRuleC0628ActiveForGoodsLocationDescription_FR()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_Procedure = "1122F15";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "3344D01";

			Assert("Rule C0628 should apply when entry has at least one invoice line whose procedure is not ending with F15.", validationDecider.IsRuleC0628ActiveForGoodsLocationDescription);

			invoiceLine2.JI_Procedure = "3344F15";
			Assert("Rule C0628 should not apply when entry has all invoice lines whose procedure is ending with F15.", !validationDecider.IsRuleC0628ActiveForGoodsLocationDescription);
		}
	}
}
