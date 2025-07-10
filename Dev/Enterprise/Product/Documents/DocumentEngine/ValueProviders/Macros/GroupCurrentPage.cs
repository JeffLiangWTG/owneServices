using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class GroupCurrentPage : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<GroupCurrentPage({GroupByField})>",
				ResString.GetMultilingualString("cf323aa1-d0ed-4aa7-9419-aed24823cf38", @"Gives the current page of the group.
See also {0}",
"GroupTotalPages"),
				new List<(string example, object expectedResult)> { ("<GroupCurrentPage(GST)>", 2) });
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
				result = sectionBody != null ? (report.Renderer.CurrentAreaToProcess.RenderedToPageNumber - sectionBody.RenderedToPageNumber) + 1 : 0;
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
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)GroupCurrentPage(?:[\s]*)\((?:[\s]*)([^\s,.]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
