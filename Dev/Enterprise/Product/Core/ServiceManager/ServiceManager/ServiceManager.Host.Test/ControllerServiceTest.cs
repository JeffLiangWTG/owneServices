using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.CW;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ControllerServiceTest : TestCase
	{
		public void TestWrongConstructorParams()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var lazyServiceManagerApplication = new Lazy<IServiceManagerApplication>(() => serviceManagerApplication.Object);

				var result = AssertExceptionThrown<ArgumentNullException>(() => new ControllerService(null, hostLogger.Object, applicationEmergencyExit.Object, serviceNameProvider.Object, cancellationTokenRequestor.Object, cancellationTokenProvider.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("serviceManagerApplication"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ControllerService(lazyServiceManagerApplication, null, applicationEmergencyExit.Object, serviceNameProvider.Object, cancellationTokenRequestor.Object, cancellationTokenProvider.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ControllerService(lazyServiceManagerApplication, hostLogger.Object, null, serviceNameProvider.Object, cancellationTokenRequestor.Object, cancellationTokenProvider.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("applicationEmergencyExit"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ControllerService(lazyServiceManagerApplication, hostLogger.Object, applicationEmergencyExit.Object, serviceNameProvider.Object, null, cancellationTokenProvider.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("cancellationRequester"));

				result = AssertExceptionThrown<ArgumentNullException>(() => new ControllerService(lazyServiceManagerApplication, hostLogger.Object, applicationEmergencyExit.Object, serviceNameProvider.Object, cancellationTokenRequestor.Object, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("cancellationTokenProvider"));
			});
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestOnStopHasNoObjectDisposedExceptionFromUnfinishedMainThreadAfterDispose()
		{
			// Arrange
			using (var taskStartedEvent = new ManualResetEvent(false))
			using (var onStopEvent = new ManualResetEvent(false))
			using (var backgroundThreadFinishedEvent = new ManualResetEvent(false))
			{
				var serviceManagerTaskMock = new Mock<IServiceManagerTask>();
				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				var applicationEmergenceExitMock = new Mock<IApplicationEmergencyExit>();
				applicationEmergenceExitMock
					.Setup(x => x.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback((string msg, Exception ex) =>
					{
						errorReporterProxyMock.Object.ReportOnce(msg, ex);
					});
				serviceManagerTaskMock
					.Setup(task => task.Run(It.IsAny<CancellationToken>()))
					.Callback<CancellationToken>(cancellationToken =>
					{
						try
						{
							var actionQueue = new BackgroundThreadActionQueue(cancellationToken);
							taskStartedEvent.Set();
							onStopEvent.WaitOne(TimeSpan.FromSeconds(30));
							actionQueue.WaitForEnqueue(TimeSpan.FromSeconds(1), cancellationToken);
						}
						finally
						{
							backgroundThreadFinishedEvent.Set();
						}
					});

				using var serviceProvider = new ServiceCollection()
					.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName })
					.RegisterCommonServices()
					.RegisterHostLoggerServices()
					.RegisterSharedServices()
					.RemoveAll<IServiceManagerTask>()
					.AddSingleton(serviceManagerTaskMock.Object)
					.AddSingleton(errorReporterProxyMock.Object)
					.AddTransient(_ => applicationEmergenceExitMock.Object)
					.BuildServiceProvider();

				var controllerService = serviceProvider.GetRequiredService<ControllerService>();

				controllerService.ConsoleRun(() => taskStartedEvent.WaitOne(TimeSpan.FromSeconds(30)));

				// Act
				controllerService.Dispose();
				onStopEvent.Set();

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					NUnit.Framework.Assert.That(backgroundThreadFinishedEvent.WaitOne(TimeSpan.FromSeconds(30)), Is.EqualTo(true));
					errorReporterProxyMock.Verify(x => x.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
				});
			}
		}

		public void TestDispatchingThreadIsDisposedOnControllerServiceDispose()
		{
			// Arrange

			using var controllerService = CreateControllerService();
			controllerService.ConsoleRun();

			// Act
			// Assert
			AssertNoExceptionThrown(() => controllerService.Dispose());
		}

		[ExpectNoExceptions]
		public void TestServiceName()
		{
			// Arrange, Act
			using var controllerService = CreateControllerService();

			// Assert
			NUnit.Framework.Assert.That(controllerService.ServiceName, Does.Contain("ServiceName"));
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestControllerServiceStop_DatabaseUpgradeCompleted_OthersLockedOut()
		{
			const string failureMessage = "ControllerService can be stopped when db logins are disabled, connections killed and db schema version updated.";
			TestControllerServiceStop(true, true, true, failureMessage);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestControllerServiceStop_DatabaseUpgraded_BinaryNotUpgraded()
		{
			const string failureMessage = "ControllerService can be stopped when db has been fully upgraded but host binary not upgraded yet.";
			TestControllerServiceStop(false, false, true, failureMessage);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestControllerServiceStop_DatabaseUpgradeInProgress_OthersLockedOut()
		{
			const string failureMessage = "ControllerService can be stopped when db logins are disabled, connections killed, db not upgraded yet.";
			TestControllerServiceStop(true, true, false, failureMessage);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestControllerServiceStop_DatabaseUpgradeInProgress_DbLoginsDisabled()
		{
			const string failureMessage = "ControllerService can be stopped when db logins are disabled.";
			TestControllerServiceStop(true, false, false, failureMessage);
		}

		void TestControllerServiceStop(bool acquireLockOut, bool killOtherConnections, bool updateSchemaVersion, string failureMessage)
		{
			// Arrange
			using var controller = CreateControllerService();

			using (Db.DisposableUpgrade_ForTest(acquireLockOut, killOtherConnections, updateSchemaVersion))
			{
				// Act
				// Assert
				AssertNoExceptionThrown(failureMessage, () => controller.Stop());
			}

			applicationEmergencyExit.Verify(
				x => x.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()),
				Times.Never);
		}

		[ExpectNoExceptions]
		public void TestOnStart_OnCriticalExceptions_ControllerServiceCallsApplicationEmergencyExit()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				ExceptionSource.CriticalExceptions.ForEach(exception => TestOnCriticalExceptions_ApplicationEmergencyExitIsInvoked(exception, "OnStart", o => o.ConsoleRun()));
			});
		}

		[ExpectNoExceptions]
		public void TestOnStop_OnCriticalExceptions_ControllerServiceCallsApplicationEmergencyExit()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				ExceptionSource.CriticalExceptions.ForEach(exception => TestOnCriticalExceptions_ApplicationEmergencyExitIsInvoked(exception, "OnStop", o => o.Stop()));
			});
		}

		[ExpectNoExceptions]
		public void TestOnShutDown_OnCriticalExceptions_ControllerServiceCallsApplicationEmergencyExit()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				ExceptionSource.CriticalExceptions.ForEach(exception => TestOnCriticalExceptions_ApplicationEmergencyExitIsInvoked(exception, "OnShutdown", o => o.ConsoleRun()));
			});
		}

		void TestOnCriticalExceptions_ApplicationEmergencyExitIsInvoked(Exception exception, string logMessage, Action<ControllerService> testAction)
		{
			// Arrange
			hostLogger.Reset();
			hostLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), logMessage)).Throws(exception);

			using var controller = CreateControllerService();

			// Act
			testAction(controller);

			// Assert
			NUnit.Framework.Assert.That(exception.IsCriticalException(), Is.True);

			applicationEmergencyExit.Verify(x => x.ExitApplicationUnsafe(It.IsAny<string>(), exception), Times.Once);
			applicationEmergencyExit.VerifyNoOtherCalls();

			hostLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), logMessage), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestLoggerOnShutDownExceptionLogsToHost()
		{
			var repo = new MockRepository(MockBehavior.Strict);
			{
				using var controller = CreateControllerService();

				hostLogger.Setup(x => x.Log(It.IsAny<LogLevel>(), "OnShutdown"));
				controller.ConsoleRun();
				hostLogger.VerifyAll();

				hostLogger.Reset();
				hostLogger.Setup(logger => logger.Log(LogLevel.Information, "OnStart"));
				controller.ConsoleRun();
				hostLogger.VerifyAll();

				hostLogger.Reset();
				hostLogger.Setup(logger => logger.Log(LogLevel.Information, "OnStop"));
				controller.Stop();
				hostLogger.VerifyAll();
			}
		}

		[ExpectNoExceptions]
		public void TestRunWithExcepionGetsHandled()
		{
			// Arrange
			serviceManagerApplication.Setup(o => o.Run(CancellationToken.None)).Throws<Exception>();

			using var controller = CreateControllerService();

			// Act
			controller.ConsoleRun();

			// Assert
			applicationEmergencyExit.Verify(o => o.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()), Times.AtLeastOnce);
		}

		protected override void SetUp()
		{
			serviceManagerApplication = new Mock<IServiceManagerApplication>();
			hostLogger = new Mock<IHostLogger>();
			applicationEmergencyExit = new Mock<IApplicationEmergencyExit>();
			applicationEmergencyExit.Setup(x => x.ExitApplicationUnsafe(It.IsAny<string>(), It.IsAny<Exception>()));
			serviceNameProvider = new Mock<IServiceNameProvider>();
			cancellationTokenRequestor = new Mock<ICancellationRequester>();
			cancellationTokenProvider = new Mock<ICancellationTokenProvider>();

			serviceNameProvider.Setup(o => o.GetServiceName()).Returns("ServiceName");
		}

		ControllerService CreateControllerService()
		{
			return new ControllerService(new Lazy<IServiceManagerApplication>(() => serviceManagerApplication.Object), hostLogger.Object, applicationEmergencyExit.Object, serviceNameProvider.Object, cancellationTokenRequestor.Object, cancellationTokenProvider.Object);
		}

		Mock<IApplicationEmergencyExit> applicationEmergencyExit;
		Mock<ICancellationTokenProvider> cancellationTokenProvider;
		Mock<ICancellationRequester> cancellationTokenRequestor;
		Mock<IHostLogger> hostLogger;
		Mock<IServiceManagerApplication> serviceManagerApplication;
		Mock<IServiceNameProvider> serviceNameProvider;
	}
}
