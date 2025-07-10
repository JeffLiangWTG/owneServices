using System.IO;
using System.Linq;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ShrinkToFitForBillOfLading))]
	sealed class ShrinkToFitForBillOfLadingTest : ShrinkToFitTest
	{
		public void TestShrinkToFitForBillOfLadingWithBlankReport()
		{
			ShrinkToFitForBillOfLading s = new ShrinkToFitForBillOfLading();
			AssertEquals(string.Empty, s.GetReplacement("<ShrinkToFitForBillOfLading(1)>", new Report(new DocumentPack(), null)));
		}

		protected override string GetReportStringForTestShrinkToFitWithHideRowIfMacros()
		{
			return @"<ShrinkToFitForBillOfLading(1)>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 2015
Australia";
		}

		protected override string GetReportStringForTestShrinkToFitWithSingleMacroAndAutoHeightMacros()
		{
			return "<ShrinkToFitForBillOfLading(1)><Collection.Text>";
		}

		protected override void AssertForTestShrinkToFitWithHideRowIfMacros(CellFormat format)
		{
			Assert("Font should have been reduced", format.FontSize <= 5f); //The font size in excel cell cannot be calculated by DpiScalingHelper, and the font will only become smaller since we can only make dpi scaling larger than the standard value.
		}

		public new void TestIsResponsibleForReplacing()
		{
			AssertIsResponsibleForReplacing("<ShrinkToFitForBillOfLading>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("<ShrinkToFitForBillOfLading(4.5)>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("<ShrinkToFitForBillOfLading(15)>", Passes.SecondPass);
			AssertIsResponsibleForReplacing("< Shrink To Fit For Bill Of Lading >", Passes.SecondPass);
			AssertIsResponsibleForReplacing("< Shrink To Fit For Bill Of Lading ( 9.5 ) >", Passes.SecondPass);

			AssertNotResponsibleForReplacing("<ShrinkToFitForBillOfLading>", Passes.FirstPass);
			AssertNotResponsibleForReplacing("<>", Passes.SecondPass);
			AssertNotResponsibleForReplacing("<Sh rinkToFitForBillOfLading>", Passes.SecondPass);
		}

		public new void TestReplacement()
		{
			AssertIsReplacedWith("", "<Shrink To Fit For Bill Of Lading>", Passes.SecondPass);
		}

		public new void TestIsINonVisualisableValueProvider()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(ShrinkToFitForBillOfLading.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		protected override string GetReportStringForTestMinimumFontSizeIsGreaterThanZero()
		{
			return @"<ShrinkToFitForBillOfLading>CargoWise
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

		public void TestShrinkToFitForBillOfLadingForDifferentDocumentTypes()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[#SectionBody]
{B}-[]
{B}-[]
{B}-[]
{B}-[]   {C}
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				excelInterface.MergeCells(0, 2, 1, 5, 1);
				excelInterface.SetCellValue(0, 2, 1, @"<ShrinkToFitForBillOfLading(1)>CargoWise
Unit 3a, 72 O'Riordan Street
Alexandria NSW 
2015
Australia");

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

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand, attachmentType: OrgConstants.AttachmentType.TIF).First();
			AssertTextAndFont(printJob, fontSize: 8.5f, attachmentFormat: OrgConstants.AttachmentType.TIF);

			printJob = DeliveryTestHelper.DeliverReport(reportCommand, attachmentType: OrgConstants.AttachmentType.PDF).First();
			AssertTextAndFont(printJob, fontSize: 7.5f, attachmentFormat: OrgConstants.AttachmentType.PDF);

			printJob = DeliveryTestHelper.DeliverReport(reportCommand, attachmentType: string.Empty).First();
			AssertTextAndFont(printJob, fontSize: 7.5f, attachmentFormat: string.Empty);

			printJob = DeliveryTestHelper.DeliverReport(reportCommand, attachmentType: OrgConstants.AttachmentType.XLS).First();
			AssertTextAndFont(printJob, fontSize: 5.5f, attachmentFormat: OrgConstants.AttachmentType.XLS);
		}

		void AssertTextAndFont(StmPrintJob printJob, float fontSize, string attachmentFormat)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJob.SP_CustomProperties);
				var workSheet = excelInterface.WorkSheets.First();
				AssertMultilineASCIIEquals(
					"Address should of not changed.",
					"{B}-[CargoWise|>Unit 3a, 72 O'Riordan Street|>Alexandria NSW |>2015|>Australia]",
					workSheet.ToString());

				var format = workSheet.GetCellFormat(0, 1);
				AssertEquals(string.Format("Font should have been reduced to {0} for {1} format", fontSize, attachmentFormat), fontSize, format.FontSize);
			}
		}

		#region Implementation

		protected override ValueProvider GetNewValueProvider()
		{
			return new ShrinkToFitForBillOfLading();
		}

		#endregion
	}
}
