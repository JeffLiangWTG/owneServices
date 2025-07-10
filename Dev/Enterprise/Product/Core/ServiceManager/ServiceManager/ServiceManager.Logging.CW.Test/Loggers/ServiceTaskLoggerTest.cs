using System;
using System.IO;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing.Logging;
using Moq;
using NLog;
using NLog.Config;
using NUnit.Framework;
using ServiceManager.Logging.CW;
using ServiceManager.Runner.Abstractions;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CargoWise.ServiceManager.Runner.Test.Loggers
{
	class ServiceTaskLoggerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerFactoryMock = new Mock<ILoggerFactory>();
			runnerLoggerMock = new Mock<IRunnerLogger>();

			serviceTaskLogger = new ServiceTaskLogger(runnerLoggerMock.Object, loggerFactoryMock.Object);
		}

		public void TestCreatesNewLogger()
		{
			CombineAssertions(() =>
			{
				Test("asd");
				Test("zxc");
			});

			void Test(string taskCode)
			{
				// Arrange
				loggerFactoryMock.Reset();

				// Act
				using (serviceTaskLogger.SetTaskLoggerCode(taskCode))
				{
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerFactoryMock.Verify(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
					loggerFactoryMock.Verify(factory => factory.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, taskCode, null), Times.Once);
				});
			}
		}

		public void TestDoubleTaskLogger()
		{
			CombineAssertions(() =>
			{
				Test("asd");
				Test("zxc");
			});

			void Test(string secondTaskCode)
			{
				loggerFactoryMock
					.Setup(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(new Mock<Enterprise.Integration.ILogger>().Object);

				using (serviceTaskLogger.SetTaskLoggerCode("asd"))
				{
					AssertExceptionThrown<InvalidOperationException>(() => serviceTaskLogger.SetTaskLoggerCode(secondTaskCode));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLoggersAreNotInstantiatedInConstructor()
		{
			loggerFactoryMock.Verify(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestLogsExceptionsToTaskLogger()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Debug, "message1", new Exception());
				Test(LogLevel.Information, "message2", new IndexOutOfRangeException());
				Test(LogLevel.Error, "message3", new InvalidOperationException());
				Test(LogLevel.Warning, "message4", new AccessViolationException());
			});

			void Test(LogLevel logLevel, string message, Exception exception)
			{
				// Arrange
				var taskLoggerMock = new Mock<Enterprise.Integration.ILogger>();
				loggerFactoryMock
					.Setup(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(taskLoggerMock.Object);

				// Act
				using (serviceTaskLogger.SetTaskLoggerCode("asd"))
				{
					serviceTaskLogger.Log(logLevel, message, exception);
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					taskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
					taskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
					taskLoggerMock.Verify(logger => logger.Log(logLevel.ToLogType(), message, exception), Times.Once);
				});
			}
		}

		public void TestLogsExceptionsToRunnerLoggerIfNoTask()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Debug, "message1", new Exception());
				Test(LogLevel.Information, "message2", new IndexOutOfRangeException());
				Test(LogLevel.Error, "message3", new InvalidOperationException());
				Test(LogLevel.Warning, "message4", new AccessViolationException());
			});

			void Test(LogLevel logType, string message, Exception exception)
			{
				// Arrange
				runnerLoggerMock.Reset();

				// Act
				serviceTaskLogger.Log(logType, message, exception);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					runnerLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
					runnerLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Once);
					runnerLoggerMock.Verify(logger => logger.Log(logType, message, exception), Times.Once);
					runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<string>()), Times.Once);
					loggerFactoryMock.Verify(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
				});
			}
		}

		public void TestLogsMessagesToTaskLogger()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Debug, "message1");
				Test(LogLevel.Information, "message2");
				Test(LogLevel.Error, "message3");
				Test(LogLevel.Warning, "message4");
			});

			void Test(LogLevel logLevel, string message)
			{
				// Arrange
				var taskLoggerMock = new Mock<Enterprise.Integration.ILogger>();
				loggerFactoryMock
					.Setup(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
					.Returns(taskLoggerMock.Object);

				// Act
				using (serviceTaskLogger.SetTaskLoggerCode("asd"))
				{
					serviceTaskLogger.Log(logLevel, message);
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					taskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
					taskLoggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
					taskLoggerMock.Verify(logger => logger.Log(logLevel.ToLogType(), message), Times.Once);
				});
			}
		}

		public void TestLogsMessagesToRunnerLoggerIfNoTask()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Debug, "message1");
				Test(LogLevel.Information, "message2");
				Test(LogLevel.Error, "message3");
				Test(LogLevel.Warning, "message4");
			});

			void Test(LogLevel logLevel, string message)
			{
				// Arrange
				runnerLoggerMock.Reset();

				// Act
				serviceTaskLogger.Log(logLevel, message);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					runnerLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
					runnerLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Exactly(2));
					runnerLoggerMock.Verify(logger => logger.Log(logLevel, message), Times.Once);
					runnerLoggerMock.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<string>()), Times.AtLeastOnce);
					loggerFactoryMock.Verify(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
				});
			}
		}

		public void TestTaskLoggerIsActive()
		{
			loggerFactoryMock
				.Setup(factory => factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new Mock<Enterprise.Integration.ILogger>().Object);

			CombineAssertions(() =>
			{
				AssertEquals(false, serviceTaskLogger.TaskLoggerIsActive);

				using (serviceTaskLogger.SetTaskLoggerCode("asd"))
				{
					AssertEquals(true, serviceTaskLogger.TaskLoggerIsActive);
				}

				AssertEquals(false, serviceTaskLogger.TaskLoggerIsActive);
			});
		}

		Mock<ILoggerFactory> loggerFactoryMock;
		Mock<IRunnerLogger> runnerLoggerMock;
		ServiceTaskLogger serviceTaskLogger;

		public class IntegrationTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				runnerLoggerMock = new Mock<RunnerLogger>() { CallBase = true };
				loggerFactoryMock = new Mock<LoggerFactory> { CallBase = true };
				loggerFactoryMock.As<ILoggerFactory>();
				serviceTaskLogger = new ServiceTaskLogger(runnerLoggerMock.Object, loggerFactoryMock.Object);
			}

			[RequiresSoftware(RequiredSoftware.IsVM)]
			public void TestArchivesLogFilesForServiceTask()
			{
				// Arrange
				var directoryPath = ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
				using (LoggerTestHelper.CleanupCurrentLogFolderThenStopLoggerAndCleanupLeftoversOnDispose(directoryPath))
				{
					LogManager.Configuration = new LoggingConfiguration();
					const string logFilePattern = "TSK_????????*.txt";

					serviceTaskLogger.Log(LogLevel.Information, "Something to trigger Runner logger initialization");
					using (serviceTaskLogger.SetTaskLoggerCode("TSK"))
					{
						// Act
						LoggerTestHelper.LogDummyDataToTriggerLogArchive(serviceTaskLogger);

						// Assert
						CombineAssertions(() =>
						{
							AssertGreaterThan(Directory.GetFiles(directoryPath, logFilePattern).Length, 1);
							AssertNoExceptionThrown(() => loggerFactoryMock.As<ILoggerFactory>().Verify(factory => factory.NewServiceTaskLogger(Db.ServerName, Db.DatabaseName, "TSK", null), Times.Once));
							AssertNoExceptionThrown(() => loggerFactoryMock.As<ILoggerFactory>().VerifyNoOtherCalls());
						});
					}
				}
			}

			[RequiresSoftware(RequiredSoftware.IsVM)]
			public void TestDoesNotArchiveLogFilesForRunner()
			{
				// Arrange
				var directoryPath = ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
				using (LoggerTestHelper.CleanupCurrentLogFolderThenStopLoggerAndCleanupLeftoversOnDispose(directoryPath))
				{
					LogManager.Configuration = new LoggingConfiguration();
					const string logFilePattern = "HOST_????????*.txt";

					serviceTaskLogger.Log(LogLevel.Information, "Something to trigger Runner logger initialization");
					// Act
					LoggerTestHelper.LogDummyDataToTriggerLogArchive(serviceTaskLogger);

					// Assert
					CombineAssertions(() =>
					{
						AssertEquals(1, Directory.GetFiles(directoryPath, logFilePattern).Length);
						AssertNoExceptionThrown(() => loggerFactoryMock.VerifyNoOtherCalls());
					});
				}
			}

			Mock<LoggerFactory> loggerFactoryMock;
			Mock<RunnerLogger> runnerLoggerMock;
			ServiceTaskLogger serviceTaskLogger;
		}
	}
}
