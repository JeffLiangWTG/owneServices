using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.RemotePrinting.Types;
using FlexCel.Core;
using FlexCel.Render;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public sealed class FlexCelPrinterTest : BasePrinterForXLSFileTest
	{
		public void TestRefreshPrinterWhenPrintFileOccursWin32Exception()
		{
			var emails = new List<string>();
			SendNotificationEmailHelper.Instance.SendNotificationEmail += (sender, e) => { emails.Add(e.Subject + "\r\n" + e.Body); };
			var controller = new FlexCelPrinter.FlexCelStandardPrintController();
			var messages = new List<string>();
			var serialisableJob = GetSerialisablePrintJob(DocumentWithUnderlinesXlsPath);
			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = new MockFlexCelPrinterForRefreshPrinter(job))
			{
				printer.EnablePrintFile = true;
				printer.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);

				var expectedLogs = @"Loading Excel file into memory
Preparing Excel document to print
Printing Excel document sheet 1
The printing job failed because the printer [TestNullQueue] could not be accessed.
System exception could not be handled. Exception messages: Win32Exception for refresh printer test.
Retries enabled: False, Retry attempted: 1.";
				AssertExceptionThrown<Win32Exception>(() => printer.Print());
				AssertEquals(expectedLogs, string.Join("\r\n", messages));
				var email = @"Printing failed on printer TestNullQueue
A system-level error occurred a document on printer TestNullQueue.
This error may indicate an issue with printer or its connection to print server machine.
Internal refreshing list of printers did not resolve this error.

Document which was being printed: DocumentWithUnderlines.
Error message: The printing job failed because the printer [TestNullQueue] could not be accessed.

Please contact your administrator to check if the printer is working properly, and restart this WebPrint Client.
";
				AssertEquals(email, emails[0]);

				SendNotificationEmailHelper.Instance.SetLastSendNotificationTimeForTest("Printing failed on printer TestNullQueue", DateTime.MinValue);
				emails.Clear();
				messages.Clear();
				printer.OpenPrinterForTest = true;
				expectedLogs = @"Loading Excel file into memory
Preparing Excel document to print
Printing Excel document sheet 1
Refresh printer [TestNullQueue] failed with error code:";
				AssertExceptionThrown<Win32Exception>(() => printer.Print());
				AssertContains(expectedLogs, string.Join("\r\n", messages));

				email = @"Printing failed on printer TestNullQueue
A system-level error occurred a document on printer TestNullQueue.
This error may indicate an issue with printer or its connection to print server machine.
Internal refreshing list of printers did not resolve this error.

Document which was being printed: DocumentWithUnderlines.
Error message: Refresh printer [TestNullQueue] failed with error code:";
				AssertContains(email,  emails[0]);
				AssertContains("Please contact your administrator to check if the printer is working properly, and restart this WebPrint Client.", emails[0]);

				SendNotificationEmailHelper.Instance.SetLastSendNotificationTimeForTest("", DateTime.MinValue);
				emails.Clear();
				messages.Clear();
				printer.OpenPrinterForTest = true;
				printer.SetPrinterForTest = true;
				expectedLogs = @"Loading Excel file into memory
Preparing Excel document to print
Printing Excel document sheet 1
The printer [TestNullQueue] has been refreshed.
Printing Excel document sheet 1
The printer [TestNullQueue] has been refreshed.
Printing Excel document sheet 1
The printer [TestNullQueue] has been refreshed.
System exception could not be handled. Exception messages: Win32Exception for refresh printer test.
Retries enabled: True, Retry attempted: 3.";
				AssertExceptionThrown<Win32Exception>(() => printer.Print());
				AssertEquals(expectedLogs, string.Join("\r\n", messages));
				AssertEquals("Should no notification email", 0, emails.Count);
			}
		}

		public void TestPrintDocumentNameForOnlyOneSheet()
		{
			var messages = new List<string>();

			var serialisableJob = GetSerialisablePrintJob(DocumentWithUnderlinesXlsPath);
			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (MockFlexCelPrinter)GetPrinter(job))
			{
				printer.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);

				printer.EnablePrintFile = true;
				AssertNoExceptionThrown(() => printer.Print());
				AssertEquals("DocumentWithUnderlines", job.DocumentName);
			}

			AssertCollectionContains("Loading Excel file into memory", messages);
			AssertCollectionContains("Preparing Excel document to print", messages);
			AssertCollectionContains("Printing Excel document sheet 1", messages);
			AssertCollectionContains("Disposing printer", messages);
		}

		public void TestIsRollPaper()
		{
			var serializableJob = GetSerialisablePrintJob(MixedPaperSizesXlsPath);
			serializableJob.Copies = 0;
			var queue = GetSerialisablePrintQueue();
			queue.IsRollPaper = true;

			var job = new PrintEngineJob(serializableJob, queue, null);
			using (var printer = (MockFlexCelPrinter)GetPrinter(job))
			{
				Assert(printer.IsRollPaperExposed);
			}
		}

		public void TestPrintInvalidNoOfCopiesProcessdsWithoutThrowingException()
		{
			var serialisableJob = GetSerialisablePrintJob(MixedPaperSizesXlsPath);
			serialisableJob.Copies = 0;

			var queue = GetSerialisablePrintQueue();

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (MockFlexCelPrinter)GetPrinter(job))
			{
				printer.EnablePrintFile = true;
				printer.SupportsCollation = true;
				printer.AfterGetPrintDocument += delegate
				{
					var paperSize = new PaperSize();
					paperSize.RawKind = (int)PaperKind.A4;
					printer.CurrentPrintDocument.DefaultPageSettings.PaperSize = paperSize;
				};

				AssertNoExceptionThrown(() => printer.Print());
				AssertEquals("PrintDocuments.Count", 0, printer.PrintDocuments.Count);
			}
		}

		public void TestPrintWithScalingSheet()
		{
			var messages = new List<string>();

			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				printer.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);
				AssertNoExceptionThrown(() => printer.Print());
			}

			AssertCollectionContains("Applying scaling sheet to Excel file", messages);
		}

		public void TestScalingToA4()
		{
			AssertScaling(PaperKind.A4,
				new PrintDetails("A4", 100, GetA4PaperSize()),
				new PrintDetails("Letter", 100, GetLetterPaperSize()));
		}

		public void TestScalingToLetter()
		{
			PaperSize a4Details = new PaperSize();
			a4Details.RawKind = (int)PaperKind.Letter;
			AssertScaling(PaperKind.Letter,
				new PrintDetails("A4", 92, a4Details),
				new PrintDetails("Letter", 100, GetLetterPaperSize()));
		}

		[ExpectNoExceptions]
		[TestDate(2010, 10, 10)]
		[RequiresSoftware(RequiredSoftware.XpsPrinter)]
		public void TestUnderlinesAndWatermark()
		{
			RunTestInAnotherThreadWithTimeout(delegate
			{
				var startTime = DateTime.UtcNow;
				var queue = new SerialisablePrintQueue();
				queue.Name = XPSPrinterName;

				var serialisableJob = GetSerialisablePrintJob(DocumentWithUnderlinesXlsPath);
				serialisableJob.HasWatermark = true;

				var watermark = new TextWatermark(
					"Draft",
					WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle,
					0f, 0f, 45, Color.Black, "Arial", 30, FontStyle.Bold);

				string tempFile = TempForTest.GetTempFileName();

				try
				{
					var job = new PrintEngineJob(serialisableJob, queue, watermark);
					using (var printer = (MockFlexCelPrinter)GetPrinter(job))
					{
						printer.EnablePrintFile = true;
						printer.AfterGetPrintDocument += delegate
						{
							var pageSettings = new PageSettings(printer.CurrentPrintDocument.PrinterSettings);
							pageSettings.PaperSize = GetA4PaperSize();
							printer.CurrentPrintDocument.DefaultPageSettings = pageSettings;
							printer.CurrentPrintDocument.ReallyPrint = true;
							printer.CurrentPrintDocument.PrinterSettings.PrintToFile = true;
							printer.CurrentPrintDocument.PrinterSettings.PrintFileName = tempFile;
						};

						printer.Print();
					}
					//The printer seems to release the lock on the tempFile AFTER the printer object has been disposed.
					//Same thing when trying to access the tempFile on PrintDocument.EndPrint event, file is still locked.
					do
					{
						Thread.Sleep(TimeSpan.FromMilliseconds(100));
					}
					while (IsFileStillLockedByPrinter(tempFile) && DateTime.UtcNow.Subtract(startTime) < TimeSpan.FromSeconds(150));

					Assert("the file should still exist", File.Exists(tempFile));
					Assert("the file should not be empty", new FileInfo(tempFile).Length != 0);

					var actualXpsContent = GetActualXpsContent(tempFile);
					var actualXml = new XmlDocument();
					actualXml.LoadXml(actualXpsContent.FullContent);

					var expectedXml = GetExpectedXpsXml(actualXpsContent.FontGuid1, actualXpsContent.FontGuid2);
					AssertXpsXmlEquals("Xml", expectedXml.ChildNodes, actualXml.ChildNodes);
				}
				catch (Win32Exception ex)
				{
					if (ex.NativeErrorCode != 6)
					{
						throw;
					}
				}
				finally
				{
					int retries = 0;
					while (retries <= 4)
					{
						try
						{
							Thread.Sleep((int)Math.Pow(10, retries));
							DeleteIfExists(tempFile);
							break;
						}
						catch (IOException)
						{
							++retries;
						}
					}
				}
			}
			, TimeSpan.FromSeconds(160));
		}

		public const string XPSPrinterName = "Microsoft XPS Document Writer";

		protected override string TestFileToPrint => resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.DocumentWithScalingSheet.xls", "DocumentWithScalingSheet.xls");

		protected override BasePrinter GetPrinter(PrintEngineJob printJob) => new MockFlexCelPrinter(printJob);

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

		string mixedPaperSizesXlsPath;
		string MixedPaperSizesXlsPath
		{
			get
			{
				if (string.IsNullOrEmpty(mixedPaperSizesXlsPath))
				{
					mixedPaperSizesXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.MixedPaperSizes.xls", "MixedPaperSizes.xls");
				}
				return mixedPaperSizesXlsPath;
			}
		}

		string documentWithUnderlinesXlsPath;
		string DocumentWithUnderlinesXlsPath
		{
			get
			{
				if (string.IsNullOrEmpty(documentWithUnderlinesXlsPath))
				{
					documentWithUnderlinesXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.DocumentWithUnderlines.xls", "DocumentWithUnderlines.xls");
				}
				return documentWithUnderlinesXlsPath;
			}
		}

		static void AssertXpsXmlEquals(string nodeName, XmlNodeList expectedNodes, XmlNodeList actualNodes)
		{
			AssertEquals(nodeName + ".Count", expectedNodes.Count, actualNodes.Count);

			for (int i = 0; i < expectedNodes.Count; i++)
			{
				XmlNode actualNode = actualNodes[i];
				XmlNode expectedNode = expectedNodes[i];
				string nodeId = nodeName + ".ChildNodes[" + i + "].";

				AssertEquals(nodeId + "Name", expectedNode.Name, actualNode.Name);
				AssertNodeEquals(nodeId + "Value", expectedNode.Value, actualNode.Value);
				AssertNodeEquals(nodeId + "InnerText", expectedNode.InnerText, actualNode.InnerText);

				if (expectedNode.Attributes == null && actualNode.Attributes != null)
				{
					Fail("expectedNode.Attributes is null while actualNode.Attributes is not null");
				}
				else if (expectedNode.Attributes != null && actualNode.Attributes == null)
				{
					Fail("expectedNode.Attributes is not null while actualNode.Attributes is null");
				}
				else if (expectedNode.Attributes != null && actualNode.Attributes != null)
				{
					AssertEquals(nodeId + ".Attributes.Count", expectedNode.Attributes.Count, actualNode.Attributes.Count);

					for (int j = 0; j < expectedNode.Attributes.Count; j++)
					{
						XmlAttribute actualAttribute = actualNode.Attributes[j];
						XmlAttribute expectedAttribute = expectedNode.Attributes[j];
						string attributeId = nodeId + "Attributes[" + j + "].";

						AssertEquals(attributeId + "Name", expectedAttribute.Name, actualAttribute.Name);
						if ((expectedAttribute.Name == "OriginX") || (expectedAttribute.Name == "OriginY"))
						{
							double actualOrigin = Math.Floor(double.Parse(expectedAttribute.Value));
							double expectedOrigin = Math.Floor(double.Parse(expectedAttribute.Value));
							AssertEquals(attributeId + "Value", expectedOrigin, actualOrigin);
						}
						else
						{
							AssertEquals(attributeId + "Value", expectedAttribute.Value, actualAttribute.Value);
						}
					}
				}

				AssertXpsXmlEquals(nodeName + '.' + expectedNode.Name, expectedNode.ChildNodes, actualNode.ChildNodes);
			}
		}

		static void AssertNodeEquals(string message, string expected, string result)
		{
			var xpsVersionRegex = new Regex(@"Microsoft\sXPS\sDocument\sConverter\s\(MXDC\)\sGenerated!\sVersion:\s?\d{1,9}\.\d{1,9}\.\d{1,9}\.\d{1,9}");

			if (!string.IsNullOrEmpty(expected) && xpsVersionRegex.IsMatch(expected))
			{
				Assert(message, xpsVersionRegex.IsMatch(result));
			}
			else
			{
				AssertEquals(message, expected, result);
			}
		}

		static PaperSize GetA4PaperSize()
		{
			return new PaperSize("A4", 827, 1169);
		}

		static XpsContent GetActualXpsContent(string xpsFilePath)
		{
			string fullContent = null;
			string fontGuid1 = null;
			string fontGuid2 = null;

			using (ZipArchive zipFile = new ZipArchive(File.OpenRead(xpsFilePath)))
			{
				ZipArchiveEntry entry = zipFile.GetEntry("Documents/1/Pages/_rels/1.fpage.rels");
				using (Stream zipStream = entry.Open())
				{
					XPathDocument xml = new XPathDocument(zipStream);
					XPathNavigator navigator = xml.CreateNavigator();
					XmlNamespaceManager namespaceManager = new XmlNamespaceManager(navigator.NameTable);
					namespaceManager.AddNamespace("Relationships", "http://schemas.openxmlformats.org/package/2006/relationships");
					XPathNodeIterator iterator = navigator.Select("//Relationships:Relationship", namespaceManager);

					iterator.MoveNext();
					fontGuid1 = GetFontGuid(iterator.Current);
					iterator.MoveNext();
					fontGuid2 = GetFontGuid(iterator.Current);
				}

				entry = zipFile.GetEntry("Documents/1/Pages/1.fpage");
				using (Stream zipStream = entry.Open())
				{
					byte[] buffer = new byte[8000];
					int bytesRead;
					while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						fullContent += Encoding.ASCII.GetString(buffer, 0, bytesRead);
					}
				}
			}

			return new XpsContent(fontGuid1, fontGuid2, fullContent);
		}

		static XmlDocument GetExpectedXpsXml(string fontGuid1, string fontGuid2)
		{
			string contents = string.Format(ContentsDefault, fontGuid1, fontGuid2);

			var result = new XmlDocument();
			result.LoadXml(contents);
			return result;
		}

		static string GetFontGuid(XPathNavigator navigator)
		{
			string target = navigator.GetAttribute("Target", string.Empty);
			if (new VersionHelper().IsWindows8OrGreater())
			{
				return target.Substring(19, 36);
			}
			else
			{
				return target.Substring(29, 36);
			}
		}

		static PaperSize GetLetterPaperSize()
		{
			return new PaperSize(nameof(PaperKind.Letter), 850, 1100);
		}

		void AssertScaling(PaperKind defaultPaperSizeKind, PrintDetails expectedA4Details, PrintDetails expectedLetterDetails)
		{
			var mixedPaperSizesXlsPath = MixedPaperSizesXlsPath;
			var serialisableJob = GetSerialisablePrintJob(mixedPaperSizesXlsPath);
			serialisableJob.Copies = 3;
			var queue = GetSerialisablePrintQueue();

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (MockFlexCelPrinter)GetPrinter(job))
			{
				printer.EnablePrintFile = true;
				printer.AfterGetPrintDocument += delegate
				{
					var paperSize = new PaperSize();
					paperSize.RawKind = (int)defaultPaperSizeKind;
					printer.CurrentPrintDocument.DefaultPageSettings.PaperSize = paperSize;
				};

				printer.Print();

				AssertEquals("PrintDocuments.Count", 3, printer.PrintDocuments.Count);
				for (int i = 0; i < printer.PrintDocuments.Count; i++)
				{
					var printDocument = printer.PrintDocuments[i];
					string id = "PrintDocuments[" + i + "]";
					AssertEquals(id + ".PrintDetails.Count", 2, printDocument.PrintDetails.Count);
					AssertEquals(id + ".PrintDetails[0]", expectedA4Details, printDocument.PrintDetails[0]);
					AssertEquals(id + ".PrintDetails[1]", expectedLetterDetails, printDocument.PrintDetails[1]);
					AssertEquals(Path.GetFileNameWithoutExtension(mixedPaperSizesXlsPath), printDocument.ExcelFile.ActiveFileName);
				}
			}
		}

		bool IsFileStillLockedByPrinter(string filename)
		{
			FileStream stream = null;
			try
			{
				stream = new FileInfo(filename).Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}
			return false;
		}

		const string ContentsDefault = @"<FixedPage Width=""793.76"" Height=""1122.56"" xmlns=""http://schemas.openxps.org/oxps/v1.0"" xml:lang=""und"">
	<!-- Microsoft XPS Document Converter (MXDC) Generated! Version: 0.3.9600.17415 -->
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{0}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""208.32"" OriginY=""107.52"" Indices=""43;72,55;68;71,62;72,55;85"" UnicodeString=""Header"" />
	<Path Data=""F1 M 208.32,108.96 L 253.28,108.96""  Stroke=""#ff000000"" StrokeThickness=""1.44"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""73.6"" OriginY=""123.68"" Indices=""41;76;72,55;79;71,55;20"" UnicodeString=""Field1"" />
	<Path Data=""F1 M 73.6,125.12 L 109.6,125.12""  Stroke=""#ff000000"" StrokeThickness=""0.96"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""198.08"" OriginY=""123.68"" Indices=""20"" UnicodeString=""1"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""275.68"" OriginY=""123.68"" Indices=""41;76;72,55;79;71,55;22"" UnicodeString=""Field3"" />
	<Path Data=""F1 M 275.68,125.12 L 311.68,125.12""  Stroke=""#ff000000"" StrokeThickness=""0.96"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""400"" OriginY=""123.68"" Indices=""23"" UnicodeString=""4"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""73.6"" OriginY=""140.16"" Indices=""41;76;72,55;79;71,55;21"" UnicodeString=""Field2"" />
	<Path Data=""F1 M 73.6,141.6 L 109.6,141.6""  Stroke=""#ff000000"" StrokeThickness=""0.96"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""198.08"" OriginY=""140.16"" Indices=""21"" UnicodeString=""2"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""275.68"" OriginY=""140.16"" Indices=""41;76;72,55;79;71,55;23"" UnicodeString=""Field4"" />
	<Path Data=""F1 M 275.68,141.6 L 311.68,141.6""  Stroke=""#ff000000"" StrokeThickness=""0.96"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""400"" OriginY=""140.16"" Indices=""24"" UnicodeString=""5"" />
	<Glyphs Fill=""#ff000000"" FontUri=""../Resources/Fonts/{1}.odttf"" FontRenderingEmSize=""13.2805"" StyleSimulations=""None"" OriginX=""208.32"" OriginY=""156.64"" Indices=""55,63;68,55;76;79"" UnicodeString=""Tail"" />
	<Glyphs RenderTransform=""0.0863201,-0.0863201,0.0863167,0.0863167,0,0"" Fill=""#ff000000"" FontUri=""../Resources/Fonts/{0}.odttf"" FontRenderingEmSize=""327.68"" StyleSimulations=""None"" OriginX=""-1346.56"" OriginY=""5643.36"" Indices=""39,72;85,39;68,56;73,33;87"" UnicodeString=""Draft"" />
