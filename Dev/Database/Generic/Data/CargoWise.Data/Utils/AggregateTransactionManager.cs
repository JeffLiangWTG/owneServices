using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Integration;

namespace CargoWise.Data
{
	public class AggregateTransactionManager<T> : BaseTransactionManager<T>
	{
		readonly ITransactionManager innerTransactionManager;

		public AggregateTransactionManager(T owner, ITransactionManager innerTransactionManager, Action rollbackAction = null) : base(owner, rollbackAction)
		{
			Argument.NotNull(innerTransactionManager, nameof(innerTransactionManager));
			Argument.NotNull(owner, nameof(owner));

			this.innerTransactionManager = innerTransactionManager;
		}

		protected override void Commit()
		{
			innerTransactionManager.CommitTransaction();
		}

		protected override void Rollback()
		{
			innerTransactionManager.RollbackTransaction();
		}

		[SuppressMessage("Microsoft.Usage", "CA2215:DisposeMethodsShouldCallBaseClassDispose", Justification = "Base dispose is always called, just indirectly.")]
		protected override void Dispose(bool isDisposing)
		{
			InvokeAllAndGatherExceptions(() => base.Dispose(isDisposing), () => { if (isDisposing) { innerTransactionManager.Dispose(); } });
		}
	}
}
