using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	class DirectReceiptReversing : CashBookReversing
	{
		public DirectReceiptReversing(IDirectReceipt directReceipt)
			: base(directReceipt)
		{
		}

		protected override void GenerateReverseTransactions()
		{
			base.GenerateReverseTransactions();
			DirectReceipt originalReceipt = OriginalTransaction as DirectReceipt;
			if (originalReceipt != null)
			{
				DepositBatch originalDepositBatch = originalReceipt.RelatedDepositBatch;
				if (originalDepositBatch != null)
				{
					ReversingDepositBatch = OriginalTransaction.Factory.New<DepositBatch>();
					ReversingDepositBatch.ReversingReceipt = originalReceipt;
					ReversingDepositBatch.AH_OSTotal = -originalReceipt.AH_OSTotal;
					ReversingDepositBatch.AH_InvoiceAmount = -(originalReceipt.AH_LocalTotal);
					ReversingDepositBatch.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
					ReversingDepositBatch.AH_PostDate = ZDateTime.Now;
					ReversingDepositBatch.AH_InvoiceDate = ZDateTime.Now;
					ReversingDepositBatch.AH_DueDate = ZDateTime.Now;
					ReversingDepositBatch.AH_FullyPaidDate = ZDateTime.Now;
					ReversingDepositBatch.AH_AB = originalDepositBatch.AH_AB;
					ReversingDepositBatch.AH_OH = originalDepositBatch.AH_OH;
					ReversingDepositBatch.AH_Desc = Res.GetString("2a8e59a1-ccd5-4a7e-aecf-92f02882302f", "Cancellation of {0}", originalDepositBatch.AH_ReceiptBatchNo);
				}
			}
		}

		protected override void SetOtherNumberFountainFields(BusinessObjectFactory factory)
		{
			base.SetOtherNumberFountainFields(factory);
			if (ReversingDepositBatch != null)
			{
				ZString nextDepositBatchNo = AccountingNumberFountainWrapperFactory.Instance.BatchReceiptNo.GetNext(ReversingDepositBatch.Factory);
				DirectReceipt revReceipt = (DirectReceipt)ReverseTransaction;
				revReceipt.AH_ReceiptBatchNo = nextDepositBatchNo;
				ReversingDepositBatch.AH_TransactionNum = nextDepositBatchNo;
				ReversingDepositBatch.AH_ReceiptBatchNo = nextDepositBatchNo;
			}
		}

		DepositBatch ReversingDepositBatch;
	}
}