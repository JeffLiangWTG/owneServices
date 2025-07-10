using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public class DataRowViewConverter
	{
		public DataRowViewConverter(ZQuery filter, RowFactory factory)
		{
			Argument.NotNull(filter, "filter");
			this.filter = filter;
			this.factory = factory;
		}
		readonly ZQuery filter;
		readonly RowFactory factory;

		public DataRow[] Convert(DataRowView[] dataRowViews)
		{
			List<DataRow> result = new List<DataRow>();
			if (dataRowViews.Length > 0)
			{
				DataTable table = dataRowViews[0].Row.Table;

				PrimaryKeyProvider primaryKeyProvider = new PrimaryKeyProvider(filter);
				var lazyFilterComparer = new Lazy<RowFilterComparer>(() => GetRowFilterComparer(table, primaryKeyProvider.FilterWithoutPrimaryKeys));

				foreach (DataRowView rowView in dataRowViews)
				{
					if ((!primaryKeyProvider.ContainsPrimaryKeys || primaryKeyProvider.ContainsKey(new ZGuid(rowView.Row[0]))) && lazyFilterComparer.Value.IsMatch(rowView.Row))
					{
						result.Add(rowView.Row);
					}
				}

				string orderBy = filter.OrderBy;
				if (orderBy.Length > 0)
				{
					DataRowSorter dataRowSorter = new DataRowSorter(table, orderBy);
					if (dataRowSorter.IsSortable)
					{
						dataRowSorter.Sort(result);
					}
					else
					{
						return null;
					}
				}

				if (filter.IsTopNQuery)
				{
					int maxRows = filter.MaximumRows.Value;
					if (result.Count > maxRows)
					{
						result.RemoveRange(maxRows, result.Count - maxRows);
					}
				}
			}

			return result.ToArray();
		}

		internal RowFilterComparer GetRowFilterComparer(DataTable table, IFilterPart filterWithoutPrimaryKeys)
		{
			RowFilterComparer rfc;
			var key = filterWithoutPrimaryKeys.LiteralTextADO + table.GetHashCode();
			if (key.Length < RowFilterCacheMaxKeySize)
			{
				var cache = factory.RowFilterComparerCache;

				if (!cache.TryGetValue(key, out rfc))
				{
					rfc = new RowFilterComparer(table, filterWithoutPrimaryKeys, filterWithoutPrimaryKeys.LiteralTextADO);
					cache.Add(key, rfc);
				}
			}
			else
			{
				rfc = new RowFilterComparer(table, filterWithoutPrimaryKeys);
			}

			return rfc;
		}

		const int RowFilterCacheMaxKeySize = 4096;
	}
}
