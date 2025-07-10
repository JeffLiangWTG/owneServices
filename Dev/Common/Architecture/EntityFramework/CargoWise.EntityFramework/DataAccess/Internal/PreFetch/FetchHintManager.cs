using System;
using System.Collections.Generic;
using CargoWise.Common.Testing;

namespace CargoWise.EntityFramework
{
	internal class FetchHintManager
	{
		#region Constructor

		public FetchHintManager(RowFactory factory)
		{
#if DEBUG
			_Instance = _NextInstance++;
#endif
			this.factory = factory;
			TableFetchers = new Dictionary<string, TableFetcher>(StringComparer.OrdinalIgnoreCase);
		}
#if DEBUG
		public readonly int _Instance;
		[SuppressThreadStaticFieldMessage]
		static int _NextInstance;
#endif

		#endregion

		public void AddFetchHint(IFetchHint fetchHint)
		{
			TableFetcher tableFetcher = GetTableFetcher(fetchHint.TableName);
			tableFetcher.AddFetchHint(fetchHint);
		}

		public bool HasFetchHints(string tableName)
		{
			return ActiveFetchHintsForTable(tableName) > 0;
		}

		#region Statistics
		public int ActiveTableFetchHints
		{
			get { return TableFetchers.Count; }
		}

		public virtual int GetLoadedFetchHintCountForTable(string tableName)
		{
			return -1;
		}

#if DEBUG

		public void Clear()
		{
			TableFetchers.Clear();
		}

		public IEnumerable<string> GetAllFetchHintedTableNames()
		{
			return TableFetchers.Keys;
		}

#endif

		public virtual void ClearLoadedFetchHintCountForTable(string tableName)
		{
		}

		#endregion

		public int ActiveFetchHintsForTable(string tableName)
		{
			TableFetcher tableFetcher;
			if (TableFetchers.TryGetValue(tableName, out tableFetcher))
			{
				return tableFetcher.PreCachedFetchHints;
			}
			else
			{
				return 0;
			}
		}

		public void FetchAllTables()
		{
			try
			{
				List<TableNameQuery> tableNameFilters = new List<TableNameQuery>();
				using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(factory.QueryCache))
				{
					foreach (TableFetcher fetcher in TableFetchers.Values)
					{
						tableNameFilters.AddRange(fetcher.GetTableNameQueries(fetchHintQueryStorer));
					}
					TableFetchers.Clear();
#if DEBUG
					using (factory.SuspendTableHitCounterForFetchHintsIfNeeded())
#endif
					{
						factory.FetchRowsIntoDataSet(tableNameFilters);
					}
				}
			}
			catch (SqlException)
			{
				PersistentFactoryCacheManager.Instance.ClearAllQueryCaches();
				throw;
			}
		}

		public virtual void FetchTable(string tableName)
		{
			FetchTable(tableName, out _);
		}

		protected void FetchTable(string tableName, out int loadedFetchHints)
		{
			TableFetcher tableFetcher;
			loadedFetchHints = 0;
			if (TableFetchers.TryGetValue(tableName, out tableFetcher))
			{
				List<TableNameQuery> tableNameFilters = new List<TableNameQuery>();
				try
				{
					using (FetchHintQueryStorer fetchHintQueryStorer = new FetchHintQueryStorer(factory.QueryCache))
					{
						tableNameFilters.AddRange(tableFetcher.GetTableNameQueries(fetchHintQueryStorer));

						loadedFetchHints = tableFetcher.LoadedFetchHintCount;

						TableFetchers.Remove(tableName);
#if DEBUG
						using (factory.SuspendTableHitCounterForFetchHintsIfNeeded())
#endif
						{
							factory.FetchRowsIntoDataSet(tableNameFilters);
						}
					}
				}
				catch (SqlException)
				{
					PersistentFactoryCacheManager.Instance.ClearAllQueryCaches([tableName]);
					throw;
				}
			}
		}

		#region Implementation

		TableFetcher GetTableFetcher(String tableName)
		{
			TableFetcher result;
			if (!TableFetchers.TryGetValue(tableName, out result))
			{
				result = new TableFetcher(tableName);
				TableFetchers.Add(tableName, result);
			}
			return result;
		}

		internal readonly Dictionary<string, TableFetcher> TableFetchers;
		readonly RowFactory factory;

		#endregion
	}
}
