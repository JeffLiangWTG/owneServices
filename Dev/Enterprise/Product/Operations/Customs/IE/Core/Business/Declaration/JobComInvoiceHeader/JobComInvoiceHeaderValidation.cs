namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		protected JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override bool ShouldCheckRelatedHouseBillEntered => false;
	}
}
