using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEverything()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			var lu = pack.Lookups;
			AssertEquals(bill.Lookups.PackageTypeList.CodesAsString, lu.PackUQList.CodesAsString);
			AssertEquals(bill.Lookups.WeightUQList.CodesAsString, lu.WeightUQList.CodesAsString);
			AssertEquals(bill.Lookups.VolumeUQList.CodesAsString, lu.VolumeUQList.CodesAsString);
			AssertCollectionContains(cont, lu.Containers);
		}
	}
}
