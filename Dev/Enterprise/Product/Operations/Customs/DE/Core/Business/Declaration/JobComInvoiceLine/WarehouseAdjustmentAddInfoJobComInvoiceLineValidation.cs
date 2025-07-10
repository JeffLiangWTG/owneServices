namespace Enterprise.Customs.DE.Business.Declaration
{
	public class WarehouseAdjustmentAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public WarehouseAdjustmentAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckZG_CountryOfSupply()
		{
		}

		protected override void CheckZG_CessionFlag()
		{
		}

		protected override void CheckZG_QuotaQty()
		{
		}

		protected override void CheckZG_QuotaUQ()
		{
		}

		protected override void CheckZG_TobaccoStamp()
		{
		}
	}
}
