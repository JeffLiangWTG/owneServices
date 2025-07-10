using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(UCC6ExportDeclarationValidationDecider))]
sealed class UCC6ExportDeclarationValidationDeciderTest
	: EU.Business.Declaration.Testing.DeclarationValidationDeciderTest<UCC6ExportDeclarationValidationDecider>
{
	protected override bool ExpectedIsRuleC0002Active => false;

	protected override bool ExpectedIsRuleC0211Active => true;

	protected override bool ExpectedIsRuleC0623Active => false;

	protected override bool ExpectedIsRuleC0646Active => false;

	protected override bool ExpectedIsRuleC0729Active => false;

	protected override bool ExpectedIsRuleC0738Active => false;

	protected override bool ExpectedIsRuleC0841Active => true;

	protected override bool ExpectedIsRuleC0843Active => true;
}
