using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using FlexCel.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class RendererGeneralTest : RendererGeneralAbstractTest
	{
		public void TestCellsWithFormulaThatCanEvaluateToEmptyStringAreRendered()
		{
			var area = new PageHeaderArea(1, 5, TestReport, "#PageHeader");
			var renderer = new RendererGeneral(area);
			var dS = new VisualiserDataSet();

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 2] = new TFormula("=IF(FALSE,\"TestValue\", \"\")");

			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());
			AssertEquals("Pre-condition: Only 1 created component.", 1, createdComponents.Count);

			var label = (VisualiserComponentLabel)createdComponents[0];
			AssertEquals(TestReport.WorkSheetCurrentlyBeingProcessed.ToString(), "", label.Caption);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetControlTypeForCellWithWhiteSpaceAndSingleMacroReturnsStaticText()
		{
			using (var documentPack = new DocumentPack())
			{
				var excelTemplate = new ExcelTemplateForUnitTesting("OnlySingleMacrosReturnEditableComponents.xls", TestFilesSubFolder.DocumentTestFiles);
				var topLevelDataSource = Factory.New<DummyBusinessObject>();
				topLevelDataSource.Z0_NVarCharMax = "Test";

				using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				{
					using (var excel = new ExcelInterface())
					{
						excel.LoadExcelFile(excelTemplate.GetAsTemplateStream());
						AssertNotContains("Precondition: excel.WorkSheets[0].ToString()", "<Z0_NVarCharMax>", excel.WorkSheets[0].ToString());
					}

					report.WorkSheetCurrentlyBeingProcessed[7, 1] = " <Z0_NVarCharMax>";
					report.PrepareForRender();

					var sectionBodyArea = new SectionBodyArea(5, 20, report, "#SectionBody");
					var renderer = new RendererGeneral(sectionBodyArea);
					var visualiserDataSet = new VisualiserDataSet();
					var createdComponents = renderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, visualiserDataSet, 0, new List<string>());

					AssertEquals("createdComponents[1].GetType()", typeof(VisualiserComponentLabel), createdComponents[1].GetType());
					AssertEquals("createdComponents[1]).Caption", " Test", ((VisualiserComponentLabel)createdComponents[1]).Caption);
				}
			}
		}

		public void TestCellWithIfFormulaAndMacrosEvaluateToVisualiserComponents()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestField1", "TestValue1"));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestField2", "TestValue2"));

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 2] = new TFormula("=IF(TRUE, \"<TestField1>\",\"<TestField2>\")", "=IF(TRUE, \"<TestField1>\",\"<TestField2>\")");
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 2] = new TFormula("=IF(FALSE, \"<TestField1>\",C6)", "=IF(FALSE, \"<TestField1>\",C6)");
			TestReport.WorkSheetCurrentlyBeingProcessed[5, 2] = new TFormula("=UPPER(\"<TestField2>\")", "=UPPER(\"<TestField2>\")");

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();

			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals("Pre-condition: Only 3 created component.", 3, createdComponents.Count);

			var label1 = (VisualiserComponentLabel)createdComponents[0];
			AssertEquals(TestReport.WorkSheetCurrentlyBeingProcessed.ToString(), "TestValue1", label1.Caption);

			var label2 = (VisualiserComponentLabel)createdComponents[1];
			AssertEquals(TestReport.WorkSheetCurrentlyBeingProcessed.ToString(), "TESTVALUE2", label2.Caption);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExcelUpperFormulaIsEvaluatedAfterDependantCellsEvaluated()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestField1", "TestValue1"));

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 2] = new TFormula("=UPPER(C5)", "=UPPER(C5)");
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 2] = "<TestField1>";

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();

			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals("Pre-condition: TestReport.WorkSheetCurrentlyBeingProcessed[3, 2].ToString()", "=UPPER(C5)", ((TFormula)TestReport.WorkSheetCurrentlyBeingProcessed[3, 2]).Text);
			AssertEquals("Pre-condition: TestReport.WorkSheetCurrentlyBeingProcessed[4, 2].ToString()", "<TestField1>", TestReport.WorkSheetCurrentlyBeingProcessed[4, 2].ToString());
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 2] = "<TestField1>";
			AssertEquals("Pre-condition: Only 2 created component.", 2, createdComponents.Count);

			var label = createdComponents[0] as VisualiserComponentLabel;
			AssertNotNull("Pre-condition: Visualiser component created is not a label.", label);

			AssertEquals(TestReport.WorkSheetCurrentlyBeingProcessed.ToString(), "TESTVALUE1", label.GetRenderedControlValueForTesting().ToString());
			AssertEquals("TestReport.WorkSheetCurrentlyBeingProcessed[3, 2].ToString()", "=UPPER(C5)", ((TFormula)TestReport.WorkSheetCurrentlyBeingProcessed[3, 2]).Text);
			AssertEquals("TestReport.WorkSheetCurrentlyBeingProcessed[4, 2].ToString()", "<TestField1>", TestReport.WorkSheetCurrentlyBeingProcessed[4, 2].ToString());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestControlTypeForCellWithAutoHeightIsModifiableForNormalModifiableFields()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();
			var excelTemplate = new ExcelTemplateForUnitTesting("OnlySingleMacrosReturnEditableComponents.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				using (var excel = new ExcelInterface())
				{
					excel.LoadExcelFile(excelTemplate.GetAsTemplateStream());
					AssertNotContains("Precondition: excel.WorkSheets[0].ToString()", "<Z0_NVarCharMax>", excel.WorkSheets[0].ToString());
				}

				report.WorkSheetCurrentlyBeingProcessed[7, 1] = "<AutoHeight><Z0_NVarCharMax>";
				report.PrepareForRender();

				var sectionBodyArea = new SectionBodyArea(5, 20, report, "#SectionBody");
				var renderer = new RendererGeneral(sectionBodyArea);
				var visualiserDataSet = new VisualiserDataSet();
				var createdComponents = renderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, visualiserDataSet, 0, new List<string>());

				AssertEquals("createdComponents[1].GetType()", typeof(VisualiserComponentTextBox), createdComponents[1].GetType());
				AssertEquals("Make sure we've got the right field", "<Z0_NVarCharMax>", ((VisualiserComponentTextBox)createdComponents[1]).BindToName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGridStyleAlignmentsForEachAlignment()
		{
			var menuCommand = Factory.New<DocumentCommand>();
			menuCommand.SU_MenuName = "CellFormatAlignments";
			var pack = new DocumentPack(menuCommand);
			var view = new DeliverableCollectionView(pack, new DocDeliveryContactCollection(new BusinessObjectFactory()));

			var topLevelDataSource = Factory.New<DummyBOWithDecimalAndTextFields>();
			topLevelDataSource.Collection.AddNew();

			var excelTemplate = new ExcelTemplateForUnitTesting("CellFormatAlignments.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				pack.Add(report);
				var visualiserManager = new DocPackVisualiserManager(pack, view);
				var boundDataSet = report.OverridingDataSet;
				var convertor = new TemplateToVisualiserComponentsConverter(report, boundDataSet);
				AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());
				var components = convertor.Components;

				AssertNotNull("Has a grid", components.Find(delegate(VisualiserComponent match)
				{ return match is VisualiserComponentGrid; }));
				AssertEquals("Should have 8 grid styles from 4 decimals and 4 texts", 8, boundDataSet.VisualiserGridStyles.Count);

				AssertEquals("<Collection.Decimal1> (general)", HorizontalTextAlignment.General, boundDataSet.VisualiserGridStyles[0].CellFormat.HTextAlign);
				AssertEquals("<Collection.Decimal2> (left)", HorizontalTextAlignment.Left, boundDataSet.VisualiserGridStyles[1].CellFormat.HTextAlign);
				AssertEquals("<Collection.Decimal3> (center)", HorizontalTextAlignment.Centre, boundDataSet.VisualiserGridStyles[2].CellFormat.HTextAlign);
				AssertEquals("<Collection.Decimal4> (right)", HorizontalTextAlignment.Right, boundDataSet.VisualiserGridStyles[3].CellFormat.HTextAlign);

				AssertEquals("<Collection.Text1> (general)", HorizontalTextAlignment.General, boundDataSet.VisualiserGridStyles[4].CellFormat.HTextAlign);
				AssertEquals("<Collection.Text2> (left)", HorizontalTextAlignment.Left, boundDataSet.VisualiserGridStyles[5].CellFormat.HTextAlign);
				AssertEquals("<Collection.Text3> (center)", HorizontalTextAlignment.Centre, boundDataSet.VisualiserGridStyles[6].CellFormat.HTextAlign);
				AssertEquals("<Collection.Text4> (right)", HorizontalTextAlignment.Right, boundDataSet.VisualiserGridStyles[7].CellFormat.HTextAlign);

				AssertTextBoxAlignment(components, "<Decimal1>", "(general)", HorizontalTextAlignment.Right);
				AssertTextBoxAlignment(components, "<Decimal2>", "(left)", HorizontalTextAlignment.Left);
				AssertTextBoxAlignment(components, "<Decimal3>", "(center)", HorizontalTextAlignment.Centre);
				AssertTextBoxAlignment(components, "<Decimal4>", "(right)", HorizontalTextAlignment.Right);

				AssertTextBoxAlignment(components, "<Text1>", "(general)", HorizontalTextAlignment.Left);
				AssertTextBoxAlignment(components, "<Text2>", "(left)", HorizontalTextAlignment.Left);
				AssertTextBoxAlignment(components, "<Text3>", "(center)", HorizontalTextAlignment.Centre);
				AssertTextBoxAlignment(components, "<Text4>", "(right)", HorizontalTextAlignment.Right);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnlySingleMacrosReturnEditableComponents()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();

			var excelTemplate = new ExcelTemplateForUnitTesting("OnlySingleMacrosReturnEditableComponents.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();

				var sectionBodyArea = new SectionBodyArea(5, 20, report, "#SectionBody");

				var renderer = new RendererGeneral(sectionBodyArea);

				var visualiserDataSet = new VisualiserDataSet();
				var createdComponents = renderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, visualiserDataSet, 0, new List<string>());

				AssertEquals("Precondition: createdComponents.Count", 13, createdComponents.Count);
				AssertEquals("createdComponents[0].GetType()", typeof(VisualiserComponentTextBox), createdComponents[0].GetType());
				AssertEquals("createdComponents[1].GetType()", typeof(VisualiserComponentLabel), createdComponents[1].GetType());
				AssertEquals("createdComponents[2].GetType()", typeof(VisualiserComponentLabel), createdComponents[2].GetType());
				AssertEquals("createdComponents[3].GetType()", typeof(VisualiserComponentLabel), createdComponents[3].GetType());
				AssertEquals("createdComponents[4].GetType()", typeof(VisualiserComponentLabel), createdComponents[4].GetType());
				AssertEquals("createdComponents[5].GetType()", typeof(VisualiserComponentTextBox), createdComponents[5].GetType());
				AssertEquals("createdComponents[6].GetType()", typeof(VisualiserComponentTextBox), createdComponents[6].GetType());
				AssertEquals("createdComponents[7].GetType()", typeof(VisualiserComponentTextBox), createdComponents[7].GetType());
				AssertEquals("createdComponents[8].GetType()", typeof(VisualiserComponentTextBox), createdComponents[8].GetType());
				AssertEquals("createdComponents[9].GetType()", typeof(VisualiserComponentTextBox), createdComponents[9].GetType());
				AssertEquals("createdComponents[10].GetType()", typeof(VisualiserComponentTextBox), createdComponents[10].GetType());
				AssertEquals("createdComponents[11].GetType()", typeof(VisualiserComponentTextBox), createdComponents[11].GetType());
				AssertEquals("createdComponents[12].GetType()", typeof(VisualiserComponentTextBox), createdComponents[12].GetType());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetLabelTextProperlyUsesCellFormattingForDates()
		{
			var topLevelDataSource = Factory.New<DummyBOWithManyDateFields>();

			var excelTemplate = new ExcelTemplateForUnitTesting("DateTimeCellFormat.xls", TestFilesSubFolder.DocumentTestFiles);
			var testReport = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
			testReport.PrepareForRender();

			var sectionBodyArea = new SectionBodyArea(5, 11, testReport, "#SectionBody");

			var renderer = new RendererGeneral(sectionBodyArea);

			var dS = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(testReport.WorkSheetCurrentlyBeingProcessed, dS, 0, new List<string>());

			AssertEquals("Precondition: createdComponents.Count", 6, createdComponents.Count);
			AssertEquals("Precondition: DS.MainTable.Rows.Count", 6, dS.MainTable.Columns.Count);
			AssertBindingField(createdComponents, 0, "<Date1>");
			AssertEquals("DS.MainRow[\"<Date1>\"]", "1 January 2006", dS.MainRow["<Date1>"]);
			AssertBindingField(createdComponents, 1, "<Date2>");
			AssertEquals("DS.MainRow[\"<Date2>\"]", "2/02/2006", dS.MainRow["<Date2>"]);
			AssertBindingField(createdComponents, 2, "<Date3>");
			AssertEquals("DS.MainRow[\"<Date3>\"]", "3/3/06", dS.MainRow["<Date3>"]);
			AssertBindingField(createdComponents, 3, "<Date4>");
			AssertEquals("DS.MainRow[\"<Date4>\"]", "2006/04/04", dS.MainRow["<Date4>"]);
			AssertBindingField(createdComponents, 4, "<Date5>");
			AssertEquals("DS.MainRow[\"<Date5>\"]", "05-May-06", dS.MainRow["<Date5>"]);
			AssertBindingField(createdComponents, 5, "<Date6>");
			AssertEquals("DS.MainRow[\"<Date6>\"]", "06-Jun-06", dS.MainRow["<Date6>"]);

			testReport.Dispose();
			createdComponents.Clear();
		}

		public void TestConstructorWontAcceptNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{ new RendererGeneral(null); });
		}

		public override void TestGetHeight()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");
			var renderer = new RendererGeneral(parentArea);
			AssertEquals(2300, renderer.GetHeightInXL());
		}

		public void TestCreateComponentsAndDataContainers_TVCTextBox()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestField1", "TestValue1"));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestField2", "TestValue2"));
			TestReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestField3", "TestValue3"));

			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<TestField1>";
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 7] = "<Modifiable(<TestField2> <TestField3>)>";

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals(2, dS.MainTable.Columns.Count);
			AssertEquals("TestValue1", dS.MainRow["<TestField1>"]);
			AssertEquals("TestValue2 TestValue3", dS.MainRow["<Modifiable(<TestField2> <TestField3>)>"]);

			AssertEquals(2, createdComponents.Count);

			var componentTextBox1 = createdComponents[0] as VisualiserComponentTextBox;
			AssertNotNull("componentTextBox1", componentTextBox1);

			AssertKindaEquals("componentTextBox1.Location", ControlDpiScalingHelper.NewScaledPoint(80, 11), componentTextBox1.Location);
			AssertKindaEquals("componentTextBox1.Size", ControlDpiScalingHelper.NewScaledSize(463, 18), componentTextBox1.Size);
			AssertEquals("componentTextBox1.BindToName", "<TestField1>", componentTextBox1.BindToName);

			var componentTextBox2 = createdComponents[1] as VisualiserComponentTextBox;
			AssertNotNull("componentTextBox2", componentTextBox2);
			AssertKindaEquals("componentTextBox2.Location", ControlDpiScalingHelper.NewScaledPoint(470, 32), componentTextBox2.Location);
			AssertKindaEquals("componentTextBox2.Size", ControlDpiScalingHelper.NewScaledSize(73, 18), componentTextBox2.Size);
			AssertEquals("<Modifiable(<TestField2> <TestField3>)>", componentTextBox2.BindToName);
		}

		public void TestCreateComponentsAndDataContainers_TVCLabel()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			TestReport.WorkSheetCurrentlyBeingProcessed[4, 4] = "TestConstant";

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals(0, dS.MainTable.Columns.Count);

			AssertEquals(1, createdComponents.Count);

			var componentLabel = createdComponents[0] as VisualiserComponentLabel;
			AssertNotNull("componentLabel", componentLabel);
			AssertEquals(typeof(VisualiserComponentLabel), componentLabel.GetType());
			AssertKindaEquals("componentLabel.Location", ControlDpiScalingHelper.NewScaledPoint(236, 53), componentLabel.Location);
			AssertKindaEquals("componentLabel.Size", ControlDpiScalingHelper.NewScaledSize(75, 18), componentLabel.Size);
			AssertEquals("TestConstant", componentLabel.Caption);
		}

		public void TestCreateComponentsAndDataContainers_TVCImage()
		{
			Registry.Business.SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(5, 5));

			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			TestReport.Renderer.CurrentAreaToProcess = parentArea;
			TestReport.Renderer.SaveOriginalColumnWidths();
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "<Image(CompanyLogo, 1,2)>";

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals(0, dS.MainTable.Columns.Count);

			AssertEquals(1, createdComponents.Count);

			AssertEquals(typeof(VisualiserComponentImage), createdComponents[0].GetType());
			AssertEquals(ControlDpiScalingHelper.NewScaledPoint(78, 9), createdComponents[0].Location);
			AssertEquals(ControlDpiScalingHelper.NewScaledSize(156, 21), createdComponents[0].Size);
		}

		public void TestCreateComponentsAndDataContainers_Border()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			var borderFormat = new CellFormat();
			borderFormat.Borders.Bottom.BorderStyle = CellBorderStyle.None;
			borderFormat.Borders.Left.BorderStyle = CellBorderStyle.None;
			borderFormat.Borders.Right.BorderStyle = CellBorderStyle.None;
			borderFormat.Borders.Top.BorderStyle = CellBorderStyle.None;
			TestReport.WorkSheetCurrentlyBeingProcessed[2, 2] = "";
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(2, 2, borderFormat);

			borderFormat.Borders.Bottom.BorderStyle = CellBorderStyle.Medium;
			TestReport.WorkSheetCurrentlyBeingProcessed[3, 3] = "";
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(3, 3, borderFormat);
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 4] = "TestConstant";
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(4, 4, borderFormat);

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals("createdComponents.Count", 5, createdComponents.Count);
			createdComponents.Sort((x, y) => x.Location.Y == y.Location.Y ? x.Location.X.CompareTo(y.Location.X) : x.Location.Y.CompareTo(y.Location.Y));

			AssertEquals("createdComponents[0].GetType()", typeof(VisualiserComponentBorder), createdComponents[0].GetType());
			AssertEquals("createdComponents[1].GetType()", typeof(VisualiserComponentBorder), createdComponents[1].GetType());
			AssertEquals("createdComponents[2].GetType()", typeof(VisualiserComponentBorder), createdComponents[2].GetType());
			AssertEquals("createdComponents[3].GetType()", typeof(VisualiserComponentLabel), createdComponents[3].GetType());
			AssertEquals("createdComponents[4].GetType()", typeof(VisualiserComponentBorder), createdComponents[4].GetType());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetImagesPresentInArea()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			var image = new ExcelImage(System.Drawing.Image.FromFile(UnitTestingConstants.TestFilesDir + "Soap Bubbles.bmp"), 10, 10, "InsertedImage");
			TestReport.WorkSheetCurrentlyBeingProcessed.InsertPicture(2, 2, 1, 1, image);

			var renderer = new RendererGeneral(parentArea);
			var dataSet = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dataSet, 110, new List<string>());

			AssertEquals("createdComponents.Count", 1, createdComponents.Count);
			var point = ControlDpiScalingHelper.NewScaledPoint(78, 9);
			var size = ControlDpiScalingHelper.NewScaledSize(78, 21);

			AssertEquals("createdComponents[0].Location", point, createdComponents[0].Location);
			AssertEquals("createdComponents[0].Size", size, createdComponents[0].Size);

			TestReport.WorkSheetCurrentlyBeingProcessed.MergeCells(1, 1, 3, 3);
			createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dataSet, 110, new List<string>());
			AssertEquals("createdComponents[0].Location", point, createdComponents[0].Location);
			AssertEquals("createdComponents[0].Size", size, createdComponents[0].Size);
		}

		public void TestMergeCellInTheFirstRow()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");

			var borderFormat = new CellFormat();
			borderFormat.Borders.Bottom.BorderStyle = CellBorderStyle.None;
			borderFormat.Borders.Left.BorderStyle = CellBorderStyle.None;
			borderFormat.Borders.Right.BorderStyle = CellBorderStyle.None;
			borderFormat.Borders.Top.BorderStyle = CellBorderStyle.None;

			TestReport.WorkSheetCurrentlyBeingProcessed[3, 3] = "TestConstant";
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(3, 3, borderFormat);
			TestReport.WorkSheetCurrentlyBeingProcessed[5, 5] = "TestConstant";
			TestReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(5, 5, borderFormat);
			TestReport.WorkSheetCurrentlyBeingProcessed.MergeCells(0, 3, 3, 3);

			var renderer = new RendererGeneral(parentArea);
			var dS = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dS, 110, new List<string>());

			AssertEquals("createdComponents.Count", 2, createdComponents.Count);
		}

		public void TestGetControlTypeForModifiableCellWithFormatting()
		{
			AssertCellWithFormattingYieldsComponentType("<Modifiable(Blah)>", VisualiserComponentTypes.TextEdit);
			AssertCellWithFormattingYieldsComponentType("<ModifiableField(\"Blah\")>", VisualiserComponentTypes.TextEdit);
			AssertCellWithFormattingYieldsComponentType("Blah", VisualiserComponentTypes.StaticText);
		}

		[ExpectNoExceptions]
		public void TestRenderSection_ShouldSetCurrentColumnOnReport()
		{
			var parentArea = new PageHeaderArea(1, 10, TestReport, "#PageHeader");
			var reportRenderer = new Mock<IReportRenderer>();

			TestReport.Renderer = reportRenderer.Object;
			TestReport.WorkSheetCurrentlyBeingProcessed[4, 4] = "TestConstant";

			var renderer = new RendererGeneral(parentArea);
			var dataSet = new VisualiserDataSet();
			var createdComponents = renderer.RenderSection(TestReport.WorkSheetCurrentlyBeingProcessed, dataSet, 110, new List<string>());

			for (var i = 0; i < 26; i++)
			{
				reportRenderer.VerifySet(r => r.CurrentColumn = i);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleVisualiserComponentsWithSameMacrosInDifferentCultures()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();

			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleVisualiserComponentsWithSameMacrosInDifferentCultures.xlsx", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(topLevelDataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();

				var sectionBodyArea = new SectionBodyArea(5, 9, report, "#SectionBody");

				var renderer = new RendererGeneral(sectionBodyArea);

				var visualiserDataSet = new VisualiserDataSet();
				var createdComponents = renderer.RenderSection(report.WorkSheetCurrentlyBeingProcessed, visualiserDataSet, 0, new List<string>());

				AssertEquals(2, createdComponents.Count);
				AssertEquals(2, visualiserDataSet.MainTable.Columns.Count);
			}
		}

		static void AssertTextBoxAlignment(List<VisualiserComponent> components, string fieldName, string templateCellAlignment, HorizontalTextAlignment expectedAlignment)
		{
			var textBoxComponent = (VisualiserComponentTextBox)components.Find(delegate(VisualiserComponent match)
			{ return match is VisualiserComponentTextBox && ((VisualiserComponentTextBox)match).BindToName == fieldName; });
			AssertNotNull("Has a Text Box for " + fieldName, textBoxComponent);
			AssertEquals("HTextAlign for " + fieldName + " field where the Excel Cell Alignment is " + templateCellAlignment, expectedAlignment, textBoxComponent.CellFormat.HTextAlign);
		}

		void AssertBindingField(List<VisualiserComponent> createdComponents, int index, string expectedBinding)
		{
			AssertEquals("Precondition: createdComponents[" + index + "].BindToName"
				, expectedBinding
				, ((VisualiserComponentTextBox)createdComponents[index]).BindToName);
		}

		void AssertKindaEquals(string message, Point expected, Point actual)
		{
			AssertKindaEquals(message, expected, expected.X, expected.Y, actual, actual.X, actual.Y);
		}

		void AssertKindaEquals(string message, Size expected, Size actual)
		{
			AssertKindaEquals(message, expected, expected.Width, expected.Height, actual, actual.Width, actual.Height);
		}

		void AssertKindaEquals(string message, object expected, int expectedX, int expectedY, object actual, int actualX, int actualY)
		{
			var assertionMessage = string.Format("Message: {0}<br />Expected: {1}<br />Actual: {2}", message, HtmlFormatGoodValue(expected), HtmlFormatBadValue(actual));

			var xIsOk = Math.Abs(expectedX - actualX) <= 1;
			var yIsOk = Math.Abs(expectedY - actualY) <= 1;

			HtmlAssert(assertionMessage, yIsOk && xIsOk);
		}

		void AssertCellWithFormattingYieldsComponentType(string cellContent, VisualiserComponentTypes expectedType)
		{
			var area = new PageHeaderArea(1, 10, TestReport, "#PageHeader");
			var renderer = new RendererGeneral(area);
			var componentType = renderer.GetControlType(cellContent, new CellFormat { FillPattern = FillPatternStyle.Solid });
			AssertEquals(expectedType, componentType);
		}

		sealed class DummyBOWithDecimalAndTextFields : DummyBusinessObject
		{
			public DummyBOWithDecimalAndTextFields(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			DummyChildBOWithDecimalAndTextFieldsCollection collection;
			internal new DummyChildBOWithDecimalAndTextFieldsCollection Collection
				=> collection ?? (collection = new DummyChildBOWithDecimalAndTextFieldsCollection(Factory));

			public ZDecimal Decimal1 => 1m;

			public ZDecimal Decimal2 => 2m;

			public ZDecimal Decimal3 => 3m;

			public ZDecimal Decimal4 => 4m;

			public ZString Text1 => "Text1";

			public ZString Text2 => "Text2";

			public ZString Text3 => "Text3";

			public ZString Text4 => "Text4";
		}

		sealed class DummyChildBOWithDecimalAndTextFieldsCollection : BusinessObjectCollection<DummyBOWithDecimalAndTextFields>
		{
			public DummyChildBOWithDecimalAndTextFieldsCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		sealed class DummyBOWithManyDateFields : DummyBODocSupportable
		{
			public DummyBOWithManyDateFields(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDateTime Date1 => new ZDateTime(2006, 1, 1);

			public ZDateTime Date2 => new ZDateTime(2006, 2, 2);

			public ZDateTime Date3 => new ZDateTime(2006, 3, 3);

			public ZDateTime Date4 => new ZDateTime(2006, 4, 4);

			public ZDateTime Date5 => new ZDateTime(2006, 5, 5);

			public ZDateTimeOffset Date6 => new ZDateTimeOffset(new ZDateTime(2006, 6, 6));
		}
	}
}
