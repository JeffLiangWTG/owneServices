using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using CargoWise.IO;
using Enterprise.RemotePrinting.Types;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class PrintEngineProcessorTest : PrintEngineTestCase
	{
		public void TestSetPrinter()
		{
			var processor = new PrintEngineProcessor(new NullPrinterFactory());
			processor.SetPrinter(new SerialisablePrintQueue { Name = "A" });

			var job = new SerialisablePrintJob { JobPk = Guid.NewGuid(), BlobType = "XLS", QueueName = "A" };
			var randomGuid = Guid.NewGuid();
			AssertEquals("Printer A has a different Stamp.", true, processor.HasQueueChanged("A", randomGuid));
			AssertEquals("A should be considered a valid printer.", true, processor.PrintAsync(job, null).Result.IsSuccess);

			processor.SetPrinter(new SerialisablePrintQueue { Name = "A", StateChangedStamp = randomGuid });
			AssertEquals("Printer A should be overriden with identical stamp.", false, processor.HasQueueChanged("A", randomGuid));
			AssertEquals("A should be considered a valid printer.", true, processor.PrintAsync(job, null).Result.IsSuccess);
		}

		public void TestUpdatePrinters()
		{
			var processor = new PrintEngineProcessor(new NullPrinterFactory());
			processor.UpdatePrinters(new[] { "A", "B" });

			var randomGuid = Guid.NewGuid();
			var job = new SerialisablePrintJob { JobPk = Guid.NewGuid(), BlobType = "XLS", QueueName = "A" };
			AssertEquals("A should be considered a valid printer.", true, processor.PrintAsync(job, null).Result.IsSuccess);

			job.QueueName = "B";
			AssertEquals("B should be considered a valid printer.", true, processor.PrintAsync(job, null).Result.IsSuccess);

			job.QueueName = "C";
			var failureResult1 = processor.PrintAsync(job, null).Result;
			AssertEquals("C should not be considered a valid printer.", false, failureResult1.IsSuccess);
			AssertEquals("C should not be considered a valid printer.", "Printer not found.", failureResult1.FailureReason);
			AssertEquals("C", failureResult1.PrinterName);

			processor.UpdatePrinters(new[] { "C" });
			job.QueueName = "A";
			var failureResult2 = processor.PrintAsync(job, null).Result;
			AssertEquals("A should not be considered a valid printer.", false, failureResult2.IsSuccess);
			AssertEquals("A should not be considered a valid printer.", "Printer not found.", failureResult2.FailureReason);
			AssertEquals("A", failureResult2.PrinterName);

			job.QueueName = "B";
			var failureResult3 = processor.PrintAsync(job, null).Result;
			AssertEquals("B should not be considered a valid printer.", false, failureResult3.IsSuccess);
			AssertEquals("B should not be considered a valid printer.", "Printer not found.", failureResult3.FailureReason);
			AssertEquals("B", failureResult3.PrinterName);

			job.QueueName = "C";
			AssertEquals("C should be considered a valid printer.", true, processor.PrintAsync(job, null).Result.IsSuccess);
		}

		public void TestPrintQueuesAreCaseInsensitive()
		{
			var processor = new PrintEngineProcessor(new NullPrinterFactory());
			processor.SetPrinter(new SerialisablePrintQueue { Name = "test" });

			var job = new SerialisablePrintJob { JobPk = Guid.NewGuid(), BlobType = "XLS", QueueName = "Test" };
			AssertEquals("Test that queue name is Case Insensitive, can only return result if queue is in dictionary.", true, processor.PrintAsync(job, null).Result.IsSuccess);

			job.QueueName = "TEST";
			AssertEquals("Test that queue name is Case Insensitive, can only return result if queue is in dictionary.", true, processor.PrintAsync(job, null).Result.IsSuccess);
		}

		public void TestPrintWhenJobHasNoBlobType()
		{
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var job = new SerialisablePrintJob { JobPk = Guid.NewGuid(), BlobType = null, QueueName = NullPrintQueueNameForTesting };
			var printResult = processor.PrintAsync(job, null).Result;
			AssertEquals("Without a File Type, the Print should fail.", false, printResult.IsSuccess);
			AssertEquals("Without a File Type, the Print should fail.", "File Type of Print Job unknown.", printResult.FailureReason);
			AssertEquals(NullPrintQueueNameForTesting, printResult.PrinterName);
		}

		public void TestPrint()
		{
			var documentWithScalingSheetXlsPath = DocumentWithScalingSheetXlsPath;
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var printJob = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			printJob.QueueName = NullPrintQueueNameForTesting;

			var printResult1 = processor.PrintAsync(printJob, null).Result;
			AssertEquals("Should have successfully printed.", true, printResult1.IsSuccess);

			var printerJob1 = factory.GetPrinterJob(printJob.JobPk);
			AssertEquals("should have printed 1 document", 1, printerJob1.PrintCount);
			AssertEquals("No trailing escape sequence required", 0, printerJob1.PrintTrailingEscapeSequenceCount);

			var serialisablePrintJob1 = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			serialisablePrintJob1.QueueName = NullPrintQueueNameForTesting;

			var serialisablePrintJob2 = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			serialisablePrintJob2.QueueName = NullPrintQueueNameForTesting;

			var serialisablePrintJob_WithInvalidPrinterName = GetSerialisablePrintJob(DocumentXlsPath);
			serialisablePrintJob_WithInvalidPrinterName.QueueName = "Invalid Print Queue";

			var printResult2 = processor.PrintAsync(serialisablePrintJob1, null).Result;
			var printResult3 = processor.PrintAsync(serialisablePrintJob2, null).Result;
			var printResult4 = processor.PrintAsync(serialisablePrintJob_WithInvalidPrinterName, null).Result;

			var printerJob2 = factory.GetPrinterJob(serialisablePrintJob1.JobPk);
			AssertEquals("Should have successfully printed.", true, printResult2.IsSuccess);
			AssertEquals("Should have printed document.", 1, printerJob2.PrintCount);
			AssertEquals("No trailing escape sequence required", 0, printerJob2.PrintTrailingEscapeSequenceCount);

			var printerJob3 = factory.GetPrinterJob(serialisablePrintJob2.JobPk);
			AssertEquals("Should have successfully printed.", true, printResult3.IsSuccess);
			AssertEquals("Should have printed document.", 1, printerJob3.PrintCount);
			AssertEquals("No trailing escape sequence required", 0, printerJob3.PrintTrailingEscapeSequenceCount);

			AssertNull(factory.GetPrinterJob(serialisablePrintJob_WithInvalidPrinterName.JobPk));
			AssertEquals("Should not have printed.", false, printResult4.IsSuccess);
			AssertEquals("Should not have printed.", "Printer not found.", printResult4.FailureReason);
		}

		public void TestPrint_WithWatermark()
		{
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var printJob = GetSerialisablePrintJob(DocumentWithScalingSheetXlsPath);
			printJob.QueueName = NullPrintQueueNameForTesting;
			printJob.HasWatermark = true;

			var watermark = WatermarkFactory.GetWatermark(new SerialisableWatermark { UseTextWatermark = true, TextWatermark = "POPCORN" });
			var printResult = processor.PrintAsync(printJob, watermark).Result;
			AssertEquals("Should have successfully printed.", true, printResult.IsSuccess);
			AssertEquals("Watermark should be part of the Printer Job.", "POPCORN", factory.GetPrinterJob(printJob.JobPk).PrintJob.Watermark.AsText);
		}

		[ExpectNoExceptions]
		public void TestPrintWithEmptySubject()
		{
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var serialisablePrintJob = GetSerialisablePrintJob(DocumentWithScalingSheetXlsPath);
			serialisablePrintJob.QueueName = NullPrintQueueNameForTesting;
			serialisablePrintJob.EmailSubjectLine = "";

			var printResult = processor.PrintAsync(serialisablePrintJob, null).Result;
			AssertEquals("Should have successfully printed.", true, printResult.IsSuccess);

			var printerJob = factory.GetPrinterJob(serialisablePrintJob.JobPk);
			AssertEquals("should have printed 1 document", 1, printerJob.PrintCount);
			AssertEquals("No trailing escape sequence required", 0, printerJob.PrintTrailingEscapeSequenceCount);
			AssertEquals("EmailSubjectLine should have defaulted a value.", "[NO SUBJECT]", serialisablePrintJob.EmailSubjectLine);
		}

		public void TestPrintWithNoCulture()
		{
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var job = GetSerialisablePrintJob(DocumentWithScalingSheetXlsPath);
			job.QueueName = NullPrintQueueNameForTesting;

			var printResult = processor.PrintAsync(job, null).Result;
			AssertEquals("Should have successfully printed.", true, printResult.IsSuccess);
			AssertEquals("should have printed 1 document", 1, factory.GetPrinterJob(job.JobPk).PrintCount);
		}

		public void TestPrintWithTrailingEscapeSequence()
		{
			var documentWithScalingSheetXlsPath = DocumentWithScalingSheetXlsPath;
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var printJob = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			printJob.QueueName = NullPrintQueueNameForTesting;
			printJob.EscapeSequence = new byte[] { 1, 2, 3 };
			var printResult1 = processor.PrintAsync(printJob, null).Result;
			AssertEquals("Should have successfully printed.", true, printResult1.IsSuccess);

			var printerJob1 = factory.GetPrinterJob(printJob.JobPk);
			AssertEquals("should have printed 1 document", 1, printerJob1.PrintCount);
			AssertEquals("Should have printed a trailing escape sequence", 1, printerJob1.PrintTrailingEscapeSequenceCount);

			var serialisablePrintJob1 = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			serialisablePrintJob1.QueueName = NullPrintQueueNameForTesting;
			serialisablePrintJob1.EscapeSequence = new byte[] { 1, 2, 3 };

			var serialisablePrintJob2 = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			serialisablePrintJob2.QueueName = NullPrintQueueNameForTesting;

			var serialisablePrintJob_WithInvalidPrinterName = GetSerialisablePrintJob(DocumentXlsPath);
			serialisablePrintJob_WithInvalidPrinterName.QueueName = "Invalid Print Queue";

			var printResult2 = processor.PrintAsync(serialisablePrintJob1, null).Result;
			var printResult3 = processor.PrintAsync(serialisablePrintJob2, null).Result;
			var printResult4 = processor.PrintAsync(serialisablePrintJob_WithInvalidPrinterName, null).Result;

			var printerJob2 = factory.GetPrinterJob(serialisablePrintJob1.JobPk);
			AssertEquals("Should have successfully printed.", true, printResult2.IsSuccess);
			AssertEquals("Should have printed document.", 1, printerJob2.PrintCount);
			AssertEquals("Should have printed a trailing escape sequence.", 1, printerJob2.PrintTrailingEscapeSequenceCount);

			var printerJob3 = factory.GetPrinterJob(serialisablePrintJob2.JobPk);
			AssertEquals("Should have successfully printed.", true, printResult3.IsSuccess);
			AssertEquals("Should have printed document.", 1, printerJob3.PrintCount);
			AssertEquals("No trailing escape sequence required", 0, printerJob3.PrintTrailingEscapeSequenceCount);

			AssertNull(factory.GetPrinterJob(serialisablePrintJob_WithInvalidPrinterName.JobPk));
			AssertEquals("Should not have printed.", false, printResult4.IsSuccess);
			AssertEquals("Should not have printed.", "Printer not found.", printResult4.FailureReason);
		}

		// This happens when the print queues have been downloaded and a printer gets deleted (or updated or offline)
		// from the local machine while the jobs are being printed
		public void TestPrintWhenPrintQueueGoesMissing()
		{
			var documentWithScalingSheetXlsPath = DocumentWithScalingSheetXlsPath;
			var queue1 = NullPrintQueueNameForTesting;
			var queue2 = "Queue 2";
			var queue3 = "Queue 3";

			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);
			processor.UpdatePrinters(new[] { queue1, queue2, queue3 });

			var serialisablePrintJob1 = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			serialisablePrintJob1.JobPk = Guid.NewGuid();
			serialisablePrintJob1.QueueName = NullPrintQueueNameForTesting;

			var serialisablePrintJob2 = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			serialisablePrintJob1.JobPk = Guid.NewGuid();
			serialisablePrintJob2.QueueName = NullPrintQueueNameForTesting;

			var serialisablePrintJob_WithMissingPrinter1 = GetSerialisablePrintJob(DocumentXlsPath);
			serialisablePrintJob_WithMissingPrinter1.JobPk = Guid.NewGuid();
			serialisablePrintJob_WithMissingPrinter1.QueueName = queue2;

			var serialisablePrintJob_WithMissingPrinter2 = GetSerialisablePrintJob(DocumentXlsPath);
			serialisablePrintJob_WithMissingPrinter2.JobPk = Guid.NewGuid();
			serialisablePrintJob_WithMissingPrinter2.QueueName = queue3;

			var serialisablePrintJob_WithMissingPrinter3 = GetSerialisablePrintJob(DocumentXlsPath);
			serialisablePrintJob_WithMissingPrinter3.JobPk = Guid.NewGuid();
			serialisablePrintJob_WithMissingPrinter3.QueueName = queue2;

			var printResult1 = processor.PrintAsync(serialisablePrintJob1, null).Result;
			var printResult2 = processor.PrintAsync(serialisablePrintJob2, null).Result;
			var printResult3 = processor.PrintAsync(serialisablePrintJob_WithMissingPrinter1, null).Result;
			var printResult4 = processor.PrintAsync(serialisablePrintJob_WithMissingPrinter2, null).Result;
			var printResult5 = processor.PrintAsync(serialisablePrintJob_WithMissingPrinter3, null).Result;
			var printResults = new[] { printResult1, printResult2, printResult3, printResult4, printResult5 };
			var printResultsWithInvalidPrinter = printResults.Where(r => !r.IsSuccess && r.PrinterName != null).ToArray();
			AssertEquals("Should have printed 2 documents.", 2, printResults.Sum(r => factory.GetPrinterJob(r.JobPK)?.PrintCount ?? 0));
			AssertEquals("Should have printed 3 Jobs that could not print.", 3, printResultsWithInvalidPrinter.Length);

			var pks = printResultsWithInvalidPrinter.Select(r => r.JobPK).ToArray();
			AssertContainsExactElementsInAnyOrder(new[] { serialisablePrintJob_WithMissingPrinter1.JobPk, serialisablePrintJob_WithMissingPrinter2.JobPk, serialisablePrintJob_WithMissingPrinter3.JobPk }, pks);
			AssertEquals("Failure reason should be correct.", 3,
				printResultsWithInvalidPrinter.Count(r => r.FailureReason == "Printer selected is no longer valid or not currently available / on-line."));

			var printers = printResultsWithInvalidPrinter.Select(t => t.PrinterName).Distinct();
			AssertEquals("There should be 2 missing printers", 2, printers.Count());
			AssertContainsExactElementsInAnyOrder(new[] { queue2, queue3 }, printers);
		}

		public void TestPrintWhenUnhandledExceptionIsThrown()
		{
			var processor = new PrintEngineProcessor(new FactoryThatCreatesExceptionPrinter());
			processor.UpdatePrinters(new[] { "Printer that throws exception" });

			var printJob = GetSerialisablePrintJob(DocumentWithScalingSheetXlsPath);
			printJob.QueueName = "Printer that throws exception";

			var fileNameAndPath = printJob.EmailSubjectLine + "." + printJob.BlobType;
			var expectedFileNameAndPathError = Path.Combine(Constants.ErrorDir, Path.GetFileName(fileNameAndPath));

			try
			{
				var printResult = processor.PrintAsync(printJob, null).Result;
				AssertEquals(nameof(printResult.IsSuccess), false, printResult.IsSuccess);
				AssertEquals(nameof(printResult.PrinterName), "Printer that throws exception", printResult.PrinterName);
				AssertEquals(nameof(printResult.FailureReason), "ERROR", printResult.FailureReason);

				var unhandledException = printResult.GetUnhandledException();
				AssertType<InvalidOperationException>(unhandledException);
				AssertEquals("ERROR", unhandledException.Message);
			}
			finally
			{
				DeleteIfExists(fileNameAndPath);
				DeleteIfExists(expectedFileNameAndPathError);
			}
		}

		public void TestVerboseLogging()
		{
			var documentWithScalingSheetXlsPath = DocumentWithScalingSheetXlsPath;
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);

			var messages = new List<string>();
			processor.EnableVerboseLogging = true;
			processor.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);

			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var printJob = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			printJob.QueueName = NullPrintQueueNameForTesting;

			var printResult1 = processor.PrintAsync(printJob, null).Result;
			AssertEquals("Should have successfully printed.", true, printResult1.IsSuccess);

			string expectedMessages = string.Format(
@"Getting printer for job [{0}].
Printer type: {1}.
Printing job [{0}].
Processing and printing
Printed job [{0}].
Disposing printer",
				Path.GetFileNameWithoutExtension(documentWithScalingSheetXlsPath),
				nameof(NullPrinter)
			);
			AssertEquals("Expected logs", expectedMessages, string.Join(Environment.NewLine, messages));
		}

		public void TestNoVerboseLogging()
		{
			var documentWithScalingSheetXlsPath = DocumentWithScalingSheetXlsPath;
			var factory = new MockPrinterFactory();
			var processor = new PrintEngineProcessor(factory);

			var messages = new List<string>();
			processor.EnableVerboseLogging = false;
			processor.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);

			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var printJob = GetSerialisablePrintJob(documentWithScalingSheetXlsPath);
			printJob.QueueName = NullPrintQueueNameForTesting;

			var printResult1 = processor.PrintAsync(printJob, null).Result;
			AssertEquals("Should have successfully printed.", true, printResult1.IsSuccess);

			string expectedMessages = string.Format(
@"Getting printer for job [{0}].
Printing job [{0}].
Printed job [{0}].",
				Path.GetFileNameWithoutExtension(documentWithScalingSheetXlsPath)
			);
			AssertEquals("Expected logs", expectedMessages, string.Join(Environment.NewLine, messages));
		}

		public void TestPrintWithPrintJobHasInvalidBlobType()
		{
			var factory = new PrinterFactory();
			var processor = new PrintEngineProcessor(factory);

			var messages = new List<string>();
			processor.Logged += (sender, logEventArgs) => messages.Add(logEventArgs.Message);
			processor.UpdatePrinters(new[] { NullPrintQueueNameForTesting });

			var serialisableJob = new SerialisablePrintJob();
			serialisableJob.JobPk = Guid.NewGuid();
			serialisableJob.BlobType = "EXE";
			serialisableJob.EmailSubjectLine = "Jerry Test EXE";
			serialisableJob.QueueName = NullPrintQueueNameForTesting;
			var result = processor.Print(serialisableJob, null);

			var expectedMessages = @"Getting printer for job [Jerry Test EXE].
The file type is EXE and cannot be printed.";
			AssertEquals("Print EXE file failed.", false, result.IsSuccess);
			AssertEquals("Expected logs for printing EXE file", expectedMessages, string.Join(Environment.NewLine, messages));

			messages.Clear();
			serialisableJob.EmailSubjectLine = "Jerry Test TXT";
			serialisableJob.BlobType = "TXT";
			result = processor.Print(serialisableJob, null);

			expectedMessages = @"Getting printer for job [Jerry Test TXT].
The file type is TXT and cannot be printed.";
			AssertEquals("Print TXT file failed.", false, result.IsSuccess);
			AssertEquals("Expected logs for printing TXT file", expectedMessages, string.Join(Environment.NewLine, messages));
		}

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

		string documentXlsPath;
		string DocumentXlsPath
		{
			get
			{
				if (string.IsNullOrEmpty(documentXlsPath))
				{
					documentXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.Document.xls", "Document.xls");
				}
				return documentXlsPath;
			}
		}

		string documentWithScalingSheetXlsPath;
		string DocumentWithScalingSheetXlsPath
		{
			get
			{
				if (string.IsNullOrEmpty(documentWithScalingSheetXlsPath))
				{
					documentWithScalingSheetXlsPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.DocumentWithScalingSheet.xls", "DocumentWithScalingSheet.xls");
				}
				return documentWithScalingSheetXlsPath;
			}
		}

		class NullPrinterFactory : IPrinterFactory
		{
			public BasePrinter GetPrinter(PrintEngineJob job)
			{
				return new NullPrinter(job);
			}
		}

		class FactoryThatCreatesExceptionPrinter : IPrinterFactory
		{
			public BasePrinter GetPrinter(PrintEngineJob job) => new ExceptionPrinter(job);

			class ExceptionPrinter : BasePrinter
			{
				public ExceptionPrinter(PrintEngineJob printJob)
					: base(printJob)
				{
				}

				protected override void ProcessAndPrintDocument()
				{
					throw new InvalidOperationException("ERROR");
				}
			}
		}

		class MockPrinterFactory : IPrinterFactory
		{
			public IEnumerable<NullPrinter> PrinterJobs => PrinterJobsInternal;
			public NullPrinter GetPrinterJob(Guid jobPK) => PrinterJobsInternal.SingleOrDefault(p => p.PrintJob.JobPk == jobPK);

			List<NullPrinter> PrinterJobsInternal { get; } = new List<NullPrinter>();

			public BasePrinter GetPrinter(PrintEngineJob job)
			{
				if (job.PrinterName.Contains(NullPrintQueueNameForTesting))
				{
					var printer = new NullPrinter(job);
					PrinterJobsInternal.Add(printer);
					return printer;
				}

				throw new InvalidPrinterException(new PrinterSettings());
			}
		}
	}
}
