using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	[TestedType(typeof(PendingUserActionSemaphore))]
	public class PendingUserActionSemaphoreTest : SemaphoreTypeTestCase
	{
		public void TestLicenceLoginSemaphore()
		{
			AssertEquals("LockInfo", PendingUserActionSemaphore.LockInfoPrefix + "KEY", TestSemaphore.LockInfo);
		}

		protected override ISemaphoreType TestSemaphore
		{
			get
			{
				return new PendingUserActionSemaphore("KEY");
			}
		}
	}
}
