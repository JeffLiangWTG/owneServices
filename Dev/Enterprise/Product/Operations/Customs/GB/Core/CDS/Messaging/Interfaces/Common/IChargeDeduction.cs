using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IChargeDeduction
	{
		IAmountAndCurrency OtherChargeDeductionAmount { get; }
		ZString ChargesTypeCode { get; }
	}

	class ChargeDeductionWrapper : IChargeDeduction
	{
		ChargeDeductionWrapper(IAmountAndCurrency otherChargeDeductionAmount, ZString chargesTypeCode)
		{
			this.otherChargeDeductionAmount = otherChargeDeductionAmount;
			this.chargesTypeCode = chargesTypeCode;
		}

		public static ChargeDeductionWrapper New(IAmountAndCurrency otherChargeDeductionAmount, ZString chargesTypeCode)
		{
			return new ChargeDeductionWrapper(otherChargeDeductionAmount, chargesTypeCode);
		}

		IAmountAndCurrency IChargeDeduction.OtherChargeDeductionAmount => otherChargeDeductionAmount;

		ZString IChargeDeduction.ChargesTypeCode => chargesTypeCode;

		readonly IAmountAndCurrency otherChargeDeductionAmount;
		readonly ZString chargesTypeCode;
	}
}
