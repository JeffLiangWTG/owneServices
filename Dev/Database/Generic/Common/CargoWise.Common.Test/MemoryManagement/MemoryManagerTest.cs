using System;
using NUnit.Framework;

namespace CargoWise.Common.MemoryManagement.Testing
{
	class MemoryManagerTest : TestCase
	{
		public void TestEndToEnd()
		{
			bigByteArray = new byte[1000000];
			MemoryManager.Register("TestFlush", this, FlushCallback.OnAnyThread, CleanUp);
			long memoryBeforeRun = GC.GetTotalMemory(true);
			AssertEquals(FlushResult.NotRequired, MemoryManager.Flush(FlushAction.Partial, 1000000000));
			Assert(bigByteArray != null);
			AssertEquals(FlushResult.Exhausted, MemoryManager.Flush(FlushAction.Full, 1));
			Assert(bigByteArray == null);
		}

		static FlushResult CleanUp(MemoryManagerTest test, FlushAction action)
		{
			test.bigByteArray = null;
			return FlushResult.Exhausted;
		}

		byte[] bigByteArray;
	}
}