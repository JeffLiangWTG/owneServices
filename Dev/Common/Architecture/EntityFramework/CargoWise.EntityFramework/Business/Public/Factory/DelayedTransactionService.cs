using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	public static class DelayedTransactionService_Extensions
	{
		public static IDelayedTransactionManager DelayedTransaction(this BusinessObjectFactory factory)
		{
			var service = factory.ServiceContainer.GetService<DelayedTransactionService>();
			if (service == null)
			{
				var transactionLockManager = new TransactionLockManagerImpl();
				service = new DelayedTransactionService(
					((IDbConnected)factory).Connection.DelayedTransactionWithManager(transactionLockManager),
					transactionLockManager);

				factory.ServiceContainer.AddService(service);
				return new ConcreteDelayedTransactionCommitter(service.DelayedTransactionManager);
			}

			return new DummyDelayedTransactionCommitter();
		}
		public static IDelayedTransactionManager GetDummyDelayedTransaction(this BusinessObjectFactory factory) => new DummyDelayedTransactionCommitter();
	}

	abstract class DelayedTransactionCommitter : IDelayedTransactionManager
	{
		bool committed;
		bool rolledBack;

		protected abstract void CommitCore();
		protected abstract void RollbackCore();
		protected abstract void DisposeCore();
		protected abstract bool IsRollingbackCore();
		protected abstract void ReportInvalidRollbackExceptionHandlingCore();

		public bool IsRollingback => IsRollingbackCore();

		public void CommitTransaction()
		{
			if (rolledBack)
			{
				throw new InvalidOperationException("You can't commit a transaction you already rolled back.");
			}
			else if (committed)
			{
				throw new InvalidOperationException("You can't commit a delayed transaction twice.");
			}
			else if (IsRollingback)
			{
				var rollback = true;
#if DEBUG
				rollback = Db.Connection.AppTransactionCount <= 1;
#endif
				if (rollback)
				{
					RollbackTransaction();
				}
			}
			committed = true;
			CommitCore();
		}

		public void Dispose()
		{
			if (!committed && !rolledBack)
			{
				RollbackCore();
				// We don't throw an exception here as we don't want to clobber other exceptions
			}
			DisposeCore();
		}

		public void RollbackTransaction()
		{
			if (committed)
			{
				throw new InvalidOperationException("You can't roll back a transaction you already committed");
			}
			rolledBack = true;
			RollbackCore();
			throw new TransactionException("A delayed transaction needs to be rolled back.");
		}

		public void ReportInvalidRollbackExceptionHandling()
		{
			ReportInvalidRollbackExceptionHandlingCore();
		}
	}

	sealed class DummyDelayedTransactionCommitter : DelayedTransactionCommitter
	{
		protected override void CommitCore()
		{
			// Do nothing, because this is a nested transaction
		}

		protected override void RollbackCore()
		{
			// Do nothing, because this is a nested transaction
		}

		protected override void DisposeCore()
		{
			// Nothing to do.
		}

		protected override bool IsRollingbackCore()
		{
			return false;
		}

		protected override void ReportInvalidRollbackExceptionHandlingCore()
		{
			// Do nothing, because this is a nested transaction
		}
	}

	sealed class ConcreteDelayedTransactionCommitter : DelayedTransactionCommitter
	{
		public ConcreteDelayedTransactionCommitter(IDelayedTransactionManager manager)
		{
			this.manager = manager;
		}

		readonly IDelayedTransactionManager manager;

		protected override void CommitCore()
		{
			try
			{
				manager.CommitTransaction();
			}
			catch (TransactionException ex) when (ex.ErrorType == OdysseyDataErrorType.NoTransactionToCommit)
			{
				// Factory.Save was never called so there is notihing to commit.
			}
		}

		protected override void RollbackCore() => manager.RollbackTransaction();
		protected override void DisposeCore() => manager.Dispose();
		protected override bool IsRollingbackCore() => manager.IsRollingback;

		protected override void ReportInvalidRollbackExceptionHandlingCore() => manager.ReportInvalidRollbackExceptionHandling();
	}

	sealed class DelayedTransactionService : IService
	{
		readonly IDelayedTransactionManager delayedTransactionManager;
		readonly ITransactionLockManager transactionLockManager;

		public DelayedTransactionService(
			IDelayedTransactionManager delayedTransactionManager,
			ITransactionLockManager transactionLockManager)
		{
			this.delayedTransactionManager = delayedTransactionManager;
			this.transactionLockManager = transactionLockManager;
		}

		public IDelayedTransactionManager DelayedTransactionManager => delayedTransactionManager;
		public ITransactionLockManager TransactionLockManager => transactionLockManager;
	}

	sealed class TransactionLockManagerImpl : ITransactionLockManager
	{
		readonly List<ISqlApplicationLock> sqlApplicationLocks = new List<ISqlApplicationLock>();

		public void AddSqlLock(ISqlApplicationLock sqlLock)
		{
			sqlApplicationLocks.Add(sqlLock);
		}

		public void ReleaseSqlLocks()
		{
			foreach (var sqlApplicationLock in sqlApplicationLocks)
			{
				sqlApplicationLock.Dispose();
			}
		}
	}
}

