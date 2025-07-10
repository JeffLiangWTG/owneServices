using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	public interface IValidationCache
	{
		IDictionary<object, object> CachedData { get; }
	}

	internal class ValidationCache : IValidationCache
	{
		const int MaxPropCacheSize = 1000;

		ZPropertyInfo topLevelInfo;
		internal BusinessObject topLevelBusinessObject;

		readonly HashSet<BusinessObject> validatedBusinessObjects;
		HashSet<ZPropertyInfo> validatedPropertyInfos;

		internal Queue<ZPropertyInfo> validatedPropertyInfosCacheQueue;
		HashSet<ZPropertyInfo> validatedPropertyInfosCache;

		internal ValidationCache()
		{
			validatedBusinessObjects = new HashSet<BusinessObject>();
			validatedPropertyInfos = new HashSet<ZPropertyInfo>();
			validatedPropertyInfosCacheQueue = new Queue<ZPropertyInfo>();
			validatedPropertyInfosCache = new HashSet<ZPropertyInfo>();
		}

		public void BeginValidation(BusinessObject bizO)
		{
			if (ReferenceEquals(bizO, null))
			{
				throw new ArgumentNullException(nameof(bizO));
			}

			if (ReferenceEquals(topLevelBusinessObject, null))
			{
				topLevelBusinessObject = bizO;
			}
		}

		public void BeginValidation(ZPropertyInfo info)
		{
			if (ReferenceEquals(info, null))
			{
				throw new ArgumentNullException(nameof(info));
			}

			if (validatedPropertyInfos.Contains(info))
			{
				throw new InvalidOperationException("You cannot begin validation on a ZPropertyInfo that is already being validated.");
			}

			if (ReferenceEquals(topLevelBusinessObject, null) && topLevelInfo == null)
			{
				topLevelInfo = info;
			}
		}

		public void EndValidation(BusinessObject bizO)
		{
			if (ReferenceEquals(bizO, null))
			{
				throw new ArgumentNullException(nameof(bizO));
			}

			validatedBusinessObjects.Add(bizO);
			validatedPropertyInfos = new HashSet<ZPropertyInfo>();

			// if we are back at out top level bizO we must clear the cache or subsequent updates on its properties will not be validated
			if (ReferenceEquals(topLevelBusinessObject, bizO))
			{
				topLevelBusinessObject = null;
				validatedPropertyInfosCacheQueue = new Queue<ZPropertyInfo>();
				validatedPropertyInfosCache = new HashSet<ZPropertyInfo>();
			}
		}

		public void EndValidation(ZPropertyInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			// if we are back at out top level info we must clear the cache or subsequent updates on this property will not be validated
			if (topLevelInfo == info)
			{
				topLevelInfo = null;
				validatedPropertyInfosCacheQueue = new Queue<ZPropertyInfo>();
				validatedPropertyInfosCache = new HashSet<ZPropertyInfo>();
			}

			validatedPropertyInfos.Remove(info);
		}

		public bool HasValidationBeenRun(ZPropertyInfo info)
		{
			return validatedBusinessObjects.Contains(info.BizObj)
				   || validatedPropertyInfos.Contains(info)
				   || validatedPropertyInfosCache.Contains(info);
		}

		public void SetHasValidationBeenRun(ZPropertyInfo info)
		{
			// make sure our cache size is no more than MaxPropCacheSize
			if (validatedPropertyInfosCacheQueue.Count == MaxPropCacheSize)
			{
				validatedPropertyInfosCache.Remove(validatedPropertyInfosCacheQueue.Dequeue());
			}

			// add info to our stack and cache
			validatedPropertyInfos.Add(info);
			validatedPropertyInfosCacheQueue.Enqueue(info);
			validatedPropertyInfosCache.Add(info);
		}

		#region IValidationCache

		IDictionary<object, object> IValidationCache.CachedData
		{
			get { return cachedData ?? (cachedData = new Dictionary<object, object>()); }
		}
		IDictionary<object, object> cachedData;

		internal void ResetCachedData()
		{
			cachedData = null;
		}

		#endregion
	}
}
