using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class ARCreditNoteApprovalRequestInvoiceReversingHelper
	{
		public static bool HandleReversing(ARCreditNoteApprovalRequest request, Action<LogType, string> appendToNotificationLogDelegate)
		{
			bool canSaveRequestFactory = false;
			if (request != null)
			{
				var transactionToReverse = request.Parent as InvoicingBase;
				if (transactionToReverse != null)
				{
					transactionToReverse.SecurityOverrideProvider = new DefaultAccessSecurityProvider();
					var reversingBase = new ReversingFactory().NewReversing(transactionToReverse);
					if (!reversingBase.CanReverseTransaction)
					{
						var errorMessage = FormattableString.Invariant($"Transaction {transactionToReverse.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} could not be reversed. {reversingBase.CantReverseErrorMessage}");
						appendToNotificationLogDelegate(LogType.Error, errorMessage);
						canSaveRequestFactory = false;
					}
					else
					{
						try
						{
							transactionToReverse.Factory.SetContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);
							transactionToReverse.ReOpenClosedJob();
						}
						finally
						{
							transactionToReverse.Factory.RemoveContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);
						}

						reversingBase.Reverse();
						SetReverseProperties(reversingBase, request);
						new ARCreditNoteApprovalHelper(request).PostApprovedRequestForThisPostingDetails();

						var message = FormattableString.Invariant($"Transaction {transactionToReverse.AH_TransactionNum} linked to ARCreditNoteApprovalRequest {request.PK} can be reversed.");
						appendToNotificationLogDelegate(LogType.Information, message);
						canSaveRequestFactory = true;
					}
				}
			}
			return canSaveRequestFactory;
		}

		static void SetReverseProperties(ReversingBase reversingBase, ARCreditNoteApprovalRequest request)
		{
			var reasonCode = request.XP_ReasonCode;
			var reversingReason = AccountingMasterFilesRegistry.Instance.CreditNoteReasonCodesList.Value.GetDescriptionFromCode(reasonCode) ?? ZString.Empty;
			reversingBase.ReverseTransaction.ReversingCode = reasonCode;
			reversingBase.ReverseTransaction.ReversingReason = reversingReason;

			if (reversingBase.ReverseTransaction is CreditNote creditNote)
			{
				creditNote.ApprovingUserPKList = request.GetApprovingUserPKs();
				creditNote.ApprovalDate = request.XP_ApprovalDate;
			}
		}
	}
}
