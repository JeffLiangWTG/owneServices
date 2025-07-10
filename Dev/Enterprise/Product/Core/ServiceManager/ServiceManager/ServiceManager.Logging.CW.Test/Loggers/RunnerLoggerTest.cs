using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing.Logging;
using Moq;
using NLog;
using NLog.Config;
using NUnit.Framework;
using ServiceManager.Logging.CW;
using ServiceManager.Logging.CW.Test;
using ServiceManager.Runner.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CargoWise.ServiceManager.Runner.Test.Loggers
{
	class RunnerLoggerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			processInfoMock = new Mock<IProcessInfo>();

			loggerMock = new Mock<ILogger>();
			runnerLogger = new RunnerLogger(new Lazy<ILogger>(() => loggerMock.Object));
		}

		public void TestRunnerLoggerDoesNotArchiveHostLogFiles()
		{
			// Arrange
			using (var tempDirectory = new TempDirectory())
			using (new DisposableAction(() => LogManager.Shutdown()))
			{
				LogManager.Configuration = new LoggingConfiguration();
				var testRunnerLogger = new RunnerLogger(
					new Lazy<ILogger>(
						() => new LoggerNLogWrapper(
							Db.ServerName,
							Db.DatabaseName,
							ServiceManagerHelper.HostLoggerCode,
							tempDirectory.DirectoryName,
							archiveLogFiles: false)));
				const string logFilePattern = "HOST_????????*.txt";

				// Act
				LoggerTestHelper.LogDummyDataToTriggerLogArchive(testRunnerLogger);

				// Assert
				AssertEquals(1, Directory.GetFiles(tempDirectory.DirectoryName, logFilePattern).Length);
			}
		}

		public void TestLog()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Warning, "abc1", "message 1");
				Test(LogLevel.Debug, "abc2", "message 2");
				Test(LogLevel.Error, "abc3", "message 3");
				Test(LogLevel.Information, "abc4", "message 4");
			});

			void Test(LogLevel logLevel, string processId, string message)
			{
				// Arrange
				loggerMock.Reset();
				processInfoMock.Setup(pi => pi.ProcessId).Returns(processId);

				// Act
				runnerLogger.Log(logLevel, message);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLogException<ILogger, Exception>(o => o != null, Times.Never);
					loggerMock.VerifyLogLevel(logLevel, Times.Once);
					loggerMock.VerifyLog(logLevel, o => o.Contains(message), Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => (string)kvp.Single(k => k.Key == "exe").Value == "Runner")), Times.Once);
				});
			}
		}

		public void TestLogWithCommandInfo()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Debug, "abc1", "message 1", "command to string 1");
				Test(LogLevel.Error, "abc2", "message 2", "command to string 2");
			});

			void Test(LogLevel logLevel, string processId, string message, string commandToString)
			{
				// Arrange
				loggerMock.Reset();
				processInfoMock.Setup(pi => pi.ProcessId).Returns(processId);
				var commandInfo = new CommandInfo(commandToString);

				// Act
				runnerLogger.Log(logLevel, message, commandInfo);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLogException<ILogger, Exception>(o => o != null, Times.Never);
					loggerMock.VerifyLogLevel(logLevel, Times.Once);
					loggerMock.VerifyLog(
							logLevel,
							s => s.Contains(message)
								&& s.Contains(commandToString),
						Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => (string)kvp.Single(k => k.Key == "exe").Value == "Runner")), Times.Once);
				});
			}
		}

		public void TestLogWithException()
		{
			CombineAssertions(() =>
			{
				Test(LogLevel.Information, "abc1", "message 1", new StackOverflowException("Congratulations! You have won a stack overflow..."));
				Test(LogLevel.Error, "abc2", "meow", new Exception());
			});

			void Test(LogLevel logLevel, string processId, string message, Exception ex)
			{
				// Arrange
				loggerMock.Reset();
				processInfoMock.Setup(pi => pi.ProcessId).Returns(processId);

				// Act
				runnerLogger.Log(logLevel, message, ex);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.VerifyLogException<ILogger, Exception>(o => o != null, Times.Once);
					loggerMock.Verify(l => l.BeginScope(It.Is<IReadOnlyCollection<KeyValuePair<string, object>>>(kvp => (string)kvp.Single(k => k.Key == "exe").Value == "Runner")), Times.Once);
					loggerMock.VerifyLog(
						logLevel,
						s => s.Contains(message)
						, ex, Times.Once);
				});
			}
		}

		public void TestWrongParameters()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new RunnerLogger(null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => runnerLogger.Log(LogLevel.Debug, "abc", (ICommandInfo)null));
				AssertEquals("commandInfo", result.ParamName);
			});
		}

		Mock<ILogger> loggerMock;
		Mock<IProcessInfo> processInfoMock;
		RunnerLogger runnerLogger;

		class CommandInfo : ICommandInfo
		{
			public CommandInfo(string toStringMessage)
			{
				this.toStringMessage = toStringMessage;
			}

			public override string ToString()
			{
				return toStringMessage;
			}

			public string FormatRequestToLogMessage(RunnerLogMessageStage logMessageStage, params object[] values)
			{
				throw new NotImplementedException();
			}

			readonly string toStringMessage;

			public Guid Id => throw new NotImplementedException();
		}
	}
}
