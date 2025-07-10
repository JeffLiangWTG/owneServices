using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.ZArchitecture.Schema.CusCodeDataSchema))]
namespace CargoWise.EntityFramework
{
	public class PersistentFactoryCacheManager
	{
		#region Construction

		internal PersistentFactoryCacheManager()
		{
			persistentFactoryWeakReferences = new List<WeakReference>();
			persistentFactoryStrongReferences = new List<BusinessObjectFactory>();
		}
#if DEBUG
		public
#endif
		readonly List<WeakReference> persistentFactoryWeakReferences;
		readonly List<BusinessObjectFactory> persistentFactoryStrongReferences;

		[Common.Testing.SuppressThreadStaticFieldMessage]
		static PersistentFactoryCacheManager instance;
		readonly ThreadLocal<List<WeakReference>> trackedFactories = new ThreadLocal<List<WeakReference>>(false);

		[Common.Testing.SuppressThreadStaticFieldMessage]
		static readonly object instanceLock = new object();

		public static PersistentFactoryCacheManager Instance
		{
			get
			{
				if (instance == null)
				{
					lock (instanceLock)
					{
						if (instance == null)
						{
							instance = new PersistentFactoryCacheManager();
						}
					}
				}

				return instance;
			}
		}

		#endregion

		#region Factories

		public IDisposable TrackFactoriesCreatedOnCurrentThread()
		{
			if (trackedFactories.IsValueCreated && trackedFactories.Value != null)
			{
				ErrorReporter.ReportOnce("trackedfactoriescreatedoncurrentthreadrecursion", "TrackFactoriesCreatedOnCurrentThread was already called.");
				return DisposableAction.NoAction;
			}
			else
			{
				trackedFactories.Value = new List<WeakReference>();
				return new DisposableAction(() => trackedFactories.Value = null);
			}
		}

		public IEnumerable<BusinessObjectFactory> GetFactoriesCreatedInTrackedRegion()
		{
			if (trackedFactories.IsValueCreated && trackedFactories.Value != null)
			{
				return GetFactoriesFromWeakReferences().Where(x => x != null);
			}
			else
			{
				return Enumerable.Empty<BusinessObjectFactory>();
			}
		}

		internal virtual IEnumerable<BusinessObjectFactory> GetFactoriesFromWeakReferences() => trackedFactories.Value.Select(x => (BusinessObjectFactory)x.Target);

		/// <summary>
		/// Be very careful with these. 
		/// Don't call any functions on the result unless you personally have guaranteed the thread safety.
		/// </summary>
		public BusinessObjectFactory[] GetBusinessObjectFactories(bool includeDeactivated = false)
		{
			var factories = new List<BusinessObjectFactory>();
			lock (persistentFactoryWeakReferences)
			{
				var i = 0;
				while (i < persistentFactoryWeakReferences.Count)
				{
					var reference = persistentFactoryWeakReferences[i];
					var factory = reference.Target as BusinessObjectFactory;
					if (factory != null && (includeDeactivated || !factory.IsDeactivated) && !factory.IsUberFactory)
					{
						factories.Add(factory);
						i++;
					}
					else
					{
						persistentFactoryWeakReferences.RemoveAt(i);
					}
				}

				return factories.ToArray();
			}
		}

		/// <summary>
		/// Be very careful with these. 
		/// Don't call any functions on the result unless you personally have guaranteed the thread safety.
		/// </summary>
		internal RowFactory[] GetRowFactories()
		{
			var result = new List<RowFactory>();

			foreach (var factory in GetBusinessObjectFactories())
			{
				result.Add(factory.GetRowFactory(suppressThreadSentryCheck_neverSetThisToTrueUnlessYouKnowWhatYouAreDoing: true)); // Talk to someone who knows about thread safety before hacking here.
			}

			return result.ToArray();
		}

		internal void Add(BusinessObjectFactory factory)
		{
			lock (persistentFactoryWeakReferences)
			{
				persistentFactoryWeakReferences.Add(new WeakReference(factory));

				if (UsingStrongReferences)
				{
					persistentFactoryStrongReferences.Add(factory);
				}
			}

			if (trackedFactories.IsValueCreated && trackedFactories.Value != null)
			{
				trackedFactories.Value.Add(new WeakReference(factory, false));
			}

#if DEBUG
			if (factoryReferenceAnchorList_ForTest != null)
			{
				lock (instanceLock)
				{
					AddForTest(factory);
				}
			}
#endif
		}

		public void ClearAllQueryCaches()
		{
			foreach (var rowFactory in GetRowFactories())
			{
				rowFactory.ClearQueryCache(); // Made thread-safe in WI00326257
			}
		}

