namespace Enterprise.Customs.DE.Business.Declaration
{
	public class WarehouseAdjustmentJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public WarehouseAdjustmentJobComInvoiceHeaderValidation(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
		}

		protected override void CheckJZ_IncoTerm()
		{
		}
	}
}
