using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	static class ExchangeRateHelper
	{
		internal static ZDecimal GetBaseRateAdjustedByCFX(ZString currencyCode, ZDecimal baseRate, ZDecimal cfxPercent, ICompany company)
		{
			Argument.NotNull(company, nameof(company));

			try
			{
				var result = baseRate;
				if (!cfxPercent.IsEmpty && !RefCurrency.IsExcludedCFXCalculation(currencyCode))
				{
					result = company.IsReciprocal ? baseRate * (100m + cfxPercent) / 100m : baseRate * (100m - cfxPercent) / 100m;
				}
				var decimalPlaces = company.ExchangeRateDecimalPlaces;
				return Utilities.Round(result, decimalPlaces);
			}
			catch (OverflowException)
			{
				return ZDecimal.Zero;
			}
		}

		internal static ZDecimal GetBaseRateAdjustedByCFXPercentAndMinimum(ZDecimal osSellAmt, ZString currencyCode, ZDecimal baseRate, ZDecimal cfxPercent, ZDecimal cfxMinimum, ICompany company)
		{
			Argument.NotNull(company, nameof(company));

			var result = GetBaseRateAdjustedByCFX(currencyCode, baseRate, cfxPercent, company);
			result = GetSellExRateAdjustedByCFXMinimum(osSellAmt, baseRate, result, cfxMinimum, company);
			var decimalPlaces = company.ExchangeRateDecimalPlaces;
			return Utilities.Round(result, decimalPlaces);
		}

		internal static ZDecimal GetSellExRateAdjustedByCFXMinimum(ZDecimal osSellAmt, ZDecimal baseRate, ZDecimal sellRate, ZDecimal cfxMinimum, ICompany company)
		{
			Argument.NotNull(company, nameof(company));

			var result = sellRate;
			if (!osSellAmt.IsEmpty && !sellRate.IsEmpty && !cfxMinimum.IsEmpty)
			{
				var localSellAmount = company.ExchangeRate.ForeignToLocal(osSellAmt, baseRate);
				var localSellAmountWithCFX = company.ExchangeRate.ForeignToLocal(osSellAmt, sellRate);

				if (Math.Abs(localSellAmountWithCFX - localSellAmount) < cfxMinimum)
				{
					result = company.ExchangeRate.GetRate(localSellAmount + (cfxMinimum * Math.Sign(osSellAmt)), osSellAmt);
				}
			}
			var decimalPlaces = company.ExchangeRateDecimalPlaces;
			return Utilities.Round(result, decimalPlaces);
		}

		internal static ZDecimal GetBaseRateAdjustedByCFXPercentAndMinimumFromLocalAmt(ZDecimal localSellAmt, ZString currencyCode, ZDecimal baseRate, ZDecimal cfxPercent, ZDecimal cfxMinimum, ICompany company)
		{
			Argument.NotNull(company, nameof(company));

			var result = GetBaseRateAdjustedByCFX(currencyCode, baseRate, cfxPercent, company);
			result = GetSellExRateAdjustedByCFXMinimumFromLocalAmt(localSellAmt, currencyCode, baseRate, result, cfxMinimum, company);
			return result;
		}

		internal static ZDecimal GetSellExRateAdjustedByCFXMinimumFromLocalAmt(ZDecimal localSellAmt, ZString currencyCode, ZDecimal baseRate, ZDecimal sellRate, ZDecimal cfxMinimum, ICompany company)
		{
			Argument.NotNull(company, nameof(company));

			var result = sellRate;

			if (!localSellAmt.IsEmpty && !sellRate.IsEmpty && !cfxMinimum.IsEmpty)
			{
				var osSellAmountWithCFXPercentage = company.ExchangeRate.LocalToForeign(localSellAmt, sellRate, currencyCode);

				var localSellAmountMinusCfxMin = localSellAmt - (cfxMinimum * Math.Sign(localSellAmt));
				var osSellAmountWithoutCfxMin = company.ExchangeRate.LocalToForeign(localSellAmountMinusCfxMin, baseRate, currencyCode);

				if (Math.Abs(osSellAmountWithCFXPercentage) > Math.Abs(osSellAmountWithoutCfxMin))
				{
					result = company.ExchangeRate.GetRate(localSellAmt, osSellAmountWithoutCfxMin);
				}
			}

			var decimalPlaces = company.ExchangeRateDecimalPlaces;
			return Utilities.Round(result, decimalPlaces);
		}

		internal static string PropertiesToString(this IExchangeRateJobBilling exchangeRate, int indentCharDepth = 0)
		{
			#region SuppressResourceStringsCheckRegion Used for Critical Validation and Debugging

			var indent = new string(' ', indentCharDepth);
			if (exchangeRate == null)
			{
				return indent + "null";
			}

			if (exchangeRate.IsDeleted)
			{
				return FormattableString.Invariant($@"{indent}ExchangeRatePk: {exchangeRate.ExchangeRatePk}
{indent}IsDeleted: {exchangeRate.IsDeleted.ToYesNoString()}");
			}

			return FormattableString.Invariant($@"{indent}ExchangeRatePk: {exchangeRate.ExchangeRatePk}
{indent}CurrencyCode: {exchangeRate.CurrencyCode}
{indent}Rate: {exchangeRate.Rate}
{indent}SellRate: {exchangeRate.SellRate}
{indent}OrgPk: {exchangeRate.OrgPk}
{indent}OrgType: {exchangeRate.OrgType}
{indent}CFXPercent: {exchangeRate.CFXPercent}
{indent}CFXMinimum: {exchangeRate.CFXMinimum}
{indent}IsUserDefinedOrTransformed: {exchangeRate.IsUserDefinedOrTransformed.ToYesNoString()}
{indent}IsDeleted: {exchangeRate.IsDeleted.ToYesNoString()}");

			#endregion
		}
	}
}
