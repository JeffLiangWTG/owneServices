using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class PaymentReversing : PayablesAndReceivablesReversing
	{
		public PaymentReversing(IPayment paymentToReverse)
			: base(paymentToReverse)
		{
		}

		protected override void DoReverseTransaction()
		{
			if (OriginalTransaction is APPayment payment)
			{
				payment.LoadCAPJournals();
			}
			base.DoReverseTransaction();
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() &&
					!OriginalPayablesAndReceivables.IsClearedInCashbook &&
					string.IsNullOrEmpty(GetErrorMessageIfThereIsAnyInvoicedCashAdvance());
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			var fullErrorMessageBuilder = new ZStringBuilder();
			var result = base.GenerateCantReverseErrorMessage();
			if (!result.IsEmpty)
			{
				fullErrorMessageBuilder.Append(result);
			}

			if (OriginalPayablesAndReceivables is TransactionHeader &&
				OriginalPayablesAndReceivables.IsClearedInCashbook)
			{
				fullErrorMessageBuilder.Append(ClearedInCashBookErrorMessage);
			}

			var invoicedCashAdvanceExistsErrorMessage = GetErrorMessageIfThereIsAnyInvoicedCashAdvance();
			if (!string.IsNullOrEmpty(invoicedCashAdvanceExistsErrorMessage))
			{
				fullErrorMessageBuilder.Append(invoicedCashAdvanceExistsErrorMessage);
			}
			return fullErrorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		string GetErrorMessageIfThereIsAnyInvoicedCashAdvance()
		{
			var errorMessageBuilder = new ZStringBuilder();
			if (OriginalPayablesAndReceivables is APPayment payment)
			{
				var errorMessagesKvps = new Dictionary<string, List<string>>();
				var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessagesKvps);
				validator.Visit(payment);

				foreach (var errKvp in errorMessagesKvps)
				{
					foreach (var errorMessage in errKvp.Value)
					{
						errorMessageBuilder.Append(errorMessage);
					}
				}
			}
			return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			Payment originalPayment = OriginalPayablesAndReceivables as Payment;
			if (originalPayment != null)
			{
				ReversingDDRBatchHeader = DirectDebitBatchHeader.ReverseDDRBatch(originalPayment);
			}
		}

		protected override void SetOtherNumberFountainFields(BusinessObjectFactory factory)
		{
			base.SetOtherNumberFountainFields(factory);
			if (ReversingDDRBatchHeader != null)
			{
				ZString nextDDRBatchNo = AccountingNumberFountainWrapperFactory.Instance.DDRBatchNo.GetNext(ReversingDDRBatchHeader.Factory);
				Payment revPayment = (Payment)ReversePayablesAndReceivables;
				revPayment.AH_ReceiptBatchNo = nextDDRBatchNo;
				ReversingDDRBatchHeader.AH_TransactionNum = nextDDRBatchNo;
				ReversingDDRBatchHeader.AH_ReceiptBatchNo = nextDDRBatchNo;
			}
		}

		DirectDebitBatchHeader ReversingDDRBatchHeader;

		protected override void AddSuspendersToTheFactory()
		{
			ServiceContainerSuspenderHelper.FunctionalitySuspender<ChargeWithCost.OnFactorySavingInCompanyContextSuspender>.GetSuspender(OriginalTransaction.Factory);
		}
	}
}
