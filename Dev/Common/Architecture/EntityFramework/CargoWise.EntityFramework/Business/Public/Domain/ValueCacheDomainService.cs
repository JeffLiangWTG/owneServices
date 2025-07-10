using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	class ValueCacheDomainService : IService
	{
		protected ValueCacheDomainService()
		{
		}

		public static ValueCacheDomainService GetIfCreated(BusinessObjectFactory factory)
		{
			return factory.ServiceContainer.GetService<ValueCacheDomainService>();
		}

		public static ValueCacheDomainService Get(BusinessObjectFactory factory)
		{
			var result = GetIfCreated(factory);

			if (result == null)
			{
				result = new ValueCacheDomainService();
				factory.ServiceContainer.AddService(result);
			}

			return result;
		}

		public T GetCachedValue<T>(object key, GetValueDelegate<T> getValueDelegate, CacheStalenessPolicy stalePolicy)
		{
			T result;

			if (!TryGetValueFromCacheOnly(key, out result))
			{
				var innerDictionary = outerDictionary.GetOrAdd(typeof(T), new ConcurrentDictionary<object, object>());

				if (stalePolicy == CacheStalenessPolicy.StaleOnFactorySave)
				{
					keysToGoStaleOnFactorySave.Add((typeof(T), key));
				}
				else if (stalePolicy == CacheStalenessPolicy.StaleBeforeFactorySavingTransaction)
				{
					keysToGoStaleBeforeFactorySaving.Add((typeof(T), key));
				}
				else if (stalePolicy.IsStaleWhenDataTableChanges())
				{
					if (tablesSubscribedToRowChanges.Add(stalePolicy.TableName))
					{
						var table = stalePolicy.Factory.RowFactory.GetTable(stalePolicy.TableName);

						if (table != null)
						{
							table.RowChanged += (s, e) => ClearAfterTableRowChanged(stalePolicy.TableName);
							table.RowDeleted += (s, e) => ClearAfterTableRowChanged(stalePolicy.TableName);

							cacheKeysByTable[stalePolicy.TableName] = new HashSet<(Type, object)>();
						}
					}
				}

				result = getValueDelegate();
				var manager = result as ICachedValueManager;
				if (manager != null)
				{
					manager.IsCacheEnabled = true;
				}
				innerDictionary[key] = result;

				if (stalePolicy.IsStaleWhenDataTableChanges())
				{
					if (cacheKeysByTable.TryGetValue(stalePolicy.TableName, out var keys))
					{
						keys.Add((typeof(T), key));
					}
				}
			}

			return result;
		}

		public bool TryGetValueFromCacheOnly<T>(object key, out T value)
		{
			bool isCached = false;
			object objValue = null;

			if (outerDictionary.TryGetValue(typeof(T), out var innerDictionary))
			{
				isCached = innerDictionary.TryGetValue(key, out objValue);
			}

			value = (T)(objValue ?? default(T));
			return isCached;
		}

		/// <summary>
		/// Clears the cache for the given key and for the given type.
		/// Please make sure you are clearing your key for the correct type. The same
		/// type as when your cache is filled. If it is different then clearing the
		/// cache for your key will have no effect.
		/// </summary>
		public void ClearCachedValue<T>(object key)
		{
			if (outerDictionary.TryGetValue(typeof(T), out var innerDictionary))
			{
				innerDictionary.TryRemove(key, out var _);
			}
		}

		internal void ClearBeforeFactorySaving()
		{
			ClearCache(keysToGoStaleBeforeFactorySaving, outerDictionary);
		}

		internal void ClearAfterFactorySave()
		{
			ClearCache(keysToGoStaleOnFactorySave, outerDictionary);
		}

		void ClearAfterTableRowChanged(string tableName)
		{
			if (cacheKeysByTable.TryGetValue(tableName, out var keys))
			{
				ClearCache(keys, outerDictionary);
			}
		}

		static void ClearCache(HashSet<(Type, object)> keysToClear, ConcurrentDictionary<Type, ConcurrentDictionary<object, object>> cache)
		{
			foreach (var key in keysToClear.ToArray())
			{
				if (cache.TryGetValue(key.Item1, out var dictionary))
				{
					dictionary.TryRemove(key.Item2, out _);
				}
			}

			keysToClear.Clear();
		}

		readonly HashSet<(Type, object)> keysToGoStaleOnFactorySave = new HashSet<(Type, object)>();
		readonly HashSet<(Type, object)> keysToGoStaleBeforeFactorySaving = new HashSet<(Type, object)>();

		readonly HashSet<string> tablesSubscribedToRowChanges = new HashSet<string>();
		readonly Dictionary<string, HashSet<(Type, object)>> cacheKeysByTable = new Dictionary<string, HashSet<(Type, object)>>();

		readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, object>> outerDictionary = new ConcurrentDictionary<Type, ConcurrentDictionary<object, object>>();
	}
}
