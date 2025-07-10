using System;
using CargoWise.IO;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	class PrinterFactoryTest : PrintEngineTestCase
	{
		public void TestGetPrinter_InvalidArguments()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PrinterFactory().GetPrinter(null));
		}

		public void TestGetPrinter()
		{
			var printerFactory = new PrinterFactory();
			var queue = new SerialisablePrintQueue { Name = "Test Printer" };
			AssertGetPrinter(typeof(PCLPrinter), "ZPL", queue, printerFactory);
			AssertGetPrinter(typeof(PCLPrinter), "PCL", queue, printerFactory);
			AssertGetPrinter(typeof(PdfPrinter), "PDF", queue, printerFactory);
			AssertGetPrinter(typeof(GraphicPrinter), "TIF", queue, printerFactory);
			AssertGetPrinter(typeof(FlexCelPrinter), "XLS", queue, printerFactory);
			AssertGetPrinter(typeof(FlexCelPrinter), "XLSX", queue, printerFactory);

			//Invalid blob type
			AssertGetPrinter(null, "TXT", queue, printerFactory);
			AssertGetPrinter(null, "EXE", queue, printerFactory);
		}

		void AssertGetPrinter(Type expectedPrinterType, string blobType, SerialisablePrintQueue queue, PrinterFactory printerFactory)
		{
			using (var directory = new TempDirectory())
			{
				var fileName = Temp.GetTempFileName(directory.DirectoryName, blobType);
				var serialisablePrintJob = GetSerialisablePrintJob(fileName);
				var job = new PrintEngineJob(serialisablePrintJob, queue, null);
				using (var printer = printerFactory.GetPrinter(job))
				{
					if (printer != null)
					{
						AssertType($"Should return the {expectedPrinterType.Name} printer", expectedPrinterType, printer);
					}
					else
					{
						AssertNull("Should return null for invalid blob type", printer);
						AssertNull("Should expect null for printer type", expectedPrinterType);
					}
				}
			}
		}
	}
}
