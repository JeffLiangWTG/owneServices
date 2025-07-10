using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(ReportMutexSemaphore))]
	sealed class ReportMutexSemaphoreTest : SemaphoreTypeTestCase
	{
		public void TestZGlobalMutexSemaphore()
		{
			AssertEquals("LockInfo", "ReportMutex", TestSemaphore.LockInfo);
		}

		protected override ISemaphoreType TestSemaphore
		{
			get { return new ReportMutexSemaphore(); }
		}
	}
}
