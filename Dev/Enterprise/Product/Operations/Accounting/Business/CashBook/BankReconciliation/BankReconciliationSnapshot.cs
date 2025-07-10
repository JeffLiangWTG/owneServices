using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook
{
	public class BankReconciliationSnapshot
	{
		public BankReconciliationSnapshot(BankReconciliation bankRecon)
		{
			Argument.NotNull(bankRecon, nameof(bankRecon));

			StatementBalance = bankRecon.ClosingBalanceReadOnly;
			UnclearedCashbookAmount = bankRecon.UnclearedCashbookAmount;
			UnclearedStatementAmount = bankRecon.UnclearedStatementAmount;
			AmendedBankStatementBalance = bankRecon.AmendedBankStatementBalance;
			CashBookBalance = bankRecon.CashBookBalance;
			ReconError = bankRecon.ReconError;
		}

		public ZDecimal StatementBalance { get; }

		public ZDecimal UnclearedCashbookAmount { get; }

		public ZDecimal UnclearedStatementAmount { get; }

		public ZDecimal AmendedBankStatementBalance { get; }

		public ZDecimal CashBookBalance { get; }

		public ZDecimal ReconError { get; }
	}
}
