using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ExportDeclarationValidationDecider))]
	sealed class UCC6ExportDeclarationValidationDeciderTest : DeclarationValidationDeciderTest<UCC6ExportDeclarationValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => false;

		protected override bool ExpectedIsRuleC0211Active => false;

		protected override bool ExpectedIsRuleC0623Active => false;

		protected override bool ExpectedIsRuleC0646Active => false;

		protected override bool ExpectedIsRuleC0729Active => false;

		protected override bool ExpectedIsRuleC0738Active => false;

		protected override bool ExpectedIsRuleC0841Active => true;

		protected override bool ExpectedIsRuleC0843Active => false;
	}
}
