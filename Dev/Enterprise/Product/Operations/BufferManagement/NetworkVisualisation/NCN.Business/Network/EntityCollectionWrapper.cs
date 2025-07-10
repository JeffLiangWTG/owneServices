using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	[DebuggerDisplay("Count: {Count}")]
	public class EntityCollectionWrapper<T> : IList<T>, INotifyCollectionChanged
	{
		public EntityCollectionWrapper(IActiveBusinessObjectCollection collectionToWrap)
		{
			this.collectionToWrap = collectionToWrap;
		}

		public IActiveBusinessObjectCollection WrappedCollection
		{
			get { return collectionToWrap; }
		}

		readonly IActiveBusinessObjectCollection collectionToWrap;

		#region IList<T>

		public int IndexOf(T item)
		{
			return collectionToWrap.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			collectionToWrap.Insert(index, item);
			OnCollectionChanged();
		}

		public void RemoveAt(int index)
		{
			collectionToWrap.RemoveAt(index);
			OnCollectionChanged();
		}

		public T this[int index]
		{
			get { return (T)collectionToWrap[index]; }
			set
			{
				collectionToWrap[index] = value;
				OnCollectionChanged();
			}
		}

		public void Add(T item)
		{
			collectionToWrap.Add(item);
			OnCollectionChanged();
		}

		public void Clear()
		{
			collectionToWrap.Clear();
			OnCollectionChanged();
		}

		public bool Contains(T item)
		{
			return collectionToWrap.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			collectionToWrap.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return collectionToWrap.Count; }
		}

		public bool IsReadOnly
		{
			get { return collectionToWrap.IsReadOnly; }
		}

		public bool Remove(T item)
		{
			var count = Count;

			var bizo = item as BusinessObject;
			if (bizo != null)
			{
				collectionToWrap.Relationship.RemoveFromRelationship(bizo);
			}
			else
			{
				collectionToWrap.Remove(item);
			}

			var result = Count == count - 1;
			if (result)
			{
				OnCollectionChanged();
			}

			return result;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return collectionToWrap.Cast<T>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		protected void OnCollectionChanged()
		{
			if (CollectionChanged != null)
			{
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;
	}
}
