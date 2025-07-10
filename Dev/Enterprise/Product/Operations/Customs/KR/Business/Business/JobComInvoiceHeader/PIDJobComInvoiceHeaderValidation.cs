namespace Enterprise.Customs.KR.Business
{
	public class PIDJobComInvoiceHeaderValidation : AutoKRJobComInvoiceHeaderValidation
	{
		public PIDJobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{ }

		protected override void CheckJZ_IncoTerm()
		{ }
	}
}
