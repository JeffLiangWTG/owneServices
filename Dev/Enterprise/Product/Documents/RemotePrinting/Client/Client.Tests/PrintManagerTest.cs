using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	class PrintManagerTest : TestCase
	{
		public void TestDuplicatePrinterNames()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting("PRN-1", "PRN-2", "PRN-1"))
			{
				var mock = new MockPrintProcessor();
				var printers = new PrintManager(mock).InitialisePrintQueuesAndReturnQueueList();
				AssertContainsExactElementsInAnyOrder(new[] { "PRN-1", "PRN-2" }, printers.Select(printer => printer.Name));
				AssertContainsExactElementsInAnyOrder(new[] { "PRN-1", "PRN-2" }, mock.LastInstalledPrintersForUpdatePrinters);
			}
		}

		public void TestSetPrinter()
		{
			var mock = new MockPrintProcessor();
			var manager = new PrintManager(mock);
			AssertEquals("Precondition:", 0, mock.ChangedQueuesForSetPrinter.Count);

			var printQueue1 = new SerialisablePrintQueue();
			var printQueue2 = new SerialisablePrintQueue();
			manager.UpdatePrintQueues(new[] { printQueue1, printQueue2 });
			AssertContainsExactElementsInAnyOrder(new[] { printQueue1, printQueue2 }, mock.ChangedQueuesForSetPrinter);
		}

		public void TestInitialisePrintQueuesAndReturnQueueList()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting(
				new PrinterInfo("Queue1", true, false),
				new PrinterInfo("Queue2", true, true),
				new PrinterInfo("Queue3", false, false)
			))
			{
				var mock = new MockPrintProcessor();
				var printers = new PrintManager(mock).InitialisePrintQueuesAndReturnQueueList();

				AssertQueue(printers, "Queue1", true, true, false);
				AssertQueue(printers, "Queue2", true, true, true);
				AssertQueue(printers, "Queue3", false);
			}
		}

		void AssertQueue(IEnumerable<PrinterInfo> printers, string printerName, bool expectIncluded, bool expectIsValid = true, bool expectIsSuspectedSurrogate = false)
		{
			var queue = printers.FirstOrDefault(printer => printer.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase));
			if (expectIncluded)
			{
				AssertNotNull($"Printer {printerName} should not be included", queue);
				AssertEquals($"{printerName}.IsValid", expectIsValid, queue.IsValid);
				AssertEquals($"{printerName}.IsSuspectedSurrogate", expectIsSuspectedSurrogate, queue.IsSuspectedSurrogate);
			}
			else
			{
				AssertNull($"Printer {printerName} should not be included", queue);
			}
		}

		public void TestPrintJobsWithTimeout()
		{
			var jobPk1 = Guid.NewGuid();
			var jobPkToTimeout = Guid.NewGuid();
			var jobPk3 = Guid.NewGuid();

			const int TimeoutSeconds = 3;
			const int ExtraDelaySeconds = 2;

			var manager = new PrintManagerForTesting();
			manager.JobPrintingTimeout = TimeSpan.FromSeconds(TimeoutSeconds);
			manager.ProcessorForTesting.Timeout = TimeSpan.FromSeconds(TimeoutSeconds + ExtraDelaySeconds);
			manager.ProcessorForTesting.JobsToTimeout.Add(jobPkToTimeout);
			try
			{
				var printResults = manager
				.Print(
					new[]
					{
						new SerialisablePrintJob { JobPk = jobPk1, QueueName = MockPrintProcessor.DefaultInstalledPrinter },
						new SerialisablePrintJob { JobPk = jobPkToTimeout, QueueName = MockPrintProcessor.DefaultInstalledPrinter },
						new SerialisablePrintJob { JobPk = jobPk3, QueueName = MockPrintProcessor.DefaultInstalledPrinter },
					},
					null)
				.ToList();

				AssertEquals(3, printResults.Count);

				AssertEquals(jobPk1, printResults[0].JobPK);
				AssertEquals(true, printResults[0].IsSuccess);

				AssertEquals(jobPkToTimeout, printResults[1].JobPK);
				AssertEquals(false, printResults[1].IsSuccess);
				AssertEquals("Printing did not finish after timeout of 0 minutes.", printResults[1].FailureReason); // We are testing with just 3 seconds timeout, so it is 0 minutes

				AssertEquals(jobPk3, printResults[2].JobPK);
				AssertEquals(true, printResults[2].IsSuccess);
			}
			finally
			{
				manager.ProcessorForTesting.PrintAsyncTasks.ForEach(x =>
				{
					if (!x.IsCompleted)
					{
						x.Wait();
					}
				}); // Wait for print job with delay task to finish
			}
		}
	}

	class MockPrintProcessor : IPrintEngineProcessor, INudgeable
	{
		public const string DefaultInstalledPrinter = "DefaultInstalledPrinter";

		public IEnumerable<string> MockInstalledQueueNames => MockInstalledQueues.Select(queue => queue.Name);

		public List<PrinterInfo> MockInstalledQueues { get; } = new List<PrinterInfo> { new PrinterInfo(DefaultInstalledPrinter, true, false) };

		public Watermark LastWatermark { get; private set; }

		public event EventHandler<LogEventArgs> Logged;

		public bool HasQueueChanged(string queueName, Guid queueStateChangedStamp) => false;

		public void SetPrinter(SerialisablePrintQueue changedQueue)
		{
			ChangedQueuesForSetPrinter.Add(changedQueue);
		}

		public List<SerialisablePrintQueue> ChangedQueuesForSetPrinter { get; } = new List<SerialisablePrintQueue>();

		public void UpdatePrinters(IEnumerable<string> installedPrinters)
		{
			LastInstalledPrintersForUpdatePrinters = installedPrinters;
		}

		public IEnumerable<string> LastInstalledPrintersForUpdatePrinters { get; private set; }

		public PrintResult Print(SerialisablePrintJob jobToPrint, Watermark watermark)
		{
			LastWatermark = watermark;
			PrintResult result;

			Log($"Printing job [{jobToPrint.EmailSubjectLine}]");

			if (MockInstalledQueueNames.Contains(jobToPrint.QueueName))
			{
				jobToPrint.Copies++;
			}

			if (JobsToThrow.Contains(jobToPrint.JobPk))
			{
				result = PrintResult.WithUnhandledException(jobToPrint.JobPk, "Printer Mock", "Failure Mock", new InvalidOperationException("Ooops"));
			}
			else if (JobsToFailWithError.Contains(jobToPrint.JobPk))
			{
				result = PrintResult.WithFailureReason(jobToPrint.JobPk, jobToPrint.QueueName, "Failure Test");
			}
			else if (JobsToFail.Contains(jobToPrint.JobPk))
			{
				result = PrintResult.Fail(jobToPrint.JobPk);
			}
			else if (JobsToTimeout.Contains(jobToPrint.JobPk))
			{
				Thread.Sleep(Timeout);
				result = PrintResult.WithFailureReason(jobToPrint.JobPk, jobToPrint.QueueName, "Hardcoded timeout reached.");
			}
			else
			{
				result = MockInstalledQueueNames.Contains(jobToPrint.QueueName)
					? PrintResult.Success(jobToPrint.JobPk)
					: PrintResult.WithFailureReason(jobToPrint.JobPk, jobToPrint.QueueName, "WHERE IS THE PRINTER?!");
			}

			Log($"Printed job [{jobToPrint.EmailSubjectLine}]");

			return result;
		}

		public async Task<PrintResult> PrintAsync(SerialisablePrintJob jobToPrint, Watermark watermark)
		{
			var task = Task.Run(() => Print(jobToPrint, watermark));
			PrintAsyncTasks.Add(task);
			return await task.ConfigureAwait(false);
		}

		void Log(string log)
		{
			Logged?.Invoke(this, new LogEventArgs(log));
		}

		public void Nudge()
		{
			throw new NotImplementedException();
		}

		public ICollection<Guid> JobsToThrow { get; } = new HashSet<Guid>();
		public ICollection<Guid> JobsToFail { get; } = new HashSet<Guid>();
		public ICollection<Guid> JobsToFailWithError { get; } = new HashSet<Guid>();
		public ICollection<Guid> JobsToTimeout { get; } = new HashSet<Guid>();
		public TimeSpan Timeout { get; set; } = TimeSpan.Zero;
		public List<Task> PrintAsyncTasks { get; } = new List<Task>();

		public bool EnableVerboseLogging { get; set; }
	}

	class PrintManagerForTesting : PrintManager
	{
		public PrintManagerForTesting()
			: this(new MockPrintProcessor())
		{
		}

		PrintManagerForTesting(MockPrintProcessor processor)
			: base(processor)
		{
			ProcessorForTesting = processor;
		}

		public MockPrintProcessor ProcessorForTesting { get; }

		public ICollection<Guid> JobsToThrow => ProcessorForTesting.JobsToThrow;
		public ICollection<Guid> JobsToFail => ProcessorForTesting.JobsToFail;
		public ICollection<Guid> JobsToFailWithError => ProcessorForTesting.JobsToFailWithError;
		public ICollection<Guid> JobsToTimeout => ProcessorForTesting.JobsToTimeout;

		protected override IEnumerable<string> GetInstalledPrinterNames() => GetInstalledPrinters().Select(queue => queue.Name);

		protected override IEnumerable<PrinterInfo> GetInstalledPrinters()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting(MockInstalledQueues))
			{
				return base.GetInstalledPrinters();
			}
		}

		public List<PrinterInfo> MockInstalledQueues => ProcessorForTesting.MockInstalledQueues;

		public void AddInstalledMockQueues(params string[] queueNames)
		{
			foreach (var queueName in queueNames)
			{
				MockInstalledQueues.Add(new PrinterInfo(queueName, true, false));
			}
		}
	}
}
