using System.Collections.Generic;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class SectionFooterArea : Area
	{
		public SectionFooterArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			ShowEvenWithNoData = ContainsParam(Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
		}

		SectionFooterArea() { }

		public override Area Clone(int position)
		{
			throw new CloneAreaException("Cannot clone a #SectionFooter Area.");
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#SectionFooter[:ShowEvenWithNoData]",
				ResString.GetMultilingualString("6d9e4c9f-c113-4801-817a-4211e9e35758",
					@"Contents of this Area are rendered at the very end of a Body Section.

This Area will not be rendered at the bottom of each page where the Body Section continues onto the next page, if that's what you want use a {0} Area.

Optional Parameters:
{1} - Show this Area even if there are no rows in the Data Row Source specified by the associated {2} Area.",
					"#SectionPageFooter", "ShowEvenWithNoData", "#SectionBody"));
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				foreach (Area area in OwnerSection.GetAreasInLevel(OwnerSection.OriginalSectionBodyAndGroupByAreas.Count - 1))
				{
					result.Add(area);
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

		public readonly bool ShowEvenWithNoData;
	}
}
