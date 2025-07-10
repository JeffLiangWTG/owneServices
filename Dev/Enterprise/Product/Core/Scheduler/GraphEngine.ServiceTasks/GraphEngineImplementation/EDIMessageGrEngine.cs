using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Data.Utils;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public abstract class EDIMessageGrEngine<T, TQueueState> : IDisposable
		where T : EDIMessage
		where TQueueState : class, IQueueState, IEquatable<TQueueState>
	{
		protected EDIMessageGrEngine(IQueueStateFactory<TQueueState> factory, GrEngineServiceSetting setting, Func<int> getBatchSize, Func<int> getFlipperBatchSize, Func<int> getCapacity, Func<int> getPreKeyBacklogSize, Func<GrEngineServiceSetup<TQueueState>, EDIMessageGraphPreEnqueuer<EDIMessage, TQueueState>> createNewPreEnqueuer, ISqlApplicationLockProvider lockProvider = null)
		{
			Setting = setting;

			if (setting == GrEngineServiceSetting.Disabled)
			{
				LazySetup = SetupExplode<GrEngineServiceSetup<TQueueState>>();
				LazyDequeuer = SetupExplode<GrEngineDequeuer<TQueueState>>();
				LazyEnqueuer = SetupExplode<EDIMessageGraphEnqueuer<T, TQueueState>>();
				LazyPreEnqueuer = SetupExplode<EDIMessageGraphPreEnqueuer<EDIMessage, TQueueState>>();
			}
			else
			{
				IsEnabled = true;
				BatchSize = getBatchSize();
				FlipperBatchSize = getFlipperBatchSize();
				Capacity = getCapacity();
				PreKeyBacklogSize = getPreKeyBacklogSize();

				LazySetup = new Lazy<GrEngineServiceSetup<TQueueState>>(() => new GrEngineServiceSetup<TQueueState>(factory, FlipperBatchSize, BatchSize, PreKeyBacklogSize, Capacity, GrEngineEnums.PreKeyOption.Mandatory, GrEngineEnums.ChainOption.Yes, lockProvider));
				LazyDequeuer = new Lazy<GrEngineDequeuer<TQueueState>>(() => new GrEngineDequeuer<TQueueState>(Setup));

				if (setting == GrEngineServiceSetting.KeyGen)
				{
					ShouldCreateKeys = true;
					LazyPreEnqueuer = new Lazy<EDIMessageGraphPreEnqueuer<EDIMessage, TQueueState>>(() => createNewPreEnqueuer(Setup));
					LazyShouldDequeueStrategy = SetupExplode<ShouldDequeueStrategy<TQueueState>>();
					LazyShouldEnqueueStrategy = SetupExplode<ShouldEnqueueStrategy>();
				}
				else if (setting == GrEngineServiceSetting.Flipper)
				{
					LazyEnqueuer = new Lazy<EDIMessageGraphEnqueuer<T, TQueueState>>(() => new EDIMessageGraphEnqueuer<T, TQueueState>(Setup, Dequeuer));
					LazyShouldDequeueStrategy = SetupExplode<ShouldDequeueStrategy<TQueueState>>();
					LazyShouldEnqueueStrategy = SetupExplode<ShouldEnqueueStrategy>();
					LazyPreEnqueuer = SetupExplode<EDIMessageGraphPreEnqueuer<EDIMessage, TQueueState>>();
				}
				else if (setting == GrEngineServiceSetting.Worker)
				{
					LazyEnqueuer = new Lazy<EDIMessageGraphEnqueuer<T, TQueueState>>(() => null);
					LazyShouldEnqueueStrategy = new Lazy<ShouldEnqueueStrategy>(() => new NeverEnqueueStrategy());
					LazyShouldDequeueStrategy = new Lazy<ShouldDequeueStrategy<TQueueState>>(() => new AlwaysDequeueStrategy<TQueueState>());
					LazyPreEnqueuer = SetupExplode<EDIMessageGraphPreEnqueuer<EDIMessage, TQueueState>>();
				}
				else
				{
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unhandled setting [{0}]", setting));
				}
			}
		}

		static Lazy<TData> SetupExplode<TData>()
		{
			return new Lazy<TData>(() => { throw new InvalidOperationException("Property is disabled."); });
		}

		Lazy<GrEngineServiceSetup<TQueueState>> LazySetup { get; }
		Lazy<EDIMessageGraphEnqueuer<T, TQueueState>> LazyEnqueuer { get; }
		Lazy<GrEngineDequeuer<TQueueState>> LazyDequeuer { get; }
		Lazy<EDIMessageGraphPreEnqueuer<EDIMessage, TQueueState>> LazyPreEnqueuer { get; }
		Lazy<ShouldEnqueueStrategy> LazyShouldEnqueueStrategy { get; }
		Lazy<ShouldDequeueStrategy<TQueueState>> LazyShouldDequeueStrategy { get; }

		public GrEngineServiceSetting Setting { get; }
		public bool IsEnabled { get; }
		public GrEngineServiceSetup<TQueueState> Setup => LazySetup.Value;
		public GrEnginePreEnqueuer<EDIMessage, TQueueState> PreEnqueuer => LazyPreEnqueuer.Value;
		public EDIMessageGraphEnqueuer<T, TQueueState> Enqueuer => LazyEnqueuer.Value;
		public GrEngineDequeuer<TQueueState> Dequeuer => LazyDequeuer.Value;

		public int BatchSize { get; }
		public int FlipperBatchSize { get; }
		public int Capacity { get; }
		public int PreKeyBacklogSize { get; }
		public bool ShouldCreateKeys { get; }

		public bool ShouldEnqueue
		{
			get { return LazyShouldEnqueueStrategy.Value.ShouldEnqueue(); }
		}

		public bool ShouldDequeue(IEnumerable<TQueueState> enqueued)
		{
			return LazyShouldDequeueStrategy.Value.ShouldDequeue(enqueued);
		}

		public void NudgeWorker()
		{
			Nudge(WorkerServiceTaskCode);
		}
		protected abstract string WorkerServiceTaskCode { get; }

		public void NudgeMaster()
		{
			Nudge(MasterServiceTaskCode);
		}
		protected abstract string MasterServiceTaskCode { get; }

		public void NudgeKeyGen()
		{
			Nudge(KeyGenServiceTaskCode);
		}
		protected abstract string KeyGenServiceTaskCode { get; }

		void Nudge(string code)
		{
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(code);
		}

		#region IDisposable Support

		bool disposedValue;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
