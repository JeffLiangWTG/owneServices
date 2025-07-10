using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;

namespace CargoWise.Data
{
	public class MultiTransactionManager<T> : BaseTransactionManager<T>
	{
		readonly ITransactionManager[] innerTransactions;

		public MultiTransactionManager(T owner, IEnumerable<ITransactionStarter> transactionStarters, Action rollbackAction = null) : base(owner, rollbackAction)
		{
			Argument.NotNull(owner, nameof(owner));
			Argument.NotNull(transactionStarters, nameof(transactionStarters));
			if (!transactionStarters.All(t => t != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(transactionStarters));
			}

			var startedTransactions = new List<ITransactionManager>();
			try
			{
				foreach (var starter in transactionStarters)
				{
					startedTransactions.Add(starter.BeginTransactionWithManager());
				}
			}
			catch (Exception beginEx)
			{
				InvokeAllAndGatherExceptions(beginEx, () => DisposeTransactions(startedTransactions));
			}

			innerTransactions = startedTransactions.ToArray();
		}

		protected override void Commit()
		{
			try
			{
				foreach (var transactionManager in innerTransactions)
				{
					transactionManager?.CommitTransaction();
				}
			}
			catch (Exception commitEx)
			{
				InvokeAllAndGatherExceptions(commitEx, innerTransactions.Select(t => new Action(() => t.RollbackTransaction())).ToArray());
			}
		}

		protected override void Rollback()
		{
			InvokeAllAndGatherExceptions(innerTransactions.Select(t => new Action(() => t.RollbackTransaction())).ToArray());
		}

		[SuppressMessage("Microsoft.Usage", "CA2215:DisposeMethodsShouldCallBaseClassDispose", Justification = "Base dispose is always called, just indirectly.")]
		protected override void Dispose(bool isDisposing)
		{
			InvokeAllAndGatherExceptions(() => base.Dispose(isDisposing), () => DisposeTransactions(innerTransactions));
		}

		void DisposeTransactions(IEnumerable<ITransactionManager> transactions)
		{
			Argument.NotNull(transactions, nameof(transactions));

			InvokeAllAndGatherExceptions(transactions.Select(t => new Action(() => t.Dispose())).ToArray());
		}
	}
}
