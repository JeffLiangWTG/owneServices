using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class MutexManagerTest : TestCaseWithFactory
	{
		public void TestMutexManager()
		{
			using (var manager = new MutexManager(header.Bills.Cast<AsycudaBill>(), (x) => x.SendToManifestMutex))
			{
				Assert(manager.HasAquiredLockForAllBills);
				AssertEquals("", manager.GetMutexLockByInfo());
			}

			var factory = new BusinessObjectFactory();
			var newBill1 = factory.Load<AsycudaBill>(bill1.PK);
			try
			{
				newBill1.SendToManifestMutex.Lock();
				using (var manager = new MutexManager(header.Bills.Cast<AsycudaBill>(), (x) => x.SendToManifestMutex))
				{
					Assert(!manager.HasAquiredLockForAllBills);
					Assert(manager.SucessfulLockedBills.Any());
					AssertNotEquals("", manager.GetMutexLockByInfo());
				}
			}
			finally
			{
				if (newBill1.SendToManifestMutex.HasLock)
				{
					newBill1.SendToManifestMutex.Unlock();
				}
			}

			var newBill2 = factory.Load<AsycudaBill>(bill2.PK);
			try
			{
				newBill1.SendToManifestMutex.Lock();
				newBill2.SendToManifestMutex.Lock();
				using (var manager = new MutexManager(header.Bills.Cast<AsycudaBill>(), (x) => x.SendToManifestMutex))
				{
					Assert(!manager.HasAquiredLockForAllBills);
					Assert(!manager.SucessfulLockedBills.Any());
					AssertNotEquals("", manager.GetMutexLockByInfo());
				}
			}
			finally
			{
				if (newBill1.SendToManifestMutex.HasLock)
				{
					newBill1.SendToManifestMutex.Unlock();
				}
				if (newBill2.SendToManifestMutex.HasLock)
				{
					newBill2.SendToManifestMutex.Unlock();
				}
			}
		}

		public void TestDispose()
		{
			try
			{
				var manager = new MutexManager(header.Bills.Cast<AsycudaBill>(), (x) => x.SendToManifestMutex);
				Assert(bill1.SendToManifestMutex.HasLock);
				Assert(bill2.SendToManifestMutex.HasLock);
				manager.Dispose();
				Assert(!bill1.SendToManifestMutex.HasLock);
				Assert(!bill2.SendToManifestMutex.HasLock);
			}
			finally
			{
				if (bill1.SendToManifestMutex.HasLock)
				{
					bill1.SendToManifestMutex.Unlock();
				}
				if (bill2.SendToManifestMutex.HasLock)
				{
					bill2.SendToManifestMutex.Unlock();
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGE");
			bill1 = header.Bills.AddNew();
			bill2 = header.Bills.AddNew();
			Factory.Save();
		}
		AsycudaManifestHeader header;
		AsycudaBill bill1;
		AsycudaBill bill2;
	}
}
