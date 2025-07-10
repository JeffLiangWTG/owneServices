using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class TotalPages : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TotalPages>",
				ResString.GetMultilingualString("ef75ad47-d4d4-44ca-a367-312c383c3497", @"Returns the total number or count of pages in the final output. 
See also: {0}.", "CurrentPage"),
				new List<(string example, object expectedResult)> { ("<TotalPages>", 2) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return report.Renderer.TotalNumberOfPages;
		}

		public override Passes PassToStartReplacingOn
		{
			get { return Passes.SecondPass; }
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)TotalPages(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
