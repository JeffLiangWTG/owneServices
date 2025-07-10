using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDeclarantList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.DeclarantList;
			AssertType<OrganisationsFindBoxCollection>(list);
		}
	}
}
