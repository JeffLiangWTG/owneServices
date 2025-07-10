namespace Enterprise.Customs.KR.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge parent) : base(parent)
		{
		}

		protected new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		protected override void CheckJ7_ExchangeRate()
		{
			base.CheckJ7_ExchangeRate();
			Parent.J7_ExchangeRateInfo.CheckCurrencyAndRate((JobDeclaration)Parent.Parent?.JobDeclaration, Parent.J7_RX_NKCurrency);
		}
	}
}
