using System;
using System.Collections.Generic;
using CargoWise.Common.Testing;

namespace CargoWise.EntityFramework
{
	internal class BusinessObjectFetchHintManager
	{
		#region Constructor

		public BusinessObjectFetchHintManager(BusinessObjectFactory factory)
		{
#if DEBUG
			_Instance = _NextInstance++;
#endif
			BusinessObjectFetchers = new Dictionary<string, BusinessObjectFetcher>();
			Factory = factory;
		}
#if DEBUG
		public readonly int _Instance;
		[SuppressThreadStaticFieldMessage]
		static int _NextInstance;
#endif

		#endregion

		internal BusinessObjectFactory Factory
		{
			get;
			private set;
		}

		public void AddFetchHint(IImmediateHint fetchHint)
		{
			BusinessObjectFetcher fetcher = GetBusinessObjectFetcher(fetchHint.TableName);
			fetcher.AddFetchHint(fetchHint);
		}

		public bool HasFetchHints(string tableName)
		{
			return ActiveFetchHintsForTable(tableName) > 0;
		}

#if DEBUG
		#region Statististics
		public int ActiveTableFetchHints
		{
			get { return BusinessObjectFetchers.Count; }
		}

		public void Clear()
		{
			BusinessObjectFetchers.Clear();
		}

		#endregion
#endif

		public int ActiveFetchHintsForTable(string tableName)
		{
			BusinessObjectFetcher fetcher;
			if (BusinessObjectFetchers.TryGetValue(tableName, out fetcher))
			{
				return fetcher.PreCachedFetchHints;
			}
			else
			{
				return 0;
			}
		}

		public void FetchTable(string tableName)
		{
			BusinessObjectFetcher fetcher;
			if (BusinessObjectFetchers.TryGetValue(tableName, out fetcher))
			{
				fetcher.Fetch(Factory);
				// If objects in this table have fetch hints on the same table (for example, CusAddInfo), this function
				// can be called recursively in "fetcher.Fetch(Factory);". Therefore, the current "fetcher" may be already
				// removed from BusinessObjectFetchers and another fetcher with new fetch hints may be added with the same key.
				// So, we need to refresh the fetcher corresponding to the tableName.
				if (BusinessObjectFetchers.TryGetValue(tableName, out fetcher) &&
					fetcher.PreCachedFetchHints == 0)
				{
					BusinessObjectFetchers.Remove(tableName);
				}
			}
		}

		#region Implementation

		BusinessObjectFetcher GetBusinessObjectFetcher(String tableName)
		{
			BusinessObjectFetcher result;
			if (!BusinessObjectFetchers.TryGetValue(tableName, out result))
			{
				result = new BusinessObjectFetcher();
				BusinessObjectFetchers[tableName] = result;
			}
			return result;
		}

		readonly Dictionary<string, BusinessObjectFetcher> BusinessObjectFetchers;

		#endregion
	}
}
