using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobStatusUpdaterForClosingJobs : JCSSubscriber
	{
		public JobStatusUpdaterForClosingJobs(IJCSLogger logger) : base(logger)
		{
		}

		protected override string Code => "JCS";

		protected override bool CanProcess(Job job)
		{
			var result = false;
			if (base.CanProcess(job))
			{
				var config = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(job.JH_GC, job.JobType?.Code ?? ZString.Empty, job.Direction, job.TransportMode, job.JH_GE, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
				if (config != null)
				{
					Logger.LogDebug("Verifying whether this job can be closed.");

					var helper = AutoJobClosureHelper.CreateHelperToCloseAJob(job);
					Logger.LogDiagnostic(FormattableString.Invariant($"Auto Job Closure eligibility verification details - \r\n {helper.GetAutoJobClosureEligibilityVerificationDetails()}"));

					if (helper.JobsThatCanBeClosed.Any())
					{
						job.CustomLogReferenceSuffixExtraDetails = helper.GetAutoJobClosureEligibilityVerificationDetailsOfJob(job.PK);
						result = true;
					}
					else
					{
						Logger.LogDiagnostic(FormattableString.Invariant($"This job cannot be closed. \r\n{helper.GetAllErrorMessages()}"));
					}
				}
			}
			return result;
		}

		protected override bool Process(Job jobToClose, bool changesWillBeSavedToDB)
		{
			var result = false;
			if (DeleteUnpostedConsolCostsIfAny(jobToClose))
			{
				Logger.LogDebug(FormattableString.Invariant($"Attempting to close {jobToClose.JH_JobNum}."));
				Job.ErrorMessageHandler errorHandler = (x, errMsg) => Logger.LogDiagnostic(FormattableString.Invariant($"[{jobToClose.Company.GC_Code}]: An error occured. Couldn't close {jobToClose.JH_JobNum}. \r\n {errMsg}"));
				EventHandler<UserQueryEventArgs> queryEventHandler = (sender, e) => { e.Response = false; };
				jobToClose.Close(errorHandler, queryEventHandler);

				if (jobToClose.IsClosed)
				{
					if (changesWillBeSavedToDB)
					{
						jobToClose.JH_A_JCL = Env.Time.CurrentLocalDate; //check whether we need to do it at all.
						Logger.LogDebug(FormattableString.Invariant($"Status of {jobToClose.JH_JobNum} is changed from '{(ZString)jobToClose.JH_StatusInfo.OriginalValue}' to '{jobToClose.JH_Status}' and Job Close Date is updated to {jobToClose.JH_A_JCL.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)}"));
					}
					else
					{
						Logger.LogDiagnostic("This job is eligible for automatic closing.");
					}
					result = true;
				}
			}
			return result;
		}

		bool DeleteUnpostedConsolCostsIfAny(Job job)
		{
			if (job.UnpostedConsolCostsLinkedToThisJob.Any())
			{
				Logger.LogDebug(FormattableString.Invariant($"Attempting to delete unposted consol cost(s) associated with job: \r\n {job.JH_JobNum}"));
				var consolCostDeletionResult = JobConsolCost.DeleteUnpostedConsolCostsLinkedWithJobs(new[] { job });
				Logger.LogDebug(consolCostDeletionResult.message);
				return consolCostDeletionResult.isDeleted;
			}
			else
			{
				Logger.LogDebug("There is no unposted consol cost to delete.");
				return true;
			}
		}
	}
}
