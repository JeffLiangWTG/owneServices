namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class IncompleteInvoicingLineBaseValidation : InvoicingLineBaseValidation
	{
		public IncompleteInvoicingLineBaseValidation(InvoicingLineBase parent)
			: base(parent)
		{
		}

		protected override void CheckAL_OSExTaxAmount()
		{
		}

		protected override void CheckGenericCharge()
		{
		}

		protected override void CheckAL_Desc()
		{
		}

		protected override void CheckAL_AT()
		{
		}

		protected override void CheckAL_GEIsNotEmpty()
		{
		}

		protected override void CheckAL_GBIsNotEmpty()
		{
		}

		protected override void CheckAL_SupplyType()
		{
		}
	}
}
