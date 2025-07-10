namespace Enterprise.Customs.KR.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge parent) : base(parent)
		{
		}

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		protected override void CheckJ7_ExchangeRate()
		{
			base.CheckJ7_ExchangeRate();
			Parent.J7_ExchangeRateInfo.CheckCurrencyAndRate((JobDeclaration)Parent.Parent?.JobDeclaration, Parent.J7_RX_NKCurrency);
		}
	}
}
