using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class TemplateToVisualiserComponentsConverterTest : TestCaseWithFactory
	{
		public void TestComponentsForModifiableFieldColumnNotInVisualiserDataSet()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "",
@"{A}-[#Config]
{A}-[Name=TestMacro]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[#SectionBody:Data=Collection]
{B}-[<ModifiableField(""ABCD"", ""DEF"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.NewWithValidTestData<DummyBODocSupportable>();
			dummy.Collection.AddNew();
			var dataProvider = new DataProviderList(BODocDataProvider.Get(dummy));

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate, dataProvider, "Test", null, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();

				var field = "Collection";
				var tableName = VisualiserDataSet.GetTableName(field);

				var overridingDataSet = new VisualiserDataSet();
				var table = overridingDataSet.Tables.Add(tableName);
				var row = table.NewRow();
				table.Rows.Add(row);

				var bodyArea = report.Analyser.Sections[0].DataAreas[0] as SectionBodyArea;
				bodyArea.SetDataSource(new VisualiserDataSource(overridingDataSet, tableName));

				var converter = new TemplateToVisualiserComponentsConverter(report, overridingDataSet);
				AssertNotNull("converter.Components", converter.Components);
			}
		}

		[ExpectNoExceptions]
		public void TestComponentsForTotalMacroWithTrailingSpaceDoesNotCauseException()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>]
{A}-[#SectionFooter]
{B}-[<Total Collection.Z0_Number> ]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBusinessObject>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					var dataSet = new VisualiserDataSet();
					var converter = new TemplateToVisualiserComponentsConverter(report, dataSet);
					AssertNotNull("converter.Components", converter.Components);
				}
			}
		}

		public void TestComponentsForTotalMacroWithFormulaInSectionFooterAreaDoesNotCauseException()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>]
{A}-[#SectionFooter]
{B}-[<Total Collection.Z0_Number>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBusinessObject>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					report.WorkSheetCurrentlyBeingProcessed[6, 3] = new TFormula("=\"<Total Collection.Z0_Number>\"", "<Total Collection.Z0_Number>");

					var dataSet = new VisualiserDataSet();
					var converter = new TemplateToVisualiserComponentsConverter(report, dataSet);
					AssertNoExceptionThrown(() => _ = converter.Components);
				}
			}
		}

		public void TestComponentsLanguageFollowsReportLanguage()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>]
{A}-[#SectionFooter]
{B}-[<Total Collection.Z0_Number> ]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBusinessObject>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					documentPack.Language = Core.SharedConstants.Languages.ChineseSimplified;
					var dataSet = new VisualiserDataSet();
					var converter = new TemplateToVisualiserComponentsConverter(report, dataSet);
					AssertNotNull("converter.Components", converter.Components);
					AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, converter.LanguageWhenLoadingComponents);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestComponentsForTotalMacroWithInAStringDoesNotCauseException()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", "",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Number>]
{A}-[#SectionFooter]
{B}-[<Total Collection.Z0_Number> World]
{B}-[Hello <Total Collection.Z0_Number>]
{B}-[Hello <Total Collection.Z0_Number> World]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyBusinessObject>();

			var child1 = dummy.Collection.AddNew();
			child1.Z0_Number = 1;

			var child2 = dummy.Collection.AddNew();
			child2.Z0_Number = 2;

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, excelTemplate, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					var dataSet = new VisualiserDataSet();
					var converter = new TemplateToVisualiserComponentsConverter(report, dataSet);
					AssertNotNull("converter.Components", converter.Components);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVisualiserProcessesSectionBodyAndGroupByAreasBeforeAllOthersToEnsureReferencedFieldsHaveAlreadyBeenSetBeforeBeingProcessed()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			AddChildToDummyObject(dummyBO, false, 3.14m, "foo");
			AddChildToDummyObject(dummyBO, true, 3.14m, "bar");
			AddChildToDummyObject(dummyBO, false, 3.14m, "foo");
			AddChildToDummyObject(dummyBO, true, 7m, "bar");
			AddChildToDummyObject(dummyBO, false, 3.14m, "bar");
			AddChildToDummyObject(dummyBO, true, 31m, "bar");

			var menuItem = Factory.New<StmMenuItem>();
			var documentPack = new DocumentPack(menuItem);
			var template = new ExcelTemplateForUnitTesting("VisualiserProcessesSectionBodyAndGroupByAreasBeforeAllOthersToEnsureReferencedFieldsHaveAlreadyBeenSetBeforeBeingProcessed.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummyBO), "", null, DocumentDirection.ANY, false))
			{
				documentPack.Add(report);
				var templateToVCConverter = new TemplateToVisualiserComponentsConverter(report, new VisualiserDataSet());
				AssertNoExceptionThrown(delegate
				{ List<VisualiserComponent> templateVisualComponents = templateToVCConverter.Components; });
				AssertEquals("Precondition: templateToVCConverter.Components.Count", 2, templateToVCConverter.Components.Count);
				var componentLabel = templateToVCConverter.Components[0] as VisualiserComponentLabel;
				AssertNotNull("componentLabel", componentLabel);
				AssertEquals("groupCountLabelControl.Text", "3", componentLabel.Caption);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVisualiserDoesntBarfWhenFlexcelCantReadAnImage()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			var dummyChildBO = dummyBO.Collection.AddNew();
			var menuItem = Factory.New<StmMenuItem>();
			var documentPack = new DocumentPack(menuItem);
			var template = new ExcelTemplateForUnitTesting("VisualisationWithDudImages.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummyBO), "", null, DocumentDirection.ANY, false))
			{
				documentPack.Add(report);
				var templateToVCConverter = new TemplateToVisualiserComponentsConverter(report, new VisualiserDataSet());
				AssertNoExceptionThrown(delegate
				{ var templateVisualComponents = templateToVCConverter.Components; });
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVisualiserDoesntRepeatImages()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Collection.AddNew();

			var tester = new ConverterTester(Factory, "VisualisationWithImageInHeader.xls", BODocDataProvider.Get(dummyBO));
			tester.RunAndAssertControlsDescriptionEquals(
@"At: {X=2,Y=2}
Size: {Width=204, Height=18}
TextBox Bound To: <Z0_Code>

At: {X=0,Y=21}
Size: {Width=209, Height=106}
Image Size: {Width=476, Height=218}
Image V.Scale: 96 DPI
Image H.Scale: 96 DPI

At: {X=2,Y=~139}
Size: {Width=204, Height=18}
TextBox Bound To: <Z0_Description>

At: {X=0,Y=159}
Size: {Width=209, Height=120}
Grid Bound To: VisualiserTable_Collection
Columns: Z0_VarCharMax, Z0_Number, Z0_FK_Code, Z0_NVarCharMax

At: {X=2,Y=281}
Size: {Width=204, Height=18}
TextBox Bound To: <Z0_NVarChar>
");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestVisualiserDealsWithFieldsInGroupHeaders()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Collection.AddNew();

			var tester = new ConverterTester(Factory, "VisualisationTesting.xls", BODocDataProvider.Get(dummyBO));
			tester.RunAndAssertControlsDescriptionEquals(
@"At: {X=2,Y=2}
Size: {Width=204, Height=18}
TextBox Bound To: <Z0_Code>

At: {X=2,Y=~23}
Size: {Width=204, Height=18}
TextBox Bound To: <Z0_Description>

At: {X=0,Y=43}
Size: {Width=209, Height=120}
Grid Bound To: VisualiserTable_Collection
Columns: Z0_VarCharMax, Z0_Number, Z0_FK_Code, Z0_NVarCharMax

At: {X=2,Y=165}
Size: {Width=204, Height=18}
TextBox Bound To: <Z0_NVarChar>
");
		}

		DummyChildBusinessObject AddChildToDummyObject(DummyBusinessObject topLevelDataSource, ZBool valueBool, ZDecimal valueDecimal, ZString valueNText)
		{
			var result = topLevelDataSource.Collection.AddNew();
			result.Z0_Bool = valueBool;
			result.Z0_Decimal = valueDecimal;
			result.Z0_NVarCharMax = valueNText;
			return result;
		}

		sealed class ConverterTester
		{
			public ConverterTester(BusinessObjectFactory factory, string templateFileName)
			{
				Factory = factory;
				TemplateFileName = templateFileName;
			}

			public ConverterTester(BusinessObjectFactory factory, string templateFileName, IBODocDataProvider boDataSource)
				: this(factory, templateFileName)
			{
				BODataSource = boDataSource;
			}
			readonly BusinessObjectFactory Factory;
			readonly string TemplateFileName;
			readonly IBODocDataProvider BODataSource;

			public void RunAndAssertControlsDescriptionEquals(string expectedControlsDescription)
			{
				var menuItem = Factory.New<StmMenuItem>();
				var documentPack = new DocumentPack(menuItem);
				var template = new ExcelTemplateForUnitTesting(TemplateFileName, TestFilesSubFolder.DocumentTestFiles);
				using (Report report = GetReport(documentPack, template))
				{
					documentPack.Add(report);
					AssertVisualiserControlsRendered(report, expectedControlsDescription);
				}
			}

			void AssertVisualiserControlsRendered(Report report, string expectedControlsDescription)
			{
				var templateToVCConverter = new TemplateToVisualiserComponentsConverter(report, new VisualiserDataSet());
				var templateVisualComponents = templateToVCConverter.Components;

				var templateResult = string.Join("\r\n", templateVisualComponents.Select(component => component.GetControlDescriptionForTesting()));

				if (!TemplateIsEqual(expectedControlsDescription, templateResult))
				{
					//Easier than formatting it myself
					AssertEquals(expectedControlsDescription, templateResult);
				}
				else
				{
					Assert(true);
				}
			}

			bool TemplateIsEqual(string expected, string actual)
			{
				// Numbers prefixed with a '~' get a 1px tolerance when comparing. This is to allow for minor rounding errors, like what you tend to get with TextBox

				if (expected == actual)
				{ return true; }
				if (!IsOnlyNumericDifferences(expected, actual))
				{ return false; }

				var allNumbersExpected = Regex.Matches(expected, @"~?\d+");
				var allNumbersActual = Regex.Matches(actual, @"\d+");

				for (var i = 0; i < allNumbersExpected.Count; i++)
				{
					var expectedNumberString = allNumbersExpected[i].Value;
					var actualNumberString = allNumbersActual[i].Value;

					if (!expectedNumberString.StartsWith("~") && expectedNumberString != actualNumberString)
					{
						return false;
					}

					var expectedNumber = int.Parse(expectedNumberString.TrimStart('~'));
					var actualNumber = int.Parse(actualNumberString);

					if (Math.Abs(expectedNumber - actualNumber) > 1)
					{
						return false;
					}
				}

				return true;
			}

			bool IsOnlyNumericDifferences(string expected, string actual)
			{
				return Regex.Replace(expected, "~?\\d+", "0") == Regex.Replace(actual, "~?\\d+", "0");
			}

			Report GetReport(DocumentPack documentPack, ExcelTemplateForUnitTesting template)
			{
				return BODataSource == null ? new Report(documentPack, template) // Report Style
					: new Report(documentPack, template, BODataSource, "", null, DocumentDirection.ANY, false); // Document Style
			}
		}
	}
}
