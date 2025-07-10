using System.Collections;
using System.Collections.Generic;

namespace CargoWise.Common
{
	/// <summary>
	/// This is just a flyweight wrapper to restrict write access to dictionaries.
	/// </summary>
	public class ReadOnlyDictionaryWrapper<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
	{
		public delegate IDictionary<TKey, TValue> GetDictionary();

		public ReadOnlyDictionaryWrapper(GetDictionary accessor)
		{
			Argument.NotNull(accessor, nameof(accessor));

			this.accessor = accessor;
		}
		readonly GetDictionary accessor;

		IDictionary<TKey, TValue> Inner => accessor();

		public TValue this[TKey key] => Inner[key];

		public int Count => Inner.Count;
		public IEnumerable<TKey> Keys => Inner.Keys;
		public IEnumerable<TValue> Values => Inner.Values;

		public bool ContainsKey(TKey key) => Inner.ContainsKey(key);
		public bool TryGetValue(TKey key, out TValue value) => Inner.TryGetValue(key, out value);
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Inner.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Inner.GetEnumerator();
	}
}
