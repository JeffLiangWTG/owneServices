using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestCaseBaseClass = NUnit.Framework.TestCase;

namespace CWNUnit.TestAdapter
{
	public class TestErrorListener : ITestListener
	{
		public List<Exception> Errors { get; } = new List<Exception>();
		public string Message { get; set; }

		public bool Skipped => Message != null;

		void ITestListener.AddError(Exception ex, ITest test)
		{
			ex = TestUtilities.StripException(ex);
			if (!Errors.Contains(ex))
			{
				Errors.Add(ex);
			}
		}

		void ITestListener.AddSlowTest(TimeSpan testDuration)
		{
		}

		void ITestListener.StartAllTests(DateTime startTime)
		{
		}

		void ITestListener.StartTest(TestCaseBaseClass test, DateTime startTime)
		{
		}

		void ITestListener.EndTest(TestCaseBaseClass test, DateTime endTime)
		{
			if (test.ShouldRunTest)
			{
				// Normal test
				// Do nothing
			}
			else if (!ITestAmnestyExtension.CanRunTest(test))
			{
				Message = "Amnesty test";
			}
			else if (DeveloperOnlyTestAttribute.IsDeveloperOnlyTest(test))
			{
				Message = "Developer Only test";
			}
			else
			{
				Message = "Shouldn't happen";
			}
		}

		void ITestListener.BeforeEachTest(DateTime startTime)
		{
			Errors.Clear();
			Message = null;
		}

		void ITestListener.AfterEachTest(DateTime endTime)
		{
		}

		void ITestListener.EndAllTests(DateTime endTime)
		{
		}
	}
}
