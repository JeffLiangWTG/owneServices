using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportDeclarationValidationDecider))]
	sealed class UCC6ImportDeclarationValidationDeciderTest : DeclarationValidationDeciderTest<UCC6ImportDeclarationValidationDecider>
	{
		protected override bool ExpectedIsRuleC0002Active => true;

		protected override bool ExpectedIsRuleC0211Active => false;

		protected override bool ExpectedIsRuleC0623Active => true;

		protected override bool ExpectedIsRuleC0646Active => true;

		protected override bool ExpectedIsRuleC0729Active => true;

		protected override bool ExpectedIsRuleC0738Active => true;

		protected override bool ExpectedIsRuleC0841Active => false;

		protected override bool ExpectedIsRuleC0843Active => false;
	}
}
