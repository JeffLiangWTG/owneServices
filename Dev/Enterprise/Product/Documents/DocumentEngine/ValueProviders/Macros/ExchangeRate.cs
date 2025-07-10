using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ExchangeRate : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ExchangeRate({exchangeratedecimalvalue})>",
				ResString.GetMultilingualString("25339320-874f-4464-8ab2-2ddf67d53dfe", @"Will format the supplied exchange rate with the right number of decimal places depending on what 'Is Reciprocal Exchange Rate' is set to on the Current Company."),
				new List<(string example, object expectedResult)> { ("<ExchangeRate(<ExRate>)>", new FormattedCellValue(1.2m, "0.000000")) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var exchangeRateValueAsString = match.Groups[1].Value;
			var format = "0.000000";

			object result;
			if (decimal.TryParse(exchangeRateValueAsString, out var exchangeRateValue))
			{
				result = new FormattedCellValue(exchangeRateValue, format);
			}
			else
			{
				result = "";

				if (!string.IsNullOrEmpty(exchangeRateValueAsString))
				{
					ReportMacroError(report, Res.GetString("9E95D6FF-0203-4559-8D17-DB63DD5A60CC", "Couldn't parse {0} to Decimal.", exchangeRateValueAsString));
				}
			}
			return result;
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Exchange(?:[\s]*)Rate(?:[\s]*)\((?:[\s]*)([^\s]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
