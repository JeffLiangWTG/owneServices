using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Total))]
	sealed class TotalTest : ValueProviderTest
	{
		public void TestGrandTotalWithSubTotals()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]   {C}-[<Collection.Decimal>]
{A}-[#GroupBy:Collection.Text]
{B}-[Sub Total]   {C}-[<Total Collection.Decimal>]
{A}-[#SectionFooter]
{B}-[GRAND TOTAL]   {C}-[<Total Collection.Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = factory.New<DummyDocumentSupportable>();
			for (var index = 0; index < 1000; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = index.ToString();
				child.Z0_Decimal = 1.0;
			}

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var expected = @"{B}-[GRAND TOTAL]   {C}-[1000]";
				var workSheet = excelInterface.WorkSheets.First();
				var result = workSheet.ToString();
				result = result.Remove(0, result.IndexOf("{B}-[GRAND TOTAL]"));

				AssertMultilineASCIIEquals("GRAND TOTAL should be 1000.", expected, result);
			}
		}

		public void TestTotalMacroWorksWithOtherStrings()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]   {C}-[<Collection.Decimal>]
{A}-[#SectionFooter]
{C}-[GRAND TOTAL <Total Collection.Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();
			for (var index = 0; index < 10; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = index.ToString();
				child.Z0_Decimal = 1.0;
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				Assert(excelInterface.WorkSheets.First().ToString().Contains("{C}-[GRAND TOTAL 10]"));
			}
		}

		public void TestReplacementNonExistingTable()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			PrepareRenderer();
			TotalThatThrowFormulaProviderException valueProviderToTest = new TotalThatThrowFormulaProviderException();
			AssertEquals(0, valueProviderToTest.GetReplacement("<TotalThatThrowFormulaProviderException Tbl.Tst>", Report));
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in TotalThatThrowFormulaProviderException Macro: Some dummy formula exception!]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <TotalPages>", !ValueProviderToTest.IsResponsibleForReplacing("<TotalPages>", Passes.FirstPass));
			Assert("should not match <   Total  \t       Lines   >", !ValueProviderToTest.IsResponsibleForReplacing("<   Total  \t       Lines   >", Passes.FirstPass));
			Assert("should not match <   Total  \t       Tbl.col   >", !ValueProviderToTest.IsResponsibleForReplacing("<   Total  \t       Tbl.Col   >", Passes.FirstPass));
			Assert("should match <   Total  \t       Lines   >", ValueProviderToTest.IsResponsibleForReplacing("<   Total  \t       Lines   >", Passes.SecondPass));
			Assert("should match <   Total  \t       Tbl.col   >", ValueProviderToTest.IsResponsibleForReplacing("<   Total  \t       Tbl.Col   >", Passes.SecondPass));
			Assert("should match <Total  Currency(<lines.dummy>, OMR)>", ValueProviderToTest.IsResponsibleForReplacing("<Total  Currency(<lines.dummy>, OMR)>", Passes.SecondPass));
			Assert("should match <Total  FormatNumber(<lines.dummy>, OMR)>", ValueProviderToTest.IsResponsibleForReplacing("<Total  FormatNumber(<lines.dummy>, OMR)>", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			var testGroupByAreaArea = new GroupByArea(3, 4, Report, "#GroupBy:Fld") { DataParent = testSectionBodyArea };
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;

			AssertEquals(typeof(TFormula), ValueProviderToTest.GetReplacement("<Total Tbl.Tst>", Report).GetType());
			AssertEquals("=SUM(B3:B7)", ((TFormula)ValueProviderToTest.GetReplacement("<Total Tbl.Tst>", Report)).Text);

			AssertEquals("Should return 0 when there's nothing to total", 0, ValueProviderToTest.GetReplacement("<Total SomeRubbish.Value>", Report));
		}

		public void TestReplacementNonExistingField()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]   {C}-[<Collection.Decimal>]
{A}-[#GroupBy:Collection.Text]
{A}-[#SectionFooter:ShowEvenWithNoData]
{B}-[GRAND TOTAL]   {C}-[<Total Collection.Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = factory.New<DummyDocumentSupportable>();
			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			AssertNoExceptionThrown("should not throw DocumentEngineException", () =>
			 {
				 var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();
				 using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
				 {
				 }
			 });

			var templateWithoutTotalFields = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#PageHeader]
{B}-[Page <Current Page> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]
{A}-[#GroupBy:Collection.Text]
{A}-[#SectionFooter:ShowEvenWithNoData]
{B}-[GRAND TOTAL]   {C}-[<Total Collection.Decimal>]
{A}-[#EndOfReport]");
			pivot.SI_SO = templateWithoutTotalFields.PK;

			var exception = AssertExceptionThrown<DocumentEngineException>("should throw DocumentEngineException", () =>
			{
				var printJobWithoutTotalFields = DeliveryTestHelper.DeliverDocument(documentCommand).First();
				using (var excelInterface = new ExcelInterface(printJobWithoutTotalFields.SP_CustomProperties))
				{
				}
			});

			AssertContains("Severity: [Warning (without error report)] Message: [Error in Total Macro: Field Collection.Decimal does not exist in the template! Input macro: [<Total Collection.Decimal>]] Cell Content: [<Total Collection.Decimal>]  Cell: [C10] Sheetname: [Document]", exception.Message);
		}

		public void TestNonExistingField_SeverityIsAdjustedWhenPreviewingSection()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=System Document Elements]
{A}-[#SectionBody]
{B}-[<Total Z0_VarCharMax>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";
			template.SO_IsSystemDefined = true;

			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_IsSystemDefined = true;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_IsSystemDefined = true;

			AssertSeverity(false, ReportProcessingErrorSeverity.Warning);
			AssertSeverity(true, ReportProcessingErrorSeverity.WarningWithoutErrorReport);

			void AssertSeverity(bool isForSectionPreview, ReportProcessingErrorSeverity severity)
			{
				using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
				{
					var report = documentPack.GetFirstReport();
					report.IsReportForSectionPreview = isForSectionPreview;
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;

					using (var stream = new MemoryStream())
					{
						report.Save(stream);
						AssertEquals("report.ErrorManager.ToString()", $@"Severity: [{severity.ToStringFormatted()}] Message: [Error in Total Macro: Field Z0_VarCharMax does not exist in the template! Input macro: [<Total Z0_VarCharMax>]]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
					}
				}
			}
		}

		public void TestReplacementWithNoCurrentArea()
		{
			AssertEquals(null, ValueProviderToTest.GetReplacement("<Total Lines.AmountExTax>", Report));
		}

		public void TestIsITFormulaProvider()
		{
			Assert(GetNewValueProvider() is ITFormulaProvider);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Total();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			ValueProviderToTest.Reset();
			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=InvoiceLines");
			var testGroupByAreaArea = new GroupByArea(5, 6, Report, "#GroupBy:Fld");
			testGroupByAreaArea.DataParent = testSectionBodyArea;
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "InvoiceLines.Amount", 0);
			Report.Analyser.Areas.Add(testSectionBodyArea);
			Report.Analyser.Areas.Add(testGroupByAreaArea);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;
		}

		class TotalThatThrowFormulaProviderException : Total
		{
			protected override object DoReplacement(string macro, Report report)
			{
				throw new FormulaProviderException("Some dummy formula exception!");
			}

			public override Regex Regex => new Regex(@"^<(?:[\s]*)TotalThatThrowFormulaProviderException(?:[\s]+)(.+)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		}
	}
}
