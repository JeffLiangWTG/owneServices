using System;
using System.Data;
using System.Threading;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataViewCacheTest : NUnit.Framework.TestCase
	{
		public void TestGetDataView()
		{
			DataView view1 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			DataView view2 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			AssertEquals("Cached instance returned", true, view1 == view2);
			AssertEquals("RowFilter", "Column1='x'", view1.RowFilter);
			AssertEquals("Sort", "Column1", view1.Sort);
			AssertEquals("RowStateFilter", DataViewRowState.ModifiedCurrent, view1.RowStateFilter);

			DataView differentFilter = DataViewCache.GetDataView(Table, "Column1='different'", "Column2", DataViewRowState.ModifiedCurrent);
			DataView differentSort = DataViewCache.GetDataView(Table, "Column1='x'", "Column2", DataViewRowState.ModifiedCurrent);
			DataView differentRowStateFilter = DataViewCache.GetDataView(Table, "Column1='x'", "Column2", DataViewRowState.OriginalRows);
			AssertEquals(false, differentFilter == view1);
			AssertEquals(false, differentSort == view1);
			AssertEquals(false, differentRowStateFilter == view1);
		}

		public void TestRemoveDataView()
		{
			var view1 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			var view2 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			AssertSame("Same DataView object should be returned", view1, view2);

			DataViewCache.RemoveDataView(Table, view1);
			var view3 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			AssertNotEquals("New DataView object should be created", view1, view3);
		}

		public void TestGetPKSortedDataView()
		{
			DataView currentRows1 = DataViewCache.GetPKSortedDataView(Table, "", DataViewRowState.CurrentRows);
			DataView deletedRows1 = DataViewCache.GetPKSortedDataView(Table, "", DataViewRowState.Deleted);
			DataView currentRows2 = DataViewCache.GetPKSortedDataView(Table, "", DataViewRowState.CurrentRows);
			DataView deletedRows2 = DataViewCache.GetPKSortedDataView(Table, "", DataViewRowState.Deleted);
			AssertEquals("Cached instance returned", true, currentRows1 == currentRows2);
			AssertEquals("Cached instance returned", true, deletedRows1 == deletedRows2);
			AssertEquals("RowStateFilter", DataViewRowState.CurrentRows, currentRows1.RowStateFilter);
			AssertEquals("RowStateFilter", DataViewRowState.Deleted, deletedRows1.RowStateFilter);
			AssertEquals("Primary key Sort", "PK1,PK2", deletedRows1.Sort);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI005:WeakReferenceTargetRaceConditionRule", Justification = "Testing")]
		public void TestNullTargetAreRemoved()
		{
			var view1 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			var dataViewCache = DataViewCache.GetInstance(Table);
			var lastCleanUpRun = dataViewCache.nextCleanUpRun;
			AssertEquals(1, dataViewCache.cache.Count);
			var key1 = new DataViewCacheKey("Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			var viewRef1 = dataViewCache.cache[key1];
			AssertEquals(view1, viewRef1.Target);
			viewRef1.Target = null;
			var view1New = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			AssertNotEquals("A new view should be created as the target was null", view1, view1New);
			viewRef1 = dataViewCache.cache[key1];
			AssertEquals(view1New, viewRef1.Target);
			viewRef1.Target = null;

			var view2 = DataViewCache.GetDataView(Table, "Column2='x'", "Column1", DataViewRowState.ModifiedCurrent);
			AssertEquals(2, dataViewCache.cache.Count);
			AssertEquals(lastCleanUpRun, dataViewCache.nextCleanUpRun);
			var key2 = new DataViewCacheKey("Column2='x'", "Column1", DataViewRowState.ModifiedCurrent);
			var viewRef2 = dataViewCache.cache[key2];
			AssertEquals(view2, viewRef2.Target);
			AssertEquals(viewRef1, dataViewCache.cache[key1]);
			AssertNull(viewRef1.Target);

			dataViewCache.nextCleanUpRun = DateTime.UtcNow.AddMinutes(-DataViewCache.CleanUpIntervalMinutes);
			System.Threading.Thread.Sleep(5);
			AssertEquals(view2, DataViewCache.GetDataView(Table, "Column2='x'", "Column1", DataViewRowState.ModifiedCurrent));
			AssertEquals(1, dataViewCache.cache.Count);
			AssertEquals(viewRef2, dataViewCache.cache[key2]);
			Assert(lastCleanUpRun < dataViewCache.nextCleanUpRun);

			view1 = DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent);
			AssertEquals(2, dataViewCache.cache.Count);
			AssertEquals(viewRef2, dataViewCache.cache[key2]);
			AssertEquals(view2, viewRef2.Target);
			viewRef1 = dataViewCache.cache[key1];
			AssertEquals(view1, viewRef1.Target);
			viewRef2.Target = null;
			AssertEquals(view1, DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent));
			AssertEquals(2, dataViewCache.cache.Count);
			AssertEquals(viewRef1, dataViewCache.cache[key1]);
			AssertEquals(view1, viewRef1.Target);
			AssertEquals(viewRef2, dataViewCache.cache[key2]);
			AssertNull(viewRef2.Target);

			dataViewCache.nextCleanUpRun = dataViewCache.nextCleanUpRun.AddMinutes(-DataViewCache.CleanUpIntervalMinutes);
			System.Threading.Thread.Sleep(10);
			AssertEquals(view1, DataViewCache.GetDataView(Table, "Column1='x'", "Column1", DataViewRowState.ModifiedCurrent));
			AssertEquals(1, dataViewCache.cache.Count);
			AssertEquals(viewRef1, dataViewCache.cache[key1]);
			AssertEquals(view1, viewRef1.Target);
		}

		public void TestMultiThreadedAccess()
		{
			for (int i = 0; i < 100; i++)
			{
				AddRow((100 - i).ToString(), "x");
			}

			var thread = new Thread(() =>
			{
				while (Table.Rows.Count > 0)
				{
					Table.Rows[0].Delete();
				}
			});
			thread.Start();

			var view = DataViewCache.GetDataView(Table, "Column2='x'", "Column1", DataViewRowState.ModifiedCurrent);

			AssertNotNull("Should come here without exceptions", view);
		}

		#region Implementation

		DataTable Table
		{
			get
			{
				if (table == null)
				{
					table = new DataTable("Table");
					table.Columns.Add("PK1");
					table.Columns.Add("PK2");
					table.Columns.Add("Column1");
					table.Columns.Add("Column2");
					table.PrimaryKey = new DataColumn[] { table.Columns[0], table.Columns[1] };
				}
				return table;
			}
		}
		DataTable table;

		DataRow AddRow(string column1 = null, string column2 = null)
		{
			var row = Table.NewRow();
			row["PK1"] = Guid.NewGuid();
			row["PK2"] = Guid.NewGuid();
			if (column1 != null)
			{
				row["Column1"] = column1;
			}

			if (column2 != null)
			{
				row["Column2"] = column2;
			}

			Table.Rows.Add(row);
			return row;
		}

		#endregion
	}
}
