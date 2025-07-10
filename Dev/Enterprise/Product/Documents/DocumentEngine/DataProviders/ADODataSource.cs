using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.DataProviders
{
	abstract class ADODataSource : IDataRowSource
	{
		protected ADODataSource(DataTable table)
		{
			Argument.NotNull(table, "table");

			this.table = table;
			wrappedSource = null;
		}

		protected ADODataSource(ADODataSource wrappedSource, int[] wrappedRowsIndexes)
		{
			Argument.NotNull(wrappedSource, "wrappedSource");
			Argument.NotNull(wrappedRowsIndexes, "wrappedRowsIndexes");

			this.wrappedSource = wrappedSource;
			this.wrappedRowsIndexes = wrappedRowsIndexes;
		}

		protected ADODataSource(DataTable table, int[] wrappedRowsIndexes)
		{
			Argument.NotNull(table, "table");
			Argument.NotNull(wrappedRowsIndexes, "wrappedRowsIndexes");

			this.table = table;
			this.wrappedRowsIndexes = wrappedRowsIndexes;
		}

		readonly DataTable table;
		ADODataSource wrappedSource;
		int[] wrappedRowsIndexes;
		DataTable extraRowsTable;

		protected DataTable Table
		{
			get
			{
				return table == null && wrappedSource != null
					? wrappedSource.Table
					: table;
			}
		}

		#region Exposed DataTable Properties and Methods

		public int RowCount
		{
			get
			{
				return wrappedRowsIndexes != null
					? wrappedRowsIndexes.Length
					: Table.Rows.Count;
			}
		}

		public DataRow RowByIndex(int index)
		{
			if (wrappedRowsIndexes != null)
			{
				var wrappedIndex = wrappedRowsIndexes[index];
				if (wrappedIndex >= 0)
				{
					return wrappedSource != null ? wrappedSource.RowByIndex(wrappedIndex) : table.Rows[wrappedIndex];
				}
				else
				{
					return extraRowsTable.Rows[-wrappedIndex - 1];
				}
			}
			else
			{
				return Table.Rows[index];
			}
		}

		public string TableName
		{
			get { return Table.TableName; }
		}

		public bool ColumnExists(string columnName)
		{
			return Table.Columns.Contains(columnName);
		}

		#endregion

		#region Single result

		IDataRowSource[] SingleResult
		{
			get { return singleResult ?? (singleResult = new IDataRowSource[] { this }); }
		}
		IDataRowSource[] singleResult;

		#endregion

		#region GetNewDataSource

		protected abstract ADODataSource GetNewDataSource(DataTable table);
		protected abstract ADODataSource GetNewDataSource(ADODataSource wrappedSource, int[] wrappedRowsIndexes);

		#endregion

		#region GroupBy

		public IDataRowSource[] GroupBy(string[] groupByColumnNames)
		{
			try
			{
				return GroupByInternal(groupByColumnNames);
			}
			catch (OutOfMemoryException ex)
			{
				throw new DocumentEngineTooManyRowsException(ex); // Attempting to double vector size beyond maximum row size of an Excel file
			}
			catch (Exception ex)
			{
				if (ex.InnerException is OutOfMemoryException)
				{
					throw new DocumentEngineTooManyRowsException(ex.InnerException);
				}

				throw;
			}
		}

		IDataRowSource[] GroupByInternal(string[] columns)
		{
			if (columns.Length == 0 || RowCount == 0)
			{
				return SingleResult;
			}

			var groups = GroupByCore(columns);

			var helper = new GroupOrderByHelper();
			return helper.GetOrderedGroupBy(groups.Select(group => new GroupBySource(group.Keys, GetNewDataSource(this, group.Group.ToArray()))));
		}

		protected virtual IEnumerable<GroupByIndexes> GroupByCore(string[] columns)
		{
			if (columns.Length == 0 || RowCount == 0)
			{
				return new[] { new GroupByIndexes(Enumerable.Empty<IComparable>(), new List<int>()) };
			}

			var groups = new Dictionary<IEnumerable<IComparable>, GroupByIndexes>(new GroupEqualityComparer());

			for (int i = 0; i < RowCount; i++)
			{
				DataRow row = RowByIndex(i);

				var rowValues = new List<IComparable>();
				foreach (var column in columns)
				{
					var columnName = column.Trim();
					var dotPosition = columnName.LastIndexOf('.');
					if (dotPosition > -1)
					{
						columnName = columnName.Substring(dotPosition + 1);
					}
					if (!Table.Columns.Contains(columnName))
					{
						throw new InvalidGroupByColumnException(this.MissingColumnErrorMessage, columnName);
					}

					var value = row[columnName];
					if (value == null || value == DBNull.Value)
					{
						value = string.Empty;
					}

					var comparable = value as IComparable;
					rowValues.Add(comparable ?? value.ToString());
				}

				GroupByIndexes indexes;
				if (!groups.TryGetValue(rowValues, out indexes))
				{
					indexes = new GroupByIndexes(rowValues, new List<int>());
					groups.Add(rowValues, indexes);
				}
				indexes.Group.Add(i);
			}

			return groups.Values;
		}

		protected virtual string MissingColumnErrorMessage
		{
			get
			{
				return string.Empty;
			}
		}

		#endregion

		#region Split

		public IDataRowSource Split(int rowsToKeep)
		{
			if (rowsToKeep < RowCount)
			{
				var firstSource = wrappedSource != null ? GetNewDataSource(wrappedSource, wrappedRowsIndexes) : GetNewDataSource(Table);
				firstSource.wrappedSource = wrappedSource;
				firstSource.wrappedRowsIndexes = wrappedRowsIndexes;
				wrappedSource = firstSource;

				if (rowsToKeep <= 0)
				{
					wrappedRowsIndexes = Array.Empty<int>();
					return firstSource;
				}
				else
				{
					wrappedRowsIndexes = Enumerable.Range(0, rowsToKeep).ToArray();
					return GetNewDataSource(firstSource, Enumerable.Range(rowsToKeep, firstSource.RowCount - rowsToKeep).ToArray());
				}
			}
			else
			{
				return GetNewDataSource(this, Array.Empty<int>());
			}
		}

		#endregion

		#region GetFirstNRows

		public IDataRowSource GetFirstNRows(int n)
		{
			if (n <= RowCount)
			{
				return GetNewDataSource(this, Enumerable.Range(0, n).ToArray());
			}
			else
			{
				var newSource = GetNewDataSource(this, Enumerable.Range(0, RowCount).Concat(Enumerable.Range(1, n - RowCount).Select(i => -1)).ToArray());

				newSource.extraRowsTable = Table.Clone();
				for (int i = 0; i < n - RowCount; i++)
				{
					newSource.extraRowsTable.Rows.Add(newSource.extraRowsTable.NewRow());
				}

				return newSource;
			}
		}

		#endregion

		#region Filter

		public IDataRowSource Filter(string expressions)
		{
			return FilterInternal(expressions);
		}

		protected abstract IDataRowSource FilterInternal(string expressions);

		#endregion

		#region GroupCount

		public int GroupCount(string[] columnNames)
		{
			return GroupByCore(columnNames).Count();
		}

		#endregion

		public IDataRowSource GetRowsFromIndexes(int[] indexes)
		{
			if (wrappedRowsIndexes != null)
			{
				var newWrappedRowsIndexes = DataRowSourceHelper.GetRowsFromIndexes(indexes, wrappedRowsIndexes);

				return GetNewDataSource(this, newWrappedRowsIndexes.ToArray());
			}
			else
			{
				return GetNewDataSource(this, indexes);
			}
		}
	}
}
