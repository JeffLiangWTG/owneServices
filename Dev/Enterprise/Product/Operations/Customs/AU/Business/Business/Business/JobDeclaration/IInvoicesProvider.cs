namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IInvoicesProvider : Customs.Business.IInvoicesProvider
	{
		new InvoiceLineViewCollection FilteredInvoiceLines { get; }
		new InvoiceHeaderActiveCollection Invoices { get; }
		Customs.Business.BaseJobComInvoiceGroupHeader TopGroupInvoice { get; }
	}
}
