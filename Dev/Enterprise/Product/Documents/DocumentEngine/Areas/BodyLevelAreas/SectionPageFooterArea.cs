using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class SectionPageFooterArea : FooterArea
	{
		public SectionPageFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			ShouldDelete = true;
		}

		SectionPageFooterArea() { }

		public override Area Clone(int position)
		{
			Area cloned = new SectionPageFooterArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			cloned.ShouldDelete = false;
			return cloned;
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#SectionPageFooter",
				ResString.GetMultilingualString("806af23a-bffe-4e3a-8f2d-aebdc0d7d074",
					@"Contents of this Area will be rendered at the bottom of each page above the {0} Area where the Body Section continues onto the next page.

This Area will not be rendered at the very end of a Body Section, if that's what you want use a {1} Area.",
					"#PageFooter", "#SectionFooter"));
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				int positionInAreaList = ParentReport.Analyser.Areas.IndexOf(this);
				for (int i = positionInAreaList - 1; i > 0 && !(ParentReport.Analyser.Areas[i] is PageFooterArea || ParentReport.Analyser.Areas[i] is PageFooterArea); i--)
				{
					if (ParentReport.Analyser.Areas[i] is SectionBodyArea)
					{
						SectionBodyArea sectionBody = ParentReport.Analyser.Areas[i] as SectionBodyArea;
						if (sectionBody.OwnerSection == OwnerSection)
						{
							result.Add(ParentReport.Analyser.Areas[i]);
						}
					}
				}
				return result;
			}
		}

		public override bool CanCloseAPage
		{
			get
			{
				return true;
			}
		}
	}
}
