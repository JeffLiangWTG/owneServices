using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business.Test
{
	class ReleaseBuildLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProductTypeList()
		{
			var collection = new SystemProductCollection();
			collection.AddNew("PLT", "Plato", true);
			collection.AddNew("SOC", "Socrates", true);
			collection.AddNew("ARI", "Aristotle", true);

			EDIDataRegistry.Instance.SystemProductMappings.SetValue(System.Guid.Empty, System.Guid.Empty, System.Guid.Empty, collection);

			ReleaseBuildLookups lookups = new ReleaseBuildLookups(null);

			Assert(lookups.ProductTypeList.ContainsCode("PLT"));
			Assert(lookups.ProductTypeList.ContainsCode("SOC"));
			Assert(lookups.ProductTypeList.ContainsCode("ARI"));
			Assert("CargoWise Next should be in the list", lookups.ProductTypeList.ContainsCode("CWN"));
			Assert("CargoWise should be in the list", lookups.ProductTypeList.ContainsCode("CGW"));
		}
		public void TestStatuses()
		{
			ReleaseBuildLookups lookups = new ReleaseBuildLookups(null);
			Assert("Statuses should be a ReleaseRingsList.", lookups.Statuses is ReleaseRingsList);
		}
	}
}
