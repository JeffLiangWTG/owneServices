using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Product.DataLoad.Helper;
using CargoWise.Data;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.Bi.Product.DataLoad.Testing.Helper
{
	internal class SqlMessagePassthroughLoggerTest : TestCase
	{
		public void TestPassesThroughToLogger()
		{
			var script = @"
RAISERROR ('this is a message', 1, 1) WITH NOWAIT;
SELECT 1;
";
			var logger = new LoggerForTest();
			using (new SqlMessagePassthroughLogger(Db.Connection, logger))
			{
				Db.Connection.ExecuteScalar<int>(script);
			}

			AssertArrayEqualsByElements(logger.LogEntries.ToArray(), new string[] { "this is a message" });
		}

		public void TestLogsBeforeCompletion()
		{
			var script = @"
RAISERROR ('message before', 1, 1) WITH NOWAIT;
WAITFOR DELAY '00:00:3';
RAISERROR ('message after', 1, 2) WITH NOWAIT;
SELECT 1;
";
			var logTimes = new List<DateTime>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(l => l.Log(LogType.Information, It.IsAny<string>())).Callback(() => logTimes.Add(DateTime.Now));

			using (new SqlMessagePassthroughLogger(Db.Connection, mockLogger.Object))
			{
				Db.Connection.ExecuteScalar<int>(script);
			}

			AssertEquals(2, logTimes.Count);
			var diff = logTimes[1] - logTimes[0];
			AssertGreaterThanOrEqualTo(diff, TimeSpan.FromSeconds(2));
		}

		public void TestLogSeverityDefault()
		{
			var script = @"
RAISERROR ('a', 0, 1) WITH NOWAIT; -- debug
RAISERROR ('aa', 1, 1) WITH NOWAIT; -- info
RAISERROR ('aaah', 2, 1) WITH NOWAIT; -- warning
RAISERROR ('aaaahhhh', 3, 1) WITH NOWAIT; -- error
RAISERROR ('aaaaauuughhh', 4, 1) WITH NOWAIT; -- invalid (debug)
PRINT 'asdfasdf' -- debug
SELECT 1;
";
			var logLevels = new List<LogType>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((level, msg) => logLevels.Add(level));

			using (new SqlMessagePassthroughLogger(Db.Connection, mockLogger.Object))
			{
				Db.Connection.ExecuteScalar<int>(script);
			}

			AssertEquals(6, logLevels.Count);
			var expectedLogLevels = new LogType[] { LogType.Debug, LogType.Information, LogType.Warning, LogType.Error, LogType.Debug, LogType.Debug };
			AssertArrayEqualsByElements(expectedLogLevels, logLevels.ToArray());
		}

		public void TestUsesSeverityMapper()
		{
			var script = @"
RAISERROR ('a', 0, 1) WITH NOWAIT; -- debug
RAISERROR ('aa', 1, 1) WITH NOWAIT; -- info
RAISERROR ('aaah', 2, 1) WITH NOWAIT; -- warning
RAISERROR ('aaaahhhh', 3, 1) WITH NOWAIT; -- error
RAISERROR ('aaaaauuughhh', 4, 1) WITH NOWAIT; -- invalid (debug)
PRINT 'asdfasdf' -- debug
SELECT 1;
";
			var logLevels = new List<LogType>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback<LogType, string>((level, msg) => logLevels.Add(level));

			using (new SqlMessagePassthroughLogger(Db.Connection, mockLogger.Object, _ => LogType.Information))
			{
				Db.Connection.ExecuteScalar<int>(script);
			}

			AssertEquals(6, logLevels.Count);
			var expectedLogLevels = new LogType[] { LogType.Information, LogType.Information, LogType.Information, LogType.Information, LogType.Information, LogType.Information };
			AssertArrayEqualsByElements(expectedLogLevels, logLevels.ToArray());
		}

		#region Behavior documentation

		[DeveloperOnlyTest]
		public void TestNowaitIsRequired()
		{
			// This is to document that messages get buffered if they are not called with NOWAIT
			var script = @"
RAISERROR ('message before', 1, 1);
WAITFOR DELAY '00:00:3';
RAISERROR ('message after', 1, 2);
SELECT 1;
";
			var logTimes = new List<DateTime>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(l => l.Log(LogType.Information, It.IsAny<string>())).Callback(() => logTimes.Add(DateTime.Now));
			using (new SqlMessagePassthroughLogger(Db.Connection, mockLogger.Object))
			{
				Db.Connection.ExecuteScalar<int>(script);
			}
			AssertEquals(logTimes.Count, 2);
			var diff = logTimes[1] - logTimes[0];
			AssertLessThanOrEqualTo(diff, TimeSpan.FromSeconds(1));
		}

		[DeveloperOnlyTest]
		public void TestNonQueryBuffersMessages()
		{
			// This is to document that using ExecuteNonQuery does not call the InfoMessage event until the entire batch is finished, this is a limitation with System.DbConnection
			var script = @"
RAISERROR ('message before', 1, 1) WITH NOWAIT;
WAITFOR DELAY '00:00:3';
RAISERROR ('message after', 1, 2) WITH NOWAIT;
SELECT 1;
";
			var logTimes = new List<DateTime>();
			var mockLogger = new Mock<ILogger>();
			mockLogger.Setup(l => l.Log(LogType.Information, It.IsAny<string>())).Callback(() => logTimes.Add(DateTime.Now));
			using (new SqlMessagePassthroughLogger(Db.Connection, mockLogger.Object))
			{
				Db.Connection.ExecuteNonQuery(script);
			}
			AssertEquals(logTimes.Count, 2);
			var diff = logTimes[1] - logTimes[0];
			AssertLessThanOrEqualTo(diff, TimeSpan.FromSeconds(1));
		}

		#endregion
	}
}
