namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineViewCollection : TypeSafeJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader parent, Customs.Business.InvoiceLineDependentCollection completeCollection)
			: base(parent, completeCollection)
		{
		}
	}
}
