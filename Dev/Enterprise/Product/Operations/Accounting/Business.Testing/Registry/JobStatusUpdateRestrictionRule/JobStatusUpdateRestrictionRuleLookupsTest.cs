using CargoWise.EntityFramework.Testing;
using static Enterprise.Accounting.Registry.Business.JobStatusUpdateRestrictionRuleLookups;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class JobStatusUpdateRestrictionRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobStatusList()
		{
			AssertEquals(@"WRK - Working
WHL - Work on Hold
IHL - Invoice On Hold
CUS - Customs Processing Active
JRA - Job Ready for Revenue and Cost Posting
JRB - Job Ready for Revenue Posting
JRC - Job Ready for Cost Posting
RDD - Job Ready for Delivery
INV - Job Invoiced
CMP - Complete
JFC - Job Ready for Financial Closure
ARC - Schedule for Archive", lookups.JobStatusList.ElementsAsString);
		}

		public void TestYesOrNoList()
		{
			AssertEquals(@"NO - NO
YES - YES", lookups.YesOrNoList.ElementsAsString);
		}

		public void TestJobHeaderStatusList()
		{
			var jobStatusList = new JobHeaderStatusRestrictionList();
			AssertEquals(@"WRK - Working
WHL - Work on Hold
IHL - Invoice On Hold
CUS - Customs Processing Active
JRA - Job Ready for Revenue and Cost Posting
JRB - Job Ready for Revenue Posting
JRC - Job Ready for Cost Posting
RDD - Job Ready for Delivery
INV - Job Invoiced
CMP - Complete
JFC - Job Ready for Financial Closure
ARC - Schedule for Archive", jobStatusList.ElementsAsString);
		}

		public void TestYesNoList()
		{
			var yesOrNoList = new YesNoList();
			AssertEquals(@"NO - NO
YES - YES", yesOrNoList.ElementsAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			lookups = new JobStatusUpdateRestrictionRuleLookups(jobStatusUpdateRestrictionRule);
		}

		JobStatusUpdateRestrictionRuleLookups lookups;
	}
}
