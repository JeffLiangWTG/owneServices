using System.Collections.Generic;
using Enterprise.DocumentEngine.Renderer;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class SectionHeaderArea : Area
	{
		public SectionHeaderArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			ShowEvenWithNoData = ContainsParam(Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
			fBreakPage = ContainsParam(Constants.CommonAreaParameters.PageBreakSignature);
			Sticky = ContainsParam(Constants.CommonAreaParameters.Sticky);
		}

		SectionHeaderArea() { }

		readonly bool Sticky;
		public override Area Clone(int position)
		{
			var sectionHeader = new SectionHeaderArea(position, position + fEnd - fStart, ParentReport, AreaHeaderText);
			CopyCommonMembers(sectionHeader);
			return sectionHeader;
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				return result;
			}
		}

		public override bool CanCloseAPage
		{
			get { return false; }
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"#SectionHeader[:ShowEvenWithNoData][:PageBreak][:Sticky]",
				ResString.GetMultilingualString("ea53baea-4aab-4b65-ab67-889d9357a141",
					@"A {0} Area shows once at the beginning of a Body Section and once only. 

This Area will not repeat if the Body Section flows over onto the next page, if you want a header to print on follow pages you will need to use a {1} Area as well.

A Body Section may comprise of any combination of the Areas {0}, {1}, {2}, {3}, {4} and {5} with the following conditions:
1. You can only have one of each Area type in each Body Section except for the {3} Area which may be defined many times where {3} headers and footers are required, or nested levels of grouping are required.
2. Any Areas specified *must* be in the order specified or the Document Engine will see them as part of the next Body Section.
3. You can have multiple Body Sections, even operating out of the same collections of tables.
4. Each Body Section will use the data source defined in the parameters on the {2} for all Areas in that Body Section.

Optional Parameters:
{6} - Will cause the contents of the {0} Area to show even if there are no rows in the data source specified by the {2} Area.
{7} - Makes the Body Section start on a new page.
{8} - Will make sure that the {0} Area won't print on a page if there's not enough room to print the first row of the {2} Area with it.",
					"#SectionHeader", "#SectionPageHeader", "#SectionBody", "#GroupBy", "#SectionPageFooter", "#SectionFooter",
					"ShowEvenWithNoData", "PageBreak", "Sticky"));
		}

		public override bool FitsInPage(int heightAvailableInPage, Page page)
		{
			bool result = base.FitsInPage(heightAvailableInPage, page);
			if (result)
			{
				if (Sticky)
				{
					if (OwnerSection.SectionBody.GetRowsToKeep(heightAvailableInPage - HeightInXls, page) == 0)
					{
						result = false;
					}
				}
			}

			return result;
		}

		public readonly bool ShowEvenWithNoData;

		readonly bool fBreakPage;

		public override bool BreakPage
		{
			get
			{
				return fBreakPage;
			}
		}

		public override bool SplitIfNotFitInAPage => true;
		public override bool TryHardPutInOnePage => true;
	}
}
