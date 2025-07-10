using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common.MemoryManagement;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public class NudgeEventDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NudgeEventDetails(ZString eventType, ZString description, ZDateTime eventTime, ZString tasks, ZInt retriesRemaining)
		{
			EventType = eventType;
			Description = description;
			EventTime = eventTime;
			Tasks = tasks;
			RetriesRemaining = retriesRemaining;
		}

		public ZDateTime EventTime { get; private set; }
		public ZString EventType { get; private set; }
		public ZString Description { get; private set; }
		public ZString Tasks { get; private set; }
		public ZInt RetriesRemaining { get; private set; }
	}

	public class StaticCacheDetails : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StaticCacheDetails(IReclaimable reclaimable)
		{
			Reclaimable = reclaimable;
			CleanUpState = Res.GetString("08b236a4-7985-4bac-b8a5-5e854cf7c22c", "Unknown");
		}
		internal IReclaimable Reclaimable { get; private set; }

		public ZString Description
		{
			get { return Reclaimable.Description; }
		}

		public ZDateTime ConstructionTime
		{
			get { return Reclaimable.ConstructionTime; }
		}

		public ZString CleanUpState
		{
			get;
			internal set;
		}
	}

	public class StaticCacheCollection : NonPersistentBusinessObjectCollection<StaticCacheDetails>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}

	public class NudgeEventsCollection : NonPersistentBusinessObjectCollection<NudgeEventDetails>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}

	public class UncollectedTypesCollection : NonPersistentBusinessObjectCollection<UncollectedType>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}

	public class UncollectedType : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZString TypeName { get; internal set; }
		public ZInt Quantity { get; internal set; }
	}

	public class PerformanceStatistic : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PerformanceStatistic()
		{
			uncollectedTypes = new UncollectedTypesCollection();
			Load();
		}

		#region Schema

		public static class Schema
		{
			public const string CollectionSubscriptionCount = "CollectionSubscriptionCount";
			public const string SubscriptionCount = "SubscriptionCount";
		}

		#endregion

		public void Reclaim(FlushAction action, IEnumerable<StaticCacheDetails> itemsToReclaim)
		{
			foreach (var cacheDetail in itemsToReclaim)
			{
				string result = null;
				switch (cacheDetail.Reclaimable.CleanUp(action))
				{
					case FlushResult.NotRequired:
						result = Res.GetString("f99ba89b-48bc-4818-b929-aad13fdd9b0e", "Not Required");
						break;
					case FlushResult.Partial:
						result = Res.GetString("6b206f1e-8c96-4851-bc4a-b88a33b28d56", "Partial");
						break;
					case FlushResult.Exhausted:
						result = Res.GetString("7bc2d695-5eab-45a3-baf7-8ccfaa02a247", "Complete");
						break;
					default:
						result = Res.GetString("fac8661e-6ec3-4f2d-8f74-0e07e85d4278", "Unknown");
						break;
				}
				cacheDetail.CleanUpState = result;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "Force clean released objects for correct performance statistics report")]
		public virtual void Load()
		{
			FactoryStatistics.RemoveAll();
			StaticCacheContents.RemoveAll();
			UncollectedTypes.RemoveAll();
			NudgeEventsContents.RemoveAll();

			GC.Collect(); // Force clean released objects for correct performance statistics report

			foreach (BusinessObjectFactory factoryToMonitor in PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories(includeDeactivated: true))
			{
				FactoryStatistics.Add(new BusinessObjectFactoryStatistic(factoryToMonitor));
			}

			foreach (IReclaimable reclaimable in MemoryManager.Examine())
			{
				StaticCacheContents.Add(new StaticCacheDetails(reclaimable));
			}

			foreach (var type in DisposableLeakListener.Instance.GetDisposedNotCollectedTypes())
			{
				UncollectedTypes.Add(new UncollectedType() { TypeName = type.TypeName, Quantity = type.Quantity });
			}

			if (Globals.IsUserInteractive)
			{
				foreach (var nudgeEvent in ObjectFactory.Get<INudgingEventsTracker>().Events)
				{
					NudgeEventsContents.Add(new NudgeEventDetails(nudgeEvent.EventType, nudgeEvent.Description, nudgeEvent.EventTime, string.Join(",", nudgeEvent.TaskCodes), nudgeEvent.RetriesRemaining));
				}
			}

			SortCollections();

			RefreshBindingIncludingChildren();
		}

		void SortCollections()
		{
			BusinessObjectCollection[] collections = { FactoryStatistics, StaticCacheContents, UncollectedTypes, NudgeEventsContents };
			foreach (var item in collections)
			{
				if (item.SortInformation != null)
				{
					item.Sort(item.SortInformation);
				}
			}
		}

		#region Properties

		public ZBool LeakTrackingEnabled
		{
			get { return DisposableLeakListener.Instance.LeakTrackingEnabled; }
			set { DisposableLeakListener.Instance.LeakTrackingEnabled = value; }
		}

		public ZInt CollectionSubscriptionCount
		{
			get { return DataRefreshManager.CollectionSubscriptionCount; }
		}

		public ZPropertyInfo CollectionSubscriptionCountInfo
		{
			get { return GetZPropertyInfo(Schema.CollectionSubscriptionCount); }
		}

		public ZInt SubscriptionCount
		{
			get { return DataRefreshManager.SubscriptionCount; }
		}

		public ZPropertyInfo SubscriptionCountInfo
		{
			get { return GetZPropertyInfo(Schema.SubscriptionCount); }
		}

		#endregion

		#region Collections

		public StaticCacheCollection StaticCacheContents
		{
			get
			{
				if (staticCacheContents == null)
				{
					staticCacheContents = new StaticCacheCollection();
				}
				return staticCacheContents;
			}
		}
		StaticCacheCollection staticCacheContents;

		public NudgeEventsCollection NudgeEventsContents
		{
			get
			{
				if (nudgeEventsContents == null)
				{
					nudgeEventsContents = new NudgeEventsCollection();
				}
				return nudgeEventsContents;
			}
		}
		NudgeEventsCollection nudgeEventsContents;

		public BusinessObjectFactoryStatisticCollection FactoryStatistics
		{
			get
			{
				if (factoryStatistics == null)
				{
					factoryStatistics = new BusinessObjectFactoryStatisticCollection(Factory);
				}
				return factoryStatistics;
			}
		}

		public UncollectedTypesCollection UncollectedTypes
		{
			get { return uncollectedTypes; }
		}
		readonly UncollectedTypesCollection uncollectedTypes;

		#endregion

		#region Overrides

		public override void Delete()
		{
			throw new NotSupportedException();
		}
		#endregion

		#region Implementation

		BusinessObjectFactoryStatisticCollection factoryStatistics;

		#endregion
	}
}
