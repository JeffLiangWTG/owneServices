namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACWithoutLinesJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public SACWithoutLinesJobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override void CheckJI_Tariff()
		{
		}

		protected override void CheckJI_Description()
		{
		}

		protected override void CheckJI_InvoiceUQ()
		{
		}
	}
}
