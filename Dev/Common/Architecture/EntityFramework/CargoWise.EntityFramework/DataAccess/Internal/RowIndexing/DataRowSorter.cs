using System;
using System.Collections.Generic;
using System.Data;

namespace CargoWise.EntityFramework
{
	internal class DataRowSorter
	{
		public DataRowSorter(DataTable table, string orderBy)
		{
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}

			if (orderBy == null)
			{
				throw new ArgumentNullException(nameof(orderBy));
			}

			this.table = table;
			indexKeys = ParseOrderBy(orderBy);
		}

		public void Sort(List<DataRow> rows)
		{
			rows.Sort(Compare);
		}

		IndexKey[] ParseOrderBy(string orderBy)
		{
			IndexKey[] indexKeys;
			if (orderBy.Length > 0)
			{
				string[] keys = orderBy.Split(',');
				indexKeys = new IndexKey[keys.Length];
				for (int keyLoop = 0; keyLoop < indexKeys.Length; keyLoop++)
				{
					string key = keys[keyLoop].Trim();
					int keyLength = key.Length;
					bool isDescending = false;
					if ((keyLength >= 5) && (string.Compare(key, keyLength - 4, " ASC", 0, 4, StringComparison.OrdinalIgnoreCase) == 0))
					{
						key = key.Substring(0, keyLength - 4).Trim();
					}
					else if ((keyLength >= 6) && (string.Compare(key, keyLength - 5, " DESC", 0, 5, StringComparison.OrdinalIgnoreCase) == 0))
					{
						isDescending = true;
						key = key.Substring(0, keyLength - 5).Trim();
					}
					DataColumn column = table.Columns[key];
					indexKeys[keyLoop] = new IndexKey(column, isDescending);
				}
				isSortable = true;
			}
			else
			{
				indexKeys = Array.Empty<IndexKey>();
			}
			return indexKeys;
		}

		public bool IsSortable
		{
			get { return isSortable; }
		}

		#region Implementation

		int Compare(DataRow row1, DataRow row2)
		{
			int result = 0;
			foreach (IndexKey key in indexKeys)
			{
				object row1Value = row1[key.Column];
				object row2Value = row2[key.Column];
				if (row1Value == null || row1Value == DBNull.Value)
				{
					result = row2Value == null || row2Value == DBNull.Value ? 0 : -1;
				}
				else if (row2Value == null || row2Value == DBNull.Value)
				{
					result = 1;
				}
				else
				{
					result = ((IComparable)row1Value).CompareTo((IComparable)row2Value);
				}
				if (result != 0)
				{
					if (key.isDescending)
					{
						result = -result;
					}
					break;
				}
			}
			return result;
		}

		#endregion

		readonly DataTable table;
		readonly IndexKey[] indexKeys;
		bool isSortable;

		struct IndexKey
		{
			public IndexKey(DataColumn column, bool isDescending)
			{
				Column = column;
				this.isDescending = isDescending;
			}
			public DataColumn Column;
			public bool isDescending;
		}
	}
}
