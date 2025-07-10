namespace Enterprise.Customs.DE.Business.Declaration
{
	public class StockMovementJobComInvoiceHeaderValidation : ImportJobComInvoiceHeaderValidation
	{
		public StockMovementJobComInvoiceHeaderValidation(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override void CheckJZ_IncoTerm()
		{
		}
	}
}
