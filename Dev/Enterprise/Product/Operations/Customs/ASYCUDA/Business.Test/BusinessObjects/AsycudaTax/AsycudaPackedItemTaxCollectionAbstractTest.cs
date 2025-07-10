using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	abstract class AsycudaPackedItemTaxCollectionAbstractTest<B, MasterB, T> : BusinessObjectCollectionTestCase
		where B : AsycudaTax
		where MasterB : AsycudaPackedItem
		where T : AsycudaPackedItemTaxCollection<B, MasterB>
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
			var packedItems = pack.PackedItemsForBinding;
			var packedItem = packedItems.AddNew();
			return GetNewCollection(packedItem);
		}

		protected abstract T GetNewCollection(AsycudaPackedItem packedItem);
	}
}