		public void ClearAllQueryCaches(IEnumerable<string> tableNames)
		{
			tableNames = tableNames.WhereNotNull();
			foreach (var rowFactory in GetRowFactories())
			{
				foreach (var tableName in tableNames)
				{
					rowFactory.ClearQueryCache(tableName); // Made thread-safe in WI00326257
				}
				rowFactory.ClearViewsQueryCache(tableNames); // Made thread-safe in WI00326257
			}
		}

		public bool UsingStrongReferences { get; private set; }

		public IDisposable MakeFactoryReferencesStrong()
		{
			if (UsingStrongReferences)
			{
				return new DisposableAction(() => { });
			}
			lock (persistentFactoryWeakReferences)
			{
				foreach (var reference in persistentFactoryWeakReferences)
				{
					if (reference.Target is BusinessObjectFactory factory)
					{
						persistentFactoryStrongReferences.Add(factory);
					}
				}
			}
			UsingStrongReferences = true;
			return new DisposableAction(() => DiscardFactoryStrongReferences());
		}

		void DiscardFactoryStrongReferences()
		{
			persistentFactoryStrongReferences.Clear();
			UsingStrongReferences = false;
		}

		#endregion

		#region Test
#if DEBUG

		public IDisposable TrackAllCreatedFactories_ForTest(string[] tablesToTrack = null, Func<string, bool> shouldEnableLoggingForExistingFactory = null, bool logNonPersistentTableHit = false)
		{
			if (factoryReferenceAnchorList_ForTest == null)
			{
				lastCallStackForTrackAllCreatedFactories_ForTest = Environment.StackTrace;
				fTablesToTrack = tablesToTrack;
				this.logNonPersistentTableHit = logNonPersistentTableHit;
				factoryReferenceAnchorList_ForTest = new LinkedList<(IDisposable disposable, BusinessObjectFactory factory)>();
				if (tablesToTrack != null)
				{
					EnableLoggingAgainstExistingFactories(shouldEnableLoggingForExistingFactory);
				}
				return new DisposableAction(() =>
				{
					if (factoryReferenceAnchorList_ForTest != null)
					{
						factoryReferenceAnchorList_ForTest.ForEach(x => x.disposable?.Dispose());
						factoryReferenceAnchorList_ForTest = null;
						lastCallStackForTrackAllCreatedFactories_ForTest = null;
					}
				});
			}
			else
			{
				throw new InvalidOperationException("TrackAllCreatedFactories_ForTest was called while previous call was not disposed:\r\n" + lastCallStackForTrackAllCreatedFactories_ForTest);
			}
		}

		void EnableLoggingAgainstExistingFactories(Func<string, bool> shouldEnableLoggingForExistingFactory = null)
		{
			foreach (var factory in GetBusinessObjectFactories().Where(x =>
			{
				var name = x.NameForDebugging;
				return name.StartsWith("Registry Factory", StringComparison.Ordinal) || (shouldEnableLoggingForExistingFactory?.Invoke(name) ?? false);
			}))
			{
				AddForTest(factory);
			}
		}

		void AddForTest(BusinessObjectFactory factory)
		{
			IDisposable disposableTableHitQueryCollection = null;
			if (fTablesToTrack != null)
			{
				disposableTableHitQueryCollection = factory.EnableTableHitQueryCollection(fTablesToTrack);
			}

			IDisposable disposableLogNonPersistentTableHit = null;
			if (logNonPersistentTableHit)
			{
				disposableLogNonPersistentTableHit = factory.EnableLogNonPersistentTableHit();
			}
			factoryReferenceAnchorList_ForTest.AddLast((new DisposableAction(() =>
			{
				disposableTableHitQueryCollection?.Dispose();
				disposableLogNonPersistentTableHit?.Dispose();
			}), factory));
			onFactoryAdded.Value?.Invoke(factory);
		}

		string[] fTablesToTrack;
		bool logNonPersistentTableHit;

		LinkedList<(IDisposable disposable, BusinessObjectFactory factory)> factoryReferenceAnchorList_ForTest;
		string lastCallStackForTrackAllCreatedFactories_ForTest;

		public IEnumerable<BusinessObjectFactory> TrackedFactories_ForTest
		{
			get
			{
				lock (instanceLock)
				{
					return factoryReferenceAnchorList_ForTest?.Select(x => x.factory).ToList();
				}
			}
		}

		public Action<BusinessObjectFactory> OnFactoryAdded
		{
			get => onFactoryAdded.Value;
			set => onFactoryAdded.Value = value;
		}
		readonly Overridable<Action<BusinessObjectFactory>> onFactoryAdded = new Overridable<Action<BusinessObjectFactory>>();

#endif
		#endregion
	}
}
