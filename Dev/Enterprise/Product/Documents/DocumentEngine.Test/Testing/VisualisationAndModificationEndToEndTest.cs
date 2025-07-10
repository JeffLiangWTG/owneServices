using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class VisualisationAndModificationEndToEndTest : TestCaseWithFactory
	{
		public void TestModifyingDoesNotMessUpDocumentsThatProduceMultipleDocumentWrappers()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("CH1");
			dummy.Collection.AddNew("CH2");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=DummyChildren]
{A}-[#SectionBody]
{C}-[<Code>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "DummyChildren";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				AssertEquals("There should be two documents in the pack.", 2, documentPack.Count);
				AssertMultilineASCIIEquals("This report should render the code for the first child.", @"{C}-[CH1]", GetReportOutputForTesting((Report)documentPack[0]));
				AssertMultilineASCIIEquals("This report should render the code for the second child.", @"{C}-[CH2]", GetReportOutputForTesting((Report)documentPack[1]));

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.Email = "unit.test@cargowise.com";

				var visualizerManager = new DocPackVisualiserManager(deliveryInstructions.DocPack, deliveryInstructions.DeliverablesToBePrinted);

				var view = new Mock<IVisualizerView>();

				var reportView1 = new Mock<IVisualizedReportView>();
				var reportView2 = new Mock<IVisualizedReportView>();
				var drawer = new Mock<IVisualiserDrawer>();

				view.SetupSequence(m => m.GetNewReportView()).Returns(reportView1.Object).Returns(reportView2.Object);
				reportView1.Setup(m => m.ControlDrawer).Returns(drawer.Object);
				reportView1.Setup(m => m.BorderDrawer).Returns(drawer.Object);

				reportView2.Setup(m => m.ControlDrawer).Returns(drawer.Object);
				reportView2.Setup(m => m.BorderDrawer).Returns(drawer.Object);

				var controller = new VisualizerViewController(visualizerManager, view.Object);

				view.Raise(m => m.Load += null, EventArgs.Empty);

				reportView1.Raise(m => m.FirstShown += null, EventArgs.Empty);
				reportView2.Raise(m => m.FirstShown += null, EventArgs.Empty);

				var report1 = visualizerManager.Reports.ElementAt(0);
				report1.OverridingDataSet.MainRow["<Code>"] = "CH1";
				report1.Factory.Save();
				view.Raise(m => m.SaveButtonClicked += null, EventArgs.Empty);

				AssertMultilineASCIIEquals("This report should render the code for the first child.", @"{C}-[CH1]", GetReportOutputForTesting((Report)documentPack[0]));
				AssertMultilineASCIIEquals("This report should render the code for the second child.", @"{C}-[CH2]", GetReportOutputForTesting((Report)documentPack[1]));
				view.VerifyAll();
				reportView1.VerifyAll();
				reportView2.VerifyAll();
				drawer.VerifyAll();
			}
		}

		string GetReportOutputForTesting(Report report)
		{
			var result = new StringBuilder();

			using (var stream = new MemoryStream())
			{
				report.Save(stream);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(stream);

					foreach (var workSheet in excelInterface.WorkSheets)
					{
						result.AppendLine(workSheet.ToString());
					}
				}
			}

			return result.ToString();
		}

		public void TestFallbackIfEmpty()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "the answer";
			dummyBO.Z0_NVarCharMax = "not the answer!";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<FallbackIfEmpty(\"<Z0_VarCharMax>\",\"<Z0_NVarCharMax>\")>", "{B}-[the answer]"
				, typeof(VisualiserComponentTextBox), "the answer", "<FallbackIfEmpty(\"<Z0_VarCharMax>\",\"<Z0_NVarCharMax>\")>", "what's the question?", "{B}-[what's the question?]");

			dummyBO.Z0_VarCharMax = "";
			dummyBO.Z0_NVarCharMax = "not the answer!";

			testHelper.EndToEnd("<FallbackIfEmpty(\"<Z0_VarCharMax>\",\"<Z0_NVarCharMax>\")>", "{B}-[not the answer!]"
				, typeof(VisualiserComponentTextBox), "not the answer!", "<FallbackIfEmpty(\"<Z0_VarCharMax>\",\"<Z0_NVarCharMax>\")>", "what's the question?", "{B}-[what's the question?]");
		}

		[TestDate(2008, 1, 1, 1, 0, 0)]
		public void TestFormatWithNowMacroInIt()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "the answer";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format(<Now>)>", "{B}-[01-Jan-08 01:00:00]"
				, typeof(VisualiserComponentTextBox), "01-Jan-08 01:00:00", "<Format(<Now>)>", "what's the question?", "{B}-[what's the question?]");
		}

		public void TestMacroModifiableWithFormatMacroInside()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "the answer";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Modifiable(<Z0_VarCharMax> : <Format({Z0_VarCharMax:Upper})>)>", "{B}-[the answer : THE ANSWER]"
				, typeof(VisualiserComponentTextBox), "the answer : THE ANSWER", "<Modifiable(<Z0_VarCharMax> : <Format({Z0_VarCharMax:Upper})>)>", "what's the question?", "{B}-[what's the question?]");
		}

		public void TestMacroModifiableWithSingleMacro()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "the answer";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Modifiable(<Z0_VarCharMax>)>", "{B}-[the answer]"
				, typeof(VisualiserComponentTextBox), "the answer", "<Modifiable(<Z0_VarCharMax>)>", "what's the question?", "{B}-[what's the question?]");
		}

		public void TestMacroModifiable()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "the answer";
			DummyBusinessObject relatedDummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Guid = relatedDummyBO.PK;
			dummyBO.RelatedDummy.Z0_VarCharMax = "DADADEE";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Modifiable(<Z0_VarCharMax> : <RelatedDummy.Z0_VarCharMax>)>", "{B}-[the answer : DADADEE]"
				, typeof(VisualiserComponentTextBox), "the answer : DADADEE", "<Modifiable(<Z0_VarCharMax> : <RelatedDummyZ0_VarCharMax>)>", "what's the question?", "{B}-[what's the question?]");
		}

		public void TestMacroNonModifiable()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "foo bar";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<NonModifiable><Z0_VarCharMax>", "{B}-[foo bar]"
				, typeof(VisualiserComponentLabel), "foo bar");
		}

		public void TestColumnName()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			DummyBusinessObject relatedDummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Guid = relatedDummyBO.PK;
			dummyBO.RelatedDummy.Z0_VarCharMax = "DADADEE";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<RelatedDummy.Format(\"...{Z0_VarCharMax}...\")>", "{B}-[...DADADEE...]"
				, typeof(VisualiserComponentTextBox), "...DADADEE...", "<RelatedDummyFormat(\"{Z0_VarCharMax}\")>", "DA-DUM", "{B}-[DA-DUM]");
		}

		public void TestPropertyOfRelatedObject()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			DummyBusinessObject relatedDummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Guid = relatedDummyBO.PK;
			dummyBO.RelatedDummy.Z0_VarCharMax = "DADADEE";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<RelatedDummy.Z0_VarCharMax>", "{B}-[DADADEE]"
				, typeof(VisualiserComponentTextBox), "DADADEE", "<RelatedDummyZ0_VarCharMax>", "DA-DUM", "{B}-[DA-DUM]");
		}

		public void TestPropertyOfRelatedObjectViaFormat()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			DummyBusinessObject relatedDummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Guid = relatedDummyBO.PK;
			dummyBO.RelatedDummy.Z0_VarCharMax = "DADADEE";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<RelatedDummy.Format(\"{Z0_VarCharMax}\")>", "{B}-[DADADEE]"
				, typeof(VisualiserComponentTextBox), "DADADEE", "<RelatedDummyFormat(\"{Z0_VarCharMax}\")>", "DA-DUM", "{B}-[DA-DUM]");
		}

		public void TestMultipleBOFieldsContainedInOneFormatMacro()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Number = 42;
			dummyBO.Z0_VarCharMax = "the answer";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format(\"{Z0_VarCharMax} : {Z0_Number}\")>", "{B}-[the answer : 42]"
				, typeof(VisualiserComponentTextBox), "the answer : 42", "<Format(\"{Z0_VarCharMax} : {Z0_Number}\")>", "what's the question?", "{B}-[what's the question?]");
		}

		public void TestMultipleMacrosContainedInOneOuterMacro()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "the answer";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format(\"{Z0_VarCharMax} : <NumberToWords(42)>\")>", "{B}-[the answer : forty two]"
				, typeof(VisualiserComponentTextBox), "the answer : forty two", "<Format(\"{Z0_VarCharMax} : <NumberToWords(42)>\")>", "what's the question?", "{B}-[what's the question?]");

			testHelper.EndToEnd("<Format(\"{Z0_VarCharMax} : <NumberToWords(42)> <NumberToWords(42)>\")>", "{B}-[the answer : forty two forty two]"
				, typeof(VisualiserComponentTextBox), "the answer : forty two forty two", "<Format(\"{Z0_VarCharMax} : <NumberToWords(42)> <NumberToWords(42)>\")>", "what's the question?", "{B}-[what's the question?]");
		}

		public void TestMultipleMacros()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Number = 42;
			dummyBO.Z0_VarCharMax = "the answer";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Z0_VarCharMax> <Format(\": {Z0_Number}\")>", "{B}-[the answer : 42]"
				, typeof(VisualiserComponentLabel), "the answer : 42");

			testHelper.EndToEnd("<Z0_VarCharMax> <Format(\": {Z0_Number}\")> <Z0_VarCharMax>", "{B}-[the answer : 42 the answer]"
				, typeof(VisualiserComponentLabel), "the answer : 42 the answer");
		}

		public void TestFormattedNumber()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Number = 42;

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format({Z0_Number:D10})>", "{B}-[0000000042]"
				, typeof(VisualiserComponentTextBox), "0000000042", "<Format({Z0_Number:D10})>", "216", "{B}-[216]");
		}

		public void TestNumber()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Number = 42;

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Z0_Number>", "{B}-[42]"
				, typeof(VisualiserComponentTextBox), "42", "<Z0_Number>", "71", "{B}-[71]");
		}

		public void TestFormattedBooleanField()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Bool = true;

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format(\"{Z0_Bool:TrueFalse}\")>", "{B}-[True]"
				, typeof(VisualiserComponentTextBox), "True", "<Format(\"{Z0_Bool:TrueFalse}\")>", "False", "{B}-[False]");
		}

		public void TestBooleanField()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Bool = true;

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Z0_Bool>", "{B}-[Y]"
				, typeof(VisualiserComponentTextBox), "Y", "<Z0_Bool>", "N", "{B}-[N]");
		}

		public void TestFormattedText()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "foo bar";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format(\"this here is some text\")>", "{B}-[this here is some text]"
				, typeof(VisualiserComponentTextBox), "this here is some text", "<Format(\"this here is some text\")>", "overruled!", "{B}-[overruled!]");
		}

		public void TestLabel()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "foo bar";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("this is just a label", "{B}-[this is just a label]"
				, typeof(VisualiserComponentLabel), "this is just a label");
		}

		public void TestFormattedBOTextField()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "format this!";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Format(\"{Z0_VarCharMax:Upper}\")>", "{B}-[FORMAT THIS!]"
				, typeof(VisualiserComponentTextBox), "FORMAT THIS!", "<Format(\"{Z0_VarCharMax:Upper}\")>", "MAKE ME", "{B}-[MAKE ME]");
		}

		public void TestSimpleTextField()
		{
			DummyBusinessObject dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_VarCharMax = "foo bar";

			TestHelper testHelper = new TestHelper(BODocDataProvider.Get(dummyBO), Factory);
			testHelper.EndToEnd("<Z0_VarCharMax>", "{B}-[foo bar]"
				, typeof(VisualiserComponentTextBox), "foo bar", "<Z0_VarCharMax>", "hello world", "{B}-[hello world]");
		}

		class TestHelper
		{
			public TestHelper(IBODocDataProvider topLevelDataSource, BusinessObjectFactory factory)
			{
				TopLevelDataSource = topLevelDataSource;
				Factory = factory;
			}
			readonly IBODocDataProvider TopLevelDataSource;
			readonly BusinessObjectFactory Factory;

			ExcelTemplate GetExcelTemplateSetupFor(ZString cellContent)
			{
				using (ExcelInterface xlInterface = new ExcelInterface())
				{
					xlInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=DummyTemplate";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=.DummyBusinessObject";
					workSheet[4, 0] = "#DocumentHeader";
					workSheet[5, 1] = cellContent;
					workSheet[6, 0] = "#EndOfReport";

					using (MemoryStream saveStream = new MemoryStream())
					{
						xlInterface.SaveToStream(saveStream);
						return new ExcelTemplateReadFromByteArray("DummyTemplate", "GeneratedUsingFlexCel", saveStream.ToArray());
					}
				}
			}

			public void EndToEnd(ZString cellContent, ZString expectedWorksheetOutput, Type expectedComponentType, object initialValueInVisualiserDataSetOrControl)
			{
				EndToEnd(cellContent, expectedWorksheetOutput
				, expectedComponentType, initialValueInVisualiserDataSetOrControl, null, null, expectedWorksheetOutput);
			}

			public void EndToEnd(ZString cellContent, ZString expectedWorksheetOutput, Type expectedComponentType, object initialValueInVisualiserDataSetOrControl, ZString expectedColumnName, object valueToModifyWith, ZString expectedWorkSheetOutputAfterModification)
			{
				//========================================================================================================================
				// This method simulates how the DocumentEngine currently (31/03/08) Visualises, Modifies and Previews documents.
				// The invdividual steps taken by the user in a UAT are commented in the form "x. step".
				// If the DocumentEngine is modified, this method will need to be modified to reflect any changes to the steps
				// in which a document is visualised, modified, or previewed.
				//========================================================================================================================

				ExcelTemplate excelTemplate = GetExcelTemplateSetupFor(cellContent);

				var menuItem = Factory.New<StmMenuItem>();
				menuItem.SU_MenuName = ZGuid.NewZGuid().ToString();

				string expectedValues = GetFormattedAssertionMessage(cellContent, expectedComponentType, initialValueInVisualiserDataSetOrControl, expectedColumnName, valueToModifyWith);

				//1. Open the Delivery Form.
				using (DocumentPack pack = new DocumentPack(menuItem))
				using (Report report = GetNewReport(pack, TopLevelDataSource, excelTemplate))
				{
					pack.Add(report);
					DeliverableCollectionView deliveryDestinations = new DeliverableCollectionView(pack, new DocDeliveryContactCollection(new BusinessObjectFactory()));

					//2. Preview (and check values).
					PreviewAndCheck(report, cellContent, expectedWorksheetOutput, expectedValues);

					//3. Close the Preview.
					//4. Click the Modify Button (Visualiser Form - to visualise).
					DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(pack, deliveryDestinations);
					AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());

					//5a. Check the Visualiser Form has the right controls.
					var boundDataSet = report.OverridingDataSet;
					VisualiserComponent component = VisualiseAndGetComponent(report, boundDataSet);
					AssertEquals("component.GetType()", expectedComponentType, component.GetType());
					AssertEquals("component.GetControlValueForTesting()", initialValueInVisualiserDataSetOrControl, component.GetRenderedControlValueForTesting());

					if (component.IsModifiableForTesting)
					{
						//5b. Check the the Visualiser Form has the right values.
						AssertDataSetContents(boundDataSet, expectedColumnName, initialValueInVisualiserDataSetOrControl);

						//6. Modify the value of the component.
						component.SimulateUserSettingValueForTesting(valueToModifyWith);
						AssertDataSetContents(boundDataSet, expectedColumnName, valueToModifyWith);
					}
					//7. Save and Close.
					report.Factory.Save();
					visualiserManager.SaveData();
					if (component.IsModifiableForTesting)
					{
						AssertDataSetContents(report.OverridingDataSet, expectedColumnName, valueToModifyWith);
					}

					//8. Preview again (don't close Delivery Form) and check values.
					PreviewAndCheck(report, cellContent, expectedWorkSheetOutputAfterModification, expectedValues);
					//9. Close the preview.
					//10. Close the Delivery Form.
					visualiserManager.SaveData();
				}

				//11. Open the Delivery Form. (serialises from xml)
				using (DocumentPack pack = new DocumentPack(menuItem))
				using (Report report = GetNewReport(pack, TopLevelDataSource, excelTemplate))
				{
					pack.Add(report);
					DeliverableCollectionView deliveryDestinations = new DeliverableCollectionView(pack, new DocDeliveryContactCollection(new BusinessObjectFactory()));

					//12. Preview (and check values).
					PreviewAndCheck(report, cellContent, expectedWorkSheetOutputAfterModification, expectedValues);

					//13. Close the preview.
					//14. Click the Modify Button (Visualiser Form - to visualise).
					DocPackVisualiserManager visualiserManager = new DocPackVisualiserManager(pack, deliveryDestinations);
					AssertEquals("Precondition: visualiserManager.Reports.Count()", 1, visualiserManager.Reports.Count());

					//15a. Check the the Visualiser Form has the right controls.
					var boundDataSet = report.OverridingDataSet;
					VisualiserComponent component = VisualiseAndGetComponent(report, boundDataSet);
					AssertEquals("component.GetType()", expectedComponentType, component.GetType());

					if (component.IsModifiableForTesting)
					{
						//15b. Check the the Visualiser Form has the right values.
						AssertDataSetContents(boundDataSet, expectedColumnName, valueToModifyWith);
						AssertEquals("component.GetControlValueForTesting()", valueToModifyWith, component.GetRenderedControlValueForTesting());
						visualiserManager.SaveData();
						AssertDataSetContents(report.OverridingDataSet, expectedColumnName, valueToModifyWith);
					}
					else
					{
						AssertEquals("component.GetControlValueForTesting()", initialValueInVisualiserDataSetOrControl, component.GetRenderedControlValueForTesting());
					}
				}
			}

			string GetFormattedAssertionMessage(ZString cellContent, Type expectedComponentType, object initialValueInVisualiserDataSetOrControl, ZString expectedColumnName, object valueToModifyWith)
			{
				ZStringBuilder result = new ZStringBuilder();
				result.Append("cellContent: " + cellContent);
				result.Append("initialValueInVisualiserDataSetOrControl: " + initialValueInVisualiserDataSetOrControl);
				result.Append("valueToModifyWith: " + valueToModifyWith);
				result.Append("expectedComponentType: " + expectedComponentType);
				result.Append("expectedColumnName: " + expectedColumnName);
				result.Append("");
				return result.ToStringWithNewLineBetweenAppends();
			}

			void PreviewAndCheck(Report report, ZString cellContent, ZString expectedWorksheetOutput, ZString expectations)
			{
				report.PrepareForRender();

				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface xlInterface = new ExcelInterface())
					{
						xlInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
						AssertWorksheetEquals(expectations, workSheet, expectedWorksheetOutput);
					}
				}
			}

			VisualiserComponent VisualiseAndGetComponent(Report report, VisualiserDataSet boundDataSet)
			{
				TemplateToVisualiserComponentsConverter convertor = new TemplateToVisualiserComponentsConverter(report, boundDataSet);
				AssertEquals("Precondition: Components.Count", 1, convertor.Components.Count);
				return convertor.Components[0];
			}

			void AssertDataSetContents(VisualiserDataSet dataSet, ZString columnName, object expectedColumnValue)
			{
				AssertEquals("MainTable.Columns.Count", 1, dataSet.MainTable.Columns.Count);
				AssertEquals("dataSet.MainTable.Columns[0].ColumnName", columnName, dataSet.MainTable.Columns[0].ColumnName);
				object actualColumnValue = dataSet.MainRow[columnName];
				AssertEquals("dataSet.MainRow[columnName]", expectedColumnValue, actualColumnValue);
			}

			void AssertWorksheetEquals(ZString expectations, ExcelWorkSheet workSheet, string expectedValue)
			{
				AssertMultilineASCIIEquals(expectations, expectedValue.Trim(), workSheet.ToString());
			}

			Report GetNewReport(DocumentPack pack, IBODocDataProvider topLevelDataSource, ExcelTemplate excelTemplate)
			{
				return new Report(pack, excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
			}
		}
	}
}
