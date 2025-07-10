using System;
using System.Linq;
using Enterprise.Integration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TokenValidationLoggerTest : TestCase
	{
		public void TestIsEnabled()
		{
			var logger = new TokenValidationLogger(logger: null);

			AssertIsEnabled(LogLevel.Debug, expected: true);
			AssertIsEnabled(LogLevel.Information, expected: true);
			AssertIsEnabled(LogLevel.Warning, expected: true);
			AssertIsEnabled(LogLevel.Error, expected: true);
			AssertIsEnabled(LogLevel.Critical, expected: true);
			AssertIsEnabled(LogLevel.Trace, expected: false);
			AssertIsEnabled(LogLevel.None, expected: false);

			void AssertIsEnabled(LogLevel logLevel, bool expected)
			{
				var result = logger.IsEnabled(logLevel);
				AssertEquals(expected, result);
			}
		}

		public void TestLog()
		{
			var detailedLogger = new DetailedLoggerForTest();
			var tokenValidationLogger = new TokenValidationLogger(logger: detailedLogger);

			AssertThenClearLogs(LogLevel.Debug, "debugMsg", exception: null, expectedLog: Tuple.Create<LogType, string, Exception>(LogType.Debug, "debugMsg", null));
			AssertThenClearLogs(LogLevel.Information, "infoMsg", exception: null, expectedLog: Tuple.Create<LogType, string, Exception>(LogType.Information, "infoMsg", null));
			AssertThenClearLogs(LogLevel.Warning, "warningMsg", exception: null, expectedLog: Tuple.Create<LogType, string, Exception>(LogType.Warning, "warningMsg", null));
			AssertThenClearLogs(LogLevel.Error, "errorMsg", exception: null, expectedLog: Tuple.Create<LogType, string, Exception>(LogType.Error, "errorMsg", null));
			AssertThenClearLogs(LogLevel.Critical, "criticalMsg", exception: null, expectedLog: Tuple.Create<LogType, string, Exception>(LogType.Error, "criticalMsg", null));
			AssertThenClearLogs(LogLevel.Trace, "traceMsg", exception: null, expectedLog: null);
			AssertThenClearLogs(LogLevel.None, "noneMsg", exception: null, expectedLog: null);

			var testException = new Exception("exceptionMessage");
			AssertThenClearLogs(LogLevel.Debug, "debugMsg", testException, expectedLog: Tuple.Create(LogType.Debug, "debugMsg", testException));
			AssertThenClearLogs(LogLevel.Information, "infoMsg", testException, expectedLog: Tuple.Create(LogType.Information, "infoMsg", testException));
			AssertThenClearLogs(LogLevel.Warning, "warningMsg", testException, expectedLog: Tuple.Create(LogType.Warning, "warningMsg", testException));
			AssertThenClearLogs(LogLevel.Error, "errorMsg", testException, expectedLog: Tuple.Create(LogType.Error, "errorMsg", testException));
			AssertThenClearLogs(LogLevel.Critical, "criticalMsg", testException, expectedLog: Tuple.Create(LogType.Error, "criticalMsg", testException));
			AssertThenClearLogs(LogLevel.Trace, "traceMsg", testException, expectedLog: null);
			AssertThenClearLogs(LogLevel.None, "noneMsg", testException, expectedLog: null);

			void AssertThenClearLogs(LogLevel logLevel, string message, Exception exception, Tuple<LogType, string, Exception> expectedLog)
			{
				tokenValidationLogger.Log(logLevel, exception, message);

				AssertEquals(expectedLog, detailedLogger.Logs.FirstOrDefault());

				detailedLogger.Logs.Clear();
			}
		}
	}
}
