using System.Collections;
using System.Collections.Generic;

namespace CargoWise.Common
{
	/// <summary>
	/// A HashSet that preserves insertion order.
	/// </summary>
	public class LinkedHashSet<T> : IEnumerable<T>
	{
		public LinkedHashSet()
		{
			nodeLookup = new Dictionary<T, LinkedListNode<T>>();
			linkedList = new LinkedList<T>();
		}

		readonly IDictionary<T, LinkedListNode<T>> nodeLookup;
		readonly LinkedList<T> linkedList;

		public void Add(T item)
		{
			Argument.NotNull(item, nameof(item));

			if (!nodeLookup.ContainsKey(item))
			{
				nodeLookup.Add(item, linkedList.AddLast(item));
			}
		}

		public void UnionWith(IEnumerable<T> items)
		{
			Argument.NotNull(items, nameof(items));

			foreach (var item in items)
			{
				Add(item);
			}
		}

		public bool Remove(T item)
		{
			Argument.NotNull(item, nameof(item));

			if (nodeLookup.TryGetValue(item, out var node))
			{
				linkedList.Remove(node);
				nodeLookup.Remove(item);

				return true;
			}

			return false;
		}

		public void Clear()
		{
			linkedList.Clear();
			nodeLookup.Clear();
		}

		public int Count => nodeLookup.Count;

		public bool Contains(T item)
		{
			Argument.NotNull(item, nameof(item));

			return nodeLookup.ContainsKey(item);
		}

		public IEnumerator<T> GetEnumerator() => linkedList.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
