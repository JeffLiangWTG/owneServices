namespace Enterprise.Customs.IL.Business
{
	public class JobComInvoiceLineViewCollection : TypeSafeJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}
	}
}
