using System.IO;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ShrinkToFit))]
	class ShrinkToFitTest : ValueProviderTest
	{
		public void TestShrinkToFitWithBlankReport()
		{
			var shrinkToFit = new ShrinkToFit();
			AssertEquals(string.Empty, shrinkToFit.GetReplacement("<ShrinkToFit(1)>", new Report(new DocumentPack(), null)));
		}

		public void TestShrinkToFitWithHideRowIfMacros()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[]
{B}-[]
{B}-[]
{B}-[]   {C}-[<HideRowIf(1==1)>]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				excelInterface.MergeCells(0, 2, 1, 5, 1);
				excelInterface.SetCellValue(0, 2, 1, GetReportStringForTestShrinkToFitWithHideRowIfMacros());

				var workSheet = excelInterface.WorkSheets.First();

				var format = workSheet.GetCellFormat(2, 1);
				format.VTextAlign = VerticalTextAlignment.Top;
				format.WrapText = true;

				workSheet.SetCellFormat(2, 1, format);
				workSheet.SetColWidth(1, 20000);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);

				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals(
					"Address should not have changed.",
					"{B}-[CargoWise|>Unit 3a, 72 O'Riordan Street|>Alexandria NSW 2015|>Australia]",
					workSheet.ToString());

				var format = workSheet.GetCellFormat(0, 1);

				AssertForTestShrinkToFitWithHideRowIfMacros(format);
			}
		}

		protected virtual string GetReportStringForTestShrinkToFitWithHideRowIfMacros()
		{
			return @"<ShrinkToFit(1)>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia";
		}

		protected virtual void AssertForTestShrinkToFitWithHideRowIfMacros(CellFormat format)
		{
			Assert("Font should have been reduced", format.FontSize <= 8f); //The font size in excel cell cannot be calculated by DpiScalingHelper, and the font will only become smaller since we can only make dpi scaling larger than the standard value.
		}

		public void TestShrinkToFitWithSingleMacroAndAutoHeightMacros()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[#SectionBody:Data=Collection]
{B}-[]    {C}-[]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				excelInterface.SetCellValue(0, 2, 1, GetReportStringForTestShrinkToFitWithSingleMacroAndAutoHeightMacros());
				excelInterface.SetCellValue(0, 2, 2, @"<AutoHeight>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia");

				var workSheet = excelInterface.WorkSheets.First();

				var format = workSheet.GetCellFormat(2, 1);
				format.VTextAlign = VerticalTextAlignment.Top;
				format.WrapText = true;

				workSheet.SetCellFormat(2, 1, format);
				workSheet.SetColWidth(1, 20000);
				workSheet.SetColWidth(2, 20000);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_VarCharMax = @"CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia";
			child = dummy.Collection.AddNew();
			child.Z0_VarCharMax = @"CargoWise
Nanjing Office
Blah Blah";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();

						AssertMultilineASCIIEquals(
							"Address should not have changed.",
							@"{B}-[CargoWise|>Unit 3a, 72 O'Riordan Street|>Alexandria NSW 2015|>Australia]   {C}-[CargoWise]
{C}-[Unit 3a, 72 O'Riordan Street]
{C}-[Alexandria NSW 2015]
{C}-[Australia]
{B}-[CargoWise|>Nanjing Office|>Blah Blah]   {C}-[CargoWise]
{C}-[Unit 3a, 72 O'Riordan Street]
{C}-[Alexandria NSW 2015]
{C}-[Australia]",
							workSheet.ToString());

						var format = workSheet.GetCellFormat(0, 1);
						AssertForTestShrinkToFitWithSingleMacroAndAutoHeightMacros(format);
					}
				}
			}
		}

		protected virtual string GetReportStringForTestShrinkToFitWithSingleMacroAndAutoHeightMacros()
		{
			return "<ShrinkToFit(1)><Collection.Text>";
		}

		void AssertForTestShrinkToFitWithSingleMacroAndAutoHeightMacros(CellFormat format)
		{
			Assert("Font should have been reduced", format.FontSize <= 2.5f);
		}

		public void TestShrinkToFit_ShrinkOnlyWhenAllMacrosHaveBeenReplaced()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-]DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ShrinkToFit><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax><Z0_VarCharMax>]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets.First();
				var format = workSheet.GetCellFormat(2, 1);
				format.FontSize = 10f;
				workSheet.SetCellFormat(3, 1, format);
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = @"0";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

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

						AssertMultilineASCIIEquals("", @"{B}-[00000]", excelInterface.WorkSheets[0].ToString());

						var workSheet = excelInterface.WorkSheets.First();
						var format = workSheet.GetCellFormat(3, 1);
						AssertEquals("Font shouldn't have been reduced", 10f, format.FontSize);
					}
				}
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<ShrinkToFit>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("<ShrinkToFit(4.5)>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("<ShrinkToFit(15)>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("< Shrink To Fit >", Passes.SecondPass);
			AssertIsResponsibleForReplacing("< Shrink To Fit ( 9.5 ) >", Passes.SecondPass);

			AssertNotResponsibleForReplacing("<ShrinkToFit>", Passes.FirstPass);
			AssertNotResponsibleForReplacing("<>", Passes.SecondPass);
			AssertNotResponsibleForReplacing("<Shr inkToFit>", Passes.SecondPass);
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith("", "<Shrink To Fit>", Passes.SecondPass);
		}

		public void TestMinimumFontSizeIsGreaterThanZero()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[]
{B}-[]
{B}-[]
{B}-[]   {C}-[<HideRowIf(1==1)>]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				excelInterface.SetCellValue(0, 2, 1, GetReportStringForTestMinimumFontSizeIsGreaterThanZero());

				var workSheet = excelInterface.WorkSheets.First();

				var format = workSheet.GetCellFormat(2, 1);
				format.VTextAlign = VerticalTextAlignment.Top;
				format.WrapText = true;

				workSheet.SetCellFormat(2, 1, format);
				workSheet.SetColWidth(1, 1000);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				var workSheet = excelInterface.WorkSheets.First();
				var format = workSheet.GetCellFormat(0, 1);

				AssertEquals("Font should hit the minimum value", ShrinkToFit.MinimumPossibleFontSize, format.FontSize);
			}
		}

		protected virtual string GetReportStringForTestMinimumFontSizeIsGreaterThanZero()
		{
			return @"<ShrinkToFit>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia
Blah blah blah
Antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism
Antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism
Antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism
Antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism
Antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism
Antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism antidisestablishmentarianism";
		}

		public void TestShrinkVeryLongText()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[]
{B}-[]
{B}-[]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				excelInterface.SetCellValue(0, 2, 1, GetVeryLongReportStringForTestShrinkToFit());

				var workSheet = excelInterface.WorkSheets.First();

				var format = workSheet.GetCellFormat(2, 1);
				format.VTextAlign = VerticalTextAlignment.Top;
				format.WrapText = true;

				workSheet.SetCellFormat(2, 1, format);
				workSheet.SetColWidth(1, 3000);
				for (int i = 2; i <= 4; i++)
				{
					workSheet.SetRowHeight(i, ExcelWorkSheet.MaxRowHeight);
				}

				excelInterface.MergeCells(0, 2, 1, 4, 1);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface1 = new ExcelInterface())
			{
				excelInterface1.LoadExcelFile(printJob.SP_CustomProperties);
				var workSheet = excelInterface1.WorkSheets.First();
				var format = workSheet.GetCellFormat(0, 1);

				Assert("Font should be larger than the minimum font size", format.FontSize > ShrinkToFit.MinimumPossibleFontSize);
			}
		}

		public void TestIsINonVisualisableValueProvider()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(ShrinkToFit.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		protected virtual string GetVeryLongReportStringForTestShrinkToFit()
		{
			return @"<ShrinkToFit>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia
Blah blah blah
AntidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismAntidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianism
AntidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismAntidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianism
AntidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismAntidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianismantidisestablishmentarianism
";
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider() => new ShrinkToFit();

		#endregion
	}
}
