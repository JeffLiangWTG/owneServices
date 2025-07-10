using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class FirstPageFooterArea : FooterArea
	{
		public FirstPageFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			ShouldDelete = true;
		}

		FirstPageFooterArea() { }

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#FirstPageFooter",
				ResString.GetMultilingualString("e69e62ba-02f4-4b5a-a7b3-e9db1a5f4eb3",
					@"Is rendered at the very bottom of the first page when there's more than one page in the generated output.

This means that a {0} or {1} will be used in preference over a {2} for a one page result.

If you want an Area to be rendered at the bottom of the first page always, you'll need to include an {3} *and* a {2} Area with the contents repeated where necessary.",
					"#LastPageFooter", "#PageFooter", "#FirstPageFooter", "#OnlyOnePageFooter"));
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override Area Clone(int position)
		{
			Area cloned = new FirstPageFooterArea(position, position + fEnd - fStart, ParentReport, (NoResString)"#FirstPageFooter");
			CopyCommonMembers(cloned);
			cloned.ShouldDelete = false;
			return cloned;
		}
	}
}
