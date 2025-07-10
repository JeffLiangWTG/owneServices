using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using CargoWise.Common;
using CargoWise.Integration;

namespace CargoWise.Data
{
	public abstract class BaseTransactionManager<T> : ITransactionManager
	{
		protected readonly T owner;
		bool committed;
		bool rolledBack;
		readonly Action rollbackAction;
		bool disposed;

		protected BaseTransactionManager(T owner, Action rollbackAction = null)
		{
			Argument.NotNull(owner, nameof(owner));

			this.owner = owner;
			this.rollbackAction = rollbackAction;
		}

		public void CommitTransaction()
		{
			if (committed)
			{
				throw new InvalidOperationException("Cannot commit a transaction twice");
			}

			if (rolledBack)
			{
				throw new InvalidOperationException("Cannot commit a rolled back transaction");
			}

			try
			{
				Commit();
			}
			finally
			{
				committed = true;
			}
		}

		public void Dispose()
		{
			if (disposed)
			{
				return;
			}

			try
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			finally
			{
				disposed = true;
			}
		}

		public void RollbackTransaction()
		{
			if (committed || rolledBack)
			{
				return;
			}

			try
			{
				InvokeAllAndGatherExceptions(Rollback, rollbackAction);
			}
			finally
			{
				rolledBack = true;
			}
		}

		protected void InvokeAllAndGatherExceptions(params Action[] actions)
		{
			Argument.NotNull(actions, nameof(actions));

			InvokeAllAndGatherExceptions(null, actions);
		}

		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")] //Code contract is wrong, it does not evaluate to a constant.
		protected void InvokeAllAndGatherExceptions(Exception preException, params Action[] actions)
		{
			Argument.NotNull(actions, nameof(actions));

			var exceptionList = new List<Exception>();
			if (preException != null)
			{
				exceptionList.Add(preException);
			}

			foreach (var action in actions)
			{
				try
				{
					action?.Invoke();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					exceptionList.Add(ex);
				}
			}

			if (exceptionList.Count == 1)
			{
				var dispatchInfo = ExceptionDispatchInfo.Capture(exceptionList[0]);
				dispatchInfo.Throw();
			}
			else if (exceptionList.Count > 1)
			{
				throw new AggregateException(exceptionList);
			}
		}

		protected abstract void Commit();

		protected abstract void Rollback();

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				RollbackTransaction();
			}
		}
	}
}
