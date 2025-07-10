using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common.Collections;

namespace CargoWise.EntityFramework
{
	public static class PropertyInfoFetcher
	{
		public static PropertyInfo GetFromLowestSubclass(Type type, string propertyName)
		{
			PropertyInfo result = GetFromLowestSubclassCache(type, propertyName);
			if (result == null)
			{
				foreach (PropertyInfo property in type.GetProperties())
				{
					if (property.Name == propertyName)
					{
						// if the last collection the indexer found is more base than this current indexer, use this current indexer instead
						if (result == null ||
							property.DeclaringType.IsSubclassOf(result.DeclaringType))
						{
							result = property;
						}
					}
				}
				if (result != null)
				{
					IDictionary<string, PropertyInfo> innerDictionary = CachedPropertyInfos[type];
					if (innerDictionary == null)
					{
						innerDictionary = new Dictionary<string, PropertyInfo>();
						CachedPropertyInfos.Add(type, innerDictionary);
					}
					innerDictionary[propertyName] = result;
				}
			}
			return result;
		}

		internal static PropertyInfo GetFromLowestSubclassCache(Type type, string propertyName)
		{
			PropertyInfo result = null;
			IDictionary<string, PropertyInfo> innerDictionary = CachedPropertyInfos[type];
			if (innerDictionary != null)
			{
				innerDictionary.TryGetValue(propertyName, out result);
			}
			return result;
		}

		#region Implementation

		static LRUCache<Type, IDictionary<string, PropertyInfo>> CachedPropertyInfos
		{
			get { return cachedPropertyInfos ?? (cachedPropertyInfos = new LRUCache<Type, IDictionary<string, PropertyInfo>>()); }
		}
		[ThreadStatic]
		static LRUCache<Type, IDictionary<string, PropertyInfo>> cachedPropertyInfos;

		#endregion

		public static void RemoveAll()
		{
			CachedPropertyInfos.Clear();
		}

		public static void RemoveType(Type typeToRemove)
		{
			CachedPropertyInfos.Remove(typeToRemove);
		}
	}
}
