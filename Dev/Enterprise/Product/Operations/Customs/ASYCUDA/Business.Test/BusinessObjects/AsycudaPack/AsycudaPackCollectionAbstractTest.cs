using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	abstract class AsycudaPackCollectionAbstractTest<T> : BusinessObjectCollectionTestCase
			where T : AsycudaPackCollection<AsycudaPack, AsycudaBill>
	{
		protected override Type GetExpectedCollectionType() => typeof(T);

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return GetNewCollection(bill);
		}

		protected abstract T GetNewCollection(AsycudaBill bill);
	}
}
