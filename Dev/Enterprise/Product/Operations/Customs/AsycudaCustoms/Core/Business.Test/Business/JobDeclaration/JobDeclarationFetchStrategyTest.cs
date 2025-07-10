using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class JobDeclarationFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHint()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.InvoiceLines.AddNew();
			var strategy = new JobDeclarationFetchStrategy(dec);
			var count = Factory.ActiveTableFetchHints;
			strategy.FetchForMerge();
			AssertEquals("Additional Fetch hints should be added for preparing to merge", count + 4, Factory.ActiveTableFetchHints);
		}
	}
}
