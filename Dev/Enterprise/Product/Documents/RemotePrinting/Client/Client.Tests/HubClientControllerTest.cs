using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Enterprise.RemotePrinting.Engine;
using Enterprise.RemotePrinting.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class HubClientControllerTest : TestCase
	{
		public void TestRegisterClientForReconnecting()
		{
			var processor = new MockPrintProcessor();
			var updater = new Mock<IUpdateProcessor>();
			var controller = new HubClientController("Borris", new PrintManager(processor), _ => updater.Object, processor, null);
			var logs = LogToStringBuilder(controller);
			var server = new Mock<IRemoteServer>();
			controller.Start(server.Object);
			server.Verify(s => s.Initialise(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Once);

			((IRemoteClient)controller).RegisterClientForReconnecting();
			server.Verify(s => s.Initialise(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()), Times.Exactly(2));

			var expectedLog = @"Configuring connection to hub
RegisterClientForReconnecting received.
Register client for reconnecting.
";
			AssertEquals(expectedLog, logs.ToString());
		}

		public void TestErrorLogsWhenPrintOccursAggregateException()
		{
			var processor = new MockPrintProcessorForTest();
			var updater = new Mock<IUpdateProcessor>();
			var controller = new HubClientController("Borris", new PrintManager(processor), _ => updater.Object, processor, null);
			var logs = LogToStringBuilder(controller);
			processor.Log(logs.ToString());
			((IRemoteClient)controller).Print(GetDummyPrintJob());

			var expectedLog = "Error when print document: Connection was disconnected before invocation result was received";
			AssertContains(expectedLog, logs.ToString());
		}

		public void TestWatermarkReceived()
		{
			var processor = new MockPrintProcessor();
			var updater = new Mock<IUpdateProcessor>();
			updater.Setup(m => m.IsUpdateRequired()).Returns(false);

			var controller = new HubClientController("Borris", new PrintManager(processor), _ => updater.Object, processor, null);
			var logs = LogToStringBuilder(controller);

			controller.Start(new Mock<IRemoteServer>().Object);
			((IRemoteClient)controller).SetWatermark(new SerialisableWatermark { UseTextWatermark = true, TextWatermark = "POPCORN" });
			((IRemoteClient)controller).Print(GetDummyPrintJob());

			AssertEquals("The print job's watermark should match the text value from SetWatermark", "POPCORN", processor.LastWatermark?.AsText);
		}

		public void TestIncorrectUpdateReceived()
		{
			using (UpdateProcessor.OverrideInstalledVersionForTest("2.16.6"))
			{
				var processor = new MockPrintProcessor();
				processor.MockInstalledQueues.AddRange(new[] { new PrinterInfo("First", true, false), new PrinterInfo("Second", true, false) });

				var controller = new HubClientController("Borris", new PrintManager(processor), _ => new DummyUpdateProcessor(false), processor, null);
				var logs = LogToStringBuilder(controller);

				controller.Start(new Mock<IRemoteServer>().Object);

				var updateEventFired = 0;
				controller.Updated += (o, e) => updateEventFired++;
				((IRemoteClient)controller).Update("6.9", "http://bogus.com");

				AssertEquals(0, updateEventFired);

				var expectedLog = string.Format("Was requested to update to {0}, but is already on {1}. Update will be ignored.", "6.9", UpdateProcessor.GetInstalledVersion());
				AssertContains(expectedLog, logs.ToString());
			}
		}

		public void TestUpdateReceived()
		{
			var processor = new MockPrintProcessor();
			processor.MockInstalledQueues.AddRange(new[] { new PrinterInfo("First", true, false), new PrinterInfo("Second", true, false) });

			var controller = new HubClientController("Borris", new PrintManager(processor), _ => new DummyUpdateProcessor(true), processor, null);
			var logs = LogToStringBuilder(controller);

			controller.Start(new Mock<IRemoteServer>().Object);

			var updateEventFired = 0;
			controller.Updated += (o, e) => updateEventFired++;
			((IRemoteClient)controller).Update("6.9", "http://bogus.com");

			AssertEquals(1, updateEventFired);
		}

		public void TestStartInitialisesOnHub()
		{
			using (UpdateProcessor.OverrideInstalledVersionForTest("2.16.6"))
			{
				var currentVersion = UpdateProcessor.GetInstalledVersion();

				var server = new Mock<IRemoteServer>();
				server.Setup(s => s.Initialise("Borris", currentVersion, new[] { "First", "Second" }));

				var manager = new PrintManagerForTesting();
				manager.MockInstalledQueues.Clear();
				manager.AddInstalledMockQueues("First", "Second");

				var controller = new HubClientController("Borris", manager, _ => new DummyUpdateProcessor(false), manager.ProcessorForTesting, null);
				var logs = LogToStringBuilder(controller);

				controller.Start(server.Object);

				server.VerifyAll();
				AssertContains("Configuring connection to hub", logs.ToString());
			}
		}

		public void TestPrintReturnsStatus()
		{
			var logs = PrintAndVerifyResponse(new PrintManagerForTesting(), Guid.NewGuid(), ProcessedStatus.Processed, string.Empty);
			AssertContains("Returning success to server", logs);
		}

		public void TestExceptionFailureReturnsFailed()
		{
			var jobPk = Guid.NewGuid();

			var processor = new PrintManagerForTesting();
			processor.JobsToThrow.Add(jobPk);

			var logs = PrintAndVerifyResponse(processor, jobPk, ProcessedStatus.Failed, "Failure Mock");
			AssertContains("The document failed to print.", logs);
		}

		public void TestFailureReturnsReason()
		{
			var jobPk = Guid.NewGuid();

			var processor = new PrintManagerForTesting();
			processor.JobsToFailWithError.Add(jobPk);

			var logs = PrintAndVerifyResponse(processor, jobPk, ProcessedStatus.Failed, "Failure Test");
			AssertContains("The document failed to print.", logs);
			AssertContains("Failure Test", logs);
		}

		public void TestFailureWithoutMessage()
		{
			var jobPk = Guid.NewGuid();

			var processor = new PrintManagerForTesting();
			processor.JobsToFailWithError.Add(jobPk);

			var logs = PrintAndVerifyResponse(processor, jobPk, ProcessedStatus.Failed, "Failure Test");
			AssertContains("The document failed to print.", logs);
		}

		string PrintAndVerifyResponse(PrintManagerForTesting printManager, Guid jobPk, ProcessedStatus expectedStatus, string expectedMessage)
		{
			var client = new HubClientController("whatevs", printManager, _ => new DummyUpdateProcessor(), printManager.ProcessorForTesting, new WebClient(new RemotePrintingServiceAdaptor(new WebClientConfiguration())));

			var logs = LogToStringBuilder(client);
			var server = new Mock<IRemoteServer>();
			server.Setup(c => c.SetPrintStatus(jobPk, expectedStatus, expectedMessage));
			client.Start(server.Object);

			((IRemoteClient)client).Print(GetDummyPrintJob(jobPk));
			server.Verify(c => c.SetPrintStatus(jobPk, expectedStatus, expectedMessage), Times.Once());

			return logs.ToString();
		}

		static StringBuilder LogToStringBuilder(HubClientController controller)
		{
			var sb = new StringBuilder();
			controller.Logged += (o, e) => sb.AppendLine(e.Message);
			return sb;
		}

		SerialisablePrintJob GetDummyPrintJob(Guid? jobPk = null, string printer = MockPrintProcessor.DefaultInstalledPrinter)
			=> new SerialisablePrintJob { JobPk = jobPk ?? Guid.NewGuid(), BlobType = "XLS", QueueName = printer };
	}

	class DummyUpdateProcessor : IUpdateProcessor
	{
		readonly bool updateRequired;

		public event EventHandler Updated;

		public DummyUpdateProcessor(bool updateRequired = false)
			=> this.updateRequired = updateRequired;

		public bool IsUpdateRequired() => updateRequired;

		public void Process()
		{
			if (!updateRequired)
			{
				Assertion.Fail("Don't try to update unless you need to.");
			}

			Updated?.Invoke(this, EventArgs.Empty);
		}
	}

	class MockPrintProcessorForTest : IPrintEngineProcessor, INudgeable
	{
		public bool EnableVerboseLogging { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public event EventHandler<LogEventArgs> Logged;

		public bool HasQueueChanged(string queueName, Guid queueStateChangedStamp)
		{
			throw new NotImplementedException();
		}

		public void Nudge()
		{
			throw new NotImplementedException();
		}

		public PrintResult Print(SerialisablePrintJob jobToPrint, Watermark watermark)
		{
			// HubClientController uses PrintAsync
			throw new NotImplementedException();
		}

		public Task<PrintResult> PrintAsync(SerialisablePrintJob jobToPrint, Watermark watermark)
		{
			var tcs = new TaskCompletionSource<PrintResult>();
			Task.Run(() =>
			{
				tcs.TrySetException(new InvalidOperationException("Connection was disconnected before invocation result was received"));
			});
			return tcs.Task;
		}

		public void SetPrinter(SerialisablePrintQueue changedQueue)
		{
			throw new NotImplementedException();
		}

		public void UpdatePrinters(IEnumerable<string> installedPrinters)
		{
			throw new NotImplementedException();
		}

		public void Log(string log)
		{
			Logged?.Invoke(this, new LogEventArgs(log));
		}
	}
}
