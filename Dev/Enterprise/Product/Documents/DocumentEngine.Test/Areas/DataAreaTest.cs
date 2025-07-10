using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class DataAreaTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUsedDataFields()
		{
			var template = new ExcelTemplateForUnitTesting("SelectListTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), template))
			{
				report.PrepareForRender();
				var groupByTitle = GetGroupByTitle(report);
				var groupBy = GetGroupByFooter(report);
				var sectionBody = GetSectionBody(report);

				var expectedGroupTitleColumns = "JH_JOBNUM,AL_LINETYPE,AL_JH";
				var expectedGroupFooterColumns = "AL_OSAMOUNT,AL_JH";
				var expectedSectionBodyColumns = "AL_LINEAMOUNT,AL_LINETYPE,AL_EXCHANGERATE,AL_POSTPERIOD,AL_UNITQTY,AL_UNITPRICE,JH_A_JOP,JH_PK,JH_OA_LOCALCHARGESADDR,JH_ISPROFITSHAREPOSTED,JH_OA_AGENTCOLLECTADDR,AL_POSTTOGL";

				AssertEquals(expectedGroupTitleColumns, string.Join(",", groupByTitle.UsedDatafields));
				AssertEquals(expectedGroupFooterColumns, string.Join(",", groupBy.UsedDatafields));
				AssertEquals(expectedSectionBodyColumns, string.Join(",", sectionBody.UsedDatafields));
			}
		}

		GroupByArea GetGroupByTitle(Report report)
		{
			GroupByArea result = null;
			foreach (var section in report.Analyser.Sections)
			{
				foreach (var area in section.DataAreas)
				{
					var groupbyArea = area as GroupByArea;
					if (groupbyArea != null)
					{
						if (groupbyArea.GroupByPosition == Enterprise.DocumentEngine.Areas.GroupByArea.Position.Top)
						{
							result = groupbyArea;
						}
					}
				}
			}
			return result;
		}

		GroupByArea GetGroupByFooter(Report report)
		{
			GroupByArea result = null;
			foreach (var section in report.Analyser.Sections)
			{
				foreach (var area in section.DataAreas)
				{
					var groupbyArea = area as GroupByArea;
					if (groupbyArea != null)
					{
						if (groupbyArea.GroupByPosition == Enterprise.DocumentEngine.Areas.GroupByArea.Position.Bottom)
						{
							result = groupbyArea;
						}
					}
				}
			}
			return result;
		}

		SectionBodyArea GetSectionBody(Report report)
		{
			SectionBodyArea result = null;
			foreach (var section in report.Analyser.Sections)
			{
				foreach (var area in section.DataAreas)
				{
					var sectionBodyArea = area as SectionBodyArea;
					if (sectionBodyArea != null)
					{
						result = sectionBodyArea;
					}
				}
			}
			return result;
		}
	}
}
