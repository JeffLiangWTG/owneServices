using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(AccumulativeTotal))]
	sealed class AccumulativeTotalTest : ValueProviderTest
	{
		public void TestAccumulativeTotalReplacementWithMultipleMacros()
		{
			AssertAccumulativeTotalReplacementWithMultipleMacros("<ShrinkToFit><If('Test'=='Test', \"GRAND ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>\", \"Not Me\")>", "{C}-[GRAND ACCUMULATIVETOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("<ShrinkToFit><If('Test'=='Test', \"GRAND TOTAL <Total Collection.Decimal> ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>\", \"Not Me\")>", "{C}-[GRAND TOTAL 30 ACCUMULATIVETOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("<If('Test'=='Test', \"GRAND ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>\", \"Not Me\")>", "{C}-[GRAND ACCUMULATIVETOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("<If('Test'=='Test', \"GRAND TOTAL <Total Collection.Decimal> ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>\", \"Not Me\")>", "{C}-[GRAND TOTAL 30 ACCUMULATIVETOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("<ShrinkToFit><AccumulativeTotal Collection.Decimal>", "{C}-[30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("<ShrinkToFit>GRAND ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>", "{C}-[GRAND ACCUMULATIVETOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("GRAND ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>", "{C}-[GRAND ACCUMULATIVETOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("GRAND ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal> TOTAL <Total Collection.Decimal>", "{C}-[GRAND ACCUMULATIVETOTAL 30 TOTAL 30]");
			AssertAccumulativeTotalReplacementWithMultipleMacros("<If('Test'=='Test', \"GRAND TOTAL <Total Collection.Decimal> ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>\", \"Not Me\")> ACCUMULATIVETOTAL <AccumulativeTotal Collection.Decimal>", "{C}-[GRAND TOTAL 30 ACCUMULATIVETOTAL 30 ACCUMULATIVETOTAL 60]");
		}

		void AssertAccumulativeTotalReplacementWithMultipleMacros(string macro, string expected)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Code>]   {C}-[<Collection.Decimal>]
{A}-[#SectionFooter]
{C}-[" + macro + @"]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();

			dummy.Collection.AddNew("AAA", "", 0, 10);
			dummy.Collection.AddNew("AAA", "", 0, 10);
			dummy.Collection.AddNew("AAA", "", 0, 10);

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertContains("AccumulativeTotal macro shoule be translated correctly", expected, excelInterface.WorkSheets.First().ToString());
			}
		}

		class  AccumulativeTotalForTest : AccumulativeTotal
		{
			public bool IsNumericTypeForTest(object resultObject)
			{
				return IsNumericType(resultObject);
			}
		}

		class AccumulativeTotalThatThrowFormulaProviderException : AccumulativeTotal
		{
			protected override object DoReplacement(string macro, Report report)
			{
				throw new FormulaProviderException("Some dummy formula exception!");
			}
		}

		public void TestAccumulativeTotalWithPageHeaderHasStartFromSecondPage()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#DocumentHeader]
{A}-[#PageHeader:StartFromSecondPage]
{D}-[<AccumulativeTotal Decimal>]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";
			var dummy = factory.New<DummyDocumentSupportable>();
			for (var i = 0; i < 110; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = $"T_{i}";
				child.Z0_Decimal = 1;
			}

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Result is incorrect.",
@"{B}-[T_0]   {C}-[1]
{B}-[T_1]   {C}-[1]
{B}-[T_2]   {C}-[1]
{B}-[T_3]   {C}-[1]
{B}-[T_4]   {C}-[1]
{B}-[T_5]   {C}-[1]
{B}-[T_6]   {C}-[1]
{B}-[T_7]   {C}-[1]
{B}-[T_8]   {C}-[1]
{B}-[T_9]   {C}-[1]
{B}-[T_10]   {C}-[1]
{B}-[T_11]   {C}-[1]
{B}-[T_12]   {C}-[1]
{B}-[T_13]   {C}-[1]
{B}-[T_14]   {C}-[1]
{B}-[T_15]   {C}-[1]
{B}-[T_16]   {C}-[1]
{B}-[T_17]   {C}-[1]
{B}-[T_18]   {C}-[1]
{B}-[T_19]   {C}-[1]
{B}-[T_20]   {C}-[1]
{B}-[T_21]   {C}-[1]
{B}-[T_22]   {C}-[1]
{B}-[T_23]   {C}-[1]
{B}-[T_24]   {C}-[1]
{B}-[T_25]   {C}-[1]
{B}-[T_26]   {C}-[1]
{B}-[T_27]   {C}-[1]
{B}-[T_28]   {C}-[1]
{B}-[T_29]   {C}-[1]
{B}-[T_30]   {C}-[1]
{B}-[T_31]   {C}-[1]
{B}-[T_32]   {C}-[1]
{B}-[T_33]   {C}-[1]
{B}-[T_34]   {C}-[1]
{B}-[T_35]   {C}-[1]
{B}-[T_36]   {C}-[1]
{B}-[T_37]   {C}-[1]
{B}-[T_38]   {C}-[1]
{B}-[T_39]   {C}-[1]
{B}-[T_40]   {C}-[1]
{B}-[T_41]   {C}-[1]
{B}-[T_42]   {C}-[1]
{B}-[T_43]   {C}-[1]
{B}-[T_44]   {C}-[1]
{B}-[T_45]   {C}-[1]
{B}-[T_46]   {C}-[1]
{B}-[T_47]   {C}-[1]
{B}-[T_48]   {C}-[1]
{B}-[T_49]   {C}-[1]
{B}-[T_50]   {C}-[1]
{B}-[T_51]   {C}-[1]
{B}-[T_52]   {C}-[1]
{B}-[T_53]   {C}-[1]
{B}-[T_54]   {C}-[1]
{D}-[55]
{B}-[T_55]   {C}-[1]
{B}-[T_56]   {C}-[1]
{B}-[T_57]   {C}-[1]
{B}-[T_58]   {C}-[1]
{B}-[T_59]   {C}-[1]
{B}-[T_60]   {C}-[1]
{B}-[T_61]   {C}-[1]
{B}-[T_62]   {C}-[1]
{B}-[T_63]   {C}-[1]
{B}-[T_64]   {C}-[1]
{B}-[T_65]   {C}-[1]
{B}-[T_66]   {C}-[1]
{B}-[T_67]   {C}-[1]
{B}-[T_68]   {C}-[1]
{B}-[T_69]   {C}-[1]
{B}-[T_70]   {C}-[1]
{B}-[T_71]   {C}-[1]
{B}-[T_72]   {C}-[1]
{B}-[T_73]   {C}-[1]
{B}-[T_74]   {C}-[1]
{B}-[T_75]   {C}-[1]
{B}-[T_76]   {C}-[1]
{B}-[T_77]   {C}-[1]
{B}-[T_78]   {C}-[1]
{B}-[T_79]   {C}-[1]
{B}-[T_80]   {C}-[1]
{B}-[T_81]   {C}-[1]
{B}-[T_82]   {C}-[1]
{B}-[T_83]   {C}-[1]
{B}-[T_84]   {C}-[1]
{B}-[T_85]   {C}-[1]
{B}-[T_86]   {C}-[1]
{B}-[T_87]   {C}-[1]
{B}-[T_88]   {C}-[1]
{B}-[T_89]   {C}-[1]
{B}-[T_90]   {C}-[1]
{B}-[T_91]   {C}-[1]
{B}-[T_92]   {C}-[1]
{B}-[T_93]   {C}-[1]
{B}-[T_94]   {C}-[1]
{B}-[T_95]   {C}-[1]
{B}-[T_96]   {C}-[1]
{B}-[T_97]   {C}-[1]
{B}-[T_98]   {C}-[1]
{B}-[T_99]   {C}-[1]
{B}-[T_100]   {C}-[1]
{B}-[T_101]   {C}-[1]
{B}-[T_102]   {C}-[1]
{B}-[T_103]   {C}-[1]
{B}-[T_104]   {C}-[1]
{B}-[T_105]   {C}-[1]
{B}-[T_106]   {C}-[1]
{B}-[T_107]   {C}-[1]
{B}-[T_108]   {C}-[1]
{D}-[109]
{B}-[T_109]   {C}-[1]",
					workSheet.ToString());
			}
		}

		public void TestAccumulativeTotalOnTwoRecordsWithAutoHeightInSameRow()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<AccumulativeTotal Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "Hello World";
			child1.Z0_Decimal = 1;

			var child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "Goodbye World";
			child2.Z0_Decimal = 2;

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Result is incorrect.",
@"{B}-[Hello]   {C}-[1]   {D}-[1]
{B}-[World]
{B}-[Goodbye]   {C}-[2]   {D}-[3]
{B}-[World]",
					workSheet.ToString());
			}
		}

		public void TestAccumulativeTotalOnMultipleRecordsWithAutoHeight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<AccumulativeTotal Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "Child1";
			child1.Z0_Decimal = 1;

			var child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "Child2";
			child2.Z0_Decimal = 2;

			var child3 = dummy.Collection.AddNew();
			child3.Z0_VarCharMax = "Long Long Long Long Long Long Long Text in Child3";
			child3.Z0_Decimal = 3;

			var child4 = dummy.Collection.AddNew();
			child4.Z0_VarCharMax = "Child4";
			child4.Z0_Decimal = 4;

			var child5 = dummy.Collection.AddNew();
			child5.Z0_VarCharMax = "Child5";
			child5.Z0_Decimal = 5;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;
			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();
				AssertMultilineASCIIEquals("Result should be correct",
					@"{B}-[Child1]   {C}-[1]   {D}-[1]
{B}-[Child2]   {C}-[2]   {D}-[3]
{B}-[Long]   {C}-[3]   {D}-[6]
{B}-[Long]
{B}-[Long]
{B}-[Long]
{B}-[Long]
{B}-[Long]
{B}-[Long]
{B}-[Text in]
{B}-[Child3]
{B}-[Child4]   {C}-[4]   {D}-[10]
{B}-[Child5]   {C}-[5]   {D}-[15]
",
					workSheet.ToString());
			}
		}

		public void TestReplacementNonExistingTable()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			PrepareRenderer();
			AccumulativeTotalThatThrowFormulaProviderException valueProviderToTest = new AccumulativeTotalThatThrowFormulaProviderException();
			AssertEquals("", valueProviderToTest.GetReplacement("<AccumulativeTotalThatThrowFormulaProviderException Tbl.Tst>", Report));
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in AccumulativeTotalThatThrowFormulaProviderException Macro: Formula Provider Error :- Some dummy formula exception!]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match <TotalPages>", !ValueProviderToTest.IsResponsibleForReplacing("<TotalPages>", Passes.FirstPass));
			Assert("should not match <   AccumulativeTotal  \t       Lines   >", !ValueProviderToTest.IsResponsibleForReplacing("<   AccumulativeTotal  \t       Lines   >", Passes.FirstPass));
			Assert("should not match <   AccumulativeTotal  \t       Tbl.col   >", !ValueProviderToTest.IsResponsibleForReplacing("<   AccumulativeTotal  \t       Tbl.Col   >", Passes.FirstPass));
			Assert("should match <   AccumulativeTotal  \t       Lines   >", ValueProviderToTest.IsResponsibleForReplacing("<   AccumulativeTotal  \t       Lines   >", Passes.SecondPass));
			Assert("should match <   AccumulativeTotal  \t       Tbl.col   >", ValueProviderToTest.IsResponsibleForReplacing("<   AccumulativeTotal  \t       Tbl.Col   >", Passes.SecondPass));
			Assert("should match <   AccumulativeTotalWithReset  \t       Tbl.col   >", ValueProviderToTest.IsResponsibleForReplacing("<   AccumulativeTotal  \t       Tbl.Col   >", Passes.SecondPass));
			Assert("should match <AccumulativeTotal GenericTransactionHeader.Transactions.Balance>", ValueProviderToTest.IsResponsibleForReplacing("<AccumulativeTotal GenericTransactionHeader.Transactions.Balance>", Passes.SecondPass));
		}

		public void TestReplacementWithReset()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<Collection Decimal>]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<Collection Decimal>]
{A}-[#GroupBy:Collection.Text]
{B}-[Total]   {C}-[<Collection.Text>]   {D}-[<AccumulativeTotalWithReset Collection.Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();

			for (var i = 0; i < 2; i++)
			{
				var text = $"Test{i}";
				for (var j = 0; j < 5; j++)
				{
					var child = dummy.Collection.AddNew();
					child.Z0_VarCharMax = text;
					child.Z0_Decimal = j;
				}
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new System.IO.MemoryStream())
				{
					report.Save(stream);
					ValueProviderToTest.Reset();
					report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[3];

					var result = ValueProviderToTest.GetReplacement("<AccumulativeTotalWithReset Collection.Decimal>", report);
					AssertEquals(new ZDecimal(30), result);

					report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[5];
					result = ValueProviderToTest.GetReplacement("<AccumulativeTotalWithReset Collection.Decimal>", report);
					AssertEquals(new ZDecimal(10), result);

					ValueProviderToTest.Reset();
					report.Renderer.CurrentAreaToProcess = report.Analyser.Areas[5];
					result = ValueProviderToTest.GetReplacement("<AccumulativeTotalWithReset Collection.Decimal>", report);
					AssertEquals(new ZDecimal(40), result);
				}
			}
		}

		public void TestReplacement_WhenAnalyserIsNull_ShouldNotThrow()
		{
			Report.Analyser = null;
			AssertNoExceptionThrown(() => ValueProviderToTest.GetReplacement("<AccumulativeTotalWithReset Tbl.Tst>", Report));
		}

		public void TestReplacement_WhenFormulaProviderIsNull_ShouldNotThrow()
		{
			PrepareRenderer();
			ValueProviderToTest.Reset();
			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			var testSectionBodyArea2 = new SectionBodyArea(7, 8, Report, "#SectionBody:DATA=Tbl");
			var testGroupByAreaArea = new GroupByArea(5, 6, Report, "#GroupBy:Fld");
			testGroupByAreaArea.DataParent = testSectionBodyArea;
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea2.ExpandForDataRows(4);
			Report.Analyser.Areas.Clear();
			Report.Analyser.Areas.Add(testSectionBodyArea);
			Report.Analyser.Areas.Add(testGroupByAreaArea);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;

			AssertNoExceptionThrown(() => ValueProviderToTest.GetReplacement("<AccumulativeTotalWithReset Tbl.Tst>", Report));
		}

		public void TestIsITFormulaProvider()
		{
			Assert(GetNewValueProvider() is ITFormulaProvider);
		}

		public void TestIsNumericTypeWillReturnFalseWhenResultObjectIsNull()
		{
			var isNumericType = true;
			AssertNoExceptionThrown("no NullReferenceException will be thrown", () => { isNumericType = new AccumulativeTotalForTest().IsNumericTypeForTest(null); });
			AssertEquals("isNumericType should be false", false, isNumericType);
		}

		public void TestTemplateToVisualiserComponentsConverterWhenSectionBodyDataIsEmpty()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test Template", "",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#SectionBody]
{D}-[<AccumulativeTotal Collection.Z0_Number>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyBusinessObject>();

			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), "Test", null, DocumentDirection.ANY, false))
				{
					var dataSet = new VisualiserDataSet();
					var converter = new TemplateToVisualiserComponentsConverter(report, dataSet);
					AssertNotNull("converter.Components", converter.Components);
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new AccumulativeTotal();
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>()
				{
					typeof(AccumulativeTotal).GetField("areasByFieldName", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(AccumulativeTotal).GetField("lastProcessedArea", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(AccumulativeTotal).GetField("accumulativeTotalData", BindingFlags.Instance | BindingFlags.NonPublic),
				};
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<Collection Decimal>]
{A}-[#GroupBy:Collection.Text]
{B}-[Total]   {C}-[<Collection.Text>]   {D}-[<AccumulativeTotal Collection.Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();

			var child = dummy.Collection.AddNew();
			child.Z0_VarCharMax = $"Test1";
			child.Z0_Decimal = 1;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var documentPack = new DocumentPack(documentCommand, dummy, null, null);
			Report = documentPack.GetFirstReport();
		}
	}
}
