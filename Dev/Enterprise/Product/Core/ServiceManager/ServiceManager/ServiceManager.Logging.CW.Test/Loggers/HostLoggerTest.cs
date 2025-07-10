using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NLog.Config;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Logging.CW;
using ServiceManager.Logging.CW.Test;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.Logger
{
	class HostLoggerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLogRunnableServiceTaskWithMessage()
		{
			// Arrange
			var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
			runnableServiceTaskMock
				.Setup(x => x.Code)
				.Returns(hostedServiceConfigMock.Object.Code);
			runnableServiceTaskMock
				.Setup(x => x.Info)
				.Returns(new ServiceTaskInfo(hostedServiceConfigMock.Object));

			CombineAssertions(() =>
			{
				foreach (var logLevel in Source.LogLevelSource)
				{
					foreach (var taskCode in Source.CodeSource)
					{
						hostedServiceConfigMock.Setup(x => x.Code).Returns(taskCode);

						foreach (var message in Source.MessageSource)
						{
							Test(logLevel, null, message, message);
							Test(logLevel, runnableServiceTaskMock.Object, message, $"{taskCode}: {message} ({TestTaskDescription})");
						}
					}
				}
			});

			void Test(LogLevel logLevel, IRunnableServiceTask task, string message, string expectedMessage)
			{
				loggerMock.Invocations.Clear();

				// Act
				hostLogger.Log(logLevel, task?.Info?.HostedServiceAttribute, message);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLog(logLevel, expectedMessage, null, Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => kvp.Single().Key == "exe" && (string)kvp.Single().Value == "Host")), Times.Once);
					loggerMock.VerifyNoOtherCalls();
				});
			}
		}

		[ExpectNoExceptions]
		public void TestLogRunnableServiceTaskWithMessageAndException()
		{
			// Arrange
			var runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
			runnableServiceTaskMock
				.Setup(x => x.Code)
				.Returns(hostedServiceConfigMock.Object.Code);
			runnableServiceTaskMock
				.Setup(x => x.Info)
				.Returns(new ServiceTaskInfo(hostedServiceConfigMock.Object));

			var exceptions = new[] { new Exception(), new InvalidOperationException() };

			CombineAssertions(() =>
			{
				foreach (var logLevel in Source.LogLevelSource)
				{
					foreach (var taskCode in Source.CodeSource)
					{
						hostedServiceConfigMock.Setup(x => x.Code).Returns(taskCode);

						foreach (var message in Source.MessageSource)
						{
							foreach (var exception in exceptions)
							{
								Test(logLevel, null, message, exception, message);
								Test(logLevel, runnableServiceTaskMock.Object, message, exception, $"{taskCode}: {message} ({TestTaskDescription})");
							}
						}
					}
				}
			});

			void Test(LogLevel logLevel, IRunnableServiceTask task, string message, Exception exception, string expectedMessage)
			{
				loggerMock.Invocations.Clear();

				// Act
				hostLogger.Log(logLevel, task?.Info?.HostedServiceAttribute, message, exception);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLog(logLevel, expectedMessage, exception, Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => kvp.Single().Key == "exe" && (string)kvp.Single().Value == "Host")), Times.Once);
					loggerMock.VerifyNoOtherCalls();
				});
			}
		}

		[ExpectNoExceptions]
		public void TestLogServiceConfigWithMessage()
		{
			// Arrange
			CombineAssertions(() =>
			{
				foreach (var logLevel in Source.LogLevelSource)
				{
					foreach (var taskCode in Source.CodeSource)
					{
						hostedServiceConfigMock.Setup(x => x.Code).Returns(taskCode);

						foreach (var message in Source.MessageSource)
						{
							Test(logLevel, null, message, message);
							Test(logLevel, hostedServiceConfigMock.Object, message, $"{taskCode}: {message} ({TestTaskDescription})");
						}
					}
				}
			});

			void Test(LogLevel logLevel, IHostedServiceAttribute config, string message, string expectedMessage)
			{
				loggerMock.Invocations.Clear();

				// Act
				hostLogger.Log(logLevel, config, message);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLog(logLevel, expectedMessage, null, Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => kvp.Single().Key == "exe" && (string)kvp.Single().Value == "Host")), Times.Once);
					loggerMock.VerifyNoOtherCalls();
				});
			}
		}

		[ExpectNoExceptions]
		public void TestLogServiceConfigWithProcessIdAndMessage()
		{
			// Arrange
			const int testProcessId = 1234;

			CombineAssertions(() =>
			{
				foreach (var logLevel in Source.LogLevelSource)
				{
					foreach (var taskCode in Source.CodeSource)
					{
						hostedServiceConfigMock.Setup(x => x.Code).Returns(taskCode);

						foreach (var message in Source.MessageSource)
						{
							Test(logLevel, null, message, $"PID={testProcessId}: {message}");
							Test(logLevel, hostedServiceConfigMock.Object, message, $"{taskCode}: PID={testProcessId}: {message} ({TestTaskDescription})");
						}
					}
				}
			});

			void Test(LogLevel logLevel, IHostedServiceAttribute config, string message, string expectedMessage)
			{
				loggerMock.Invocations.Clear();

				// Act
				hostLogger.Log(logLevel, config, testProcessId, message);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLog(logLevel, expectedMessage, null, Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => kvp.Single().Key == "exe" && (string)kvp.Single().Value == "Host")), Times.Once);
					loggerMock.VerifyNoOtherCalls();
				});
			}
		}

		public void TestHostLoggerArchivesLogFiles()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			using (new DisposableAction(() => NLog.LogManager.Shutdown()))
			{
				NLog.LogManager.Configuration = new LoggingConfiguration();
				var logger = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, ServiceManagerHelper.HostLoggerCode, tempDirectory.DirectoryName);
				var testHostLogger = new Mock<HostLogger>(new Lazy<ILogger>(() => logger)) { CallBase = true };
				const string logFilePattern = "HOST_????????*.txt";

				// Act
				LoggerTestHelper.LogDummyDataToTriggerLogArchive(testHostLogger.Object);

				// Assert
				AssertGreaterThan(Directory.GetFiles(tempDirectory.DirectoryName, logFilePattern).Length, 1);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
			hostedServiceConfigMock.Setup(x => x.Description).Returns(TestTaskDescription);

			loggerMock = new Mock<ILogger>();
			hostLogger = new HostLogger(new Lazy<ILogger>(() => loggerMock.Object));
		}

		protected override void TearDown()
		{
			ObjectFactory.DisposeSubstitutions();
			base.TearDown();
		}

		const string TestTaskDescription = "test description 1";
		Mock<IHostedServiceAttribute> hostedServiceConfigMock;
		HostLogger hostLogger;
		Mock<ILogger> loggerMock;

		static class Source
		{
			public static IEnumerable<LogLevel> LogLevelSource { get; } = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().ToArray();
			public static IEnumerable<string> CodeSource { get; } = new[] { "code1", "code2" };
			public static IEnumerable<string> MessageSource { get; } = new[] { "message1", "message2" };
		}
	}
}
