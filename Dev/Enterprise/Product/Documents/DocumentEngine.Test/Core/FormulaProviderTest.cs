using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class FormulaProviderTest : TransactionedTestCase
	{
		public void TestAccumulativeTotalOnVeryLargeNumberOfRowsWithCurrencyMacro()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test Template",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[PageStyle=Landscape]
{A}-[#PageHeader]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#SectionBody:Data=Collection]
{B}-[<AutoHeight><Collection.Text>]   {C}-[<Collection.Decimal>]   {D}-[<Currency(<AccumulativeTotal Decimal>, AUD)>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = factory.New<DummyDocumentSupportable>();
			for (var index = 0; index < 300; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = "Hello World";
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
				var result = workSheet.ToString();

				Assert(string.Format("There should be 300 rows:\n{0}", result), result.Contains(@"{C}-[1]   {D}-[300]"));
				Assert(string.Format("There should not be 301 rows:\n{0}", result), !result.Contains(@"{C}-[1]   {D}-[301]"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestClearRows()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(1, 1));
				rowRanges.Add(new RowRange(2, 2));
				rowRanges.Add(new RowRange(3, 3));
				rowRanges.Add(new RowRange(4, 4));

				AssertEquals("=SUM(A1:A4)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
				rowRanges.Clear();
				rowRanges.Add(new RowRange(1, 1));
				rowRanges.Add(new RowRange(2, 2));
				AssertEquals("=SUM(A1:A2)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoRows()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				AssertEquals(0, testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Result);
				AssertEquals("=0", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectException(typeof(FormulaProviderNotReadyException))]
		public void TestNoColumnAdded()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				RowRangeList rowRanges = new RowRangeList();
				AssertEquals("", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormula()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(22, 22));
				rowRanges.Add(new RowRange(52, 52));
				rowRanges.Add(new RowRange(88, 88));
				rowRanges.Add(new RowRange(120, 120));
				AssertEquals("=A12+A22+A52+A88+A120", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		public void TestGetFormulaFromAreaList()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, System.Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = rpt.Analyser.Sections[0].SectionBody;

				var expansionSize = 6;
				createdArea.ExpandForDataRows(expansionSize);

				createdArea.SetWorksheetForFormulaProvider(rpt.WorkSheetCurrentlyBeingProcessed);
				var testFormulaProvider = createdArea.FormulaProvider;
				testFormulaProvider.AddColumn(0, "Lines.Description", 0);

				AssertEquals("=SUM(A41:A47)", testFormulaProvider.GetFormula("Lines.Description", new List<Area>(new Area[] { createdArea }), TFileFormats.Xls).Text);
			}
		}

		public void TestGetFormulaWithMaxDBRows()
		{
			var pack = new DocumentPack();
			TestData.CreateLinesTestTable();
			var excelTemplate = NewStyleTemplate;
			using (Report.TemporarilyUseMainConnection())
			using (var rpt = new Report(pack, excelTemplate, System.Guid.NewGuid(), Core.Constants.DataContext.UnitTest))
			{
				rpt.PrepareForRender();

				var createdArea = rpt.Analyser.Sections[0].SectionBody;

				var expansionSize = 6;
				createdArea.ExpandForDataRows(expansionSize);

				createdArea.SetWorksheetForFormulaProvider(rpt.WorkSheetCurrentlyBeingProcessed);
				var testFormulaProvider = createdArea.FormulaProvider;
				testFormulaProvider.AddColumn(0, "Lines.Description", 0);

				AssertEquals("=SUM(A41:A43)", testFormulaProvider.GetFormula("Lines.Description", new List<Area>(new Area[] { createdArea }), 3, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaCaseInsensetive()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(22, 22));
				rowRanges.Add(new RowRange(52, 52));
				rowRanges.Add(new RowRange(88, 88));
				rowRanges.Add(new RowRange(120, 120));
				AssertEquals("=A12+A22+A52+A88+A120", testFormulaProvider.GetFormulaForTesting("TESTFIELD", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaOneRow()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				AssertEquals("=A12", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaOneRange()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(13, 13));
				rowRanges.Add(new RowRange(14, 14));
				AssertEquals("=SUM(A12:A14)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaOneRangeAndOneRow()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(13, 13));
				rowRanges.Add(new RowRange(14, 14));
				rowRanges.Add(new RowRange(16, 16));
				AssertEquals("=SUM(A12:A14,A16)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaTwoRanges()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(13, 13));
				rowRanges.Add(new RowRange(14, 14));
				rowRanges.Add(new RowRange(16, 16));
				rowRanges.Add(new RowRange(17, 17));
				rowRanges.Add(new RowRange(18, 18));
				rowRanges.Add(new RowRange(19, 19));
				AssertEquals("=SUM(A12:A14,A16:A19)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaReturnsTheResultForHugeFormula()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				for (int i = 1; i < 4000; i += 2)
				{
					rowRanges.Add(new RowRange(i + 1, i + 1));
					xlInterface.WorkSheets[0][i, 0] = 1;
				}
				AssertEquals("=2000", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
				AssertEquals(0, testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaTwoRangesValueIsCalculated()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(13, 13));
				rowRanges.Add(new RowRange(14, 14));
				rowRanges.Add(new RowRange(16, 16));
				rowRanges.Add(new RowRange(17, 17));
				rowRanges.Add(new RowRange(18, 18));
				rowRanges.Add(new RowRange(19, 19));
				xlInterface.WorkSheets[0][11, 0] = 1;
				xlInterface.WorkSheets[0][12, 0] = 1;
				xlInterface.WorkSheets[0][13, 0] = 1;
				xlInterface.WorkSheets[0][15, 0] = 1;
				xlInterface.WorkSheets[0][16, 0] = 1;
				xlInterface.WorkSheets[0][17, 0] = 1;
				xlInterface.WorkSheets[0][18, 0] = 1;
				AssertEquals("=SUM(A12:A14,A16:A19)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
				AssertEquals(0, testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaTwoRangesValueIsCalculatedForArgumentMaxSize()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				var testFormulaProvider = new FormulaProvider(excelInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				var rowRanges = new RowRangeList();
				for (int i = 0; i <= 100; i++)
				{
					rowRanges.Add(new RowRange(i + 1, i + 2));
					excelInterface.WorkSheets[0][i, 0] = 1;
					excelInterface.WorkSheets[0][i + 1, 0] = 1;
					i++;
					i++;
				}

				AssertEquals("=SUM(A1:A2,A4:A5,A7:A8,A10:A11,A13:A14,A16:A17,A19:A20,A22:A23,A25:A26,A28:A29,A31:A32,A34:A35,A37:A38,A40:A41,A43:A44,A46:A47,A49:A50,A52:A53,A55:A56,A58:A59,A61:A62,A64:A65,A67:A68,A70:A71,A73:A74,A76:A77,A79:A80,A82:A83,A85:A86,A88:A89)+SUM(A91:A92,A94:A95,A97:A98,A100:A101)", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
				AssertEquals(0, testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaColumn8()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(8, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(120, 120));
				rowRanges.Add(new RowRange(1200, 1200));
				AssertEquals("=I12+I120+I1200", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaColumn25()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(25, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(120, 120));
				rowRanges.Add(new RowRange(1200, 1200));
				AssertEquals("=Z12+Z120+Z1200", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaColumn26()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(26, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(120, 120));
				rowRanges.Add(new RowRange(1200, 1200));
				AssertEquals("=AA12+AA120+AA1200", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaColumn255()
		{
			using (ExcelInterface xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				FormulaProvider testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(255, "TestField", 0);
				RowRangeList rowRanges = new RowRangeList();
				rowRanges.Add(new RowRange(12, 12));
				rowRanges.Add(new RowRange(120, 120));
				rowRanges.Add(new RowRange(1200, 1200));
				AssertEquals("=IV12+IV120+IV1200", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetFormulaInFrenchCulture()
		{
			using (var xlInterface = new ExcelInterface())
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.French)))
			{
				xlInterface.LoadExcelFile(UnitTestingConstants.TestFilesDir + "EmptySheet.xls");
				var testFormulaProvider = new FormulaProvider(xlInterface.WorkSheets[0]);
				testFormulaProvider.AddColumn(0, "TestField", 0);
				var rowRanges = new RowRangeList();
				for (int i = 1; i < 4000; i += 2)
				{
					rowRanges.Add(new RowRange(i + 1, i + 1));
					xlInterface.WorkSheets[0][i, 0] = 0.1;
				}
				AssertEquals("=199.999999999993", testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Text);
				AssertEquals(0, testFormulaProvider.GetFormulaForTesting("TestField", rowRanges, TFileFormats.Xls).Result);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}
	}
}
