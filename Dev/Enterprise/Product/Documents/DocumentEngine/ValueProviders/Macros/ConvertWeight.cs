using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class ConvertWeight : ValueProvider
	{
		static readonly Regex regex = new Regex(@"^<[\s]*ConvertWeight[\s]*\([\s]*'(?<Value>[^']*)'[\s]*,[\s]*'(?<UnitFrom>[^']*)'[\s]*,[\s]*'(?<UnitTo>[^']*)'[\s]*(,[\s]*(?<DecimalPlaces>\d+)[\s]*)?\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly string unitDescription = new StringBuilder().AppendLine(string.Join(System.Environment.NewLine, Weight.Codes.Select(code => $"{code}: {Weight.GetDescription(code, PluralState.NonPlural)}"))).ToString();
		static readonly int defaultDecimalPlaces = 3;
		static readonly int maxDecimalPlaces = 10;

		public override Regex Regex => regex;

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<ConvertWeight('{Value}', '{UnitFrom}', '{UnitTo}' [,{DecimalPlaces}])>",
ResString.GetMultilingualString("1b9756f4-22a5-4b84-8bf2-f22d5bb04933", @"Convert weight unit.

{0}: The decimal value to convert
{1}: The unit of the value
{2}: The unit to convert to
{3}: (optional) The decimal places to round the result to, default is {4}.

Supported Units
{5}", "Value", "UnitFrom", "UnitTo", "DecimalPlaces", defaultDecimalPlaces, unitDescription),
				new List<(string example, object expectedResult)>
				{
					((NoResString)"<ConvertWeight('1', 'KG', 'HG', 1)>", (NoResString)"10.0"),
					((NoResString)"<ConvertWeight('1', 'HG', 'KG')>", (NoResString)"0.100")
				});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var result = string.Empty;
			var match = regex.Match(macro);
			if (match.Success)
			{
				var value = match.Groups["Value"].Value;
				var unitFrom = match.Groups["UnitFrom"].Value.ToUpper();
				var unitTo = match.Groups["UnitTo"].Value.ToUpper();
				var decimalPlaces = defaultDecimalPlaces;

				var isDecimalPlacesValid = true;
				if (match.Groups["DecimalPlaces"].Success)
				{
					isDecimalPlacesValid = int.TryParse(match.Groups["DecimalPlaces"].Value, out decimalPlaces) && decimalPlaces >= 0 && decimalPlaces <= maxDecimalPlaces;
				}

				if (!isDecimalPlacesValid)
				{
					ReportMacroError(report, string.Format((NoResString)"DecimalPlaces should be an integer within the range [0, {0}] but is {1}.", maxDecimalPlaces, match.Groups["DecimalPlaces"].Value));
				}
				else if (!decimal.TryParse(value, out var valueDecimal))
				{
					ReportMacroError(report, string.Format((NoResString)"Couldn't parse {0} to Decimal.", value));
				}
				else if (!Weight.ContainsCode(unitFrom))
				{
					ReportMacroError(report, string.Format((NoResString)"{0} is not a weight unit.", unitFrom));
				}
				else if (!Weight.ContainsCode(unitTo))
				{
					ReportMacroError(report, string.Format((NoResString)"{0} is not a weight unit.", unitTo));
				}
				else
				{
					var convertedValue = Weight.Convert(valueDecimal, unitFrom, unitTo, applyDefaultRounding: false);
					result = Utilities.Round(convertedValue, decimalPlaces).ToString($"F{decimalPlaces}");
				}
			}
			return result;
		}
	}
}
