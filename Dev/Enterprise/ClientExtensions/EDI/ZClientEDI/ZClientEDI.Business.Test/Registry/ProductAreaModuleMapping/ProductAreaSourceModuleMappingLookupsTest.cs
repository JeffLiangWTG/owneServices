using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	internal sealed class ProductAreaSourceModuleMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSourceModuleList()
		{
			var lookups = new ProductAreaSourceModuleMappingLookups(null);
			AssertNotNull(lookups.SourceModuleList);
		}
	}
}