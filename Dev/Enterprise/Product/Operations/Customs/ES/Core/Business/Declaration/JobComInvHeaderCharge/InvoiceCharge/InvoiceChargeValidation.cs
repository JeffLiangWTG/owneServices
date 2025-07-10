
namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceChargeValidation : EU.Business.Declaration.InvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge) : base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (!Parent.Lookups.ChargeTypeList.ContainsCode(Parent.J7_ChargeType))
			{
				if (Parent.J7_IsDutiable && !Parent.J7_IsIncludedInITOT)
				{
					Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("998F6D67-D481-4BF4-A520-2DD47CFBD636", "Invalid Charge code dutiable and not included in lines is not allowed."));
				}
			}
		}
	}
}
