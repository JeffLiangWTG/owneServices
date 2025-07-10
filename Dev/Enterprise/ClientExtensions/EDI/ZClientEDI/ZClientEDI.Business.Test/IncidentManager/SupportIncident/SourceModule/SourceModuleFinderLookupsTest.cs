using CargoWise.EntityFramework.Testing;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	sealed class SourceModuleFinderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModuleFilterList()
		{
			var finder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			var lookups = new SourceModuleFinderLookups(finder);

			AssertNotNull(lookups.ModuleFilterList);
		}

		public void TestProductAreaList()
		{
			var finder = new SourceModuleFinder("", ModuleListType.MenuSection, "", Factory);
			var lookups = new SourceModuleFinderLookups(finder);

			AssertNotNull(lookups.ProductAreaList);
		}
	}
}
