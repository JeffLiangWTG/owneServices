namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaGJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public DeltaGJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckJI_CountryOfOriginMandatoryValidation()
		{
			if (!Parent.IsExport)
			{
				base.CheckJI_CountryOfOriginMandatoryValidation();
			}
		}

		protected override bool InventoryManagementSettingMatchesProcedure => base.InventoryManagementSettingMatchesProcedure &&
			Parent.SupportsBondedWarehousing && (Parent.IsIntoWarehouseWarehousing || Parent.IsIntoInwardProcessing || Parent.IsIntoOutwardProcessing || Parent.IsOutOfWarehouseWarehousing || Parent.IsOutOfInwardProcessing || Parent.IsOutOfOutwardProcessing);
	}
}
