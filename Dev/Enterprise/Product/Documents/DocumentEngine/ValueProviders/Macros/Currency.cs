using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class Currency : ValueProviderWithLoadControlFactory
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Currency({decimalamount},{currencycode})>",
				ResString.GetMultilingualString("68668cc2-70f1-44d3-9bbf-36c9f0a5182c", @"Formats the cell inserting the decimal amount as the value and setting the right number of decimal places for the currency specified. Please note that the currency code itself is NOT included in the output."),
				new List<(string example, object expectedResult)> {
					((NoResString)"<Currency(4.5, USD)>", new FormattedCellValue(4.5m, "#,##0.00")),
					((NoResString)"<Currency(<Amount>, <CurrencyCode>)>", new FormattedCellValue(1m, "#,##0")) });
		}

		public string GetFormatPatternForCurrency(string currencyCode)
		{
			string format = "";// DefaultFormat;

			RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
			if (currency != null)
			{
				format = "#,##0.";
				int subunitRatio = currency.RX_SubUnitRatio;
				while (subunitRatio > 1)
				{
					subunitRatio /= 10;
					format += "0";
				}

				if (format == "#,##0.")
				{
					format = "#,##0";
				}
			}
			return format;
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result;
			Match match = Regex.Match(macro);
			string currencyValueAsString = match.Groups[1].Value;
			string currencyUnitAsString = match.Groups[2].Value;

			if (string.IsNullOrEmpty(currencyUnitAsString))
			{
				currencyUnitAsString = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}

			string format = GetFormatPatternForCurrency(currencyUnitAsString);
			if (!string.IsNullOrEmpty(format))
			{
				decimal @out = 0;
				if (decimal.TryParse(currencyValueAsString, out @out))
				{
					result = new FormattedCellValue(@out, format);
				}
				else if (currencyValueAsString.StartsWith("="))
				{
					result = new TFormula(new FormattedCellValue(currencyValueAsString, format).ToString(), 0);
				}
				else
				{
					result = new FormattedCellValue(currencyValueAsString, format);
				}
			}
			else
			{
				// If we get passed in some funky amount with a comma in the number (e.g. European decimal separator) such as <Currency(6,2100,GBP)>
				// then we should just return the macro literally.  We'll return "6,2100,GBP" which is better tha nothing.
				result = macro.Replace("<Currency(", "").Replace(")>", "");

#if DEBUG
				throw new System.FormatException(string.Format(@"**DEBUG-ONLY EXCEPTION** The document engine was passed in crap, and could not format the currency.  
																	In the real world we will display this as best we can to the user. 
																	We were given {0} as the macro, and the regex of {1} pulled it into a supposedly numeric value of {2} and a 
																	supposed currency of {3}. In the real world, the user would see this as {4}",
														macro, this.Regex, currencyValueAsString, currencyUnitAsString, result));
#endif
			}

			return result;
		}

		protected override bool PassNestedMacroFormulaAsText { get { return true; } }

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)currency(?:[\s]*)\((?:[\s]*)(.*?)(?:[\s]*),(?:[\s]*)([^\s,]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