</FixedPage>
";

		#region MockFlexCelPrinterForRefreshPrinter

		class MockFlexCelPrinterForRefreshPrinter : MockFlexCelPrinter
		{
			public MockFlexCelPrinterForRefreshPrinter(PrintEngineJob printJob)
				: base(printJob)
			{
			}

			public bool OpenPrinterForTest { get; set; }

			public bool SetPrinterForTest { get; set; }

			protected override bool OpenPrinterCore(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault)
			{
				phPrinter = IntPtr.Zero;
				return OpenPrinterForTest;
			}

			protected override bool ClosePrinterCore(IntPtr hPrinter)
			{
				return true;
			}

			protected override bool SetPrinterCore(IntPtr hPrinter, int level, IntPtr pPrinter, int command)
			{
				return SetPrinterForTest;
			}

			protected override IPrintDocument GetPrintDocument(ExcelFile excelFile)
			{
				return new MockPrintDocumentForRefreshPrinter(excelFile);
			}
		}

		class MockPrintDocumentForRefreshPrinter : MockPrintDocument, IPrintDocument
		{
			public MockPrintDocumentForRefreshPrinter(ExcelFile excelFile)
				: base(excelFile)
			{
			}

			void IPrintDocument.Print()
			{
				((FlexCelPrinter.FlexCelStandardPrintController)PrintController).ExceptionForRefreshPrinterTest = new Win32Exception("Win32Exception for refresh printer test.");
				PrintController.OnStartPage(this, null);
			}
		}

		#endregion

		#region MockFlexCelPrinter

		class MockFlexCelPrinter : FlexCelPrinter
		{
			MockPrintDocument currentPrintDocument;
			List<MockPrintDocument> printDocuments;

			public MockFlexCelPrinter(PrintEngineJob printJob)
				: base(printJob)
			{
			}

			public MockPrintDocument CurrentPrintDocument
			{
				get { return currentPrintDocument; }
			}

			public bool EnablePrintFile { get; set; }

			public bool SupportsCollation { get; set; }

			public List<MockPrintDocument> PrintDocuments
			{
				get { return printDocuments ?? (printDocuments = new List<MockPrintDocument>()); }
			}

			public event EventHandler AfterGetPrintDocument;

			protected override IPrintDocument GetPrintDocument(ExcelFile excelFile)
			{
				currentPrintDocument = new MockPrintDocument(excelFile);
				PrintDocuments.Add(currentPrintDocument);
				OnAfterGetPrintDocument();
				return currentPrintDocument;
			}

			void OnAfterGetPrintDocument()
			{
				EventHandler afterGetPrintDocument = AfterGetPrintDocument;
				if (afterGetPrintDocument != null)
				{
					afterGetPrintDocument(this, EventArgs.Empty);
				}
			}

			protected override void PrintFile(ExcelFile excelFile)
			{
				if (EnablePrintFile)
				{
					base.PrintFile(excelFile);
				}
			}

			protected override bool PrinterSupportsCollation()
			{
				return SupportsCollation;
			}

			public bool IsRollPaperExposed => IsRollPaper;
		}

		#endregion

		#region MockPrintDocument

		class MockPrintDocument : FlexCelPrintDocument, IPrintDocument
		{
			readonly ExcelFile excelFile;
			List<PrintDetails> printDetails;
			bool reallyPrint;

			public MockPrintDocument(ExcelFile excelFile)
				: base(excelFile)
			{
				this.excelFile = excelFile;
				DefaultPageSettings = new PageSettings();
			}

			public ExcelFile ExcelFile
			{
				get { return excelFile; }
			}

			public List<PrintDetails> PrintDetails
			{
				get { return printDetails ?? (printDetails = new List<PrintDetails>()); }
			}

			public bool ReallyPrint
			{
				get { return reallyPrint; }
				set { reallyPrint = value; }
			}

			#region IPrintDocument Members

			PaperSize IPrintDocument.DefaultPaperSize
			{
				get { return DefaultPageSettings.PaperSize; }
				set { DefaultPageSettings.PaperSize = value; }
			}

			event PrintPageEventHandler IPrintDocument.BeforePrintPage
			{
				add { }
				remove { }
			}

			event PrintHardMarginsEventHandler IPrintDocument.GetPrinterHardMargins
			{
				add { }
				remove { }
			}

			void IPrintDocument.Print()
			{
				if (ReallyPrint)
				{
					Print();
				}
				else
				{
					PrintDetails.Add(new PrintDetails(ExcelFile.SheetName, ExcelFile.PrintScale, DefaultPageSettings.PaperSize));
				}
			}

			#endregion

			#region IDisposable Members

			void IDisposable.Dispose()
			{
			}

			#endregion
		}

		#endregion

		#region PrintDetails

		struct PrintDetails : IEquatable<PrintDetails>
		{
			readonly PaperSize paperSize;
			readonly int printScale;
			readonly string sheetName;

			public PrintDetails(string sheetName, int printScale, PaperSize paperSize)
			{
				this.sheetName = sheetName;
				this.printScale = printScale;
				this.paperSize = paperSize;
			}

			public PaperSize PaperSize
			{
				get { return paperSize; }
			}

			public int PrintScale
			{
				get { return printScale; }
			}

			public string SheetName
			{
				get { return sheetName; }
			}

			public override bool Equals(object obj)
			{
				return (obj is PrintDetails) && Equals((PrintDetails)obj);
			}

			public bool Equals(PrintDetails other)
			{
				return other.SheetName == SheetName
					&& other.PrintScale == PrintScale
					&& PaperSizeEquals(other.PaperSize, PaperSize);
			}

			public override int GetHashCode()
			{
				int sheetNameHash = (SheetName == null) ? 0 : SheetName.GetHashCode();
				return PaperSize.PaperName.GetHashCode() ^ PrintScale ^ sheetNameHash;
			}

			static bool PaperSizeEquals(PaperSize x, PaperSize y)
			{
				return x.Height == y.Height
					&& x.Kind == y.Kind
					&& x.PaperName == y.PaperName
					&& x.RawKind == y.RawKind
					&& x.Width == y.Width;
			}

			public override string ToString()
			{
				return string.Format("SheetName: {0}, PrintScale: {1}, PaperSize: {2}", SheetName, PrintScale.ToString(), PaperSize.ToString());
			}
		}

		#endregion

		#region XpsContent

		struct XpsContent
		{
			readonly string fontGuid1;
			readonly string fontGuid2;
			readonly string fullContent;

			public XpsContent(string fontGuid1, string fontGuid2, string fullContent)
			{
				this.fontGuid1 = fontGuid1;
				this.fontGuid2 = fontGuid2;
				this.fullContent = fullContent;
			}

			public string FontGuid1
			{
				get { return fontGuid1; }
			}

			public string FontGuid2
			{
				get { return fontGuid2; }
			}

			public string FullContent
			{
				get { return fullContent; }
			}
		}

		#endregion
	}
}
