using System;
using System.Drawing.Printing;
using System.IO;
using CargoWise.PdfiumWrapper;

namespace Enterprise.RemotePrinting.Engine
{
	/// <summary>
	/// Prints a Pdf file. Good for printing one file only. If you would like to print multiple files, instantiate 
	/// a PdfPrinter for each file.
	/// </summary>
	public class PdfPrinter : BasePrinter
	{
		public PdfPrinter(PrintEngineJob printJob)
			: base(printJob)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override void ProcessAndPrintDocument()
		{
			using (var document = GetPrintDocument())
			{
				Log("Printing Pdf document");
				document.Document.Print();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected PrintDocumentDisposer GetPrintDocument()
		{
			Log("Loading Pdf document content");

			var memoryStream = new MemoryStream(Contents);
			var pdfToPrint = new PdfDocument(memoryStream);
			var nbOfCopies = (short)Copies;

			Log("Preparing Pdf document to print");
			var document = pdfToPrint.ToPrintDocument();
			document.DocumentName = DocumentName;
			document.PrinterSettings.PrinterName = PrinterName;
			document.PrinterSettings.Copies = nbOfCopies;
			if (nbOfCopies > 1 && PrinterSupportsCollation())
			{
				document.PrinterSettings.Collate = true;
			}

			return new PrintDocumentDisposer(document, pdfToPrint, memoryStream);
		}

		protected sealed class PrintDocumentDisposer : IDisposable
		{
			public PrintDocumentDisposer(PrintDocument document, PdfDocument pdfToPrint, Stream pdfStream)
			{
				Document = document;
				PdfToPrint = pdfToPrint;
				PdfStream = pdfStream;
			}

			public PrintDocument Document { get; }
			readonly PdfDocument PdfToPrint;
			readonly Stream PdfStream;

			public void Dispose()
			{
				Document?.Dispose();
				PdfToPrint?.Dispose();
				PdfStream?.Dispose();
			}
		}
	}
}
