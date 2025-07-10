#if DEBUG

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	partial class BankReconciliation
	{
		public ZQuery GetTextFilterForTransaction_ForTestOnly() => GetTextFilterForTransaction();

		public void SetUnclearedAmountsFromDB_ForTestOnly(ZGuid bank, ZDateTime postDate, ZDateTime statementDate)
			=> SetUnclearedAmountsFromDB(bank, postDate, statementDate);

		public ZDecimal GetUndepositedReceiptAmount_ForTestOnly(ZGuid aH_AB, ZDateTime aH_PostDate)
			=> GetUndepositedReceiptAmount(aH_AB, aH_PostDate);

		public ZDecimal GetLateDepositedReceiptAmount_ForTestOnly(ZGuid aH_AB, ZDateTime aH_PostDate)
			=> GetLateDepositedReceiptAmount(aH_AB, aH_PostDate);

		public ZDateTime LastStatementDate_ForTestOnly => LastStatementDate;

		public ZDateTime LastReconcileDate_ForTestOnly => LastReconcileDate;

		public BankReconTransCollection Transactions_ForTestOnly => Transactions;

		public void ResetCombinedTransactions_ForTestOnly() => ResetCombinedTransactions();

		public void ResetFactoryAndReloadRecords_ForTestOnly() => ResetFactoryAndReloadRecords();

		public ZQuery GetDateFiltersForTransaction_ForTestOnly() => GetDateFiltersForTransaction();

		public static string DATE_IN_STATEMENT_ForTestOnly => DATE_IN_STATEMENT;

		public void ResetTransactions_ForTestOnly() => ResetTransactions();

		public BusinessObjectFactory TransactionsFactory_innerValue_ForTestOnly => TransactionsFactory_innerValue;

		public BusinessObjectFactory TransactionsFactory_ForTestOnly => TransactionsFactory;

		public ZDecimal UnclearedStatementAmountFromDB_ForTestOnly => UnclearedStatementAmountFromDB;

		public ZDecimal UnclearedCashbookAmountFromDB_ForTestOnly => UnclearedCashbookAmountFromDB;

		public ZDecimal? FUnclearedCashbookAmountFromDB_ForTestOnly
		{
			get => fUnclearedCashbookAmountFromDB;
			set => fUnclearedCashbookAmountFromDB = value;
		}
		public ZDecimal? FUnclearedStatementAmountFromDB_ForTestOnly
		{
			get => fUnclearedStatementAmountFromDB;
			set => fUnclearedStatementAmountFromDB = value;
		}
	}
}

#endif
