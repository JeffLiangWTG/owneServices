using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	abstract class AsycudaTaxCollectionAbstractTest<B, MasterB, T> : BusinessObjectCollectionTestCase
		where B : AsycudaTax
		where MasterB : AsycudaBill
		where T : AsycudaTaxCollection<B, MasterB>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(T);
		}

		protected sealed override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return GetNewCollection(bill);
		}

		protected abstract T GetNewCollection(AsycudaBill bill);
	}
}
