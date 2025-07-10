using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	sealed class SharedDataTableIndexShard : IEnumerable<DataRow>
	{
		public SharedDataTableIndexShard(IZType key, int sizeHint)
		{
			Key = key;
			rows = new List<DataRow>(sizeHint);
		}

		public IZType Key { get; }
		readonly List<DataRow> rows;
		int suspended;
		public int Count => rows.Count;
		public event EventHandler<ListChangedEventArgs> ListChanged;

		internal IDisposable BeginReset()
		{
			rows.Clear();
			suspended++;

			return new DisposableAction(() =>
			{
				suspended--;
				Reset();
			});
		}

		internal bool Remove(DataRow row)
		{
			return RemoveAt(GetIndex(row));
		}

		bool RemoveAt(int index)
		{
			if (index >= 0)
			{
				rows.RemoveAt(index);
				if (suspended <= 0)
				{
					ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
				}
				return true;
			}

			return false;
		}

		internal void Add(DataRow row)
		{
			rows.Add(row);
			if (suspended <= 0)
			{
				ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemAdded, rows.Count - 1));
			}
		}

		internal DataRow GetRow(int index) => rows[index];

		void Reset()
		{
			if (suspended <= 0)
			{
				ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
			}
		}

		public IEnumerator<DataRow> GetEnumerator() => rows.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal void RowChanged(DataRow row)
		{
			if (suspended <= 0)
			{
				var index = GetIndex(row);
				ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.ItemChanged, index, index));
			}
		}

		internal IList ListProxy => rows;

		int GetIndex(DataRow row)
		{
			var len = rows.Count;
			for (int i = 0; i < len; i++)
			{
				if (rows[i] == row)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
