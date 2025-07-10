using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class ChinaComplianceInfoEInvoicingExtension : ChinaComplianceInfo, IEInvoicingTransactionValidation
	{
		public ZString GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType) => ZString.Empty;

		public ZString GetCantReverseErrorMessage(IReversing originalTransaction)
		{
			var message = ZString.Empty;
			var transaction = originalTransaction as InvoicingBase;
			var cannotReversePivotStatus = new List<string> { EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingPivotState.Sent };

			if (transaction != null
				&& AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetFallBackValueAtAllLevels(transaction.Company.PK.ToGuid(), transaction.Branch.PK.ToGuid(), transaction.Department.PK.ToGuid())
				&& !string.IsNullOrEmpty(AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(transaction.Company.PK.ToGuid(), transaction.Branch.PK.ToGuid(), transaction.Department.PK.ToGuid())))
			{
				var pivotStatus = transaction.GetMostRecentEInvoicingTransactionPivot()?.AIP_Status ?? ZString.Empty;
				if (cannotReversePivotStatus.Contains(transaction.GetMostRecentEInvoicingTransactionPivot()?.AIP_Status ?? ZString.Empty))
				{
					message = Res.GetString("37792D81-9C5C-4529-993B-AF413CAC91B3", "You cannot reverse the AR invoice because it is in the process of E-Reporting.");
				}
				else if (pivotStatus == EInvoicingPivotState.Succeed)
				{
					var tuple = GetReferenceStatusAndAmount(transaction);
					var complianceDocumentStatus = tuple.status;
					var voidedAndCreditedAmount = tuple.amount;

					if (!GetCanReverseComplianceDocumentStatusCodes().Contains(complianceDocumentStatus))
					{
						message = Res.GetString("b84ebe66-1b31-4453-9e78-46e2917ad900", @"Reversal is allowed only if the compliance document status is CDD, CDV or CDR.
The current compliance document status is {0}.
Please get latest e-Invoice status via '{1}' action menu before attempting to reverse this invoice.", complianceDocumentStatus, DocumentRequestMenuName);
					}
					else if (GetNeedCheckVoidedAndCreditedAmountStatusCodes().Contains(complianceDocumentStatus))
					{
						if (Math.Abs(voidedAndCreditedAmount) < (transaction.AH_InvoiceAmount + transaction.AH_GSTAmount))
						{
							message = Res.GetString("32275a5c-3a5e-4d25-a166-9085db8d8704", @"The invoice has been split to multiple VAT invoices.
Reversal is allowed only if all related VAT invoices have been credited or voided.
If all related VAT invoices have been credited or voided, please get latest e-Invoice status via 'Request e-Invoice Status' action menu before attempting to reverse this invoice.");
						}
					}
				}
			}

			return message;
		}

		(ZString status, ZDecimal amount) GetReferenceStatusAndAmount(InvoicingBase invoice)
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS);
			query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, invoice.PK);
			var reference = invoice.Factory.LoadTop1<AccTransactionHeaderReference>(query);

			var result = (ZString.Empty, ZDecimal.Zero);
			if (reference != null)
			{
				result = (reference.AH1_Reference, reference.AH1_Amount);
			}
			return result;
		}

		public ZString GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
		{
			var message = ZString.Empty;

			if (!pivots.Any(x => x.IsSubmitPivotSucceedOrDelivered))
			{
				message = NotEligibleForRequestsMessage;
			}

			return message;
		}
	}
}
