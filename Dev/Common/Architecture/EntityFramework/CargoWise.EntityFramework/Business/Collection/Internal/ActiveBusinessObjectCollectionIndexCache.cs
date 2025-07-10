using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	internal interface IActiveBusinessObjectCollectionIndexCache
	{
		IEnumerable<IActiveBusinessObjectCollectionIndex> All { get; }
	}

	internal class ActiveBusinessObjectCollectionIndexCache : IActiveBusinessObjectCollectionIndexCache, IService
	{
		protected ActiveBusinessObjectCollectionIndexCache()
		{
		}

		/// <summary>
		/// Get an instance of the cache for a factory. The instance's lifecycle is tied to the factory.
		/// </summary>
		public static ActiveBusinessObjectCollectionIndexCache GetInstance(BusinessObjectFactory factory)
		{
			var result = factory.ServiceContainer.GetService<ActiveBusinessObjectCollectionIndexCache>();
			if (result == null)
			{
				lock (cacheMutex)
				{
					result = factory.ServiceContainer.GetService<ActiveBusinessObjectCollectionIndexCache>();
					if (result == null)
					{
						result = new ActiveBusinessObjectCollectionIndexCache();
						factory.ServiceContainer.AddService(result);
					}
				}
			}
			return result;
		}

		static readonly object cacheMutex = new object();

		public IEnumerable<IActiveBusinessObjectCollectionIndex> All
		{
			get
			{
				foreach (IActiveBusinessObjectCollectionIndexCache cache in caches)
				{
					foreach (IActiveBusinessObjectCollectionIndex index in cache.All)
					{
						yield return index;
					}
				}
			}
		}

		internal void AddCache(IActiveBusinessObjectCollectionIndexCache cache)
		{
			caches.Add(cache);
		}

		readonly List<IActiveBusinessObjectCollectionIndexCache> caches = new List<IActiveBusinessObjectCollectionIndexCache>();
	}

	/// <summary>
	/// A cache of ActiveBusinessObjectCollectionIndex objects that are stored in a BusinessObjectFactory.
	/// An index is removed from the cached once it is no longer referenced by any ActiveBusinessObjectCollection
	/// objects. An index is roughly cached by filter, sort and relationship.
	/// </summary>
	internal class ActiveBusinessObjectCollectionIndexCache<T> : IActiveBusinessObjectCollectionIndexCache, IService where T : BusinessObject
	{
		protected ActiveBusinessObjectCollectionIndexCache(BusinessObjectFactory factory)
		{
			this.factory = factory;
			ActiveBusinessObjectCollectionIndexCache.GetInstance(factory).AddCache(this);
		}

		/// <summary>
		/// Get an instance of the cache for a factory. The instance's lifecycle is tied to the factory.
		/// </summary>
		public static ActiveBusinessObjectCollectionIndexCache<T> GetInstance(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollectionIndexCache");
			ActiveBusinessObjectCollectionIndexCache<T> result = factory.ServiceContainer.GetService<ActiveBusinessObjectCollectionIndexCache<T>>();
			if (result == null)
			{
				result = new ActiveBusinessObjectCollectionIndexCache<T>(factory);
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		/// <summary>
		/// Get an index for a particular collection type, relationship, additional filter and sort.
		/// </summary>
		public ActiveBusinessObjectCollectionIndex<T> GetIndex(Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery additionalFilter, IComparer comparer, object[] collectionState)
		{
			CheckComparerOverridesEquals(comparer);
			ActiveBusinessObjectCollectionIndex<T> foundIndex;
			CacheKey newIndexKey = new CacheKey(collectionType, elementType, relationship, additionalFilter, comparer, collectionState);

			if (!TryGetFromCache(newIndexKey, out foundIndex))
			{
				if (relationship is AdhocCollectionRelationship)
				{
					foundIndex = new ActiveBusinessObjectCollectionIndexAdhoc<T>(factory, collectionType, elementType, relationship, additionalFilter, comparer, collectionState, newIndexKey);
				}
				else //Implement conditions for ABOCIDataTable
				{
					foundIndex = new ActiveBusinessObjectCollectionIndexDataView<T>(factory, collectionType, elementType, relationship, additionalFilter, comparer, collectionState, newIndexKey);
				}

				foundIndex.Disposed += IndexDisposedHandler;
				cache[newIndexKey] = foundIndex;
			}
			return foundIndex;
		}

		bool TryGetFromCache(CacheKey newIndexKey, out ActiveBusinessObjectCollectionIndex<T> foundIndex)
		{
			if (!cache.TryGetValue(newIndexKey, out foundIndex))
			{
				return false;
			}

			if (foundIndex.IsDisposed)
			{
				ErrorReporter.ReportOnce("DisposedActiveCollectionIndexInCache", string.Format(CultureInfo.InvariantCulture, "Disposed ABOCI<{0}> has been found in indexes cache.", typeof(T).Name));
				Remove(foundIndex);

				foundIndex = null;
				return false;
			}

			var foundIndexKey = new CacheKey(foundIndex);
			if (foundIndexKey.Equals(newIndexKey))
			{
				// Found index has same parameters as requested.
				return true;
			}

			if (!cache.ContainsKey(foundIndexKey))
			{
				// Place for correct key is vacant - move existing index there and update its key.
				foundIndex.cacheKey = foundIndexKey;
				cache[foundIndexKey] = foundIndex;
			}
			else
			{
				// There already is other index with same key in that place - just remove this index.
				Remove(foundIndex);
			}

			return false;
		}

		public IEnumerable<ActiveBusinessObjectCollectionIndex<T>> All
		{
			get { return cache.Values; }
		}

		IEnumerable<IActiveBusinessObjectCollectionIndex> IActiveBusinessObjectCollectionIndexCache.All
		{
			get
			{
				foreach (IActiveBusinessObjectCollectionIndex item in All)
				{
					yield return item;
				}
			}
		}

		#region CacheKey

		internal class CacheKey
		{
			public CacheKey(ActiveBusinessObjectCollectionIndex<T> index)
				: this(index.CollectionType, index.ElementType, index.Relationship, index.AdditionalFilter, index.SortComparer, index.CollectionState)
			{
			}

			public CacheKey(Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery additionalFilter, IComparer comparer, object[] collectionState)
			{
				this.collectionType = collectionType;
				this.elementType = elementType;
				this.relationship = relationship;
				this.additionalFilter = additionalFilter;
				this.comparer = comparer;
				this.collectionState = collectionState;
				additionalFilter.ModificationsEnabled = false;
			}

			#region Equals / GetHashCode

			public override bool Equals(object obj)
			{
				CacheKey rhs = obj as CacheKey;
				bool result = rhs != null
							&& collectionType == rhs.collectionType
							&& elementType == rhs.elementType
							&& object.Equals(relationship, rhs.relationship)
							&& object.Equals(additionalFilter, rhs.additionalFilter)
							&& object.Equals(comparer, rhs.comparer)
							&& ArrayEquals(collectionState, rhs.collectionState);
				return result;
			}

			bool ArrayEquals(object[] lhs, object[] rhs)
			{
				bool result = (lhs == null && rhs == null) || (lhs != null && rhs != null && lhs.Length == rhs.Length);
				if (result && lhs != null)
				{
					result = true;
					for (int i = 0; i < lhs.Length; i++)
					{
						if (!object.Equals(lhs[i], rhs[i]))
						{
							result = false;
							break;
						}
					}
				}
				return result;
			}

			public override int GetHashCode()
			{
				if (hashCode == -1)
				{
					hashCode =
						collectionType.GetHashCode() ^
						elementType.GetHashCode() ^
						relationship.GetHashCode() ^
						additionalFilter.GetHashCode() ^
						(comparer == null ? 0 : comparer.GetHashCode());
				}
				return hashCode;
			}
			int hashCode = -1;

			#endregion

			#region Implementation

			readonly Type collectionType;
			readonly Type elementType;
			readonly ICollectionRelationship relationship;
			readonly ZQuery additionalFilter;
			readonly IComparer comparer;
			readonly object[] collectionState;

			#endregion
		}

		#endregion

		#region Implementation

		readonly BusinessObjectFactory factory;
		readonly Dictionary<CacheKey, ActiveBusinessObjectCollectionIndex<T>> cache = new Dictionary<CacheKey, ActiveBusinessObjectCollectionIndex<T>>();

#if DEBUG
		internal Dictionary<CacheKey, ActiveBusinessObjectCollectionIndex<T>> CacheForTest => cache;
#endif

		void CheckComparerOverridesEquals(IComparer obj)
		{
			if (obj != null)
			{
				MethodInfo method = obj.GetType().GetMethod("Equals", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, new Type[1] { typeof(object) }, null);
				if (method.DeclaringType == typeof(object))
				{
					throw new ArgumentException("IComparer of type " + obj.GetType().FullName + " must override Equals / GetHashCode otherwise the fly weight collection index cannot be cached.");
				}
			}
		}

		void IndexDisposedHandler(object sender, EventArgs e)
		{
			Remove((ActiveBusinessObjectCollectionIndex<T>)sender);
		}

		void Remove(ActiveBusinessObjectCollectionIndex<T> index)
		{
			CacheKey key = index.cacheKey ?? new CacheKey(index);
			if (!cache.Remove(key))
			{
				throw new InvalidOperationException(FormattableString.Invariant(
$@"Collection index cannot be removed from the cache because it doesn't exist in the cache
Type: {typeof(T).FullName};
Factory instance: {index.Factory._Instance};
Additional filter: {index.AdditionalFilter.LiteralTextADO};
Collection type: {index.CollectionType.FullName};
Element type: {index.ElementType.FullName};"));
			}

			// Removing the delegate to prevent an exception if the object is disposed multiple times
			index.Disposed -= IndexDisposedHandler;
		}

		#endregion
	}
}
