using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class JobClosureHelper
	{
		#region Constructor
		protected JobClosureHelper(IEnumerable<Job> jobsToClose)
		{
			Argument.NotNull(jobsToClose, "jobsToClose");
			this.jobsToClose = jobsToClose;
		}
		protected readonly IEnumerable<Job> jobsToClose;

		#endregion

		#region Public Functions and Properties

		public abstract IEnumerable<Job> JobsThatCanBeClosed { get; }

		public string GetAllErrorMessages()
		{
			var discardedJobsMessage = GetAllErrorMessagesCore();
			return discardedJobsMessage.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region Functions and Properties

		protected virtual ZStringBuilder GetAllErrorMessagesCore()
		{
			var discardedJobsMessage = new ZStringBuilder();
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatAreAlreadyClosed());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForThatCannotbeClosedDueToInactive());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatWillRequireAProfitLossReasonCode());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForUserIsNotAllowedToChangeStatusOfCompleteJobs());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForMissingJobNumbersThatBelongToUnpostedConsolCostsOnSelectedJobs());
			discardedJobsMessage.AppendIfNotEmpty(GetErrorMessageForJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests());
			return discardedJobsMessage;
		}

		protected virtual void FilterListOfJobsThatCanBeClosed()
		{
			//Jobs that are already Closed
			jobsThatAreAlreadyClosed = jobsToClose.Where(x => x.IsClosed).ToList();

			//Jobs that are inactive
			jobsThatCannotbeClosedDueToInactive = jobsToClose.Where(x => !x.JH_IsActive && !x.IsClosed).ToList();

			//Jobs that require ProfitLossReasonCode
			jobsRequiringProfitLossReasonCode = new List<Job>();
			foreach (var job in jobsToClose)
			{
				if (((JobValidation)job.Validation).IsProfitLossReasonCodeInvalidForThisJobStatus(JobHeaderStatus.Closed.Code))
				{
					jobsRequiringProfitLossReasonCode.Add(job);
				}
			}

			//Jobs that has apportioned Cost but peer Jobs haven't been selected
			GetJobsThatCannotbeClosedDueToMissingPeer();

			//Not Allowed to Change Status
			notAllowedToChangeStatusJobs = new List<Job>();
			foreach (var job in jobsToClose)
			{
				if (!job.CanChangeStatusOfCompleteJobs(JobHeaderStatus.Closed.Code))
				{
					notAllowedToChangeStatusJobs.Add(job);
				}
			}

			GetJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests();
		}

		protected virtual List<Job> DiscardedJobs
		{
			get
			{
				var allDiscardedJobs = new List<Job>();
				allDiscardedJobs.AddRange(jobsThatAreAlreadyClosed);
				allDiscardedJobs.AddRange(jobsThatCannotbeClosedDueToInactive);
				allDiscardedJobs.AddRange(notAllowedToChangeStatusJobs);
				allDiscardedJobs.AddRange(jobsRequiringProfitLossReasonCode);
				allDiscardedJobs.AddRange(jobsThatCannotbeClosedDueToMissingPeer.Keys.ToList());
				allDiscardedJobs.AddRange(jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests);
				return allDiscardedJobs.Distinct().ToList();
			}
		}

		protected void GetJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests()
		{
			jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests = new List<Job>();
			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (checker.IsReceivablesCashAdvanceFunctionalityEnabled || checker.IsPayablesCashAdvanceFunctionalityEnabled)
			{
				var jobsToSkip = jobsToClose.Where(j => j.HasActiveUnInvoicedCashAdvanceRequests);
				if (jobsToSkip.Any())
				{
					jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests.AddRange(jobsToSkip);
				}
			}
		}

		protected virtual void GetJobsThatCannotbeClosedDueToMissingPeer()
		{
			//Jobs that has apportioned Cost but peer Jobs haven't been selected
			jobsThatCannotbeClosedDueToMissingPeer = new Dictionary<Job, string>();

			foreach (Job job in jobsToClose)
			{
				var allPeerJobs = job.GetAllPeerJobsThatHaveUnpostedConsolCosts().ToHashSet();
				var selectedJobPKsToClose = jobsToClose.Select(x => x.PK);
				var missingPeerJobPKs = allPeerJobs.Select(x => x.PK).Except(selectedJobPKsToClose);

				if (missingPeerJobPKs.Any())
				{
					var missingPeerJobNumbers = allPeerJobs.Where(x => missingPeerJobPKs.Contains(x.PK)).Select(x => x.JH_JobNum);
					jobsThatCannotbeClosedDueToMissingPeer.Add(job, new ZStringBuilder(missingPeerJobNumbers).ToStringWithDelimiterBetweenAppends(", "));
				}
			}
		}

		#endregion

		#region Error Messages Functions and Properties

		ZString GetErrorMessageForJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests()
		{
			return jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests.Any() ?
					TextForJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests :
					ZString.Empty;
		}

		ZString GetErrorMessageForThatCannotbeClosedDueToInactive()
		{
			return jobsThatCannotbeClosedDueToInactive.Any() ?
					TextForJobsThatCannotbeClosedDueToInactive :
					ZString.Empty;
		}

		ZString GetErrorMessageForJobsThatAreAlreadyClosed()
		{
			if (jobsThatAreAlreadyClosed.Any())
			{
				return TextForJobsThatAreAlreadyClosedErrorMessage;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetErrorMessageForJobsThatWillRequireAProfitLossReasonCode()
		{
			if (jobsRequiringProfitLossReasonCode.Any())
			{
				return TextForJobsThatWillRequireAProfitLossReasonCodeErrorMessage;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetErrorMessageForMissingJobNumbersThatBelongToUnpostedConsolCostsOnSelectedJobs()
		{
			if (jobsThatCannotbeClosedDueToMissingPeer.Any())
			{
				return TextForMissingJobNumbersThatBelongToUnpostedConsolCostsOnSelectedJobsErrorMessage;
			}
			else
			{
				return ZString.Empty;
			}
		}

		ZString GetErrorMessageForUserIsNotAllowedToChangeStatusOfCompleteJobs()
		{
			if (notAllowedToChangeStatusJobs.Any())
			{
				return TextForUserIsNotAllowedToChangeStatusOfCompleteJobsErrorMessage;
			}
			else
			{
				return ZString.Empty;
			}
		}

		protected abstract ZString TextForJobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests { get; }

		protected abstract ZString TextForJobsThatAreAlreadyClosedErrorMessage { get; }

		protected abstract ZString TextForJobsThatWillRequireAProfitLossReasonCodeErrorMessage { get; }

		protected abstract ZString TextForMissingJobNumbersThatBelongToUnpostedConsolCostsOnSelectedJobsErrorMessage { get; }

		protected abstract ZString TextForUserIsNotAllowedToChangeStatusOfCompleteJobsErrorMessage { get; }

		protected ZString TextForJobsThatCannotbeClosedDueToInactive => Res.GetString("5e45ccc4-2a9e-48b0-9e4d-a441bac32c90", "The following job(s) are inactive. Please activate them before closing:") + "\n" + ManualJobClosureHelper.GetJobNumbers(jobsThatCannotbeClosedDueToInactive) + "\n";
		#endregion

		#region Fields

		protected List<Job> jobsRequiringProfitLossReasonCode;
		protected List<Job> jobsThatAreAlreadyClosed;
		protected List<Job> jobsThatCannotbeClosedDueToInactive;
		protected Dictionary<Job, string> jobsThatCannotbeClosedDueToMissingPeer;
		protected List<Job> notAllowedToChangeStatusJobs;
		protected List<Job> jobsThatCannotbeClosedDueToActiveUnInvoicedCashAdvanceRequests;

		#endregion

		#region Static Functions
		public static string GetJobNumbers(IEnumerable<Job> jobs)
		{
			if (jobs != null)
			{
				var jobNumbers = (from Job job in jobs
								  orderby job.JH_JobNum
								  select job.JH_JobNum).ToList();

				return string.Join(", ", jobNumbers.ToArray());
			}

			return string.Empty;
		}

		#endregion
	}
}
