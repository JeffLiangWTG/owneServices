using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	internal class JobChargePostingQueueLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPostingInstructions()
		{
			var bizO = Factory.NewWithValidTestData<JobChargePostingQueue>();
			var lookup = new JobChargePostingQueueLookups(bizO).PostingInstructions;
			AssertEquals(2, lookup.Count);
			AssertEquals("Post Cost", lookup[JobChargePostingQueueLookups.PostCost].Description);
			AssertEquals("Post Revenue", lookup[JobChargePostingQueueLookups.PostRevenue].Description);
		}
	}
}