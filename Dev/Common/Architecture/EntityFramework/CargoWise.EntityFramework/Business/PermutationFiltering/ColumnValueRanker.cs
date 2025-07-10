using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public interface IColumnValueRankerInternals
	{
		IEnumerable<ColumnValueRanker.ColumnValuesPair> ColumnValues { get; }
	}

	public class ColumnValueRanker : ColumnValueRankerBase, IColumnValueRanker, IColumnValueRankerInternals
	{
		public ColumnValueRanker()
		{
			list = new List<ColumnValuesPair>();
		}

		void IColumnValueRanker.Add(object schemaColumn, params object[] values)
		{
			Add((SchemaColumn)schemaColumn, values);
		}

		/// <summary>
		/// Adds a ranking of the given column with the values in order of
		/// priority, from most important to least important.
		///
		/// The ranking will first check that `column` matches the first parameter,
		/// if not then it will match the second, if not then the third, and so on.
		/// </summary>
		public void Add(SchemaColumn column, params object[] values)
		{
			elementCount += values.Length;
			list.Add(new ColumnValuesPair(column, values));
		}

		public void AddEnumerable(SchemaColumn column, IEnumerable<object> values)
		{
			Add(column, values.ToArray());
		}

		int elementCount;
		readonly List<ColumnValuesPair> list;

		#region Get Values - Test Only
#if DEBUG

		public object[] GetValues(SchemaColumn column)
		{
			foreach (ColumnValuesPair pair in list)
			{
				if (pair.Column == column)
				{
					return pair.Values;
				}
			}
			throw new ArgumentException("No values have been created using the Add(SchemaColumn, params IZType[]) method for:" + column.Name, nameof(column));
		}

#endif
		#endregion

		#region Get Best Match

		/// <summary>
		/// Load the business object matching the 'mainQuery' and colum ranker values.
		/// The results will be ordered in the order the column ranker values were specified.
		/// The results will be cached for the specified amount of minutes.
		/// If 'useInMemoryFiltering' is true, the result of 'mainQuery' will be cached and the column values will be filtered in memory.
		/// Use in memory filtering when the results of the query provided are small and can be reused when the column ranker values change.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="factory">The factory to load the results into</param>
		/// <param name="mainQuery">Results are returned matching this query and the column ranker values</param>
		/// <param name="useInMemoryFiltering">When true, the full results from 'mainQuery' will be loaded and the column ranker values will be filtered in memory</param>
		/// <param name="cacheKey">Only used for in memory filtering</param>
		/// <param name="cacheDurationInMins">The results will be cached for the specified time in minutes</param>
		/// <returns></returns>
		public T[] GetBestMatches<T>(BusinessObjectFactory factory, ZQuery mainQuery, bool useInMemoryFiltering, ZString cacheKey, int cacheDurationInMins)
			where T : BusinessObject
		{
			if (useInMemoryFiltering)
			{
				return GetBestMatchesWithInMemoryFiltering<T>(factory, mainQuery, cacheKey, cacheDurationInMins);
			}
			else
			{
				return GetBestMatches<T>(factory, mainQuery, cacheDurationInMins);
			}
		}

		T[] GetBestMatches<T>(BusinessObjectFactory factory, ZQuery mainQuery, int cacheDurationInMins)
			where T : BusinessObject
		{
			T[] result;
			var fullQuery = GetFullQuery<T>(mainQuery);

			if (cacheDurationInMins > 0)
			{
				var bizOsInCachedFactory = GetBizOsFromCacheFactoryWithQueryAsCacheKey<T>(fullQuery, cacheDurationInMins);

				var bizOList = new List<T>(bizOsInCachedFactory.AllItems.Length);
				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				foreach (var bizOInCachedFactory in bizOsInCachedFactory.AllItems)
				{
					var pkQuery = new ZQuery(schemaResolver.GetPkColumn(bizOInCachedFactory.TableName), bizOInCachedFactory.PK)
					{
						FetchOnlyFromLocalCache = true
					};

					var bizO = factory.LoadTop1<T>(pkQuery) ?? (T)factory.ImportFromAnotherFactory(bizOInCachedFactory);
					bizOList.Add(bizO);
				}

				result = bizOList.ToArray();
			}
			else
			{
				result = factory.Load<T>(fullQuery);
			}

			return result;
		}

		T[] GetBestMatchesWithInMemoryFiltering<T>(BusinessObjectFactory factory, ZQuery query, ZString cacheKey, int cacheDurationInMins)
			where T : BusinessObject
		{
			T[] result;

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(T));
			dbOnlyQuery.AddToFilter(query);

			if (cacheDurationInMins > 0)
			{
				var bizOsInCachedFactory = FilterInMemory(GetBizOsFromCacheFactory<T>(cacheKey, dbOnlyQuery, cacheDurationInMins));

				var bizOList = new List<T>(bizOsInCachedFactory.Length);
				var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
				foreach (var bizOInCachedFactory in bizOsInCachedFactory)
				{
					var pkQuery = new ZQuery(schemaResolver.GetPkColumn(bizOInCachedFactory.TableName), bizOInCachedFactory.PK)
					{
						FetchOnlyFromLocalCache = true
					};

					var bizO = factory.LoadTop1<T>(pkQuery) ?? (T)factory.ImportFromAnotherFactory(bizOInCachedFactory);
					bizOList.Add(bizO);
				}

				result = bizOList.ToArray();
			}
			else
			{
				result = FilterInMemory(new ColumnValueRankerCache<T>(factory.Load<T>(dbOnlyQuery), 0));
			}

			return result;
		}

		public T GetBestMatch<T>(BusinessObjectFactory factory, ZQuery mainQuery) where T : BusinessObject
		{
			ZQuery fullQuery = GetFullQuery<T>(mainQuery);
			return factory.LoadTop1<T>(fullQuery);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetFullQuery<T>(ZQuery mainQuery)
		{
			ZQuery fullQuery = new ZDBOnlyQuery(typeof(T));
			fullQuery.AddToFilter(mainQuery);

			double score = Math.Pow(2, elementCount);
			StringBuilder caseStatement = new StringBuilder();
			var paramNo = 0;
			foreach (ColumnValuesPair columnValuesPair in list)
			{
				ZQuery query = new ZQuery();
				foreach (IZType value in columnValuesPair.Values)
				{
					if (caseStatement.Length > 0)
					{
						caseStatement.Append(" + ");
					}

					var isNull = (value is ZGuid && value.IsEmpty);
					paramNo++;
					var sneakyComparisonValue = (value is ZGuid && value.IsEmpty && columnValuesPair.Column.IsNullable) ? null : value;
					var conditionQuery = new ZQuery("@CWRanker" + paramNo.ToString(CultureInfo.InvariantCulture), columnValuesPair.Column, sneakyComparisonValue);
					// optimisation to avoid using ZQuery.LiteralTextSQL and parameter renaming when parameters are not needed
					var parameter = (ZSqlParameter)conditionQuery.FilterParts.First();
					query.AddToFilter(conditionQuery, JoinCondition.Or);

					var comparisonText = parameter.ComparisonOperator.ComparisonText(sneakyComparisonValue);
					var isNullComparison = comparisonText == EqualComparisonOperator.Is || comparisonText == NotEqualComparisonOperator.IsNot;
					var comparisonString = columnValuesPair.Column.Name + " " + comparisonText + " " + (isNullComparison ? "null" : parameter.ParameterName);
					caseStatement.Append((NoResString)"CASE WHEN " + comparisonString + (NoResString)" THEN " + score.ToString(CultureInfo.InvariantCulture) + (NoResString)" ELSE 0 END");
					score = score / 2;
				}
				fullQuery.AddToFilter(query, JoinCondition.And);
			}

			if (caseStatement.Length > 0)
			{
				caseStatement.Append(" DESC");
				fullQuery.OrderBy = caseStatement.ToString();
			}
			return fullQuery;
		}

		T[] FilterInMemory<T>(ColumnValueRankerCache<T> cache) where T : BusinessObject
		{
			if (list.Count == 0)
			{
				return cache.AllItems;
			}
			IEnumerable<T> enumerable;
			int count;
			var cacheIndex = 0;
			do
			{
				(enumerable, count) = cache.GetItems(list[cacheIndex].Column, list[cacheIndex].Values);
				cacheIndex++;
			} while (count > 100 && cacheIndex < list.Count);

			var orderDictionary = new Dictionary<ZGuid, double>();
			var result = enumerable.Where(t =>
			{
				var order = Math.Pow(2, elementCount);
				var itemOrder = 0.0;
				foreach (var columnValue in list)
				{
					if (columnValue.Values.Length == 0)
					{
						break;
					}

					var value = t[columnValue.ColumnName];
					var index = columnValue.Values.IndexOf(p => DatabaseLikeEqualityComparer.IsEqual(p, value));
					if (index == -1)
					{
						if (value is ZGuid guid && guid.IsEmpty)
						{
							index = Array.IndexOf(columnValue.Values, null);
						}

						if (index == -1)
						{
							return false;
						}
					}
					itemOrder += order / Math.Pow(2, index + 1);
					order = order / Math.Pow(2, columnValue.Values.Length);
				}
				orderDictionary[t.PK] = itemOrder;
				return true;
			});

			return result.OrderByDescending(t => orderDictionary[t.PK]).ToArray();
		}

		#region Caching

		static BusinessObjectFactory CacheFactory
		{
			get
			{
				if (cacheFactoryReference == null)
				{
					cacheFactoryReference = new WeakReference(null);
				}
				BusinessObjectFactory result = (BusinessObjectFactory)cacheFactoryReference.Target;
				if (result == null)
				{
					result = new BusinessObjectFactory();
					result.NameForDebugging = "Column Value Ranker Static Cache";
					result.RefreshEnabled = false;
					result.Saved += delegate
					{ throw new InvalidOperationException("Cache factory should not be saved"); };
					cacheFactoryReference.Target = result;
				}
				return result;
			}
		}

		[ThreadStatic]
		static WeakReference cacheFactoryReference;

		ColumnValueRankerCache<T> GetBizOsFromCacheFactoryWithQueryAsCacheKey<T>(ZQuery query, int cacheDurationInMins)
			where T : BusinessObject
		{
			var cacheKey = query.LiteralTextADO;
			return GetBizOsFromCacheFactory<T>(cacheKey, query, cacheDurationInMins);
		}

		ColumnValueRankerCache<T> GetBizOsFromCacheFactory<T>(ZString cacheKey, ZQuery query, int cacheDurationInMins)
			where T : BusinessObject
		{
			var cachedInstance = (ColumnValueRankerCache<T>)BestMatchesCache[cacheKey];

			if (cachedInstance == null || cachedInstance.HasTimedOut)
			{
				ZQuery fullQuery = new ZQuery(query);
				fullQuery.ReLoadExistingRows = true;
				var bizos = CacheFactory.Load<T>(fullQuery);
				var result = new ColumnValueRankerCache<T>(bizos, cacheDurationInMins);
				BestMatchesCache.Add(cacheKey, result);
				return result;
			}
			else
			{
				return cachedInstance;
			}
		}

		static LRUCache<string, object> BestMatchesCache
		{
			get { return bestMatchesCache ?? (bestMatchesCache = new LRUCache<string, object>()); }
		}

		[ThreadStatic]
		static LRUCache<string, object> bestMatchesCache;

#if DEBUG

		internal static BusinessObjectFactory CacheFactoryForTest
		{
			get { return CacheFactory; }
		}

		internal static IDisposable ResetCacheOnDisposedForTest()
		{
			return new DisposableAction(delegate
			{
				BestMatchesCache.Clear();
				if (cacheFactoryReference != null)
				{
					cacheFactoryReference.Target = null;
					cacheFactoryReference = null;
				}
			});
		}

#endif

		#endregion

		#endregion

		#region Get Best Match from a Collection

		public override IEnumerable<T> GetBestMatch<T>(IEnumerable<T> collection)
		{
			var ranks = (this as IColumnValueRankerInternals).ColumnValues;
			if (ranks == null || !ranks.Any())
			{
				return null;
			}

			var stack = new Stack<ColumnValuesPairBase>(ranks.Reverse());
			return stack.Any() ? GetBestMatchFromCollection(collection, stack) : null;
		}

		#endregion

		#region IColumnValueRankerInternals

		IEnumerable<ColumnValuesPair> IColumnValueRankerInternals.ColumnValues => list;

		#endregion

		public class ColumnValuesPair : ColumnValuesPairBase
		{
			public ColumnValuesPair(SchemaColumn column, object[] values)
				: base(values)
			{
				Column = column;
				ColumnName = column.Name;
			}

			public readonly SchemaColumn Column;
		}
	}
}
