using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class RendererSectionBodyTest : RendererGeneralAbstractTest
	{
		public void TestCreateComponentsAndDataContainersShouldContainsMergedCell()
		{
			var bodyArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=Test");
			TestReport.WorkSheetCurrentlyBeingProcessed.MergeCells(2, 2, 2, 7);
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<Test.Description>";

			var dataSet = new VisualiserDataSet();
			var renderer = new RendererSectionBody(bodyArea);
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dataSet, 110, new List<string>());

			AssertEquals(1, createdComponents.Count);
			AssertEquals(1, dataSet.Tables[1].Columns.Count);

			var componentGrid = createdComponents[0] as VisualiserComponentGrid;
			AssertEquals(ControlDpiScalingHelper.NewScaledSize(468, 120), componentGrid.Size);
		}

		public void TestVisualizeWithNumberOfRowsToShowGreaterThanActualNumberOfRows()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_Number = 888;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			AssertEquals("Pre-condition: Actual numbers of rows should be 1.", 1, dummy.Collection.Count);

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection, NumberOfRowsToShow=3]
{B}-[Row]   {C}-[<Collection.Number>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();

				var tableName = VisualiserDataSet.GetTableName("Collection");
				var dataSet = report.OverridingDataSet;
				var dataTable = dataSet.Tables.Add(tableName);
				dataTable.Columns.Add("Number");
				dataTable.Rows.Add(child.Z0_Number);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("There should be three rows shown.",
@"{B}-[Row]   {C}-[888]
{B}-[Row]
{B}-[Row]",
							excelInterface.WorkSheets[0].ToString());
					}

					AssertEquals("There should only be one actual row.", 1, report.OverridingDataSet.Tables[VisualiserDataSet.GetTableName("Collection")].Rows.Count);
				}
			}
		}

		public void TestTotalInSectionFooterOfDecimalValuesInHiddenColumns()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("A", "X").Z0_AnotherDecimal = 1.000m;
			dummy.Collection.AddNew("B", "X").Z0_AnotherDecimal = 2.000m;
			dummy.Collection.AddNew("C", "Y").Z0_AnotherDecimal = 3.000m;
			dummy.Collection.AddNew("D", "Y").Z0_AnotherDecimal = 4.000m;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Code>]   {D}-[<Collection.AnotherDecimal>]
{A}-[#SectionFooter:ShowEvenWithNoData]
{B}-[Total]   {C}-[<Total Collection.AnotherDecimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected =
@"{B}-[A]
{B}-[B]
{B}-[C]
{B}-[D]
{B}-[Total]   {C}-[10]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Pre-condition: Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();
				documentPack.SaveVisualizerContentNote();

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Totals should be calculated correctly.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestCreatingDataContainerWithEmptySectionBodyButValidGroupByShouldProduceGridWithWidthGreaterThanZero()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Dummy Report", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT Z0_Number, Z0_VarCharMax FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{A}-[#GroupBy:ReportData.Z0_Number]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate))
				{
					var dataSet = new VisualiserDataSet();
					var converter = new TemplateToVisualiserComponentsConverter(report, dataSet);

					var components = converter.Components;
					AssertEquals(1, converter.Components.Count);

					var grid = components[0] as VisualiserComponentGrid;
					Assert(grid.Size.Width > 0);
				}
			}
		}

		public void TestConstructorDoesntAllowNullAreaParameter()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate { new RendererSectionBody(null); });
		}

		public void TestComponentGridWithAutoHeightHasCorrectSize()
		{
			var parentArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=Test");
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<AutoHeight><Test.Description>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 7] = "<Test.GSTRate> <Test.AmountExTax>";

			var dS = new VisualiserDataSet();
			var renderer = new RendererSectionBody(parentArea);
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals(1, createdComponents.Count);
			var componentGrid = createdComponents[0] as VisualiserComponentGrid;
			AssertNotNull("componetGrid", componentGrid);
			AssertEquals("componentGrid.AutoHeight", true, componentGrid.AutoHeight);
			AssertEquals(ControlDpiScalingHelper.NewScaledSize(468, 180), componentGrid.Size);
		}

		public void TestGetHeightFromBodyArea()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Dummy Report", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT Z0_Number, Z0_VarCharMax FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate))
				{
					report.PrepareForRender();
					DummyArea area = new DummyArea(report);
					DummyRendererSectionBody renderer = new DummyRendererSectionBody(new SectionBodyArea(1, 10, report, ""));
					renderer.AreaContainingFieldsOverride = area;
					var dataSet = new VisualiserDataSet();
					renderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, dataSet, 110, new List<string>());
					area.HeightInXlsOverride = 9999;
					AssertEquals(10254, renderer.GetHeightInXL());//9999+255

					area.HeightInXlsOverride = 1;

					AssertEquals(1440, renderer.GetHeightInXL());
				}
			}
		}

		public override void TestGetHeight()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Dummy Report", string.Empty,
