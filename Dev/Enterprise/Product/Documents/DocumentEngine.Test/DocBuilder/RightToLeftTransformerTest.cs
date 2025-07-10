using System.Drawing;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ExcelTemplates;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class RightToLeftTransformerTest : TestCaseWithFactory
	{
		public void TestImageAreMovedFromRightToLeft()
		{
			AssertRightToLeftTransform(
				@"{B}-[B]   {C}-[C]   {D}-[D]   {E}-[E]
{B}-[B]   {C}-[C]   {D}-[D]   {E}-[E]",
				(ExcelInterface xls) =>
				{
					var anchor = new TClientAnchor(TFlxAnchorType.MoveAndDontResize, 5, 0, 4, 0, 6, 0, 5, 0, xls.Xls);
					var imageProps = new TImageProperties(anchor, "ImageName", "ImageName");
					xls.Xls.AddImage(TUIImage.CreateBitmap(1, 1, ImageColorDepth.Color256, 1, 1), imageProps);
				},
				@"{B}-[E]   {C}-[D]   {D}-[C]   {E}-[B]
{B}-[E]   {C}-[D]   {D}-[C]   {E}-[B]",
				(ExcelInterface xls) =>
				{
					var image = xls.Xls.GetImageProperties(1);
					AssertEquals("ImageName", image.FileName);
					AssertEquals("ImageName", image.ShapeName);
					AssertEquals(5, image.Anchor.Row1);
					AssertEquals(6, image.Anchor.Row2);
					AssertEquals(2, image.Anchor.Col1);
					AssertEquals(3, image.Anchor.Col2);
					AssertEquals(0, image.Anchor.Dx1);
					AssertEquals(0, image.Anchor.Dx2);
					AssertEquals(0, image.Anchor.Dy1);
					AssertEquals(0, image.Anchor.Dy2);
				});
		}

		public void TestRightToLeftColumns()
		{
			AssertRightToLeftTransform(
				"{B}-[B]   {C}-[C]   {D}-[D]   {E}-[E]",
				null,
				"{B}-[E]   {C}-[D]   {D}-[C]   {E}-[B]",
				null);
		}

		public void TestRightToLeftMergedCells()
		{
			AssertRightToLeftTransform(
@"{B}-[B-E]
{B}-[B-D]   {E}-[E]
{B}-[B-C]   {D}-[D-E]
{K}-[ExtraRow]
{B}-[B]   {C}-[C-E]",
				(ExcelInterface xls) =>
				{
					xls.WorkSheets[0].MergeCells(SectionBodyStartRow, 1, SectionBodyStartRow, 4);
					xls.WorkSheets[0].MergeCells(SectionBodyStartRow + 1, 1, SectionBodyStartRow + 1, 3);
					xls.WorkSheets[0].MergeCells(SectionBodyStartRow + 2, 1, SectionBodyStartRow + 3, 2);
					xls.WorkSheets[0].MergeCells(SectionBodyStartRow + 2, 3, SectionBodyStartRow + 3, 4);
					xls.WorkSheets[0].MergeCells(SectionBodyStartRow + 4, 2, SectionBodyStartRow + 4, 4);
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow + 2, 1, new CellFormat() { WrapText = true });
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow + 2, 3, new CellFormat() { WrapText = true });
				},
@"{B}-[B-E]
{B}-[E]   {C}-[B-D]
{B}-[D-E]   {D}-[B-C]
{K}-[ExtraRow]
{B}-[C-E]   {E}-[B]",
				(ExcelInterface xls) =>
				{
					AssertEquals(new ExcelCellRange(SectionBodyStartRow, 1, SectionBodyStartRow, 4), xls.WorkSheets[0].GetMergedCellRange(SectionBodyStartRow, 1));
					AssertEquals(new ExcelCellRange(SectionBodyStartRow + 1, 2, SectionBodyStartRow + 1, 4), xls.WorkSheets[0].GetMergedCellRange(SectionBodyStartRow + 1, 2));
					AssertEquals(new ExcelCellRange(SectionBodyStartRow + 2, 1, SectionBodyStartRow + 3, 2), xls.WorkSheets[0].GetMergedCellRange(SectionBodyStartRow + 2, 1));
					AssertEquals(new ExcelCellRange(SectionBodyStartRow + 2, 3, SectionBodyStartRow + 3, 4), xls.WorkSheets[0].GetMergedCellRange(SectionBodyStartRow + 2, 3));
					AssertEquals(new ExcelCellRange(SectionBodyStartRow + 4, 1, SectionBodyStartRow + 4, 3), xls.WorkSheets[0].GetMergedCellRange(SectionBodyStartRow + 4, 1));

					AssertEquals("WrapText", true, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow + 2, 1).WrapText);
					AssertEquals("WrapText", true, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow + 2, 3).WrapText);
				}
			);
		}

		public void TestRightToLeftTextAlignment()
		{
			AssertRightToLeftTransform(
				"{B}-[Label]   {C}-[Value]",
				(ExcelInterface xls) =>
				{
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow, 1, new CellFormat() { HTextAlign = HorizontalTextAlignment.Right });
				},
				"{B}-[Value]   {C}-[Label]",
				(ExcelInterface xls) =>
				{
					AssertEquals(HorizontalTextAlignment.Right, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow, 1).HTextAlign);
					AssertEquals(HorizontalTextAlignment.Left, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow, 2).HTextAlign);
				}
			);
		}

		public void TestRightToLeftBorderSwap()
		{
			AssertRightToLeftTransform(
				"{B}-[B]   {C}-[C]   {D}-[D]",
				(ExcelInterface xls) =>
				{
					var border = new CellBorder() { BorderColor = Color.Black, BorderStyle = CellBorderStyle.Thin };
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow, 1, new CellFormat() { Borders = new CellBorderFormat() { Left = border } });
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow, 3, new CellFormat() { Borders = new CellBorderFormat() { Right = border } });
				},
				"{B}-[D]   {C}-[C]   {D}-[B]",
				(ExcelInterface xls) =>
				{
					AssertEquals(CellBorderStyle.Thin, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow, 1).Borders.Left.BorderStyle);
					AssertEquals(CellBorderStyle.Thin, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow, 3).Borders.Right.BorderStyle);
				}
			);
		}

		public void TestMergedCellBorderSwap()
		{
			AssertRightToLeftTransform(
				"{B}-[BCD]",
				(ExcelInterface xls) =>
				{
					var border = new CellBorder() { BorderColor = Color.Black, BorderStyle = CellBorderStyle.Thin };
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow, 1, new CellFormat() { Borders = new CellBorderFormat() { Left = border } });
					xls.WorkSheets[0].SetCellFormat(SectionBodyStartRow, 3, new CellFormat() { Borders = new CellBorderFormat() { Right = border } });
					xls.WorkSheets[0].MergeCells(SectionBodyStartRow, 1, SectionBodyStartRow, 3);
				},
				"{B}-[BCD]",
				(ExcelInterface xls) =>
				{
					AssertEquals(CellBorderStyle.Thin, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow, 1).Borders.Left.BorderStyle);
					AssertEquals(CellBorderStyle.Thin, xls.WorkSheets[0].GetCellFormat(SectionBodyStartRow, 3).Borders.Right.BorderStyle);
				}
			);
		}

		public void TestDoesNotBlowUpWithTooManyColumns()
		{
			AssertRightToLeftTransform(
				"{B}-[B]   {C}-[C]   {D}-[D]",
				(ExcelInterface xls) =>
				{
					xls.WorkSheets[0][2, 255] = "FAIL";
				},
				"{B}-[D]   {C}-[C]   {D}-[B]",
				null
			);
		}

		#region Implementation

		void AssertRightToLeftTransform(string sectionBody, ExcelInterfaceCallbackDelegate setupFormattingCallback, string expectedSectionBodyResult, ExcelInterfaceCallbackDelegate checkFormattingCallback)
		{
			AssertResults(Setup(sectionBody, setupFormattingCallback), expectedSectionBodyResult, checkFormattingCallback);
		}

		ExcelTemplate Setup(string sectionBody, ExcelInterfaceCallbackDelegate setupFormattingCallback = null)
		{
			DocumentEngineTestHelper.ClearTemplates();

			var systemTemplateHelper = new TemplateTestHelper();
			systemTemplateHelper.AddWorkSheet("Document",
@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[HideColumnIf]   {K}-[1=1]
{A}-[#ConfigurableSection:GEN:Test, Test]
" + sectionBody + @"
{A}-[#EndOfReport]");

			var systemTemplate = systemTemplateHelper.CreateTemplate(Factory, "System Document Elements");
			if (setupFormattingCallback != null)
			{
				using (var excelInterface = new ExcelInterface(systemTemplate.SO_Template))
				{
					setupFormattingCallback(excelInterface);
					using (var ms = new MemoryStream())
					{
						excelInterface.SaveToStream(ms);
						systemTemplate.SO_Template = ms.ToArray();
					}
				}
			}

			var documentCommand = Factory.New<DocumentCommand>();
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = systemTemplate.PK;
			pivot.SI_SU = documentCommand.PK;

			var config = pivot.DocConfigs.AddNew();
			config.S3_IsSystem = ZBool.True;
			config.ConfigItems.AddFromTemplateSection(systemTemplate.TemplateSections.Find("Test"));

			var generator = new TemplateGenerator(pivot, null, Enterprise.Core.SharedConstants.Languages.Arabic);
			return generator.Generate(new FilterEvaluator());
		}

		void AssertResults(ExcelTemplate excelTemplate, string expectedSectionBody, ExcelInterfaceCallbackDelegate checkFormattingCallback = null)
		{
			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				AssertMultilineASCIIEquals("Right to Left",
@"{A}-[#config]
{A}-[Name=System Document Elements]
{A}-[HideColumnIf]   {K}-[1=1]
{A}-[#SectionBody]
" + expectedSectionBody + @"
{A}-[#EndOfReport]",
				excelInterface.WorkSheets[0].ToString());

				if (checkFormattingCallback != null)
				{
					checkFormattingCallback(excelInterface);
				}
			}
		}

		delegate void ExcelInterfaceCallbackDelegate(ExcelInterface excelInterface);

		const int SectionBodyStartRow = 4;

		#endregion
	}
}
