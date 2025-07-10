using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	internal class TableFetcher
	{
		#region Constructor

		internal TableFetcher(string tableName)
		{
			this.TableName = tableName;
			FetchHints = new Dictionary<IQueryHashKey, IFetchHint>(new QueryHashKeyComparer());
		}

		#endregion

		public int PreCachedFetchHints
		{
			get { return FetchHints.Count; }
		}

		public void AddFetchHint(IFetchHint fetchHint)
		{
			Argument.NotNull(fetchHint, nameof(fetchHint));

			var hashKey = fetchHint.GetHashKeyObject();

			if (fetchHint.TableName.Equals("StmALog", StringComparison.OrdinalIgnoreCase))
			{
				if (HasFetchHintOnStmALogPK(hashKey))
				{
					ErrorReporter.ReportOnce("FetchHintOnStmALogPK", "Fetch hints may not be added Querying StmaLog by SL_PK");
					return;
				}
			}

			LoadedFetchHintCount++;

			if (!FetchHints.ContainsKey(hashKey))
			{
				FetchHints.Add(hashKey, fetchHint);
			}
		}

		bool HasFetchHintOnStmALogPK(IQueryHashKey fetchHintPart)
		{
			foreach (var part in fetchHintPart.KeyParts)
			{
				if (part is string s &&
					s.IndexOf("SL_PK", StringComparison.OrdinalIgnoreCase) > -1
					&& s.IndexOf("SL_Parent", StringComparison.OrdinalIgnoreCase) < 0)
				{
					return true;
				}
			}
			return false;
		}

		public TableNameQuery[] GetTableNameQueries(FetchHintQueryStorer fetchHintQueryStorer)
		{
			var fetchHintQueryCacheManager = new FetchHintQueryCacheManager(FetchHints.Values);
			var result = new List<TableNameQuery>();

			while (FetchHints.Count > 0)
			{
				var fetchHintKeys = BuildQueries(fetchHintQueryStorer, fetchHintQueryCacheManager);
				result.AddRange(GetQueries());

				if (fetchHintKeys.Count == FetchHints.Count)
				{
					FetchHints.Clear();
				}
				else
				{
					foreach (var key in fetchHintKeys)
					{
						FetchHints.Remove(key);
					}
				}
			}

			return result.ToArray();
		}

		public int LoadedFetchHintCount { get; private set; }

#if DEBUG
		public void ClearLoadedFetchHintCount()
		{
			LoadedFetchHintCount = 0;
		}
#endif

		#region Implementation

		[ThreadSafe]
		internal static readonly int MAXIMUM_PARAMETER_COUNT = ObjectFactory.Get<IEntityFrameworkSettings>().MaximumParametersPerFetchHint;

		internal List<IQueryHashKey> BuildQueries(FetchHintQueryStorer fetchHintQueryStorer, FetchHintQueryCacheManager fetchHintQueryCacheManager)
		{
			var fetchHintKeys = new List<IQueryHashKey>();
			activeProviderQueries = new Dictionary<string, QueryBuilderProvider>();
			providerQueriesWithMaximumParameterCount = new List<QueryBuilderProvider>();
			HashSet<SchemaColumn> loadWithBlobs = null;
			foreach (var keyValuePair in FetchHints.ToArray())
			{
				if (!FetchHints.ContainsKey(keyValuePair.Key))
				{
					continue;
				}

				var hint = keyValuePair.Value;
				if (loadWithBlobs == null)
				{
					loadWithBlobs = new HashSet<SchemaColumn>(hint.LoadWithBlobs);
				}

				if (loadWithBlobs.SetEquals(hint.LoadWithBlobs))
				{
					fetchHintKeys.Add(keyValuePair.Key);
					hint.IsDataHintLoaded = true;
					if (fetchHintQueryCacheManager.IsShortestSQLScriptFetchHint(hint))
					{
						var provider = GetProviderForHint(hint);
						var builder = provider.Builder;

						fetchHintQueryStorer.AddQuery(TableName, hint.GetQuery());
						hint.GenerateQuery(builder);
					}
				}
			}

			return fetchHintKeys;
		}

		QueryBuilderProvider GetProviderForHint(IFetchHint fetchHint)
		{
			var builderKey = fetchHint.BuilderKey;

			if (!activeProviderQueries.TryGetValue(builderKey, out var provider) || provider.Builder.ParameterCount >= MAXIMUM_PARAMETER_COUNT)
			{
				if (provider != null)
				{
					providerQueriesWithMaximumParameterCount.Add(provider);
					activeProviderQueries.Remove(builderKey);
				}

				provider = new QueryBuilderProvider
				{
					Builder = new QueryBuilder(),
					FetchHint = fetchHint
				};
				activeProviderQueries.Add(builderKey, provider);
			}

			return provider;
		}

		IList<TableNameQuery> GetQueries()
		{
			var result = new List<TableNameQuery>();
			var filter = new ZQuery();

			var concatenateHintTypes = false;
			var settings = ObjectFactory.Get<IEntityFrameworkSettings>();
			if (settings != null)
			{
				concatenateHintTypes = settings.ConcatenateMultipleFetchHintTypes;
			}
			var parameterCounts = 0;

			foreach (var provider in activeProviderQueries.Values.Union(providerQueriesWithMaximumParameterCount))
			{
				if (!filter.IsEmpty && (!concatenateHintTypes
						|| filter.MaximumRows > 0
						|| !new HashSet<SchemaColumn>(provider.FetchHint.LoadWithBlobs).SetEquals(filter.LoadWithBlobs)
						|| parameterCounts + provider.Builder.ParameterCount > ZSQLInFilter.MAXIMUM_SUPPORTS_COUNT_FOR_PARAMETER))
				{
					result.Add(new TableNameQuery(TableName, filter));
					filter = new ZQuery();
					parameterCounts = 0;
				}

				var fetchHintQuery = provider.Builder.GetQuery();
				if (!fetchHintQuery.IsEmpty)
				{
					if (!filter.IsEmpty)
					{
						filter.AddToFilter(fetchHintQuery, JoinCondition.UnionAll);
						filter.IsDBOnlyQuery |= fetchHintQuery.IsDBOnlyQuery;
						filter.MaximumRows = fetchHintQuery.MaximumRows;
					}
					else
					{
						filter = fetchHintQuery;
					}
					filter.IncludeBlob(provider.FetchHint.LoadWithBlobs);
					parameterCounts += provider.Builder.ParameterCount;
				}
			}

			if (!filter.IsEmpty)
			{
				result.Add(new TableNameQuery(TableName, filter));
			}

			return result;
		}

		internal Dictionary<string, QueryBuilderProvider> activeProviderQueries;
		List<QueryBuilderProvider> providerQueriesWithMaximumParameterCount;
		internal readonly Dictionary<IQueryHashKey, IFetchHint> FetchHints;
		readonly string TableName;

		#endregion

		internal class QueryBuilderProvider
		{
			public QueryBuilder Builder { get; set; }
			public IFetchHint FetchHint { get; set; }
		}
	}
}
