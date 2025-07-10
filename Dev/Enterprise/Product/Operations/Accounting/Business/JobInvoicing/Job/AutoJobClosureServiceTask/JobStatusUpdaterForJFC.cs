using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobStatusUpdaterForJFC : JCSSubscriber
	{
		public JobStatusUpdaterForJFC(IJCSLogger logger) : base(logger)
		{
		}

		protected override string Code => "JFC";

		protected override bool CanProcess(Job job)
		{
			var result = false;
			if (base.CanProcess(job))
			{
				var config = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(job.JH_GC, job.JobType?.Code ?? ZString.Empty, job.Direction, job.TransportMode, job.JH_GE, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
				if (config != null)
				{
					Logger.LogDebug("Verifying whether this job can be updated job status to JFC.");

					var helper = new AutoUpdateJobStatusToJFCHelper(job);
					Logger.LogDiagnostic(FormattableString.Invariant($"Auto Update Job Status To JFC eligibility verification details - \r\n {helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails()}"));

					if (helper.JobThatCanBeUpdatedJobStatusToJFC)
					{
						job.CustomLogReferenceSuffixExtraDetails = helper.GetAutoUpdateJobStatusToJFCEligibilityVerificationDetailsOfJob();
						result = true;
					}
					else
					{
						Logger.LogDiagnostic(FormattableString.Invariant($"Job status cannot be updated to JFC. \r\n{helper.GetAllErrorMessages()}"));
					}
				}
			}
			return result;
		}

		protected override bool Process(Job jobUpdateJobStatusToJFC, bool changesWillBeSavedToDB)
		{
			if (changesWillBeSavedToDB)
			{
				Logger.LogDebug(FormattableString.Invariant($"Attempting to update status to JFC for {jobUpdateJobStatusToJFC.JH_JobNum}."));
				jobUpdateJobStatusToJFC.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
				Logger.LogDebug(FormattableString.Invariant($"Status of {jobUpdateJobStatusToJFC.JH_JobNum} is changed from '{(ZString)jobUpdateJobStatusToJFC.JH_StatusInfo.OriginalValue}' to '{jobUpdateJobStatusToJFC.JH_Status}'"));
			}
			else
			{
				Logger.LogDiagnostic("This job is eligible for updating status to JFC.");
			}
			return true;
		}
	}
}
