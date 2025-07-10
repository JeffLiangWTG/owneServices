using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	class ColumnValueRankerCache<T> where T : BusinessObject
	{
		const int MinCacheSizeBeforeIndexing = 32;

		public ColumnValueRankerCache(T[] bizos, int cacheDurationInMins)
		{
			this.AllItems = bizos;
			this.expiry = ZDateTime.Now.AddMinutes(cacheDurationInMins);
			this.cacheDurationInMins = cacheDurationInMins;
		}

		public bool HasTimedOut
		{
			get { return expiry < ZDateTime.Now; }
		}

		public readonly int cacheDurationInMins;
		public readonly ZDateTime expiry;

		public T[] AllItems { get; }
		readonly Dictionary<string, IDictionary<object, IList<T>>> columnValueLookup = new Dictionary<string, IDictionary<object, IList<T>>>(StringComparer.OrdinalIgnoreCase);

		internal (IEnumerable<T>, int count) GetItems(SchemaColumn column, object[] values)
		{
			if (AllItems.Length < MinCacheSizeBeforeIndexing || values.Length == 0)
			{
				return (AllItems, AllItems.Length);
			}

			if (!columnValueLookup.TryGetValue(column.Name, out var valuesLookup))
			{
				columnValueLookup[column.Name] = valuesLookup = AllItems.ToDictionaryList(d => d[column], new DatabaseLikeEqualityComparer());
			}

			IEnumerable<T> result = Enumerable.Empty<T>();
			var count = 0;
			foreach (var val in values.Select(v => v ?? ZGuid.Empty).Distinct()) // The only nullable key is ZGuid.Empty
			{
				if (valuesLookup.TryGetValue(val, out var coll))
				{
					result = result.Concat(coll);
					count += coll.Count;
				}
			}
			return (result, count);
		}
	}

	sealed class DatabaseLikeEqualityComparer : IEqualityComparer<object>
	{
		static bool TryGetString(object x, out string str)
		{
			if (x is string s)
			{
				str = s;
				return true;
			}
			else if (x is ZString zs)
			{
				str = zs;
				return true;
			}
			str = null;
			return false;
		}

		public new bool Equals(object x, object y) => IsEqual(x, y);

		public static bool IsEqual(object x, object y)
		{
			if (TryGetString(x, out var str1) && TryGetString(y, out var str2))
			{
				return StringComparer.OrdinalIgnoreCase.Equals(str1, str2);
			}
			else
			{
				return EqualityComparer<object>.Default.Equals(x, y);
			}
		}

		public int GetHashCode(object obj)
		{
			if (TryGetString(obj, out var str))
			{
				return StringComparer.OrdinalIgnoreCase.GetHashCode(str);
			}
			else
			{
				return EqualityComparer<object>.Default.GetHashCode(obj);
			}
		}
	}
}
