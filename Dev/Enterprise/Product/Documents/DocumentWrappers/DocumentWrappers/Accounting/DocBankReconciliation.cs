using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook;

namespace Enterprise.DocumentWrappers
{
	public class DocBankReconciliation : DocBaseWrapper
	{
		#region Constructor

		public static DocBankReconciliation New(BankReconciliation bankRecon, BusinessObjectFactory factory)
		{
			if (bankRecon == null)
			{
				return null;
			}
			return new DocBankReconciliation(bankRecon, factory);
		}

		protected DocBankReconciliation(BankReconciliation bankRecon, BusinessObjectFactory factory)
			: base(bankRecon, factory)
		{
			BankAccount = DocBankAccount.New(bankRecon.BankAccount, factory);
			TransactionsCleared = new DocBankReconciliationTransactionCollection(bankRecon.TransactionsClearedInCurrentSession(), factory);
			TransactionsUncleared = new DocBankReconciliationTransactionCollection(bankRecon.TransactionsUnclearedInCurrentSession(), factory);
			TransactionsAdded = new DocBankReconciliationTransactionCollection(bankRecon.TransactionsAddedInCurrentSession(), factory);
			TransactionsRemoved = new DocBankReconciliationTransactionCollection(bankRecon.TransactionsRemovedInCurrentSession(), factory);
		}

		#endregion

		BankReconciliation BankReconciliation
		{
			get { return (BankReconciliation)WrappedObject; }
		}

		#region Properties

		public DocBankAccount BankAccount { get; }

		public ZDateTime ReconcileDate => BankReconciliation.ReconcileDate;
		public ZDateTime StatementDate => BankReconciliation.StatementDate;

		public ZDecimal OpeningStatementBalance => (BankReconciliation.OpeningSnapshot?.StatementBalance).GetValueOrDefault(0m);
		public ZDecimal OpeningUnclearedCashbookAmount => (BankReconciliation.OpeningSnapshot?.UnclearedCashbookAmount).GetValueOrDefault(0m);
		public ZDecimal OpeningUnclearedStatementAmount => (BankReconciliation.OpeningSnapshot?.UnclearedStatementAmount).GetValueOrDefault(0m);
		public ZDecimal OpeningAmendedBankStatementBalance => (BankReconciliation.OpeningSnapshot?.AmendedBankStatementBalance).GetValueOrDefault(0m);
		public ZDecimal OpeningCashBookBalance => (BankReconciliation.OpeningSnapshot?.CashBookBalance).GetValueOrDefault(0m);
		public ZDecimal OpeningReconError => (BankReconciliation.OpeningSnapshot?.ReconError).GetValueOrDefault(0m);

		public ZDecimal ClosingStatementBalance => BankReconciliation.ClosingBalanceReadOnly;
		public ZDecimal ClosingUnclearedCashbookAmount => BankReconciliation.UnclearedCashbookAmount;
		public ZDecimal ClosingUnclearedStatementAmount => BankReconciliation.UnclearedStatementAmount;
		public ZDecimal ClosingAmendedBankStatementBalance => BankReconciliation.AmendedBankStatementBalance;
		public ZDecimal ClosingCashBookBalance => BankReconciliation.CashBookBalance;
		public ZDecimal ClosingReconError => BankReconciliation.ReconError;

		public DocBankReconciliationTransactionCollection TransactionsUncleared { get; }
		public DocBankReconciliationTransactionCollection TransactionsCleared { get; }
		public DocBankReconciliationTransactionCollection TransactionsAdded { get; }
		public DocBankReconciliationTransactionCollection TransactionsRemoved { get; }
		#endregion
	}
}
