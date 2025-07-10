using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.GraphEngine;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Scheduler.GraphEngine
{
	public abstract class GrEngineEnqueuer<TEntity, TQueueState>
		where TEntity : BusinessObject
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		protected GrEngineEnqueuer(GrEngineServiceSetup<TQueueState> setup, GrEngineDequeuer<TQueueState> dequeuer)
		{
			this.setup = Argument.NotNull(setup, nameof(setup));
			this.dequeuer = Argument.NotNull(dequeuer, nameof(dequeuer));
			grengine = SetupGrEngine();
		}

		ServiceTaskGrEngine<string, TQueueState> SetupGrEngine() => new ServiceTaskGrEngine<string, TQueueState>();

		protected readonly GrEngineServiceSetup<TQueueState> setup;
		readonly GrEngineDequeuer<TQueueState> dequeuer;
		readonly ServiceTaskGrEngine<string, TQueueState> grengine;
		bool hasInitialised;

		public int ItemsLoaded => grengine.Count;

		public LoadEntitiesResult Enqueue(BusinessObjectFactory factory, Lazy<ZQuery> filter, INotifications notifications = null, Func<string> getReasonForNotSaving = null) => EnqueueCore(factory, filter, notifications, getReasonForNotSaving);

		LoadEntitiesResult EnqueueCore(BusinessObjectFactory factory, Lazy<ZQuery> filter, INotifications notifications, Func<string> getReasonForNotSaving)
		{
			using (grengine.WithNotifications(notifications))
			{
				GrEngine<string, TQueueState>.LoadEntitiesResult loadProcessed = null;
				if (!hasInitialised)
				{
					// When we spin up an enqueuer, load everything that the previous enqueuer serialized.
					notifications.AddInfo((NoResString)"Initializing Grengine");
					var unprocessed = dequeuer.LoadAllUnprocessed();
					LogLoad(notifications, unprocessed.ToList());
					grengine.AddNew(unprocessed);
					loadProcessed = grengine.LoadEntities();
					LogFirstLoad(loadProcessed, notifications);

					hasInitialised = true;
				}
				// Get stmQueueStates that have been Notified as processed in order to free up waiting items.
				var notified = dequeuer.LoadNotified();
				notifications.AddInfo(FormattableString.Invariant($"{QueueStatusCodes.Codes.Processed} ({notified.Count()})"));
				grengine.AddNew(notified);
				loadProcessed = grengine.LoadEntities();
				LogFirstLoad(loadProcessed, notifications);

				// Get new business objects to be queued.
				notifications.AddInfo((NoResString)"Enqueuing");
				var unqueuedStms = GetUnqueuedStms(factory, filter);
				LogLoad(notifications, unqueuedStms);
				grengine.AddNew(unqueuedStms);

				var result = grengine.LoadEntities();
				LogFirstLoad(result, notifications);

				var unblocked = grengine.FetchUnblocked();
				UpdateStatus(result.NewEntities, unblocked, notifications);

				var chains = grengine.FindChains().Where(c => c.Count > 1).ToArray();
				notifications.AddInfo(FormattableString.Invariant($"Chains loaded ({chains.Length})"));

				var updatedChains = setup.ChainMode == GrEngineEnums.ChainOption.Yes
					? SetChainIds(chains, notifications) : Enumerable.Empty<TQueueState>();
				notifications.AddInfo(FormattableString.Invariant($"Updated chains ({updatedChains.Count()})"));

				var reason = getReasonForNotSaving?.Invoke();
				if (!string.IsNullOrEmpty(reason))
				{
					throw new OperationCanceledException(reason);
				}
				var queueStates = result.NewEntities.Union(unblocked.Where(u => u.HasChanges)).Union(updatedChains).ToList();
				notifications.AddInfo(FormattableString.Invariant($"Union done ({queueStates.Count})"));
				setup.Save(queueStates, notifications);
				notifications.AddInfo(FormattableString.Invariant($"Queues saved ({queueStates.Count})"));

				LogAfterSave(notifications, chains);
				return new LoadEntitiesResult(ItemsLoaded, result, loadProcessed);
			}
		}

		#region Logging

		#region SuppressResourceStringsCheckRegion

		void LogLoad(INotifications notifications, IList<TQueueState> unprocessed)
		{
			if (notifications != null && setup.LogOptions.EnqueueLoads && unprocessed.Count > 0)
			{
				notifications.AddInfo(FormattableString.Invariant($"Messages loaded ({unprocessed.Count}): {string.Join(", ", unprocessed.Select(s => s.GetParentDetails()))}"));
			}
		}

		void LogFirstLoad(GrEngine<string, TQueueState>.LoadEntitiesResult result, INotifications notifications)
		{
			if (notifications != null && result.NewEntities.Count > 0 && setup.LogOptions.FirstEnqueueLoad)
			{
				notifications.AddInfo(FormattableString.Invariant($"Loaded messages for the first time: {string.Join(", ", result.NewEntities.Select(s => s.GetParentDetails()))}"));
			}
		}

		void LogAfterSave(INotifications notifications, Chain<TQueueState>[] chains)
		{
			if (notifications != null)
			{
				if (setup.LogOptions.MessageAtFront)
				{
					var unblocked = grengine.FetchUnblocked();
					if (unblocked.Any())
					{
						notifications.AddInfo(FormattableString.Invariant($"Messages at front of queue: {string.Join(", ", unblocked.Select(b => b.GetFrontQueueDetails()))}"));
					}
				}

				if (setup.LogOptions.OldestMessage)
				{
					var unblocked = grengine.FetchUnblocked();
					if (unblocked.Any())
					{
						notifications.AddInfo(FormattableString.Invariant($"Oldest message: {unblocked.Min(b => b.GetParentDetails())}"));
					}
				}

				if (setup.LogOptions.ChainStatistics)
				{
					notifications.AddInfo(FormattableString.Invariant($"Number of chains: {chains.Length}"));
					notifications.AddInfo(FormattableString.Invariant($"Chain Lengths: {string.Join(", ", chains.Select(s => s.Count))}"));
				}
			}
		}

		#endregion

		#endregion

		IList<TQueueState> GetUnqueuedStms(BusinessObjectFactory factory, Lazy<ZQuery> filter)
		{
			var maxRows = GetMaxRows();
			if (maxRows <= 0)
			{
				return Array.Empty<TQueueState>();
			}
			else
			{
				var filterResult = filter.Value;
				switch (setup.PreKeyMode)
				{
					case GrEngineEnums.PreKeyOption.None:
						return GetUnqueuedBizos(factory, filterResult, maxRows).Select(b => setup.Factory.NewQueueState(b, GetKeys(b), GetMessageNumber(b))).ToList();

					case GrEngineEnums.PreKeyOption.Mandatory:
						return dequeuer.LoadPreKeys(maxRows, filterResult).ToList();

					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unexpected key mode [{0}]", setup.PreKeyMode));
				}
			}
		}

		void UpdateStatus(IEnumerable<TQueueState> newEntities, IEnumerable<TQueueState> unblocked, INotifications notifications)
		{
			foreach (var entity in newEntities)
			{
				entity.UpdateStatus(QueueStatusCodes.Codes.Blocked);
			}

			foreach (var entity in unblocked)
			{
				entity.UpdateStatus(QueueStatusCodes.Codes.Queued);
			}

			if (notifications != null)
			{
				notifications.AddInfo(FormattableString.Invariant($"{QueueStatusCodes.Codes.Queued} ({unblocked.Count()}). {QueueStatusCodes.Codes.Blocked} ({newEntities.Count(x => x.Status == QueueStatusCodes.Codes.Blocked)})"));
			}
		}

		IList<TQueueState> SetChainIds(Chain<TQueueState>[] chains, INotifications notifications)
		{
			var changed = new List<TQueueState>();

			foreach (var chain in chains)
			{
				var id = GetID(chain);
				foreach (var item in chain.Where(c => c.ChainID != id).TakeWhile(c => Guid.Empty.Equals(c.ChainID)))
				{
					// We don't want to update anything that was already queued, because it may have already been loaded.
					if (item.Status != QueueStatusCodes.Codes.Queued || item.StatusChanged)
					{
						if (notifications != null && setup.LogOptions.SetChainID)
						{
							notifications.AddInfo(FormattableString.Invariant($"Set Chain ID for {item.GetParentDetails()} ({item.Identifier}): {id}"));
						}
						item.SetChainId(id);
						changed.Add(item);
					}
				}
			}

			return changed;
		}

		static Guid GetID(Chain<TQueueState> chain)
		{
			var id = chain.First().ChainID;
			return Guid.Empty == id ? Guid.NewGuid() : id;
		}

		TEntity[] GetUnqueuedBizos(BusinessObjectFactory factory, ZQuery unqueuedBizosFilter, int? maxRows)
		{
			var query = new ZDBOnlyQuery(typeof(TEntity));
			query.AddToFilter(unqueuedBizosFilter);
			query.ReLoadExistingRows = true;

			if (maxRows.HasValue)
			{
				query.MaximumRows = maxRows.Value;
			}

			query.OrderBy = unqueuedBizosFilter.OrderBy;
			setup.Factory.AddFilterOutAlreadyQueuedItems(query);
			return factory.Load<TEntity>(query);
		}

		int? GetMaxRows()
		{
			if (setup.Capacity > 0 && setup.EnqueueBatchSize > 0)
			{
				return Math.Min(setup.Capacity - ItemsLoaded, setup.EnqueueBatchSize);
			}
			else if (setup.Capacity > 0)
			{
				return setup.Capacity - ItemsLoaded;
			}
			else if (setup.EnqueueBatchSize > 0)
			{
				return setup.EnqueueBatchSize;
			}
			else
			{
				return null;
			}
		}

		protected abstract ZString GetMessageNumber(TEntity b);

		protected abstract string[] GetKeys(TEntity b);

		protected abstract ITableSchema GetSchema();

		public class LoadEntitiesResult
		{
			internal LoadEntitiesResult(int totalEntities, params GrEngine<string, TQueueState>.LoadEntitiesResult[] items)
			{
				TotalEntities = totalEntities;
				if (items.WhereNotNull().Count() == 1)
				{
					var item = items.WhereNotNull().Single();
					NewEntities = item.NewEntities;
					OldEntities = item.OldEntities;
				}
				else
				{
					NewEntities = items.WhereNotNull().SelectMany(s => s.NewEntities).ToList().AsReadOnly();
					OldEntities = items.WhereNotNull().SelectMany(s => s.OldEntities).ToList().AsReadOnly();
				}
			}

			public IList<TQueueState> NewEntities { get; }
			public IList<TQueueState> OldEntities { get; }
			public int TotalEntities { get; }
		}
	}

	static class Extensions
	{
		internal static void AddInfo(this INotifications notifications, string log)
		{
			if (notifications != null)
			{
				notifications.Add(CargoWise.ComponentModel.NotificationType.Information, log);
			}
		}
	}
}
