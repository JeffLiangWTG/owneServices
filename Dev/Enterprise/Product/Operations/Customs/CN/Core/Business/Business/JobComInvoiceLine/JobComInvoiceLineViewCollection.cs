namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceLineViewCollection : TypeSafeJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoiceHeader, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(invoiceHeader, completeCollection)
		{
		}
	}
}
