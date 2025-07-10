using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.ValueProviders.Macros
{
	class Contains : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)Contains(?:\s*)\([\s]*""(?<Text>[^""]*?)""[\s]*,[\s]*""(?<Pattern>[^""]*?)""[\s]*\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex responsibilityRegex = new Regex(@"^<(?:\s*)Contains(?:\s*)\((?:\s*)(.*)\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<Contains(\"string\", \"subString\")>",
	ResString.GetMultilingualString("F3C28C95-257B-4601-89B5-D5C5DF877721", "Returns \"Y\" if the string contains the substring, and \"N\" if it does not, ignoring case."),
	new List<(string example, object expectedResult)> {
					((NoResString)"<Contains(\"APPLES\", \"APP\")>", new ZString("Y")),
					((NoResString)"<Contains(\"APPLES\", \"ORANGES\")>", new ZString("N"))
	});
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			if (!match.Success)
			{
				ReportMacroError(report, Res.GetString("7EA7BEF1-E7A7-45D6-9F0B-34A392937AAC", "Unexpected arguments. Contains macro only accepts two string parameters enclosed in double quotes."));
				return string.Empty;
			}

			var text = match.Groups["Text"].Value;
			var pattern = match.Groups["Pattern"].Value;
			try
			{
				return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) ? "Y" : "N";
			}
			catch (ArgumentException)
			{
				return text.Contains(pattern) ? "Y" : "N";
			}
		}

		protected override bool IsResponsibleForReplacingCore(string macro, Passes currentPass)
		{
			return responsibilityRegex.IsMatch(macro);
		}
	}
}
