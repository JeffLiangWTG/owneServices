using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase.Extensions;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackUQList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.PackUQList;
			Assert(object.ReferenceEquals(Factory.GetPackageTypeList(), list));
		}

		public void TestWeightUQList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.WeightUQList;
			Assert(object.ReferenceEquals(Factory.GetWeightUQList(), list));
		}

		public void TestVolumeUQList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var list = pack.Lookups.VolumeUQList;
			Assert(object.ReferenceEquals(Factory.GetVolumeUQList(), list));
		}

		public void TestContainers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var containers = pack.Lookups.Containers;
			Assert(object.ReferenceEquals(header.Containers, containers));
		}
	}
}
