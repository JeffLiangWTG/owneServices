using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconDirectReceipt : DirectReceipt.DirectReceipt
	{
		public BankReconDirectReceipt(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void MakeDepositBatch()
		{
			if (fRelatedDepositBatch == null)
			{
				fRelatedDepositBatch = Factory.New<DepositBatch.DepositBatch>();
			}
			SetDepositBatchDetails();
		}

		public void SetDepositBatch(DepositBatch.DepositBatch depositBatch)
		{
			fRelatedDepositBatch = depositBatch;
			SetDepositBatchDetails();
		}

		#region Overrides

		public override ZDecimal AH_OSTotal
		{
			get { return base.AH_OSTotal; }
			set
			{
				base.AH_OSTotal = value;
				if (fRelatedDepositBatch != null)
				{
					fRelatedDepositBatch.AH_OSTotal = AH_OSTotal;
				}
			}
		}

		public override ZDateTime AH_PostDate
		{
			get { return base.AH_PostDate; }
			set
			{
				base.AH_PostDate = value;
				if (fRelatedDepositBatch != null)
				{
					fRelatedDepositBatch.AH_PostDate = AH_PostDate;
				}
			}
		}

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				base.AH_InvoiceDate = value;
				if (fRelatedDepositBatch != null)
				{
					fRelatedDepositBatch.AH_InvoiceDate = AH_InvoiceDate;
				}
			}
		}

		protected override void HandleRelatedDepositBatch()
		{
			MakeDepositBatch();
			if (IsReversing)
			{
				fRelatedDepositBatch.ReversingReceipt = this;
			}
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new BankReconDirectReceiptValidation(this);
		}

		#endregion

		#region Implementation

		void SetDepositBatchDetails()
		{
			if (fRelatedDepositBatch != null)
			{
				fRelatedDepositBatch.AH_OSTotal = AH_OSTotal;
				fRelatedDepositBatch.AH_InvoiceAmount = AH_InvoiceAmount;
				fRelatedDepositBatch.AH_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
				fRelatedDepositBatch.AH_PostDate = AH_PostDate;
				fRelatedDepositBatch.AH_InvoiceDate = AH_InvoiceDate;
				fRelatedDepositBatch.AH_DueDate = AH_DueDate;
				fRelatedDepositBatch.AH_AB = AH_AB;
				fRelatedDepositBatch.AH_OH = AH_OH;
			}
		}

		#endregion
	}
}