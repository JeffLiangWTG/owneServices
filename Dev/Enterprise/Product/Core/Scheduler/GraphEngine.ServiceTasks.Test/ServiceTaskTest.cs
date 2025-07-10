using System;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.GraphEngine.ServiceTasks.Testing
{
	class ServiceTaskTest : TestCaseWithFactory
	{
		public void TestLogExceptionAndThrow()
		{
			var logger = new DetailedLoggerForTest();
			using (var processingManager = new ProcessingManagerForTesting
			{
				Logger = new LoggingInformation(),
				ExecuteBatchForTesting = (x) => throw new InvalidOperationException("Exception Message")
			})
			{
				var serviceTask = new ServiceTaskTestClass { ProcessingManagerForTesting = processingManager, ServiceLogger = logger };
				AssertExceptionThrown<InvalidOperationException>("Handle Exception", "Exception Message", () => serviceTask.RunTask(CancellationToken.None));
				AssertMultilineASCIIEquals("Logs", "Information-[Service Task Shutdown. Reason: Exception Message\r\n]-[]\r\n", GetLogAsString(logger));
			}
		}

		public void TestLogTransactionException()
		{
			var logger = new DetailedLoggerForTest();
			using (var processingManager = new ProcessingManagerForTesting
			{
				Logger = new LoggingInformation(),
				ExecuteBatchForTesting = (x) => throw new TransactionException("Exception Message")
			})
			{
				var serviceTask = new ServiceTaskTestClass { ProcessingManagerForTesting = processingManager, ServiceLogger = logger };
				serviceTask.RunTask(CancellationToken.None);
				AssertMultilineASCIIEquals("Logs", "Information-[Service Task Shutdown. Reason: Exception Message\r\n]-[]\r\n", GetLogAsString(logger));
			}
		}

		public void TestLogInnerTransactionException()
		{
			var logger = new DetailedLoggerForTest();
			using (var processingManager = new ProcessingManagerForTesting
			{
				Logger = new LoggingInformation(),
				ExecuteBatchForTesting = (x) => throw new ApplicationException("App Exception", new TransactionException("Exception Message"))
			})
			{
				var serviceTask = new ServiceTaskTestClass { ProcessingManagerForTesting = processingManager, ServiceLogger = logger };
				serviceTask.RunTask(CancellationToken.None);
				AssertMultilineASCIIEquals("Logs", "Information-[Service Task Shutdown. Reason: App Exception\r\nException Message\r\n]-[]\r\n", GetLogAsString(logger));
			}
		}

		public void TestLogProcessingManagerLogs()
		{
			var logger = new DetailedLoggerForTest();
			using (var processingManager = new ProcessingManagerForTesting
			{
				Logger = new LoggingInformation(),
			})
			{
				processingManager.ExecuteBatchForTesting = (x) =>
				{
					processingManager.Logger.Log(LogType.Debug, "Message1");
					processingManager.Logger.Log(LogType.Error, "Message2");
					processingManager.Logger.Log(LogType.Information, "Message3");
				};
				var serviceTask = new ServiceTaskTestClass { ProcessingManagerForTesting = processingManager, ServiceLogger = logger };
				serviceTask.RunTask(CancellationToken.None);
				AssertMultilineASCIIEquals("Logs", "Debug-[Message1]-[]\r\nError-[Message2]-[]\r\nInformation-[Message3]-[]\r\n", GetLogAsString(logger));
			}
		}

		string GetLogAsString(DetailedLoggerForTest logger) => string.Join("\r\n", logger.Logs.Select(x =>
		{
			var innerException = x.Item3;
			var additionalMessage = innerException == null ? string.Empty : innerException.Message;
			return $"{x.Item1}-[{x.Item2}]-[{additionalMessage}]";
		}));
	}

	class ServiceTaskTestClass : ServiceTask
	{
		public IProcessingManager ProcessingManagerForTesting;
		protected override void RunCore(CancellationToken token)
		{
			RunWithLogger(token, ProcessingManagerForTesting);
		}
	}
}
