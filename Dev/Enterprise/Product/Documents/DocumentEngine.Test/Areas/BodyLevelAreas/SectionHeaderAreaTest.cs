using System.IO;
using CargoWise.IO;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionHeaderAreaTest : AreaAbstractTest
	{
		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionHeader");
			Assert(createdArea is SectionHeaderArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionHeader");
			var clone = createdArea.Clone(10);
			AssertNotNull(clone);
		}

		public void TestShowEvenWithNoData()
		{
			var createdArea = (SectionHeaderArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionHeader:" + Constants.CommonAreaParameters.ShowEvenWithNoDataSignature);
			AssertEquals(true, createdArea.ShowEvenWithNoData);
		}

		public void TestShowEvenWithPageBreak()
		{
			var createdArea = (SectionHeaderArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionHeader");
			AssertEquals(false, createdArea.BreakPage);

			createdArea = (SectionHeaderArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionHeader:" + Constants.CommonAreaParameters.PageBreakSignature);
			AssertEquals(true, createdArea.BreakPage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionHeaderSplit()
		{
			var pack = new DocumentPack();
			var excelTemplate = new ExcelTemplateForUnitTesting("TestLongSectionHeader.xlsx", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(pack, excelTemplate, System.Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				var pages = new PageCollection();
				pages.DefaultPageHeight = 10000;
				pages.AddNew();

				var emptyPage = new Page() { Height = 10000 };
				var breaker = new PageBreakProcessor(report, pages);

				report.PrepareForRender();

				var sectionHeaderShort = report.Analyser.Sections[0].SectionHeader;
				var sectionHeaderNormal = report.Analyser.Sections[1].SectionHeader;
				var sectionHeaderLong = report.Analyser.Sections[2].SectionHeader;

				var docHeader = report.Analyser.DocumentHeader;
				breaker.AddAreaToLastPage(docHeader);

				var caseText = "\nTest section header:\n" +
			   "(1):It can fit in the first page, because its height is low\n" +
			   "(2):It cannot fit in the last page after (1) inserted, but it can fit in an empty page\n" +
			   "(3):It cannot fit any page, because its height too high\n";

				AssertEquals($"The contents of section header (1) can be fully written on the first page.{caseText}", true, sectionHeaderShort.FitsInPage(pages.LastPage.AvailableHeight, pages.LastPage));
				AssertNoExceptionThrown(() => breaker.AddAreaToLastPage(sectionHeaderShort));
				AssertEquals(2, pages.Count);

				var nowPage = pages.LastPage;
				var nowPageAvailableHeightBeforeInsertNewOne = nowPage.AvailableHeight;

				AssertEquals($"The contents of section header (2) cannot be fully written on the last page.{caseText}", false, sectionHeaderNormal.FitsInPage(nowPage.AvailableHeight, nowPage));
				AssertEquals($"The contents of section header (2) can be fully written on an empty page.{caseText}", true, sectionHeaderNormal.FitsInPage(emptyPage.AvailableHeight, emptyPage));
				AssertNoExceptionThrown(() => breaker.AddAreaToLastPage(sectionHeaderNormal));
				AssertEquals(3, pages.Count);
				AssertEquals($"All of section header (2) should be inserted into an empty page.{caseText}", nowPageAvailableHeightBeforeInsertNewOne, nowPage.AvailableHeight);

				nowPage = pages.LastPage;
				nowPageAvailableHeightBeforeInsertNewOne = nowPage.AvailableHeight;

				AssertEquals($"The contents of section header (3) cannot be fully written on the last page.{caseText}", false, sectionHeaderLong.FitsInPage(nowPage.AvailableHeight, nowPage));
				AssertEquals($"The contents of section header (3) cannot be fully written on an empty page.{caseText}", false, sectionHeaderLong.FitsInPage(emptyPage.AvailableHeight, emptyPage));
				AssertNoExceptionThrown(() => breaker.AddAreaToLastPage(sectionHeaderLong));
				AssertEquals(4, pages.Count);
				AssertNotEquals($"Part of section header (3) should be written on the last page.{caseText}", nowPageAvailableHeightBeforeInsertNewOne, nowPage.AvailableHeight);
			}
		}

		public void TestFitsInPage()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			using (Report.TemporarilyUseMainConnection())
			using (var embeddedResourceRetriever = new EmbeddedResourceRetriever())
			{
				var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
				var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				using (var rpt = new Report(pack, excelTemplate, System.Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
				{
					rpt.WorkSheetCurrentlyBeingProcessed[39, 0] = "#SectionBody:Data=Lines:Sticky";
					rpt.WorkSheetCurrentlyBeingProcessed[20, 0] = "#SectionHeader:Sticky";
					rpt.PrepareForRender();
					var createdArea = rpt.Analyser.Sections[0].SectionBody;
					createdArea.ExpandForDataRows(9);
					var fakePage = new Page();
					var fakeSectionDocumentHeader = AreaFactory.InstantiateArea(20, 21, TestReport, "#DocumentHeader");
					fakePage.Areas.Add(fakeSectionDocumentHeader);
					rpt.Analyser.Sections[0].SectionHeader.OwnerSection = rpt.Analyser.Sections[0];
					AssertEquals(false, rpt.Analyser.Sections[0].SectionHeader.FitsInPage(8000, fakePage));
					AssertEquals(true, rpt.Analyser.Sections[0].SectionHeader.FitsInPage(8000, new Page()));
				}
			}
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionHeader");
	}
}
