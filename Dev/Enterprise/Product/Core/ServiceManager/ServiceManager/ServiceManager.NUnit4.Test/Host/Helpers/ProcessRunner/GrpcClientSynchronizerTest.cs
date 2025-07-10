using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.ProcessRunner
{
	public class GrpcClientSynchronizerTest
	{
		[SetUp]
		public void SetUp()
		{
			grpcEventHandleNames = new GrpcEventHandleNames();
			processMock = new Mock<IProcess>();
			synchronizer = new GrpcClientSynchronizer(grpcEventHandleNames);
		}

		[Test]
		public void TestSynchronization()
		{
			// Arrange

			// Act
			var task = Task.Run(ServerStartup);
			var result = synchronizer!.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened), processMock!.Object, TimeSpan.FromSeconds(5));
			task.Wait(TimeSpan.FromSeconds(5));

			// Assert
			Assert.That(result, Is.EqualTo(true));

			void ServerStartup()
			{
				// Arrange
				if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames!.RunnerEventWaitHandleName, out var hostReadyEvent))
				{
					Assert.Fail("Runner event handle should be opened by synchronizer");
				}
				hostReadyEvent?.WaitOne(TimeSpan.FromSeconds(10), false);
				if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames.HostEventWaitHandleName, out var runnerReadyEvent))
				{
					Assert.Fail("Host event handle should be opened by synchronizer");
				}
				using (runnerReadyEvent)
				using (hostReadyEvent)
				{
					runnerReadyEvent?.Set();
					hostReadyEvent?.Set();
				}
			}
		}

		[Test]
		public void TestSynchronizationFailureNoSignals()
		{
			var result = synchronizer!.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => !r.PortOpened), processMock!.Object, TimeSpan.FromSeconds(1));
			Assert.That(result, Is.EqualTo(false));
		}

		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => new GrpcClientSynchronizer(null));
				Assert.That(result?.ParamName, Is.EqualTo("eventHandleNames"));
			});
		}

		[Test]
		public void TestSynchronizationFailsImmediatelyIfProcessIsNotRunning_ProcessHasExited() => AssertSynchronizationFailsImmediatelyIfProcessIsNotRunning(Mock.Of<IProcess>(p => p.HasExited));
		[Test]
		public void TestSynchronizationFailsImmediatelyIfProcessIsNotRunning_ProcessIsNull() => AssertSynchronizationFailsImmediatelyIfProcessIsNotRunning(null);

		void AssertSynchronizationFailsImmediatelyIfProcessIsNotRunning(IProcess? process)
		{
			// Arrange
			// Act
			var task = Task.Run(ServerStartup);
			var result = synchronizer!.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => !r.PortOpened), process, TimeSpan.FromSeconds(5));
			task.Wait(TimeSpan.FromSeconds(5));

			// Assert
			Assert.That(result, Is.EqualTo(false));
			void ServerStartup()
			{
				// Arrange
				if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames!.RunnerEventWaitHandleName, out var hostReadyEvent))
				{
					Assert.Fail("Runner event handle should be opened by synchronizer");
				}
				hostReadyEvent?.WaitOne(TimeSpan.FromSeconds(10), false);
				if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames.HostEventWaitHandleName, out var runnerReadyEvent))
				{
					Assert.Fail("Host event handle should be opened by synchronizer");
				}
				using (runnerReadyEvent)
				using (hostReadyEvent)
				{
					runnerReadyEvent?.Set();
					hostReadyEvent?.Set();
				}
			}
		}

		[Test]
		public void TestSynchronizationFailsImmediatelyIfProcessExitsDuringSynchronization()
		{
			// Arrange
			var wasKilled = false;
			processMock!
				.Setup(p => p.Kill())
				.Callback(() => wasKilled = true);
			processMock
				.Setup(p => p.HasExited)
				.Returns(() => wasKilled);

			// Act
			var task = Task.Run(ServerStartup);
			var result = synchronizer!.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => !r.PortOpened), processMock.Object, TimeSpan.FromSeconds(10));
			task.Wait();

			// Assert
			Assert.That(result, Is.False);
			Assert.That(wasKilled, Is.True);

			void ServerStartup()
			{
				// Arrange
				if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames!.RunnerEventWaitHandleName, out var hostReadyEvent))
				{
					Assert.Fail("Runner event handle should be opened by synchronizer");
				}
				hostReadyEvent?.WaitOne();
				if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames.HostEventWaitHandleName, out var runnerReadyEvent))
				{
					Assert.Fail("Host event handle should be opened by synchronizer");
				}
				using (runnerReadyEvent)
				using (hostReadyEvent)
				{
					processMock.Object.Kill();
					runnerReadyEvent?.Set();
					hostReadyEvent?.Set();
				}
			}
		}

		GrpcEventHandleNames? grpcEventHandleNames;
		GrpcClientSynchronizer? synchronizer;
		Mock<IProcess>? processMock;
	}
}
