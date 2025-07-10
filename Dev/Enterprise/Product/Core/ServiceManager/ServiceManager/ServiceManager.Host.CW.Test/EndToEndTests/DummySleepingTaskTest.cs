using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.DummySleepingService;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	[TestedType(typeof(DummySleepingTask))]
	class DummySleepingTaskTest : ServiceTaskTestCase<DummySleepingTask>
	{
		public void TestRunTask()
		{
			var logs = new ConcurrentBag<string>();
			var loggerMock = new Mock<ILogger>();
			loggerMock
				.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string message) =>
				{
					logs.Add(message);
				});

			using var cancellationTokenSource = new CancellationTokenSource();
			var runnerThread = new Thread((token) =>
			{
				var task = new DummySleepingTask { ServiceLogger = loggerMock.Object };

				task.RunTask((CancellationToken)token);
			});

			try
			{
				runnerThread.Start(cancellationTokenSource.Token);

				while (true)
				{
					Thread.Sleep(10);
					if (logs.Any(x => x.Contains($"Acquired a mutex lock: '{DummySleepingTask.MutexLock}'")))
					{
						break;
					}
				}

				Assert(Mutex.TryOpenExisting(DummySleepingTask.MutexLock, out var mutex));
				var mutexAvailable = mutex?.WaitOne(1);
				mutex?.Dispose();
				AssertEquals($"mutex: '{DummySleepingTask.MutexLock}' has been acquired by other process", false, mutexAvailable);
			}
			finally
			{
				cancellationTokenSource.Cancel();
				runnerThread.Join();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
