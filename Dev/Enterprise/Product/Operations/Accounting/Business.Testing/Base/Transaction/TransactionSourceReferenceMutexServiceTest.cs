using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Testing.Base.Transaction
{
	public class TransactionSourceReferenceMutexServiceTest : TestCaseWithFactory
	{
		public void TestUnlockSourceReferenceMutex()
		{
			var service1 = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(Factory);
			var service2 = TransactionSourceReferenceMutexService.GetTransactionSourceReferenceMutexService(new CargoWise.EntityFramework.BusinessObjectFactory());
			var mutex1 = service1.GetSourceReferenceMutex("C01", "sourceRef1");
			service1.GetSourceReferenceMutex("C01", "sourceRef1");
			var mutex2 = service1.GetSourceReferenceMutex("C01", "sourceRef2");
			var mutex3 = service1.GetSourceReferenceMutex("C01", "sourceRef3");
			AssertEquals("no duplicate mutex", 3, service1.SourceReferenceMutexes_ForTestOnly.Count);

			mutex1.Lock();
			mutex2.Lock();
			mutex3.Lock();
			Assert(mutex1.IsLocked);
			Assert(mutex2.IsLocked);
			Assert(mutex3.IsLocked);

			service1.UnlockSourceReferenceMutex("C01", "sourceRef1");
			Assert("unlock completed", !mutex1.IsLocked);
			Assert(mutex2.IsLocked);
			Assert(mutex3.IsLocked);
			AssertContainsExactElementsInAnyOrder(new[] { "sourceRef2", "sourceRef3" }, service1.SourceReferenceMutexes_ForTestOnly.Keys.ToArray());

			service2.UnlockSourceReferenceMutex("C01", "sourceRef2"); //service2 doesn't own the lock so it cannot unlock it
			service2.UnlockAllSourceReferenceMutexes();
			Assert(!mutex1.IsLocked);
			Assert(mutex2.IsLocked);
			Assert(mutex3.IsLocked);
			AssertContainsExactElementsInAnyOrder(new[] { "sourceRef2", "sourceRef3" }, service1.SourceReferenceMutexes_ForTestOnly.Keys.ToArray());

			service1.UnlockAllSourceReferenceMutexes();
			Assert(!mutex1.IsLocked);
			Assert(!mutex2.IsLocked);
			Assert(!mutex3.IsLocked);
			AssertEquals(0, service1.SourceReferenceMutexes_ForTestOnly.Count);
		}
	}
}
