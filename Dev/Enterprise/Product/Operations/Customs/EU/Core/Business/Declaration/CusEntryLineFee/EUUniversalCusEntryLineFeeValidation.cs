using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class EUUniversalCusEntryLineFeeValidation : CusEntryLineFeeValidation
	{
		public EUUniversalCusEntryLineFeeValidation(AutoCusEntryLineFee parent)
			: base(parent)
		{
		}

		protected override void CheckNationalFeeTypeCode()
		{
			if (Parent.Lookups.NationalFeeTypeCodeList.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.NationalFeeTypeCodeInfo);
			}
		}

		protected override void CheckCF_BaseValue()
		{
			base.CheckCF_BaseValue();
			var baseInfo = Parent.CF_BaseValueInfo;

			CheckBaseValueZeroOrNegative(baseInfo);
			CheckBaseValuePrecision(baseInfo);
		}

		void CheckBaseValueZeroOrNegative(ZPropertyInfo baseValueInfo)
		{
			if (!Parent.AllowZeroOrEmptyAmount)
			{
				MandatoryValidation.MessageErrorIfNotEntered(baseValueInfo);
			}

			if (!Parent.AllowNegativeAmount)
			{
				MandatoryValidation.MessageErrorIfIsNegative(baseValueInfo);
			}
		}

		protected virtual ZInt DecimalPlacesAllowedForBaseValuePrecision => 2;

		protected virtual void CheckBaseValuePrecision(ZPropertyInfo baseValueInfo)
		{
			var baseValue = (ZDecimal)baseValueInfo.Value;
			if (baseValue.DecimalPlaces > DecimalPlacesAllowedForBaseValuePrecision)
			{
				baseValueInfo.AddMessageError(Res.GetString("45FF03C9-87D9-44D3-8D26-EB6B024B87D9", "Base Amount allows only {0} decimal places.", DecimalPlacesAllowedForBaseValuePrecision));
			}
		}

		protected override void CheckCF_ChargeAmount()
		{
			base.CheckCF_ChargeAmount();

			if (!Parent.AllowNegativeAmount)
			{
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CF_ChargeAmountInfo);
			}
		}

		protected override void CheckCF_ChargeType()
		{
			base.CheckCF_ChargeType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CF_ChargeTypeInfo);
		}

		protected override void CheckCF_MethodOfPayment()
		{
			base.CheckCF_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.CF_MethodOfPaymentInfo);
		}

		protected override void CheckCF_Rate()
		{
			base.CheckCF_Rate();
			if (Parent.CF_Rate.DecimalPlaces > CusEntryLineFee.TaxRateDecimalPrecision)
			{
				Parent.CF_RateInfo.AddMessageError(Res.GetString("09A96FF1-8C0D-43FA-A422-73040F8A35DC", "Tax Rate allows only {0} decimal places.", CusEntryLineFee.TaxRateDecimalPrecision));
			}
		}

		protected override void CheckCF_RateOverrideReasonCode()
		{
			base.CheckCF_RateOverrideReasonCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.CF_RateOverrideReasonCodeInfo);
		}
	}
}
