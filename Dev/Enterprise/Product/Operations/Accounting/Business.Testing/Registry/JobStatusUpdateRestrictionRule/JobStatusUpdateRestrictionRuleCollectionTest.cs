using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobStatusUpdateRestrictionRuleCollection))]
	public class JobStatusUpdateRestrictionRuleCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobStatusUpdateRestrictionRuleCollection>
	{
		public void TestGetDefault()
		{
			var defaultRule = JobStatusUpdateRestrictionRuleCollection.GetDefault();

			var i = 0;
			AssertEquals(12, defaultRule.Count);
			AssertDefaultValue(defaultRule[i++], "WRK (Working)", "Change Status of Working Jobs", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(defaultRule[i++], "WHL (Work on Hold)", "Change Status of Work On Hold Jobs", "NO", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(defaultRule[i++], "IHL (Invoice On Hold)", "Change Status of Invoice On Hold Jobs", "NO", "NO", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(defaultRule[i++], "CUS (Customs Processing Active)", "Change Status of Customs Processing Active Jobs", "NO", "NO", "NO", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(defaultRule[i++], "JRA (Job Ready for Revenue and Cost Posting)", "Change Status of Ready to Post Jobs", "YES", "YES", "YES", "YES", "", "YES", "YES", "YES", "YES", "YES", "YES", "YES");
			AssertDefaultValue(defaultRule[i++], "JRB (Job Ready for Revenue Posting)", "Change Status of Ready to Post Jobs", "YES", "YES", "YES", "YES", "NO", "", "YES", "YES", "YES", "YES", "YES", "YES");
			AssertDefaultValue(defaultRule[i++], "JRC (Job Ready for Cost Posting)", "Change Status of Ready to Post Jobs", "YES", "YES", "YES", "YES", "NO", "YES", "", "YES", "YES", "YES", "YES", "YES");
			AssertDefaultValue(defaultRule[i++], "RDD (Job Ready for Delivery)", "Change Status of Ready for Delivery Jobs", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "", "NO", "NO", "NO", "NO");
			AssertDefaultValue(defaultRule[i++], "INV (Job Invoiced)", "Change Status of Invoiced Jobs", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "", "NO", "NO", "NO");
			AssertDefaultValue(defaultRule[i++], "CMP (Complete)", "Change Status of Complete Jobs", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "", "YES", "YES");
			AssertDefaultValue(defaultRule[i++], "JFC (Job Ready for Financial Closure)", "Change Status of Ready for Financial Closure Jobs", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "", "YES");
			AssertDefaultValue(defaultRule[i++], "ARC (Schedule for Archive)", "Change Status of Schedule for Archive Jobs", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "");

			void AssertDefaultValue(JobStatusUpdateRestrictionRule jobStatusUpdateRestrictionRule,
									string expectedJobStatus,
									string expectedRelatedSecurityRight,
									string expectedWorking,
									string expectedWorkOnHold,
									string expectedInvoiceOnHold,
									string expectedCustomsProcessActive,
									string expectedJobReadyForRevenueAndCostPosting,
									string expectedJobReadyForRevenuePosting,
									string expectedJobReadyForCostPosting,
									string expectedJobReadyForDelivery,
									string expectedJobInvoiced,
									string expectedComplete,
									string expectedJobReadyForFinancialClosure,
									string expectedScheduledForArchive)
			{
				AssertEquals("JobStatus", expectedJobStatus, jobStatusUpdateRestrictionRule.JobStatusDescription);
				AssertEquals("RelatedSecurityRight", expectedRelatedSecurityRight, jobStatusUpdateRestrictionRule.RelatedSecurityRightDescription);
				AssertEquals("Working", expectedWorking, jobStatusUpdateRestrictionRule.Working);
				AssertEquals("WorkOnHold", expectedWorkOnHold, jobStatusUpdateRestrictionRule.WorkOnHold);
				AssertEquals("InvoiceOnHold", expectedInvoiceOnHold, jobStatusUpdateRestrictionRule.InvoiceOnHold);
				AssertEquals("CustomsProcessActive", expectedCustomsProcessActive, jobStatusUpdateRestrictionRule.CustomsProcessActive);
				AssertEquals("JobReadyForRevenueAndCostPosting", expectedJobReadyForRevenueAndCostPosting, jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting);
				AssertEquals("JobReadyForRevenuePosting", expectedJobReadyForRevenuePosting, jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting);
				AssertEquals("JobReadyForCostPosting", expectedJobReadyForCostPosting, jobStatusUpdateRestrictionRule.JobReadyForCostPosting);
				AssertEquals("JobReadyForDelivery", expectedJobReadyForDelivery, jobStatusUpdateRestrictionRule.JobReadyForDelivery);
				AssertEquals("JobInvoiced", expectedJobInvoiced, jobStatusUpdateRestrictionRule.JobInvoiced);
				AssertEquals("Complete", expectedComplete, jobStatusUpdateRestrictionRule.Complete);
				AssertEquals("JobReadyForFinancialClosure", expectedJobReadyForFinancialClosure, jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure);
				AssertEquals("ScheduledForArchive", expectedScheduledForArchive, jobStatusUpdateRestrictionRule.ScheduledForArchive);
			}
		}

		public void TestAllowSort()
		{
			var collection = new JobStatusUpdateRestrictionRuleCollectionForTest();
			AssertEquals(false, collection.AllowSortForTest);
		}

		public void TestAllowNewCore()
		{
			var collection = new JobStatusUpdateRestrictionRuleCollectionForTest();
			AssertEquals(false, collection.AllowNewCoreForTest);
		}

		public void TestAllowRemoveCore()
		{
			var collection = new JobStatusUpdateRestrictionRuleCollectionForTest();
			AssertEquals(false, collection.AllowRemoveCoreForTest);
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override JobStatusUpdateRestrictionRuleCollection GetCollectionToTest() => new JobStatusUpdateRestrictionRuleCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new JobStatusUpdateRestrictionRule();

		#endregion

		public class JobStatusUpdateRestrictionRuleCollectionForTest : JobStatusUpdateRestrictionRuleCollection
		{
			public bool AllowSortForTest => base.AllowSort;

			public bool AllowNewCoreForTest => base.AllowNewCore;

			public bool AllowRemoveCoreForTest => base.AllowRemoveCore;
		}
	}
}
