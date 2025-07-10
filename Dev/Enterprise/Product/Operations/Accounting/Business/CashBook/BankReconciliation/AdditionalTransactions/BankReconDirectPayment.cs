using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconDirectPayment : DirectPayment.DirectPayment
	{
		public BankReconDirectPayment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public DirectDebitBatch.DirectDebitBatchHeader RelatedDirectDebitBatch
		{
			get { return relatedDirectDebitBatch; }
		}
		DirectDebitBatch.DirectDebitBatchHeader relatedDirectDebitBatch;

		public void MakeDirectDebitBatch()
		{
			if (relatedDirectDebitBatch == null)
			{
				relatedDirectDebitBatch = Factory.New<DirectDebitBatch.DirectDebitBatchHeader>();
			}

			if (relatedDirectDebitBatch != null)
			{
				relatedDirectDebitBatch.RelatedTransactionPK = PK;
				relatedDirectDebitBatch.AH_OSTotal = AH_OSTotal;
				relatedDirectDebitBatch.AH_InvoiceAmount = AH_InvoiceAmount;
				relatedDirectDebitBatch.AH_RX_NKTransactionCurrency = AH_RX_NKTransactionCurrency;
				relatedDirectDebitBatch.AH_PostDate = AH_PostDate;
				relatedDirectDebitBatch.AH_InvoiceDate = AH_InvoiceDate;
				relatedDirectDebitBatch.AH_DueDate = AH_DueDate;
				relatedDirectDebitBatch.AH_AB = AH_AB;
			}
		}

		public void DeleteDirectDebitBatch()
		{
			if (relatedDirectDebitBatch != null)
			{
				relatedDirectDebitBatch.Delete();
				relatedDirectDebitBatch = null;
			}
		}

		#region Overrides

		public override ZDecimal AH_OSTotal
		{
			get { return base.AH_OSTotal; }
			set
			{
				base.AH_OSTotal = value;
				if (relatedDirectDebitBatch != null)
				{
					relatedDirectDebitBatch.AH_OSTotal = -AH_OSTotal;
				}
			}
		}

		public override ZDateTime AH_PostDate
		{
			get { return base.AH_PostDate; }
			set
			{
				base.AH_PostDate = value;
				if (relatedDirectDebitBatch != null)
				{
					relatedDirectDebitBatch.AH_PostDate = AH_PostDate;
				}
			}
		}

		public override ZDateTime AH_InvoiceDate
		{
			get { return base.AH_InvoiceDate; }
			set
			{
				base.AH_InvoiceDate = value;
				if (relatedDirectDebitBatch != null)
				{
					relatedDirectDebitBatch.AH_InvoiceDate = AH_InvoiceDate;
				}
			}
		}

		public override ZDateTime AH_DueDate
		{
			get { return base.AH_DueDate; }
			set
			{
				base.AH_DueDate = value;
				if (relatedDirectDebitBatch != null)
				{
					relatedDirectDebitBatch.AH_DueDate = AH_DueDate;
				}
			}
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new BankReconDirectPaymentValidation(this);
		}

		protected override ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				return ZBool.False;
			}
		}

		public override CodeDescriptionPairList PaymentMethods
		{
			get
			{
				CodeDescriptionPairList basePaymentMethods = base.PaymentMethods;
				basePaymentMethods.RemoveCode(ZArchitecture.Core.ReceiptTypes.Cheque);
				return basePaymentMethods;
			}
		}

		public void UpdateRelatedDirectDebitBatchAmount()
		{
			if (RelatedDirectDebitBatch != null
				&& RelatedDirectDebitBatch.Lines.Any()
				&& RelatedDirectDebitBatch.Lines[0] is DirectPayment.DirectPayment directPayment)
			{
				if (directPayment.DDRCollection?.DDRHeader != null && (directPayment.AH_OSTotalAmount != directPayment.DDRCollection.DDRHeader.AH_OSExTaxAmount || directPayment.AH_LocalTotalAmount != directPayment.DDRCollection.DDRHeader.AH_LocalExTaxAmount))
				{
					directPayment.DDRCollection.ClearTotalAmount();
					directPayment.IncludeInTheBatch = true;
				}
			}
		}

		#endregion
	}
}
