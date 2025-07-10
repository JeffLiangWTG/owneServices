using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public abstract class SaveInTransactionAction : ITransactionParticipant
	{
		protected abstract IChangedTableNames SaveInTransaction();
		protected abstract ITransactionManager BeginTransactionWithManager();
		protected abstract bool IsInTransaction { get; }

		protected virtual void OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
		}

		protected virtual void OnAllTransactionsBeginning()
		{
		}

		protected virtual void OnAllTransactionsRolledBack()
		{
		}

		protected virtual bool AllowTransactionWithOtherParticipant => false;

		protected virtual ITransactionParticipant[] ChildParticipants
		{
			get { return Array.Empty<ITransactionParticipant>(); }
		}

		#region ITransactionParticipant Members

		ITransactionManager ITransactionStarter.BeginTransactionWithManager()
		{
			return BeginTransactionWithManager();
		}

		void ITransactionParticipant.OnAllTransactionsBeginning()
		{
			OnAllTransactionsBeginning();
		}

		void ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)
		{
			OnAllTransactionsCommitted(changedTableNames);
		}

		void ITransactionParticipant.OnAllTransactionsRolledBack()
		{
			OnAllTransactionsRolledBack();
		}

		IChangedTableNames ITransactionParticipant.SaveInTransaction()
		{
			return SaveInTransaction();
		}

		bool ITransactionParticipant.IsInTransaction
		{
			get { return IsInTransaction; }
		}

		bool ITransactionParticipant.AllowTransactionWithOtherParticipant => AllowTransactionWithOtherParticipant;

		ITransactionParticipant[] ITransactionParticipant.ChildParticipants
		{
			get { return ChildParticipants; }
		}

		IEnumerable<ISqlApplicationLock> ITransactionParticipant.TransactionLocks => Enumerable.Empty<ISqlApplicationLock>();

		#endregion
	}

	public abstract class SaveInTransactionActionWithConnection : SaveInTransactionAction
	{
		internal SaveInTransactionActionWithConnection(IDbConnected connected)
		{
			this.connected = connected ?? throw new ArgumentNullException(nameof(connected));
		}

		protected sealed override ITransactionManager BeginTransactionWithManager()
		{
			return connected.Connection.BeginTransactionWithManager();
		}

		protected sealed override bool IsInTransaction
		{
			get { return connected.Connection.IsInTransactionOtherThanTransactionedTestCase; }
		}

		protected readonly IDbConnected connected;
	}

	public abstract class SaveInTransactionActionWithMainConnection : SaveInTransactionActionWithConnection
	{
		protected SaveInTransactionActionWithMainConnection() : base(Db.Connection) { }
	}

	public abstract class SaveInTransactionActionWithFactory : SaveInTransactionActionWithConnection
	{
		protected SaveInTransactionActionWithFactory(BusinessObjectFactory factory)
			: base(factory)
		{
			this.Factory = factory ?? throw new ArgumentNullException(nameof(factory));
		}

		protected readonly BusinessObjectFactory Factory;
	}

	public sealed class SaveInTransactionDelegateAction : SaveInTransactionActionWithConnection
	{
		public SaveInTransactionDelegateAction(IDbConnected connection, Func<IChangedTableNames> method)
			: base(connection)
		{
			this.Delegate = method;
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			return Delegate();
		}

		readonly Func<IChangedTableNames> Delegate;
	}
}
