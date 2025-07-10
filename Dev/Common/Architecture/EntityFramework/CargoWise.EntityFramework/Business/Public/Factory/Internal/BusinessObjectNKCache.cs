using System;
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	internal class BusinessObjectNKCache
	{
		#region Add Bizo to Cache

		internal void Add(BusinessObject bizo, SchemaColumn column, IZType value)
		{
			if (bizo != null
				&& column != null
				&& value != null
				&& !Equals(value, ZString.Empty))
			{
				Dictionary<IZType, BusinessObject> bizoCache;
				if (columnCache.TryGetValue(column, out bizoCache))
				{
					BusinessObject existingBizo;
					if (!bizoCache.TryGetValue(value, out existingBizo))
					{
						bizoCache.Add(value, bizo);
					}
				}
				else
				{
					bizoCache = new Dictionary<IZType, BusinessObject>();
					bizoCache.Add(value, bizo);

					columnCache.Add(column, bizoCache);
				}
			}
		}

		#endregion

		#region Fetch Bizo from Cache

		internal BusinessObject Fetch(Type bizoType, SchemaColumn column, IZType value)
		{
			BusinessObject result = null;

			if (bizoType != null && column != null && value != null)
			{
				Dictionary<IZType, BusinessObject> bizoCache;

				if (columnCache.TryGetValue(column, out bizoCache))
				{
					if (bizoCache.TryGetValue(value, out result))
					{
						if (result.GetType() != bizoType)
						{
							result = null;
						}
						if (result != null && !result[column].Equals(value))
						{
							result = null;
							bizoCache.Remove(value);
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Remove Bizo from Cache

		internal void Remove(BusinessObject bizo)
		{
			foreach (var pair in columnCache)
			{
				IZType key = null;
				if (bizo.Table == null)
				{
					key = GetKey(pair.Value, bizo);
				}
				else if (bizo.Table.Columns.Contains(pair.Key.Name))
				{
					key = GetKey(pair.Value, pair.Key, bizo);
					if (key == null && bizo.IsDeleted) // May be cached with other value
					{
						key = GetKey(pair.Value, bizo);
					}
				}

				if (key != null)
				{
					pair.Value.Remove(key);
					break;
				}
			}
		}

		#endregion

		#region Update Natural Key Cache

		internal void UpdateNaturalKeyCache(BusinessObject bizo, SchemaColumn column, IZType oldKeyValue, IZType newKeyValue)
		{
			if (bizo != null && column != null)
			{
				Dictionary<IZType, BusinessObject> bizoCache;
				if (columnCache.TryGetValue(column, out bizoCache))
				{
					BusinessObject result;
					if (bizoCache.TryGetValue(oldKeyValue, out result))
					{
						if (result.GetType() == bizo.GetType())
						{
							bizoCache.Remove(oldKeyValue);
						}
					}
					if (!Equals(newKeyValue, ZString.Empty) && !bizoCache.ContainsKey(newKeyValue))
					{
						bizoCache.Add(newKeyValue, bizo);
					}
				}
			}
		}

		#endregion

		#region BusinessObject Count

#if DEBUG
		internal int BusinessObjectCount
		{
			get
			{
				int result = 0;

				foreach (var cache in columnCache.Values)
				{
					result += cache.Count;
				}

				return result;
			}
		}
#endif

		#endregion

		#region Implementation

		IZType GetKey(Dictionary<IZType, BusinessObject> cache, BusinessObject bizo)
		{
			foreach (KeyValuePair<IZType, BusinessObject> pair in cache)
			{
				if (pair.Value == bizo)
				{
					return pair.Key;
				}
			}

			return null;
		}

		IZType GetKey(Dictionary<IZType, BusinessObject> cache, SchemaColumn column, BusinessObject bizo)
		{
			var columnValue = bizo[column] as IZType;
			if (columnValue != null)
			{
				BusinessObject cachedValue;
				if (cache.TryGetValue(columnValue, out cachedValue) && object.ReferenceEquals(cachedValue, bizo))
				{
					return columnValue;
				}
			}

			return null;
		}

		readonly Dictionary<SchemaColumn, Dictionary<IZType, BusinessObject>> columnCache = new Dictionary<SchemaColumn, Dictionary<IZType, BusinessObject>>();

		#endregion
	}
}
