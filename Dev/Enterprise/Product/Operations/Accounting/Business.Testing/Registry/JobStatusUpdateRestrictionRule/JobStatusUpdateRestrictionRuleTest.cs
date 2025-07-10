using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Security;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobStatusUpdateRestrictionRule))]
	public class JobStatusUpdateRestrictionRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Properties

		public void TestJobStatus()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			AssertEquals("", jobStatusUpdateRestrictionRule.JobStatus);
			jobStatusUpdateRestrictionRule.JobStatus = "WRK";
			AssertEquals("WRK", jobStatusUpdateRestrictionRule.JobStatus);
		}

		public void TestJobStatusDescription()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();

			var expectedJobStatusDescriptionList = new string[] {
				"WRK (Working)" ,
				"WHL (Work on Hold)" ,
				"IHL (Invoice On Hold)",
				"CUS (Customs Processing Active)",
				"JRA (Job Ready for Revenue and Cost Posting)",
				"JRB (Job Ready for Revenue Posting)",
				"JRC (Job Ready for Cost Posting)",
				"RDD (Job Ready for Delivery)",
				"INV (Job Invoiced)",
				"CMP (Complete)",
				"JFC (Job Ready for Financial Closure)",
				"ARC (Schedule for Archive)" };

			var jobHeaderStatusList = new JobStatusUpdateRestrictionRuleLookups.JobHeaderStatusRestrictionList();
			for (int i = 0; i < jobHeaderStatusList.Count; i++)
			{
				var jobStatus = jobHeaderStatusList[i].Code;
				jobStatusUpdateRestrictionRule.JobStatus = jobStatus;
				AssertEquals($"The description of status {jobStatus}", expectedJobStatusDescriptionList[i], jobStatusUpdateRestrictionRule.JobStatusDescription);
			}
		}

		public void TestRelatedSecurityRight()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();

			var expectedRelatedSecurityRightList = new SecurityCheckpoint[] {
				Env.Security.ChangeStatusOfWorkingJobs ,
				Env.Security.ChangeStatusOfWorkOnHoldJobs ,
				Env.Security.ChangeStatusOfInvoiceOnHoldJobs,
				Env.Security.ChangeStatusOfCustomsProcessingActiveJobs,
				Env.Security.ChangeStatusOfReadyToPostJobs,
				Env.Security.ChangeStatusOfReadyToPostJobs,
				Env.Security.ChangeStatusOfReadyToPostJobs,
				Env.Security.ChangeStatusOfReadyforDeliveryJobs,
				Env.Security.ChangeStatusOfInvoicedJobs,
				Env.Security.ChangeStatusOfCompleteJobs,
				Env.Security.ChangeStatusOfReadyForFinancialClosureJobs,
				Env.Security.ChangeStatusOfScheduleForArchiveJobs,
			};

			var jobHeaderStatusList = new JobStatusUpdateRestrictionRuleLookups.JobHeaderStatusRestrictionList();
			for (int i = 0; i < jobHeaderStatusList.Count; i++)
			{
				var jobStatus = jobHeaderStatusList[i].Code;
				jobStatusUpdateRestrictionRule.JobStatus = jobStatus;
				AssertEquals($"The related security right of status {jobStatus}", expectedRelatedSecurityRightList[i], jobStatusUpdateRestrictionRule.RelatedSecurityRight);
			}
		}

		public void TestRelatedSecurityRightDescription()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();

			var expectedRelatedSecurityRightDescription = new string[] {
				"Change Status of Working Jobs" ,
				"Change Status of Work On Hold Jobs" ,
				"Change Status of Invoice On Hold Jobs",
				"Change Status of Customs Processing Active Jobs",
				"Change Status of Ready to Post Jobs",
				"Change Status of Ready to Post Jobs",
				"Change Status of Ready to Post Jobs",
				"Change Status of Ready for Delivery Jobs",
				"Change Status of Invoiced Jobs",
				"Change Status of Complete Jobs",
				"Change Status of Ready for Financial Closure Jobs",
				"Change Status of Schedule for Archive Jobs" };

			var jobHeaderStatusList = new JobStatusUpdateRestrictionRuleLookups.JobHeaderStatusRestrictionList();
			for (int i = 0; i < jobHeaderStatusList.Count; i++)
			{
				var jobStatus = jobHeaderStatusList[i].Code;
				jobStatusUpdateRestrictionRule.JobStatus = jobStatus;
				AssertEquals($"The related security right description of status {jobStatus}", expectedRelatedSecurityRightDescription[i], jobStatusUpdateRestrictionRule.RelatedSecurityRightDescription);
			}
		}

		public void TestWorking()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.WorkOnHold.Code;

			jobStatusUpdateRestrictionRule.Working = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.Working);

			jobStatusUpdateRestrictionRule.Working = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.Working);
		}

		public void TestInvoiceOnHold()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.InvoiceOnHold = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.InvoiceOnHold);

			jobStatusUpdateRestrictionRule.InvoiceOnHold = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.InvoiceOnHold);
		}

		public void TestCustomsProcessActive()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.CustomsProcessActive = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.CustomsProcessActive);

			jobStatusUpdateRestrictionRule.CustomsProcessActive = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.CustomsProcessActive);
		}

		public void TestJobReadyForRevenuePosting()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting);

			jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting);
		}

		public void TestJobReadyForCostPosting()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.JobReadyForCostPosting = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.JobReadyForCostPosting);

			jobStatusUpdateRestrictionRule.JobReadyForCostPosting = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.JobReadyForCostPosting);
		}

		public void TestJobReadyForRevenueAndCostPosting()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting);

			jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting);
		}

		public void TestJobInvoiced()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.JobInvoiced = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.JobInvoiced);

			jobStatusUpdateRestrictionRule.JobInvoiced = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.JobInvoiced);
		}

		public void TestJobReadyForDelivery()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.JobReadyForDelivery = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.JobReadyForDelivery);

			jobStatusUpdateRestrictionRule.JobReadyForDelivery = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.JobReadyForDelivery);
		}

		public void TestComplete()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.Complete = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.Complete);

			jobStatusUpdateRestrictionRule.Complete = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.Complete);
		}

		public void TestJobReadyForFinancialClosure()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure);

			jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure);
		}

		public void TestScheduledForArchive()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.JobStatus = JobHeaderStatus.Working.Code;

			jobStatusUpdateRestrictionRule.ScheduledForArchive = "YES";
			AssertEquals("YES", jobStatusUpdateRestrictionRule.ScheduledForArchive);

			jobStatusUpdateRestrictionRule.ScheduledForArchive = "NO";
			AssertEquals("NO", jobStatusUpdateRestrictionRule.ScheduledForArchive);
		}

		public void TestPropertyReadOnly()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();

			AssertReadOnly(jobStatusUpdateRestrictionRule.WorkingInfo, JobHeaderStatus.Working.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.WorkingInfo, JobHeaderStatus.InvoiceOnHold.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.WorkOnHoldInfo, JobHeaderStatus.WorkOnHold.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.WorkOnHoldInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.InvoiceOnHoldInfo, JobHeaderStatus.InvoiceOnHold.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.InvoiceOnHoldInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.CustomsProcessActiveInfo, JobHeaderStatus.CustomsProcessActive.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.CustomsProcessActiveInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPostingInfo, JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPostingInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForRevenuePostingInfo, JobHeaderStatus.JobReadyForRevenuePosting.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForRevenuePostingInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForCostPostingInfo, JobHeaderStatus.JobReadyForCostPosting.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForCostPostingInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForDeliveryInfo, JobHeaderStatus.JobReadyForDelivery.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForDeliveryInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.JobInvoicedInfo, JobHeaderStatus.JobInvoiced.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.JobInvoicedInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.CompleteInfo, JobHeaderStatus.Complete.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.CompleteInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForFinancialClosureInfo, JobHeaderStatus.JobReadyForFinancialClosure.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.JobReadyForFinancialClosureInfo, JobHeaderStatus.Working.Code, false);

			AssertReadOnly(jobStatusUpdateRestrictionRule.ScheduledForArchiveInfo, JobHeaderStatus.ScheduledForArchive.Code, true);
			AssertReadOnly(jobStatusUpdateRestrictionRule.ScheduledForArchiveInfo, JobHeaderStatus.Working.Code, false);

			void AssertReadOnly(ZPropertyInfo propertyInfo, string jobStatus, bool expectedReadOnly)
			{
				jobStatusUpdateRestrictionRule.JobStatus = jobStatus;
				if (expectedReadOnly)
				{
					AssertEquals($"Should be readonly when column name is the same as job status", expectedReadOnly, propertyInfo.ReadOnly);
				}
				else
				{
					AssertEquals($"Should not be readonly when column name is not the same as job status", expectedReadOnly, propertyInfo.ReadOnly);
				}
			}
		}

		#endregion

		#region Lookups

		public void TestLookups()
		{
			AssertType<JobStatusUpdateRestrictionRuleLookups>(jobStatusUpdateRestrictionRule.Lookups);
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			AssertType<JobStatusUpdateRestrictionRuleValidation>(jobStatusUpdateRestrictionRule.Validation);
		}

		#endregion

		#region Set Defaults

		public void TestSetDefaults()
		{
			AssertDefaultValue(JobHeaderStatus.Codes.Working, "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(JobHeaderStatus.Codes.WorkOnHold, "NO", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(JobHeaderStatus.Codes.InvoiceOnHold, "NO", "NO", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(JobHeaderStatus.Codes.CustomsProcessActive, "NO", "NO", "NO", "", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO");
			AssertDefaultValue(JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting, "YES", "YES", "YES", "YES", "", "YES", "YES", "YES", "YES", "YES", "YES", "YES");
			AssertDefaultValue(JobHeaderStatus.Codes.JobReadyForRevenuePosting, "YES", "YES", "YES", "YES", "NO", "", "YES", "YES", "YES", "YES", "YES", "YES");
			AssertDefaultValue(JobHeaderStatus.Codes.JobReadyForCostPosting, "YES", "YES", "YES", "YES", "NO", "YES", "", "YES", "YES", "YES", "YES", "YES");
			AssertDefaultValue(JobHeaderStatus.Codes.JobReadyForDelivery, "NO", "NO", "NO", "NO", "NO", "NO", "NO", "", "NO", "NO", "NO", "NO");
			AssertDefaultValue(JobHeaderStatus.Codes.JobInvoiced, "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "", "NO", "NO", "NO");
			AssertDefaultValue(JobHeaderStatus.Codes.Complete, "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "", "YES", "YES");
			AssertDefaultValue(JobHeaderStatus.Codes.JobReadyForFinancialClosure, "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "YES", "", "YES");
			AssertDefaultValue(JobHeaderStatus.Codes.ScheduledForArchive, "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "NO", "");

			void AssertDefaultValue(string jobStatus,
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
				var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
				jobStatusUpdateRestrictionRule.SetDefaults(jobStatus);

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

		#endregion

		public void TestIsRestricted()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.SetDefaults(JobHeaderStatus.Codes.JobReadyForRevenuePosting);

			AssertEquals("Precondtion:Working", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Working.Code));
			AssertEquals("Precondtion:WorkOnHold", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.WorkOnHold.Code));
			AssertEquals("Precondtion:InvoiceOnHold", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.InvoiceOnHold.Code));
			AssertEquals("Precondtion:CustomsProcessActive", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.CustomsProcessActive.Code));
			AssertEquals("Precondtion:JobReadyForRevenuePosting", false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.JobReadyForRevenuePosting.Code));
			AssertEquals("Precondtion:JobReadyForCostPosting", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.JobReadyForCostPosting.Code));
			AssertEquals("Precondtion:JobReadyForRevenueAndCostPosting", false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code));
			AssertEquals("Precondtion:JobInvoiced", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.JobInvoiced.Code));
			AssertEquals("Precondtion:JobReadyForDelivery", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.JobReadyForDelivery.Code));
			AssertEquals("Precondtion:Complete", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Complete.Code));
			AssertEquals("Precondtion:JobReadyForFinancialClosure", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.JobReadyForFinancialClosure.Code));
			AssertEquals("Precondtion:ScheduledForArchive", true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.ScheduledForArchive.Code));

			jobStatusUpdateRestrictionRule.Working = "NO";
			jobStatusUpdateRestrictionRule.WorkOnHold = "NO";
			jobStatusUpdateRestrictionRule.InvoiceOnHold = "NO";
			jobStatusUpdateRestrictionRule.CustomsProcessActive = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting = "";
			jobStatusUpdateRestrictionRule.JobReadyForCostPosting = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting = "YES";
			jobStatusUpdateRestrictionRule.JobInvoiced = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForDelivery = "NO";
			jobStatusUpdateRestrictionRule.Complete = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure = "NO";
			jobStatusUpdateRestrictionRule.ScheduledForArchive = "NO";

			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.Working));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.WorkOnHold));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.InvoiceOnHold));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.CustomsProcessActive));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.JobReadyForRevenuePosting));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.JobReadyForCostPosting));
			AssertEquals(true, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.JobInvoiced));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.JobReadyForDelivery));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.Complete));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.JobReadyForFinancialClosure));
			AssertEquals(false, jobStatusUpdateRestrictionRule.IsRestricted(JobHeaderStatus.Codes.ScheduledForArchive));
		}

		public void TestGetRestricteValueFromJobStatus()
		{
			var jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			jobStatusUpdateRestrictionRule.SetDefaults(JobHeaderStatus.Codes.JobReadyForRevenuePosting);

			AssertEquals("Precondtion:Working", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Working.Code));
			AssertEquals("Precondtion:WorkOnHold", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.WorkOnHold.Code));
			AssertEquals("Precondtion:InvoiceOnHold", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.InvoiceOnHold.Code));
			AssertEquals("Precondtion:CustomsProcessActive", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.CustomsProcessActive.Code));
			AssertEquals("Precondtion:JobReadyForRevenuePosting", "", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.JobReadyForRevenuePosting.Code));
			AssertEquals("Precondtion:JobReadyForCostPosting", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.JobReadyForCostPosting.Code));
			AssertEquals("Precondtion:JobReadyForRevenueAndCostPosting", "NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code));
			AssertEquals("Precondtion:JobInvoiced", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.JobInvoiced.Code));
			AssertEquals("Precondtion:JobReadyForDelivery", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.JobReadyForDelivery.Code));
			AssertEquals("Precondtion:Complete", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Complete.Code));
			AssertEquals("Precondtion:JobReadyForFinancialClosure", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.JobReadyForFinancialClosure.Code));
			AssertEquals("Precondtion:ScheduledForArchive", "YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.ScheduledForArchive.Code));

			jobStatusUpdateRestrictionRule.Working = "NO";
			jobStatusUpdateRestrictionRule.WorkOnHold = "NO";
			jobStatusUpdateRestrictionRule.InvoiceOnHold = "NO";
			jobStatusUpdateRestrictionRule.CustomsProcessActive = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForRevenuePosting = "";
			jobStatusUpdateRestrictionRule.JobReadyForCostPosting = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPosting = "YES";
			jobStatusUpdateRestrictionRule.JobInvoiced = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForDelivery = "NO";
			jobStatusUpdateRestrictionRule.Complete = "NO";
			jobStatusUpdateRestrictionRule.JobReadyForFinancialClosure = "NO";
			jobStatusUpdateRestrictionRule.ScheduledForArchive = "NO";

			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.Working));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.WorkOnHold));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.InvoiceOnHold));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.CustomsProcessActive));
			AssertEquals("", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.JobReadyForRevenuePosting));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.JobReadyForCostPosting));
			AssertEquals("YES", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.JobReadyForRevenueAndCostPosting));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.JobInvoiced));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.JobReadyForDelivery));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.Complete));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.JobReadyForFinancialClosure));
			AssertEquals("NO", jobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(JobHeaderStatus.Codes.ScheduledForArchive));
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new JobStatusUpdateRestrictionRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override void TestBizObjectField(ZPropertyInfo info)
		{
			var jobStatusUpdateRestrictionRule = info.BizObj as JobStatusUpdateRestrictionRule;
			var jobHeaderStatusList = new JobHeaderStatusList();
			jobStatusUpdateRestrictionRule.JobStatus = jobHeaderStatusList.GetAllCodes().FirstOrDefault(x => x != jobHeaderStatusList.GetCodeFromDescription(info.Name));
			base.TestBizObjectField(info);
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
		}
		JobStatusUpdateRestrictionRule jobStatusUpdateRestrictionRule;

		#endregion
	}
}
