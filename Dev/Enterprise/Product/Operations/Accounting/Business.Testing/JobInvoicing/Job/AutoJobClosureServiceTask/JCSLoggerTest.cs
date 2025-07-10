using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Integration;
using Moq;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JCSLoggerTest : TestCaseWithFactory
	{
		public void TestLogInformation()
		{
			var logText = "test log information";
			var logger = new LoggerForTesting();
			var jcsLogger = new JCSLogger(logger) as IJCSLogger;
			jcsLogger.LogInformation(logText);
			AssertEquals(logText, logger.ToString());
			AssertEquals(logText, logger.InformationEventList.First());
		}

		public void TestLogError()
		{
			var logText = "test log error";
			var logger = new LoggerForTesting();
			var jcsLogger = new JCSLogger(logger) as IJCSLogger;
			jcsLogger.LogError(logText);
			AssertEquals(logText, logger.ToString());
			AssertEquals(logText, logger.ErrorEventList.First());
		}

		public void TestLogDebug()
		{
			var logText = "test log debug";
			var logger = new LoggerForTesting();
			var jcsLogger = new JCSLogger(logger) as IJCSLogger;
			jcsLogger.LogDebug(logText);
			AssertEquals(logText, logger.ToString());
			AssertEquals(logText, logger.DebugEventList.First());
		}

		public void TestLogPrefix()
		{
			var testPrefix = "test prefix:";
			var logText = "test log information";
			var logger = new LoggerForTesting();
			var jcsLogger = new JCSLogger(logger) as IJCSLogger;
			using (jcsLogger.SetLogPrefix(testPrefix))
			{
				jcsLogger.LogInformation(logText);
			}
			AssertEquals(testPrefix + logText, logger.ToString());
		}

		public void TestDiagnosticLogger()
		{
			foreach (var (logFunction, shouldBeLogged) in new (Action<IJCSLogger, string>, bool)[]
																{ ((logger, m) => logger.LogInformation(m), true)
																, ((logger, m) => logger.LogDiagnostic(m), true)
																, ((logger, m) => logger.LogDebug(m), false)
																, ((logger, m) => logger.LogError(m), true) })
			{
				var mockNotIDiagnosticLogger = new Mock<IDiagnosticLogger>();
				var jcsLogger = new JCSLogger(mockNotIDiagnosticLogger.Object) as IJCSLogger;
				logFunction.Invoke(jcsLogger, "test log message");
				mockNotIDiagnosticLogger.Verify((l) => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), shouldBeLogged ? Times.Once : Times.Never);
				Assert("To avoid empty test failure", true);
			}
		}
	}
}
