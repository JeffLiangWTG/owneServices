using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	/// <summary>
	/// All this service tas does is maps the value of Z0_Number to Z0_AnotherNumber.
	/// </summary>
	public class DummyGrEngineServiceTask
	{
		public DummyGrEngineServiceTask(Func<BusinessObjectFactory> factoryProvider, int dequeueBatchSize, int maxBacklogSize = -1, int capacity = 0, GrEngineEnums.PreKeyOption keyOption = GrEngineEnums.PreKeyOption.None, GrEngineEnums.ChainOption chainOption = GrEngineEnums.ChainOption.No)
		{
			FactoryProvider = factoryProvider;
			var lockProvider =  new LockProvider(((IDbConnected)FactoryProvider()).Connection);
			Setup = new GrEngineServiceSetup<StmQueueState>(new StmQueueStateFactory(() => new GrEngineLogOptions()), int.MaxValue, dequeueBatchSize, maxBacklogSize, capacity, keyOption, chainOption, lockProvider);
			Dequeuer = new GrEngineDequeuer<StmQueueState>(Setup);
			Enqueuer = new DummyEnqueuer(Setup, Dequeuer);
			PreEnqueuer = new DummyPreEnqueuer(Setup);
			MaxEnqueues = 1000;
		}

		public GrEngineServiceSetup<StmQueueState> Setup { get; }
		public DummyPreEnqueuer PreEnqueuer { get; }
		public GrEngineEnqueuer<DummyBusinessObject, StmQueueState> Enqueuer { get; }
		public GrEngineDequeuer<StmQueueState> Dequeuer { get; }

		public int MaxEnqueues { get; set; }
		public Func<BusinessObjectFactory> FactoryProvider { get; }

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			var enqueues = 0;
			notifications?.Add(CargoWise.ComponentModel.NotificationType.Information, "Starting");

			var factory = FactoryProvider();

			var query = Setup.PreKeyMode == GrEngineEnums.PreKeyOption.Mandatory ? new Lazy<ZQuery>(() => new ZQuery()) : new Lazy<ZQuery>(() => new ZQuery(DummyBizoSchema.Z0_AnotherNumber, 0));

			while (enqueues++ < MaxEnqueues && Enqueuer.Enqueue(factory, query, notifications).TotalEntities > 0)
			{
				using (var batch = Dequeuer.LoadBatch(factory, notifications))
				{
					TransformBatch(factory, notifications, batch);
					Dequeuer.Notify(batch.Select(e => new QueueStateResult<StmQueueState>(e, QueueStateResultType.Processed)));
				}
			}
			var queueStateFactory = Setup.Factory;
			queueStateFactory.DeleteOldProcessed(notifications, 48);
			notifications?.Add(CargoWise.ComponentModel.NotificationType.Information, "Ending");
		}

		protected virtual void TransformBatch(BusinessObjectFactory factory, INotifications notifications, IEnumerable<StmQueueState> batch)
		{
			foreach (var item in batch)
			{
				var bizo = factory.Load<DummyBusinessObject>(item.ParentID);
				bizo.Z0_AnotherNumber += bizo.Z0_Number;
				bizo.Z0_Decimal = processOrder++;
				bizo.Factory.Save();
			}
		}

		int processOrder;

		#region Enqueuer Implementation

		class DummyEnqueuer : GrEngineEnqueuer<DummyBusinessObject, StmQueueState>
		{
			public DummyEnqueuer(GrEngineServiceSetup<StmQueueState> setup, GrEngineDequeuer<StmQueueState> dequeuer)
			: base(setup, dequeuer)
			{
			}

			protected override ZString GetMessageNumber(DummyBusinessObject b)
			{
				var date = b.Z0_Date;
				if (date.IsValid)
				{
					return date.ToLongTimeString();
				}
				else
				{
					return b.InstantiationTime.ToLongTimeString();
				}
			}

			protected override string[] GetKeys(DummyBusinessObject b)
			{
				return b.GetKeys().Select(s => s.ToString()).ToArray();
			}

			protected override ITableSchema GetSchema()
			{
				return DummyBizoSchema.Instance;
			}
		}

		public class DummyPreEnqueuer : GrEnginePreEnqueuer<DummyBusinessObject, StmQueueState>
		{
			public DummyPreEnqueuer(GrEngineServiceSetup<StmQueueState> setup) : base(setup)
			{
			}

			public DummyPerformanceMonitor DummyPerformanceMonitor { get; } = new DummyPerformanceMonitor(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
			protected override PerformanceMonitor Monitor => DummyPerformanceMonitor;

			protected override ZString GetMessageNumber(DummyBusinessObject b)
			{
				var date = b.Z0_Date;
				if (date.IsValid)
				{
					return date.ToLongTimeString();
				}
				else
				{
					return b.InstantiationTime.ToLongTimeString();
				}
			}
		}

		#endregion

		#region LockProvider Implementation

		class LockProvider : ISqlApplicationLockProvider
		{
			public LockProvider(DbConnection connection)
			{
				this.connection = connection;
			}
			readonly DbConnection connection;

			public bool TryGetLock(string key, out ISqlApplicationLock appLock, string dbName = null)
			{
				var result = connection.TryGetLock(key, out var sqlLock, dbName);
				appLock = sqlLock;
				return result;
			}

			public bool TryGetLock(string key, TimeSpan lockTimeout, out ISqlApplicationLock appLock, string dbName = null)
			{
				var result = connection.TryGetLock(key, lockTimeout, out var sqlLock, dbName);
				appLock = sqlLock;
				return result;
			}
		}

		#endregion
	}
}
