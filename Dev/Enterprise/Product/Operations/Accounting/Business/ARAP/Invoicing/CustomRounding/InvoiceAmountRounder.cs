using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IInvoiceAmountRounder
	{
		ZDecimal ApplyCustomRounding(ZDecimal amount, ZString currencyCode, InvoiceRoundingOption roundingOption, InvoiceRoundCurrencyUnit roundCurrencyUnit, Action<string> reportUnexpectedError);
	}

	class InvoiceAmountRounder : IInvoiceAmountRounder
	{
		ZDecimal IInvoiceAmountRounder.ApplyCustomRounding(ZDecimal amount, ZString currencyCode, InvoiceRoundingOption roundingOption, InvoiceRoundCurrencyUnit roundCurrencyUnit, Action<string> reportUnexpectedError)
		{
			if (amount.IsInteger)
			{
				return 0M;
			}

			var roundedAmount = Utilities.Round(amount, 2);
			if (amount != roundedAmount && reportUnexpectedError != null)
			{
				reportUnexpectedError(FormattableString.Invariant($"amount {amount} has incorrect decimal places with currency {currencyCode}."));
			}

			if (roundingOption == InvoiceRoundingOption.RBM)
			{
				return Utilities.Round(roundedAmount, 0) - roundedAmount;
			}

			var currencyUnit = (int)roundCurrencyUnit;
			var roundedAmountDecimal = roundedAmount - Math.Truncate(roundedAmount);
			var decimalAmount = Math.Truncate(roundedAmountDecimal * 100);
			var remainder = decimalAmount % currencyUnit;
			if (remainder == 0)
			{
				return 0M;
			}

			var roundingAmount = 0M;
			if (roundingOption == InvoiceRoundingOption.ARU)
			{
				roundingAmount = (currencyUnit - remainder) / 100;
			}
			else if (roundingOption == InvoiceRoundingOption.ARD)
			{
				roundingAmount = (remainder / 100) * -1;
			}

			return roundingAmount;
		}
	}
}