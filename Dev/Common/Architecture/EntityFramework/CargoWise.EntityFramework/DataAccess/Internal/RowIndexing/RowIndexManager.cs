using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	internal class RowIndexManager
	{
		internal RowIndexManager()
		{
			MaximumRowsBeforeUsingIndex = 100;
		}

		public DataRow[] GetRows(DataTable table, ZQuery filter, RowFactory rowFactory)
		{
			DataRow[] result = null;
			if (filter.IsDataViewOptimisable)
			{
				if (table.Rows.Count >= MaximumRowsBeforeUsingIndex)
				{
					result = GetRowsUsingCompositeParts(table, filter, rowFactory);
					if (result != null)
					{
						compositePartLoadCount++;
					}
					else
					{
						result = GetRowsUsingOrParts(table, filter, rowFactory);
						if (result != null)
						{
							orPartLoadCount++;
						}
					}
				}
				else
				{
					insufficientRowsToLoadCount++;
				}
			}
			else
			{
				dataViewNotOptimisableCount++;
			}
			return result;
		}

		public int CompositePartLoadCount
		{
			get { return compositePartLoadCount; }
		}
		public int OrPartLoadCount
		{
			get { return orPartLoadCount; }
		}
		public int InsufficientRowsToLoadCount
		{
			get { return insufficientRowsToLoadCount; }
		}
		public int DataViewNotOptimisableCount
		{
			get { return dataViewNotOptimisableCount; }
		}

		int compositePartLoadCount;
		int orPartLoadCount;
		int insufficientRowsToLoadCount;
		int dataViewNotOptimisableCount;

		DataRow[] GetRowsUsingOrParts(DataTable table, ZQuery filter, RowFactory rowFactory)
		{
			DataRow[] result = null;
			ZQuery[] orParts = filter.GetOrParts();
			if (orParts.Length > 0)
			{
				bool discardResult = false;
				Dictionary<DataRow, DataRowView> dataRowViews = new Dictionary<DataRow, DataRowView>();
				foreach (ZQuery orFilter in orParts)
				{
					ZSqlParameter matchingParameter = orFilter.GetMostUniqueSingleEqualParameter();
					if (matchingParameter != null)
					{
						DataView dataView = GetDataView(table, matchingParameter.SchemaColumn);
						if (dataView != null)
						{
							foreach (DataRowView rowView in dataView.FindRows(matchingParameter.Value))
							{
								dataRowViews[rowView.Row] = rowView;
							}
						}
					}
					else
					{
						discardResult = true;
						break;
					}
				}
				if (!discardResult)
				{
					DataRowViewConverter converter = new DataRowViewConverter(filter, rowFactory);
					DataRowView[] dataRowViewArray = new DataRowView[dataRowViews.Count];
					dataRowViews.Values.CopyTo(dataRowViewArray, 0);
					result = converter.Convert(dataRowViewArray);
				}
			}
			return result;
		}

		DataRow[] GetRowsUsingCompositeParts(DataTable table, ZQuery filter, RowFactory rowFactory)
		{
			DataRow[] result = null;
			ZQuery[] compositeParts = filter.GetCompositeParts();
			if (compositeParts.Length > 0)
			{
				ZSqlParameter matchingParameter = filter.GetMostUniqueSingleEqualParameter();
				if (matchingParameter != null)
				{
					DataView dataView = GetDataView(table, matchingParameter.SchemaColumn);
					if (dataView != null)
					{
						DataRowView[] dataRowViews = dataView.FindRows(matchingParameter.Value);

						DataRowViewConverter converter = new DataRowViewConverter(filter, rowFactory);
						result = converter.Convert(dataRowViews);
					}
				}
			}
			return result;
		}

		#region Implementation

		DataView GetDataView(DataTable table, SchemaColumn column)
		{
			DataView result = DataViewCache.FindDataView(table, "", column.Name, DataViewRowState.CurrentRows);
			if (result == null)
			{
				if (table.Rows.Count >= MaximumRowsBeforeUsingIndex)
				{
					result = DataViewCache.GetDataView(table, "", column.Name, DataViewRowState.CurrentRows);
				}
			}
			return result;
		}

		public int MaximumRowsBeforeUsingIndex { get; set; }

		#endregion
	}
}
