#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public static class APInvoiceChargesBulkLevelAuthorizationWithApprovalRequest
	{
		public static APInvoiceCharges[] PerformLevelAuthorizationAndAddNotProcessedHere(APInvoiceCharges[] allInvoiceChargesForPostingAction)
		{
			ISecurityOverrideProviderWithApprovalRequest commonSecurityProvider = null;
			foreach (var invoiceCharges in allInvoiceChargesForPostingAction.OrderByDescending(x => x.AH_LocalTotalAmount))
			{
				if (commonSecurityProvider != null)
				{
					invoiceCharges.SecurityProviderOverride = commonSecurityProvider;
				}

				var helper = new APInvoiceChargesLevelAuthorizationWithApprovalRequest(invoiceCharges);
#if DEBUG
				helper.IsLevelAuthorizationRequired_ForTestOnly = IsLevelAuthorizationRequired_ForTestOnly;
				helper.CheckLevelSecurityRights_ForTestOnly = CheckLevelSecurityRights_ForTestOnly;
				helper.InitializeApprovalRequest_ForTestOnly = InitializeApprovalRequest_ForTestOnly;
				helper.GetNewHelper_ForTestOnly = GetNewHelper_ForTestOnly;
#endif

				helper.PerformLevelAuthorization();

				if (commonSecurityProvider == null)
				{
					commonSecurityProvider = SecurityOverrideProviderSource.Get(invoiceCharges).Provider as ISecurityOverrideProviderWithApprovalRequest;
				}
			}
#if DEBUG
			IsLevelAuthorizationRequired_ForTestOnly = null;
			CheckLevelSecurityRights_ForTestOnly = null;
			InitializeApprovalRequest_ForTestOnly = null;
			GetNewHelper_ForTestOnly = null;
#endif

			var canceledRequestsNotProcessedHere = GetAndCancelAllRelatedRequestsNotProcessedHere(allInvoiceChargesForPostingAction);

			return allInvoiceChargesForPostingAction.Concat(canceledRequestsNotProcessedHere).ToArray();
		}

		static APInvoiceCharges[] GetAndCancelAllRelatedRequestsNotProcessedHere(APInvoiceCharges[] allInvoiceChargesForPostingAction)
		{
			var isForPreviewOnly = allInvoiceChargesForPostingAction.Any(x => x.PostingGUIProvider.IsForPreviewOnly);
			var isRequestModuleOperation = allInvoiceChargesForPostingAction.Any(x => x.PostingGUIProvider.RequestToCompare != null);
			var isNothingToContinue = allInvoiceChargesForPostingAction.All(x => !x.ContinueProcessing);
			if (allInvoiceChargesForPostingAction.Length == 0 || isForPreviewOnly || isNothingToContinue || isRequestModuleOperation)
			{
				return Array.Empty<APInvoiceCharges>();
			}

			var factoryForCanceledRequestsToBeSavedLater = new BusinessObjectFactory();
			var requestsForThisPostingActionTemporaryCreatedForComparisonOnly = Array.Empty<APInvoiceChargesApprovalRequest>();
			try
			{
				requestsForThisPostingActionTemporaryCreatedForComparisonOnly = allInvoiceChargesForPostingAction.Select(invoiceCharges =>
				{
					var requestForComparisonOnly = factoryForCanceledRequestsToBeSavedLater.New<APInvoiceChargesApprovalRequest>();
					var parent = invoiceCharges.PostingGUIProvider.GetParentIdAndTableCodeForJobPostingAction();
					requestForComparisonOnly.InitializeJobRelated(invoiceCharges, parent.Item1, parent.Item2);

					return requestForComparisonOnly;
				}).ToArray();

				var canceledRequests = new List<APInvoiceChargesApprovalRequest>();
				foreach (var grouppedRequests in requestsForThisPostingActionTemporaryCreatedForComparisonOnly.GroupBy(x => x.XP_ParentID))
				{
					var helper = new APInvoiceChargesApprovalHelper(grouppedRequests.First());
					canceledRequests.AddRange(helper.GetCanceledRequestedAndApprovedRequestsNotInThisPostingAction(grouppedRequests.ToArray()));
				}

				var invoiceChargesToRepresentCanceledRequestsAndTheirFactory = canceledRequests.Select(x => new APInvoiceCharges(x));

				return invoiceChargesToRepresentCanceledRequestsAndTheirFactory.ToArray();
			}
			finally
			{
				requestsForThisPostingActionTemporaryCreatedForComparisonOnly.ForEach(x => x.Delete());
			}
		}

#if DEBUG
		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Func<APInvoiceCharges, bool> IsLevelAuthorizationRequired_ForTestOnly;

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Func<APInvoiceCharges, bool> CheckLevelSecurityRights_ForTestOnly;

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Action<APInvoiceChargesApprovalRequest, APInvoiceCharges[]> InitializeApprovalRequest_ForTestOnly;

		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Func<APInvoiceChargesApprovalRequest, ITransactionApprovalHelper> GetNewHelper_ForTestOnly;
#endif
	}

	class APInvoiceChargesLevelAuthorizationWithApprovalRequest : LevelAuthorizationWithApprovalRequest<APInvoiceCharges, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceChargesLevelAuthorizationWithApprovalRequest(APInvoiceCharges invoiceCharges)
			: base(invoiceCharges.PostingGUIProvider)
		{
			this.invoiceChargesToApprove = invoiceCharges;
		}

		readonly APInvoiceCharges invoiceChargesToApprove;

		public void PerformLevelAuthorization()
		{
			var postingGUIProvider_Casted = (IPostingJobTransactionsApprovalGUIProvider)postingGUIProvider;
			if (postingGUIProvider_Casted.IsForPreviewOnly)
			{
				invoiceChargesToApprove.ContinueProcessing = PerformTransactionLevelAuthorizationForTransactionPreviewOnly(new[] { invoiceChargesToApprove }, postingGUIProvider_Casted.RequestToCompare);
			}
			else
			{
				invoiceChargesToApprove.ContinueProcessing = PerformTransactionLevelAuthorization(new[] { invoiceChargesToApprove }, out bool canContinueReversing, postingGUIProvider_Casted.RequestToCompare != null);
			}
		}

		protected override void SetApprovingUser(Guid userPK, List<APInvoiceCharges> transactions)
		{
			invoiceChargesToApprove.ApprovingUserPK = userPK;
		}

		public static bool CheckLevelSecurityRights(ITransactionForApproval transaction, UnapprovedTransactionValidationHelper validator)
		{
			return validator.CheckLevelSecurityRights(transaction);
		}

		protected override bool ProcessExistingRequestsWithoutShowingMessages { get { return true; } }

		protected override void OnPostApprovedRequestForThisPostingDetails(APInvoiceChargesApprovalRequest postedRequest, List<APInvoiceCharges> approvedTransactions)
		{
			base.OnPostApprovedRequestForThisPostingDetails(postedRequest, approvedTransactions);

			invoiceChargesToApprove.IsActionDone_PostApprovedRequestForThisPostingDetails = true;
			invoiceChargesToApprove.PostedRequest = postedRequest;
		}

		protected override void OnCancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus(bool isUserConfirmationRequired)
		{
			base.OnCancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus(isUserConfirmationRequired);

			invoiceChargesToApprove.IsActionDone_CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus = true;
			invoiceChargesToApprove.IsUserConfirmationRequired |= isUserConfirmationRequired;
		}

		protected override void OnCancelApprovedRequestForThisPostingActionButAnotherPostingDetails(bool isUserConfirmationRequired)
		{
			base.OnCancelApprovedRequestForThisPostingActionButAnotherPostingDetails(isUserConfirmationRequired);

			invoiceChargesToApprove.IsActionDone_CancelApprovedRequestForThisPostingActionButAnotherPostingDetails = true;
			invoiceChargesToApprove.IsUserConfirmationRequired |= isUserConfirmationRequired;
		}

		protected override bool IsLevelAuthorizationRequiredCore(APInvoiceCharges invoiceCharges)
		{
			var requiredCheckpoint = ValidationHelper.RequiredSecurityCheckPoint(invoiceCharges);
			return !requiredCheckpoint.IsAllowed;
		}

		protected override bool CheckLevelSecurityRightsCore(APInvoiceCharges invoiceCharges)
		{
			return CheckLevelSecurityRights(invoiceCharges, ValidationHelper);
		}

		protected override void InitializeApprovalRequestCore(APInvoiceChargesApprovalRequest request, APInvoiceCharges[] invoiceCharges)
		{
			var parent = postingGUIProvider.GetParentIdAndTableCodeForJobPostingAction();
			request.InitializeJobRelated(this.invoiceChargesToApprove, parent.Item1, parent.Item2);
		}

		protected override ITransactionApprovalHelper GetNewHelperCore(APInvoiceChargesApprovalRequest request)
		{
			return new APInvoiceChargesApprovalHelper(request);
		}

		UnapprovedTransactionValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new UnapprovedTransactionValidationHelper()); }
		}
		UnapprovedTransactionValidationHelper validationHelper;
	}
}
