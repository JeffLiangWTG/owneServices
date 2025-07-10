using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GroupIndex))]
	sealed class GroupByIndexTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GroupIndex>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GroupIndex  >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("  < GroupIndex  >   ", Passes.FirstPass));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GroupIndex();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();

			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Lines");
			var testGroupByAreaArea = new GroupByArea(3, 4, Report, "#GroupBy:GST");

			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "Lines.GST", 0);

			var ownerSection = new Section();
			testGroupByAreaArea.OwnerSection = ownerSection;
			testGroupByAreaArea.OwnerSection.SplitSectionBodyAreaInstances = new List<SectionBodyArea>();
			testGroupByAreaArea.OwnerSection.SplitSectionBodyAreaInstances.Add(testSectionBodyArea);
			testGroupByAreaArea.DataParent = testSectionBodyArea;
			testGroupByAreaArea.RenderedToPageNumber = 1;
			testGroupByAreaArea.SectionBody = testSectionBodyArea;

			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;
			Report.Analyser.Sections.Add(ownerSection);
		}
	}
}
