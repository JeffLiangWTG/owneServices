using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FormatNumber : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FormatNumber({value},{currency code})>",
				ResString.GetMultilingualString("77d9e227-73d8-4ef7-906a-942cfdf556a8", @"Will format the supplied numeric value with respect to the login company's culture number format settings.
Replace {0} with a positive integer to force the number of decimals,
or a negative integer to decide the number of decimals based on the current login company's reciprocal nature.

Note: this macro will return a formatted representation of the number as a STRING and will be treated as such by Excel. It won't be usable for calculation in Excel.
If you just need the value to respect the number of decimals from the currency and want to do calculation with it, use the {1} macro instead.",
"{currency code}", "<Currency(...)>"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<FormatNumber(<Amount>, <CurrencyCode>)>", "12,412,351.33"),
					((NoResString)"<FormatNumber(<Amount>, <DecimalPlaces>)>", "12,412,351.3"),
					("<FormatNumber(12412351.33, 1)>", "12,412,351.3")
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var valueAsString = match.Groups["Value"].Value;
			var secondParam = match.Groups["CurrencyCode"].Value;

			var success = decimal.TryParse(valueAsString, out decimal value);
			if (!success)
			{
				//Double.TryParse handles scientific notation by default.
				success = double.TryParse(valueAsString, out double value2);
				if (success)
				{
					value = (decimal)value2;
				}
			}

			if (success)
			{
				int decimalsPlaces;

				if (int.TryParse(secondParam, out int decimals))
				{
					if (decimals > 99)
					{
						ReportMacroError(report, Res.GetString("85F8F103-6266-4A00-A036-7B63DCFA342D", "Decimal places can not be greater than 99."));
						return null;
					}

					decimalsPlaces = decimals < 0 ? GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces : decimals;
				}
				else
				{
					var currency = RefCurrency.LoadFromCurrencyCode(Factory, secondParam) ?? GlbCompany.CurrentCompany.LocalCurrency;
					decimalsPlaces = currency.Decimals;
				}

				return FormatAmount(value, decimalsPlaces);
			}

			return valueAsString;
		}

		string FormatAmount(IFormattable amount, int noOfDecimals)
		{
			var noCurrencySymbolFormat = (NumberFormatInfo)Culture.CurrentCompanyCountryCulture.NumberFormat.Clone();
			noCurrencySymbolFormat.CurrencySymbol = "";
			noCurrencySymbolFormat.CurrencyDecimalDigits = noOfDecimals;

			return amount.ToString("C", noCurrencySymbolFormat).TrimEnd();
		}

		public override Regex Regex => fRegex;

		static readonly Regex fRegex = new Regex(@"^<(?:\s*)Format(?:\s*)Number(?:\s*)\((?:\s*)(?<Value>.*)(?:\s*),(?:\s*)(?<CurrencyCode>\S*)(?:\s*)\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
