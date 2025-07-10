using System;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions
{
	public class WebExceptionReporter_WebExceptionReporterTest : TransactionedTestCase
	{
		public void TestGetNewExceptionEmailBuilder()
		{
			var testArgs = new ExceptionReportArgs(new Exception(), "Test Error Id", "Test Key", "Test Description");
			var testBuilder = TestReporter.GetNewExceptionReportBuilderForTesting(testArgs);
			AssertNotNull("Exception Report Builder should not be null", testBuilder);
			AssertType<WebExceptionReportBuilder>("WebExceptionReporter should return WebEmailBuilder", testBuilder);
		}

		protected WebExceptionReporter TestReporter
		{
			get
			{
				if (testReporter == null)
				{
					testReporter = new WebExceptionReporter();
				}
				return testReporter;
			}
		}
		WebExceptionReporter testReporter;
	}
}
