using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Internal.Indexing
{
	static class RegistryIndexingHelper
	{
		public static string[] GetCategorySegments(this IRegistryItem item, int categoryIndex)
		{
			var category = item.Categories[categoryIndex];

			var categoryUntranslated = item.CategoriesUntranslated[categoryIndex];
			var unescapedSlashCount = categoryUntranslated.Select((character, index) => (character, index))
				.Count(n => n.character == '/' && (n.index == 0 || categoryUntranslated[n.index - 1] != '\\'));

			return SplitCategory(category, unescapedSlashCount).ToArray();
		}

		public static string[] GetUntranslatedCategorySegments(this IRegistryItem item, int categoryIndex)
		{
			return SplitCategory(item.CategoriesUntranslated[categoryIndex], 100).ToArray();
		}

		static IEnumerable<string> SplitCategory(string category, int maxSegments)
		{
			var start = 0;
			for (var i = 0; i < category.Length; i++)
			{
				if (maxSegments < 1)
				{
					break;
				}

				if (category[i] != '/' || (i != 0 && category[i - 1] == '\\'))
				{
					continue;
				}

				maxSegments--;
				yield return category.Substring(start, i - start).Replace("\\/", "/");
				start = i + 1;
			}

			if (start < category.Length)
			{
				yield return category.Substring(start).Replace("\\/", "/");
			}
		}

		public static IEnumerable<PropertyInfo> GetStaticItemPropertyInfos(this RegistryItemSet set)
		{
			var type = set.GetType();
			return type.GetProperties(PropertyBindingFlags)
				.Where(p => typeof(IRegistryItem).IsAssignableFrom(p.PropertyType) && !IsExplicitInterfaceImpl(p));
		}

		static bool IsExplicitInterfaceImpl(PropertyInfo property)
		{
			return property.Name.Contains(".");
		}

		internal static bool TryGetItemByPropertyName(this RegistryItemSet set, string propertyName, out IRegistryItem registryItem)
		{
			registryItem = null;
			var result = false;
			var property = set.GetType().GetProperty(propertyName, PropertyBindingFlags);
			if (property != null)
			{
				result = true;
				registryItem = (IRegistryItem)property.GetValue(set, null);
			}
			return result;
		}

		public static IRegistryItem GetItemByPropertyName(this RegistryItemSet set, string propertyName)
		{
			if (!set.TryGetItemByPropertyName(propertyName, out var registryItem))
			{
				throw new KeyNotFoundException($"Property not found '{propertyName}' on type '{set.GetType().FullName}'");
			}
			return registryItem;
		}

		public static IEnumerable<RegistryCategoryRef> CombineAndSort(IEnumerable<RegistryCategory> categoriesA, IEnumerable<RegistryCategory> categoriesB, IReadOnlyDictionary<Type, RegistryItemSet> sets)
		{
			return CombineAndSort(categoriesA?.Select(c => (c.GetText(sets), c)), categoriesB?.Select(c => (c.GetText(sets), c)), RegistryCategory.Combine)
				.Select(c => new RegistryCategoryRef(c.item, c.text));
		}

		public static IEnumerable<IRegistryItem> CombineAndSort(IEnumerable<IRegistryItem> itemsA, IEnumerable<IRegistryItem> itemsB)
		{
			return CombineAndSort(itemsA?.Select(i => (i.Caption, i)), itemsB?.Select(i => (i.Caption, i)), (a, b) => a)
				.Select(i => i.item);
		}

		static IEnumerable<(string text, T item)> CombineAndSort<T>(IEnumerable<(string text, T item)> itemsA, IEnumerable<(string text, T item)> itemsB, Func<T, T, T> combine)
		{
			if (itemsA == null && itemsB == null)
			{
				throw new InvalidOperationException("Cannot combine two null lists");
			}

			var sortedA = itemsA?.OrderBy(i => i.text, StringComparer.OrdinalIgnoreCase);
			var sortedB = itemsB?.OrderBy(i => i.text, StringComparer.OrdinalIgnoreCase);

			if (sortedA == null)
			{
				return sortedB;
			}

			if (sortedB == null)
			{
				return sortedA;
			}

			return CombineSorted(sortedA, sortedB, combine);
		}

		static IEnumerable<(string text, T item)> CombineSorted<T>(IEnumerable<(string text, T item)> sortedA, IEnumerable<(string text, T item)> sortedB, Func<T, T, T> combine)
		{
			using (var a = sortedA.GetEnumerator())
			using (var b = sortedB.GetEnumerator())
			{
				var aHasCurrent = a.MoveNext();
				var bHasCurrent = b.MoveNext();

				while (aHasCurrent || bHasCurrent)
				{
					var compare = aHasCurrent && bHasCurrent ? string.Compare(a.Current.text, b.Current.text, StringComparison.OrdinalIgnoreCase) : aHasCurrent ? -1 : 1;
					if (compare < 0)
					{
						yield return a.Current;
						aHasCurrent = a.MoveNext();
					}
					else if (compare > 0)
					{
						yield return b.Current;
						bHasCurrent = b.MoveNext();
					}
					else
					{
						yield return (a.Current.text, combine(a.Current.item, b.Current.item));
						aHasCurrent = a.MoveNext();
						bHasCurrent = b.MoveNext();
					}
				}
			}
		}

		const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
	}
}
