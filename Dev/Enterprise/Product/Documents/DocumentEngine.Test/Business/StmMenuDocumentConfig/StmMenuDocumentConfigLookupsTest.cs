using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuDocumentConfigLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAvailablePageStyles()
		{
			var documentConfig = Factory.NewWithValidTestData<StmMenuDocumentConfig>();
			AssertNotNull("documentConfig.Lookups.AvailablePageStyles", documentConfig.Lookups.AvailablePageStyles);
		}

		public void TestHeaders()
		{
			StmMenuDocumentConfig docConfig = Factory.New<StmMenuDocumentConfig>();
			AssertNotNull("Headers", docConfig.Lookups.Headers);
		}
	}
}
