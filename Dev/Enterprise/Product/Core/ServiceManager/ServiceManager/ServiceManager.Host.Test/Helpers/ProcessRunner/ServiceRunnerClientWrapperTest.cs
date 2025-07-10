using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NUnit.Framework;
using ServiceManagerProto;
using Enum = System.Enum;

namespace Enterprise.ServiceManager.Host.Testing
{
	public class ServiceRunnerClientWrapperTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			cancellationTokenSource = new CancellationTokenSource();
			server = new Server(new[] { new ChannelOption("grpc.keepalive_permit_without_calls", 1), })
			{
				Services = { ServiceRunner.BindService(new GrpcServer((bool b) => exitWasClean = b)) },
				Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) },
			};
			server.Start();
			var port = server.Ports.Single().BoundPort;
			wrapper = new ServiceRunnerClientWrapper(port);
		}

		protected override void TearDown()
		{
			cancellationTokenSource.Cancel();
			wrapper?.Dispose();
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
		public void TestSendRequest_RunRequest()
		{
			AssertRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.DirectRun,
				AssemblyName = "someAssembly",
				Code = "DSA",
				GuidId = Guid.NewGuid().ToString(),
				ConfigString = "emptyConfig"
			});
		}

		[ExpectNoExceptions]
		public void TestSendRequest_ScheduledRunRequest()
		{
			AssertRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.ScheduledRun,
				AssemblyName = "someAssembly",
				Code = "DSA",
				GuidId = Guid.NewGuid().ToString(),
				NextRunTime = Timestamp.FromDateTime(new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToUniversalTime()),
				ExpectedNextRunTime = Timestamp.FromDateTime(new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc).ToUniversalTime()),
				ConfigString = "emptyConfig",
			});
		}

		[ExpectNoExceptions]
		public void TestSendRequest_StopRequest()
		{
			AssertRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.Stop,
			});
		}

		void AssertRequest(ServiceTaskRunRequest request)
		{
			// Arrange
			var expectedResult = $"{Enum.GetName(typeof(RequestCommandType), request.Command)}:{request.Code}";

			// Act
			var task = Task.Run(() => wrapper.SendRequest(request));
			task.Wait();

			// Assert
			NextResponse(cancellationTokenSource.Token);
			var response = wrapper.CurrentResponse;

			NUnit.Framework.Assert.That(response, Is.Not.EqualTo(default(ServiceTaskRunResponse)));
			NUnit.Framework.Assert.That(response.TaskCode, Is.EqualTo(expectedResult));
		}

		public void TestSendRequest_AfterClose()
		{
			// Arrange
			wrapper.Close();

			// Act and Assert
			AssertExceptionThrown<HostGrpcIsClosedException>(() =>
			{
				wrapper.SendRequest(new ServiceTaskRunRequest
				{
					Command = RequestCommandType.DirectRun,
					AssemblyName = "someAssembly",
					Code = "DSA",
					GuidId = Guid.NewGuid().ToString(),
					ConfigString = "emptyConfig"
				});
			});
		}

		[ExpectNoExceptions]
		public void TestNextResponseAsync()
		{
			// Arrange
			var codes = new [] { "DSA", "QWE", "ASD" };

			foreach (var code in codes)
			{
				wrapper.SendRequest(new ServiceTaskRunRequest
				{
					Command = RequestCommandType.DirectRun,
					AssemblyName = "someAssembly",
					Code = code,
					GuidId = Guid.NewGuid().ToString(),
					ConfigString = "emptyConfig"
				});
			}

			var token = cancellationTokenSource.Token;

			foreach (var code in codes)
			{
				// Act
				NextResponse(token);

				// Assert
				NUnit.Framework.Assert.That(wrapper.CurrentResponse.TaskCode, Is.EqualTo($"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.DirectRun)}:{code}"));
			}
		}

		public void TestNextResponseAsync_NoResponse()
		{
			// Arrange

			var task = NextResponseTask(cancellationTokenSource.Token);

			// Act: need to wait for timeout to check the current response if there are no responses
			var complete = task.Wait(TimeSpan.FromSeconds(2));

			// Assert
			NUnit.Framework.Assert.That(complete, Is.EqualTo(false));
			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var response = wrapper.CurrentResponse;
			});

			// has to do this to clean up the test case properly
			cancellationTokenSource.Cancel();

			AssertExceptionThrown<Exception>(() => task.Wait());
		}

		public void TestNextResponseAsync_AfterClose()
		{
			// Arrange
			wrapper.SendRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.DirectRun,
				AssemblyName = "someAssembly",
				Code = "DSA",
				GuidId = Guid.NewGuid().ToString(),
				ConfigString = "emptyConfig"
			});

			wrapper.SendRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.DirectRun,
				AssemblyName = "someAssembly",
				Code = "QWE",
				GuidId = Guid.NewGuid().ToString(),
				ConfigString = "emptyConfig"
			});

			// close
			wrapper.Close();

			var token = cancellationTokenSource.Token;

			// consume the last two responses
			NextResponse(token);
			NUnit.Framework.Assert.That(wrapper.CurrentResponse.TaskCode, Is.EqualTo($"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.DirectRun)}:DSA"));

			NextResponse(token);
			NUnit.Framework.Assert.That(wrapper.CurrentResponse.TaskCode, Is.EqualTo($"{Enum.GetName(typeof(RequestCommandType), RequestCommandType.DirectRun)}:QWE"));

			// Act: no more responses
			NextResponse(token);

			// Assert
			AssertExceptionThrown<InvalidOperationException>(() => {
				var response = wrapper.CurrentResponse;
			});
		}

		public void TestNextResponseAsync_Canceled()
		{
			// Arrange
			var wrapperTask = NextResponseTask(cancellationTokenSource.Token);

			using var syncEvent = new ManualResetEvent(false);

			var task = Task.Run(() =>
			{
				syncEvent.Set();

				// wait synchronously
				wrapperTask.Wait();
			});

			syncEvent.WaitOne();

			// Act
			cancellationTokenSource.Cancel();

			// Assert
			AssertExceptionThrown<Exception>(() => task.Wait());
			NUnit.Framework.Assert.That(task.IsCompleted, Is.EqualTo(true));
			NUnit.Framework.Assert.That(task.IsFaulted, Is.EqualTo(true));

			NUnit.Framework.Assert.That(cancellationTokenSource.IsCancellationRequested, Is.EqualTo(true));
			NUnit.Framework.Assert.That(wrapperTask.IsCompleted, Is.EqualTo(true));
			NUnit.Framework.Assert.That(wrapperTask.IsFaulted, Is.EqualTo(true));

			AssertExceptionThrown<InvalidOperationException>(() =>
			{
				var response = wrapper.CurrentResponse;
			});
		}

		[ExpectNoExceptions]
		public void TestClose()
		{
			// Arrange

			// Act
			wrapper.Close();
			Thread.Sleep(TimeSpan.FromSeconds(3));

			// Assert
			NUnit.Framework.Assert.That(exitWasClean, Is.EqualTo(true));
		}

		void NextResponse(CancellationToken token)
		{
			NextResponseTask(token).Wait();
		}

		Task<bool> NextResponseTask(CancellationToken token)
		{
			return Task.Run(async () => await wrapper.NextResponseAsync(token));
		}

		CancellationTokenSource cancellationTokenSource;
		ServiceRunnerClientWrapper wrapper;
		bool exitWasClean;
		Server server;
	}
}
