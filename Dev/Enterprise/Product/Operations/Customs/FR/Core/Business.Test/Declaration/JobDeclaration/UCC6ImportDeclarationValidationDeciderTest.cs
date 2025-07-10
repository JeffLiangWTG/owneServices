using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Testing
{
	[TestedType(typeof(UCC6ImportDeclarationValidationDecider))]
	sealed class UCC6ImportDeclarationValidationDeciderTest : DeclarationValidationDeciderTest<UCC6ImportDeclarationValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => true;

		protected override bool ExpectedIsRuleC0211Active => false;

		protected override bool ExpectedIsRuleC0623Active => true;

		protected override bool ExpectedIsRuleC0646Active => true;

		protected override bool ExpectedIsRuleC0729Active => false;

		protected override bool ExpectedIsRuleC0738Active => false;

		protected override bool ExpectedIsRuleC0841Active => false;

		protected override bool ExpectedIsRuleC0843Active => false;

		protected override bool ExpectedIsRuleNAT_020Active => true;

		protected override bool ExpectedIsRuleNAT_021Active => true;

		protected override bool ExpectedIsRuleNAT_130BisActive => true;

		protected override bool ExpectedIsRuleNat_145BisActive => true;

		protected override bool ExpectedIsRuleNAT_041QuinquiesActive => true;

		public void TestIsRuleC0810_N01Active()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			entryinstruction.CEI_Style = "H1";
			entryinstruction.CEI_SubStyle = "A";
			var validationDecider = new UCC6ImportDeclarationValidationDecider(declaration);

			Assert("Prerequisite: entry instruction is not simplified.", !entryinstruction.IsSimplified);
			Assert("Prerequisite: declaration has not simplified entry.", !declaration.HasSimplifiedEntry);
			Assert("Rule C0810_N01 should apply when declaration has not simplified entry.", validationDecider.IsRuleC0810_N01Active);

			entryinstruction.CEI_Style = "I1";
			entryinstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryinstruction.IsSimplified);
			Assert("Prerequisite: declaration has simplified entry.", declaration.HasSimplifiedEntry);
			Assert("Rule C0810_N01 should not apply when the declaration has a simplified entry.", !validationDecider.IsRuleC0810_N01Active);
		}

		public void TestIsRuleC0623Active()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			entryinstruction.CEI_Style = "H1";
			entryinstruction.CEI_SubStyle = "A";
			var validationDecider = new UCC6ImportDeclarationValidationDecider(declaration);

			Assert("Prerequisite: entry instruction is not simplified.", !entryinstruction.IsSimplified);
			Assert("Prerequisite: declaration has not simplified entry.", !declaration.HasSimplifiedEntry);
			Assert("Rule C0623 should apply when the declaration does not has a simplifiedEntry.", validationDecider.IsRuleC0623Active);

			entryinstruction.CEI_Style = "I1";
			entryinstruction.CEI_SubStyle = "C";
			Assert("Prerequisite: entry instruction is simplified.", entryinstruction.IsSimplified);
			Assert("Prerequisite: declaration has simplified entry.", declaration.HasSimplifiedEntry);
			Assert("Rule C0623 should not apply when the declaration has a simplifiedEntry.", !validationDecider.IsRuleC0623Active);
		}
	}
}
