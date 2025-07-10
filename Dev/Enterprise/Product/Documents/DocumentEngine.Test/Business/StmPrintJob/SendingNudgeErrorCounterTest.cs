using System;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class SendingNudgeErrorCounterTest : TestCase
	{
		public void TestIsSuspendSendingNudgeForMinutes()
		{
			using (ClearSendingNudgeErrorCounter())
			{
				AssertEquals("IsSuspendSendingNudge should return false", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				SendingNudgeErrorCounter.Instance.AddErrorCount();
				SendingNudgeErrorCounter.Instance.AddErrorCount();
				SendingNudgeErrorCounter.Instance.AddErrorCount();

				AssertEquals("IsSuspendSendingNudge should return true", true, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());
			}
		}

		public void TestIsSuspendSendingNudgeForHours()
		{
			using (ClearSendingNudgeErrorCounter())
			{
				var now = ZDateTime.UtcNow;
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now);
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now);
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(15));
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(15));
				AssertEquals("IsSuspendSendingNudge should return false", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(30));
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(30));
				AssertEquals("IsSuspendSendingNudge should return false", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(45));
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(45));
				AssertEquals("IsSuspendSendingNudge should return false", false, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());

				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(59));
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(59));
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(59));
				SendingNudgeErrorCounter.Instance.AddErrorCountForTest(now.AddMinutes(59));

				AssertEquals(12, SendingNudgeErrorCounter.Instance.ErrorCount_Exposed);
				AssertEquals("IsSuspendSendingNudge should return true", true, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());
				AssertEquals(10, SendingNudgeErrorCounter.Instance.ErrorCount_Exposed);

				AssertEquals("IsSuspendSendingNudge should return true", true, SendingNudgeErrorCounter.Instance.IsSuspendSendingNudge());
				AssertEquals(0, SendingNudgeErrorCounter.Instance.ErrorCount_Exposed);
			}
		}

		IDisposable ClearSendingNudgeErrorCounter()
		{
			SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest();
			return new DisposableAction(() => SendingNudgeErrorCounter.Instance.ClearCounterTimesForTest());
		}
	}
}
