using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Scheduler.GraphEngine
{
	/// <summary>
	/// Normally, circumventing ZQuery would be a bad thing because it avoids parameterised SQL.
	/// </summary>
	public class GrEngineDequeuer<TQueueState>
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		readonly GrEngineServiceSetup<TQueueState> setup;

		public GrEngineDequeuer(GrEngineServiceSetup<TQueueState> setup)
		{
			Argument.NotNull(setup, nameof(setup));
			this.setup = setup;
		}

		#region Public API

		public QueueBatch<TQueueState> LoadBatch(BusinessObjectFactory factory, INotifications notifications) =>
			setup.Factory.GetDequeueBatch(factory, notifications, setup.LockProvider, setup.LogOptions, setup.DequeueBatchSize,
				setup.DequeueListSizeEstimate, setup.ChainMode);

		public int Notify(IEnumerable<QueueStateResult<TQueueState>> batch) => setup.Factory.Notify(batch);

		public IList<TQueueState> LoadPreKeys(int? maximumRows, ZQuery filter) =>
			setup.Factory.LoadPreKeys(setup.EnqueueBatchSize, maximumRows, filter);

		public IEnumerable<TQueueState> LoadNotified() => setup.Factory.LoadNotified(setup.EnqueueBatchSize);

		public IEnumerable<TQueueState> LoadAllUnprocessed() => setup.Factory.LoadAllUnprocessed(setup.EnqueueBatchSize);
		public IEnumerable<TQueueState> LoadAll() => setup.Factory.LoadAll(setup.EnqueueBatchSize);

		#endregion
	}
}
