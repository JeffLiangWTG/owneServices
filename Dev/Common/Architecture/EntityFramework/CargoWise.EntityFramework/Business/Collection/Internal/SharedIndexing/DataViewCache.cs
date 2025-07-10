using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace CargoWise.EntityFramework
{
	class DataViewCacheKey
	{
		public DataViewCacheKey(string filter, string sort, DataViewRowState rowState)
		{
			this.filter = filter;
			this.sort = sort;
			this.rowState = rowState;
		}

		public string Filter => filter;
		public string Sort => sort;
		public DataViewRowState RowState => rowState;

		readonly string filter;
		readonly string sort;
		readonly DataViewRowState rowState;

		public override int GetHashCode()
		{
			return
				filter.GetHashCode() ^
				(string.IsNullOrEmpty(sort) ? 0 : sort.GetHashCode()) ^
				rowState.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			DataViewCacheKey cacheKey = obj as DataViewCacheKey;

			return
				cacheKey != null &&
				cacheKey.filter == filter &&
				cacheKey.sort == sort &&
				cacheKey.rowState == rowState;
		}
	}

	/// <summary>
	/// A cache of DataView objects.
	/// </summary>
	class DataViewCache : IndexCache<DataViewCacheKey, DataView>
	{
		internal DataViewCache(DataTable table)
		{
			this.table = table;
		}

		#region GetDataView

		/// <summary>
		/// Get a cached instance of a DataView.
		/// IMPORTANT: Do not change the RowFilter, Sort or RowStateFilter on the DataView
		/// returned by this method. The DataView returned is shared by other callers.
		/// </summary>
		public static DataView GetDataView(DataTable table, string rowFilter, string sort, DataViewRowState rowStateFilter)
		{
			return GetInstance(table).GetDataViewCore(new DataViewCacheKey(rowFilter, sort, rowStateFilter));
		}

		/// <summary>
		/// Find an already cached instance of a DataView. If no cached version exists, null is returned.
		/// IMPORTANT: Do not change the RowFilter, Sort or RowStateFilter on the DataView
		/// returned by this method. The DataView returned is shared by other callers.
		/// </summary>
		public static DataView FindDataView(DataTable table, string rowFilter, string sort, DataViewRowState rowStateFilter)
		{
			return GetInstance(table).FindDataViewCore(new DataViewCacheKey(rowFilter, sort, rowStateFilter));
		}

		/// <summary>
		/// Removes unwanted DataView object from cache.
		/// </summary>
		/// <param name="table">Table, for which DataView belongs.</param>
		/// <param name="dataView">Unwanted DataView object</param>
		public static void RemoveDataView(DataTable table, DataView dataView)
		{
			if (dataView != null && table != null)
			{
				GetInstance(table).RemoveDataViewCore(dataView);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		protected override DataView GetNewIndex(DataViewCacheKey key)
		{
			const string exceptionMessage = "Exception occurred during creation of DataView, probably due to multi-threading access to DataTable ";

			var repeats = 3;
			while (true)
			{
				try
				{
					return new DataView(table, key.Filter, key.Sort, key.RowState);
				}
				catch (InvalidOperationException ex)
				{
					if (--repeats == 0)
					{
						throw new DataException(exceptionMessage + table.TableName, ex);
					}
				}
				catch (ArgumentException ex)
				{
					if (--repeats == 0)
					{
						throw new DataException(exceptionMessage + table.TableName, ex);
					}
				}

				Thread.Sleep(1);
			}
		}

		#endregion

		#region GetPKSortedDataView

		/// <summary>
		/// Get a cached instace of a DataView, sorted by primary key.
		/// IMPORTANT: Do not change the RowFilter, Sort or RowStateFilter on the DataView
		/// returned by this method. The DataView returned is shared by other callers.
		/// </summary>
		public static DataView GetPKSortedDataView(DataTable table)
		{ return GetPKSortedDataView(table, ""); }

		public static DataView GetPKSortedDataView(DataTable table, string rowFilter)
		{ return GetPKSortedDataView(table, rowFilter, DataViewRowState.CurrentRows); }

		public static DataView GetPKSortedDataView(DataTable table, string rowFilter, DataViewRowState rowStateFilter)
		{
			DataViewCache instance = GetInstance(table);
			return instance.GetPKSortedDataView(rowFilter, rowStateFilter);
		}

		DataView GetPKSortedDataView(string rowFilter, DataViewRowState rowStateFilter)
		{
			foreach (DataView view in pkSortedDataViews)
			{
				if (view.RowFilter == rowFilter && view.RowStateFilter == rowStateFilter)
				{
					return view;
				}
			}
			DataView newView = GetNewIndex(new DataViewCacheKey(rowFilter, PKSortString, rowStateFilter));
			pkSortedDataViews.Add(newView);
			return newView;
		}
		readonly List<DataView> pkSortedDataViews = new List<DataView>();

		string PKSortString
		{
			get
			{
				if (pkSortString == null)
				{
					pkSortString = "";
					foreach (DataColumn pkColumn in table.PrimaryKey)
					{
						if (pkSortString.Length > 0)
						{
							pkSortString += ",";
						}

						pkSortString += pkColumn.ColumnName;
					}
				}
				return pkSortString;
			}
		}
		string pkSortString;

		#endregion

		#region Implementation

		readonly DataTable table;
		internal static DataViewCache GetInstance(DataTable table) => IndexCacheDataTableMixin.GetInstance(table).DataViewCache;

		#endregion

		#region Test
#if DEBUG

		internal static DataViewCache GetInstance_Test(DataTable table) => GetInstance(table);
		internal Dictionary<DataViewCacheKey, WeakReference> Cache_Test => cache;

#endif
		#endregion
	}
}
