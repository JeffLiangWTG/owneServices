using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Runner;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using static System.FormattableString;

namespace CargoWise.ServiceManager.Runner.Test
{
	[UseSnapshotProtection(true)]
	class GrpcRunnerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{ 
			var disposable = new Mock<IDisposable>().Object;
			runnerRegistryMock = new Mock<IRunnerRegistrySettings>();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			commandExecutionStrategyMock = new Mock<ICommandExecutionStrategy>();
			hostedServiceAttributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostCommunicationsStrategyMock = new Mock<IHostCommunicationStrategy>();
			queueMock = new Mock<IServiceTaskRunnerQueue>();
			queueServiceMock = new Mock<IServiceTaskRunnerQueueService>();
			queueServiceMock
				.Setup(x => x.Start(It.IsAny<string>()))
				.Returns(queueMock.Object);
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			nudgingController = ObjectFactory.Get<INudgingController>();
		}

		protected override void TearDown()
		{
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			base.TearDown();
		}

		public void TestSingleRunStopCommandCancelled()
		{
			var command = new StopCommandInfo();
			var runResult = ServiceTaskRunResult.Cancelled;
			AssertRunWithSingleRun(command, runResult, RunnerExitCode.NoIssues);
			queueMock.Verify(s => s.CloseStream(true, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestSingleRunDirectRunCommandCommandSuccess()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.Success;
			AssertRunWithSingleRun(command, runResult, RunnerExitCode.NoIssues);
			hostCommunicationsStrategyMock.Verify(s => s.Completed(queueMock.Object), Times.Once);
			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestSingleRunDirectRunCommandCommandUnhandledException()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.UnhandledException;
			AssertRunWithSingleRun(command, runResult, RunnerExitCode.ServiceTaskUnhandledException);
			hostCommunicationsStrategyMock.Verify(s => s.Completed(queueMock.Object), Times.Once);
			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestSingleRunDirectRunCommandCommandServiceTaskLockNotAcquired()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.ServiceTaskLockNotAcquired;
			AssertRunWithSingleRun(command, runResult, RunnerExitCode.NoIssues);
			hostCommunicationsStrategyMock.Verify(s => s.ServiceTaskLockNotAcquired(queueMock.Object, command), Times.Once);
			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestSingleRunDirectRunCommandCommandGroupLockNotAcquired()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.GroupLockNotAcquired;
			AssertRunWithSingleRun(command, runResult, RunnerExitCode.NoIssues);
			hostCommunicationsStrategyMock.Verify(s => s.GroupLockNotAcquired(queueMock.Object, command), Times.Once);
			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestSingleRunDirectRunCommandCommandLockNotReleased()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.LockNotReleased;
			AssertRunWithSingleRun(command, runResult, RunnerExitCode.ServiceTaskLockNotReleased);
		}

		public void AssertRunWithSingleRun(ICommandInfo commandInfo, ServiceTaskRunResult runResult, RunnerExitCode expectedResult)
		{
			// Arrange
			var savedContext = WebRequest.DefaultWebProxy;
			var grpcGuid = Guid.NewGuid().ToString();

			// Act
			queueMock
				.SetupSequence(q => q.GetNextCommand())
				.Returns(commandInfo)
				.Returns((ICommandInfo)null);
			commandExecutionStrategyMock
				.Setup(e => e.Execute(It.IsAny<ICommandInfo>()))
				.Returns(runResult);
			using (new DisposableAction(() => WebRequest.DefaultWebProxy = savedContext))
			{
				var runner = new GrpcRunner(queueServiceMock.Object, runnerLoggerMock.Object, grpcGuid, hostedServiceAttributeProviderMock.Object, hostCommunicationsStrategyMock.Object, commandExecutionStrategyMock.Object, runnerRegistryMock.Object, nudgingController);
				var result = runner.Run(true, new ApplicationExceptionHandler(errorReporterProxyMock.Object));
				AssertEquals(expectedResult, result);
			}

			// Assert
			commandExecutionStrategyMock.Verify(e => e.Execute(commandInfo), Times.Once);
		}

		public void TestMultipleRunsStopCommandCancelled()
		{
			var command = new StopCommandInfo();
			var runResult = ServiceTaskRunResult.Cancelled;
			AssertRunWithMultipleRunsWaitsForCancelResult(command, runResult, RunnerExitCode.NoIssues, 3, 1, 1);
			queueMock.Verify(s => s.CloseStream(true, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestMultipleRunsDirectRunCommandCommandSuccess()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.Success;
			AssertRunWithMultipleRunsWaitsForCancelResult(command, runResult, RunnerExitCode.NoIssues, 4, 4, 3);
			hostCommunicationsStrategyMock.Verify(s => s.Completed(queueMock.Object), Times.Exactly(3));
			queueMock.Verify(s => s.CloseStream(true, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestMultipleRunsDirectRunCommandCommandUnhandledException()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.UnhandledException;
			AssertRunWithMultipleRunsWaitsForCancelResult(command, runResult, RunnerExitCode.ServiceTaskUnhandledException, 5, 1, 1);
			hostCommunicationsStrategyMock.Verify(s => s.Completed(queueMock.Object), Times.Once);
			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestMultipleRunsDirectRunCommandCommandServiceTaskLockNotAcquired()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.ServiceTaskLockNotAcquired;
			AssertRunWithMultipleRunsWaitsForCancelResult(command, runResult, RunnerExitCode.NoIssues, 6, 6, 5);
			hostCommunicationsStrategyMock.Verify(s => s.ServiceTaskLockNotAcquired(queueMock.Object, command), Times.Exactly(5));
			queueMock.Verify(s => s.CloseStream(true, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestMultipleRunsDirectRunCommandCommandGroupLockNotAcquired()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.GroupLockNotAcquired;
			AssertRunWithMultipleRunsWaitsForCancelResult(command, runResult, RunnerExitCode.NoIssues, 7, 7, 6);
			hostCommunicationsStrategyMock.Verify(s => s.GroupLockNotAcquired(queueMock.Object, command), Times.Exactly(6));
			queueMock.Verify(s => s.CloseStream(true, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestMultipleRunsDirectRunCommandCommandLockNotReleased()
		{
			var command = new DirectRunCommandInfo("someAssembly", "someCode", Guid.NewGuid(), string.Empty);
			var runResult = ServiceTaskRunResult.LockNotReleased;
			AssertRunWithMultipleRunsWaitsForCancelResult(command, runResult, RunnerExitCode.ServiceTaskLockNotReleased, 7, 1, 1);
		}

		public void AssertRunWithMultipleRunsWaitsForCancelResult(ICommandInfo commandInfo, ServiceTaskRunResult runResult, RunnerExitCode expectedResult, int numberOfCallsBeforeCancellation, int expectedNumberOfCalls, int expectedNumberOfExecutions)
		{
			// Arrange
			var numberOfCalls = 0;
			var savedContext = WebRequest.DefaultWebProxy;
			var grpcGuid = Guid.NewGuid().ToString();

			queueMock
				.Setup(q => q.GetNextCommand())
				.Returns(() =>
				{
					numberOfCalls++;
					if (numberOfCalls < numberOfCallsBeforeCancellation)
					{
						return commandInfo;
					}
					if (numberOfCalls == numberOfCallsBeforeCancellation)
					{
						return new StopCommandInfo();
					}
					return null;
				});
			commandExecutionStrategyMock
				.Setup(e => e.Execute(It.IsAny<ICommandInfo>()))
				.Returns(runResult);
			commandExecutionStrategyMock
				.Setup(e => e.Execute(It.IsAny<StopCommandInfo>()))
				.Returns(ServiceTaskRunResult.Cancelled);
			using (new DisposableAction(() => WebRequest.DefaultWebProxy = savedContext))
			{
				// Act
				var runner = new GrpcRunner(queueServiceMock.Object, runnerLoggerMock.Object, grpcGuid, hostedServiceAttributeProviderMock.Object, hostCommunicationsStrategyMock.Object, commandExecutionStrategyMock.Object, runnerRegistryMock.Object, nudgingController);
				var result = runner.Run(false, new ApplicationExceptionHandler(errorReporterProxyMock.Object));
				AssertEquals(expectedResult, result);
			}

			// Assert
			queueMock.Verify(s => s.GetNextCommand(), Times.Exactly(expectedNumberOfCalls));
			commandExecutionStrategyMock.Verify(e => e.Execute(commandInfo), Times.Exactly(expectedNumberOfExecutions));
		}

		public void TestRunnerExitsWhenNoCommandReceivedForUnloadTimeout()
		{
			// Arrange
			var savedContext = WebRequest.DefaultWebProxy;
			var grpcGuid = Guid.NewGuid().ToString();

			var timeout = TimeSpan.FromSeconds(60);
			runnerRegistryMock.SetupGet(o => o.ServiceTaskUnloadTimeout).Returns(timeout);

			queueMock
				.Setup(q => q.GetNextCommand())
				.Returns((ICommandInfo)null);
			commandExecutionStrategyMock
				.Setup(e => e.Execute(It.IsAny<ICommandInfo>()))
				.Returns(ServiceTaskRunResult.Success);
			commandExecutionStrategyMock
				.Setup(e => e.Execute(It.IsAny<StopCommandInfo>()))
				.Returns(ServiceTaskRunResult.Cancelled);
			using (new DisposableAction(() => WebRequest.DefaultWebProxy = savedContext))
			{
				// Act
				var runner = new GrpcRunner(queueServiceMock.Object, runnerLoggerMock.Object, grpcGuid, hostedServiceAttributeProviderMock.Object, hostCommunicationsStrategyMock.Object, commandExecutionStrategyMock.Object, runnerRegistryMock.Object, nudgingController);
				var result = runner.Run(false, new ApplicationExceptionHandler(errorReporterProxyMock.Object));
				AssertEquals(RunnerExitCode.NoIssues, result);
			}

			// Assert
			queueMock.Verify(s => s.GetNextCommand(), Times.Exactly(1));
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Error, Invariant($"Quit: no commands were received for twice the unload timeout.")), Times.Once);
			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestRunnerExitsWhenConnectionPoolingValueChangedDuringOperation()
		{
			// Arrange
			runnerRegistryMock
				.SetupSequence(o => o.ServiceTaskRunnerConnectionPoolingEnabled)
				.Returns(true)
				.Returns(false);

			var numberOfCalls = 0;
			var numberOfCallsBeforeCancellation = 5;
			var savedContext = WebRequest.DefaultWebProxy;
			var grpcGuid = Guid.NewGuid().ToString();

			queueMock
				.Setup(q => q.GetNextCommand())
				.Returns(() =>
				{
					numberOfCalls++;

					if (numberOfCalls == numberOfCallsBeforeCancellation)
					{
						return new StopCommandInfo();
					}

						return null;
					});
				commandExecutionStrategyMock
					.Setup(e => e.Execute(It.IsAny<ICommandInfo>()))
					.Returns(ServiceTaskRunResult.Success);
				commandExecutionStrategyMock
					.Setup(e => e.Execute(It.IsAny<StopCommandInfo>()))
					.Returns(ServiceTaskRunResult.Cancelled);
				using (new DisposableAction(() => WebRequest.DefaultWebProxy = savedContext))
				{
					// Act
					var runner = new GrpcRunner(queueServiceMock.Object, runnerLoggerMock.Object, grpcGuid, hostedServiceAttributeProviderMock.Object, hostCommunicationsStrategyMock.Object, commandExecutionStrategyMock.Object, runnerRegistryMock.Object, nudgingController);
					var result = runner.Run(false, new ApplicationExceptionHandler(errorReporterProxyMock.Object));
					AssertEquals(RunnerExitCode.NoIssues, result);
				}

			// Assert
			queueMock.Verify(s => s.GetNextCommand(), Times.Exactly(1));
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, Invariant($"Quit: connection pooling enabled value changed to [{runnerRegistryMock.Object.ServiceTaskRunnerConnectionPoolingEnabled}].")), Times.Once);

			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		public void TestRunnerExitsWhenRunnerProcessPriorityChangedDuringOperation()
		{
			// Arrange

			runnerRegistryMock
				.SetupSequence(o => o.RunnerProcessPriorityValue)
				.Returns(ProcessPriorityClass.BelowNormal)
				.Returns(ProcessPriorityClass.Idle);

			var numberOfCalls = 0;
			var numberOfCallsBeforeCancellation = 5;
			var savedContext = WebRequest.DefaultWebProxy;
			var grpcGuid = Guid.NewGuid().ToString();

			queueMock
				.Setup(q => q.GetNextCommand())
				.Returns(() =>
				{
					numberOfCalls++;

					if (numberOfCalls == numberOfCallsBeforeCancellation)
					{
						return new StopCommandInfo();
					}

						return null;
					});
				commandExecutionStrategyMock
					.Setup(e => e.Execute(It.IsAny<ICommandInfo>()))
					.Returns(ServiceTaskRunResult.Success);
				commandExecutionStrategyMock
					.Setup(e => e.Execute(It.IsAny<StopCommandInfo>()))
					.Returns(ServiceTaskRunResult.Cancelled);
				using (new DisposableAction(() => WebRequest.DefaultWebProxy = savedContext))
				{
					// Act
					var runner = new GrpcRunner(queueServiceMock.Object, runnerLoggerMock.Object, grpcGuid, hostedServiceAttributeProviderMock.Object, hostCommunicationsStrategyMock.Object, commandExecutionStrategyMock.Object, runnerRegistryMock.Object, nudgingController);
					var result = runner.Run(false, new ApplicationExceptionHandler(errorReporterProxyMock.Object));
					AssertEquals(RunnerExitCode.NoIssues, result);
				}

			// Assert
			queueMock.Verify(s => s.GetNextCommand(), Times.Exactly(1));
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, Invariant($"Quit: Process priority value changed from [{ProcessPriorityClass.BelowNormal}] to [{runnerRegistryMock.Object.RunnerProcessPriorityValue}].")), Times.Once);

			queueMock.Verify(s => s.CloseStream(false, It.IsAny<CancellationToken>()), Times.Once);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgingFailureExceptionIsLogged()
		{
			AssertNudging(
				() => NudgingController.ReportNudgeFailed(new[] { "XXX" }, new NotSupportedException("SomeNotSupportedException"), 0),
				LogLevel.Debug,
				"Nudge of Task(s)='XXX' failed. System.NotSupportedException: SomeNotSupportedException");
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgingFailureExceptionIsLoggedWhenRetriesRemaining()
		{
			AssertNudging(
				() => NudgingController.ReportNudgeFailed(new[] { "XXX" }, new NotSupportedException("SomeNotSupportedException"), 5),
				LogLevel.Debug,
				"Nudge of Task(s)='XXX' failed (retries remaining = 5). System.NotSupportedException: SomeNotSupportedException");
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgingFailureDescriptionIsLogged()
		{
			AssertNudging(
				() => NudgingController.ReportNudgeFailed(new[] { "XXX" }, "SomeDesc", 0),
				LogLevel.Debug,
				"Nudge of Task(s)='XXX' failed. SomeDesc");
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgingSuccessIsLogged()
		{
			{
				AssertNudging(
					() => NudgingController.ReportNudgeSucceeded(new[] { "XXX" }),
					LogLevel.Debug,
					"Nudge of Task(s)='XXX' succeeded. ");
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgeIgnoredDescriptionIsLogged()
		{
			{
				AssertNudging(
					() => NudgingController.ReportNudgeIgnored(new[] { "XXX" }, "IgnoreReason"),
					LogLevel.Debug,
					"Nudge of Task(s)='XXX' ignored. IgnoreReason");
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgeStartedIsLogged()
		{
			{
				AssertNudging(
					() => NudgingController.ReportNudgeStarted(new[] { "XXX" }, stackTrace: null),
					LogLevel.Debug,
					"Nudge of Task(s)='XXX' started. ");
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestNudgingFailureIsLoggedAsDebugWhenTaskDisabledOrInactive()
		{
			AssertNudging(
				() => NudgingController.ReportNudgeFailed(new[] { "XXX" }, "TaskDisabledOrInactive", 0),
				LogLevel.Debug,
				"Nudge of Task(s)='XXX' failed. TaskDisabledOrInactive");
		}

		static INudgingController NudgingController => ObjectFactory.Get<INudgingController>();

		public void AssertNudging(Action nudgeAction, LogLevel expectedLogLevel, string expectedLog)
		{
			var runner = new GrpcRunner(queueServiceMock.Object, runnerLoggerMock.Object, "1", hostedServiceAttributeProviderMock.Object, hostCommunicationsStrategyMock.Object, commandExecutionStrategyMock.Object, runnerRegistryMock.Object, nudgingController);
			queueMock
				.Setup(queue => queue.GetNextCommand())
				.Returns(new StopCommandInfo());
			commandExecutionStrategyMock
				.Setup(e => e.Execute(It.IsAny<StopCommandInfo>()))
				.Callback(nudgeAction)
				.Returns(ServiceTaskRunResult.Cancelled);
			var savedContext = WebRequest.DefaultWebProxy;
			using (new DisposableAction(() => WebRequest.DefaultWebProxy = savedContext))
			using (ErrorReporter.SetTemporaryInstanceForTest(Mock.Of<IErrorReporter>()))
			{
				runner.Run(true, new ApplicationExceptionHandler(errorReporterProxyMock.Object));
			}
			runnerLoggerMock.Verify(l => l.Log(expectedLogLevel, expectedLog), Times.Once);
		}

		public void TestWrongConstructorCall()
		{
			var loggerMock = new Mock<IRunnerLogger>();
			var hostedServiceConfigProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			var hostCommunicationStrategyMock = new Mock<IHostCommunicationStrategy>();
			var commandExecutionStrategy = new Mock<ICommandExecutionStrategy>().Object;
			var nudgingController = new Mock<INudgingController>();

			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => new GrpcRunner(queueServiceMock.Object, null, "SomeGuid", hostedServiceConfigProviderMock.Object, hostCommunicationStrategyMock.Object, commandExecutionStrategy, runnerRegistryMock.Object, nudgingController.Object));
				AssertEquals("logger", result?.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new GrpcRunner(queueServiceMock.Object, loggerMock.Object, "SomeGuid", null, hostCommunicationStrategyMock.Object, commandExecutionStrategy, runnerRegistryMock.Object, nudgingController.Object));
				AssertEquals("hostedServiceAttributeProvider", result?.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new GrpcRunner(queueServiceMock.Object, loggerMock.Object, "SomeGuid", hostedServiceConfigProviderMock.Object, null, commandExecutionStrategy, runnerRegistryMock.Object, nudgingController.Object));
				AssertEquals("hostCommunicationStrategy", result?.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new GrpcRunner(queueServiceMock.Object, loggerMock.Object, "SomeGuid", hostedServiceConfigProviderMock.Object, hostCommunicationStrategyMock.Object, null, runnerRegistryMock.Object, nudgingController.Object));
				AssertEquals("commandExecutionStrategy", result?.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new GrpcRunner(queueServiceMock.Object, loggerMock.Object, "SomeGuid", hostedServiceConfigProviderMock.Object, hostCommunicationStrategyMock.Object, commandExecutionStrategy, null, nudgingController.Object));
				AssertEquals("runnerRegistry", result?.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => new GrpcRunner(queueServiceMock.Object, loggerMock.Object, "SomeGuid", hostedServiceConfigProviderMock.Object, hostCommunicationStrategyMock.Object, commandExecutionStrategy, runnerRegistryMock.Object, null));
				AssertEquals("nudgingController", result?.ParamName);
			});
		}

		Mock<IRunnerLogger> runnerLoggerMock;
		Mock<IServiceTaskRunnerQueue> queueMock;
		Mock<IServiceTaskRunnerQueueService> queueServiceMock;
		Mock<IClientHostedServiceAttributeProvider> hostedServiceAttributeProviderMock;
		Mock<IHostCommunicationStrategy> hostCommunicationsStrategyMock;
		Mock<ICommandExecutionStrategy> commandExecutionStrategyMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<IRunnerRegistrySettings> runnerRegistryMock;
		INudgingController nudgingController;
	}
}
