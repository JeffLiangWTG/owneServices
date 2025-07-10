using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SemaphoreManagerTest : TestCase
	{
		public void TestSemaphoreIncrementOnConstruction()
		{
			AssertEquals("Precondition", false, Semaphore.IsSuspended);

			using (new SemaphoreManager(Semaphore))
			{
				AssertEquals(true, Semaphore.IsSuspended);
			}
		}

		public void TestSemaphoreDecrementOnDispose()
		{
			using (new SemaphoreManager(Semaphore))
			{
				AssertEquals("Precondition", true, Semaphore.IsSuspended);
			}

			AssertEquals(false, Semaphore.IsSuspended);
		}

		#region Implementation

		Semaphore Semaphore
		{
			get { return fSemaphore ?? (fSemaphore = new Semaphore()); }
		}

		Semaphore fSemaphore;

		#endregion
	}
}
