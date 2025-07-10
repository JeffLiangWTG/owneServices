namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobComInvoiceGroupHeaderLookups : Customs.Business.JobComInvoiceGroupHeaderLookups
	{
		public JobComInvoiceGroupHeaderLookups(JobComInvoiceGroupHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceGroupHeader Invoice
		{
			get { return Parent; }
		}

		protected new JobComInvoiceGroupHeader Parent
		{
			get { return (JobComInvoiceGroupHeader)base.Parent; }
		}
	}
}
