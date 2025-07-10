using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class FeeHelper
{
	public static ZDecimal RoundChargeAmountIfNeeded(ZDecimal valueToRound)
	{
		if (valueToRound.DecimalPlaces > ChargeAmountDecimalPlaces)
		{
			return RoundChargeAmount(valueToRound);
		}

		return valueToRound;
	}

	static ZDecimal RoundChargeAmount(ZDecimal valueToRound)
	{
		var chargeAmountAbsValue = Math.Abs(valueToRound);
		if (chargeAmountAbsValue > ZDecimal.Zero && chargeAmountAbsValue < MinimumChargeAmount)
		{
			return MinimumChargeAmount * (valueToRound > ZDecimal.Zero ? 1 : -1);
		}
		return valueToRound.Round(ChargeAmountDecimalPlaces);
	}

	const decimal MinimumChargeAmount = 0.01m;
	const int ChargeAmountDecimalPlaces = 2;
}
