namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceGroupHeaderLookups : Customs.Business.JobComInvoiceGroupHeaderLookups
	{
		public JobComInvoiceGroupHeaderLookups(JobComInvoiceGroupHeader parent)
			: base(parent)
		{
		}

		public new JobComInvoiceGroupHeader Invoice => Parent;

		protected new JobComInvoiceGroupHeader Parent => (JobComInvoiceGroupHeader)base.Parent;
	}
}
