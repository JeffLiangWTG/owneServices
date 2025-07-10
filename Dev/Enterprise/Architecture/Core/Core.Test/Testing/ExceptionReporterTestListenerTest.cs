using System;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ExceptionReporterTestListenerTest : TestCase
	{
		#region TestExceptionHasFullStackTrace

		[DeveloperOnlyTest] // Should fail
		public void TestReportOnce_Exception()
		{
			try
			{
				throw new Exception("Boo");
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce("Test should fail from this", ex);
			}

			Assert(true);
		}

		[DeveloperOnlyTest] // Should fail
		public void TestReportOnce_Developer_Exception()
		{
			try
			{
				throw new Exception("Boo");
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportDeveloperExceptionOnce("Test should fail from this", ex);
			}

			Assert(true);
		}

		[DeveloperOnlyTest] // Should fail
		public void TestReportOnce_Developer_NoException()
		{
			ErrorReporter.ReportDeveloperExceptionOnce("Test should fail from this", null);

			Assert(true);
		}

		[DeveloperOnlyTest] // Should fail
		public void TestReportOnce_NoException()
		{
			ErrorReporter.ReportOnce("Test should fail from this");

			Assert(true);
		}

		public void TestExceptionHasFullStackTrace()
		{
			try
			{
				SuperDuperMethod(5);
			}
			catch (Exception ex)
			{
				var key = "Exception Key";
				var message = "Exception Message";
				var stack = new TestExceptionWithStack(ex, key, message);
				Assert("Precondition: Has ReportException in trace", ex.StackTrace.Contains("ReportException(Int32 counter)"));
				AssertAllLinesStartWith("Contains expected stace trace with no report elements", ExpectedStackTrace, stack.Stack.TrimEnd());
				AssertEquals("Exception Key", key, stack.ExceptionKey);
				AssertEquals("Exception Message", message, stack.ExceptionMessage);
			}
		}

		void SuperDuperMethod(int counter)
		{
			if (counter <= 0)
			{
				HandleThreadException();
			}
			SuperDuperMethod(counter - 1);
		}

		void HandleThreadException()
		{
			ReportException(5); // since the two top lines will be ignored in GenerateStack
		}

		void ReportException(int counter)
		{
			if (counter <= 0)
			{
				throw new InvalidOperationException();
			}
			ReportException(counter - 1);
		}

		const string ExpectedStackTrace =
@"   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.HandleThreadException()
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListenerTest.TestExceptionHasFullStackTrace()";

		#endregion

		class TestExceptionWithStack : ExceptionReporterTestListener.ExceptionWithStack
		{
			public TestExceptionWithStack(Exception ex, string key, string message)
				: base(ex, key, message)
			{
			}

			protected override string CurrentStackTrace()
			{
				return Ex.StackTrace;
			}
		}
	}
}
