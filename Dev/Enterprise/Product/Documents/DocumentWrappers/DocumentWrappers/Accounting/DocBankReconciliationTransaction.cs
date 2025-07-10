using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.CashBook;

namespace Enterprise.DocumentWrappers
{
	public class DocBankReconciliationTransaction : DocBaseWrapper
	{
		#region Constructor

		protected DocBankReconciliationTransaction(BankReconTransactionSnapshot transaction, BusinessObjectFactory factory)
			: base(transaction, factory)
		{
		}

		public static DocBankReconciliationTransaction New(BankReconTransaction transaction, BusinessObjectFactory factory)
		{
			if (transaction == null)
			{
				return null;
			}
			return new DocBankReconciliationTransaction(new BankReconTransactionSnapshot(transaction), factory);
		}

		public static DocBankReconciliationTransaction New(Statement transaction, BusinessObjectFactory factory)
		{
			if (transaction == null)
			{
				return null;
			}
			return new DocBankReconciliationTransaction(new BankReconTransactionSnapshot(transaction), factory);
		}

		public static DocBankReconciliationTransaction New(BankReconTransactionSnapshot transaction, BusinessObjectFactory factory)
		{
			if (transaction == null)
			{
				return null;
			}
			return new DocBankReconciliationTransaction(transaction, factory);
		}

		public static DocBankReconciliationTransaction New(IBankReconMergedTransaction transaction, BusinessObjectFactory factory)
		{
			if (transaction == null)
			{
				return null;
			}
			return new DocBankReconciliationTransaction(new BankReconTransactionSnapshot(transaction), factory);
		}

		#endregion

		#region Properties

		BankReconTransactionSnapshot Transaction => (BankReconTransactionSnapshot)WrappedObject;

		public ZGuid Identifier => Transaction.PK;
		public ZDateTime TransactionDate => Transaction.TransactionDate;
		public ZDateTime InvoiceDate => Transaction.InvoiceDate;
		public ZString Type => Transaction.Type;
		public ZString Method => Transaction.Method;
		public ZString ChequeRef => Transaction.ChequeRef;
		public ZString BatchNo => Transaction.BatchNo;
		public ZString Payee => Transaction.Payee;
		public ZDecimal Debit => Transaction.Debit;
		public ZDecimal Credit => Transaction.Credit;
		public ZBool IsCleared => Transaction.IsCleared;
		public ZString LineType => Transaction.LineType;
		public ZDateTime ClearedDate => Transaction.ClearedDate;

		#endregion
	}
}
