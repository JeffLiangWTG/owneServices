using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	sealed partial class CompactTableStrategy
	{
		[DebuggerDisplay("Count = {Count}")]
		sealed class ColumnKeyList : IList<ColumnKey>
		{
			public ColumnKeyList()
			{
				lookup = new Dictionary<ColumnKey, int>();
				list = new List<ColumnKey>();
			}

			public int GetIndex(ColumnKey key)
			{
				int index;

				if (!lookup.TryGetValue(key, out index))
				{
					index = list.Count;
					list.Add(key);
					lookup.Add(key, index);
				}

				return index;
			}

			public ColumnKey this[int index] => list[index];

			public int Count => list.Count;

			#region IList<ColumnKey> Members

			public int IndexOf(ColumnKey item)
			{
				int index;

				if (!lookup.TryGetValue(item, out index))
				{
					index = -1;
				}

				return index;
			}

			void IList<ColumnKey>.Insert(int index, ColumnKey item) => throw new NotSupportedException();

			void IList<ColumnKey>.RemoveAt(int index) => throw new NotSupportedException();

			ColumnKey IList<ColumnKey>.this[int index]
			{
				[DebuggerStepThrough]
				get { return this[index]; }
				set { throw new NotSupportedException(); }
			}

			#endregion

			#region ICollection<ColumnKey> Members

			void ICollection<ColumnKey>.Add(ColumnKey item) => new NotSupportedException();

			void ICollection<ColumnKey>.Clear() => throw new NotSupportedException();

			public bool Contains(ColumnKey item) => lookup.ContainsKey(item);

			public void CopyTo(ColumnKey[] array, int arrayIndex)
			{
				list.CopyTo(array, arrayIndex);
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			bool ICollection<ColumnKey>.IsReadOnly => true;

			bool ICollection<ColumnKey>.Remove(ColumnKey item) => throw new NotSupportedException();

			#endregion

			#region IEnumerable<ColumnKey> Members

			public IEnumerator<ColumnKey> GetEnumerator() => list.GetEnumerator();

			#endregion

			#region IEnumerable Members

			[DebuggerStepThrough]
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			#endregion

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			readonly Dictionary<ColumnKey, int> lookup;

			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			readonly List<ColumnKey> list;
		}
	}
}
