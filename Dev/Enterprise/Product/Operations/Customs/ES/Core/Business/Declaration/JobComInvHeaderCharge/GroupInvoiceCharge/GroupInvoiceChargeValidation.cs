
namespace Enterprise.Customs.ES.Business.Declaration
{
	public class GroupInvoiceChargeValidation : EU.Business.Declaration.GroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge charge) : base(charge)
		{
		}

		public new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (!Parent.Lookups.ChargeTypeList.ContainsCode(Parent.J7_ChargeType))
			{
				if (Parent.J7_IsDutiable && !Parent.J7_IsIncludedInITOT)
				{
					Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("D8F4DFF8-8533-4BF1-9E13-5DD2E836326D", "Invalid Charge code dutiable and not included in lines is not allowed."));
				}
			}
		}
	}
}
