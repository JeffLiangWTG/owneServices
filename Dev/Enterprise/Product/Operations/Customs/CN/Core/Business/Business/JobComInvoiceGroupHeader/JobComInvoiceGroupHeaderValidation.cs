namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceGroupHeaderValidation : Customs.Business.JobComInvoiceGroupHeaderValidation
	{
		public JobComInvoiceGroupHeaderValidation(JobComInvoiceGroupHeader groupHeader)
			: base(groupHeader)
		{
		}

		public new JobComInvoiceGroupHeader InvoiceGroupHeader => (JobComInvoiceGroupHeader)base.InvoiceGroupHeader;

		protected new JobComInvoiceGroupHeader Parent => (JobComInvoiceGroupHeader)base.Parent;
	}
}
