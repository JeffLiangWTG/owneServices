using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.MemoryManagement.Internal.Testing
{
	class ReclaimableHolderTest : TestCase
	{
		public void TestIsEligibleForCurrentThreadOnAnyThread()
		{
			var holder = new ReclaimableHolder(new Reclaimable(null, (target, action) =>
			{
				return FlushResult.Exhausted;
			}), FlushCallback.OnAnyThread);
			AssertEquals("Object should be reclaimable as current thread", true, holder.IsEligibleForCurrentThread);
			object result = null;
			var otherThread = new Thread(delegate()
			{
				result = holder.IsEligibleForCurrentThread;
			});
			otherThread.Start();
			otherThread.Join();
			Assert("Object should still be collectable as OnAnyThread object", result != null && (bool)result);
		}

		public void TestIsEligibleForCurrentThreadOnRegisteringThread()
		{
			var holder = new ReclaimableHolder(new Reclaimable(null, (target, action) =>
			{
				return FlushResult.Exhausted;
			}), FlushCallback.OnRegisteringThread);
			Assert("Object should be reclaimable as current thread", holder.IsEligibleForCurrentThread);
			object result = null;
			var otherThread = new Thread(delegate()
			{
				result = holder.IsEligibleForCurrentThread;
			});
			otherThread.Start();
			otherThread.Join();
			Assert("Object should NOT collectable as OnRegisteringThread object", result != null && !(bool)result);
		}
	}
}
