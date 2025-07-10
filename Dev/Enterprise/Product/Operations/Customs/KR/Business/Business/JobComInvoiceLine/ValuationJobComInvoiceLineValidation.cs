namespace Enterprise.Customs.KR.Business
{
	public class ValuationJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public ValuationJobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}
		protected override void CheckJI_InvoiceUQ()
		{
		}
	}
}
