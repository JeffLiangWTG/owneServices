using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	internal class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var containers = bill.Lookups.Containers;
			Assert(object.ReferenceEquals(header.Containers, containers));
		}
	}
}
