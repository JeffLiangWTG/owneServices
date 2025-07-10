using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.Scheduler.GraphEngine
{
	/// <summary>
	/// This class is a supplementary class for GrEngine implementations with expensive key calculation.
	/// When using the pre-enqueuer, keys can be calculated in parallel, and synchronisation occurs asyncronously from this state.
	/// </summary>
	public abstract class GrEnginePreEnqueuer<TEntity, TQueueState>
		where TEntity : BusinessObject
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		protected GrEnginePreEnqueuer(GrEngineServiceSetup<TQueueState> setup)
		{
			Argument.NotNull(setup, nameof(setup));
			this.setup = setup;
		}

		readonly GrEngineServiceSetup<TQueueState> setup;

		IPreKeyBacklogChecker backlogChecker;
		protected IPreKeyBacklogChecker BacklogChecker => backlogChecker ?? (backlogChecker = ObjectFactory.Get<IPreKeyBacklogChecker>(nameof(IPreKeyBacklogChecker), setup));

		#region Abstract members

		protected abstract ZString GetMessageNumber(TEntity b);

		protected abstract PerformanceMonitor Monitor { get; }

		#endregion

		#region API

		public EnqueueResult Enqueue(TEntity e, string[] keys)
		{
			var state = setup.Factory.NewQueueState(e, keys, GetMessageNumber(e));
			if (keys.Length == 0)
			{
				// This is a cheap optimisation so that entities without states do not bother.
				state.UpdateStatus(QueueStatusCodes.Codes.Queued);
			}
			else
			{
				state.UpdateStatus(QueueStatusCodes.Codes.PreKey);
			}
			setup.Save(new[] { state });
			return new EnqueueResult(1);
		}

		/// <summary>
		/// Load a set of rows that are not already attached to StmQueueState for the purpose of generating GrEngine Keys.
		/// </summary>
		public LoadWithAppLockResult<T> LoadPreKeyBatch<T>(BusinessObjectFactory factory, string appLockKey,
			ZQuery query, ZQuery checkFilter, int batchSize, AppLockLogger<T> logger, Func<ZQuery> getFullBacklogMessagesToProcessFilter = null)
			where T : BusinessObject
		{
			using (Monitor.MonitorQuery())
			{
				var queuedItemQuery = new ZDBOnlyQuery(typeof(T));
				setup.Factory.AddFilterOutAlreadyQueuedItems(queuedItemQuery);
				queuedItemQuery.AddToFilter(query);
				if (BacklogChecker.IsBacklogTooBig)
				{
					if (getFullBacklogMessagesToProcessFilter != null) //If the backlog is too big, we only process the messages that are left .
					{
						queuedItemQuery.AddToFilter(getFullBacklogMessagesToProcessFilter());
					}
					else
					{
						return new LoadWithAppLockResult<T>(Array.Empty<AppLockedItem<T>>());
					}
				}
				return factory.LoadWithApplocks<T>(appLockKey, setup.LockProvider, new[] { queuedItemQuery.PKColumn }, queuedItemQuery, checkFilter, batchSize, logger);
			}
		}

		#region Result

		public class EnqueueResult
		{
			internal EnqueueResult(int numberOfRows)
			{
				EffectedRows = numberOfRows;
			}

			public int EffectedRows { get; }
		}

		#endregion

		#endregion
	}
}
