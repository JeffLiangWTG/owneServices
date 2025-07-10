using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine
{
	internal sealed class ProviderCache
	{
		public ProviderCache()
		{
			firstCache = new ConcurrentDictionary<string, ValueProvider>();
			secondCache = new ConcurrentDictionary<string, ValueProvider>();
			AddedProviderIDs = new List<string>();
			DBProvider = new DBOrBOValueProvider();
			Providers = new ValueProviderList();
		}

		ProviderCache(ConcurrentDictionary<string, ValueProvider> firstCache, ConcurrentDictionary<string, ValueProvider> secondCache, List<string> addedProviderIDs, ValueProviderList providers)
		{
			this.firstCache = firstCache;
			this.secondCache = secondCache;
			DBProvider = new DBOrBOValueProvider();
			this.AddedProviderIDs = addedProviderIDs;
			this.Providers = providers;
		}

		readonly ConcurrentDictionary<string, ValueProvider> firstCache;
		readonly ConcurrentDictionary<string, ValueProvider> secondCache;
#if DEBUG
		internal
#endif
		readonly List<string> AddedProviderIDs;
		public readonly DBOrBOValueProvider DBProvider;
		public readonly ValueProviderList Providers;

		public void AddProvider(ValueProvider provider)
		{
			var providerID = provider.PassToStartReplacingOn.ToString() + provider.Regex.ToString();
			if (!AddedProviderIDs.Contains(providerID))
			{
				Providers.Add(provider);
				AddedProviderIDs.Add(providerID);
			}
		}

		/// <summary>
		/// Shallow clone on AddedProviderIDs and Providers
		/// </summary>
		internal ProviderCache Clone()
		{
			return new ProviderCache(this.firstCache, this.secondCache, new List<string>(this.AddedProviderIDs), new ValueProviderList(this.Providers));
		}

		public ValueProvider GetProviderResponsibleForIncludingDBProvider(string macro, Passes pass, Report report)
		{
			var result = GetProviderResponsibleFor(macro, pass);
			if (result == null)
			{
				if (report != null && report.Style == Report.Styles.Report && !report.ColumnHeadingsProcessedByReportAnalyser)
				{
					return null;
				}

				if (DBProvider.IsResponsibleForReplacingFromDataBaseOrBusinessObject(macro, report))
				{
					result = DBProvider;
				}
				GetCacheForPass(pass)[macro] = result;
			}
			return result;
		}

		internal static ProviderCache InitialValueProvidersCache
		{
			get
			{
				if (!providersCache.IsOverriden)
				{
					providersCache.Value = new ValueProviderCollector().ValueProviders;
				}
				return providersCache.Value;
			}
		}

		[ThreadSafe]
		readonly static Overridable<ProviderCache> providersCache = new Overridable<ProviderCache>(null);

#if DEBUG
		public int Count => Providers.Count;

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		public bool IsCached(string macro)
		{
			return GetCacheForPass(Passes.FirstPass).ContainsKey(macro) || GetCacheForPass(Passes.SecondPass).ContainsKey(macro);
		}
#endif
		#region Implementation

		int sortCount;

		int ProviderSorter(ValueProvider x, ValueProvider y)
		{
			return x.GetUsageCount().CompareTo(y.GetUsageCount());
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		public ValueProvider GetProviderResponsibleFor(string macro, Passes pass)
		{
			var cache = GetCacheForPass(pass);
			if (!cache.TryGetValue(macro, out var result))
			{
				sortCount++;
				if (sortCount % 100 == 0)
				{
					Providers.Sort(ProviderSorter);
				}

				result = Providers.AsParallel()
								.AsOrdered()
								.WithDegreeOfParallelism(10)
								.Where(provider => provider.IsResponsibleForReplacing(macro, pass) || provider.ShouldReplaceNestedMacrosWhileIrrisponsible(macro, pass))
								.FirstOrDefault();
				result?.IncrementUsageCount();
				cache[macro] = result;
			}
			return result;
		}

		ConcurrentDictionary<string, ValueProvider> GetCacheForPass(Passes pass)
		{
			return pass == Passes.FirstPass ? firstCache : secondCache;
		}

		#endregion
	}
}
