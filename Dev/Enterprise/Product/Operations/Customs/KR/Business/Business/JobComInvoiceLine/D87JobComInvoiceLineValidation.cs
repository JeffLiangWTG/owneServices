namespace Enterprise.Customs.KR.Business
{
	public class D87JobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public D87JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckJI_InvoiceUQ()
		{
		}

		protected override void CheckJI_NetWeightUQ()
		{
		}

		protected override void CheckJI_WeightUQ()
		{
		}

		protected override void CheckJI_Description()
		{
		}

		protected override void CheckJI_Tariff()
		{
		}

		protected override void CheckJI_LinePrice() { }
	}
}
