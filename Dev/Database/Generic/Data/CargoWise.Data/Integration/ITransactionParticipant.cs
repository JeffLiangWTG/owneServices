using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data.Utils;

namespace CargoWise.Integration
{
	public interface ITransactionParticipant : ITransactionStarter
	{
		IChangedTableNames SaveInTransaction();

		void OnAllTransactionsBeginning();
		void OnAllTransactionsCommitted(IChangedTableNames changedTableNames);
		void OnAllTransactionsRolledBack();

		bool IsInTransaction { get; }

		bool AllowTransactionWithOtherParticipant { get; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		ITransactionParticipant[] ChildParticipants { get; }
		IEnumerable<ISqlApplicationLock> TransactionLocks { get; }
	}
}
