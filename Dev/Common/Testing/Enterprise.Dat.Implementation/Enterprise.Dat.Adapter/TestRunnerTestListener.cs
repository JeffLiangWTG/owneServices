using System;
using System.Diagnostics;
using System.Linq;
using Dat.Integration;
using NUnit.Framework;
using DatTestResult = Dat.Integration.TestResult;

namespace Enterprise.Dat.Implementation
{
	public class TestRunnerTestListener : ITestListener
	{
		public TestRunnerTestListener(ErrorDescriptionListFactory errorListFactory)
		{
			this.errorListFactory = errorListFactory;
			errorList = null;
			stopwatch = new Stopwatch();
		}

		public void AddError(Exception e, ITest test)
		{
			if (errorList == null)
			{
				errorList = errorListFactory.New();
			}
			errorList.AddException(e, test);
		}

		public void AddSlowTest(TimeSpan testDuration)
		{
		}

		public void AfterEachTest(DateTime endTime)
		{
		}

		public void BeforeEachTest(DateTime startTime)
		{
			errorList = null;
		}

		public void EndAllTests(DateTime endTime)
		{
		}

		public void EndTest(TestCase test, DateTime endTime)
		{
			stopwatch.Stop();
		}

		public void StartAllTests(DateTime startTime)
		{
		}

		public void StartTest(TestCase test, DateTime startTime)
		{
			stopwatch.Restart();
		}

		public DatTestResult GetTestResult(TestDescriptor td)
		{
			var errors = errorList == null ? null : errorList.Cast<string>().ToArray();
			return new DatTestResult(td, stopwatch.Elapsed, DateTime.UtcNow, errors);
		}

		readonly ErrorDescriptionListFactory errorListFactory;
		ErrorDescriptionList errorList;
		readonly Stopwatch stopwatch;
	}
}
