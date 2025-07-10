using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class ARCreditNoteApprovalBulk : TransactionApprovalBulk<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public ARCreditNoteApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params ARCreditNoteApprovalRequest[] approvalRequests)
			: base(factory, interactiveSecurityOverrideProvider, approvalRequests)
		{
		}

		public List<ARCreditNoteApprovalRequest> SetARCreditNoteApprovalsStatus(string status, out bool errorsAlreadyHandled)
		{
			errorsAlreadyHandled = false;
			var approvingUser = GlbStaff.CurrentUser.GS_Code;
			var allowedApprovals = new List<ARCreditNoteApprovalRequest>();
			var loginController = new UserLoginController();
			if (Approvals.Count == 1)
			{
				errorsAlreadyHandled = true;
				var isAllowed = status == Constants.GenApprovalRequestApprovalStatus.Cancelled && GlbStaff.CurrentUser.GS_Code == Approvals[0].XP_SystemCreateUser;
				SecurityCheckpoint checkpoint = null;
				if (!isAllowed)
				{
					checkpoint = Approvals[0].GetSecurityCheckpointForRequest(loginController);
					isAllowed = checkpoint == null || InteractiveSecurityOverrideProvider.SecurityCertificates[checkpoint].IsAllowed;
				}

				if (isAllowed)
				{
					if (checkpoint == null)
					{
						approvingUser = GlbStaff.CurrentUser.GS_Code;
					}
					else if (InteractiveSecurityOverrideProvider.UserSecurityOverride != null)
					{
						approvingUser = Factory.GetCachedReadOnlyFactory().Load<GlbStaff>(InteractiveSecurityOverrideProvider.UserSecurityOverride.UserPK).GS_Code;
					}
					allowedApprovals.Add(Approvals[0]);
				}
			}
			else if (Approvals.Count > 1)
			{
				foreach (var approval in Approvals)
				{
					if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled && GlbStaff.CurrentUser.GS_Code == approval.XP_SystemCreateUser
						|| ((approval.XP_SystemCreateUser != GlbStaff.CurrentUser.GS_Code || AllowToApproveOwnRequest)
							&& (approval.GetSecurityCheckpointForRequest(loginController)?.IsAllowed ?? true)))
					{
						allowedApprovals.Add(approval);
					}
				}
			}

			// When auto posting the credit note on approval any associated closed jobs will be
			// reopened by the log subscriber. Ensure the approver has the required permissions.
			if (status == Constants.GenApprovalRequestApprovalStatus.Approved
				&& AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.Value)
			{
				var finalApprovalsWithClosedJobs = allowedApprovals
					.Where(x => x.IsFinalApproval && GetJobsForApprovalRequest(x)
						.Any(y => y.JH_Status == JobHeaderStatus.Closed.Code))
					.ToList();

				if (finalApprovalsWithClosedJobs.Count > 0)
				{
					var closedJobs = finalApprovalsWithClosedJobs
						.SelectMany(x => GetJobsForApprovalRequest(x))
						.Where(x => x.JH_Status == JobHeaderStatus.Closed.Code)
						.ToList();

					if (!CanReopenClosedJobs(closedJobs))
					{
						allowedApprovals = allowedApprovals.Except(finalApprovalsWithClosedJobs).ToList();
					}
				}
			}

			UpdateApprovalUserAndStatusForRequests(allowedApprovals, status, approvingUser);
			if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled && allowedApprovals.Any())
			{
				allowedApprovals.ToList().ForEach(x => x.SetContext(BusinessContext.CancelApprovalRequestByUser));
			}

			return allowedApprovals;
		}

		IEnumerable<Job> GetJobsForApprovalRequest(ARCreditNoteApprovalRequest request)
		{
			if (request.IsTransactionRelated)
			{
				return ((InvoicingBase)request.Parent).RelatedJobsForReversing;
			}

			return Enumerable.Empty<Job>();
		}

		bool CanReopenClosedJobs(IList<Job> closedJobs)
		{
			var closedJobsPastReopenPeriod = closedJobs
				.Where(x => x.IsPastAllowedRestrictionDate())
				.ToArray();

			if (!HasSecurityCheckpointForJobs(Env.Security.ReopenJobPastAllowedReOpenPeriod, closedJobsPastReopenPeriod))
			{
				return false;
			}
			LogJobReopenSecurityOverride(InteractiveSecurityOverrideProvider.UserSecurityOverride, closedJobsPastReopenPeriod);

			var closedJobsWithinReopenPeriod = closedJobs.Except(closedJobsPastReopenPeriod).ToArray();
			if (!HasSecurityCheckpointForJobs(Env.Security.ReopenJob, closedJobsWithinReopenPeriod))
			{
				return false;
			}
			LogJobReopenSecurityOverride(InteractiveSecurityOverrideProvider.UserSecurityOverride, closedJobsWithinReopenPeriod);

			return true;
		}

		bool HasSecurityCheckpointForJobs(SecurityCheckpoint checkpoint, Job[] jobs)
		{
			var provider = InteractiveSecurityOverrideProvider as IReopenClosedJobSecurityOverrideProvider;
			Func<SecurityCheckpoint, bool> hasCheckpoint = (checkpoint) => InteractiveSecurityOverrideProvider.SecurityCertificates[checkpoint]?.IsAllowed ?? false;

			if (jobs.Length > 0)
			{
				provider?.AddClosedJobForSecurityProvider(jobs);
				return hasCheckpoint(checkpoint);
			}

			return true;
		}

		void LogJobReopenSecurityOverride(SecurityCore authorizingUser, IEnumerable<Job> affectedJobs)
		{
			ZGuid authorizingUserPK = authorizingUser?.UserPK ?? Guid.Empty;
			if (authorizingUserPK.IsValid)
			{
				var staff = Factory.Load<GlbStaff>(authorizingUserPK);
				if (staff != null)
				{
					var message = string.Format((NoResString)"Job reopening authorized by {0} ({1})", staff.GS_LoginName, staff.GS_FullName);
					affectedJobs.ForEach(x => x.Logs.AddNew(Events.Authorised, message));
				}
			}
		}

		protected override bool SetApprovingUserAndStatus(bool isActionAllowed, bool isCurrentUserRequestSelected, string status, InvoicingBase transactionToApprove, bool needToUpdateApproverOneForSequentialMode = false)
		{
			string approvingUser = null;
			if (!isActionAllowed &&
					(!isCurrentUserRequestSelected || AllowToApproveOwnRequest))
			{
				SecurityOverrideProviderSource.Get(transactionToApprove).Provider = InteractiveSecurityOverrideProvider;

				if (CheckLevelSecurityRights(transactionToApprove))
				{
					isActionAllowed = true;
				}

				SecurityOverrideProviderSource.Get(transactionToApprove).Provider = null;
			}
			if (needToUpdateApproverOneForSequentialMode)
			{
				var requestsInSEQMode = Approvals.Where(x => x.PostingDetails.ApprovingOption == ApprovalCredentialOption.SequentialLogin);
				var requestsInOtherModes = Approvals.Where(x => x.PostingDetails.ApprovingOption != ApprovalCredentialOption.SequentialLogin);
				if (isActionAllowed)
				{
					UpdateApprovalUserAndStatusForRequests(requestsInOtherModes, status, approvingUser);
				}
				requestsInSEQMode.ForEach(x => x.SetLevel1ApproverToCurrentUserForSEQRequest());
			}
			else
			{
				if (isActionAllowed)
				{
					UpdateApprovalUserAndStatusForRequests(Approvals, status, approvingUser);
				}
			}

			return isActionAllowed;
		}

		void UpdateApprovalUserAndStatusForRequests(IEnumerable<ARCreditNoteApprovalRequest> approvalRequests, string status, string approvingUser)
		{
			if (!approvalRequests.Any())
			{
				return;
			}

			var approvalRequestUserPairs = new List<KeyValuePair<Guid, string>>();

			foreach (var approvingUserMapping in CreditNoteApprovingUserMapping)
			{
				var creditNoteRequestPair = RequestToCreditNoteMapping.First(x => x.Value == approvingUserMapping.Key);
				var approval = approvalRequests.First(x => x.PK == creditNoteRequestPair.Key);

				var approvalPKWithSameBranchDepartment = approvalRequests.Where(x => x.XP_GB_JobBranch == approval.XP_GB_JobBranch && x.XP_GE_JobDepartment == approval.XP_GE_JobDepartment).Select(x => x.PK.ToGuid()).ToList();
				foreach (var approvalRequestPK in approvalPKWithSameBranchDepartment)
				{
					var keyValuePair = new KeyValuePair<Guid, string>(approvalRequestPK, Factory.GetCachedReadOnlyFactory().Load<GlbStaff>(approvingUserMapping.Value).GS_Code);
					approvalRequestUserPairs.Add(keyValuePair);
				}
			}

			foreach (var approvalRequest in approvalRequests)
			{
				approvalRequest.XP_ApprovalStatus = status;
				approvingUser = approvingUser ?? approvalRequestUserPairs.Where(x => x.Key == approvalRequest.PK).Select(x => x.Value).FirstOrDefault();

				if (status == Constants.GenApprovalRequestApprovalStatus.Approved ||
						status == Constants.GenApprovalRequestApprovalStatus.Rejected)
				{
					approvalRequest.UpdateApprovalUserAndStatus(approvingUser, status);
				}
				if (status == Constants.GenApprovalRequestApprovalStatus.Cancelled ||
					status == Constants.GenApprovalRequestApprovalStatus.Rejected)
				{
					approvalRequest.FinalizeCancelling();
				}
			}
		}

		protected override InvoicingBase GetTransactionWithHighestApprovalLevel()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var transactionToApprove = (InvoicingBase)factory.GetCachedReadOnlyFactory().New(Approvals[0].ApprovingTransactionType);
			transactionToApprove.TransactionsForAuthorisationCalculation = new List<InvoicingBase>();
			RequestToCreditNoteMapping = new Dictionary<Guid, Guid>();

			foreach (var approvalRequest in Approvals)
			{
				var transaction = (InvoicingBase)factory.GetCachedReadOnlyFactory().New(approvalRequest.ApprovingTransactionType);
				transaction.AH_LocalExTaxAmount = approvalRequest.PostingDetails.MaxAmountToApprove;
				transaction.AH_GB = approvalRequest.XP_GB_JobBranch.IsEmpty ? Env.CurrentBranchPK : approvalRequest.XP_GB_JobBranch;
				transaction.AH_GE = approvalRequest.XP_GE_JobDepartment.IsEmpty ? Env.CurrentDepartmentPK : approvalRequest.XP_GE_JobDepartment;
				if (!approvalRequest.PostingDetails.Description.IsEmpty)
				{
					transaction.AH_Desc = approvalRequest.PostingDetails.Description;
				}
				if (!approvalRequest.PostingDetails.InvoiceTerm.IsEmpty)
				{
					transaction.AH_InvoiceTerm = approvalRequest.PostingDetails.InvoiceTerm;
					transaction.AH_InvoiceTermDays = approvalRequest.PostingDetails.InvoiceTermDays;
				}

				if (transaction is ARInvoice)
				{
					transaction.IsCreatingCreditNoteForReversal = true;
				}

				if (!transactionToApprove.TransactionsForAuthorisationCalculation.Contains(transaction))
				{
					transactionToApprove.TransactionsForAuthorisationCalculation.Add(transaction);
				}

				if (!RequestToCreditNoteMapping.ContainsKey(approvalRequest.PK.ToGuid()))
				{
					RequestToCreditNoteMapping.Add(approvalRequest.PK.ToGuid(), transaction.PK.ToGuid());
				}
			}

			return transactionToApprove;
		}

		protected override bool CheckLevelSecurityRights(InvoicingBase invoice)
		{
			Dictionary<Guid, Guid> creditNoteApprovedByUserPairs = new Dictionary<Guid, Guid>();
			CreditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
			bool result = ARCreditNoteLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(invoice, out creditNoteApprovedByUserPairs, true);
			CreditNoteApprovingUserMapping = creditNoteApprovedByUserPairs;

			return result;
		}

		Dictionary<Guid, Guid> CreditNoteApprovingUserMapping
		{
			get
			{
				if (creditNoteApprovingUserMapping == null)
				{
					creditNoteApprovingUserMapping = new Dictionary<Guid, Guid>();
				}

				return creditNoteApprovingUserMapping;
			}
			set
			{
				creditNoteApprovingUserMapping = value;
			}
		}
		Dictionary<Guid, Guid> creditNoteApprovingUserMapping;

		Dictionary<Guid, Guid> RequestToCreditNoteMapping
		{
			get
			{
				if (requestToCreditNoteMapping == null)
				{
					requestToCreditNoteMapping = new Dictionary<Guid, Guid>();
				}

				return requestToCreditNoteMapping;
			}
			set
			{
				requestToCreditNoteMapping = value;
			}
		}

		Dictionary<Guid, Guid> requestToCreditNoteMapping;
	}
}
