using System.Collections.Specialized;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;

namespace Enterprise.DataTransfer.BatchProcessor.Testing
{
	public class BaseLoggedDataBatchProcessTest : BaseLoggedDataBatchProcessTestCase
	{
		public void TestName()
		{
			AssertEquals("Log Walker", Process.HumanReadableName);
		}

		public void TestLogsArePostedToLogger()
		{
			Process.Notifications.Notify(new ErrorNotification(ErrorType.Error, "Error"));
			AssertCollectionContains("Logger should be notified", "\tError: Error", Process.Logger.UserLogStrings);
		}

		public void TestLagTime()
		{
			var mockProcess = new Mock<MockLoggedDataBatchProcess>(new object[] { new LoggingInformation() });
			mockProcess.CallBase = true;
			mockProcess.Protected()
				.Setup<int>("MinutesToLagBy")
				.Returns(5);

			var process = mockProcess.Object;
			process.TestHighWaterMark = ZDateTime.UtcNow;
			Thread.Sleep(1000);
			GenerateTestLogs(1);
			Thread.Sleep(1000);

			process.ExecuteBatch();

			mockProcess.VerifyAll();

			AssertEquals("LogsLastExecute is zero", 0, process.LogsLastExecute.Length);

			mockProcess.Protected()
				.Setup<int>("MinutesToLagBy")
				.Returns(0);

			process = mockProcess.Object;

			process.ExecuteBatch();
			mockProcess.VerifyAll();

			AssertEquals("LogsLastExecute", 1, process.LogsLastExecute.Length);
		}

		public void TestNumberOfHoursInFuture()
		{
			var mockProcess = new Mock<MockLoggedDataBatchProcess>(new object[] { new LoggingInformation() });
			mockProcess.CallBase = true;
			mockProcess.Protected()
				.Setup<int>("NumberOfHoursInFuture")
				.Returns(3);

			var process = mockProcess.Object;
			process.TestHighWaterMark = ZDateTime.UtcNow;
			Thread.Sleep(1000);
			GenerateTestLogs(1);
			Thread.Sleep(1000);

			process.ExecuteBatch();
			mockProcess.VerifyAll();
			AssertEquals("LogsLastExecute", 1, process.LogsLastExecute.Length);
		}

		public void TestCreateFilter()
		{
			Process.TestHighWaterMark = ZDateTime.UtcNow;
			Thread.Sleep(1000);
			GenerateTestLogs(100);
			var logFilter = Process.CreateFilter(ZDateTime.UtcNow);

			var logsReturnedByFilter = (StmALog[])Factory.Load(typeof(StmALog), logFilter);

			AssertEquals(100, logsReturnedByFilter.Length);
			Assert("All log walkers must not have  otherwise they will miss records", !logFilter.IsNoLock);
		}

		public void TestEnsureThatLogsAreProcessedInAscendingOrder()
		{
			Process.TestHighWaterMark = ZDateTime.UtcNow;

			GenerateTestLogs(10);
			Thread.Sleep(1000);
			GenerateTestLogs(10);
			Thread.Sleep(1000);
			GenerateTestLogs(10);
			Thread.Sleep(1000);

			Process.ExecuteBatch();

			StmALog[] logsProcessed = Process.LogsLastExecute;

			StmALog log = null;
			for (int i = 0; i < logsProcessed.Length; i++)
			{
				if (log != null)
				{
					Assert("last processed log was older than the current Log", logsProcessed[i].SL_PostedTimeUtc >= log.SL_PostedTimeUtc);
				}
				log = logsProcessed[i];
			}
		}

		public void TestLogsBeforeWaterMarkAreNotProcessedInBatch()
		{
			StmALog[] logsBeforeHwm = GenerateTestLogs(10);

			Process.TestHighWaterMark = ZDateTime.UtcNow;
			Thread.Sleep(1000);

			StmALog[] logsAfterHwm = GenerateTestLogs(10);
			Process.ExecuteBatch();

			AssertEquals("Logs created after the hwm was set should have been processed", 10, Process.LogsLastExecute.Length);

			foreach (StmALog logAfterHwm in Process.LogsLastExecute)
			{
				foreach (StmALog logBeforeHwm in logsBeforeHwm)
				{
					Assert("Log posted before HWM was processed", logAfterHwm.PK != logBeforeHwm.PK);
				}
			}
		}

