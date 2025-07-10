namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobComInvoiceHeaderLookups : Customs.Business.JobComInvoiceHeaderLookups
	{
		public JobComInvoiceHeaderLookups(JobComInvoiceHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceHeader Invoice
		{
			get { return (JobComInvoiceHeader)base.Invoice; }
		}

		protected new JobComInvoiceHeader Parent
		{
			get { return (JobComInvoiceHeader)base.Parent; }
		}
	}
}
