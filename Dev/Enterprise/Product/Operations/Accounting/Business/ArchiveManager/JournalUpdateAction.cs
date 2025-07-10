using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Accounting.Business.ArchiveManager
{
	public class JournalUpdateAction : ITransactionParticipant
	{
		public IList<Guid> TransactionHeaderPKList { get; set; }

		#region IArchiveAction Members

		public void TakeAction()
		{
			if (TransactionHeaderPKList == null)
			{
				throw new InvalidOperationException("TransactionHeaderPKList must be set first.");
			}

			// Create a set of archive journals, and keep updating these journals for every transaction archived...ever!
			// DON'T create a new set of journals for every transaction, as that would defeat the purpose of archiving them in the first place.
		}

		#endregion

		#region ITransactionParticipant Members

		public ITransactionManager BeginTransactionWithManager()
		{
			return new StubTransactionManager();
		}

		public ITransactionParticipant[] ChildParticipants
		{
			get { return null; }
		}

		public bool AllowTransactionWithOtherParticipant => false;

		public bool IsInTransaction
		{
			get { return false; }
		}

		public IEnumerable<ISqlApplicationLock> TransactionLocks => Enumerable.Empty<ISqlApplicationLock>();

		public void OnAllTransactionsBeginning()
		{
		}

		public void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
		}

		public void OnAllTransactionsRolledBack()
		{
		}

		public IChangedTableNames SaveInTransaction()
		{
			return ChangedTableNames.Empty;
		}

		#endregion
	}
}
