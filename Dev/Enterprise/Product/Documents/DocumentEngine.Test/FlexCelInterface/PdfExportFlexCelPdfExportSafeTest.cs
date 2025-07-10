using System;
using Enterprise.DocumentEngineCore.Registry;
using FlexCel.Pdf;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class PdfExportFlexCelPdfExportSafeTest : TransactionedTestCase
	{
		public void TestInitialise()
		{
			using (DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var pdfExport = new FlexCelPdfExportSafe(null, true, TPdfType.Standard))
			{
				AssertEquals("Font embedding in PDF should respect registry setting.", FlexCel.Pdf.TFontEmbed.None, pdfExport.FontEmbed);
				Assert("Should not use Excel properties.", !pdfExport.UseExcelProperties);
				Assert("Should have Leelawadee UI as a fall back font for Khmer", pdfExport.FallbackFonts.Contains(";Leelawadee UI"));
				AssertEquals("Should ignore license check when embedding fonts in PDF.", FlexCel.Pdf.TUnlicensedFontAction.Ignore, pdfExport.UnlicensedFontAction);
			}

			using (DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var pdfExport = new FlexCelPdfExportSafe(null, true, TPdfType.Standard))
			{
				AssertEquals("Font embedding in PDF should respect registry setting.", FlexCel.Pdf.TFontEmbed.Embed, pdfExport.FontEmbed);
			}

			using (DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var pdfExport = new FlexCelPdfExportSafe(null, true, TPdfType.PDFA1))
			{
				AssertEquals("PDF/A type should be PDF/A-1.", TPdfType.PDFA1, pdfExport.PdfType);
				Assert("Should not use Excel properties.", !pdfExport.UseExcelProperties);
				AssertEquals("TUnlicensedFontAction should be Replace for PDF/A.", FlexCel.Pdf.TUnlicensedFontAction.Replace, pdfExport.UnlicensedFontAction);
			}

			using (DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var pdfExport = new FlexCelPdfExportSafe(null, true, TPdfType.PDFA2))
			{
				AssertEquals("PDF/A type should be PDF/A-2.", TPdfType.PDFA2, pdfExport.PdfType);
				Assert("Should not use Excel properties.", !pdfExport.UseExcelProperties);
				AssertEquals("TUnlicensedFontAction should be Replace for PDF/A.", FlexCel.Pdf.TUnlicensedFontAction.Replace, pdfExport.UnlicensedFontAction);
			}

			using (DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var pdfExport = new FlexCelPdfExportSafe(null, true, TPdfType.PDFA3))
			{
				AssertEquals("PDF/A type should be PDF/A-3.", TPdfType.PDFA3, pdfExport.PdfType);
				Assert("Should not use Excel properties.", !pdfExport.UseExcelProperties);
				AssertEquals("TUnlicensedFontAction should be Replace for PDF/A.", FlexCel.Pdf.TUnlicensedFontAction.Replace, pdfExport.UnlicensedFontAction);
			}
		}

		public void TestFallbackFonts()
		{
			using (var pdfExport = new FlexCelPdfExportSafe(true))
			{
				AssertEquals("Arial Unicode MS;Segoe UI Symbol;Yu Mincho;Yu Gothic;Ms Mincho;Ms Gothic;Leelawadee UI", pdfExport.FallbackFonts);
			}

			using (var pdfExport = new FlexCelPdfExportSafe(false))
			{
				AssertEquals("Microsoft YaHei;Microsoft JhengHei;Malgun Gothic;Nirmala UI;Segoe UI Symbol;Yu Mincho;Yu Gothic;Ms Mincho;Ms Gothic;Leelawadee UI", pdfExport.FallbackFonts);
			}
		}
	}
}
