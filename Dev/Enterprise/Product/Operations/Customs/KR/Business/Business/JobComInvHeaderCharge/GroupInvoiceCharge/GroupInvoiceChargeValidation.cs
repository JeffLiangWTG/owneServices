namespace Enterprise.Customs.KR.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge parent) : base(parent)
		{
		}

		protected new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;

		protected override void CheckJ7_ExchangeRate()
		{
			base.CheckJ7_ExchangeRate();
			Parent.J7_ExchangeRateInfo.CheckCurrencyAndRate((JobDeclaration)Parent.Parent?.JobDeclaration, Parent.J7_RX_NKCurrency);
		}
	}
}
