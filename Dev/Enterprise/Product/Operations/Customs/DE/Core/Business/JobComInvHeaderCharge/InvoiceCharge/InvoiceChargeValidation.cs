using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class InvoiceChargeValidation : EU.Business.Declaration.InvoiceChargeValidation
	{
		public InvoiceChargeValidation(InvoiceCharge parent) : base(parent)
		{
		}

		protected new InvoiceCharge Parent => (InvoiceCharge)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateIsJ7_ExchangeRateIATA();
			ValidateIsJ7_ExchangeRateUserEnterable();
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			ChargeValidationHelper.CheckJ7_ChargeType_010_014_EachOtherRequired(Parent);
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			var chargeCode = Parent.J7_ChargeType;
			var chargeCurrency = Parent.J7_RX_NKCurrency;
			var invoice = Parent.Invoice;
			if (invoice != null && (invoice.HasChargeCodeWithDifferentCurrency(chargeCode, chargeCurrency) || invoice.GroupHeader.HasChargeCodeWithDifferentCurrency(chargeCode, chargeCurrency)))
			{
				Parent.J7_RX_NKCurrencyInfo.AddMessageError(Res.GetString("169B1E61-D14D-43F6-96BC-4336C538E9CD", "Group and Invoice Charges of the same type cannot be entered with different currencies."));
			}
			ChargeValidationHelper.CheckJ7_RX_NKCurrency_010_014_HaveSame(Parent);
		}

		protected override void CheckJ7_AmountIsValidMoney()
		{
			var parent = Parent;
			var isImportDeclaration = parent.Invoice?.IsImport ?? false;
			if (isImportDeclaration && parent.J7_ChargeType == ImportChargeCodeList.Codes.AIR)
			{
				TypeValidation.CheckValidDecimal(parent.J7_AmountInfo, 11, 2);
			}
			else
			{
				base.CheckJ7_AmountIsValidMoney();
			}
		}

		protected override void CheckJ7_ExchangeRateDate()
		{
			base.CheckJ7_ExchangeRateDate();
			if (Parent.J7_ExchangeRateDate > ZDate.Today)
			{
				Parent.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("D75AC657-6EBF-4409-B1AD-1C0873C6E217", "The Exchange Rate Date must not be in the future."));
			}
			else if (Parent.J7_ExchangeRateDate.IsEmpty && Parent.IsJ7_ExchangeRateUserEnterable)
			{
				Parent.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("9A093B6B-E5FD-495C-88ED-BBB57BDF1F4D", "You have not entered an Exchange Rate Date."));
			}
			base.CheckJ7_ExchangeRateDate();
			ChargeValidationHelper.CheckJ7_ExchangeRateDate_010_014_HaveSame(Parent);
		}

		protected override void CheckJ7_IsGSTApplicable()
		{
			if (!Parent.IsFreightChargeToEUBorderByAirInsideEU())
			{
				base.CheckJ7_IsGSTApplicable();
			}
		}

		public void ValidateIsJ7_ExchangeRateIATA()
		{
			ValidateCalculatedProperty(Parent.IsJ7_ExchangeRateIATAInfo);
		}

		protected virtual void CheckIsJ7_ExchangeRateIATA()
		{
			ChargeValidationHelper.CheckIsJ7_ExchangeRateIATA_010_014(Parent, x => x.IsJ7_ExchangeRateIATAInfo);
		}

		public void ValidateIsJ7_ExchangeRateUserEnterable()
		{
			ValidateCalculatedProperty(Parent.IsJ7_ExchangeRateUserEnterableInfo);
		}

		protected virtual void CheckIsJ7_ExchangeRateUserEnterable()
		{
			base.CheckJ7_ExchangeRateType();
			ChargeValidationHelper.CheckIsJ7_ExchangeRateUserEnterable_010_014(Parent);
		}
	}
}
