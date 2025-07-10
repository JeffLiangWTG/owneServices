using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration;

public sealed class UCC6ExportDeclarationValidationDecider : IDeclarationValidationDecider
{
	public bool IsRuleC0002Active => false;

	public bool IsRuleC0211Active => true;

	public bool IsRuleC0623Active => false;

	public bool IsRuleC0646Active => false;

	public bool IsRuleC0729Active => false;

	public bool IsRuleC0738Active => false;

	public bool IsRuleC0841Active => true;

	public bool IsRuleC0843Active => true;
}
