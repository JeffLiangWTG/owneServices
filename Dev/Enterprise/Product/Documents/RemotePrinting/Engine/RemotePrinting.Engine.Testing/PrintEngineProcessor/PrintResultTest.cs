using System;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	class PrintResultTest : PrintEngineTestCase
	{
		public void TestSuccess()
		{
			var jobPK = Guid.NewGuid();
			var success = PrintResult.Success(jobPK);
			AssertEquals(nameof(success.IsSuccess), true, success.IsSuccess);
			AssertEquals(nameof(success.JobPK), jobPK, success.JobPK);
			AssertNull(nameof(success.PrinterName), success.PrinterName);
			AssertNull(nameof(success.FailureReason), success.FailureReason);
			AssertNull(nameof(success.GetUnhandledException), success.GetUnhandledException);
		}

		public void TestSuccess_InvalidArguments()
		{
			AssertNoExceptionThrown("JobPK may be blank for cases where the PrintServer is passed a job directly without using the DB", () => PrintResult.Success(Guid.Empty));
		}

		public void TestFail()
		{
			var jobPK = Guid.NewGuid();
			var fail = PrintResult.Fail(jobPK);
			AssertEquals(nameof(fail.IsSuccess), false, fail.IsSuccess);
			AssertEquals(nameof(fail.JobPK), jobPK, fail.JobPK);
			AssertNull(nameof(fail.PrinterName), fail.PrinterName);
			AssertNull(nameof(fail.FailureReason), fail.FailureReason);
			AssertNull(nameof(fail.GetUnhandledException), fail.GetUnhandledException);
		}

		public void TestFail_InvalidArguments()
		{
			AssertNoExceptionThrown("JobPK may be blank for cases where the PrintServer is passed a job directly without using the DB", () => PrintResult.Fail(Guid.Empty));
		}

		public void TestWithFailureReason()
		{
			var jobPK = Guid.NewGuid();
			var fail = PrintResult.WithFailureReason(jobPK, "PRINTER", "FAILURE");
			AssertEquals(nameof(fail.IsSuccess), false, fail.IsSuccess);
			AssertEquals(nameof(fail.JobPK), jobPK, fail.JobPK);
			AssertEquals(nameof(fail.PrinterName), "PRINTER", fail.PrinterName);
			AssertEquals(nameof(fail.FailureReason), "FAILURE", fail.FailureReason);
			AssertNull(nameof(fail.GetUnhandledException), fail.GetUnhandledException);
		}

		public void TestWithFailureReason_InvalidArguments()
		{
			AssertNoExceptionThrown("JobPK may be blank for cases where the PrintServer is passed a job directly without using the DB", () => PrintResult.WithFailureReason(Guid.Empty, "PRINTER", "FAILURE"));

			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithFailureReason(Guid.NewGuid(), null, "FAILURE"));
			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithFailureReason(Guid.NewGuid(), "", "FAILURE"));
			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithFailureReason(Guid.NewGuid(), "PRINTER", null));
			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithFailureReason(Guid.NewGuid(), "PRINTER", ""));
		}

		public void TestWithUnhandledException()
		{
			var jobPK = Guid.NewGuid();
			var fail = PrintResult.WithUnhandledException(jobPK, "PRINTER", "FAILURE", new InvalidOperationException("ERROR"));
			AssertEquals(nameof(fail.IsSuccess), false, fail.IsSuccess);
			AssertEquals(nameof(fail.JobPK), jobPK, fail.JobPK);
			AssertEquals(nameof(fail.PrinterName), "PRINTER", fail.PrinterName);
			AssertEquals(nameof(fail.FailureReason), "FAILURE", fail.FailureReason);
			AssertNotNull(nameof(fail.GetUnhandledException), fail.GetUnhandledException);

			var exception = fail.GetUnhandledException();
			AssertType<InvalidOperationException>(exception);
			AssertEquals("ERROR", exception.Message);
		}

		public void TestWithUnhandledException_InvalidArguments()
		{
			AssertNoExceptionThrown("JobPK may be blank for cases where the PrintServer is passed a job directly without using the DB", () => PrintResult.WithUnhandledException(Guid.Empty, "PRINTER", "FAILURE", new InvalidOperationException("ERROR")));

			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithUnhandledException(Guid.NewGuid(), null, "FAILURE", new InvalidOperationException("ERROR")));
			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithUnhandledException(Guid.NewGuid(), "", "FAILURE", new InvalidOperationException("ERROR")));
			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithUnhandledException(Guid.NewGuid(), "PRINTER", null, new InvalidOperationException("ERROR")));
			AssertExceptionThrown(typeof(ArgumentException), () => PrintResult.WithUnhandledException(Guid.NewGuid(), "PRINTER", "", new InvalidOperationException("ERROR")));
			AssertExceptionThrown(typeof(ArgumentNullException), () => PrintResult.WithUnhandledException(Guid.NewGuid(), "PRINTER", "FAILURE", null));
		}

		public void TestHadUnhandledException()
		{
			var jobPK = Guid.NewGuid();
			AssertEquals(false, PrintResult.Success(jobPK).HadUnhandledException);
			AssertEquals(true, PrintResult.WithUnhandledException(jobPK, "PRINTER", "FAILURE", new InvalidOperationException("ERROR")).HadUnhandledException);
		}
	}
}
