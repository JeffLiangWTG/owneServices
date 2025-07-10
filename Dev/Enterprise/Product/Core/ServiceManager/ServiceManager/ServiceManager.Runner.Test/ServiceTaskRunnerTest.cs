using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CargoWise.Common;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class ServiceTaskRunnerTest
	{
		[Test]
		public void TestWrongParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(null, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, Mock.Of<IEnvironmentCheckerStrategy>(), errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory()));
				Assert.That(result.ParamName, Is.EqualTo("serviceTaskLogger"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(serviceTaskLoggerMock.Object, null, processEnvironmentRecorderMock.Object, Mock.Of<IEnvironmentCheckerStrategy>(), errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory()));
				Assert.That(result.ParamName, Is.EqualTo("runnerLogger"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, null, Mock.Of<IEnvironmentCheckerStrategy>(), errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory()));
				Assert.That(result.ParamName, Is.EqualTo("processEnvironmentRecorder"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, null, errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory()));
				Assert.That(result.ParamName, Is.EqualTo("environmentCheckerStrategy"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, Mock.Of<IEnvironmentCheckerStrategy>(), null, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory()));
				Assert.That(result.ParamName, Is.EqualTo("errorReporterProxy"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, Mock.Of<IEnvironmentCheckerStrategy>(), errorReporterProxyMock.Object, null, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory()));
				Assert.That(result.ParamName, Is.EqualTo("serviceTaskHandlerInitializer"));

				result = Assert.Throws<ArgumentNullException>(() => _ = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, Mock.Of<IEnvironmentCheckerStrategy>(), errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, null));
				Assert.That(result.ParamName, Is.EqualTo("loggerFactory"));

				result = Assert.Throws<ArgumentNullException>(() => serviceTaskRunner.RunServiceTask(null, new CancellationTokenSource()));
				Assert.That(result.ParamName, Is.EqualTo("runCommandInfo"));
			});
		}

		[Test]
		public void TestRunServiceTaskDisposesServiceTaskSuccessful()
		{
			// Arrange
			var runCommandInfo = Mock.Of<IRunCommandInfo>(x => x.Code == "AAA");
			var sequenceMock = new MockSequence();
			using var cancellationTokenSource = new CancellationTokenSource();
			serviceTaskMock
				.SetupGet(task => task.HostedServiceAttribute)
				.Returns(hostedServiceConfigMock.Object);
			serviceTaskMock
				.InSequence(sequenceMock)
				.Setup(task => task.Run(It.IsAny<CancellationToken>()));

			var referenceObject = serviceTaskMock.Object;
			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler(runCommandInfo.AssemblyName, runCommandInfo.Code, runCommandInfo.ConfigString))
				.Returns(referenceObject);

			// Act
			using (ErrorReporter.SetTemporaryInstanceForTest(new Mock<IErrorReporter>().Object))
			{
				var result = serviceTaskRunner.RunServiceTask(runCommandInfo, cancellationTokenSource);

				// Assert
				Assert.That(result, Is.EqualTo(ServiceTaskRunResult.Success));
			}

			serviceTaskMock.Verify(s => s.Dispose(), Times.Once);
		}

		[Test]
		public void TestRunServiceTaskStartsActivityForServiceTask()
		{
			// Arrange
			const string serviceTaskCode = "TST";
			var runCommandInfoMock = new Mock<IRunCommandInfo>();
			runCommandInfoMock.SetupGet(o => o.Code).Returns(serviceTaskCode);

			serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler(runCommandInfoMock.Object.AssemblyName, runCommandInfoMock.Object.Code, runCommandInfoMock.Object.ConfigString))
				.Returns(serviceTaskMock.Object);

			Activity activity = null;

			// Act
			using (ApplicationLoggingTestHelper.ListenFor(serviceTaskCode, "ServiceTaskRunner.RunServiceTask", o => activity = o))
			{
				using var tokeSource = new CancellationTokenSource();
				serviceTaskRunner.RunServiceTask(runCommandInfoMock.Object, tokeSource);

				// Assert
				Assert.That(activity, Is.Not.Null);
				Assert.That(activity.Tags, Has.One.Matches<KeyValuePair<string, string>>(o => o is { Key: "ServiceTaskCode", Value: "TST" }));
			}
		}

		[Test]
		public void TestRunServiceTaskLogsOperationCanceledException()
		{
			// Arrange
			using var cancellationTokenSource = new CancellationTokenSource();
			serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
			serviceTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback((CancellationToken cancellationToken) =>
				{
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				});

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler("assemblyName", "AAA", string.Empty))
				.Returns(serviceTaskMock.Object);

			// Act
			var result = serviceTaskRunner.RunServiceTask(new DirectRunCommandInfo("assemblyName", "AAA", Guid.Empty), cancellationTokenSource);

			// Assert
			serviceTaskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Once);
			serviceTaskLoggerMock.Verify(logger => logger.Log(LogLevel.Warning, "Service task run has been canceled"), Times.Once);
			Assert.That(result, Is.EqualTo(ServiceTaskRunResult.Cancelled));
			Assert.That(cancellationTokenSource.IsCancellationRequested, Is.True);
		}

		[Test]
		public void TestRunServiceTaskReportsWrongOperationCanceledException()
		{
			RunServiceTaskReportsException(new OperationCanceledException());
		}

		[Test]
		public void TestRunServiceTaskReportsNotCriticalException()
		{
			Assert.Multiple(() =>
			{
				foreach (var exception in ExceptionSource.NotCriticalExceptions)
				{
					RunServiceTaskReportsException(exception);
				}
			});
		}

		void RunServiceTaskReportsException(Exception exception)
		{
			// Arrange
			serviceTaskMock.Reset();
			serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
			serviceTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Throws(exception);

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler("assemblyName", "AAA", string.Empty))
				.Returns(serviceTaskMock.Object);

			using var cancellationTokenSource = new CancellationTokenSource();

			var result = serviceTaskRunner.RunServiceTask(new DirectRunCommandInfo("assemblyName", "AAA", Guid.Empty), cancellationTokenSource);

			// Assert
			serviceTaskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Never);
			serviceTaskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			serviceTaskMock.Verify(task => task.HandleException(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
			Assert.That(result, Is.EqualTo(ServiceTaskRunResult.UnhandledException));
			errorReporterProxyMock.Reset();
		}

		[ExpectNoExceptions]
		[Test]
		public void TestRunServiceTaskLogsAssemblyLoaded()
		{
			// Arrange
			var runCommandInfoMock = new Mock<IRunCommandInfo>();

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler(null, null, null))
				.Returns(serviceTaskMock.Object);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunner.RunServiceTask(runCommandInfoMock.Object, cancellationTokenSource);

			// Assert
			runCommandInfoMock.Verify(x => x.FormatRequestToLogMessage(RunnerLogMessageStage.AssemblyLoaded), Times.Once);
			runnerLoggerMock.Verify(o => o.Log(LogLevel.Debug, It.IsAny<string>()), Times.Once());
		}

		[Test]
		public void TestEnvironmentCheckerStrategyInitializeIsCalledBeforeServiceTaskRuns()
		{
			//Arrange
			var callList = new List<string>();
			serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
			serviceTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() => callList.Add(nameof(IServiceTaskHandler.Run)));
			environmentCheckerStrategyMock
				.Setup(strategy => strategy.Initialize(It.IsAny<IRunCommandInfo>()))
				.Callback(() => callList.Add(nameof(IEnvironmentCheckerStrategy.Initialize)));

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler("assemblyName", "AAA", string.Empty))
				.Returns(serviceTaskMock.Object);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunner.RunServiceTask(new DirectRunCommandInfo("assemblyName", "AAA", Guid.Empty), cancellationTokenSource);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				environmentCheckerStrategyMock.Verify(strategy => strategy.Initialize(It.IsAny<IRunCommandInfo>()), Times.Once);
				serviceTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Once);
				Assert.That(callList.ToArray(), Is.EqualTo(new[] { "Initialize", "Run" }));
			});
		}

		[Test]
		public void TestEnvironmentCheckerStrategyExecuteOnServiceTaskCompletionIsCalledAfterServiceTaskCompletes()
		{
			//Arrange
			var callList = new List<string>();
			serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
			serviceTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Callback(() => callList.Add(nameof(IServiceTaskHandler.Run)));
			environmentCheckerStrategyMock
				.Setup(strategy => strategy.ExecuteOnServiceTaskCompletion(It.IsAny<IServiceTaskHandler>()))
				.Callback(() => callList.Add(nameof(IEnvironmentCheckerStrategy.ExecuteOnServiceTaskCompletion)));

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler("assemblyName", "AAA", string.Empty))
				.Returns(serviceTaskMock.Object);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			serviceTaskRunner.RunServiceTask(new DirectRunCommandInfo("assemblyName", "AAA", Guid.Empty), cancellationTokenSource);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				serviceTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Once);
				environmentCheckerStrategyMock.Verify(strategy => strategy.ExecuteOnServiceTaskCompletion(It.IsAny<IServiceTaskHandler>()), Times.Once);
				Assert.That(callList.ToArray(), Is.EqualTo(new[] { "Run", "ExecuteOnServiceTaskCompletion" }));
			});
		}

		[Test]
		public void TestEnvironmentCheckerStrategyExecuteOnServiceTaskExceptionIsCalledIfServiceTaskThrowsException()
		{
			//Arrange
			var callList = new List<string>();
			serviceTaskMock.SetupGet(task => task.HostedServiceAttribute).Returns(hostedServiceConfigMock.Object);
			environmentCheckerStrategyMock
				.Setup(strategy => strategy.Initialize(It.IsAny<IRunCommandInfo>()))
				.Callback(() => callList.Add(nameof(IEnvironmentCheckerStrategy.Initialize)));
			serviceTaskMock
				.Setup(task => task.Run(It.IsAny<CancellationToken>()))
				.Throws<ArgumentNullException>();
			environmentCheckerStrategyMock
				.Setup(strategy => strategy.ExecuteOnServiceTaskException(It.IsAny<IServiceTaskHandler>()))
				.Callback(() => callList.Add(nameof(IEnvironmentCheckerStrategy.ExecuteOnServiceTaskException)));

			serviceTaskHandlerInitializer
				.Setup(x => x.CreateServiceTaskHandler("assemblyName", "AAA", string.Empty))
				.Returns(serviceTaskMock.Object);

			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			var result = serviceTaskRunner.RunServiceTask(new DirectRunCommandInfo("assemblyName", "AAA", Guid.Empty), cancellationTokenSource);

			// Assert
			Assert.DoesNotThrow(() =>
			{
				environmentCheckerStrategyMock.Verify(strategy => strategy.Initialize(It.IsAny<IRunCommandInfo>()), Times.Once);
				serviceTaskMock.Verify(task => task.Run(It.IsAny<CancellationToken>()), Times.Once);
				environmentCheckerStrategyMock.Verify(strategy => strategy.ExecuteOnServiceTaskException(It.IsAny<IServiceTaskHandler>()), Times.Once);
				Assert.That(callList.ToArray(), Is.EqualTo(new[] { "Initialize", "ExecuteOnServiceTaskException" }));
				Assert.That(result, Is.EqualTo(ServiceTaskRunResult.UnhandledException));
			});
		}

		[SetUp]
		public void SetUp()
		{
			listener = ApplicationLoggingTestHelper.Listen();
			serviceTaskLoggerMock = new Mock<IServiceTaskLogger>();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			processEnvironmentRecorderMock = new Mock<IProcessEnvironmentRecorder>();
			environmentCheckerStrategyMock = new Mock<IEnvironmentCheckerStrategy>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			serviceTaskHandlerInitializer = new Mock<IServiceTaskHandlerInitializer>();

			serviceTaskRunner = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, environmentCheckerStrategyMock.Object, errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory());

			serviceTaskMock = new Mock<IDisposableServiceTaskHandler>();
			hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
		}

		[TearDown]
		public void TearDown()
		{
			listener.Dispose();
		}

		Mock<IHostedServiceAttribute> hostedServiceConfigMock;
		Mock<IProcessEnvironmentRecorder> processEnvironmentRecorderMock;
		Mock<IRunnerLogger> runnerLoggerMock;
		Mock<IServiceTaskLogger> serviceTaskLoggerMock;
		Mock<IDisposableServiceTaskHandler> serviceTaskMock;
		Mock<IEnvironmentCheckerStrategy> environmentCheckerStrategyMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<IServiceTaskHandlerInitializer> serviceTaskHandlerInitializer;
		ServiceTaskRunner serviceTaskRunner;
		IDisposable listener;
	}
}
