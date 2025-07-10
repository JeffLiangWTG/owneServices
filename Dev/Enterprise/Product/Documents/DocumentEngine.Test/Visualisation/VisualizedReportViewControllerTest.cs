using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualizedReportViewControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDrawVisualiserOnFirstShown()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var view = mocks.Create<IVisualizedReportView>();
			var controlDrawer = mocks.Create<IVisualiserDrawer>();
			var borderDrawer = mocks.Create<IVisualiserDrawer>();

			controlDrawer.Setup(m => m.Draw(It.IsAny<IEnumerable<VisualiserComponent>>()));
			borderDrawer.Setup(m => m.Draw(It.IsAny<IEnumerable<VisualiserComponent>>()));
			view.Setup(m => m.ClientSize).Returns(new Size(VisualizerPageSizes.PortraitPageWidth, VisualizerPageSizes.PortraitPageHeight));
			view.Setup(m => m.ControlDrawer).Returns(controlDrawer.Object);
			view.Setup(m => m.BorderDrawer).Returns(borderDrawer.Object);
			view.Object.FirstShown += null;

			using (var documentPack = GetNewDocumentPack(templateString))
			{
				var report = documentPack.GetFirstReport();
				var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
				controller.View = view.Object;
				view.Raise(m => m.FirstShown += null, EventArgs.Empty);
			}
		}

		public void TestViewTextTranslated()
		{
			var view = new Mock<IVisualizedReportView>();
			view.SetupAllProperties();
			using (var resourceStrings = Res.UseMockData())
			using (var documentPack = GetNewDocumentPack(templateString))
			{
				resourceStrings.Put("ReportName|The Title", new ResourceStringData("", "Der Titel"));
				var report = documentPack.GetFirstReport();
				report.Name = "The Title";
				var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
				controller.View = view.Object;
				AssertEquals("Der Titel", view.Object.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestDoNotDrawVisualiserIfNotShown()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var view = mocks.Create<IVisualizedReportView>();

			view.Verify(m => m.ControlDrawer, Times.Never());
			view.Verify(m => m.BorderDrawer, Times.Never());

			using (DocumentPack documentPack = GetNewDocumentPack(templateString))
			{
				var report = documentPack.GetFirstReport();
				var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
				controller.View = view.Object;
			}
		}

		[ExpectNoExceptions]
		public void TestDrawVisualiserOnFirstShown_DoesNotThrowWhenTemplateContainsInvalidColumnInGroupCount()
		{
			var templateStringWithInvalidColumnInGroupCount = @"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionHeader]
{B}-[<GroupCount(Collection, ARandomNonExistingField)>]
{A}-[#SectionBody]
{B}-[<Text>]
{A}-[#EndOfReport]";

			var mocks = new MockRepository(MockBehavior.Default);
			var view = mocks.Create<IVisualizedReportView>();

			view.Verify(m => m.ControlDrawer, Times.Never());
			view.Verify(m => m.BorderDrawer, Times.Never());
			view.Object.FirstShown += null;

			using (DocumentPack documentPack = GetNewDocumentPack(templateStringWithInvalidColumnInGroupCount))
			{
				var report = documentPack.GetFirstReport();
				report.MenuItem.SU_IsSystemDefined = true;
				report.StTemplate.SO_IsSystemDefined = true;
				// Overriding data with exactly the same values from real data.
				var tableName = VisualiserDataSet.GetTableName("Collection");
				var dataSet = report.OverridingDataSet;
				var dataTable = dataSet.Tables.Add(tableName);
				dataTable.Columns.Add("Z0_Code");
				dataTable.Columns.Add("Z0_Description");

				foreach (DummyChildBusinessObject item in dummy.Collection)
				{
					dataTable.Rows.Add(item.Z0_Code, item.Z0_Description);
				}

				var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
				controller.View = view.Object;
				ErrorReporter.Clear();
				view.Raise(m => m.FirstShown += null, EventArgs.Empty);

				var expectedMessage = @"GroupBy column 'ARandomNonExistingField' is invalid. Please verify the GroupBy areas and/or GroupCount macros in your template.
Error processing GroupBy columns - Could not find column [ARandomNonExistingField].";

				AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(ZString.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestAllowVisualizeReportRegardlessOfWarnings()
		{
			var rightTemplate = @"{A}-[#config]
{A}-[Name=TestBeginLoopNotNestedInSectionBody]
{A}-[#DocumentHeader]
{B}-[THIS IS DOCUMENTHEADER]
{A}-[#EndOfReport]";
			var errorTemplate = @"{A}-[#config]
{A}-[Name=TestBeginLoopNotNestedInSectionBody]
{A}-[#DocumentHeader2]
{B}-[THIS IS DOCUMENTHEADER]
{A}-[#EndOfReport]";
			var warningTemplate = @"{A}-[#config]
{A}-[Name=TestBeginLoopNestedInSectionBody]
{A}-[DataContext=.DummyDocumentSupportable]
{A}-[#DocumentHeader]
{B}-[THIS IS DOCUMENTHEADER]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel1]
{B}-[<CollectionLevel1.Z0_VarCharMax>]
{A}-[#EndLoop]
{A}-[#SectionFooter]
{A}-[#EndOfReport]";

			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			var mockIVisualizedReportView = new Mock<IVisualizedReportView>();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				using (var documentPack = GetNewDocumentPack(rightTemplate))
				{
					var report = documentPack.GetFirstReport();
					var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
					controller.View = mockIVisualizedReportView.Object;
					AssertEquals("AllowVisualizeReportRegardlessOfWarnings should return true", true, controller.AllowVisualizeReportRegardlessOfWarnings());
				}

				using (var documentPack = GetNewDocumentPack(warningTemplate))
				{
					var report = documentPack.GetFirstReport();
					var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
					controller.View = mockIVisualizedReportView.Object;
					mockIPrintTaskUIProvider.Setup(m => m.ShowErrors(report)).Returns(true);
					AssertEquals("AllowVisualizeReportRegardlessOfWarnings should return true", true, controller.AllowVisualizeReportRegardlessOfWarnings());
				}

				using (var documentPack = GetNewDocumentPack(errorTemplate))
				{
					var report = documentPack.GetFirstReport();
					var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
					controller.View = mockIVisualizedReportView.Object;
					mockIPrintTaskUIProvider.Setup(m => m.ShowErrors(report)).Returns(false);
					AssertEquals("AllowVisualizeReportRegardlessOfWarnings should return false", false, controller.AllowVisualizeReportRegardlessOfWarnings());
					AssertEquals("ErrorsCount is 1", 1, report.ErrorManager.ErrorsCount);
					AssertEquals("Occurrences of first error is 1", 1, ((IHaveReportProcessingErrorsForGUI)report.ErrorManager).GetErrors().ElementAt(0).Occurrences);

					var controller2 = new VisualizedReportViewController(report, new VisualiserDataSet());
					var mockIVisualizedReportView2 = new Mock<IVisualizedReportView>();
					controller2.View = mockIVisualizedReportView2.Object;
					AssertEquals("AllowVisualizeReportRegardlessOfWarnings should return false", false, controller2.AllowVisualizeReportRegardlessOfWarnings());
					AssertEquals("ErrorsCount is still 1", 1, report.ErrorManager.ErrorsCount);
					AssertEquals("Occurrences of first error is still 1", 1, ((IHaveReportProcessingErrorsForGUI)report.ErrorManager).GetErrors().ElementAt(0).Occurrences);
				}
			}
		}

		public void TestDrawVisualiserOnFirstShownTests()
		{
			AssertDrawVisualiserOnFirstShowGenericTest("DoesNotThrowWhenTemplateContainsInvalidCurrencyToWordsMacro",
														"{A}-[#Config]\n{A}-[Name=Test]\n{A}-[#SectionBody]\n{B}-[<CURRENCY TO WORDS(-12, USD,GRM)>]\n{A}-[#EndOfReport]",
														true,
														"You cannot pass a negative number to the <NumberToWords> or <CurrencyToWords> macro.",
														false);

			AssertDrawVisualiserOnFirstShowGenericTest("DataProviderException in MenusCustomisationForm",
														"{A}-[#Config]\n{A}-[Name=Test]\n{A}-[#SectionBody:Data=InvalidData]\n{B}-[<InvalidData.InvalidField>]\n{A}-[#EndOfReport]",
														true,
														"The data source for this document (Enterprise.DocumentEngine.Testing.DummyBODocSupportable) does not contain a collection called [InvalidData]. Please check the template 'VisualizedReportViewControllerTest'. Typical syntax would be '#SectionBody:Data=InvalidData'.",
														false);

			AssertDrawVisualiserOnFirstShowGenericTest("DataProviderException out of MenusCustomisationForm",
														"{A}-[#Config]\n{A}-[Name=Test]\n{A}-[#SectionBody:Data=InvalidData]\n{B}-[<InvalidData.InvalidField>]\n{A}-[#EndOfReport]",
														false,
														"The data source for this document (Enterprise.DocumentEngine.Testing.DummyBODocSupportable) does not contain a collection called [InvalidData]. Please check the template 'VisualizedReportViewControllerTest'. Typical syntax would be '#SectionBody:Data=InvalidData'.",
														true);
		}

		#region Implementation

		void AssertDrawVisualiserOnFirstShowGenericTest(string testName, string input, bool isFromMenusCustomisationForm, string output, bool reportError)
		{
			ErrorReporter.Clear();

			var mocks = new MockRepository(MockBehavior.Default);
			var view = mocks.Create<IVisualizedReportView>();

			view.Verify(m => m.ControlDrawer, Times.Never());
			view.Verify(m => m.BorderDrawer, Times.Never());
			view.Object.FirstShown += null;

			using (var documentPack = GetNewDocumentPack(input))
			{
				var report = documentPack.GetFirstReport();
				report.MenuItem.SU_IsSystemDefined = true;
				report.StTemplate.SO_IsSystemDefined = true;
				report.Parent.IsRunFromMenusCustomisationForm = isFromMenusCustomisationForm;

				var controller = new VisualizedReportViewController(report, new VisualiserDataSet());
				controller.View = view.Object;
				view.Raise(m => m.FirstShown += null, EventArgs.Empty);

				var expectedErrorReported = ZString.Empty;
				if (reportError)
				{
					expectedErrorReported = output;
				}

				var expectedOutput = @"Could not render the document due to an error in the template:
" + output;

				AssertEquals(testName, expectedOutput, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(testName, expectedErrorReported, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		DocumentPack GetNewDocumentPack(string templateAsString)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "VisualizedReportViewControllerTest", templateAsString);
			template.SO_DataContext = "UnitTest";

			dummy = Factory.New<DummyBODocSupportable>();
			dummy.Collection.AddNew("A", "A1");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_IsSystemDefined = true;

			return new DocumentPack(documentCommand, dummy, null, null);
		}

		DummyBODocSupportable dummy;

		const string templateString = @"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<Text>]
{A}-[#EndOfReport]";

		#endregion
	}
}
