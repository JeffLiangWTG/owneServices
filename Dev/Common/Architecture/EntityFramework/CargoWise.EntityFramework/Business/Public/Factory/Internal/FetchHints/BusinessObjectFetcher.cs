using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.EntityFramework
{
	internal class BusinessObjectFetcher
	{
		#region Constructor

		internal BusinessObjectFetcher()
		{
			FetchHints = new Dictionary<IQueryHashKey, IImmediateHint>(new QueryHashKeyComparer());
		}

		#endregion

		public int PreCachedFetchHints
		{
			get { return FetchHints.Count; }
		}

		public void AddFetchHint(IImmediateHint fetchHint)
		{
			var hashKey = fetchHint.GetHashKeyObject();
			if (!FetchHints.ContainsKey(hashKey))
			{
				FetchHints[hashKey] = fetchHint;
			}
		}

		bool isFetching;

		int databaseLoadCountAtLastFetchExecute;

		class TypeQueryPair
		{
			public TypeQueryPair(Type businessObjectType, ZQuery query)
			{
				BusinessObjectType = businessObjectType;
				Query = query;
			}
			public readonly Type BusinessObjectType;
			public readonly ZQuery Query;
		}

		public void Fetch(BusinessObjectFactory factory)
		{
			if (!isFetching)    // Stops re-entrancy due to large queries...
			{
				if (factory.DatabaseLoadCount == databaseLoadCountAtLastFetchExecute)
				{
					return;
				}

				databaseLoadCountAtLastFetchExecute = factory.DatabaseLoadCount;

				isFetching = true;

				GroupSimpleHints();

				Type bizOType = null;
				ZQuery fetchQuery = null;
				int aggregatedFetches = 0;
				List<IQueryHashKey> executedHints = null;

				List<TypeQueryPair> queries = new List<TypeQueryPair>();
				foreach (var keyValuePair in FetchHints.ToArray())
				{
					IImmediateHint hint = keyValuePair.Value;

					if (hint.IsDataHintLoaded)
					{
						if (executedHints == null)
						{
							executedHints = new List<IQueryHashKey>(FetchHints.Count);
						}
						executedHints.Add(keyValuePair.Key);
						ZQuery hintQuery = hint.GetQuery();
						if (hintQuery != null)
						{
							if (bizOType != null && (bizOType != hint.BusinessObjectType || aggregatedFetches > 20))
							{
								bizOType = null;
								aggregatedFetches = 0;
							}
							if (bizOType == null)
							{
								bizOType = hint.BusinessObjectType;
								fetchQuery = new ZQuery();
								fetchQuery.FetchOnlyFromLocalCache = true;
								fetchQuery.DefaultJoinCondition = JoinCondition.Or;
								queries.Add(new TypeQueryPair(bizOType, fetchQuery));
							}
							fetchQuery.AddToFilter(hintQuery);
							aggregatedFetches++;
						}
					}
				}

				if (executedHints != null)
				{
					if (executedHints.Count == FetchHints.Count)
					{
						FetchHints.Clear();
					}
					else
					{
						foreach (var key in executedHints)
						{
							FetchHints.Remove(key);
						}
					}

					foreach (TypeQueryPair typeQueryPair in queries)
					{
						factory.Load(typeQueryPair.BusinessObjectType, typeQueryPair.Query);
					}
				}

				isFetching = false;
			}
		}

		internal void GroupSimpleHints()
		{
			if (FetchHints.Count > 1)
			{
				var groupedFetchHints = new Dictionary<IQueryHashKey, IList<IQueryHashKey>>(new QueryHashKeyComparer());

				foreach (var hintKeyValuePair in FetchHints)
				{
					FetchHint fetchHint = hintKeyValuePair.Value as FetchHint;
					if (fetchHint != null && fetchHint.IsDataHintLoaded)
					{
						var key = new FetchHint.EnumerableHashObject { hintKeyValuePair.Value.BuilderKey, hintKeyValuePair.Value.BusinessObjectType };

						IList<IQueryHashKey> fetchHints;
						if (!groupedFetchHints.TryGetValue(key, out fetchHints))
						{
							fetchHints = new List<IQueryHashKey>();
							groupedFetchHints.Add(key, fetchHints);
						}

						fetchHints.Add(hintKeyValuePair.Key);
					}
				}

				foreach (var fetchHints in groupedFetchHints.Values)
				{
					if (fetchHints.Count > 1)
					{
						var firstHintKey = fetchHints.First();
						var boType = FetchHints[firstHintKey].BusinessObjectType;
						var values = fetchHints.Select(key => ((FetchHint)FetchHints[key]).Value).ToArray();
						var column = ((FetchHint)FetchHints[firstHintKey]).Column;
						foreach (var key in fetchHints)
						{
							FetchHints.Remove(key);
						}

						for (int i = 0; i < values.Length; i += ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION)
						{
							var query = new ZQuery(column, values.Skip(i).Take(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION));
							var immediateZQueryFetchHint = new ImmediateZQueryFetchHint(boType, query) { IsDataHintLoaded = true };
							AddFetchHint(immediateZQueryFetchHint);
						}
					}
				}
			}
		}

		#region Implementation

		internal readonly Dictionary<IQueryHashKey, IImmediateHint> FetchHints;

		#endregion
	}
}
