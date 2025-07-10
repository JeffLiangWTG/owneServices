using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class SelectedGroupbyOption : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SelectedGroupbyOption>",
				ResString.GetMultilingualString("bc43c6e4-ba85-46ba-9c07-8a4919279f27", "Returns the selected 'Group By' option for Reports."),
				new List<(string example, object expectedResult)> { ("<SelectedGroupbyOption>", (NoResString)"Expiry Date") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return report.GroupByCollection.SelectedGroupBy.DisplayNameLocalized;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<\s*selected\s*groupby\s*option\s*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
