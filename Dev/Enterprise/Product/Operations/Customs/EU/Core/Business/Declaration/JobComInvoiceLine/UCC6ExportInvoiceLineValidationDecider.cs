namespace Enterprise.Customs.EU.Business.Declaration;

public class UCC6ExportInvoiceLineValidationDecider : IExportInvoiceLineValidationDecider
{
	public bool IsRuleR0222Active => false;

	public bool IsRuleR0223Active => true;

	public bool IsRuleR0224Active => true;
}
