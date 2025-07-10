using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuDocumentConfigItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLegacySectionTypes()
		{
			var lookups = new StmMenuDocumentConfigItemLookups(Factory.New<StmMenuDocumentConfigItem>());
			AssertNotNull(lookups.LegacySectionTypes);
		}

		public void TestGenericSectionTypes()
		{
			var lookups = new StmMenuDocumentConfigItemLookups(Factory.New<StmMenuDocumentConfigItem>());
			AssertNotNull(lookups.GenericSectionTypes);
		}
	}
}
