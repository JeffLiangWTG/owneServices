using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Common
{
	public static class DataHelper
	{
		public static string[] NonUnique(this CodeDescriptionPairList codeDescriptionPairList)
		{
			return codeDescriptionPairList != null ?
				codeDescriptionPairList.OfType<ICustomsNumberTypeCodeDescription>().Where((numType) => !numType.IsUnique).Select((numType) => numType.Code).ToArray() : Array.Empty<string>();
		}

		/// <summary>
		/// This method will always return 1 where the decimal value is between 0 and 1 + critival value
		/// </summary>
		public static ZDecimal RoundDownValueIfLessThanCriticalValueRoundUpOtherwise(this ZDecimal originalValue, decimal criticalValue = 0.50M)
		{
			decimal result = 0M;

			if (originalValue > 0 && originalValue < 1)
			{
				result = 1M;
			}
			else
			{
				result = originalValue.RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(criticalValue);
			}

			return result;
		}

		/// <summary>
		/// This method will round all the way to zero if necessary
		/// </summary>
		public static ZDecimal RoundDownValueIncludingToZeroIfLessThanCriticalValueRoundUpOtherwise(this ZDecimal originalValue, decimal criticalValue = 0.51M)
		{
			decimal result = 0M;

			if (originalValue % 1 < criticalValue)
			{
				result = (decimal)(Math.Floor((double)originalValue));
			}
			else
			{
				result = (decimal)(Math.Ceiling((double)originalValue));
			}

			return result;
		}

		public static string GetChildLockByInfo(this BusinessObjectFactory factory, MutexID mutexID, ZGuid parentPK, ZString childKey)
		{
			var result = ZString.Empty;
			if (factory != null && parentPK.IsValid)
			{
				var mutex = factory.GetMutex(mutexID, parentPK, childKey);
				result = mutex.GetMutexLockByInfo();
			}

			return result;
		}

		public static bool IsChildLocked(this BusinessObjectFactory factory, MutexID mutexID, ZGuid parentPK, ZString childKey)
		{
			var result = false;
			if (factory != null && parentPK.IsValid)
			{
				var mutex = factory.GetMutex(mutexID, parentPK, childKey);
				result = mutex.IsLocked;
			}

			return result;
		}

		public static bool LockChild(this BusinessObjectFactory factory, MutexID mutexID, ZGuid parentPK, ZString childKey)
		{
			var result = false;
			if (factory != null && parentPK.IsValid)
			{
				var mutex = factory.GetMutex(mutexID, parentPK, childKey);
				result = mutex.Lock();
			}

			return result;
		}

		public static void UnlockChild(this BusinessObjectFactory factory, MutexID mutexID, ZGuid parentPK, ZString childKey)
		{
			if (parentPK.IsValid && factory != null)
			{
				if (factory.ParentDictionary().TryGetValue(parentPK, out var mutexIdDictionary)
					&& mutexIdDictionary.TryGetValue(mutexID, out var childMutexDictionary)
					&& childMutexDictionary.TryGetValue(childKey, out var mutex))
				{
					UnlockAndDispose(mutex);
					childMutexDictionary.Remove(childKey);
				}
			}
		}

		public static void UnlockAllChildrenOnParent(this BusinessObjectFactory factory, MutexID mutexID, ZGuid parentPK)
		{
			if (parentPK.IsValid && factory != null)
			{
				var parentDictionary = factory.ParentDictionary();
				if (parentDictionary.TryGetValue(parentPK, out var mutexIdDictionary) && mutexIdDictionary.TryGetValue(mutexID, out var childMutexDictionary))
				{
					foreach (var childMutex in childMutexDictionary)
					{
						UnlockAndDispose(childMutex.Value);
					}

					childMutexDictionary.Clear();
				}
			}
		}

		public static ZQuery GenerateClusterKeyQuery(ZInt clusterKey, ZGuid foreignKey, SchemaIntColumn clusterKeyColumn, SchemaGuidColumn foreignKeyColumn, bool fetchOnlyFromLocalCache)
		{
			var query = new ZQuery(clusterKeyColumn, clusterKey);
			query.AddToFilter(foreignKeyColumn, foreignKey);
			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;
			return query;
		}

		public static void AddFetchHintWithClusterKey(this BusinessObjectFactory factory, ZInt clusterKey, ZGuid foreignKey, Type businessObjectType, SchemaIntColumn clusterKeyColumn, SchemaGuidColumn foreignKeyColumn, bool fetchOnlyFromLocalCache)
		{
			factory.AddFetchHint(businessObjectType, GenerateClusterKeyQuery(clusterKey, foreignKey, clusterKeyColumn, foreignKeyColumn, fetchOnlyFromLocalCache));
		}

		static void UnlockAndDispose(ZGlobalMutex mutex)
		{
			if (mutex.IsLocked && mutex.HasLock)
			{
				mutex.Unlock();
			}

			((IDisposable)mutex).Dispose();
		}

		static ZGlobalMutex GetMutex(this BusinessObjectFactory factory, MutexID mutexID, ZGuid parentPK, ZString childKey)
		{
			var parentDictionary = factory.ParentDictionary();
			if (!parentDictionary.TryGetValue(parentPK, out var mutexIdDictionary))
			{
				mutexIdDictionary = new Dictionary<MutexID, Dictionary<ZString, ZGlobalMutex>>();
				parentDictionary.Add(parentPK, mutexIdDictionary);
			}

			if (!mutexIdDictionary.TryGetValue(mutexID, out var childMutexDictionary))
			{
				childMutexDictionary = new Dictionary<ZString, ZGlobalMutex>();
				mutexIdDictionary.Add(mutexID, childMutexDictionary);
			}

			if (!childMutexDictionary.TryGetValue(childKey, out var mutex))
			{
				mutex = new ZGlobalMutex(mutexID, parentPK + childKey);
				childMutexDictionary.Add(childKey, mutex);
			}

			return mutex;
		}

		static Dictionary<ZGuid, Dictionary<MutexID, Dictionary<ZString, ZGlobalMutex>>> ParentDictionary(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("40D0B139-7FF8-45D1-9DF6-5E8EC0A0286F", () => new Dictionary<ZGuid, Dictionary<MutexID, Dictionary<ZString, ZGlobalMutex>>>());
		}
	}
}
