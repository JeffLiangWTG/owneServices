namespace Enterprise.Customs.BR.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		protected JobDeclaration JobDeclaration => Parent?.Parent?.JobDeclaration as JobDeclaration;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();

			if (!Parent.J7_RX_NKCurrency.IsEmpty)
			{
				InvoiceChargeValidation.CheckAllChargesHaveTheSameCurrency(Parent.J7_ChargeTypeInfo, JobDeclaration);
			}
		}
		protected override void CheckJ7_DistributeBy()
		{
			base.CheckJ7_DistributeBy();
			InvoiceChargeValidation.CheckDistributeBy(Parent.J7_DistributeByInfo, Parent.J7_ChargeType, JobDeclaration);
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			ValidateJ7_ChargeType();
		}
	}
}
