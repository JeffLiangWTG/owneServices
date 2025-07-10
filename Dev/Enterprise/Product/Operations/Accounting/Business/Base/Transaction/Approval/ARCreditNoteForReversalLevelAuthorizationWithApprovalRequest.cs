using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest : ARCreditNoteLevelAuthorizationWithApprovalRequest
	{
		public ARCreditNoteForReversalLevelAuthorizationWithApprovalRequest(IPostingJobTransactionsApprovalGUIProvider postingGUIProvider, bool isReversingMultipleFromReceivablesModule = false, string reversingCode = "") : base(postingGUIProvider)
		{
			TransactionsWithoutApprovedRequest = new HashSet<ZGuid>();
			IsReversingMultipleFromReceivablesModule = isReversingMultipleFromReceivablesModule;
			ReversingCode = reversingCode;
		}

		public HashSet<ZGuid> TransactionsWithoutApprovedRequest { get; }
		readonly bool IsReversingMultipleFromReceivablesModule;
		readonly string ReversingCode;

#if DEBUG
		public bool IsTestingCreditNoteReversalReasonForm { get; set; }
#endif

		public Tuple<bool, bool> PerformLevelAuthorizationForReversing(InvoicingBase[] transactions)
		{
			var canContinueReversing = false;
			transactions.ForEach(x => x.IsCreatingCreditNoteForReversal = true);
			var result = PerformTransactionLevelAuthorization(transactions, out canContinueReversing, shouldCreateRequestPerTransaction: true);
			return Tuple.Create(canContinueReversing, result);
		}

		protected override ARCreditNoteApprovalRequest CreateNewApprovalRequest(BusinessObjectFactory factory)
		{
			var request = base.CreateNewApprovalRequest(factory);
			request.ChangeApprovalTypeForInvoiceReversal();
			if (!string.IsNullOrEmpty(ReversingCode))
			{
				request.XP_ReasonCode = ReversingCode;
			}
			return request;
		}

		protected override void InitializeApprovalRequestCore(ARCreditNoteApprovalRequest request, InvoicingBase[] transactions)
		{
			//Assumption : initialize one request per invoice
			request.Initialize(new[] { transactions[0] }, transactions[0].PK, AccTransactionHeaderSchema.Constants.Prefix, postingGUIProvider.PostingOption);
		}

		protected override Type GetTransactionType()
		{
			return typeof(ARInvoice);
		}

		protected override string GetMessageCaption(InvoicingBase maxTransaction)
		{
			return Res.GetString("ddea316b-98a0-46ec-9ee0-efc32bd6ad63", "Accounts Receivable Credit Note Approval Request");
		}

#if DEBUG
		public
#else
		protected
#endif
		override bool ConfirmAndPreEditApprovalRequestByUser(TransactionApprovalRequest<ARCreditNoteApprovalRequestDetails>[] approvalRequests, InvoicingBase maxTransaction)
		{
			if (approvalRequests.Length == 1)
			{
				return base.ConfirmAndPreEditApprovalRequestByUser(approvalRequests, maxTransaction);
			}
			else
			{
				var reversingReasonCode = ZString.Empty;
				var reversingReasonDescription = ZString.Empty;
#if DEBUG
				if (Globals.IsTest && !IsTestingCreditNoteReversalReasonForm)
				{
					reversingReasonCode = Core.Constants.GenApprovalRequestReasonCode.Code.IncorrectCharges;
					reversingReasonDescription = "Incorrect Charges";
				}
				else
				{
#endif
					var result = postingGUIProvider.ShowCreditNoteReversalReasonForm(approvalRequests.FirstOrDefault()?.XP_ReasonCode ?? string.Empty);
					reversingReasonCode = result.Item1;
					reversingReasonDescription = result.Item2;
#if DEBUG
				}
#endif

				var isCodeAndDescritionNotEmpty = !string.IsNullOrEmpty(reversingReasonCode) && !string.IsNullOrEmpty(reversingReasonDescription);
				if (isCodeAndDescritionNotEmpty)
				{
					foreach (var request in approvalRequests)
					{
						request.XP_ReasonCode = reversingReasonCode;
						request.XP_ReasonDescription = reversingReasonDescription;
					}

					if (!maxTransaction.LevelAuthorizationRequired)
					{
						foreach (var approvalRequest in approvalRequests)
						{
							PrepopulateApprovingUser1ForMultipleLogin(approvalRequest);
						}
					}
				}
				return isCodeAndDescritionNotEmpty;
			}
		}

		protected override void ShowExitMessage(bool isCancellingOutFromLoginForm)
		{
			var messageText = string.Empty;
			if (isCancellingOutFromLoginForm)
			{
				messageText = Res.GetString("afdf36a7-1084-435b-acae-faef5da098bd", "You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can reverse these transactions.");
			}
			else
			{
				messageText = Res.GetString("6936cc9d-0b6c-4d2a-b8b0-06bf7fb55f62", "You do not have security rights to post a credit note for the required amount. An approval request has been queued. Once the approval has been granted, you can reverse these transactions.");
			}
			postingGUIProvider.ShowMessage(messageText, GetMessageCaption(null), ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
		}

		protected override void ProcessApprovedRequests(List<ITransactionApprovalHelper> approvalHelpers, List<InvoicingBase> approvedTransactions)
		{
			foreach (var approvalHelper in approvalHelpers)
			{
				var approvingRequest = approvalHelper.PostApprovedRequestForThisPostingDetails();//change APP to PST
				if (approvingRequest is ARCreditNoteApprovalRequest arRequest)
				{
					OnPostApprovedRequestForThisPostingDetails(arRequest, approvedTransactions);
				}
			}
		}

		protected override void DeleteNewApprovalRequestCreatedForApprovedRequest(List<ITransactionApprovalHelper> helpersForApprovedRequests)
		{
			helpersForApprovedRequests.Cast<ARCreditNoteApprovalHelper>().ForEach(x => x.DeleteNewReqeustCreatedForApprovedRequest());
		}

		protected override void UpdateExisitingApprovalRequestsStatus(InvoicingBase[] transactions)
		{
			var approvalRequests = new List<ARCreditNoteApprovalRequest>() { };
			var approvalHelpers = new List<ITransactionApprovalHelper>() { };
			CreateApprovalRequests(transactions.ToList(), approvalRequests, true);
			approvalRequests.ForEach(x => approvalHelpers.Add(GetNewHelper(x)));
			approvalHelpers.Where(x => x.IsThereApprovedRequestForThisPostingDetails).ForEach(x => x.PostApprovedRequestForThisPostingDetails()); //Change APP to PST
			approvalHelpers.Where(x => x.AreThereAnyApprovalRequestForThisPostingActionWithRequestedStatus).ForEach(x => x.CancelApprovalRequestForThisPostingActionWithRequestedOrErrorStatus()); //Change REQ to CAN
			approvalRequests.ForEach(x => x.Delete());
		}

		protected override bool PopulateTransactionsWithoutApprovedRequest(List<ITransactionApprovalHelper> approvalHelpers, List<ITransactionApprovalHelper> helpersForApprovedRequests, List<InvoicingBase> approvedTransactions)
		{
			var shouldResetFactory = true;
			if (IsReversingMultipleFromReceivablesModule)
			{
				approvalHelpers.Except(helpersForApprovedRequests).Cast<ARCreditNoteApprovalHelper>().ForEach(x => TransactionsWithoutApprovedRequest.Add(x.ParentId));
				if (helpersForApprovedRequests.Any())
				{
					ProcessApprovedRequests(helpersForApprovedRequests, approvedTransactions);
					shouldResetFactory = false;
				}
			}
			return shouldResetFactory;
		}
	}
}
