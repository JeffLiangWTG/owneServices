using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class GroupByAreaTest : AreaAbstractTest
	{
		public void TestGroupBySortsNumericallyInReport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Data:ReportData=Select Foo = 1 Union All Select Foo = 8 Union All Select Foo = 26 Union All Select Foo = 88 Union All Select Foo = 18 Union All Select Foo = 6]
{A}-[#SectionBody:Data=ReportData]
{A}-[#GroupBy:ReportData.Foo]
{B}-[<ReportData.Foo>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("The values should be numerically sorted.",
@"{B}-[1]
{B}-[6]
{B}-[8]
{B}-[18]
{B}-[26]
{B}-[88]", workSheet.ToString());
			}
		}

		public void TestGroupBy2ColumnsSortsNumericallyInReport()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Data:ReportData=Select Soo = 1, Foo = 8 Union All Select Soo = 1, Foo = 26 Union All Select Soo = 1, Foo = 1 Union All Select Soo = 2, Foo = 88 Union All Select Soo = 2, Foo = 6 Union All Select Soo = 2, Foo = 18]
{A}-[#SectionBody:Data=ReportData]
{A}-[#GroupBy:ReportData.Soo+ReportData.Foo]
{B}-[<ReportData.Soo>, <ReportData.Foo>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("The values should be numerically sorted.",
@"{B}-[1, 1]
{B}-[1, 8]
{B}-[1, 26]
{B}-[2, 6]
{B}-[2, 18]
{B}-[2, 88]", workSheet.ToString());
			}
		}

		public void TestGroupBySectionWithNoDataDoesNotGenerateEmptyPagesWithGroupTitle()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[Placeholder]
{A}-[#GroupBy:Collection.Z0_Number:GroupTitle]
{B}-[Placeholder]
{A}-[#GroupBy:Collection.Z0_VarCharMax:GroupTitle:PageBreak]
{B}-[Placeholder]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			AssertEquals("Pre-condition: Collection should be empty.", 0, dummy.Collection.Count);

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			var result = printJobs[0];

			using (var excelInterface = new ExcelInterface(result.SP_CustomProperties))
			{
				AssertMultilineASCIIEquals("There should be only one page with no other data, as collection is empty.",
@"{B}-[Page 1 of 1]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestGroupByCanHandleFormatStringsAsParameters()
		{
			var area = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:InvoiceLines.CusEntryLine.Format(\"{CL_LineNumber:D9}\"):GroupTitle");
			AssertEquals("area.GroupByPosition", GroupByArea.Position.Top, area.GroupByPosition);
			AssertEquals("area.GroupByColumns.Length", 1, area.GroupByColumns.Length);
			AssertEquals("area.GroupByColumns[0]", "InvoiceLines.CusEntryLine.Format(\"{CL_LineNumber:D9}\")", area.GroupByColumns[0]);
		}

		public override void TestVisualisationManagerHasAppropriateVisualisationRenderer()
		{
			var groupByArea = (GroupByArea)GetNewAreaToTest();
			groupByArea.SectionBody = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=Test");
			AssertEquals(typeof(RendererGroupByArea), AreaVisualisationManagerFactory.New(groupByArea).VisualisationRenderer.GetType());
		}

		public void TestInstantiateArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col");
			Assert(createdArea is GroupByArea);
			AssertEquals(1, ((GroupByArea)createdArea).GroupByColumns.Length);
			AssertEquals("Test.Col", ((GroupByArea)createdArea).GroupByColumns[0]);
			AssertEquals(GroupByArea.Position.Bottom, ((GroupByArea)createdArea).GroupByPosition);
		}

		[ExpectException(typeof(DocumentEngineException))]
		public void TestInstantiateGroupTitleByArea()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupTitleBy:Test.Col");
			AssertNotNull(createdArea);
		}

		public void TestInstantiateAreaWithOption()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:GroupTitle");
			Assert(createdArea is GroupByArea);
			AssertEquals(1, ((GroupByArea)createdArea).GroupByColumns.Length);
			AssertEquals("Test.Col", ((GroupByArea)createdArea).GroupByColumns[0]);
			AssertEquals(GroupByArea.Position.Top, ((GroupByArea)createdArea).GroupByPosition);
		}

		public void TestClone()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col");
			Assert(createdArea.Clone(10) is GroupByArea);
		}

		public void TestCloneWithTitle()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:GroupTitle");
			Assert(createdArea.Clone(10) is GroupByArea);
			AssertEquals(GroupByArea.Position.Top, ((GroupByArea)createdArea.Clone(10)).GroupByPosition);
		}

		public void TestInstantiateAreaWithPageBreak()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:PageBreak");
			Assert(createdArea is GroupByArea);
			AssertEquals(true, ((GroupByArea)createdArea).BreakPage);
		}

		public void TestCloneBreakPage()
		{
			var createdArea = AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:PageBreak");
			AssertEquals(true, ((GroupByArea)createdArea.Clone(10)).BreakPage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFitsInPage()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate1 = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate1, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed[41, 0] = "#GroupBy:Lines.AccountingGroupName:Sticky:GroupTitle";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 140 * from ##LinesTest";
				rpt.PrepareForRender();
				AssertEquals(false, rpt.Analyser.Sections[0].SectionBodyAndGroupByAreas[1].FitsInPage(270, new Page()));
			}

			var excelTemplate2 = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (var rpt = new Report(pack, excelTemplate2, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed[41, 0] = "#GroupBy:Lines.AccountingGroupName:GroupTitle";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 140 * from ##LinesTest";
				rpt.PrepareForRender();
				AssertEquals(true, rpt.Analyser.Sections[0].SectionBodyAndGroupByAreas[1].FitsInPage(270, new Page()));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFitsInPage_KeepInSamePage()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate1 = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate1, Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.WorkSheetCurrentlyBeingProcessed[41, 0] = "#GroupBy:Lines.AccountingGroupName:GroupTitle:KeepInSamePage";
				rpt.WorkSheetCurrentlyBeingProcessed[6, 0] = "Data:Lines=select top 10 * from ##LinesTest";
				rpt.PrepareForRender();
				Assert("Should return true because the whole GroupBy + Body area could not possibly fit in one page so the SectionBody has to be split", rpt.Analyser.Sections[0].SectionBodyAndGroupByAreas[1].FitsInPage(270, new Page()));

				var nonEmptyPage = new Page();
				var bodyArea = new SectionBodyArea(1, 2, rpt, "");
				nonEmptyPage.Areas.Add(bodyArea);
				rpt.Analyser.Sections[0].SectionBodyAndGroupByAreas[1].DataParent = bodyArea;
				Assert("Should go to the next page", !rpt.Analyser.Sections[0].SectionBodyAndGroupByAreas[1].FitsInPage(270, nonEmptyPage));
				Assert(rpt.Analyser.Sections[0].SectionBodyAndGroupByAreas[1].FitsInPage(520, nonEmptyPage));
			}
		}

		public void TestSplitAndReturnNewArea()
		{
			var page = new Page();
			page.Height = 3000; //page available height is bigger than the height of groupby section
			var createdArea = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:KeepInSamePage");
			createdArea.OwnerSection = new Section();

			var bodyArea = new SectionBodyArea(1, 2, TestReport, "#SectionBody:DATA=Test");
			page.Areas.Add(bodyArea);
			Assert("Current area Height should be smaller than page Height", createdArea.HeightInXls < page.Height);
			AssertNull("Should not split in a old page if KeepInSamePage is true.", createdArea.SplitAndReturnNewArea(500, page, null));

			page.Areas.Clear();
			AssertNotNull("Should split in a new page although KeepInSamePage is true.", createdArea.SplitAndReturnNewArea(500, page, null));

			createdArea = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col");
			createdArea.OwnerSection = new Section();
			AssertNotNull(createdArea.SplitAndReturnNewArea(500, page, null));

			createdArea = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:KeepInSamePage");
			createdArea.OwnerSection = new Section();
			page.Height = 2000;//page available height is smaller than the height of groupby section
			Assert("Current area Height should be bigger than page Height", createdArea.HeightInXls > page.Height);
			AssertNotNull(createdArea.SplitAndReturnNewArea(500, page, null));

			page.Areas.Clear();
			var headerArea = (DocumentHeaderArea)AreaFactory.InstantiateArea(1, 1, TestReport, "#DocumentHeader");
			page.Areas.Add(headerArea);

			createdArea = (GroupByArea)AreaFactory.InstantiateArea(2, 11, TestReport, "#GroupBy:Test.Col:KeepInSamePage");
			createdArea.OwnerSection = new Section();
			page.Height = 3000;//page available height is bigger than the height of groupby section
			Assert("Current area Height should be bigger than page Height", createdArea.HeightInXls < page.Height);
			AssertNotNull(createdArea.SplitAndReturnNewArea(500, page, null));
		}

		public void TestCloneKeepInSamePage()
		{
			var createdArea = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col:KeepInSamePage");
			Assert(createdArea.KeepInSamePage);
			Assert(((GroupByArea)createdArea.Clone(10)).KeepInSamePage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupBySectionWithInvalidGroupByColumnLength()
		{
			var dataItem = (BusinessObject)Factory.New<Enterprise.Integration.TransportBooking.IDtbBooking>();
			var instruction = Factory.New<Enterprise.Integration.TransportBooking.IDtbBookingInstruction>();
			instruction.KN_KM_BookingMovement = dataItem.PK;
			var wrapper = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, dataItem);
			var dataProviders = new DataProviderList(wrapper);

			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);

			var template = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			var report = new Report(pack, template, dataProviders, "name", null, DocumentDirection.ANY, false);

			var expectedMessage = "Malformed GroupByColumn provided: '#GroupBy::KeepInSamePage'. Please specify at least one column to group by.";

			AssertExceptionThrown(typeof(DocumentEngineException), expectedMessage, () => AreaFactory.InstantiateArea(1, 10, report, "#GroupBy::KeepInSamePage"));
		}

		public void TestGroupBySectionWithInvalidGroupByColumnName()
		{
			var groupbyArea = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:ReportData.Consol");
			var fields1 = groupbyArea.AllDatafields;
			AssertEquals(1, fields1.Count);
			AssertEquals("REPORTDATA", fields1.Keys.First());
			AssertEquals(1, fields1.Values.First().Count);
			AssertEquals("CONSOL", fields1.Values.First().First());

			var groupbyArea2 = (GroupByArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:.Consol");
			groupbyArea2.SectionBody = (SectionBodyArea)AreaFactory.InstantiateArea(1, 10, TestReport, "#SectionBody:Data=REPORTDATA");
			var fields2 = groupbyArea2.AllDatafields;
			AssertEquals(1, fields2.Count);
			AssertEquals("REPORTDATA", fields2.Keys.First());
			AssertEquals(1, fields2.Values.First().Count);
			AssertEquals("CONSOL", fields2.Values.First().First());

			var expectMessage = "Malformed GroupByColumn provided: '#GroupBy:ReportData.'. GroupByColumn {ReportData.} should not be defined end with '.'.";
			AssertExceptionThrown(typeof(DocumentEngineException), expectMessage, () => AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:ReportData."));
		}

		public void TestGroupByArea_InvalidCastException()
		{
			AssertExceptionThrown<DocumentEngineException>(() => { new GroupByArea(1, 10, TestReport, "#GroupBy:<CurrentCompany>"); });
		}

		protected override Area GetNewAreaToTest() => AreaFactory.InstantiateArea(1, 10, TestReport, "#GroupBy:Test.Col");
	}
}
