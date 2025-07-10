using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class JobStatusUpdateRestrictionRuleValidationTest : TestCaseWithFactory
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(JobStatusUpdateRestrictionRuleValidation), validation.AutoValidationType);
		}

		public void TestCheckProperties()
		{
			var jobHeaderStatusList = new JobHeaderStatusList();
			jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();

			AssertCheckProperty(jobStatusUpdateRestrictionRule.WorkingInfo, JobHeaderStatus.Working.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.WorkOnHoldInfo, JobHeaderStatus.WorkOnHold.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.InvoiceOnHoldInfo, JobHeaderStatus.InvoiceOnHold.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.CustomsProcessActiveInfo, JobHeaderStatus.CustomsProcessActive.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.JobReadyForRevenueAndCostPostingInfo, JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.JobReadyForRevenuePostingInfo, JobHeaderStatus.JobReadyForRevenuePosting.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.JobReadyForCostPostingInfo, JobHeaderStatus.JobReadyForCostPosting.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.JobReadyForDeliveryInfo, JobHeaderStatus.JobReadyForDelivery.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.JobInvoicedInfo, JobHeaderStatus.JobInvoiced.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.CompleteInfo, JobHeaderStatus.Complete.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.JobReadyForFinancialClosureInfo, JobHeaderStatus.JobReadyForFinancialClosure.Code);
			AssertCheckProperty(jobStatusUpdateRestrictionRule.ScheduledForArchiveInfo, JobHeaderStatus.ScheduledForArchive.Code);

			void AssertCheckProperty(ZPropertyInfo info, string jobStatus)
			{
				jobStatusUpdateRestrictionRule.JobStatus = jobHeaderStatusList.GetAllCodes().FirstOrDefault(x => x != jobStatus);
				AssertNoErrors("Preconditioin", info);

				info.Value = (ZString)"11";
				AssertHasError("Should have errors when value is invalid", info, "Enter a valid selection.");

				info.Value = (ZString)"";
				AssertHasError("Should have errors when value is empty", info, "Please enter a value.");

				info.Value = (ZString)"YES";
				AssertNoErrors("Should have no errors when value is 'YES'", info);

				info.Value = (ZString)"NO";
				AssertNoErrors("Should have no errors when value is 'NO'", info);

				jobStatusUpdateRestrictionRule.JobStatus = jobHeaderStatusList.GetAllCodes().FirstOrDefault(x => x == jobStatus);

				info.Value = (ZString)"YES";
				AssertHasError("Please do not enter a value when property is readonly", info, "Please do not enter a value.");

				info.Value = (ZString)"";
				AssertNoErrors("Should have no errors when property is readonly", info);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobStatusUpdateRestrictionRule = new JobStatusUpdateRestrictionRule();
			validation = jobStatusUpdateRestrictionRule.Validation;
		}

		JobStatusUpdateRestrictionRuleValidation validation;
		JobStatusUpdateRestrictionRule jobStatusUpdateRestrictionRule;
	}
}
