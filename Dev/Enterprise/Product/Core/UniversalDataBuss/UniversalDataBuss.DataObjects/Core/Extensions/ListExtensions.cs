using System.Collections.Generic;
using System.Linq;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class ListExtension
	{
		public static List<T> AddSafe<T>(this List<T> collection, T itemToAddIfNotNull)
		{
			if (itemToAddIfNotNull != null)
			{
				if (collection == null)
				{
					collection = new List<T>();
				}
				collection.Add(itemToAddIfNotNull);
			}

			return collection;
		}

		public static List<T> AddSafe<T>(this List<T> collection, IEnumerable<T> itemsToAddIfNotNull)
		{
			if (itemsToAddIfNotNull != null)
			{
				var items = itemsToAddIfNotNull.ToArray();
				if (items.Length > 0)
				{
					if (collection == null)
					{
						collection = new List<T>();
					}

					collection.AddRange(items);
				}
			}

			return collection;
		}
	}
}
