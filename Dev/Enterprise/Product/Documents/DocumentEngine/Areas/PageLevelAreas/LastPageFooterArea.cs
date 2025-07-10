namespace Enterprise.DocumentEngine.Areas
{
	sealed class LastPageFooterArea : FooterArea
	{
		public LastPageFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			ShouldDelete = true;
		}

		LastPageFooterArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#LastPageFooter",
				ResString.GetMultilingualString("240077b0-1545-4fdc-8413-fac069e96e48",
					@"Will always be rendered at the very bottom of the last page in the generated output unless there is an {0} Area and the generated output has one page.

This means that a {1} will be used in preference on the last page over a {2} or a {3} even in a one page result.",
					"#OnlyOnePageFooter", "#LastPageFooter", "#FirstPageFooter", "#PageFooter"));
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override Area Clone(int position)
		{
			Area cloned = new LastPageFooterArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			cloned.ShouldDelete = false;
			return cloned;
		}

		public override int FooterRankForVisualisation
		{
			get { return 2; }
		}
	}
}
