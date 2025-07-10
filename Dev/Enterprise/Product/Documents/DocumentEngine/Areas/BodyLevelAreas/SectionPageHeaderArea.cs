using System.Collections.Generic;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Renderer;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class SectionPageHeaderArea : HeaderArea
	{
		public SectionPageHeaderArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			Sticky = ContainsParam(Constants.CommonAreaParameters.Sticky);
		}

		SectionPageHeaderArea() { }

		readonly bool Sticky;
		public override Area Clone(int position)
		{
			Area cloned = new SectionPageHeaderArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			return cloned;
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				return result;
			}
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#SectionPageHeader[:Sticky]",
				ResString.GetMultilingualString("e0f7f0eb-4f5c-43ae-91d9-1b49f9a00a31",
					@"Is printed at the top of each new page when a Body Section is split across multiple pages. 

Please note that this does not print on the first page the Body Section starts on if a {0} Area already exists. And make sure that the excel template file's page style setting is consistent with the {5} setting in the {4} Area.

Optional Parameters:
{1} - Will make sure that the {2} Area won't print on a page if there's not enough room to print the first row of the {3} Area with it.",
					"#SectionHeader", "Sticky", "#SectionPageHeader", "#SectionBody", "#Config", "PageStyle"));
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

		public override bool CanCloseAPage
		{
			get
			{
				return false;
			}
		}

		public override object GetColumnValue(int rowIndex, string fieldName)
		{
			IDataRowSource dS = null;
			int rowNumber = rowIndex;
			if (DataSourceArea != null)
			{
				dS = DataSourceArea.DataRowSource;
				rowNumber += DataSourceArea.FirstDataRowInArea;
			}

			return ParentReport.DataProvider.GetColumnValue(dS, rowNumber, fieldName);
		}

		public override SectionBodyArea DataSourceArea
		{
			get
			{
				SectionBodyArea result = null;
				for (int areaNumber = ParentReport.Analyser.Areas.IndexOf(this) + 1; areaNumber < ParentReport.Analyser.Areas.Count; areaNumber++)
				{
					if (ParentReport.Analyser.Areas[areaNumber] is SectionBodyArea && !(((SectionBodyArea)ParentReport.Analyser.Areas[areaNumber]).DataRowSource is ZStringArrayDataSource))
					{
						result = (SectionBodyArea)ParentReport.Analyser.Areas[areaNumber];
						break;
					}
				}
				return result;
			}
		}
	}
}
