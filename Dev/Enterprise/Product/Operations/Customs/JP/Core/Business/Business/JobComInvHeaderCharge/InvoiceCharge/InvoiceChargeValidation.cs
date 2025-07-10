using CargoWise.Types;

namespace Enterprise.Customs.JP.Business
{
	public class InvoiceChargeValidation : Customs.Business.BaseInvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		protected new InvoiceCharge Parent
		{
			get { return (InvoiceCharge)base.Parent; }
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();
			if (!Parent.J7_Amount.IsInteger && Parent.J7_RX_NKCurrency == Core.Constants.CurrencyCodes.Japan)
			{
				Parent.J7_AmountInfo.AddMessageError(Res.GetString("9B3063D9-7E95-4432-8B1E-037E570C2538", "Amount must be whole number when Currency is JPY."));
			}
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			var parent = Parent;
			if (parent.J7_RX_NKCurrency != (parent.Invoice?.JZ_RX_NKInvoice_Currency ?? ZString.Empty))
			{
				parent.J7_RX_NKCurrencyInfo.AddMessageError(ValidationConstants.Charge.CurrencyIsDifferentToInvoice);
			}
		}
	}
}
