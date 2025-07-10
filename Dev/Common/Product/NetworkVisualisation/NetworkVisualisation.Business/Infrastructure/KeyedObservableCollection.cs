using System;
using System.Collections;
using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Business
{
	public class KeyedObservableCollection<TKey, TValue> : ImpObservableSet<TValue>
		where TValue : IEquatable<TValue>
	{
		public KeyedObservableCollection(Func<TValue, TKey> getKey)
		{
			dictionary = new Dictionary<TKey, TValue>();
			this.getKey = getKey;
		}

		readonly Func<TValue, TKey> getKey;

		readonly Dictionary<TKey, TValue> dictionary;

		protected override void OnItemsAdded(ICollection items)
		{
			base.OnItemsAdded(items);

			foreach (TValue item in items)
			{
				var key = getKey(item);
				if (dictionary.ContainsKey(key))
				{
					throw new InvalidOperationException("Key already found.");
				}
				else
				{
					dictionary[key] = item;
				}
			}
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			dictionary.Clear();
		}

		protected override void OnItemsRemoved(ICollection items)
		{
			base.OnItemsRemoved(items);

			foreach (TValue item in items)
			{
				var key = getKey(item);
				if (dictionary.ContainsKey(key))
				{
					dictionary.Remove(getKey(item));
				}
				else
				{
					throw new InvalidOperationException("When removing items, could not find key. Don't use keys that change please.");
				}
			}
		}

		public TValue GetValueFromKey(TKey key)
		{
			TValue item = default(TValue);
			dictionary.TryGetValue(key, out item);
			return item;
		}
	}
}
