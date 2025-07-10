using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportInvoiceLineChargeValidation : InvoiceLineChargeValidation
	{
		public ImportInvoiceLineChargeValidation(InvoiceLineCharge parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateIsJ7_ExchangeRateIATA();
			ValidateIsJ7_ExchangeRateUserEnterable();
		}

		protected override void CheckJ7_RX_NKCurrency()
		{
			base.CheckJ7_RX_NKCurrency();
			if (!Parent.J7_ChargeType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.J7_RX_NKCurrencyInfo);
			}
			ChargeValidationHelper.CheckJ7_RX_NKCurrency_010_014_HaveSame(Parent);
		}

		protected override void CheckJ7_Amount()
		{
			base.CheckJ7_Amount();
			if (!Parent.J7_ChargeType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.J7_AmountInfo, Res.GetString("25461337-3CD3-4867-A465-EA57A1001CED", "Amount"));
			}
		}

		protected override void CheckJ7_AmountIsValidMoney()
		{
			if (Parent.J7_ChargeType == ImportChargeCodeList.Codes.AIR)
			{
				TypeValidation.CheckValidDecimal(Parent.J7_AmountInfo, 11, 2);
			}
		}

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();

			var parent = Parent;
			var chargeType = parent.J7_ChargeType;
			var propertyInfo = parent.J7_ChargeTypeInfo;
			var charges = parent.InvoiceLine.Charges.Cast<InvoiceLineCharge>();
			if (charges.Count(x => x.J7_ChargeType == chargeType) > 1)
			{
				var message = Res.GetString("AD73C7A3-89A2-4391-9BBD-3ECFCE66A162", "A charge code of type '{0}' has already been entered.", chargeType);
				if (chargeType == Common.CustomsChargeTypeList.Codes.Discount)
				{
					propertyInfo.AddError(message);
				}
				else
				{
					propertyInfo.AddMessageError(message);
				}
			}
			if (chargeType.In(new ZString[] { ImportChargeCodeList.Codes._010, ImportChargeCodeList.Codes._014 }) && charges.Any(x => x.J7_ChargeType == ImportChargeCodeList.Codes.AIR))
			{
				propertyInfo.AddMessageError(Res.GetString("411464A2-D3BF-4072-A223-E9716D14C2D6", "Charge Codes '010' and '014' are not allowed in combination with Charge Code 'AIR'."));
			}
			ChargeValidationHelper.CheckJ7_ChargeType_010_014_EachOtherRequired(Parent);
		}

		protected override void CheckJ7_ExchangeRateDate()
		{
			base.CheckJ7_ExchangeRateDate();
			ChargeValidationHelper.CheckJ7_ExchangeRateDate_010_014_HaveSame(Parent);
		}

		protected override void CheckChargeTypeShouldNotHaveErrors(ZPropertyInfo chargeTypeInfo)
		{
			if (Parent.J7_ChargeType != Common.CustomsChargeTypeList.Codes.Discount)
			{
				base.CheckChargeTypeShouldNotHaveErrors(chargeTypeInfo);
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
