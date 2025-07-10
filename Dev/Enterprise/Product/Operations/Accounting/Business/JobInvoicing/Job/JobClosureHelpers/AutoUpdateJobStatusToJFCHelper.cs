using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AutoUpdateJobStatusToJFCHelper
	{
		#region Constructor

		public AutoUpdateJobStatusToJFCHelper(Job jobUpdateJobStatusToJFC)
		{
			this.jobUpdateJobStatusToJFC = jobUpdateJobStatusToJFC;
			this.jobUpdateJobStatusToJFC.InitializeParentFromGenericJobWithoutSettingDefaults();
			JobCannotBeUpdateJobStatusToJFCDueToAge = false;
			JobCannotBeUpdateJobStatusToJFCDueToStatus = false;
			JobStatusIsAlreadyJFC = false;
			JobStatusIsAlreadyCLS = false;
			AutoUpdateJobStatusToJFCEligibilityVerificationDetails = new ZStringBuilder();
			FilterOfJobCanBeUpdatedJobStatusToJFC();
		}

		#endregion

		#region Public Functions and Properties

		public bool JobThatCanBeUpdatedJobStatusToJFC => !(JobCannotBeUpdateJobStatusToJFCDueToAge || JobCannotBeUpdateJobStatusToJFCDueToStatus || JobStatusIsAlreadyJFC || JobStatusIsAlreadyCLS);

		public string GetAutoUpdateJobStatusToJFCEligibilityVerificationDetailsOfJob()
		{
			return AutoUpdateJobStatusToJFCEligibilityVerificationDetails.ToStringWithNewLineBetweenAppends();
		}

		public string GetAutoUpdateJobStatusToJFCEligibilityVerificationDetails()
		{
			return FormattableString.Invariant($@"[{jobUpdateJobStatusToJFC.JH_JobNum}]:
{AutoUpdateJobStatusToJFCEligibilityVerificationDetails.ToStringWithNewLineBetweenAppends()}");
		}

		#endregion

		#region Implementations

		#region Functions and Properties

		public string GetAllErrorMessages()
		{
			var discardedJobsMessage = new ZStringBuilder();
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobNotAllowedToUpdateJobStatusToJFCDueToAge());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobNotAllowedToUpdateJobStatusToJFCDueToStatus());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobStatusIsAlreadyJFC());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobStatusIsAlreadyCLS());
			return discardedJobsMessage.ToStringWithNewLineBetweenAppends();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Added to Service Task Logs., Service Task Logs should be in English Only")]
		void FilterOfJobCanBeUpdatedJobStatusToJFC()
		{
			AppendToVerificationMessage(FormattableString.Invariant($@"Job Type: {jobUpdateJobStatusToJFC.JobType?.Code ?? string.Empty} | Direction: {jobUpdateJobStatusToJFC.ServiceDirection} | Mode: {jobUpdateJobStatusToJFC.TransportMode} | Department: {jobUpdateJobStatusToJFC.Department.GE_Code} | Status: {jobUpdateJobStatusToJFC.JH_Status}."));
			var config = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(jobUpdateJobStatusToJFC.JH_GC, jobUpdateJobStatusToJFC.JobType?.Code ?? ZString.Empty, jobUpdateJobStatusToJFC.Direction, jobUpdateJobStatusToJFC.TransportMode, jobUpdateJobStatusToJFC.JH_GE, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			if (config != null)
			{
				AppendToVerificationMessage(FormattableString.Invariant($@"Matched Configuration: (JobType: {config.JobType}-Direction: {config.DirectionCode}- Mode: {config.Mode}- Department: {config.Department?.GE_Code}- From Status: {config.FromJobStatus}) -> (Date Option: {config.JobClosureDateOptionCode}- Offset: {config.Offset} {config.OffsetType})."));

				JobStatusIsAlreadyCLS = VerifyWhetherJobStatusIsAlreadyClosed();
				JobStatusIsAlreadyJFC = VerifyWhetherJobStatusIsAlreadyJFC();
				JobCannotBeUpdateJobStatusToJFCDueToStatus = VerifyWhetherJobCannotBeUpdatedJobStatusToJFCDueToStatus(config);
				JobCannotBeUpdateJobStatusToJFCDueToAge = VerifyWhetherJobsAreOldEnoughToBeAutoUpdatedJobStatusToJFC(config);

				if (JobStatusIsAlreadyCLS || JobStatusIsAlreadyJFC || JobCannotBeUpdateJobStatusToJFCDueToStatus || JobCannotBeUpdateJobStatusToJFCDueToAge)
				{
					AppendToVerificationMessage((NoResString)"Did not satisfy registry settings.");
				}
				else
				{
					AppendToVerificationMessage((NoResString)"Satisfied registry settings.");
				}
			}
			else
			{
				JobCannotBeUpdateJobStatusToJFCDueToAge = true;
				AppendToVerificationMessage("No matching configuration found.");
			}
		}

		bool VerifyWhetherJobStatusIsAlreadyClosed() => jobUpdateJobStatusToJFC.IsClosed;

		bool VerifyWhetherJobStatusIsAlreadyJFC() => jobUpdateJobStatusToJFC.IsReadyForFinancialClosure;

		bool VerifyWhetherJobCannotBeUpdatedJobStatusToJFCDueToStatus(JobClosureConfiguration config) => !config.IsJobInAllowedStatusToBeUpdated(jobUpdateJobStatusToJFC);

		bool VerifyWhetherJobsAreOldEnoughToBeAutoUpdatedJobStatusToJFC(JobClosureConfiguration config)
		{
			var discardJob = false;
			//Verify each job against registry Days Offset settings to see whether they can be updated job status to JFC
			var (calculatedJobReadyForFinancialClosureDate, message) = TryToGetJobReadyForFinancialClosureDateAndMessage(config);
			if (!calculatedJobReadyForFinancialClosureDate.IsValid || calculatedJobReadyForFinancialClosureDate.Date.ToDateTime() > Env.Time.CurrentLocalDate.Date)
			{
				JobCannotBeUpdateJobStatusToJFCDueToAge = true;
				discardJob = true;
			}
			AppendToVerificationMessage(message);
			return discardJob;
		}

		void AppendToVerificationMessage(string msgToAppend)
		{
			AutoUpdateJobStatusToJFCEligibilityVerificationDetails.Append(msgToAppend);
		}

		(ZDateTime calculatedJobReadyForFinancialClosureDate, string message) TryToGetJobReadyForFinancialClosureDateAndMessage(JobClosureConfiguration config = null)
		{
			var jobReadyForFinancialClosureDate = ZDateTime.Empty;
			var msg = string.Empty;

			if (config != null)
			{
				var significantDate = AutoJobClosureHelper.TryToGetJobSignificantDate(jobUpdateJobStatusToJFC, config);
				jobReadyForFinancialClosureDate = AutoJobClosureHelper.GetThresholdDate(significantDate, config.OffsetType, config.Offset, 1);

				var significantDateString = significantDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
				var calculatedJobReadyForFinancialClosureDateString = jobReadyForFinancialClosureDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

				msg = FormattableString.Invariant($@"Calculated significant Date: {significantDateString}.
Calculated earliest job ready for financial closure date: {calculatedJobReadyForFinancialClosureDateString}.");
			}

			return (jobReadyForFinancialClosureDate, msg);
		}

		#endregion

		#region Error Messages Functions and Properties

		ZString GetErrorMessageForJobNotAllowedToUpdateJobStatusToJFCDueToAge() => JobCannotBeUpdateJobStatusToJFCDueToAge
			? TextForJobNotAllowedToUpdateJobStatusToJFCDueToAgeErrorMessage
			: ZString.Empty;

		ZString GetErrorMessageForJobNotAllowedToUpdateJobStatusToJFCDueToStatus() => JobCannotBeUpdateJobStatusToJFCDueToStatus
			? TextForJobCannotBeUpdateJobStatusToJFCDueToStatusErrorMessage
			: ZString.Empty;

		ZString GetErrorMessageForJobStatusIsAlreadyJFC() => JobStatusIsAlreadyJFC
			? TextForJobStatusIsAlreadyJFCErrorMessage
			: ZString.Empty;

		ZString GetErrorMessageForJobStatusIsAlreadyCLS() => JobStatusIsAlreadyCLS
			? TextForJobStatusIsAlreadyCLSErrorMessage
			: ZString.Empty;

		ZString TextForJobStatusIsAlreadyCLSErrorMessage
		{
			get { return FormattableString.Invariant($"Following job status is already CLS: {jobUpdateJobStatusToJFC.JH_JobNum}"); }
		}

		ZString TextForJobStatusIsAlreadyJFCErrorMessage
		{
			get { return FormattableString.Invariant($"Following job status is already JFC: {jobUpdateJobStatusToJFC.JH_JobNum}"); }
		}

		ZString TextForJobNotAllowedToUpdateJobStatusToJFCDueToAgeErrorMessage
		{
			get { return FormattableString.Invariant($"Following job are not old enough to be automatically updated job status to JFC: {jobUpdateJobStatusToJFC.JH_JobNum}"); }
		}

		ZString TextForJobCannotBeUpdateJobStatusToJFCDueToStatusErrorMessage
		{
			get { return FormattableString.Invariant($"Following job could not be automatically updated job status to JFC due to their current status: {jobUpdateJobStatusToJFC.JH_JobNum}"); }
		}

		#endregion

		#region Fields

		readonly Job jobUpdateJobStatusToJFC;
		bool JobStatusIsAlreadyCLS { get; set; }
		bool JobStatusIsAlreadyJFC { get; set; }
		bool JobCannotBeUpdateJobStatusToJFCDueToAge { get; set; }
		bool JobCannotBeUpdateJobStatusToJFCDueToStatus { get; set; }
		ZStringBuilder AutoUpdateJobStatusToJFCEligibilityVerificationDetails { get; }

		#endregion

		#endregion
	}
}