@"{A}-[#Config]
{A}-[Name=Dummy Report]
{A}-[Data:ReportData=SELECT Z0_Number, Z0_VarCharMax FROM dbo.DummyBizo]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Number>]    {C}-[<ReportData.Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate))
				{
					report.PrepareForRender();
					var sectionBodyArea = report.Analyser.Sections[0].SectionBody;

					var renderer = new RendererSectionBody(sectionBodyArea);
					AssertExceptionThrown(typeof(InvalidOperationException), delegate { renderer.GetHeightInXL(); });

					var dataSet = new VisualiserDataSet();
					renderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, dataSet, 110, new List<string>());
					AssertEquals(1440, renderer.GetHeightInXL());
				}
			}
		}

		[ExpectException(typeof(Exception))]
		public void TestGetHeightWithoutCallingCreateComponentsAndDataContainers()
		{
			var parentArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=Test");
			var renderer = new RendererSectionBody(parentArea);
			AssertEquals(1440, renderer.GetHeightInXL());
		}

		public void TestCreateComponentsAndDataContainers()
		{
			var parentArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=Test");
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<Test.Description>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 7] = "<Test.GSTRate> <Test.AmountExTax>";
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 4] = "TestConstant";

			var dS = new VisualiserDataSet();
			var renderer = new RendererSectionBody(parentArea);
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals(2, dS.Tables.Count);
			AssertEquals(3, dS.Tables[1].Columns.Count);
			AssertEquals("Description", dS.Tables[1].Columns[0].ColumnName);
			AssertEquals("GSTRate", dS.Tables[1].Columns[1].ColumnName);
			AssertEquals("AmountExTax", dS.Tables[1].Columns[2].ColumnName);
			AssertEquals(160, dS.Tables[1].Rows.Count);

			AssertEquals(1, createdComponents.Count);
			var componentGrid = createdComponents[0] as VisualiserComponentGrid;
			AssertNotNull("componetGrid", componentGrid);
			AssertEquals(ControlDpiScalingHelper.NewScaledPoint(78, 9), componentGrid.Location);
			AssertEquals(ControlDpiScalingHelper.NewScaledSize(468, 120), componentGrid.Size);
			AssertEquals("VisualiserTable_Test", componentGrid.BindToName);
		}

		public void TestGetHeightWithSectionBodyAlreadyAdded()
		{
			var parentArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=Test");
			var renderer = new RendererSectionBody(parentArea);
			var sectionbodyNames = new List<string>();
			sectionbodyNames.Add("VisualiserTable_Test");
			var dS = new VisualiserDataSet();
			renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, sectionbodyNames);
			AssertEquals(0, renderer.GetHeightInXL());
		}

		public void TestCreateComponentsAndDataContainersWithFormat()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_Date = new ZDateTime(2008, 8, 8);

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Format(""{Z0_Date:Date}"")>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var visualizerManager = new DocPackVisualiserManager(documentPack, deliveryInstructions.DeliverablesToBePrinted);
				var convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				var components = convertor.Components;

				AssertEquals("Z0_Date", report.OverridingDataSet.Tables[1].Columns[0].ColumnName);
			}
		}

		public void TestCreateComponentsAndDataContainersWithTotal()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_Number = 1;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Total(""Z0_Number"")>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var visualizerManager = new DocPackVisualiserManager(documentPack, deliveryInstructions.DeliverablesToBePrinted);
				var convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				var components = convertor.Components;

				AssertEquals("TotalZ0_Number", report.OverridingDataSet.Tables[1].Columns[0].ColumnName);
			}
		}

		public void TestCreateComponentsAndDataContainersWithTotalMacroAndOtherMacroInSameCell()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_Number = 1000;
			child.Z0_Code = "Code";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Total Z0_Number ><Z0_Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			Factory.Save();

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var visualizerManager = new DocPackVisualiserManager(documentPack, deliveryInstructions.DeliverablesToBePrinted);
				var convertor = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				var components = convertor.Components;

				AssertEquals("Z0_Number", report.OverridingDataSet.Tables[1].Columns[0].ColumnName);
				AssertEquals("Z0_Code", report.OverridingDataSet.Tables[1].Columns[1].ColumnName);
			}
		}

		public void TestCreateComponentsAndDataContainersWithSectionBodyAlreadyAdded()
		{
			var parentArea = new SectionBodyArea(1, 10, TestReport, "#SectionBody:Data=Test");
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<Test.Description>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 7] = "<Test.GSTRate> <Test.AmountExTax>";
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 4] = "TestConstant";

			var dS = new VisualiserDataSet();
			var renderer = new RendererSectionBody(parentArea);
			var sectionbodyNames = new List<string>();
			sectionbodyNames.Add("VisualiserTable_Test");
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, sectionbodyNames);

			AssertEquals(2, dS.Tables.Count);
			AssertEquals(3, dS.Tables[1].Columns.Count);
			AssertEquals("Description", dS.Tables[1].Columns[0].ColumnName);
			AssertEquals("GSTRate", dS.Tables[1].Columns[1].ColumnName);
			AssertEquals("AmountExTax", dS.Tables[1].Columns[2].ColumnName);
			AssertEquals(160, dS.Tables[1].Rows.Count);

			AssertEquals(0, createdComponents.Count);
		}

		public void TestModifiableFieldColumnInSectionBody()
		{
			ClearPrintJobs();

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=TestMacro]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection]
{B}-[<ModifiableField(""ABC"", ""DEF"")>]
{A}-[#EndOfReport]");

			var documentCommand = GetDocumentCommand(template, 3);

			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var instructions = GetDeliveryInstructions();

				var pack = printTask.GetFirstDocumentPack();
				var report = pack.GetFirstReport();
				var visualizerManager = new DocPackVisualiserManager(pack, instructions.DeliverablesToBePrinted);
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();

				AssertEquals(1, converter.Components.Count);

				var dataTable = report.OverridingDataSet.Tables[1];
				dataTable.Rows[0]["<ModifiableField(\"ABC\", \"DEF\")>"] = "TEST MODIFY DATA 1";
				dataTable.Rows[1]["<ModifiableField(\"ABC\", \"DEF\")>"] = "TEST MODIFY DATA 2";
				dataTable.Rows[2]["<ModifiableField(\"ABC\", \"DEF\")>"] = "TEST MODIFY DATA 3";
				pack.SaveVisualizerContentNote();

				printTask.Run(instructions);

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
				{
					excelInterface.LoadExcelFile(stream);
					var value1 = excelInterface.WorkSheets[0].GetCell(0, 1).Value.ToString();
					var value2 = excelInterface.WorkSheets[0].GetCell(1, 1).Value.ToString();
					var value3 = excelInterface.WorkSheets[0].GetCell(2, 1).Value.ToString();

					AssertEquals("TEST MODIFY DATA 1", value1);
					AssertEquals("TEST MODIFY DATA 2", value2);
					AssertEquals("TEST MODIFY DATA 3", value3);
				}
			}
		}

		public void TestModifiableFieldColumnInSectionBodyWithoutDataCollection()
		{
			ClearPrintJobs();

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=TestMacro]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody]
{B}-[<ModifiableField(""ABC"", ""DEF"")>]
{A}-[#EndOfReport]");
			var documentCommand = GetDocumentCommand(template, 1);
			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var instructions = GetDeliveryInstructions();

				var pack = printTask.GetFirstDocumentPack();
				var report = pack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();

				AssertEquals(1, converter.Components.Count);

				var dataTable = report.OverridingDataSet.MainTable;
				dataTable.Rows[0]["<ModifiableField(\"ABC\", \"DEF\")>"] = "TEST MODIFY DATA";
				pack.SaveVisualizerContentNote();

				printTask.Run(instructions);

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
				{
					excelInterface.LoadExcelFile(stream);
					var value = excelInterface.WorkSheets[0].GetCell(0, 1).Value.ToString();
					AssertEquals("TEST MODIFY DATA", value);
				}
			}
		}

		public void TestCurrentSectionBodyAreaWithMoreThanOneRowData()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=TestMacro]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection]
{B}-[<Z0_Bool>]
{A}-[#EndOfReport]");
			var documentCommand = GetDocumentCommand(template, 1);
			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var pack = printTask.GetFirstDocumentPack();
				var report = pack.GetFirstReport();
				report.PrepareForRender();

				report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[1];

				var result = report.Renderer.IsCurrentSectionBodyAreaWithMoreThanOneRowData;
				Assert("Current Section Body With More Than One Row Data", result);
			}
		}

		public void TestCurrentSectionBodyAreaWithOneRowData()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=TestMacro]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody]
{B}-[<Z0_Bool>]
{A}-[#EndOfReport]");
			var documentCommand = GetDocumentCommand(template, 1);
			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var pack = printTask.GetFirstDocumentPack();
				var report = pack.GetFirstReport();
				report.PrepareForRender();

				report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[1];

				var result = report.Renderer.IsCurrentSectionBodyAreaWithMoreThanOneRowData;
				Assert("Current Section Body With One Row Data", !result);
			}
		}

		public void TestShouldShowWarningMessageWhenBeginLoopNestedInSectionBody()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew(typeof(DummyBusinessObjectLevel0)) as DummyBusinessObjectLevel0;
			child1.Z0_VarCharMax = "child-1";

			var child11 = child1.CollectionLevel1.AddNew();
			child11.Z0_VarCharMax = "child-1-1";

			var child111 = child11.CollectionLevel2.AddNew();
			child111.Z0_VarCharMax = "child-1-1-1";

			var child12 = child1.CollectionLevel1.AddNew();
			child12.Z0_VarCharMax = "child-1-2";

			var child2 = dummy.Collection.AddNew(typeof(DummyBusinessObjectLevel0)) as DummyBusinessObjectLevel0;
			child2.Z0_VarCharMax = "child-2";

			var child21 = child2.CollectionLevel1.AddNew();
			child21.Z0_VarCharMax = "child-2-1";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestBeginLoopNestedInSectionBody",
