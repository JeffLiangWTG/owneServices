using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Progress.Test
{
	public class IProgressExtensionsTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestSafeSetStatusAndPercentComplete()
		{
			IEdiProgress progress = null;
			progress.SafeSetStatusAndPercentComplete("hello", 13);

			progress = dummyProgress;
			AssertEquals("", dummyProgress.StatusAndPercentComplete);

			progress.SafeSetStatusAndPercentComplete("hello", 13);
			AssertEquals("hello13", dummyProgress.StatusAndPercentComplete);
		}

		[ExpectNoExceptions]
		public void TestSafeSetExpectedCount()
		{
			IEdiProgress progress = null;
			progress.SafeSetExpectedCount(13);

			progress = dummyProgress;
			AssertEquals("", dummyProgress.ExpectedCount);

			progress.SafeSetExpectedCount(13);
			AssertEquals("13", dummyProgress.ExpectedCount);
		}

		[ExpectNoExceptions]
		public void TestSafeUpdateCurrentCount()
		{
			IEdiProgress progress = null;
			progress.SafeUpdateCurrentCount("hello");

			progress = dummyProgress;
			AssertEquals("", dummyProgress.CurrentCount);

			progress.SafeUpdateCurrentCount("hello");
			AssertEquals("hello", dummyProgress.CurrentCount);
		}

		[ExpectNoExceptions]
		public void TestSafeIsCancelled()
		{
			IEdiProgress progress = null;
			AssertEquals(false, progress.SafeIsCancelled());

			progress = dummyProgress;
			AssertEquals(false, dummyProgress.SafeIsCancelled());

			dummyProgress.isCancelled = true;
			AssertEquals(true, dummyProgress.SafeIsCancelled());

			dummyProgress.isCancelled = false;
			AssertEquals(false, dummyProgress.SafeIsCancelled());
		}

		#region Implementation

		readonly DummyProgress dummyProgress = new DummyProgress();

		class DummyProgress : IEdiProgress
		{
			#region IEdiProgress Members

			public void SetStatusAndPercentComplete(string status, int percentComplete)
			{
				StatusAndPercentComplete = status + percentComplete.ToString();
			}
			public ZString StatusAndPercentComplete;

			public void SetExpectedCount(int count)
			{
				ExpectedCount = count.ToString();
			}
			public ZString ExpectedCount;

			public void UpdateCurrentCount(string status)
			{
				CurrentCount = status;
			}
			public ZString CurrentCount;

			public bool IsCancelled
			{
				get { return isCancelled; }
			}
			public bool isCancelled;

			#endregion
		}

		#endregion
	}
}
