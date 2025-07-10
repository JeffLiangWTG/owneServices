using CargoWise.Types;

namespace Enterprise.Customs.JP.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			var parent = Parent;
			if (parent.J7_RX_NKCurrency != (parent?.InvoiceLine?.JI_RX_NKLinePriceCurr ?? ZString.Empty))
			{
				parent.J7_RX_NKCurrencyInfo.AddMessageError(ValidationConstants.Charge.CurrencyIsDifferentToInvoice);
			}
		}
	}
}
