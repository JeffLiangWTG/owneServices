using System;
using System.Drawing.Printing;
using System.IO;
using CargoWise.IO;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class PdfPrinterTest : BasePrinterTestCase
	{
		public void TestDocumentName()
		{
			using (var printer = (MockPdfPrinter)GetPrinter())
			{
				AssertEquals(Path.GetFileNameWithoutExtension(PdfFileHavingThreePagesPath), printer.GetPrintDocumentExposed().DocumentName);
			}
		}

		public void TestNoOfCopiesSingleCopy()
		{
			using (var printer = (MockPdfPrinter)GetPrinter())
			{
				AssertEquals("1 copy should be printed", (short)1, printer.GetPrintDocumentExposed().PrinterSettings.Copies);
			}
		}

		public void TestNoOfCopiesMultipleCopies()
		{
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			serialisableJob.Copies = 3;
			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (MockPdfPrinter)GetPrinter(job))
			{
				AssertEquals("3 copies should be printed", (short)3, printer.GetPrintDocumentExposed().PrinterSettings.Copies);
			}
		}

		public void TestSupportCollation()
		{
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			serialisableJob.Copies = 3;
			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (MockPdfPrinter)GetPrinter(job))
			{
				printer.SupportsCollation = false;
				Assert("Collate should be false", !printer.GetPrintDocumentExposed().PrinterSettings.Collate);

				printer.SupportsCollation = true;
				Assert("Collate should be true", printer.GetPrintDocumentExposed().PrinterSettings.Collate);
			}
		}

		protected override string TestFileToPrint => PdfFileHavingThreePagesPath;

		protected override BasePrinter GetPrinter(PrintEngineJob printJob) => new MockPdfPrinter(printJob);

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string pdfFileHavingThreePagesPath;
		string PdfFileHavingThreePagesPath
		{
			get
			{
				if (string.IsNullOrEmpty(pdfFileHavingThreePagesPath))
				{
					pdfFileHavingThreePagesPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.PdfFileHavingThreePages.pdf", "PdfFileHavingThreePages.pdf");
				}
				return pdfFileHavingThreePagesPath;
			}
		}

		class MockPdfPrinter : PdfPrinter
		{
			public MockPdfPrinter(PrintEngineJob printJob)
				: base(printJob)
			{
			}

			public bool SupportsCollation { get; set; }

			public PrintDocument GetPrintDocumentExposed()
			{
				return GetPrintDocument().Document;
			}

			protected override bool PrinterSupportsCollation()
			{
				return SupportsCollation;
			}
		}
	}
}
