using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(UCC6ImportDeclarationValidationDecider))]
sealed class UCC6ImportDeclarationValidationDeciderTest
	: EU.Business.Declaration.Testing.DeclarationValidationDeciderTest<UCC6ImportDeclarationValidationDecider>
{
	protected override bool ExpectedIsRuleC0002Active => false;

	protected override bool ExpectedIsRuleC0211Active => false;

	protected override bool ExpectedIsRuleC0623Active => false;

	protected override bool ExpectedIsRuleC0646Active => false;

	protected override bool ExpectedIsRuleC0729Active => false;

	protected override bool ExpectedIsRuleC0738Active => false;

	protected override bool ExpectedIsRuleC0841Active => false;

	protected override bool ExpectedIsRuleC0843Active => false;
}
