namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineDependentCollection : EU.Business.Declaration.InvoiceLineDependentCollection
	{
		public InvoiceLineDependentCollection(JobComInvoiceHeader invoice) : base(invoice)
		{
		}

		public new JobComInvoiceLine AddNew()
		{
			return (JobComInvoiceLine)base.AddNew();
		}

		public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];
	}
}
