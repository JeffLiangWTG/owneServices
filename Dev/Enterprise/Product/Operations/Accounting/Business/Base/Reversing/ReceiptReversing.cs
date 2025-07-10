using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class ReceiptReversing : PayablesAndReceivablesReversing
	{
		public ReceiptReversing(IPayablesAndReceivables payablesAndReceivablesTransaction)
			: base(payablesAndReceivablesTransaction)
		{
		}

		protected override void DoReverseTransaction()
		{
			if (OriginalTransaction is ARReceipt receipt)
			{
				receipt.LoadCARJournals();
			}
			base.DoReverseTransaction();
		}

		#region GenerateReverseTransactions

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			Receipt originalReceipt = OriginalPayablesAndReceivables as Receipt;
			if (originalReceipt != null)
			{
				DepositBatch originalDepositBatch = originalReceipt.RelatedDepositBatch;
				if (originalDepositBatch != null)
				{
					ReverseDepositBatch = OriginalTransaction.Factory.New<DepositBatch>();
					ReverseDepositBatch.IsReverseTransaction = true;
					ReverseDepositBatch.ReversingReceipt = originalReceipt;
					ReverseDepositBatch.AH_OSTotal = originalReceipt.IsBankCurrencyLocal ? -originalReceipt.AH_LocalExTaxAmount : -originalReceipt.AH_OSTotalAmount;
					ReverseDepositBatch.AH_InvoiceAmount = -originalReceipt.AH_LocalExTaxAmount;
					ReverseDepositBatch.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					ReverseDepositBatch.AH_PostDate = ZDateTime.Now;
					ReverseDepositBatch.AH_InvoiceDate = ZDateTime.Now;
					ReverseDepositBatch.AH_DueDate = ZDateTime.Now;
					ReverseDepositBatch.AH_AB = originalDepositBatch.AH_AB;
					ReverseDepositBatch.AH_OH = originalDepositBatch.AH_OH;
					ReverseDepositBatch.AH_Desc = (NoResString)"Cancellation of " + originalDepositBatch.AH_ReceiptBatchNo;
				}
			}
		}

		#endregion

		#region SetOtherNumberFountainFields

		protected override void SetOtherNumberFountainFields(BusinessObjectFactory factory)
		{
			base.SetOtherNumberFountainFields(factory);
			if (ReverseDepositBatch != null)
			{
				ZString nextDepositBatchNo = AccountingNumberFountainWrapperFactory.Instance.BatchReceiptNo.GetNext(ReverseDepositBatch.Factory);
				Receipt revReceipt = (Receipt)ReversePayablesAndReceivables;
				revReceipt.AH_ReceiptBatchNo = nextDepositBatchNo;
				ReverseDepositBatch.AH_TransactionNum = nextDepositBatchNo;
				ReverseDepositBatch.AH_ReceiptBatchNo = nextDepositBatchNo;
			}
		}

		#endregion

		#region CanTransactionBeReversed

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() &&
					!OriginalPayablesAndReceivables.IsClearedInCashbook &&
					string.IsNullOrEmpty(GetErrorMessageIfThereIsAnyInvoicedCashAdvance());
		}

		#endregion

		#region GenerateCantReverseErrorMessage

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
			if (OriginalPayablesAndReceivables is ARReceipt receipt)
			{
				var errorMessagesKvps = new Dictionary<string, List<string>>();
				var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessagesKvps);
				validator.Visit(receipt);

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

		#endregion

		DepositBatch ReverseDepositBatch;

		protected override void AddSuspendersToTheFactory()
		{
			ServiceContainerSuspenderHelper.FunctionalitySuspender<ChargeWithCost.OnFactorySavingInCompanyContextSuspender>.GetSuspender(OriginalTransaction.Factory);
		}
	}
}
