using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineFetchStrategy))]
	sealed class JobComInvoiceLineFetchStrategyTest : Customs.Business.FetchStrategies.Testing.JobComInvoiceLineFetchStrategyTest
	{
		public void TestFetchForLoad()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);
			strategy.FetchForLoad();
			AssertEquals("hint for JobComInvHeaderCharge", "JobComInvHeaderCharge", string.Join(",", Factory.GetAllFetchHintedTableNames()));
		}
	}
}
