using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GroupTotalPages : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<GroupTotalPages({GroupByField})>",
				ResString.GetMultilingualString("8c6d3191-fd33-4d83-acd6-09cf59c575ec", @"Gives the total pages count of the group. 
See also {0}.", "GroupCurrentPage"),
				new List<(string example, object expectedResult)> { ("<GroupTotalPages(GST)>", 2) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			int result = 0;
			string groupByField = Regex.Match(macro).Groups[1].Value;

			GroupByArea matchedGroupArea = null;

			report.Renderer.Pages.FindIndex(report.Renderer.CurrentAreaToProcess.RenderedToPageNumber - 1, page =>
			{
				matchedGroupArea = (GroupByArea)page.Areas.Find(area =>
				{
					var groupByArea = area as GroupByArea;
					return groupByArea != null && string.Concat(groupByArea.GroupByColumns).Contains(groupByField);
				});

				return matchedGroupArea != null;
			});

			if (matchedGroupArea != null)
			{
				var sectionBody = matchedGroupArea.OwnerSection.SplitSectionBodyAreaInstances.Find(sb => sb.Equals(matchedGroupArea.SectionBody));
				result = sectionBody != null ? (matchedGroupArea.RenderedToPageNumber - sectionBody.RenderedToPageNumber) + 1 : 0;
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
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)GroupTotalPages(?:[\s]*)\((?:[\s]*)([^\s,.]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
