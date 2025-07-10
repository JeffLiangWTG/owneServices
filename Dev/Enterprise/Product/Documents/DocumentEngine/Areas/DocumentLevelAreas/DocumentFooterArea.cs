using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class DocumentFooterArea : Area
	{
		public DocumentFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			fSplitIfNotFitInAPage = !ContainsParam(Constants.DocumentFooterParameters.DontSplitSignature);
		}
		readonly bool fSplitIfNotFitInAPage;

		DocumentFooterArea() { }

		public override bool SplitIfNotFitInAPage
		{
			get { return fSplitIfNotFitInAPage; }
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"#DocumentFooter[:Don't Split]",
				ResString.GetMultilingualString("636ff1ce-cad9-431a-a6b8-18aedf834e02",
					@"The {0} Area prints after the last Body Section, but before the last Page Footer Area regardless of which Page Footer type that ends up being.

This means that although the {0} will certainly end on the last page, it will not necessarily be the last thing showing at the *BOTTOM* of the last page. If that's what you're after, use the {1} Area instead.

Generally this Area is used for signature Areas on notification Documents or for grand total Areas on Reports

You can include the optional '{2}' parameter to tell the Document Engine to keep all rows in this Area together so that you don't get half the Document Footer on one page and half on the next. This parameter is both case and space sensitive, so make sure it's entered character for character including the space between Don't and Split.

Please note that you can also keep rows in an Area together by merging the cells in a column vertically across the rows you want to keep together. The Document Engine know you can't split merged cells and will deal with this appropriately. This works in all Areas, not just the {0}.",
					"#DocumentFooter", "#LastPageFooter", ":Don't Split"));
		}

		public override Area Clone(int position)
		{
			Area cloned = new DocumentFooterArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			return cloned;
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				foreach (Section section in ParentReport.Analyser.Sections)
				{
					if (section.SectionFooter != null)
					{
						result.Add(section.SectionFooter);
					}
				}
				return result;
			}
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}
	}
}
