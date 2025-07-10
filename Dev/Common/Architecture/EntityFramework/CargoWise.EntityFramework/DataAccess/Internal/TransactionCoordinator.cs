using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public class SaveInTransactionResult
	{
		public static SaveInTransactionResult Empty => new SaveInTransactionResult();

		SaveInTransactionResult() : this(ChangedTableNames.Empty)
		{
		}

		public SaveInTransactionResult(IChangedTableNames savedTables)
		{
			SavedTables = savedTables;
		}

		public IChangedTableNames SavedTables { get; }
	}

	public class TransactionCoordinator
	{
		public TransactionCoordinator(ITransactionParticipant[] participants)
		{
			if (participants == null)
			{
				throw new ArgumentNullException(nameof(participants));
			}
			else if (participants.Any(p => p == null))
			{
				throw new ArgumentException("Empty participant found", nameof(participants));
			}

			this.participants = participants.DistinctDepthFirstHeirarchyTraversal(participant => participant.ChildParticipants ?? Enumerable.Empty<ITransactionParticipant>()).ToArray();
			CheckNotInTransactionIfMoreThan1Participant();
		}

		internal void ReleaseSqlLocks()
		{
			foreach (var participant in participants)
			{
				if (participant is BusinessObjectFactory factory)
				{
					factory.ReleaseSqlLocks();
				}
			}
		}

		public ITransactionParticipant[] SavedParticipants
		{
			get { return participants; }
		}

		public TransactionManager BeginTransactionWithManager()
		{
			CheckHasNotBegunTransactions();

			participants.ForEach(x => x.OnAllTransactionsBeginning());
			return new TransactionManager(this, participants);
		}

		public SaveInTransactionResult SaveInTransactions()
		{
			CheckHasBegunTransactions("SaveInTransactions");

			var savedTables = new ChangedTableNames(participants.Select(x => x.SaveInTransaction()).ToArray());
			return new SaveInTransactionResult(savedTables);
		}

		#region Get All Participants

		ITransactionParticipant[] GetAllParticipants(ITransactionParticipant participant)
		{
			ITransactionParticipant[] result;

			if (participant == null)
			{
				result = new ITransactionParticipant[] { participant };
			}
			else
			{
				ArrayList list = new ArrayList
				{
					participant
				};

				ITransactionParticipant[] childParticipants = participant.ChildParticipants;

				if (childParticipants != null)
				{
					foreach (ITransactionParticipant childParticipant in childParticipants)
					{
						list.AddRange(GetAllParticipants(childParticipant));
					}
				}

				result = (ITransactionParticipant[])list.ToArray(typeof(ITransactionParticipant));
			}

			return result;
		}

		#endregion

		#region Implementation

		void CheckNotInTransactionIfMoreThan1Participant()
		{
			var relevantParticipants = participants.Where(participant => !participant.AllowTransactionWithOtherParticipant).ToArray();
			if (!AllowMultipleParticipantsInTransaction && relevantParticipants.Length > 1)
			{
				var participantsInTransaction = relevantParticipants.Where(participant => participant.IsInTransaction).ToList();
				if (participantsInTransaction.Any())
				{
					IEnumerable<string> types = participantsInTransaction.Select(transactionParticipant => transactionParticipant.GetType().ToString()).ToList();
					throw new InvalidOperationException(string.Format("TransactionParticipants must not be in a pre-exising transaction. " +
																	"There were a total of {0} transaction participants. " +
																	"{1} of them already participating in another transaction(s), " +
																	"of types: {2}", relevantParticipants.Length, participantsInTransaction.Count, string.Join(", ", types)));
				}
			}
		}

		protected virtual bool AllowMultipleParticipantsInTransaction
		{
			get { return false; }
		}

		void CheckHasBegunTransactions(string methodName)
		{
			if (!begunTransaction)
			{
				throw new InvalidOperationException("Attempted to call " + methodName + " without a corresponding BeginTransactions.");
			}
		}

		void CheckHasNotBegunTransactions()
		{
			if (begunTransaction)
			{
				throw new InvalidOperationException("Attempted to call BeginTransactions without calling CommitTransactions or RollbackTranasactions for previous BeginTransactions.");
			}
		}

		internal readonly ITransactionParticipant[] participants;

		bool begunTransaction;

		#endregion

		public class TransactionManager : MultiTransactionManager<TransactionCoordinator>
		{
			public TransactionManager(TransactionCoordinator owner, ITransactionParticipant[] transactions, Action rollbackAction = null) : base(owner, transactions, rollbackAction)
			{
				owner.begunTransaction = true;
			}

			public void CommitTransaction(SaveInTransactionResult saveInTransactionResult)
			{
				try
				{
					CommitTransaction();

					foreach (ITransactionParticipant participant in owner.participants)
					{
						participant.OnAllTransactionsCommitted(saveInTransactionResult.SavedTables);
					}
				}
				finally
				{
					owner.begunTransaction = false;
				}
			}

			protected override void Rollback()
			{
				try
				{
					InvokeAllAndGatherExceptions(() => base.Rollback(), () => owner.participants.ForEach(x => x.OnAllTransactionsRolledBack()));
				}
				finally
				{
					owner.begunTransaction = false;
				}
			}

			protected override void Dispose(bool isDisposing)
			{
				owner.ReleaseSqlLocks();
				base.Dispose(isDisposing);
			}
		}
	}
}
