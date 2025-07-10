namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceApportionChargeValidation : EU.Business.Declaration.InvoiceApportionChargeValidation
	{
		public InvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		protected override void CheckJ7_IsIncludedInITOT()
		{
			if (!ChargeHelper.IsJ7_IsIncludedInITOT_NoValidation(Parent.J7_ChargeType))
			{
				base.CheckJ7_IsIncludedInITOT();
			}
		}

		protected override void CheckJ7_IsGSTApplicable()
		{
			if (!Parent.IsFreightChargeToEUBorderByAirInsideEU())
			{
				base.CheckJ7_IsGSTApplicable();
			}
		}
	}
}
