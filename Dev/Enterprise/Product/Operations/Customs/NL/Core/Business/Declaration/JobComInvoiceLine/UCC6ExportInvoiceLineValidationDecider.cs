using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Declaration;

public sealed class UCC6ExportInvoiceLineValidationDecider : IExportInvoiceLineValidationDecider
{
	public bool IsRuleR0222Active => true;

	public bool IsRuleR0223Active => true;

	public bool IsRuleR0224Active => true;
}
