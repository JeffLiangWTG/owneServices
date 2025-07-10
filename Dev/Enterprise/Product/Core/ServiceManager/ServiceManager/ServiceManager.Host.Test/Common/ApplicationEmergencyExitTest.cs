using System;
using System.Net;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Common
{
	class ApplicationEmergencyExitTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestWrongConstructorParams()
		{
			var eventLoggerMock = new Mock<IEventLogger>();
			var applicationExitProxyMock = new Mock<IApplicationExitProxy>();
			var hostLoggerMock = new Mock<IHostLogger>();
			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = NUnit.Framework.Assert.Throws<ArgumentNullException>(() => _ = new ApplicationEmergencyExit(null, applicationExitProxyMock.Object, hostLoggerMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("eventLogger"));

				result = NUnit.Framework.Assert.Throws<ArgumentNullException>(() => _ = new ApplicationEmergencyExit(eventLoggerMock.Object, null, hostLoggerMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("applicationExitProxy"));

				result = NUnit.Framework.Assert.Throws<ArgumentNullException>(() => _ = new ApplicationEmergencyExit(eventLoggerMock.Object, applicationExitProxyMock.Object, null, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

				result = NUnit.Framework.Assert.Throws<ArgumentNullException>(() => _ = new ApplicationEmergencyExit(eventLoggerMock.Object, applicationExitProxyMock.Object, hostLoggerMock.Object, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("errorReporterProxy"));
			});
		}

		[ExpectNoExceptions]
		public void TestExitApplicationUnsafe_NoDatabaseUpgradeThrownTwiceReport()
		{
			AssertExitApplicationUnsafe_NoDatabaseUpgradeThrownTwiceReport(hasException: false);
		}

		[ExpectNoExceptions]
		public void TestExitApplicationUnsafe_Exception_NoDatabaseUpgradeThrownTwiceReport()
		{
			AssertExitApplicationUnsafe_NoDatabaseUpgradeThrownTwiceReport(hasException: true);
		}

		void AssertExitApplicationUnsafe_NoDatabaseUpgradeThrownTwiceReport(bool hasException)
		{
			RegistryItemDictionary.Instance.PurgeAll();

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(bumpedSchemaVersion, 0));

			var eventLoggerMock = new Mock<IEventLogger>();
			var applicationExitProxyMock = new Mock<IApplicationExitProxy>();

			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

			using (var dbEnv = new DbEnvironmentWithMockGuiPlugin())
			using (ObjectFactory.Substitute(versionMock.Object))
			{
				try
				{
					// Arrange
					var message = "test";
					var exception = new InvalidOperationException();

					// Act
					var applicationEmergencyExit = new ApplicationEmergencyExit(eventLoggerMock.Object,
						applicationExitProxyMock.Object, Mock.Of<IHostLogger>(), errorReporterProxyMock.Object);
					if (hasException)
					{
						applicationEmergencyExit.ExitApplicationUnsafe(message, exception);
					}
					else
					{
						applicationEmergencyExit.ExitApplicationUnsafe(message);
					}

					// Assert
					NUnit.Framework.Assert.DoesNotThrow(() =>
					{
						Mock.Get(dbEnv.ConnectionGuiPlugin)
							.Verify(gp => gp.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()),
								Times.Never);
						if (hasException)
						{
							errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(message, exception),
								Times.Once);
							errorReporterProxyMock.VerifyNoOtherCalls();
						}
						else
						{
							errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(message, exception),
								Times.Never);
						}
					});
				}
				finally
				{
					Db.EnableThreadSchemaVersionCheckPermanently_ForTest();
				}
			}
		}

		public class ExitApplicationUnsafeOverloadWithException : TestCase
		{
			[ExpectNoExceptions]
			public void TestReportsExceptionToErrorReporterAndWriteEventAndHostLogs()
			{
				Test(new ProcessControllerConfigurationException("SomeReason"), "ExitApplicationDoesNotReportException", "ExitApplicationDoesNotReportException");
				Test(new HttpListenerException(123), "ExitApplicationDoesNotReportException1", "ExitApplicationDoesNotReportException1");
				Test(new Exception(), "ExitApplicationDoesNotReportException123123", "ExitApplicationDoesNotReportException123123");
				Test(new AggregateException(
					new ProcessControllerConfigurationException("SomeReason"),
					new InvalidOperationException(),
					new Exception()), "something", "something\r\nSomeReason\r\nOperation is not valid due to the current state of the object.\r\nException of type 'System.Exception' was thrown.");

				void Test(Exception ex, string message, string expectedErrorMessage)
				{
					// Arrange
					errorReporterProxyMock.Reset();
					errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(message, ex));
					eventLoggerMock.Reset();
					applicationExitProxyMock.Reset();
					hostLoggerMock.Invocations.Clear();
					eventLoggerMock.Invocations.Clear();

					// Act
					applicationEmergencyExit.ExitApplicationUnsafe(message, ex);

					// Assert
					NUnit.Framework.Assert.DoesNotThrow(() =>
					{
						applicationExitProxyMock.Verify(proxy => proxy.Exit(It.IsAny<int>()), Times.Once);
						applicationExitProxyMock.Verify(proxy => proxy.Exit(-1), Times.Once);
						errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(message, ex), Times.Once);
						errorReporterProxyMock.VerifyNoOtherCalls();
						eventLoggerMock.Verify(logger => logger.Log(LogLevel.Error, message, ex), Times.Once);
						eventLoggerMock.VerifyNoOtherCalls();
						hostLoggerMock.Verify(o => o.Log(LogLevel.Error, message, ex), Times.Once);
						hostLoggerMock.VerifyNoOtherCalls();
					});
				}
			}

			[ExpectNoExceptions]
			public void TestExitsApplicationInCaseOfExceptionFromErrorReporter()
			{
				Test<InvalidOperationException>();
				Test<Exception>();
				Test<AggregateException>();

				void Test<T>() where T : Exception, new()
				{
					// Arrange
					errorReporterProxyMock.Reset();
					eventLoggerMock.Reset();
					applicationExitProxyMock.Reset();

					errorReporterProxyMock.Setup(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()))
						.Throws<T>();

					// Act
					applicationEmergencyExit.ExitApplicationUnsafe("message", new Exception());

					// Assert
					NUnit.Framework.Assert.DoesNotThrow(() =>
					{
						applicationExitProxyMock.Verify(proxy => proxy.Exit(It.IsAny<int>()), Times.Once);
						applicationExitProxyMock.Verify(proxy => proxy.Exit(-1), Times.Once);
					});
				}
			}

			protected override void SetUp()
			{
				eventLoggerMock = new Mock<IEventLogger>();
				hostLoggerMock = new Mock<IHostLogger>();
				applicationExitProxyMock = new Mock<IApplicationExitProxy>();
				errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				applicationEmergencyExit = new ApplicationEmergencyExit(eventLoggerMock.Object, applicationExitProxyMock.Object, hostLoggerMock.Object, errorReporterProxyMock.Object);
			}

			ApplicationEmergencyExit applicationEmergencyExit;
			Mock<IApplicationExitProxy> applicationExitProxyMock;
			Mock<IEventLogger> eventLoggerMock;
			Mock<IHostLogger> hostLoggerMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
		}

		public class ExitApplicationUnsafeOverloadWithMessage : TestCase
		{
			[ExpectNoExceptions]
			public void TestLogsMessageToEventAndHostLog()
			{
				Test("ExitApplicationDoesNotReportException");
				Test("ExitApplicationDoesNotReportException1");
				Test("ExitApplicationDoesNotReportException123123");
				Test("something");

				void Test(string message)
				{
					// Arrange
					errorReporterProxyMock.Reset();
					eventLoggerMock.Reset();
					applicationExitProxyMock.Reset();
					hostLoggerMock.Invocations.Clear();

					// Act
					applicationEmergencyExit.ExitApplicationUnsafe(message);

					// Assert
					NUnit.Framework.Assert.DoesNotThrow(() =>
					{
						applicationExitProxyMock.Verify(proxy => proxy.Exit(It.IsAny<int>()), Times.Once);
						applicationExitProxyMock.Verify(proxy => proxy.Exit(-1), Times.Once);
						errorReporterProxyMock.Verify(reporter => reporter.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
						eventLoggerMock.Verify(logger => logger.Log(LogLevel.Warning, message), Times.Once);
						hostLoggerMock.Verify(x => x.Log(LogLevel.Warning, message), Times.Once);
						hostLoggerMock.VerifyNoOtherCalls();
					});
				}
			}

			[ExpectNoExceptions]
			public void TestExitsApplicationInCaseOfExceptionFromEventLog()
			{
				Test<InvalidOperationException>();
				Test<Exception>();
				Test<AggregateException>();

				void Test<T>() where T : Exception, new()
				{
					// Arrange
					errorReporterProxyMock.Reset();
					eventLoggerMock.Reset();
					applicationExitProxyMock.Reset();

					eventLoggerMock.Setup(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
						.Throws<T>();

					// Act
					applicationEmergencyExit.ExitApplicationUnsafe("message", new Exception());

					// Assert
					NUnit.Framework.Assert.DoesNotThrow(() =>
					{
						applicationExitProxyMock.Verify(proxy => proxy.Exit(It.IsAny<int>()), Times.Once);
						applicationExitProxyMock.Verify(proxy => proxy.Exit(-1), Times.Once);
					});
				}
			}

			protected override void SetUp()
			{
				eventLoggerMock = new Mock<IEventLogger>();
				hostLoggerMock = new Mock<IHostLogger>();
				applicationExitProxyMock = new Mock<IApplicationExitProxy>();
				errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				applicationEmergencyExit = new ApplicationEmergencyExit(eventLoggerMock.Object, applicationExitProxyMock.Object, hostLoggerMock.Object, errorReporterProxyMock.Object);
			}

			ApplicationEmergencyExit applicationEmergencyExit;
			Mock<IApplicationExitProxy> applicationExitProxyMock;
			Mock<IEventLogger> eventLoggerMock;
			Mock<IHostLogger> hostLoggerMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
		}

		class DbEnvironmentWithMockGuiPlugin : BaseDbEnvironment, IDisposable
		{
			public DbEnvironmentWithMockGuiPlugin()
			{
				existingEnv = DbEnv.Instance;
				DbEnv.SetDbEnvironment(this);
			}

			public override IDbConnectionGuiPlugin ConnectionGuiPlugin => connectionGuiPlugin;
			readonly IDbConnectionGuiPlugin connectionGuiPlugin = Mock.Of<IDbConnectionGuiPlugin>();

			public void Dispose() => DbEnv.SetDbEnvironment(existingEnv);
			readonly IDbEnvironment existingEnv;
		}
	}
}
