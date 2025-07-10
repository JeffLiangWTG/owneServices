namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobComInvoiceHeaderValidation : Customs.Business.InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override void CheckJZ_Calc_CIFAmount_ZeroFreightInsurance()
		{
		}
	}
}
