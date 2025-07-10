using System.IO;
using System.Linq;
using CargoWise.IO;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class SectionPageHeaderAreaTest : AreaAbstractTest
	{
		public void TestSectionPageHeaderDoesNotPrintOnFirstPageIfSectionHeaderExists()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionHeader]
{B}-[My Section Header]
{A}-[#SectionPageHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Number>]
{A}-[#EndOfReport]", "UnitTest");

			var dummy = Factory.New<DummyDocumentSupportable>();

			for (var index = 0; index < 100; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Number = index;
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var result = excelInterface.WorkSheets.First().ToString().ToLower();

				Assert("Should contain #SectionHeader", result.Contains("my section header"));
				Assert("Should NOT contain #SectionPageHeader on first page", !result.Contains("page 1 of 2"));
				Assert("Should contain #SectionPageHeader on second page", result.Contains("page 2 of 2"));
			}
		}

		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionPageHeader");
			Assert(createdArea is SectionPageHeaderArea);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionPageHeader");
			Assert(createdArea.Clone(10) is SectionPageHeaderArea);
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
					rpt.WorkSheetCurrentlyBeingProcessed[35, 0] = "#SectionPageHeader:Sticky";
					rpt.PrepareForRender();
					var createdArea = rpt.Analyser.Sections[0].SectionBody;
					createdArea.ExpandForDataRows(9);
					var fakePage = new Page();
					var fakeSectionDocumentHeader = AreaFactory.InstantiateArea(20, 21, TestReport, "#DocumentHeader");
					fakePage.Areas.Add(fakeSectionDocumentHeader);
					rpt.Analyser.Sections[0].SectionPageHeader.OwnerSection = rpt.Analyser.Sections[0];
					AssertEquals(false, rpt.Analyser.Sections[0].SectionPageHeader.FitsInPage(4000, fakePage));
					AssertEquals(true, rpt.Analyser.Sections[0].SectionPageHeader.FitsInPage(4000, new Page()));
				}
			}
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionPageHeader");
	}
}
