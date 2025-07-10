using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	abstract class AsycudaPackedItemCollectionAbstractTest<T> : BusinessObjectCollectionTestCase
			where T : AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(T);
		}

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return GetNewCollection(pack);
		}

		protected abstract T GetNewCollection(AsycudaPack pack);
	}
}
