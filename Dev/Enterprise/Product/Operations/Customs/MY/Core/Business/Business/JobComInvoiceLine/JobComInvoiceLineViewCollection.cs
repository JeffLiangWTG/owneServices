namespace Enterprise.Customs.MY.Business
{
	public class JobComInvoiceLineViewCollection : TypeSafeJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}
	}
}
