using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public interface IRoundingMethodApplier
	{
		ZDecimal ApplyRounding(ZString roundingMethod, ZDecimal amount, ZBool isAmountInForeignCurrency, ZInt amountCurrencyDecimalPlaces);
	}

	public class RoundingMethodApplier : IRoundingMethodApplier
	{
		ZDecimal IRoundingMethodApplier.ApplyRounding(ZString roundingMethod, ZDecimal amount, ZBool isAmountInForeignCurrency, ZInt amountCurrencyDecimalPlaces)
		{
			if (roundingMethod.IsEmpty)
			{
				throw new ArgumentException("Invalid argument, roundingMethod argument is empty.");
			}

			if (amountCurrencyDecimalPlaces < 0)
			{
				throw new ArgumentException("Invalid argument, amountCurrencyDecimalPlaces must be >= 0");
			}

			if (roundingMethod == TaxAmountRoundingMethods.Standard.Code)
			{
				return Utilities.Round(amount, amountCurrencyDecimalPlaces);
			}
			else if (roundingMethod == TaxAmountRoundingMethods.RoundDownToMinorUnit.Code)
			{
				if (isAmountInForeignCurrency)
				{
					return Utilities.Round(amount, amountCurrencyDecimalPlaces);
				}

				ZDecimal multiplier = Math.Pow(10, amountCurrencyDecimalPlaces);
				return decimal.Truncate(amount * multiplier) / multiplier;
			}
			else
			{
				throw new ArgumentException(string.Format("Invalid argument, \"{0}\" is not a valid rounding method", roundingMethod));
			}
		}
	}
}
