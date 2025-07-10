using System;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class TaskRunnerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			hostedServiceConfigProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			applicationExceptionHandler = new ApplicationExceptionHandler(errorReporterProxyMock.Object);
			taskRunner = new Mock<TaskRunner>(MockBehavior.Strict, runnerLoggerMock.Object, hostedServiceConfigProviderMock.Object)
			{
				CallBase = true,
			};
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
		}

		protected override void TearDown()
		{
			Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
			base.TearDown();
		}

		public void TestWrongConstructorParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<TargetInvocationException>(() => new Mock<TaskRunner>(MockBehavior.Loose, null, hostedServiceConfigProviderMock.Object) { CallBase = true }.Object.Run(true, applicationExceptionHandler));
				AssertType<ArgumentNullException>(result.InnerException);
				AssertEquals("logger", ((ArgumentNullException)result.InnerException).ParamName);

				result = AssertExceptionThrown<TargetInvocationException>(() => new Mock<TaskRunner>(runnerLoggerMock.Object, null) { CallBase = true }.Object.Run(true, applicationExceptionHandler));
				AssertType<ArgumentNullException>(result.InnerException);
				AssertEquals("hostedServiceAttributeProvider", ((ArgumentNullException)result.InnerException).ParamName);
			});
		}

		public void TestCriticalExceptionIsNotReportedToErrorReporter()
		{
			CombineAssertions(() =>
			{
				foreach (var exception in ExceptionSource.CriticalExceptions)
				{
					Test(exception);
				}
			});

			void Test(Exception exception)
			{
				// Arrange
				runnerLoggerMock.Reset();
				errorReporterProxyMock.Reset();
				taskRunner.Protected()
					.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
					.Throws(exception);

				// Act
				taskRunner.Object.Run(true, applicationExceptionHandler);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
					runnerLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
					runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Error, "Unhandled exception in Task Runner", exception), Times.Once);
				});
			}
		}

		public void TestNotCriticalExceptionIsReportedToErrorReporter()
		{
			CombineAssertions(() =>
			{
				foreach (var exception in ExceptionSource.NotCriticalExceptions)
				{
					Test(exception);
				}
			});

			void Test(Exception exception)
			{
				// Arrange
				errorReporterProxyMock.Reset();
				taskRunner.Protected()
					.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
					.Throws(exception);

				// Act
				taskRunner.Object.Run(true, applicationExceptionHandler);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
					errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.Is<string>(s => s.StartsWith("Service Manager Runner exception", StringComparison.OrdinalIgnoreCase)), exception), Times.Once);
				});
			}
		}

		public void TestNoExceptionWhenAccessingDbOnRunInternal()
		{
			// Arrange
			taskRunner.Protected()
				.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
				.Callback(() => Db.Connection.ExecuteScalar("SELECT @@servername"))
				.Returns((RunnerExitCode)3);

			// Act
			taskRunner.Object.Run(true, applicationExceptionHandler);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			});
		}

		public void TestNoExceptionWhenAccessingDbOnError()
		{
			// Arrange
			errorReporterProxyMock
				.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback(() => Db.Connection.ExecuteScalar("SELECT @@servername"));

			taskRunner.Protected()
				.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
				.Throws<Exception>();

			// Act
			taskRunner.Object.Run(true, applicationExceptionHandler);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
			});
		}

		public void TestReturnsCodeFromRunInternal()
		{
			CombineAssertions(() =>
			{
				var exitCodes = Enum
					.GetValues(typeof(RunnerExitCode))
					.Cast<RunnerExitCode>();

				foreach (var exitCode in exitCodes)
				{
					Test(exitCode);
				}
			});

			void Test(RunnerExitCode resultFromRunInternal)
			{
				// Arrange
				taskRunner.Protected()
					.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
					.Returns(resultFromRunInternal);

				// Act
				var result = taskRunner.Object.Run(true, applicationExceptionHandler);

				// Assert
				AssertEquals(resultFromRunInternal, result);
			}
		}

		public void TestReturnsErrorCodeInCaseOfException()
		{
			CombineAssertions(() =>
			{
				foreach (var exception in ExceptionSource.CriticalExceptions.Concat(ExceptionSource.NotCriticalExceptions))
				{
					Test(exception);
				}
			});

			void Test(Exception exception)
			{
				// Arrange
				taskRunner.Protected()
					.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
					.Throws(exception);

				// Act
				var result = taskRunner.Object.Run(true, applicationExceptionHandler);

				// Assert
				AssertEquals(RunnerExitCode.RunnerFailure, result);
			}
		}

		[ExpectNoExceptions]
		public void TestRun_ExceptionHandlerIsCalledForTaskExceptions()
		{
			// Arrange
			taskRunner.Protected()
				.Setup<RunnerExitCode>("RunInternal", ItExpr.IsAny<bool>())
				.Throws(new OutOfMemoryException());

			var mockAppExceptionHandler = new Mock<IApplicationExceptionHandler>();
			// Act
			taskRunner.Object.Run(true, mockAppExceptionHandler.Object);

			// Assert
			mockAppExceptionHandler.Verify(x => x.HandleFromTask(It.IsAny<Exception>(), It.IsAny<Func<IRunnerLogger>>()), Times.Once);
		}

		Mock<IClientHostedServiceAttributeProvider> hostedServiceConfigProviderMock;
		Mock<IRunnerLogger> runnerLoggerMock;
		Mock<TaskRunner> taskRunner;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		ApplicationExceptionHandler applicationExceptionHandler;
	}
}
