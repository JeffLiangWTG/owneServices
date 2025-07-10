using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.ValueProviders;
using Enterprise.NumberFountain;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class SSCCCheckDigit : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SSCCCheckDigit('{StringValue}')>",
				ResString.GetMultilingualString("7eb7a444-f3de-4472-8d89-b0bcdce4c402", @"Is used to calculate the check digit from a barcode number."),
				new List<(string example, object expectedResult)> { ("<SSCCCheckDigit('34012345123458789')>", "3") });
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string barcodeNumber = match.Groups["barcode"].ToString().Trim();
			if (CheckIsAllDigit(barcodeNumber))
			{
				return SSCCBarCodeChecker.GetCheckDigit(barcodeNumber).ToString();
			}
			else
			{
				MacroErrorReporter.ReportFieldNotFound(macro, report);
				return macro;
			}
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		bool CheckIsAllDigit(string barcode)
		{
			double result;
			return double.TryParse(barcode, out result);
		}

		static readonly Regex fRegex = new Regex(@"^<[\s]*SSCCCheckDigit[\s]*\((?:\s*)(?:')(?<barcode>(?:\s*).+(?:\s*))(?:')(?:\s*)\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
