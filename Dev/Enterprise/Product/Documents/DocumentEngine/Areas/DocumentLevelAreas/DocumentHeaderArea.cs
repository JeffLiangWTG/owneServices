using System.Collections.Generic;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class DocumentHeaderArea : Area
	{
		public DocumentHeaderArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
		}

		DocumentHeaderArea() { }

		public override Area Clone(int position)
		{
			Area cloned = new DocumentHeaderArea(position, position + fEnd - fStart, ParentReport, "");
			CopyCommonMembers(cloned);
			return cloned;
		}

		public override List<Area> Parents
		{
			get
			{
				List<Area> result = new List<Area>();
				if (ParentReport.Analyser.Sections.Count > 0)
				{
					result.Add(ParentReport.Analyser.Sections[0].DataAreas[0]);
				}
				return result;
			}
		}

		public override bool SplitIfNotFitInAPage
		{
			get { return true; }
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("#DocumentHeader",
				ResString.GetMultilingualString("4f49ee9d-e00f-4b93-8672-4f4f1fd6df39",
					@"Shows at the very top of the first page before all other Areas, but doesn't get printed on any following pages. 

If you want an Area that prints at the very top of every page, use a {0} Area instead.

Please note that a {1} Area can be split across multiple pages if it's too long, but a {0} cannot.

You can only have one {1} Area.", "#PageHeader", "#DocumentHeader"));
		}
	}
}
