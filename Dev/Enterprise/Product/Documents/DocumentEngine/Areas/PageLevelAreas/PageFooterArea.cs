using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class PageFooterArea : FooterArea
	{
		public PageFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
		}

		PageFooterArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#PageFooter",
				ResString.GetMultilingualString("07e6e902-07d8-4ca0-8996-6a39cafb04c1",
					@"This Area will print at the very bottom of each and every page unless there's a {0}, {1} or {2} better suited to the particular page being rendered.

Contrary to popular belief, a {3} Area will print after the {4} Area (if defined) on every page the {4} Area is printed on.

All types of {3} Areas *cannot* be spread across multiple pages as they are designed to finish the page, not be part of the body. 

Please keep in mind that it's impossible to print a page when a {3} Area is too long to fit on one page when used in combination with the {5} and/or {6}.

If you want an Area at the end of your output that can be split across multiple pages, use the {4} Area instead.",
					"#FirstPageFooter", "#LastPageFooter", "#OnlyOnePageFooter", "#PageFooter", "#DocumentFooter",
					"#DocumentHeader", "#PageHeader"));
		}

		public override Area Clone(int position)
		{
			Area cloned = new PageFooterArea(position, position + fEnd - fStart, ParentReport, (NoResString)"#PageFooter");
			CopyCommonMembers(cloned);
			cloned.ShouldDelete = false;
			return cloned;
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override int FooterRankForVisualisation
		{
			get { return 1; }
		}
	}
}
