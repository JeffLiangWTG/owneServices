using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DateSetInRelevantTestListenerTest : TestCaseWithFactory
	{
		readonly DateSetInRelevantTestListener listener = DateSetInRelevantTestListener.Instance;

		[ExemptFromDateSetInRelevantTestListener]
		public void TestUnsetDate_ShouldTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithNoDate";

			listener.BeforeEachTest(DateTime.Now);
			DateSetInRelevantTestListener.Instance.SetWorkingDaysCalled();
			TestCaseWithFactory.SetWasAssertDbHitsCalled();

			AssertExceptionThrown<AssertionFailedError>("This test has been thrown upon the altar of cruelty, and must be made to fail at the one thing it was built to do: succeed. However, it hasn't done that!", () => listener.EndTest(test, DateTime.Now));
		}

		[ExemptFromDateSetInRelevantTestListener]
		public void TestNoAssertDbHits_ShouldNotTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithDate";

			listener.BeforeEachTest(DateTime.Now);
			DateSetInRelevantTestListener.Instance.SetWorkingDaysCalled();

			AssertNoExceptionThrown("This test should, through highly advanced flag-spying techniques, see nothing wrong, and yet...", () => listener.EndTest(test, DateTime.Now));
		}

		[ExemptFromDateSetInRelevantTestListener]
		public void TestNoAssertDbHits_InTestSetDate_ShouldNotTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithInlineDate";

			listener.BeforeEachTest(DateTime.Now);
			DateSetInRelevantTestListener.Instance.SetWorkingDaysCalled();

			AssertNoExceptionThrown("And Daniel said to the test 'Your job is to succeed'. But the test disobeyed. Sad.", () => listener.EndTest(test, DateTime.Now));
		}

		[ExemptFromDateSetInRelevantTestListener]
		public void TestNoAssertDbHits_ActiveDate_ShouldNotTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithActiveDate";

			listener.BeforeEachTest(DateTime.Now);
			DateSetInRelevantTestListener.Instance.SetWorkingDaysCalled();

			AssertNoExceptionThrown("Life is full of success. In this case it is not. Bad!", () => listener.EndTest(test, DateTime.Now));
		}

		[ExemptFromDateSetInRelevantTestListener]
		public void TestNoWorkingDaysCall_ShouldNotTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithDate";

			listener.BeforeEachTest(DateTime.Now);
			TestCaseWithFactory.SetWasAssertDbHitsCalled();
			AssertNoExceptionThrown("I am so very tired. Unlike this test listener, which has detected an issue with this test. Not good!", () => listener.EndTest(test, DateTime.Now));
		}

		[ExemptFromDateSetInRelevantTestListener]
		public void TestNoWorkingDaysCall_InTestSetDate_ShouldNotTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithInlineDate";

			listener.BeforeEachTest(DateTime.Now);
			TestCaseWithFactory.SetWasAssertDbHitsCalled();

			AssertNoExceptionThrown("If this test is failing, that's bad. This test should be VERY good at succeeding. It was built for it!", () => listener.EndTest(test, DateTime.Now));
		}

		[ExemptFromDateSetInRelevantTestListener]
		public void TestNoWorkingDaysCall_ActiveDate_ShouldNotTriggerListener()
		{
			TestCase test = new MockTestForListener();
			test.Name = "TestWithActiveDate";

			listener.BeforeEachTest(DateTime.Now);
			TestCaseWithFactory.SetWasAssertDbHitsCalled();

			AssertNoExceptionThrown("This test should not annoy the test listener. The test listener is known to complain. Loudly.", () => listener.EndTest(test, DateTime.Now));
		}
	}
}
