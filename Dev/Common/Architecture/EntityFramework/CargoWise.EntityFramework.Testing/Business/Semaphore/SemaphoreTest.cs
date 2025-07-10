using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SemaphoreTest : TestCase
	{
		public void TestIsSuspendedIncrementAndDecrement()
		{
			AssertEquals(false, Semaphore.IsSuspended);

			SemaphoreInternals.Increment();
			AssertEquals(true, Semaphore.IsSuspended);

			SemaphoreInternals.Decrement();
			AssertEquals(false, Semaphore.IsSuspended);
		}

		#region Implementation

		ISemaphoreItemInternals SemaphoreInternals
		{
			get { return Semaphore; }
		}

		Semaphore Semaphore
		{
			get { return fSemaphore ?? (fSemaphore = new Semaphore()); }
		}

		Semaphore fSemaphore;

		#endregion
	}
}
