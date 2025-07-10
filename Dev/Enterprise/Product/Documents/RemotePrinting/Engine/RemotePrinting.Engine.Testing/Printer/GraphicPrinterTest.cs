using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using CargoWise.IO;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class GraphicPrinterTest : BasePrinterTestCase
	{
		public void TestImageToPrint()
		{
			using (GraphicPrinter printer = (GraphicPrinter)GetPrinter())
			{
				Image image1 = printer.ImageToPrint;
				AssertNotNull("Image to print should exist", image1);
			}
		}

		public void TestNoOfCopiesSingleCopy()
		{
			using (MockGraphicPrinter printer = (MockGraphicPrinter)GetPrinter())
			{
				AssertNoExceptionThrown(() => printer.Print());
				AssertEquals("1 copy should be printed", (short)1, printer.CurrentPrintDocument.PrinterSettings.Copies);
			}
		}

		public void TestNoOfCopiesMultipleCopies()
		{
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			serialisableJob.Copies = 3;

			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);

			using (var printer = (MockGraphicPrinter)GetPrinter(job))
			{
				AssertNoExceptionThrown(() => printer.Print());
				AssertEquals("3 copies should be printed", (short)3, printer.CurrentPrintDocument.PrinterSettings.Copies);
			}
		}

		public void TestSupportCollation()
		{
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			serialisableJob.Copies = 3;

			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (MockGraphicPrinter)GetPrinter(job))
			{
				printer.SupportsCollation = false;
				AssertNoExceptionThrown(() => printer.Print());
				Assert("Collate should be false", !printer.CurrentPrintDocument.PrinterSettings.Collate);

				printer.SupportsCollation = true;
				AssertNoExceptionThrown(() => printer.Print());
				Assert("Collate should be true", printer.CurrentPrintDocument.PrinterSettings.Collate);
			}
		}

		public void TestTotalPagesToPrint()
		{
			using (GraphicPrinter printer = (GraphicPrinter)GetPrinter())
			{
				AssertEquals("total number of pages should be 6", 6, printer.TotalPagesToPrint);
			}
		}

		public void TestImageToPrintWithMultipageTIF()
		{
			using (var originalStream = resourceRetriever.Value.GetStream("Enterprise.RemotePrinting.Engine.Testing.TestFiles.MultipageWithLandscapePages.tif"))
			using (var original = Image.FromStream(originalStream))
			using (var printer = (GraphicPrinter)GetPrinter())
			{
				AssertEquals("Page count should be the same", original.GetFrameCount(FrameDimension.Page), printer.ImageToPrint.GetFrameCount(FrameDimension.Page));
			}
		}

		public void TestImageToPrintTwice()
		{
			var printJob = GetPrintEngineJob(MultipageWithLandscapePagesTifPath);
			using (var printer1 = new GraphicPrinter(printJob))
			{
				var image1 = printer1.ImageToPrint;
				AssertNotNull("Image to print should exist", image1);

				using (var printer2 = new GraphicPrinter(printJob))
				{
					var image2 = printer1.ImageToPrint;
					Assert("We can get to this point, no file lock", true);
					AssertNotNull("Image to print should exist", image2);
				}
			}
		}

		public void TestGetNewDimensions()
		{
			var printJob = GetPrintEngineJob(MultipageWithLandscapePagesTifPath, 80m);
			using (var printer = new GraphicPrinter(printJob))
			{
				var newSize = printer.GetNewDimensions(new Size(400, 580));
				AssertEquals("Size should be changed to fit scale", 464, newSize.Height);
				AssertEquals("Size should be changed to fit scale", 320, newSize.Width);
			}
		}

		public void TestRescaleToZoom()
		{
			var printJob = GetPrintEngineJob(MultipageWithLandscapePagesTifPath, 70m);
			using (var printer = new GraphicPrinter(printJob))
			{
				AssertEquals("new value should be 70% of old", 280, printer.ReScaleToZoom(400));
				AssertEquals("new value should be 70% of old", 56, printer.ReScaleToZoom(80));
			}
		}

		public void TestIsLandscapePage()
		{
			var printJob = GetPrintEngineJob(MultipageWithLandscapePagesTifPath);
			using (var printer = new GraphicPrinter(printJob))
			{
				AssertEquals("Precondition: the image should have 6 pages", 6, printer.ImageToPrint.GetFrameCount(FrameDimension.Page));
				Assert("First page should be Portrait", !printer.IsA4LandscapePage);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 1);
				Assert("Second page should be Landscape", printer.IsA4LandscapePage);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 2);
				Assert("Third page should be Landscape", printer.IsA4LandscapePage);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 3);
				Assert("Fourth page should be Portrait", !printer.IsA4LandscapePage);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 4);
				Assert("Fifth page should be considered Portrait because it's not an A4 size page", !printer.IsA4LandscapePage);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 5);
				Assert("Sixth page should be considered Portrait because it's not an A4 size page", !printer.IsA4LandscapePage);
			}
		}

		public void TestScaleMarginsToImageDimensions()
		{
			var printJob1 = GetPrintEngineJob(SinglePageA4TifPath);
			using (var printer = new GraphicPrinter(printJob1))
			{
				AssertEquals("Precondition: the image should have 1 page", 1, printer.ImageToPrint.GetFrameCount(FrameDimension.Page));
				var current = new Rectangle(5, 5, 827, 1169);
				var result = printer.ScaleMarginsToImageDimensions(current, 0, 0);
				AssertEquals("Normal A4 page, Margins should be unchanged", new Rectangle(5, 5, 827, 1169), result);
			}

			var printJob2 = GetPrintEngineJob(MultipageWithLandscapePagesTifPath);
			using (var printer = new GraphicPrinter(printJob2))
			{
				AssertEquals("Precondition: the image should have 6 pages", 6, printer.ImageToPrint.GetFrameCount(FrameDimension.Page));
				var current = new Rectangle(5, 5, 75, 100);
				var result1 = printer.ScaleMarginsToImageDimensions(current, 0, 0);
				AssertEquals("Not a standard size page, Margins should be changed", new Rectangle(5, 5, 75, 99), result1);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 4);
				var result2 = printer.ScaleMarginsToImageDimensions(current, 0, 0);
				AssertEquals("Not a standard size page, Margins should be changed", new Rectangle(5, 5, 75, 17), result2);

				printer.ImageToPrint.SelectActiveFrame(FrameDimension.Page, 5);
				var result3 = printer.ScaleMarginsToImageDimensions(current, 0, 0);
				AssertEquals("Not a standard size page, Margins should be changed", new Rectangle(5, 5, 22, 100), result3);
			}
		}

		public void TestScaleMarginsToImageDimensions_WithHardMargin()
		{
			var printJob = GetPrintEngineJob(SinglePageA4TifPath);
			using (var printer = new GraphicPrinter(printJob))
			{
				AssertEquals("Precondition: the image should have 1 page", 1, printer.ImageToPrint.GetFrameCount(FrameDimension.Page));
				var current = new Rectangle(5, 5, 827, 1169);
				var result = printer.ScaleMarginsToImageDimensions(current, 16, 16);
				AssertEquals("Printer has hard margins, Margins should be changed", new Rectangle(5, 5, 795, 1124), result);
			}
		}

		public void TestScaleMarginsToPrintQueueZoom()
		{
			var current = new Rectangle(5, 5, 100, 100);
			var multipageWithLandscapePagesTifPath = MultipageWithLandscapePagesTifPath;

			var printJob1 = GetPrintEngineJob(multipageWithLandscapePagesTifPath, 80m);
			using (var printer = new GraphicPrinter(printJob1))
			{
				var result = printer.ScaleMarginsToPrintQueueZoom(current);

				AssertEquals("Result should be 80% of the original", 80, result.Height);
				AssertEquals("Result should be 80% of the original", 80, result.Width);
				AssertEquals("Location should be unchanged", current.Location, result.Location);
			}

			var printJob2 = GetPrintEngineJob(multipageWithLandscapePagesTifPath, 120m);
			using (var printer = new GraphicPrinter(printJob2))
			{
				var result = printer.ScaleMarginsToPrintQueueZoom(current);

				AssertEquals("Result should be 120% of the original", 120, result.Height);
				AssertEquals("Result should be 120% of the original", 120, result.Width);
				AssertEquals("Location should be unchanged", current.Location, result.Location);
			}
		}

		protected override string TestFileToPrint => MultipageWithLandscapePagesTifPath;

		protected override BasePrinter GetPrinter(PrintEngineJob job) => new MockGraphicPrinter(job);

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

		string multipageWithLandscapePagesTifPath;
		string MultipageWithLandscapePagesTifPath
		{
			get
			{
				if (string.IsNullOrEmpty(multipageWithLandscapePagesTifPath))
				{
					multipageWithLandscapePagesTifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.MultipageWithLandscapePages.tif", "MultipageWithLandscapePages.tif");
				}
				return multipageWithLandscapePagesTifPath;
			}
		}

		string singlePageA4TifPath;
		string SinglePageA4TifPath
		{
			get
			{
				if (string.IsNullOrEmpty(singlePageA4TifPath))
				{
					singlePageA4TifPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.SinglePageA4.tif", "SinglePageA4.tif");
				}
				return singlePageA4TifPath;
			}
		}

		class MockGraphicPrinter : GraphicPrinter
		{
			PrintDocument currentPrintDocument;

			public MockGraphicPrinter(PrintEngineJob printJob)
				: base(printJob)
			{
			}

			public bool EnablePrintFile { get; set; }

			public bool SupportsCollation { get; set; }

			public PrintDocument CurrentPrintDocument
			{
				get { return currentPrintDocument; }
			}

			protected override PrintDocument GetPrintDocument()
			{
				currentPrintDocument = base.GetPrintDocument();
				return currentPrintDocument;
			}

			protected override void Print(PrintDocument printDocument)
			{
				if (EnablePrintFile)
				{
					base.Print(printDocument);
				}
			}

			override protected bool PrinterSupportsCollation()
			{
				return SupportsCollation;
			}
		}
	}
}
