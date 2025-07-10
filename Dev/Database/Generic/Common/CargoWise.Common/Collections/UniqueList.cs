using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common.Collections
{
	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class UniqueList<T> : IEnumerable<T>, IList<T>
	{
		public UniqueList()
		{
			internalList = new List<T>();
		}

		public UniqueList(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(capacity));
			}

			internalList = new List<T>(capacity);
		}

		public UniqueList(IEnumerable<T> collection)
		{
			Argument.NotNull(collection, nameof(collection)); // Suggested By ReviewBot 
			internalList = new List<T>();
			AddRangeCore(collection);
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-this.Count >= Contract.OldValue(this.Count)")] //Suppressing message since system method is called
		public void Add(T item)
		{
			if (!internalList.Contains(item))
			{
				internalList.Add(item);
			}
		}

		public void AddRange(IEnumerable<T> collection)
		{
			Argument.NotNull(collection, nameof(collection)); // Suggested By ReviewBot 
			AddRangeCore(collection);
		}

		void AddRangeCore(IEnumerable<T> collection)
		{
			Argument.NotNull(collection, nameof(collection)); // Suggested By ReviewBot 
			foreach (T item in collection)
			{
				this.Add(item);
			}
		}

		public void ForEach(Action<T> action)
		{
			Argument.NotNull(action, nameof(action)); // Suggested By ReviewBot 
			internalList.ForEach(action);
		}

		#region IList<T> Members

		[SuppressMessage("Microsoft.Contracts", "Ensures-Contract.Result<int>() < this.Count")] // Can suppress since using System method underneath
		public int IndexOf(T item)
		{
			return internalList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			if (!internalList.Contains(item))
			{
				internalList.Insert(index, item);
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-this.Count == Contract.OldValue(this.Count) - 1")] //Can suppress since using System method underneath
		public void RemoveAt(int index)
		{
			internalList.RemoveAt(index);
		}

		public T this[int index]
		{
			get
			{
				return internalList[index];
			}
			set
			{
				if (!Contains(value))
				{
					internalList[index] = value;
				}
			}
		}

		#endregion

		#region ICollection<T> Members

		public void Clear()
		{
			internalList.Clear();
		}

		public bool Contains(T item)
		{
			if (this.Count <= 0)
			{
				return false;
			}
			return internalList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			internalList.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get
			{
				return internalList.Count;
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Ensures-!Contract.Result<bool>() || this.Count >= Contract.OldValue(this.Count - 1)")] //Suppressing since using System method underneath
		[SuppressMessage("Microsoft.Contracts", "Ensures-this.Count <= Contract.OldValue(this.Count)")] //Suppressing since using System method underneath
		public bool Remove(T item)
		{
			return internalList.Remove(item);
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		#endregion

		public T[] ToArray()
		{
			return internalList.ToArray();
		}

		readonly List<T> internalList;
	}
}
