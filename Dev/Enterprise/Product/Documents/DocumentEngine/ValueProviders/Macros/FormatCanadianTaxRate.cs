using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class FormatCanadianTaxRate : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<FormatCanadianTaxRate({RateType}, {Rate})>",
ResString.GetMultilingualString("149b2d99-0c15-471a-ba7b-a9b2c406e736", @"Return the formatted Canadian tax rate for a given rate type, i.e. {0}(valorem) or {1}(specific).
Other rate types are formatted as {1}(specific)", "V", "S"),
				new List<(string example, object expectedResult)> {
					("<FormatCanadianTaxRate(<RateType>, <RateValue>)>", "5.00") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var rateType = match.Groups["ratetype"].Value;
			var rateString = match.Groups["rate"].Value;
			return TryParseWithReportOnFail(
				report: report,
				defaultValue: string.Empty,
				parseFunc: () =>
				{
					decimal rate = 0;
					var result = string.Empty;
					if (decimal.TryParse(rateString, out rate))
					{
						if (rate != 0)
						{
							if (rateType == "V")
							{
								result = rate.ToString("#,##0.0");
							}
							else
							{
								result = rate.ToString("#,##0.00#######");
							}
						}
					}
					return result;
				});
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)FormatCanadianTaxRate(?:[\s]*)\((?:[\s]*)(?<ratetype>[^,)]*)(?:[\s]*),(?:[\s]*)(?<rate>[^)]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
