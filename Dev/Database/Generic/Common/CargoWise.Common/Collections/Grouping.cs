using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.Common.Collections
{
	public class Grouping<TKey, TValue> : IGrouping<TKey, TValue>
	{
		public Grouping(TKey key, IEnumerable<TValue> items)
		{
			Key = key;
			values = new List<TValue>(items);
		}

		public TKey Key { get; }
		readonly List<TValue> values;

		public void Add(TValue value) => values.Add(value);

		public IEnumerator<TValue> GetEnumerator() => values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
