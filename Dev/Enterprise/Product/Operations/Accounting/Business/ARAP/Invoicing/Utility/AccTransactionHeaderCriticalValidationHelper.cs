using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccTransactionHeaderCriticalValidationHelper : IAccTransactionHeaderCriticalValidationHelper
	{
		public CriticalValidationHelperResult CheckCancelledTransactionHeaderWithNoLineLinkedToCharge(AccTransactionHeader target)
		{
			var result = true;
			var errorMsg = string.Empty;
			var emptyResult = new CriticalValidationHelperResult(true, string.Empty);
			var isFromAccTransactionHeader = true;
			var factory = target.Factory;

			if (!VerifyCriteria(target))
			{
				return emptyResult;
			}

			// Only verify AP/AR Invoice/CreditNote or JobRevenueJournal.
			// UA already has its own critical validation : CancelledUnapprovedPayableTransactionsShouldNotHaveLines
			var id = target.PK;
			TransactionHeaderWithLines header = null;

			if (VerifySubType(target))
			{
				header = (TransactionHeaderWithLines)target;
				isFromAccTransactionHeader = false;
			}
			else if (target.AH_TransactionType == TransactionTypes.Invoice && target.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				header = factory.Load<APInvoice>(id);
			}
			else if (target.AH_TransactionType == TransactionTypes.Invoice && target.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				header = factory.Load<ARInvoice>(id);
			}
			else if (target.AH_TransactionType == TransactionTypes.CreditNote && target.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				header = factory.Load<APCreditNote>(id);
			}
			else if (target.AH_TransactionType == TransactionTypes.CreditNote && target.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				header = factory.Load<ARCreditNote>(id);
			}
			else if (target.AH_TransactionType == TransactionTypes.JobRevenueJournal && target.AH_Ledger == LedgerTypes.JobCosting)
			{
				header = factory.Load<JobRevenueJournal>(id);
			}

			if (header == null || !header.Lines.Any())
			{
				return emptyResult;
			}

			// Verify charges against lines.
			var charges = new List<JobCharge>();
			foreach (AccTransactionLines line in header.Lines)
			{
				var charge = line.LoadRelatedJobCharge();
				if (charge != null)
				{
					charges.Add(charge);
				}
			}

			if (charges.Any())
			{
				result = false;
				var builder = new StringBuilder();

				if (isFromAccTransactionHeader)
				{
					builder.AppendLine((NoResString)"This error is captured on an AccTransactionHeader which was not loaded as AR/AP Invoice/CreditNote or Job Revenue Journal.").AppendLine();
				}

				builder.AppendLine(header.GetTransactionHeaderWithLinesInfo());

				foreach (var charge in charges)
				{
					builder.AppendLine(charge.GetJobChargeInfo());
				}

				builder.AppendLine(header.GetInvoiceApprovalInfo());

				errorMsg = builder.ToString();
			}

			return new CriticalValidationHelperResult(result, errorMsg);
		}

		public bool IsReversalOfOriginalTransaction(AccTransactionHeader target)
		{
			var result = false;
			var header = target.Factory.Load<TransactionHeader>(target.PK);
			result = header != null && header.IsReverseTransaction;
			return result;
		}

		#region Implementation

		// Create this method to reduce cyclomatic complexity and avoid CA-1502 warning.
		bool VerifyCriteria(AccTransactionHeader target)
		{
			return (target.AH_TransactionType == TransactionTypes.Invoice || target.AH_TransactionType == TransactionTypes.CreditNote || target.AH_TransactionType == TransactionTypes.JobRevenueJournal) &&
					target.AH_IsCancelled &&
					(!target.IsInDatabase || target.AH_IsCancelledInfo.HasChanges);
		}

		bool VerifySubType(AccTransactionHeader target)
		{
			return target is APInvoice || target is ARInvoice || target is APCreditNote || target is ARCreditNote || target is JobRevenueJournal;
		}

		#endregion

#if DEBUG
		public void SetIsReversalOfOriginalTransaction_ForTestOnly(AccTransactionHeader target)
		{
			var header = target.Factory.Load<TransactionHeader>(target.PK);
			if (header != null)
			{
				header.IsReverseTransaction = true;
			}
		}
#endif
	}
}