namespace Enterprise.DocumentEngine.Areas
{
	sealed class OnlyOnePageFooterArea : FooterArea
	{
		public OnlyOnePageFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			ShouldDelete = true;
		}

		OnlyOnePageFooterArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#OnlyOnePageFooter",
				ResString.GetMultilingualString("406d3a7a-fc68-4fe2-b053-2f5cf8459efb",
					@"This Area will be shown at the very bottom of the first page if there's one page only in the generated output.

If there's more than one page, this Area will not be rendered. 

This will override the {0} and {1} when there's only one page.",
					"#LastPageFooter", "#PageFooter"));
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override Area Clone(int position)
		{
			Area cloned = new OnlyOnePageFooterArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			cloned.ShouldDelete = false;
			return cloned;
		}

		public override int FooterRankForVisualisation
		{
			get { return 3; }
		}
	}
}
