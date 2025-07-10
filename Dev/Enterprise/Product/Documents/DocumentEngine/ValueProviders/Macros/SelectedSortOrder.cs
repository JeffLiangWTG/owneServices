using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class SelectedSortOrder : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<SelectedSortOrder>",
				ResString.GetMultilingualString("a367d61d-6d31-47dd-9fd9-f2dbe4cefd8c", "Returns the selected 'Sort Order' option for Reports."),
				new List<(string example, object expectedResult)> { ("<SelectedSortOrder>", (NoResString)"Expiry Date") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			return report.SortOrderCollection.SelectedOrder.DisplayNameLocalized;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Selected(?:[\s]*)Sort(?:[\s]*)Order(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
