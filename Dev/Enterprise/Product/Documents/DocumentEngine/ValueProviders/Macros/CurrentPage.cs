using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CurrentPage : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CurrentPage[({StartingPageNumber})]>",
				ResString.GetMultilingualString("3cce089b-0ba1-4ea9-84a4-d4eaacfe331e", @"Returns the current page number, optionally offset with the starting page number.
See also: {0}.", "TotalPages"),
				new List<(string example, object expectedResult)> {
					("<CurrentPage>", 3),
					("<CurrentPage(5)>", 7) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);

			if (match.Groups[2].Success && int.TryParse(match.Groups[2].Value, out var pagenumber) && pagenumber > 1)
			{
				return report.Renderer.CurrentPageNumber + pagenumber - 1;
			}

			return report.Renderer.CurrentPageNumber;
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex(@"^<\s*Current\s*Page\s*(\(\s*([^\s]*)\s*\))?\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
