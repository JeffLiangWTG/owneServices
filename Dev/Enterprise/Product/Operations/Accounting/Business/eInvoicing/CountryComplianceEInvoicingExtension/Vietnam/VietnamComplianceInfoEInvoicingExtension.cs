using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance.Interfaces.ComplianceSubTypes;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class VietnamComplianceInfoEInvoicingExtension : VietnamComplianceInfo, IEInvoicingTransactionValidation, IComplianceSubTypeGUIProvider
	{
		public ZString GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType)
		{
			var message = ZString.Empty;

			if (IsCountryEnableComplianceEInvoicing() && ((AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value && transactionType == TransactionTypes.CreditNote) || (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Value && transactionType == TransactionTypes.Invoice)))
			{
				if (originalTransaction.EInvoicingStatus != Core.Constants.EInvoicingPivotState.Succeed)
				{
					message = Res.GetString("569097E8-286E-4C7E-8CFF-06816813132A", "Invoice cannot be amended until it has been submitted with status 'SUC'.");
				}
				else
				{
					var arInvoice = originalTransaction as ARInvoice;
					if (arInvoice != null)
					{
						var relatedAmendingTransactions = arInvoice.GetRelatedAmendingTransactions().Cast<InvoicingBase>();
						if (relatedAmendingTransactions.Any(x => !x.EInvoicingStatus.IsEmpty))
						{
							var amendmentTransactionType = relatedAmendingTransactions.Any(x => x.AH_TransactionType == TransactionTypes.CreditNote)
								? Res.GetString("57614caa-e307-4953-b28b-81dce19a0d09", "credit note")
								: Res.GetString("ad3a581f-298f-41d4-98cc-61f6bd8fe786", "invoice");
							var actionTransactionType = transactionType == TransactionTypes.CreditNote
								? Res.GetString("57614caa-e307-4953-b28b-81dce19a0d09", "credit note")
								: Res.GetString("ad3a581f-298f-41d4-98cc-61f6bd8fe786", "invoice");
							message = Res.GetString("de94851d-bbe1-4654-9be9-7dfce32bd2df", "The selected invoice is linked to an amendment {0} and a second amendment with {1} is not allowed.", amendmentTransactionType, actionTransactionType);
						}
					}
				}
			}

			return message;
		}

		public ZString GetCantReverseErrorMessage(IReversing originalTransaction)
		{
			var message = ZString.Empty;

			if (IsCountryEnableComplianceEInvoicing() && AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value)
			{
				var arCreditNote = originalTransaction as ARCreditNote;
				if (arCreditNote != null && arCreditNote.IsAmendingTransaction && !arCreditNote.EInvoicingStatus.IsEmpty)
				{
					message = Res.GetString("cbb251d5-6487-4a86-bd76-99de20107323", "The credit note (adjustment invoice) has been submitted for E-Reporting and cannot be reversed.");
				}
			}

			return message;
		}

		public ZString GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
		{
			var message = ZString.Empty;

			if (!pivots.Any(x => x.IsSubmitPivotSucceedOrDelivered || x.IsCancelPivotSucceed || x.IsApprovePivotSucceed))
			{
				message = NotEligibleForRequestsMessage;
			}

			return message;
		}

		public bool IsCircular78(ZInt sequnceNumberMaxDigits)
		{
			return sequnceNumberMaxDigits == 8;
		}

		public bool ComplianceSubTypeIsReadOnly(bool hasBeenCreatedAsAmending, string ledger, string transactionType, bool originalTransactionReferenceIsEmpty)
		{
			return hasBeenCreatedAsAmending && AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Value;
		}

		public bool ShouldClearComplianceSubType(ZGuid originalTransactionReference)
		{
			return false;
		}
	}
}