@"{A}-[#config]
{A}-[Name=TestBeginLoopNestedInSectionBody]
{A}-[DataContext=.DummyDocumentSupportable]
{A}-[#DocumentHeader]
{B}-[THIS IS DOCUMENTHEADER]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel1]
{B}-[<CollectionLevel1.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel2]
{B}-[<CollectionLevel2.Z0_VarCharMax>]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#BeginLoop:Data=CollectionLevel1]
{B}-[<CollectionLevel1.Z0_VarCharMax>]
{A}-[#EndLoop]
{A}-[#SectionFooter]
{B}-[BELOW IS SECOND SECTIONBODY]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel1]
{B}-[<CollectionLevel1.Z0_VarCharMax>]
{A}-[#EndLoop]
{A}-[#SectionFooter]
{B}-[BELOW IS THIRD SECTIONBODY]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel1]
{B}-[<CollectionLevel1.Z0_VarCharMax>]
{A}-[#SectionFooter]
{A}-[#DocumentFooter]
{B}-[THIS IS DOCUMENTFOOTER]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyDocumentSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var documentPack = printTask.GetFirstDocumentPack();
				var report = documentPack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();

				var components = converter.Components;
				AssertEquals("Should be 5 components", 5, components.Count);
				AssertEquals("The first component is document header", "THIS IS DOCUMENTHEADER", (components[0] as VisualiserComponentLabel).Caption);
				AssertEquals("The second component is grid", true, components[1] is VisualiserComponentGrid);
				AssertEquals("Should be only 2 rows as BeginLoop/EndLoop will not be ignored", 2, report.OverridingDataSet.Tables[1].Rows.Count);
				AssertEquals("The first row is child-1", "child-1", report.OverridingDataSet.Tables[1].Rows[0][0]);
				AssertEquals("The second row is child-2", "child-2", report.OverridingDataSet.Tables[1].Rows[1][0]);
				AssertEquals("The third component is the second SectionBody", "BELOW IS SECOND SECTIONBODY", (components[2] as VisualiserComponentLabel).Caption);
				AssertEquals("The fourth component is the third SectionBody", "BELOW IS THIRD SECTIONBODY", (components[3] as VisualiserComponentLabel).Caption);
				AssertEquals("The fifth component is the document footer", "THIS IS DOCUMENTFOOTER", (components[4] as VisualiserComponentLabel).Caption);
				AssertEquals("ErrorManager has 3 messages", 3, report.ErrorManager.ErrorsCount);
				AssertEquals("Messages in ErrorManager", "Severity: [Error (without error report)] Message: [Error when processing SectionForeachArea, please check documentation of SectionForeachArea for how to use it.\r\n#BeginLoop:Data=CollectionLevel1, Row: 27] Cell: [N/A] Sheetname: [(unknown)]\r\nSeverity: [Warning (without error report)] Message: [This feature can not allow users to modify data in SectionFooterArea. \r\n#BeginLoop:Data=CollectionLevel1, Row: 20\r\n#EndLoop, Row: 22] Cell: [N/A] Sheetname: [(unknown)]\r\nSeverity: [Warning (without error report)] Message: [This feature can not allow users to modify data in SectionFooterArea. \r\n#BeginLoop:Data=CollectionLevel1, Row: 7\r\n#BeginLoop:Data=CollectionLevel2, Row: 9\r\n#EndLoop, Row: 11\r\n#EndLoop, Row: 12\r\n#BeginLoop:Data=CollectionLevel1, Row: 13\r\n#EndLoop, Row: 15] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString());
			}
		}

		public void TestShouldNotShowMessageWhenBeginLoopNotNestedInSectionBody()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew(typeof(DummyBusinessObjectLevel0)) as DummyBusinessObjectLevel0;
			child1.Z0_VarCharMax = "child-1";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TestBeginLoopNotNestedInSectionBody",
@"{A}-[#config]
{A}-[Name=TestBeginLoopNotNestedInSectionBody]
{A}-[DataContext=.DummyDocumentSupportable]
{A}-[#DocumentHeader]
{B}-[THIS IS DOCUMENTHEADER]
{A}-[#DocumentFooter]
{B}-[THIS IS DOCUMENTFOOTER]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyDocumentSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var printTask = new DocumentPrintSet(documentCommand, null))
			{
				var documentPack = printTask.GetFirstDocumentPack();
				var report = documentPack.GetFirstReport();
				var converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				report.Factory.Save();

				var components = converter.Components;
				AssertEquals("Should be 2 components", 2, components.Count);
				AssertEquals("The first component is document header", "THIS IS DOCUMENTHEADER", (components[0] as VisualiserComponentLabel).Caption);
				AssertEquals("The second component is the document footer", "THIS IS DOCUMENTFOOTER", (components[1] as VisualiserComponentLabel).Caption);
				AssertEquals("Report has no errors", ReportErrorManager.HasNoErrors, report.ErrorManager.ToString());
			}
		}

		void ClearPrintJobs()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);
		}

		DocumentCommand GetDocumentCommand(StmTemplate template, int collectionCount)
		{
			var dummy = Factory.NewWithValidTestData<DummyBODocSupportable>();
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_ContactType = ContactType.Consignor.Code;
			documentCommand.SU_EmailSubjectLine = "<Collection.Z0_Bool>";

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			for (var i = 0; i < collectionCount; i++)
			{
				dummy.Collection.AddNew();
			}
			Factory.Save();

			return documentCommand;
		}

		DeliveryInstructions GetDeliveryInstructions()
		{
			var instructions = new DeliveryInstructions() { Destination = DeliveryInstructionDestination.TakenFromContact };
			instructions.IsDraft = true;
			instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			instructions.Recipients.RemoveAndDeleteAll();
			var recipient = instructions.Recipients.AddNew();
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.AttachmentType = "XLS";
			recipient.Email = "unit.test@cargowise.com";
			return instructions;
		}

		public sealed class DummyArea : Area
		{
			public DummyArea(Report report)
				: base(1, 10, report, "")
			{ }

			public override Area Clone(int position) => throw new NotImplementedException();

			public override bool CanCloseAPage => throw new NotImplementedException();

			public override ValueProviderDocumenter GetDocumentation() => throw new NotImplementedException();

			public override List<Area> Parents => throw new NotImplementedException();

			public int HeightInXlsOverride;
			public override int HeightInXls => HeightInXlsOverride == 0 ? base.HeightInXls : HeightInXlsOverride;
		}

		sealed class DummyRendererSectionBody : RendererSectionBody
		{
			public DummyRendererSectionBody(SectionBodyArea area)
				: base(area)
			{ }

			public Area AreaContainingFieldsOverride;

			protected override Area AreaContainingFields => AreaContainingFieldsOverride ?? base.AreaContainingFields;
		}
	}
}
