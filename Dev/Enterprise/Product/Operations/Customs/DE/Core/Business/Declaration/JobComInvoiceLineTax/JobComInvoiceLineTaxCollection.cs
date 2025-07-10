namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceLineTaxCollection : EU.Business.Declaration.JobComInvoiceLineTaxCollection
	{
		public JobComInvoiceLineTaxCollection(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		public new JobComInvoiceLineTax AddNew() => (JobComInvoiceLineTax)base.AddNew();

		public new JobComInvoiceLineTax this[int index] => (JobComInvoiceLineTax)Elements[index];
	}
}
