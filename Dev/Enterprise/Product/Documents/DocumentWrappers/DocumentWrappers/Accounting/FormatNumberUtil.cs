using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public static class FormatNumberUtil
	{
		public static string FormatAmountWithCurrentCompanysCulture(IFormattable amount, ICurrency currency)
		{
			NumberFormatInfo noCurrencySymbolFormat = (NumberFormatInfo)Culture.CurrentCompanyCountryCulture.NumberFormat.Clone();
			noCurrencySymbolFormat.CurrencySymbol = "";
			if (currency != null)
			{
				noCurrencySymbolFormat.CurrencyDecimalDigits = currency.Decimals;
			}
			return amount.ToString("C", noCurrencySymbolFormat).Trim();
		}

		public static ZString FormatRateWithCurrentCompanysCulture(ZDecimal rate)
		{
			NumberFormatInfo percentFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;
			percentFormat.PercentPositivePattern = 1;
			percentFormat.PercentDecimalDigits = rate.DecimalPlaces;
			return (rate / 100).ToString("P", percentFormat);
		}
	}

	#region Test
#if DEBUG
	public sealed class CurrentCompanyCountryCultureChanger : IDisposable
	{
		public CurrentCompanyCountryCultureChanger(string countryCode)
		{
			cultureChange = Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(countryCode));
		}

		public void Dispose()
		{
			cultureChange.Dispose();
		}

		readonly IDisposable cultureChange;
	}
#endif
	#endregion
}
