using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AutoJobClosureHelper : JobClosureHelper
	{
		#region Helper Creators

		public static AutoJobClosureHelper CreateHelperToCloseAJob(Job jobToClose) => CreateHelper(new Job[] { jobToClose });

		public static AutoJobClosureHelper CreateHelperToCloseAJobAlongWithItsPeers(Job jobToClose)
		{
			var allJobsToClose = new List<Job>();
			allJobsToClose.Add(jobToClose);
			allJobsToClose.AddRange(jobToClose.GetAllPeerJobsThatHaveUnpostedConsolCosts());
			return CreateHelper(allJobsToClose.ToArray());
		}

		static AutoJobClosureHelper CreateHelper(Job[] jobsToClose)
		{
			var helper = new AutoJobClosureHelper(jobsToClose);
			helper.FilterListOfJobsThatCanBeClosed();
			return helper;
		}

		#endregion

		#region Constructor

		AutoJobClosureHelper(Job[] jobsToClose)
			: base(jobsToClose)
		{
			jobsToClose.ForEach(j => j.InitializeParentFromGenericJobWithoutSettingDefaults());
			JobsThatCannotBeClosedDueToAge = new List<Job>();
			JobsWithOpenWIPsAndAccruals = new List<Job>();
			JobsThatCannotBeClosedDueToStatus = new List<Job>();
			JobsThatCannotBeClosedDueToUnrecognizedCharge = new List<Job>();
			AutoClosureEligibilityVerificationDetails = new Dictionary<Job, ZStringBuilder>();
		}

		#endregion

		#region Public Functions and Properties

		public override IEnumerable<Job> JobsThatCanBeClosed => DiscardedJobs.Any() ? new List<Job>() : jobsToClose.ToList();

		public IEnumerable<Job> JobsToClose => jobsToClose.ToList();

		public string GetAutoJobClosureEligibilityVerificationDetailsOfJob(ZGuid jobPK)
		{
			var details = string.Empty;
			var key = AutoClosureEligibilityVerificationDetails.Keys.FirstOrDefault(k => k.PK == jobPK);
			if (key != null)
			{
				details = AutoClosureEligibilityVerificationDetails[key].ToStringWithNewLineBetweenAppends();
			}
			return details;
		}

		public string GetAutoJobClosureEligibilityVerificationDetails()
		{
			return string.Join("\r\n\r\n", AutoClosureEligibilityVerificationDetails.Select(x => FormattableString.Invariant($@"[{x.Key.JH_JobNum}]:
{x.Value.ToStringWithNewLineBetweenAppends()}")).ToArray());
		}

		#endregion

		#region Implementations

		#region Functions and Properties
		protected override ZStringBuilder GetAllErrorMessagesCore()
		{
			var discardedJobsMessage = base.GetAllErrorMessagesCore();
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatNotAllowCloseWithOpenWIPsandAccruals());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatNotAllowedToCloseDueToAge());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatNotAllowedToCloseDueToStatus());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatNotAllowedToClosedDueToCostOrSellRecognitionStatus());
			return discardedJobsMessage;
		}

		protected override void FilterListOfJobsThatCanBeClosed()
		{
			base.FilterListOfJobsThatCanBeClosed();
			foreach (var job in jobsToClose)
			{
				AppendToVerificationMessage(job, FormattableString.Invariant($@"Job Type: {job.JobType?.Code ?? string.Empty} | Direction: {job.ServiceDirection} | Mode: {job.TransportMode} | Department: {job.Department.GE_Code} | Open WIP: {HasOpenWip(job).ToYesNoString()} | Open Accrual: {HasOpenAccrual(job).ToYesNoString()} | Status: {job.JH_Status} | Has Unrecognized Amount: {HasUnrecognizedAmounts(job).ToYesNoString()} | Has Recognized Amount: {HasRecognizedAmounts(job).ToYesNoString()}."));
				var config = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(job.JH_GC, job.JobType?.Code ?? ZString.Empty, job.Direction, job.TransportMode, job.JH_GE, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
				if (config != null)
				{
					AppendToVerificationMessage(job, FormattableString.Invariant($@"Matched Configuration: (JobType: {config.JobType}-Direction: {config.DirectionCode}- Mode: {config.Mode}- Department: {config.Department?.GE_Code}- Open WIP: {((bool)config.CloseJobsWithOpenWip).ToYesNoString()}- Open Accrual: {((bool)config.CloseJobsWithOpenAcr).ToYesNoString()}- From Status: {config.FromJobStatus}- Charge Filter: {config.JobChargeRecognitionFilter}) -> (Date Option: {config.JobClosureDateOptionCode}- Offset: {config.Offset} {config.OffsetType})."));

					var discardJob = false;
					discardJob |= VerifyWhetherJobCannotBeClosedDueToStatus(job, config);
					discardJob |= VerifyWhetherJobsWithUnrecognizedChargeCanBeClosed(job, config);
					discardJob |= VerifyWhetherJobsWithOpenWIPAndACRCanBeClosed(job, config);
					discardJob |= VerifyWhetherJobsAreOldEnoughToBeAutoClosed(job, config);

					if (discardJob)
					{
						AppendToVerificationMessage(job, (NoResString)"Did not satisfy registry settings.");
					}
					else
					{
						AppendToVerificationMessage(job, (NoResString)"Satisfied registry settings.");
					}
				}
				else
				{
					JobsThatCannotBeClosedDueToAge.Add(job);
					AppendToVerificationMessage(job, (NoResString)"No matching configuration found.");
				}
			}
		}

		bool VerifyWhetherJobsWithOpenWIPAndACRCanBeClosed(Job job, JobClosureConfiguration config)
		{
			var shouldDiscard = (!config.CloseJobsWithOpenWip && HasOpenWip(job)) || (!config.CloseJobsWithOpenAcr && HasOpenAccrual(job));
			if (shouldDiscard)
			{
				JobsWithOpenWIPsAndAccruals.Add(job);
			}
			return shouldDiscard;
		}

		bool VerifyWhetherJobCannotBeClosedDueToStatus(Job job, JobClosureConfiguration config)
		{
			var shouldDiscard = !config.IsJobInAllowedStatusToBeUpdated(job);
			if (shouldDiscard)
			{
				JobsThatCannotBeClosedDueToStatus.Add(job);
			}
			return shouldDiscard;
		}

		bool VerifyWhetherJobsWithUnrecognizedChargeCanBeClosed(Job job, JobClosureConfiguration config)
		{
			var discardJob = false;
			switch (config?.JobChargeRecognitionFilter ?? string.Empty)
			{
				case ChargeRecognitionFilterOptionList.Codes.NRC:
					discardJob = HasRecognizedAmounts(job);
					break;
				case ChargeRecognitionFilterOptionList.Codes.REC:
					discardJob = HasUnrecognizedAmounts(job);
					break;
				case ChargeRecognitionFilterOptionList.Codes.ALL:
					discardJob = false;
					break;
			}

			if (discardJob)
			{
				JobsThatCannotBeClosedDueToUnrecognizedCharge.Add(job);
			}
			return discardJob;
		}

		protected override void GetJobsThatCannotbeClosedDueToMissingPeer()
		{
			jobsThatCannotbeClosedDueToMissingPeer = new Dictionary<Job, string>();
			//do nothing
		}

		bool HasOpenWip(Job job) => job?.Charges.OfType<BaseCharge>().Any(x => x.WIP != null) ?? false;

		bool HasOpenAccrual(Job job) => job?.Charges.OfType<BaseCharge>().Any(x => x.Accrual != null) ?? false;

		bool HasRecognizedAmounts(Job job)
		{
			if (job == null)
			{
				return false;
			}
			var charges = job.Charges.OfType<BaseCharge>();
			return charges.Any(x => x.ARLine != null && ((x.ARLine.AL_LineType == TransactionLineTypes.WIP && !x.ARLine.AL_ReverseDate.IsValid) || (x.ARLine.AL_LineType == TransactionLineTypes.Revenue && x.ARLine.AL_ReverseDate.IsValid)))
					|| charges.Any(x => x.APLine != null && ((x.APLine.AL_LineType == TransactionLineTypes.Accrual && !x.APLine.AL_ReverseDate.IsValid) || (x.APLine.AL_LineType == TransactionLineTypes.Cost && x.APLine.AL_ReverseDate.IsValid)));
		}

		bool HasUnrecognizedAmounts(Job job)
		{
			if (job == null)
			{
				return false;
			}
			var charges = job.Charges.OfType<BaseCharge>();
			return charges.Any(x => x.JR_OSSellAmt != 0 && (x.ARLine == null || (x.ARLine.AL_LineType == TransactionLineTypes.Revenue && !x.ARLine.AL_ReverseDate.IsValid)))
				|| charges.Any(x => x.JR_OSCostAmt != 0 && (x.APLine == null || (x.APLine.AL_LineType == TransactionLineTypes.Cost && !x.APLine.AL_ReverseDate.IsValid)));
		}

		bool VerifyWhetherJobsAreOldEnoughToBeAutoClosed(Job job, JobClosureConfiguration config)
		{
			var discardJob = false;
			//Verify each job against registry Days Offset settings to see whether they can be closed
			var (calculatedClosureDate, message) = TryToGetJobClosureDateAndMessage(job, config);
			if (!calculatedClosureDate.IsValid || calculatedClosureDate.Date.ToDateTime() > Env.Time.CurrentLocalDate.Date)
			{
				JobsThatCannotBeClosedDueToAge.Add(job);
				discardJob = true;
			}
			AppendToVerificationMessage(job, message);
			return discardJob;
		}

		void AppendToVerificationMessage(Job job, string msgToAppend)
		{
			if (!AutoClosureEligibilityVerificationDetails.ContainsKey(job))
			{
				AutoClosureEligibilityVerificationDetails.Add(job, new ZStringBuilder(msgToAppend));
			}
			else
			{
				AutoClosureEligibilityVerificationDetails[job].Append(msgToAppend);
			}
		}

		protected override List<Job> DiscardedJobs
		{
			get
			{
				var allDiscardedJobs = base.DiscardedJobs;
				allDiscardedJobs.AddRange(JobsWithOpenWIPsAndAccruals);
				allDiscardedJobs.AddRange(JobsThatCannotBeClosedDueToStatus);
				allDiscardedJobs.AddRange(JobsThatCannotBeClosedDueToUnrecognizedCharge);
				allDiscardedJobs.AddRange(JobsThatCannotBeClosedDueToAge);
				return allDiscardedJobs;
			}
		}

		(ZDateTime calculatedClosureDate, string message) TryToGetJobClosureDateAndMessage(Job job, JobClosureConfiguration config = null)
		{
			var jobClosureDate = ZDateTime.Empty;
			var msg = string.Empty;

			if (config != null)
			{
				var significantDate = TryToGetJobSignificantDate(job, config);
				jobClosureDate = GetThresholdDate(significantDate, config.OffsetType, config.Offset, 1);

				var significantDateString = significantDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
				var calculatedClosureDateString = jobClosureDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

				msg = FormattableString.Invariant($@"Calculated significant Date: {significantDateString}.
Calculated earliest job closure date: {calculatedClosureDateString}.");
			}

			return (jobClosureDate, msg);
		}

		internal static ZDateTime TryToGetJobSignificantDate(Job job, JobClosureConfiguration config)
		{
			var significantDate = ZDateTime.Empty;

			switch (config.JobClosureDateOptionCode)
			{
				case RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate:
					significantDate = job.JH_A_JOP;
					break;
				case RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction:
					significantDate = job.CalculateFARRevenueRecognitionDate();
					break;
			}

			if (job.PlugInData != null && !job.IsPluginDataDeleted && significantDate.IsEmpty)
			{
				var resultDate = job.PlugInData.InvoicingSupporter.GetOperationsSignificantDateByDirection(config.JobClosureDateOptionCode, job.Direction);
				if (!resultDate.IsEmpty)
				{
					significantDate = resultDate;
				}
			}

			return significantDate;
		}

		#endregion

		#region Error Messages Functions and Properties

		ZString GetErrorMessageForJobsThatNotAllowCloseWithOpenWIPsandAccruals() => (JobsWithOpenWIPsAndAccruals != null && JobsWithOpenWIPsAndAccruals.Any())
			? TextForJobsThatNotAllowedToCloseWithOpenWIPsandAccrualsErrorMessage
			: ZString.Empty;

		ZString GetErrorMessageForJobsThatNotAllowedToCloseDueToAge() => (JobsThatCannotBeClosedDueToAge != null && JobsThatCannotBeClosedDueToAge.Any())
			? TextForJobsThatNotAllowedToCloseDueToAgeErrorMessage
			: ZString.Empty;

		ZString GetErrorMessageForJobsThatNotAllowedToCloseDueToStatus() => (JobsThatCannotBeClosedDueToStatus != null && JobsThatCannotBeClosedDueToStatus.Any())
			? TextForJobsThatNotAllowedToClosedDueToStatusErrorMessage
			: ZString.Empty;

		ZString GetErrorMessageForJobsThatNotAllowedToClosedDueToCostOrSellRecognitionStatus() => (JobsThatCannotBeClosedDueToUnrecognizedCharge != null && JobsThatCannotBeClosedDueToUnrecognizedCharge.Any())
			? TextForJobsThatNotAllowedToClosedDueToCostOrSellRecognitionStatusErrorMessage
			: ZString.Empty;

		protected override ZString TextForJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests
		{
			get { return FormattableString.Invariant($"Following job(s) could not be closed due to active Advance Payment: {GetJobNumbers(jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests)}"); }
		}

		protected override ZString TextForJobsThatAreAlreadyClosedErrorMessage
		{
			get { return FormattableString.Invariant($"Following job(s) are already closed: {GetJobNumbers(jobsThatAreAlreadyClosed)}"); }
		}

		protected override ZString TextForJobsThatWillRequireAProfitLossReasonCodeErrorMessage
		{
			get { return FormattableString.Invariant($"Following job(s) require a Profit-Loss reason code for closing: {GetJobNumbers(jobsRequiringProfitLossReasonCode)}"); }
		}

		protected override ZString TextForMissingJobNumbersThatBelongToUnpostedConsolCostsOnSelectedJobsErrorMessage
		{
			get { return FormattableString.Invariant($"{GetJobNumbers(jobsThatCannotbeClosedDueToMissingPeer.Keys)} contain unposted apportioned cost. But could not delete the cost, as one or more peer job(s) are not available for auto job closure."); }
		}

		protected override ZString TextForUserIsNotAllowedToChangeStatusOfCompleteJobsErrorMessage
		{
			get { return FormattableString.Invariant($"{GetJobNumbers(notAllowedToChangeStatusJobs)}: Not allowed to Change the status of a Complete Job"); }
		}

		ZString TextForJobsThatNotAllowedToCloseWithOpenWIPsandAccrualsErrorMessage
		{
			get { return FormattableString.Invariant($"Following job(s) has open WIP/Accrual: {GetJobNumbers(JobsWithOpenWIPsAndAccruals)}"); }
		}

		ZString TextForJobsThatNotAllowedToCloseDueToAgeErrorMessage
		{
			get { return FormattableString.Invariant($"Following job(s) are not old enough to be automatically closed: {GetJobNumbers(JobsThatCannotBeClosedDueToAge)}"); }
		}

		ZString TextForJobsThatNotAllowedToClosedDueToStatusErrorMessage
		{
			get { return FormattableString.Invariant($"Following job(s) could not be automatically closed due to their current status: {GetJobNumbers(JobsThatCannotBeClosedDueToStatus)}"); }
		}

		ZString TextForJobsThatNotAllowedToClosedDueToCostOrSellRecognitionStatusErrorMessage
		{
			get { return FormattableString.Invariant($"Following job(s) could not be automatically closed due to charges' cost/sell recognition status: {GetJobNumbers(JobsThatCannotBeClosedDueToUnrecognizedCharge)}"); }
		}

		#endregion

		#region Fields

		List<Job> JobsThatCannotBeClosedDueToAge { get; }
		List<Job> JobsWithOpenWIPsAndAccruals { get; }
		List<Job> JobsThatCannotBeClosedDueToStatus { get; }
		List<Job> JobsThatCannotBeClosedDueToUnrecognizedCharge { get; }
		Dictionary<Job, ZStringBuilder> AutoClosureEligibilityVerificationDetails { get; }

		#endregion

		#endregion

		#region Static functions

		//Negative value of the offset (when multiplier = -1) has been used here to calculate the Threshold Date 
		//Any job which has a significant date (e.g. JOP, FAR, VAD, VDD, ARV, PIC etc. Which one of these will be used depends on Registry settings) older than the Threshold date 
		//will be eligible for adding to job queue. 
		//If this mechanism is not used than auto job closer service task would pick any 'not-Closed' job and close it with a job closure date which may be a future date
		public static ZDateTime GetThresholdDate(ZDateTime baseDate, string offsetType, ZInt offset, int multiplier)
		{
			if (baseDate.IsValid && offsetType == JobConfigurationSelectorHelper.OffsetTypeCodes.Days)
			{
				return baseDate.AddDays(multiplier * offset);
			}
			else if (baseDate.IsValid && offsetType == JobConfigurationSelectorHelper.OffsetTypeCodes.Month)
			{
				return baseDate.AddMonths(multiplier * offset);
			}
			else
			{
				return ZDateTime.Empty;
			}
		}

		#endregion
	}
}
