using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ManualJobClosureHelper : JobClosureHelper
	{
		public static ManualJobClosureHelper CreateHelper(IEnumerable<Job> jobsToClose)
		{
			var helper = new ManualJobClosureHelper(jobsToClose);
			helper.FilterListOfJobsThatCanBeClosed();
			return helper;
		}

		ManualJobClosureHelper(IEnumerable<Job> jobsToClose)
			: base(jobsToClose)
		{
		}

		public override IEnumerable<Job> JobsThatCanBeClosed => jobsToClose.Except(DiscardedJobs);

		public void DeleteConsolCostsLinkedToUnpostedApportionedChargesIfAny()
		{
			if (JobsThatCanBeClosed.Any())
			{
				var jobsWithUnpostedConsolCosts = JobsThatCanBeClosed.Where(j => j.UnpostedConsolCostsLinkedToThisJob.Any());
				JobConsolCost.DeleteUnpostedConsolCostsLinkedWithJobs(jobsWithUnpostedConsolCosts);
			}
		}

		protected override ZString TextForJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests
		{
			get
			{
				return Res.GetString("f1ca37a2-5dba-4373-8333-1bd8fc824065", "Following job(s) could not be closed due to active Advance Payment: {0}", GetJobNumbers(jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests));
			}
		}

		protected override ZString TextForJobsThatAreAlreadyClosedErrorMessage
		{
			get
			{
				return Res.GetString("3e0c7d77-e4ff-46a4-8e96-bb8f24353d49", "The following job(s) are already closed:") + "\n" + GetJobNumbers(jobsThatAreAlreadyClosed) + "\n";
			}
		}

		protected override ZString TextForJobsThatWillRequireAProfitLossReasonCodeErrorMessage
		{
			get
			{
				return Res.GetString("57b42950-3e9a-42b2-9368-659746e58406", @"The following job(s) require a Reason Code because their Profit Margin falls outside the tolerated margin threshold.
Please assign a Job Profit / Loss Reason Code to these jobs.
{0}", GetJobNumbers(jobsRequiringProfitLossReasonCode)) + "\n";
			}
		}

		protected override ZString TextForMissingJobNumbersThatBelongToUnpostedConsolCostsOnSelectedJobsErrorMessage
		{
			get
			{
				ZStringBuilder notClosedJobsText = new ZStringBuilder();
				foreach (KeyValuePair<Job, string> item in jobsThatCannotbeClosedDueToMissingPeer)
				{
					notClosedJobsText.AppendLine(Res.GetString("97bf71db-e6f1-49b3-8988-c00aba7313c9", "To close Job: {0} please select {1}", item.Key.JH_JobNum, item.Value));
				}
				return Res.GetString("1fa14398-9e62-486b-af2f-eb93001f0fb3", @"Following job(s) contain apportioned charges.
{0}", notClosedJobsText.ToString()) + "\n";
			}
		}

		protected override ZString TextForUserIsNotAllowedToChangeStatusOfCompleteJobsErrorMessage
		{
			get
			{
				return Res.GetString("6ce3ed90-cc02-4a6e-b6d2-e9594f83dd30", @"The following job(s) are complete and you are not allowed to change their status due to security rights:
{0}", GetJobNumbers(notAllowedToChangeStatusJobs)) + "\n";
			}
		}
	}
}
