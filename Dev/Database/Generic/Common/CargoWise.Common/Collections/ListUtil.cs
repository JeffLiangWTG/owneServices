using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CargoWise.Common.Testing;

namespace CargoWise.Common.Collections
{
	/// <summary>
	/// Utility methods for lists and collections.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Util")]
	public static class ListUtil
	{
		/// <summary>
		/// Get the type of element within the list provided
		/// </summary>
		/// <param name="list"></param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Contracts", "Nonnull-123-0")] //Suppressing the message here as unable to prove that itemProperties != null
		public static Type GetListElementType(IList list)
		{
			if (!((list as ITypedList) != null || list != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(list));
			}

			Type result = typeof(object);
			ITypedList typedList = list as ITypedList;
			if (typedList != null)
			{
				PropertyDescriptorCollection itemProperties = typedList.GetItemProperties(null);
				if (itemProperties != null)
				{
					foreach (PropertyDescriptor property in itemProperties)
					{
						if (result.IsAssignableFrom(property.ComponentType))
						{
							result = property.ComponentType;
						}
					}
				}
			}
			else if (list.Count > 0)
			{
				return list[0].GetType();
			}
			else
			{
				PropertyInfo property = list.GetType().GetProperty("Item", new Type[] { typeof(int) });
				if (property != null)
				{
					result = property.PropertyType;
				}
			}
			return result;
		}

		/// <summary>
		/// Get the type of a list's indexer, if any. The indexer must take a single argument
		/// of type int.
		/// </summary>
		public static Type GetListElementType(Type listType)
		{
			Argument.NotNull(listType, nameof(listType)); // Suggested By ReviewBot 
			if (listType.IsArray)
			{
				return listType.GetElementType();
			}
			else
			{
				if (!listElementTypeCache.TryGetValue(listType, out Tuple<Type> r))
				{
					var property = listType.GetProperty("Item", new Type[] { typeof(int) });
					Type result = null;
					if (property != null)
					{
						result = property.PropertyType;
					}
					else if (listType.IsInterface)
					{
						var inheritedIndexers = listType.GetInterfaces().Where(t => t != typeof(IList)).Select(x => x.GetProperty("Item", new Type[] { typeof(int) }))
							.WhereNotNull().Select(y => y.PropertyType).Distinct().Take(2).ToArray();
						if (inheritedIndexers.Length == 1)
						{
							result = inheritedIndexers[0];
						}
					}
					listElementTypeCache.Add(listType, Tuple.Create(result));
					return result;
				}
				return r.Item1;
			}
		}

		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, Tuple<Type>> listElementTypeCache = new LRUCache<Type, Tuple<Type>>();
	}
}