		public void TestConsecutiveBatchesDoNotReprocessTheSameLogs()
		{
			Process.TestHighWaterMark = ZDateTime.UtcNow;

			Thread.Sleep(1000);

			GenerateTestLogs(10);
			Process.ExecuteBatch();
			AssertEquals(10, Process.LogsLastExecute.Length);

			StmALog[] firstBatch = Process.LogsLastExecute;

			GenerateTestLogs(10);
			Process.ExecuteBatch();

			AssertEquals(10, Process.LogsLastExecute.Length);

			StmALog[] secondBatch = Process.LogsLastExecute;

			Assert("first batch did not process any logs", firstBatch.Length > 0);
			Assert("second batch did not process any logs", secondBatch.Length > 0);

			foreach (StmALog firstBatchLog in firstBatch)
			{
				foreach (StmALog secondBatchLog in secondBatch)
				{
					Assert("a Log was reprocessed in second batch", firstBatchLog.PK != secondBatchLog.PK);
				}
			}

			AssertEquals("All logs should be processed", 20, firstBatch.Length + secondBatch.Length);
		}

		public void TestHighWaterMarkIsUpdated()
		{
			var expectedMessage = "High water mark has been set to ";
			var mockProcess = new Mock<MockLoggedDataBatchProcess>(new object[] { new LoggingInformation() });
			mockProcess.CallBase = true;
			mockProcess.Protected()
				.Setup<int>("NumberOfLogToUpdateHighWaterMark")
				.Returns(5);

			var process = mockProcess.Object;
			process.TestHighWaterMark = ZDateTime.UtcNow.AddSeconds(-1);

			GenerateTestLogs(2);
			process.ExecuteBatch();

			mockProcess.Protected()
				.Verify<int>("NumberOfLogToUpdateHighWaterMark", Times.Exactly(2));

			mockProcess.VerifyAll();
			AssertEquals("LogsLastExecute length", 2, process.LogsLastExecute.Length);
			AssertLogMessageContain(process.Logger.UserLogStrings, expectedMessage, false);

			Thread.Sleep(1000);

			mockProcess.Protected()
				.Setup<int>("NumberOfLogToUpdateHighWaterMark")
				.Returns(2);

			process = mockProcess.Object;
			GenerateTestLogs(2);

			process.ExecuteBatch();

			mockProcess.Protected()
				.Verify<int>("NumberOfLogToUpdateHighWaterMark", Times.Exactly(4));
			mockProcess.VerifyAll();
			AssertEquals("LogsLastExecute length", 2, process.LogsLastExecute.Length);
		}

		#region Implementation

		MockLoggedDataBatchProcess Process;

		protected override void SetUp()
		{
			base.SetUp();
			Process = new MockLoggedDataBatchProcess(new LoggingInformation());
		}

		void AssertLogMessageContain(StringCollection logMessages, string expectedMessage, bool expectedResult)
		{
			bool foundMessage = false;
			foreach (string logMessage in logMessages)
			{
				if (logMessage.IndexOf(expectedMessage) >= 0)
				{
					foundMessage = true;
					break;
				}
			}
			AssertEquals(expectedResult, foundMessage);
		}

		#region MockLoggedDataBatchProcess

		public class MockLoggedDataBatchProcess : BaseLoggedDataBatchProcess
		{
			public MockLoggedDataBatchProcess(LoggingInformation logger)
				: base(logger)
			{
				TestListeners = System.Array.Empty<ILogBatchListenerProxy>();
				TestHighWaterMark = ZDateTime.UtcNow;
			}

			public new ZQuery CreateFilter(ZDateTime endDate)
			{
				return base.CreateFilter(endDate);
			}

			public ZDateTime TestHighWaterMark;

			public ILogBatchListenerProxy[] TestListeners;

			protected override ZDateTime HighWaterMark
			{
				get { return TestHighWaterMark; }
				set { TestHighWaterMark = value; }
			}

			protected override ILogBatchListenerProxy[] Listeners
			{
				get { return TestListeners; }
			}

			protected override int MinutesToLagBy
			{
				get { return 0; }
			}

			protected override int NumberOfHoursInFuture
			{
				get { return 3; }
			}

			public new INotifications Notifications
			{
				get { return base.Notifications; }
			}
		}

		#endregion

		#endregion
	}
}
