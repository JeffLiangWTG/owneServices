using System;
using System.Linq;
using System.Threading;
using Enterprise.ServiceManager.Shared.Testing.Logging;
using Enterprise.ZArchitecture.Core;
using Moq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.CW;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Enterprise.ServiceManager.Shared.Testing
{
	public class EventLoggerTest : TestCase
	{
		public void TestConstructorWithNullConfigurationNoExceptions()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				LogManager.Configuration = null;

				// Act
				_ = CreateEventLogger();

				// Assert
				AssertNotNull(LogManager.Configuration);
			}
		}

		public void TestConstructorTargetsExist()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				_ = CreateEventLogger();

				// Assert
				var targets = LogManager.Configuration.AllTargets;

				AssertNotNull(nameof(EventLogTarget), targets.SingleOrDefault(o => o is EventLogTarget));
			}
		}

		public void TestConstructorWithMultipleCallsOnlyOneTargetsExist()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				_ = CreateEventLogger();

				// Act
				_ = CreateEventLogger();

				// Assert
				var targets = LogManager.Configuration.AllTargets;

				AssertNotNull(nameof(EventLogTarget), targets.SingleOrDefault(o => o is EventLogTarget));
			}
		}

		public void TestLoggingRuleMinLevelIsInfo()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				_ = CreateEventLogger();

				// Assert
				var rule = LogManager.Configuration.LoggingRules
					.ToList()
					.FirstOrDefault(o => o.Targets.OfType<EventLogTarget>().Any());

				AssertNotNull(rule);
				AssertEquals(NLog.LogLevel.Info, rule.Levels.Min());
			}
		}

		public void TestLogWithParametersExistsInLogMessage()
		{
			CombineAssertions(() =>
			{
				Test("database", "dbName");
				Test("database_host", "dbServer");
				Test("applicationPath", AppDomain.CurrentDomain.BaseDirectory);
				Test("processcontroller:objectpath=version", ReleaseInfo.Instance.VersionNumber.ToString());
			});

			void Test(string eventProperty, string expectedValue)
			{
				using (LoggerTestHelper.TemporaryLoggingConfiguration())
				using (var memoryTarget = new MemoryTarget(nameof(MemoryTarget)))
				{
					// Arrange
					memoryTarget.Layout = $"${{event-properties:{eventProperty}}}";

					LogManager.Configuration.AddTarget(memoryTarget);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, memoryTarget) { RuleName = nameof(MemoryTarget) });
					LogManager.ReconfigExistingLoggers();

					var logger = new EventLogger(Mock.Of<IServiceManagerHostOptions>(o => o.ServerName == "dbServer" && o.DatabaseName == "dbName"));

					// Assert
					logger.Log(LogLevel.Debug, "message");

					// Assert
					var logMessage = memoryTarget.Logs.FirstOrDefault();
					AssertEquals(expectedValue, logMessage);
				}
			}
		}

		[TestRequiresAdministrativePrivileges("EventLog")]
		public void TestLogWithErrorLogsToEventLog()
		{
			// Arrange
			using var manualResetEvent = new ManualResetEvent(false);
			using var eventLog = new System.Diagnostics.EventLog("Application");

			var eventLogMessageExists = false;
			var logId = Guid.NewGuid().ToString();

			eventLog.EnableRaisingEvents = true;
			eventLog.EntryWritten += (o, args) =>
			{
				eventLogMessageExists = eventLogMessageExists || args.Entry.Message.Contains(logId);
				manualResetEvent.Set();
			};

			var logger = CreateEventLogger();

			// Act
			logger.Log(LogLevel.Error, logId);
			LogManager.Flush();

			manualResetEvent.WaitOne(1000);

			// Assert
			Assert("Log to event log", eventLogMessageExists);

			eventLog.Close();
		}

		EventLogger CreateEventLogger()
		{
			return new EventLogger(Mock.Of<IServiceManagerHostOptions>(o => o.ServerName == "server" && o.DatabaseName == "database"));
		}
	}
}
