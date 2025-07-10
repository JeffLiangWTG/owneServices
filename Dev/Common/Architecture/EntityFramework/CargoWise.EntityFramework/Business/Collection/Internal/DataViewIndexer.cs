using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	sealed class DataViewIndexer : IList, IDisposable
	{
		internal DataViewIndexer(SharedDataTableIndex heapIndex, string matchingColumn, object predicate, EventHandler<ListChangedEventArgs> dataView_ListChanged)
		{
			this.matchingColumn = matchingColumn;
			hasSharedDataView = true;
			this.dataView_ListChanged = dataView_ListChanged;
			this.predicate = heapIndex.CoerceToKey(predicate);
			heap = heapIndex.GetHeap(this.predicate);
			wrappedList = heap.ListProxy;
			this.heapIndex = heapIndex;
			heap.ListChanged += Proxy_ListChanged;
		}

		public DataViewIndexer(DataView dataView, EventHandler<ListChangedEventArgs> dataView_ListChanged)
		{
			wrappedList = View = dataView;
			dataView.ListChanged += Proxy_ListChanged;
			this.dataView_ListChanged = dataView_ListChanged;
		}

		readonly EventHandler<ListChangedEventArgs> dataView_ListChanged;
		readonly SharedDataTableIndexShard heap;
		readonly SharedDataTableIndex heapIndex;
		readonly bool hasSharedDataView;
		readonly string matchingColumn;
		readonly IZType predicate;
		readonly IList wrappedList;

		public DataView View { get; }

		DataRow GetRow(int index) => hasSharedDataView ? heap.GetRow(index) : View[index].Row;

		void Proxy_ListChanged(object sender, ListChangedEventArgs e) => dataView_ListChanged(this, e);

		internal bool MatchesIndex(DataRow row) => hasSharedDataView && heapIndex.ContainsRow(row) && heapIndex.CoerceToKey(row[matchingColumn]).Equals(predicate);

		#region IList

		public IEnumerator GetEnumerator()
		{
			if (hasSharedDataView)
			{
				return heap.GetEnumerator();
			}
			else
			{
				return View.Cast<DataRowView>().Select(s => s.Row).GetEnumerator();
			}
		}

		public int Count => wrappedList.Count;
		public bool Contains(object value) => wrappedList.Contains(value);
		public void CopyTo(Array array, int index) => wrappedList.CopyTo(array, index);
		public bool IsReadOnly => wrappedList.IsReadOnly;
		public bool IsFixedSize => wrappedList.IsFixedSize;
		public int Add(object value) => throw new InvalidOperationException("What are you doing lol");

		#endregion

		#region IList disabled

		public DataRow this[int index]
		{
			get => GetRow(index);
			set => throw new InvalidOperationException(FormattableString.Invariant($"Trying to set {index} with {value}, but this is ReadOnly."));
		}

		public bool IsSynchronized => wrappedList.IsSynchronized;
		public object SyncRoot => wrappedList.SyncRoot;

		object IList.this[int index]
		{
			get => this[index];
			set => this[index] = (DataRow)value;
		}

		public void Clear() => throw new InvalidOperationException();
		public int IndexOf(object value) => throw new InvalidOperationException();
		public void Insert(int index, object value) => throw new InvalidOperationException();
		public void Remove(object value) => wrappedList.Remove(value);
		public void RemoveAt(int index) => throw new InvalidOperationException();

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (View != null)
			{
				View.ListChanged -= Proxy_ListChanged;
			}
			if (heap != null)
			{
				heap.ListChanged -= Proxy_ListChanged;
			}
		}

		#endregion
	}
}
