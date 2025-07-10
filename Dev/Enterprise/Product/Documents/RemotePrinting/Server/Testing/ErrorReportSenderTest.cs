using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class ErrorReportSenderTest : TestCaseWithFactory
	{
		public void TestReportOnceIssue()
		{
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			ExceptionReporter.SuppressGui();
			var errorCount = ErrorReporter.TotalErrorCount;

			try
			{
				throw new ArgumentException("A Bad Argument");
			}
			catch (Exception e)
			{
				ErrorReportSender.TrySend("A Web Print Error", e);
			}

			AssertContains("A Bad Argument", ErrorReporter.LastExceptionReported.ToString());
			AssertEquals(errorCount + 1, ErrorReporter.TotalErrorCount);
		}
	}
}
