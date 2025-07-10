using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class InvoiceBatchReversing : ReversingBase
	{
		public InvoiceBatchReversing(IReversing originalTransaction)
			: base(originalTransaction)
		{
		}

		protected override void SendTransactionReversedEmail()
		{
			// DO Nothing
		}

		protected override bool CanTransactionBeReversed()
		{
			return base.CanTransactionBeReversed() && CanCancelInvoiceBatch();
		}

		protected override ZString GenerateCantReverseErrorMessage()
		{
			ZString result = base.GenerateCantReverseErrorMessage();
			if (!CanCancelInvoiceBatch())
			{
				result = CantReverseBatchIfSomeTransactionIsAlreadyMatchedErrorMessage;
			}
			return result;
		}

		bool CanCancelInvoiceBatch()
		{
			bool canCancel = true;
			InvoiceBatchHeader batchHeader = OriginalTransaction as InvoiceBatchHeader;
			if (batchHeader != null)
			{
				foreach (InvoicingBase invoice in batchHeader.Line)
				{
					if (invoice.IsPartiallyOrFullyPaid)
					{
						canCancel = false;
						break;
					}
				}
			}
			return canCancel;
		}

		string CantReverseBatchIfSomeTransactionIsAlreadyMatchedErrorMessage
		{
			get { return Res.GetString("e22b7471-ff05-4832-a7c6-2a42fe95a700", "You cannot cancel this Invoice Batch since some of the invoices included in this batch are already matched."); }
		}
	}
}
