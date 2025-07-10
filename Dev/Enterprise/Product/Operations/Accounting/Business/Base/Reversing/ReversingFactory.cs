using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public class ReversingFactory
	{
		public ReversingBase NewReversing(IReversing originalTransaction)
		{
			return NewReversing(originalTransaction, null);
		}

		public ReversingBase NewReversing(IReversing originalTransaction, JobInvoicingSecurityHelper securityHelper)
		{
			ReversingBase result = null;
			if (originalTransaction is IPayment)
			{
				result = new PaymentReversing((IPayment)originalTransaction);
			}
			else if (originalTransaction is Contra contra)
			{
				result = new ContraReversing(contra);
			}
			else if (originalTransaction is Receipt)
			{
				result = new ReceiptReversing((Receipt)originalTransaction);
			}
			else if (originalTransaction is APInvoice apInvoice)
			{
				result = new APInvoiceReversing(apInvoice);
			}
			else if (originalTransaction is ARInvoice arInvoice)
			{
				result = new ARInvoiceReversing(arInvoice, securityHelper);
			}
			else if (originalTransaction is InvoiceBatchHeader)
			{
				result = new InvoiceBatchReversing(originalTransaction);
			}
			else if (originalTransaction is InvoicingBase)
			{
				result = new InvoicingBaseReversing((IPayablesAndReceivables)originalTransaction);
			}
			else if (originalTransaction is IPayablesAndReceivables)
			{
				result = new PayablesAndReceivablesReversing((IPayablesAndReceivables)originalTransaction);
			}
			else if (originalTransaction is IDirectReceipt)
			{
				result = new DirectReceiptReversing((IDirectReceipt)originalTransaction);
			}
			else if (originalTransaction is IDirectPayment)
			{
				result = new DirectPaymentReversing((IDirectPayment)originalTransaction);
			}
			else if (originalTransaction is ICashBook)
			{
				result = new CashBookReversing((ICashBook)originalTransaction);
			}
			else if (originalTransaction is IJobCosting)
			{
				result = new JobCostingReversing((IJobCosting)originalTransaction);
			}
			else if (originalTransaction is IGeneralLedger)
			{
				if (originalTransaction is IAutoCurrencyAdjustmentGLJournal journal && journal.IsAutoCurrencyAdjustmentGLJournal)
				{
					result = new AutoCurrencyAdjustmentGLJournalReversing((IGeneralLedger)originalTransaction);
				}
				else
				{
					result = new GLJournalReversing((IGeneralLedger)originalTransaction);
				}
			}
			else if (originalTransaction is DepositBatchParent)
			{
				result = new DepositBatchReversing(originalTransaction);
			}
			else if (originalTransaction is IMiscellaneousTransaction && originalTransaction is IReversing)
			{
				result = new MiscellaneousTransactionReversing(originalTransaction);
			}
			return result;
		}
	}
}
