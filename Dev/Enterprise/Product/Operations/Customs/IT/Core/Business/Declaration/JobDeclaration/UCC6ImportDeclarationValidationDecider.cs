using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class UCC6ImportDeclarationValidationDecider : IDeclarationValidationDecider
{
	public bool IsRuleC0002Active => false;

	public bool IsRuleC0211Active => false;

	public bool IsRuleC0623Active => false;

	public bool IsRuleC0646Active => false;

	public bool IsRuleC0729Active => false;

	public bool IsRuleC0738Active => false;

	public bool IsRuleC0841Active => false;

	public bool IsRuleC0843Active => false;
}
