using System;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Utils.Policies
{
	public class RetryPolicyTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestRetry()
		{
			int i = 0;
			int retryTimes = 3;
			var policy = new RetryPolicy<Exception>();
			Action action = () => { i++; throw new Exception(); };
			policy.RetryCount = retryTimes;

			AssertExceptionThrown("Should throw error after maximum retry times", typeof(Exception), delegate { policy.Retry(action); });
			AssertEquals(String.Format("Should be called {0} times", retryTimes + 1), retryTimes + 1, i);
		}
	}

	public class CountableRetryStateTest : TestCase
	{
		public void TestCanRetry()
		{
			int retryTime = 3;

			var state = new CountableRetryState(retryTime);

			var exception = new Exception();
			for (int i = 0; i < retryTime; i++)
			{
				AssertEquals("Should be able to retry before 3 times call", true, state.CanRetry(exception));
			}
			AssertEquals("Should not be able to retry after 3 times", false, state.CanRetry(exception));
		}
	}
}
