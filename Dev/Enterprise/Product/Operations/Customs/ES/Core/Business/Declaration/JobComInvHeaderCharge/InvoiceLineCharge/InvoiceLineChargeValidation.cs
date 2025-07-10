
namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceLineChargeValidation : EU.Business.Declaration.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge) : base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (!Parent.Lookups.ChargeTypeList.ContainsCode(Parent.J7_ChargeType))
			{
				if (Parent.J7_IsDutiable && !Parent.J7_IsIncludedInITOT)
				{
					Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("8E40B138-9978-45C4-A1CF-84A262012766", "Invalid Charge code dutiable and not included in lines is not allowed."));
				}
			}
		}
	}
}
