using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	sealed class SharedDataTableIndex : IDisposable
	{
		internal SharedDataTableIndex(DataTable dataTable, ZQuery filter, SchemaColumn column)
		{
			this.dataTable = dataTable;
			this.column = column;
			dataRowFilter = new SimpleZQueryDataRowFilter(filter);
			filterExpression = filter.LiteralTextADO;
			dataTable.RowDeleted += Table_RowDeleted;
			dataTable.RowChanged += Table_RowChanged;
			dataTable.TableNewRow += Table_TableNewRow;
			RebuildIndex();
		}

		readonly DataTable dataTable;
		readonly SchemaColumn column;
		readonly SimpleZQueryDataRowFilter dataRowFilter;
		readonly string filterExpression;
		readonly Dictionary<DataRow, SharedDataTableIndexShard> currentIndexes = new Dictionary<DataRow, SharedDataTableIndexShard>();
		readonly Dictionary<IZType, SharedDataTableIndexShard> buckets = new Dictionary<IZType, SharedDataTableIndexShard>();

		#region public API

		public bool ContainsRow(DataRow row) => currentIndexes.ContainsKey(row);

		public IZType CoerceToKey(object value) => ZDataType.ObjectToZType(column.GetEquivalentZType(), value);

		public SharedDataTableIndexShard GetHeap(IZType key)
		{
			if (!buckets.TryGetValue(key, out var heap))
			{
				const int heapSizeHint = 8; // Magic constant is untested. Just a guess.
				buckets[key] = heap = new SharedDataTableIndexShard(key, heapSizeHint);
			}

			return heap;
		}

		public void Dispose()
		{
			dataTable.RowDeleting -= Table_RowDeleted;
			dataTable.RowChanged -= Table_RowChanged;
			dataTable.TableNewRow -= Table_TableNewRow;
		}

		#endregion

		#region Event handlers

		void Table_RowDeleted(object sender, DataRowChangeEventArgs e)
		{
			RemoveRowFromIndex(e.Row);
		}

		void Table_TableNewRow(object sender, DataTableNewRowEventArgs e)
		{
			var row = e.Row;
			if (dataRowFilter.IsMatch(row))
			{
				OnRowMatchesIndex(row);
			}
		}

		void Table_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			switch (e.Action)
			{
				case DataRowAction.Change:
				case DataRowAction.Add:
					{
						var row = e.Row;
						if (row.RowState != DataRowState.Detached && dataRowFilter.IsMatch(row))
						{
							OnRowMatchesIndex(row);
						}
						else
						{
							RemoveRowFromIndex(row);
						}
						break;
					}
			}
		}

		#endregion

		#region Implementation

		void OnRowMatchesIndex(DataRow row)
		{
			if (row.RowState != DataRowState.Detached)
			{
				if (currentIndexes.TryGetValue(row, out var heap))
				{
					var key = GetKey(row);
					if (!heap.Key.Equals(key))
					{
						RemoveRowFromIndex(row);
						AddRowToIndex(row);
					}
					else
					{
						heap.RowChanged(row);
					}
				}
				else
				{
					AddRowToIndex(row);
				}
			}
		}

		void RebuildIndex()
		{
			currentIndexes.Clear();
			using (new DisposableList(buckets.Values.Select(h => h.BeginReset())))
			{
				foreach (var row in dataTable.Select(filterExpression))
				{
					AddRowToIndex(row);
				}
			}
		}

		void AddRowToIndex(DataRow row)
		{
			var key = GetKey(row);
			var heap = GetHeap(key);
			AddRowToHeap(row, heap);
		}

		void AddRowToHeap(DataRow row, SharedDataTableIndexShard heap)
		{
			heap.Add(row);
			currentIndexes[row] = heap;
		}

		void RemoveRowFromIndex(DataRow row)
		{
			if (currentIndexes.TryGetValue(row, out var heap))
			{
				heap.Remove(row);
				currentIndexes.Remove(row);
			}
		}

		IZType GetKey(DataRow row) => CoerceToKey(row[column.Name]);

		#endregion
	}
}
