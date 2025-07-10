using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Scheduler.GraphEngine
{
	public interface IQueueStateFactory<TQueueState>
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		TQueueState NewQueueState<TEntity>(TEntity businessObject, string[] keys, ZString messageNumber) where TEntity : BusinessObject;

		QueueBatch<TQueueState> GetDequeueBatch(BusinessObjectFactory factory, INotifications notifications,
			ISqlApplicationLockProvider lockProvider, GrEngineLogOptions logOptions, int dequeueBatchSize,
			int dequeueListSizeEstimate, GrEngineEnums.ChainOption chainOption);

		int Notify(IEnumerable<QueueStateResult<TQueueState>> batch);
		int CountBacklog(string status, GrEngineEnums.Status includeOrExclude);
		IList<TQueueState> LoadPreKeys(int enqueueBatchSize, int? maximumRows, ZQuery filter);
		IEnumerable<TQueueState> LoadNotified(int setupEnqueueBatchSize);
		IList<TQueueState> LoadAllUnprocessed(int enqueueBatchSize);
		IEnumerable<TQueueState> LoadAll(int enqueueBatchSize);
		void Update(IEnumerable<TQueueState> queues);
		void InsertAsQueued(IEnumerable<TQueueState> queues);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void DeleteOldProcessed(INotifications notifications, int hours);
		void AddFilterOutAlreadyQueuedItems(ZDBOnlyQuery query);
		GrEngineLogOptions LogOptions { get; }
	}
}
