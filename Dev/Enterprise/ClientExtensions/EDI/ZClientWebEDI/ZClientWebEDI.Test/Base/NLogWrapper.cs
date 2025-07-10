using System;
using System.Net;
using System.Text;
using NLog;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class NLogWrapperrTest : TestCase
	{
		public void TestLog()
		{
			var logger = new NLogWrapper(typeof(NLogWrapperrTest));
			var sessionId = Guid.NewGuid().ToString();
			logger.AddLog(LogLevel.Info, "This is an info message.", ((int)HttpStatusCode.OK), sessionId);
			logger.AddLog(LogLevel.Warn, "This is a warning message.", ((int)HttpStatusCode.OK), sessionId);
			logger.AddLog(LogLevel.Error, "This is an error message.", ((int)HttpStatusCode.OK), sessionId);
			logger.AddLog(LogLevel.Debug, "This is a debug message.", ((int)HttpStatusCode.OK), sessionId);

			var expectedLogMessages = $@"Info | NLogWrapperrTest: This is an info message. 200 | {sessionId}
Warn | NLogWrapperrTest: This is a warning message. 200 | {sessionId}
Error | NLogWrapperrTest: This is an error message. 200 | {sessionId}
Debug | NLogWrapperrTest: This is a debug message. 200 | {sessionId}";
			AssertLogMessages(expectedLogMessages);
		}

		void AssertLogMessages(string expectedLogMessages)
		{
			var builder = new StringBuilder();
			foreach (string log in memoryTarget.Logs)
			{
				builder.AppendLine(log);
			}
			var actualMessages = builder.ToString().TrimEnd('\r', '\n');
			AssertEquals(expectedLogMessages, actualMessages);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var config = new NLog.Config.LoggingConfiguration();
			memoryTarget = new NLog.Targets.MemoryTarget();
			memoryTarget.Layout = "${level} | ${logger}: ${message} ${event-properties:status_code} | ${event-properties:session_id}";
			config.AddRuleForAllLevels(memoryTarget);
			LogManager.Configuration = config;
		}

		NLog.Targets.MemoryTarget memoryTarget;
	}
}
