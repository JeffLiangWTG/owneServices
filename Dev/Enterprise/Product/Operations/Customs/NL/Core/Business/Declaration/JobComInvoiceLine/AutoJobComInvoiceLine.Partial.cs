namespace Enterprise.Customs.NL.Business.Declaration;

public abstract partial class AutoJobComInvoiceLine
{
	protected override EU.Business.Declaration.AddInfoJobComInvoiceLine GetNewAddInfo() => new AddInfoJobComInvoiceLine(JI_AddInfoInfo);

	public new AddInfoJobComInvoiceLine AddInfo => (AddInfoJobComInvoiceLine)base.AddInfo;

	public new AddInfoJobComInvoiceLineLookups AddInfoLookups => AddInfo.Lookups;

	public new AddInfoJobComInvoiceLineValidation AddInfoValidation => AddInfo.Validation;
}
