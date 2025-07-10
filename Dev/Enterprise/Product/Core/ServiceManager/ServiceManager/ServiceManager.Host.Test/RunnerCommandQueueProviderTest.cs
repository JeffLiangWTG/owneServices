using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class RunnerCommandQueueProviderTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			hostLoggerMock = new Mock<IHostLogger>();
			cancellationTokenSource = new CancellationTokenSource();
			server = new Server(new[] { new ChannelOption("grpc.keepalive_permit_without_calls", 1), })
			{
				Services = { ServiceRunner.BindService(new GrpcServer((bool b) => exitWasClean = b)) },
				Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) },
			};
			server.Start();
			var port = server.Ports.Single().BoundPort;
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(x =>
				x.ReportDeveloperExceptionOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			provider = new RunnerCommandQueueProvider(hostLoggerMock.Object, port, errorReporterProxyMock.Object);
		}

		protected override void TearDown()
		{
			cancellationTokenSource.Cancel();
			provider?.Dispose();
			server.KillAsync().Wait(5);
			try
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}
			catch (Exception ex)
				when (ex is not TimeoutException)
			{
				// Ignore errors from shutdown of grpc, we're not doing it cleanly for a test
			}
			base.TearDown();
		}

		[ExpectNoExceptions]
		public void TestRunRequest()
		{
			AssertRequest((string code, Guid id) => provider.Run("someAssembly", code, id, "emptyConfig"), "ASD", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.DirectRun)}:ASD");
		}

		[ExpectNoExceptions]
		public void TestScheduledRunRequest()
		{
			AssertRequest((string code, Guid id) => provider.Run("someAssembly", code, id, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig"), "DSA", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.ScheduledRun)}:DSA");
		}

		[ExpectNoExceptions]
		public void TestScheduledRunRequestLocalTimeNextRunTime()
		{
			AssertRequest((string code, Guid id) => provider.Run("someAssembly", code, id, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Local), "emptyConfig"), "DSA", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.ScheduledRun)}:DSA");
			hostLoggerMock.Verify(l => l.Log(LogLevel.Warning, "Next runtime in request for [DSA] had incorrect DateTimeKind: [Local]. Scheduled time may not be as expected"));
			errorReporterProxyMock.Verify(x => x.ReportDeveloperExceptionOnce("Incorrect dateTimeKind on next run time", It.IsAny<Exception>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestScheduledRunRequestLocalTimeExpectedNextRunTime()
		{
			AssertRequest((string code, Guid id) => provider.Run("someAssembly", code, id, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Local), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig"), "DSA", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.ScheduledRun)}:DSA");
			hostLoggerMock.Verify(l => l.Log(LogLevel.Warning, "Expected next runtime in request for [DSA] had incorrect DateTimeKind: [Local]. Scheduled time may not be as expected"));
			errorReporterProxyMock.Verify(x => x.ReportDeveloperExceptionOnce("Incorrect dateTimeKind on expected next run time", It.IsAny<Exception>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestScheduledRunRequestUnspecifiedTimeNextRunTime()
		{
			AssertRequest((string code, Guid id) => provider.Run("someAssembly", code, id, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Unspecified), "emptyConfig"), "DSA", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.ScheduledRun)}:DSA");
			hostLoggerMock.Verify(l => l.Log(LogLevel.Warning, $"Next runtime in request for [DSA] had incorrect DateTimeKind: [Unspecified]. Scheduled time may not be as expected"));
			errorReporterProxyMock.Verify(x => x.ReportDeveloperExceptionOnce("Incorrect dateTimeKind on next run time", It.IsAny<Exception>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestScheduledRunRequestUnspecifiedTimeExpectedNextRunTime()
		{
			AssertRequest((string code, Guid id) => provider.Run("someAssembly", code, id, new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Unspecified), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig"), "DSA", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.ScheduledRun)}:DSA");
			hostLoggerMock.Verify(l => l.Log(LogLevel.Warning, $"Expected next runtime in request for [DSA] had incorrect DateTimeKind: [Unspecified]. Scheduled time may not be as expected"));
			errorReporterProxyMock.Verify(x => x.ReportDeveloperExceptionOnce("Incorrect dateTimeKind on expected next run time", It.IsAny<Exception>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestStopRequest()
		{
			AssertRequest((string code, Guid id) => provider.Stop(), "EWQ", $"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.Stop)}:");
		}

		[ExpectNoExceptions]
		void AssertRequest(Action<string, Guid> requestAction, string code, string expectedResult)
		{
			// Arrange
			ServiceTaskRunResponse result = null;
			provider.StartResponseTask((response) => result = response, cancellationTokenSource.Token);

			// Act
			requestAction(code, Guid.NewGuid());
			Task.Delay(TimeSpan.FromSeconds(5)).Wait();

			// Assert
			NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(ServiceTaskRunResponse)));
			NUnit.Framework.Assert.That(result.TaskCode, Is.EqualTo(expectedResult));
		}

		[ExpectNoExceptions]
		public void TestClose()
		{
			// Arrange
			ServiceTaskRunResponse result = null;
			provider.StartResponseTask((response) => result = response, cancellationTokenSource.Token);

			// Act
			provider.Close();
			Task.Delay(TimeSpan.FromSeconds(5)).Wait();

			// Assert
			NUnit.Framework.Assert.That(exitWasClean, Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestCloseWithMessagesInProcess()
		{
			// Arrange
			ServiceTaskRunResponse result = null;
			provider.StartResponseTask((response) =>
			{
				Task.Delay(TimeSpan.FromSeconds(1)).Wait();
				result = response;
			}, cancellationTokenSource.Token);

			// Act
			provider.Run("someAssembly", "DSA", Guid.NewGuid(), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig");
			provider.Run("someAssembly", "DSA", Guid.NewGuid(), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig");
			provider.Run("someAssembly", "DSA", Guid.NewGuid(), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig");
			provider.Close();
			provider.Dispose();
			provider = null;

			// Assert
			NUnit.Framework.Assert.That(exitWasClean, Is.EqualTo(true));
		}

		static readonly Action<RunnerCommandQueueProvider>[] sendRequestActions = new Action<RunnerCommandQueueProvider>[]
		{
			(provider) => provider.Run("someAssembly", "ASD", Guid.NewGuid(), "emptyConfig"),
			(provider) => provider.Run("someAssembly", "DSA", Guid.NewGuid(), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "emptyConfig"),
			(provider) => provider.Stop(),
		};

		public void TestSendRequestFromTwoThreads_SendThenClose()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				// test calls between SendRequest and Close()
				foreach (var sendRequestAction in sendRequestActions)
				{
					AssertFromTwoThreads_SendThenClose(sendRequestAction);
				}
			});

			void AssertFromTwoThreads_SendThenClose(Action<RunnerCommandQueueProvider> sendRequestTaskAction)
			{
				// Arrange
				var pendingRequests = 0;

				var syncEvent = new ManualResetEvent(false);
				var syncEvent2 = new ManualResetEvent(false);
				var syncEvent3 = new ManualResetEvent(false);

				var serviceRunnerClientWrapper = new Mock<IServiceRunnerClientWrapper>();
				serviceRunnerClientWrapper
					.Setup(x => x.SendRequest(It.IsAny<ServiceTaskRunRequest>()))
					.Callback(() =>
					{
						if (pendingRequests != 0)
						{
							throw new InvalidOperationException("Only one write can be pending at a time.");
						}

						pendingRequests++;
						syncEvent.Set();
						syncEvent2.WaitOne();
						pendingRequests--;
					});
				serviceRunnerClientWrapper
					.Setup(x => x.Close())
					.Callback(() =>
					{
						syncEvent3.Set();

						if (pendingRequests != 0)
						{
							throw new InvalidOperationException("Only one write can be pending at a time.");
						}
					});

				// Act
				using var newProvider = new RunnerCommandQueueProvider(hostLoggerMock.Object, serviceRunnerClientWrapper.Object, new Mock<IErrorReporterProxy>().Object);
				var sendRequestTask = Task.Run(() =>
				{
					sendRequestTaskAction.Invoke(newProvider);
				});
				var closeTask = Task.Run(() =>
				{
					syncEvent.WaitOne();
					newProvider.Close();
				});

				syncEvent3.WaitOne(TimeSpan.FromSeconds(3));
				syncEvent2.Set();

				// Assert
				AssertNoExceptionThrown(() => Task.WaitAll(sendRequestTask, closeTask));
			}
		}

		public void TestSendRequestFromTwoThreads_CloseThenSend()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				// test calls between SendRequest and Close()
				foreach (var sendRequestAction in sendRequestActions)
				{
					AssertFromTwoThreads_CloseThenSend(sendRequestAction);
				}
			});

			void AssertFromTwoThreads_CloseThenSend(Action<RunnerCommandQueueProvider> sendRequestTaskAction)
			{
				// Arrange
				var pendingRequests = 0;

				var syncEvent = new ManualResetEvent(false);
				var syncEvent2 = new ManualResetEvent(false);
				var syncEvent3 = new ManualResetEvent(false);

				var serviceRunnerClientWrapper = new Mock<IServiceRunnerClientWrapper>();
				serviceRunnerClientWrapper
					.Setup(x => x.Close())
					.Callback(() =>
					{
						if (pendingRequests != 0)
						{
							throw new InvalidOperationException("Only one write can be pending at a time.");
						}

						pendingRequests++;
						syncEvent.Set();
						syncEvent2.WaitOne();
						pendingRequests--;
					});
				serviceRunnerClientWrapper
					.Setup(x => x.SendRequest(It.IsAny<ServiceTaskRunRequest>()))
					.Callback(() =>
					{
						syncEvent3.Set();

						if (pendingRequests != 0)
						{
							throw new InvalidOperationException("Only one write can be pending at a time.");
						}
					});

				// Act
				using var newProvider = new RunnerCommandQueueProvider(hostLoggerMock.Object, serviceRunnerClientWrapper.Object, new Mock<IErrorReporterProxy>().Object);
				var closeTask = Task.Run(() =>
				{
					newProvider.Close();
				});
				var sendRequestTask = Task.Run(() =>
				{
					syncEvent.WaitOne();
					sendRequestTaskAction.Invoke(newProvider);
				});

				syncEvent3.WaitOne(TimeSpan.FromSeconds(3));
				syncEvent2.Set();

				// Assert
				AssertNoExceptionThrown(() => Task.WaitAll(closeTask, sendRequestTask));
			}
		}

		Mock<IHostLogger> hostLoggerMock;
		CancellationTokenSource cancellationTokenSource;
		RunnerCommandQueueProvider provider;
		bool exitWasClean;
		Server server;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
	}

	class GrpcServer : ServiceRunner.ServiceRunnerBase
	{
		public GrpcServer(Action<bool> wasExitCleanAction)
		{
			this.wasExitCleanAction = wasExitCleanAction;
		}

		public override async Task RunServiceTaskAsync(IAsyncStreamReader<ServiceTaskRunRequest> requestStream, IServerStreamWriter<ServiceTaskRunResponse> responseStream, ServerCallContext context)
		{
			try
			{
				while (await requestStream.MoveNext(context.CancellationToken))
				{
					var request = requestStream.Current;
					await responseStream.WriteAsync(new ServiceTaskRunResponse()
					{
						TaskCode = $"{Enum.GetName(typeof(RequestCommandType), request.Command)}:{request.Code}",
					});
				}
			}
			catch (Exception)
			{
				wasExitCleanAction(false);
			}

			wasExitCleanAction(true);
		}

		readonly Action<bool> wasExitCleanAction;
	}
}
