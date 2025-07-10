using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GroupTotalPages))]
	sealed class GroupTotalPagesTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<GroupTotalPages>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<GroupTotalPages(GroupByField)>", Passes.SecondPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< GroupTotalPages (GroupByField)  >", Passes.SecondPass));
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupTotalPagesReplacing()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();

			for (var i = 0; i < 30; i++)
			{
				topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "Child Row " + (i % 5 + 1);
			}
			var excelTemplate = new ExcelTemplateForUnitTesting("GroupCurrentAndTotalPagesTest.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(outputStream);
					var workSheet = xlInterface.WorkSheets[0];
					AssertMultilineASCIIEquals("workSheet: " + workSheet.SheetName, ExpectedOutputFromGroupTotalPagesReplacing, workSheet.ToString());
				}
			}
		}

		#region ExpectedOutputFromGroupTotalPagesReplacing
		const string ExpectedOutputFromGroupTotalPagesReplacing = @"{B}-[Group page # 1 of total 2]
{B}-[Child Row 1]
{B}-[Child Row 1]
{B}-[Child Row 1]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 2 of total 2]
{B}-[Child Row 1]
{B}-[Child Row 1]
{B}-[Child Row 1]
{B}-[*** END OF GROUP ***]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 1 of total 2]
{B}-[Child Row 2]
{B}-[Child Row 2]
{B}-[Child Row 2]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 2 of total 2]
{B}-[Child Row 2]
{B}-[Child Row 2]
{B}-[Child Row 2]
{B}-[*** END OF GROUP ***]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 1 of total 2]
{B}-[Child Row 3]
{B}-[Child Row 3]
{B}-[Child Row 3]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 2 of total 2]
{B}-[Child Row 3]
{B}-[Child Row 3]
{B}-[Child Row 3]
{B}-[*** END OF GROUP ***]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 1 of total 2]
{B}-[Child Row 4]
{B}-[Child Row 4]
{B}-[Child Row 4]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 2 of total 2]
{B}-[Child Row 4]
{B}-[Child Row 4]
{B}-[Child Row 4]
{B}-[*** END OF GROUP ***]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 1 of total 2]
{B}-[Child Row 5]
{B}-[Child Row 5]
{B}-[Child Row 5]

{B}-[*** END OF PAGE ***]
{B}-[Group page # 2 of total 2]
{B}-[Child Row 5]
{B}-[Child Row 5]
{B}-[Child Row 5]
{B}-[*** END OF GROUP ***]

{B}-[*** END OF PAGE ***]";
		#endregion

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplateForUnitTesting excelTemplate)
		{
			return new Report(new DocumentPack(), excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GroupTotalPages();
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
			var page = Report.Renderer.Pages.AddNew();
			page.Areas.Add(testGroupByAreaArea);
		}
	}
}
