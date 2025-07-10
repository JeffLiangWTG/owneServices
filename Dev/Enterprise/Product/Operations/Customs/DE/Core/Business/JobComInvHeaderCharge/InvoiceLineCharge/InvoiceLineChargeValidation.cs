using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceLineChargeValidation : EU.Business.Declaration.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge parent) : base(parent)
		{
		}

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		protected override void CheckJ7_ExchangeRateDate()
		{
			base.CheckJ7_ExchangeRateDate();
			if (Parent.J7_ExchangeRateDate > ZDate.Today)
			{
				Parent.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("104F09C9-8CFD-43D2-B91A-E25B3B9DD4F0", "The Exchange Rate Date must not be in the future."));
			}
			else if (Parent.J7_ExchangeRateDate.IsEmpty && Parent.IsJ7_ExchangeRateUserEnterable)
			{
				Parent.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("DCFCA81E-BC16-4496-9427-A014BBB89C25", "You have not entered an Exchange Rate Date."));
			}
		}

		protected override void CheckJ7_ChargeDescription()
		{
			base.CheckJ7_ChargeDescription();
			if (!Parent.J7_ChargeDescriptionInfo.ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.J7_ChargeDescriptionInfo);
			}
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			var chargeCode = Parent.J7_ChargeType;
			var chargeCurrency = Parent.J7_RX_NKCurrency;
			var invoiceLine = Parent.InvoiceLine;
			var invoiceHeader = invoiceLine?.InvoiceHeader;
			if (invoiceLine != null && (invoiceLine.HasChargeCodeWithDifferentCurrency(chargeCode, chargeCurrency)
				|| invoiceHeader.HasChargeCodeWithDifferentCurrency(chargeCode, chargeCurrency)
				|| invoiceHeader.GroupHeader.HasChargeCodeWithDifferentCurrency(chargeCode, chargeCurrency)))
			{
				Parent.J7_RX_NKCurrencyInfo.AddMessageError(Res.GetString("4ACBA92A-70DC-4A99-BAD6-C3D023BEF1E8", "Group, Invoice and Line Charges of the same type cannot be entered with different currencies."));
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
