using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class UCC6ImportInvoiceHeaderValidationDecider : IInvoiceHeaderValidationDecider
{
	public bool IsRuleC0002Active => false;

	public bool IsRuleC0624Active => false;

	public bool IsRuleC0627Active => false;

	public bool IsRuleC0729Active => false;

	public bool IsRuleC0728Active => false;

	public bool IsRuleC0738Active => false;

	public bool IsRuleR0012Active => false;
}
