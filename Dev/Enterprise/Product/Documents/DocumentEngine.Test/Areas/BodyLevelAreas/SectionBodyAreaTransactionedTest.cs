using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionBodyAreaTransactionedTest : TransactionedTestCase
	{
		public void TestClone()
		{
			var testReport = Report.NewForTesting(DocumentPack.EmptyPack);
			var createdArea = AreaFactory.InstantiateArea(1, 10, testReport, "#SectionBody:Data=Test") as SectionBodyArea;
			var foreachArea1 = new SectionForeachArea(2, 7, testReport, "#BeginLoop:Data=DummyCollection", createdArea, createdArea);
			var foreachArea2 = new SectionForeachArea(8, 9, testReport, "#BeginLoop:Data=DummyCollection", createdArea, createdArea);
			var foreachArea3 = new SectionForeachArea(3, 4, testReport, "#BeginLoop:Data=DummyCollection", foreachArea1, createdArea);
			var foreachArea4 = new SectionForeachArea(5, 6, testReport, "#BeginLoop:Data=DummyCollection", foreachArea1, createdArea);
			createdArea.OwnerSection = new Section();
			createdArea.IsForeachProcessed = true;
			Assert(createdArea.IsForeachProcessed);

			SectionBodyArea cloned;
			using ((createdArea.ParentReport.Renderer as ReportRenderer).TrackProcessingPageBreaks())
			{
				cloned = createdArea.Clone(10) as SectionBodyArea;
			}

			AssertNotNull(cloned);
			AssertEquals(cloned.IsForeachProcessed, createdArea.IsForeachProcessed);
			AssertEquals(cloned.DataRowIndexWithRowRanges, createdArea.DataRowIndexWithRowRanges);
			AssertEquals(2, cloned.Children.Count);

			var child1 = cloned.Children[0];
			AssertEquals(2, child1.Children.Count);
			AssertEquals(foreachArea1.DataRowIndexWithRowRanges, child1.DataRowIndexWithRowRanges);
			var child3 = child1.Children[0];
			AssertEquals(foreachArea3.DataRowIndexWithRowRanges, child3.DataRowIndexWithRowRanges);
			var child4 = child1.Children[1];
			AssertEquals(foreachArea4.DataRowIndexWithRowRanges, child4.DataRowIndexWithRowRanges);

			var child2 = cloned.Children[1];
			AssertEquals(0, child2.Children.Count);
			AssertEquals(foreachArea2.DataRowIndexWithRowRanges, child2.DataRowIndexWithRowRanges);
		}

		public void TestCloneWithParameters()
		{
			var testReport = Report.NewForTesting(DocumentPack.EmptyPack);
			var createdArea = AreaFactory.InstantiateArea(1, 10, testReport, "#SectionBody:Data=Test:OrderBy(col):MaximumNumberOfRowsToShow=10") as SectionBodyArea;
			createdArea.OwnerSection = new Section();

			SectionBodyArea cloned;
			using ((createdArea.ParentReport.Renderer as ReportRenderer).TrackProcessingPageBreaks())
			{
				cloned = createdArea.Clone(10) as SectionBodyArea;
			}

			AssertNotNull(cloned);

			AssertEquals("Test", cloned.TableName);
			AssertEquals("col", cloned.sortByColumn);
			AssertEquals(10, cloned.fMaximumNumberOfRowsToShow);
		}

		public void TestSplit()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = new SectionBodyArea(rpt.Analyser.Sections[0].SectionBody.StartingRow, rpt.Analyser.Sections[0].SectionBody.End, rpt, "#SectionBody:Data=Lines");// Rpt.Analyser.Sections[0].SectionBody;

				var expansionSize = 160;
				var splitSize = 23;
				createdArea.ExpandForDataRows(expansionSize);

				AssertEquals(expansionSize + 1, createdArea.RowHeightTracker.Count);

				var ownerSection = new Section();
				createdArea.OwnerSection = ownerSection;
				createdArea.OwnerSection.SplitSectionBodyAreaInstances = new List<SectionBodyArea>();
				createdArea.OwnerSection.SplitSectionBodyAreaInstances.Add(createdArea);
				var splittedArea = (SectionBodyArea)createdArea.SplitArea(splitSize);

				AssertEquals(1, createdArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description")[0].Start);//Excel formula is 1 based
				AssertEquals(createdArea.StartOfBody + splitSize, createdArea.GetRowRanges(0, "Lines.Description")[0].End);
				AssertEquals(splitSize, createdArea.RowHeightTracker.Count);

				AssertEquals(1, splittedArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(splittedArea.StartOfBody + 1, splittedArea.GetRowRanges(0, "Lines.Description")[0].Start);//Excel formula is 1 based
				AssertEquals(splittedArea.StartOfBody + 1 + expansionSize - splitSize, splittedArea.GetRowRanges(0, "Lines.Description")[0].End);
				AssertEquals(expansionSize - splitSize + 1, splittedArea.RowHeightTracker.Count);
			}
		}

		public void TestSplitInTheMiddleOfDbRow()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = new SectionBodyArea(rpt.Analyser.Sections[0].SectionBody.StartingRow, rpt.Analyser.Sections[0].SectionBody.End + 2, rpt, "#SectionBody:Data=Lines");

				var expansionSize = 4;
				var splitSize = 7;
				createdArea.ExpandForDataRows(expansionSize);

				AssertEquals((expansionSize + 1) * 3, createdArea.RowHeightTracker.Count);

				var ownerSection = new Section();
				createdArea.OwnerSection = ownerSection;
				createdArea.OwnerSection.SplitSectionBodyAreaInstances = new List<SectionBodyArea>();
				createdArea.OwnerSection.SplitSectionBodyAreaInstances.Add(createdArea);

				var splittedArea = (SectionBodyArea)createdArea.SplitArea(splitSize);

				AssertEquals(7, createdArea.RowHeightTracker.Count);
				AssertEquals(0, createdArea.RowHeightTracker[6].OriginalPosition);
				AssertEquals(3, createdArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description")[0].Start);//Excel formula is 1 based
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description")[0].End);
				AssertEquals(createdArea.StartOfBody + 1 + 3, createdArea.GetRowRanges(0, "Lines.Description")[1].Start);
				AssertEquals(createdArea.StartOfBody + 1 + 3, createdArea.GetRowRanges(0, "Lines.Description")[1].Start);
				AssertEquals(createdArea.StartOfBody + 1 + 6, createdArea.GetRowRanges(0, "Lines.Description")[2].Start);
				AssertEquals(createdArea.StartOfBody + 1 + 6, createdArea.GetRowRanges(0, "Lines.Description")[2].Start);

				AssertEquals(1, splittedArea.RowHeightTracker[0].OriginalPosition);
				AssertEquals(8, splittedArea.RowHeightTracker.Count);
				AssertEquals(2, splittedArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(splittedArea.StartOfBody + 1 + 2, splittedArea.GetRowRanges(0, "Lines.Description")[0].Start);
				AssertEquals(splittedArea.StartOfBody + 1 + 2, splittedArea.GetRowRanges(0, "Lines.Description")[0].End);
				AssertEquals(splittedArea.StartOfBody + 1 + 2 + 3, splittedArea.GetRowRanges(0, "Lines.Description")[1].Start);
				AssertEquals(splittedArea.StartOfBody + 1 + 2 + 3, splittedArea.GetRowRanges(0, "Lines.Description")[1].End);
			}
		}

		public void TestSplitInTheMiddleOfDbRowWithAutoHeightExpandedInTheRow()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = new SectionBodyArea(rpt.Analyser.Sections[0].SectionBody.StartingRow, rpt.Analyser.Sections[0].SectionBody.End + 2, rpt, "#SectionBody:Data=Lines");

				var expansionSize = 4;
				var splitSize = 7;
				var autoHeithRow = 3;
				var autoExtraSize = 5;
				createdArea.ExpandForDataRows(expansionSize);
				createdArea.Expand(createdArea.StartingRow + autoHeithRow, createdArea.StartingRow + autoHeithRow, autoExtraSize);

				AssertEquals((expansionSize + 1) * 3, createdArea.RowHeightTracker.Count);

				var ownerSection = new Section();
				createdArea.OwnerSection = ownerSection;
				createdArea.OwnerSection.SplitSectionBodyAreaInstances = new List<SectionBodyArea>();
				createdArea.OwnerSection.SplitSectionBodyAreaInstances.Add(createdArea);
				var splittedArea = (SectionBodyArea)createdArea.SplitArea(splitSize);

				AssertEquals(3, createdArea.RowHeightTracker.Count);
				AssertEquals(2, createdArea.RowHeightTracker[2].OriginalPosition);
				AssertEquals(1, createdArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description")[0].Start);//Excel formula is 1 based
				AssertEquals(createdArea.StartOfBody + 1, createdArea.GetRowRanges(0, "Lines.Description")[0].End);

				AssertEquals(13, splittedArea.RowHeightTracker.Count);
				AssertEquals(-1, splittedArea.RowHeightTracker[0].OriginalPosition);
				AssertEquals(4, splittedArea.GetRowRanges(0, "Lines.Description").Count);
				AssertEquals(splittedArea.StartOfBody + 1 + 1, splittedArea.GetRowRanges(0, "Lines.Description")[0].Start);
				AssertEquals(splittedArea.StartOfBody + 1 + 1, splittedArea.GetRowRanges(0, "Lines.Description")[0].End);
				AssertEquals(splittedArea.StartOfBody + 1 + 1 + 3, splittedArea.GetRowRanges(0, "Lines.Description")[1].Start);
				AssertEquals(splittedArea.StartOfBody + 1 + 1 + 3, splittedArea.GetRowRanges(0, "Lines.Description")[1].End);
				AssertEquals(splittedArea.StartOfBody + 1 + 1 + 6, splittedArea.GetRowRanges(0, "Lines.Description")[2].Start);
				AssertEquals(splittedArea.StartOfBody + 1 + 1 + 6, splittedArea.GetRowRanges(0, "Lines.Description")[2].End);
				AssertEquals(splittedArea.StartOfBody + 1 + 1 + 9, splittedArea.GetRowRanges(0, "Lines.Description")[3].Start);
				AssertEquals(splittedArea.StartOfBody + 1 + 1 + 9, splittedArea.GetRowRanges(0, "Lines.Description")[3].End);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}
		ExcelTemplateForUnitTesting newStyleTemplate;
	}
}
