using System;
using CargoWise.Common;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Enterprise.Licensing.Billing.Business.Testing
{
	public class ErrorReporterLoggerTest : TransactionedTestCase
	{
		public void TestLog()
		{
			var logger = new ErrorReporterLogger<ErrorReporterLoggerTest>("TestContext");
			var exception = new ArgumentNullException();
			logger.Log(LogLevel.Error, new EventId(1), "Test message", exception, (state, exception) => state);
			AssertEquals("TestContext", ErrorReporter.LastKeyReported);
			AssertEquals("Test message", ErrorReporter.LastMessageReported);
			AssertEquals(exception, ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}
	}
}
