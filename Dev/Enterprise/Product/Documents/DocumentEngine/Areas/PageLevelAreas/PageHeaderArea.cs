using System.Collections.Generic;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class PageHeaderArea : HeaderArea
	{
		public PageHeaderArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			StartFromSecondPage = ContainsParam("StartFromSecondPage");
			ShouldDelete = StartFromSecondPage;
		}

		PageHeaderArea() { }

		public override Area Clone(int position)
		{
			PageHeaderArea cloned = new PageHeaderArea(position, position + fEnd - fStart, ParentReport, (NoResString)"#PageHeader");
			CopyCommonMembers(cloned);
			cloned.ShouldDelete = false;
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
			return new ValueProviderDocumenter(
				"#PageHeader[:StartFromSecondPage]",
				ResString.GetMultilingualString("0b9affa8-143b-4f6d-b951-d572f2a34917",
					@"Will show at the top of every page but after the {0} Area on the first page (if specified) and not at all on the first page if the optional '{1}' parameter is specified.

Generally people use a {0} on the first page and {2} where they want a different header on the first page to the follow pages, or a {3} Area on its own where they want each page to start with the same thing.

You can only have one {3} Area whether you specify the '{1}' parameter or not.",
					"#DocumentHeader", ":StartFromSecondPage", "#PageHeader:StartFromSecondPage", "#PageHeader"));
		}

		public override bool CanCloseAPage
		{
			get
			{
				return false;
			}
		}

		public readonly bool StartFromSecondPage;

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
