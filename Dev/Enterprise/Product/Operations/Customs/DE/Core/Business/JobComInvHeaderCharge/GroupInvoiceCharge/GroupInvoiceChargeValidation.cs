using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class GroupInvoiceChargeValidation : EU.Business.Declaration.GroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge) : base(groupInvoiceCharge)
		{
		}

		protected new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;

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
			if (Parent.GroupInvoice.HasChargeCodeWithDifferentCurrency(Parent.J7_ChargeType, Parent.J7_RX_NKCurrency))
			{
				Parent.J7_RX_NKCurrencyInfo.AddMessageError(Res.GetString("72190E48-62DD-49B4-AC17-2FA65FD66874", "Charges of the same type cannot be entered with different currencies."));
			}
			ChargeValidationHelper.CheckJ7_RX_NKCurrency_010_014_HaveSame(Parent);
		}

		protected override void CheckJ7_AmountIsValidMoney()
		{
			var parent = Parent;
			var isImportDeclaration = parent.GroupInvoice?.JobDeclaration?.IsImport ?? false;
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
				Parent.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("88705B44-09D1-46EB-81F3-AE85B8CE0C18", "The Exchange Rate Date must not be in the future."));
			}
			else if (Parent.J7_ExchangeRateDate.IsEmpty && Parent.IsJ7_ExchangeRateUserEnterable)
			{
				Parent.J7_ExchangeRateDateInfo.AddMessageError(Res.GetString("8272AB3F-358D-4764-B0AD-D3A98CB178C4", "You have not entered an Exchange Rate Date."));
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
