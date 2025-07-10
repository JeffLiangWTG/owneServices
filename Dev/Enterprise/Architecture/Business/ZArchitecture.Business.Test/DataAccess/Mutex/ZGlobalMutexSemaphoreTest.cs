using Enterprise.Semaphores.Common;
using Enterprise.Semaphores.Common.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Data.Mutex.Testing
{
	[TestedType(typeof(ZGlobalMutexSemaphore))]
	sealed class ZGlobalMutexSemaphoreTest : SemaphoreTypeTestCase
	{
		public void TestZGlobalMutexSemaphore()
		{
			AssertEquals("LockInfo", "Mutex:XXX:YYY", TestSemaphore.LockInfo);
		}

		protected override ISemaphoreType TestSemaphore
		{
			get
			{
				return new ZGlobalMutexSemaphore("XXX", "YYY");
			}
		}
	}
}
