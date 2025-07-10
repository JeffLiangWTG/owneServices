using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	public static class IDictionaryExtensions
	{
		public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keySelector, IComparer<TKey> comparer)
		{
			Argument.NotNull(values, nameof(values));
			Argument.NotNull(keySelector, nameof(keySelector));
			Argument.NotNull(comparer, nameof(comparer));
			var result = new SortedDictionary<TKey, TValue>(comparer);

			foreach (var value in values)
			{
				var key = keySelector(value);
				result.Add(key, value);
			}

			return result;
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueCreator)
		{
			Argument.NotNull(dictionary, nameof(dictionary)); // Suggested By ReviewBot 
			Argument.NotNull(key, nameof(key));
			Argument.NotNull(valueCreator, nameof(valueCreator));
			var concurrentDictionary = dictionary as ConcurrentDictionary<TKey, TValue>;
			TValue value;
			if (concurrentDictionary != null)
			{
				value = concurrentDictionary.GetOrAdd(key, k => valueCreator());
			}
			else
			{
				if (!dictionary.TryGetValue(key, out value))
				{
					value = valueCreator();
					dictionary.Add(key, value);
				}
			}
			return value;
		}

		public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
			where TValue : new()
		{
			Argument.NotNull(dictionary, nameof(dictionary)); // Suggested By ReviewBot 
			Argument.NotNull(key, nameof(key));
			return dictionary.GetOrAdd(key, () => new TValue());
		}

		public static TValue GetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			Argument.NotNull(dictionary, nameof(dictionary)); // Suggested By ReviewBot 
			Argument.NotNull(key, nameof(key));
			TValue value;
			if (dictionary.TryGetValue(key, out value))
			{
				return value;
			}
			else
			{
				return default(TValue);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I think nesting is better than having two parameters to Action")]
		public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other, Action<KeyValuePair<TKey, TValue>> handleKeyAlreadyPresent)
		{
			Argument.NotNull(dictionary, nameof(dictionary));
			Argument.NotNull(other, nameof(other));
			Argument.NotNull(handleKeyAlreadyPresent, nameof(handleKeyAlreadyPresent));

			foreach (var keyValuePair in other)
			{
				if (keyValuePair.Key != null)
				{
					if (dictionary.ContainsKey(keyValuePair.Key))
					{
						handleKeyAlreadyPresent(keyValuePair);
					}
					else
					{
						dictionary.Add(keyValuePair);
					}
				}
			}
		}

		/// <summary>
		/// It's System.Linq.ToLookup, except that it returns IDictionary instead, since that is more useful sometimes.
		/// </summary>
		public static IDictionary<TKey, IList<T>> ToDictionaryList<TKey, T>(this IEnumerable<T> enumerable, Func<T, TKey> getKey, IEqualityComparer<TKey> comparer = null)
		{
			Argument.NotNull(enumerable, nameof(enumerable));
			Argument.NotNull(getKey, nameof(getKey));

			var result = new Dictionary<TKey, IList<T>>(comparer ?? EqualityComparer<TKey>.Default);
			foreach (var val in enumerable)
			{
				var key = getKey(val);
				if (!result.TryGetValue(key, out IList<T> list))
				{
					result[key] = list = new List<T>();
				}
				list.Add(val);
			}
			return result;
		}
	}
}
