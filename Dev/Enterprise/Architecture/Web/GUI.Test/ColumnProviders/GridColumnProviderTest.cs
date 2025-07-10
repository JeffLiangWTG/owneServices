using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public abstract class GridColumnProviderTest : GridColumnProviderBaseTest
	{
		#region TestFixOldLayout

		public virtual void TestFixOldInvalidLayout()
		{
			if (SupportsOldLayoutFix)
			{
				if (TestProvider.UniqueColumnsCount > 2)
				{
					TestFixOldLayoutCore(new KeyValuePair<string, string>("0, 1,blah,2", "0,1,2"));
					TestFixOldLayoutCore(new KeyValuePair<string, string>("0,1, 100000, 2", "0,1,2"));
				}
				else
				{
					TestFixOldLayoutCore(new KeyValuePair<string, string>("0,blah, 1", "0,1"));
					TestFixOldLayoutCore(new KeyValuePair<string, string>("0,1, 100000", "0,1"));
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestFixOldLayout()
		{
			if (SupportsOldLayoutFix)
			{
				BeforeLayoutsWithNoDynamicColumns();
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestFixOldLayoutCore(LayoutsWithNoDynamicColumns);

				BeforeLayoutsWithFewDynamicColumns();
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestFixOldLayoutCore(LayoutsWithFewDynamicColumns);

				BeforeLayoutsWithAllDynamicColumns();
				TestProvider = GetNewTestProvider();
				TestProvider.CustomizeDictionary();
				SetupColumns();
				TestFixOldLayoutCore(LayoutsWithAllDynamicColumns);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool SupportsOldLayoutFix
		{
			get
			{
				return true;
			}
		}

		protected virtual void BeforeLayoutsWithAllDynamicColumns()
		{
		}

		protected virtual void BeforeLayoutsWithFewDynamicColumns()
		{
		}

		protected virtual void BeforeLayoutsWithNoDynamicColumns()
		{
		}

		void TestFixOldLayoutCore(KeyValuePair<string, string> layouts)
		{
			if (!string.IsNullOrEmpty(layouts.Key))
			{
				AssertEquals("Incorrect transformation for old layout", layouts.Value, TestProvider.FixOldLayout(layouts.Key));
			}
			else
			{
				Assert(true);
			}
		}

		protected KeyValuePair<string, string> LayoutsWithAllDynamicColumns
		{
			get { return GetLayouts(GetColumnsForLayoutFixAllDynamicColumns()); }
		}

		protected virtual DataGridColumn[] GetColumnsForLayoutFixAllDynamicColumns()
		{
			return null;
		}

		protected KeyValuePair<string, string> LayoutsWithFewDynamicColumns
		{
			get { return GetLayouts(GetColumnsForLayoutFixFewDynamicColumns()); }
		}

		protected virtual DataGridColumn[] GetColumnsForLayoutFixFewDynamicColumns()
		{
			return null;
		}

		protected KeyValuePair<string, string> LayoutsWithNoDynamicColumns
		{
			get { return GetLayouts(GetColumnsForLayoutFixNoDynamicColumns()); }
		}

		protected virtual DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return null;
		}

		protected KeyValuePair<string, string> GetLayouts(DataGridColumn[] columns)
		{
			string oldLayout = string.Empty;
			string newLayout = string.Empty;
			List<int> oldColumns = TestProvider.OldColumnsOrder;
			if (columns != null)
			{
				if (columns != null && columns.Length != 0)
				{
					foreach (IUniqueKeyColumn column in columns)
					{
						oldLayout += (string.IsNullOrEmpty(oldLayout) ? "" : ",") + GetOldColumnIndex(oldColumns, column.UniqueKey).ToString();
						newLayout += (string.IsNullOrEmpty(newLayout) ? "" : ",") + column.UniqueKey.ToString();
					}
				}
			}
			return new KeyValuePair<string, string>(oldLayout, newLayout);
		}

		protected int GetOldColumnIndex(List<int> oldColumns, int uniqueKey)
		{
			for (int i = 0; i < oldColumns.Count; i++)
			{
				if (oldColumns[i] == uniqueKey)
				{
					return i;
				}
			}
			return -1;
		}

		#endregion

		public void TestCalcEditColumnsBindToDecimals()
		{
			var expectedColumns = ExpectedColumns;
			var expectedCalcEditColumns = expectedColumns.Where(c => c is ZCalcEditColumn);
			var actualColumns = TestProvider.GridColumnFields;
			var actualCalcEditColumns = actualColumns.Where(c => c is ZCalcEditColumn);
			AssertEquals(expectedCalcEditColumns.Count(), actualCalcEditColumns.Count());
			foreach (ZCalcEditColumn expectedColumn in expectedCalcEditColumns)
			{
				var actualColumn = TestProvider[expectedColumn.UniqueKey] as ZCalcEditColumn;
				AssertNotNull(actualColumn);
				AssertEquals(actualColumn.HeaderText, expectedColumn.BindToDecimals, actualColumn.BindToDecimals);
			}
		}

		public void TestColumnsDateTimeFormat()
		{
			var expectedColumns = ExpectedColumns;
			var expectedDateTimeColumns = expectedColumns.Where(c => c is ZDateTimeColumn);
			var actualColumns = TestProvider.GridColumnFields;
			var actualDateTimeColumns = actualColumns.Where(c => c is ZDateTimeColumn);

			AssertEquals(expectedDateTimeColumns.Count(), actualDateTimeColumns.Count());

			foreach (ZDateTimeColumn expectedColumn in expectedDateTimeColumns)
			{
				var actualColumn = TestProvider[expectedColumn.UniqueKey] as ZDateTimeColumn;

				AssertNotNull(actualColumn);
				AssertEquals(actualColumn.HeaderText, expectedColumn.DateTimeFormat, actualColumn.DateTimeFormat);
			}
		}

		public virtual void TestUniqueColumns()
		{
			AssertEquals(ZString.Format("Expected {0} unique columns", ExpectedColumns.Count), ExpectedColumns.Count, TestProvider.UniqueColumnsCount);
			foreach (IUniqueKeyColumn column in ExpectedColumns)
			{
				try
				{
					AssertNotNull(TestProvider[column.UniqueKey]);
				}
				catch (Exception)
				{
					Assert("Column '" + ((DataGridColumn)column).HeaderText + "' could not be found with this key: " + column.UniqueKey, false);
				}
				AssertColumn((DataGridColumn)column, TestProvider[column.UniqueKey]);
			}
		}

		public virtual void TestDefaultColumns()
		{
			AssertEquals(ZString.Format("Expected {0} default columns", ExpectedDefaultColumns.Count), ExpectedDefaultColumns.Count, TestProvider.DefaultColumns.Count);
			foreach (IUniqueKeyColumn column in ExpectedDefaultColumns)
			{
				Assert("Column '" + ((DataGridColumn)column).HeaderText + "' is not in the list for default columns", TestProvider.DefaultColumns.Contains(column.UniqueKey));
			}
		}

		public virtual void TestRequiredColumns()
		{
			AssertEquals(ZString.Format("Expected {0} required columns", ExpectedRequiredColumns.Count), ExpectedRequiredColumns.Count, TestProvider.RequiredColumns.Count);
			foreach (IUniqueKeyColumn column in ExpectedRequiredColumns)
			{
				Assert(TestProvider.RequiredColumns.Contains(column.UniqueKey));
			}
		}

		public void TestRestrictedColumnsAreNotAddedToRequiredAndDefault()
		{
			foreach (var columnKey in TestProvider.RequiredAndDefaultColumns)
			{
				Assert(ZString.Format("Restricted column with key {0} should not be added to Required and Default Columns", columnKey), !TestProvider.RestrictedColumns.Contains(columnKey));
			}
		}

		public void TestRestrictedColumns()
		{
			AssertEquals(ZString.Format("Expected {0} restricted columns", ExpectedRestrictedColumns.Count), ExpectedRestrictedColumns.Count, TestProvider.RestrictedColumns.Count);
			foreach (var columnKey in ExpectedRestrictedColumns)
			{
				Assert(TestProvider.RestrictedColumns.Contains(columnKey));
			}
		}

		public void TestSortableColumns()
		{
			var unsortableColumns = TestProvider
				.AllColumns
				.Where(c => string.IsNullOrEmpty(c.SortExpression) &&
							!ExpectedUnsortableColumnKeys.Contains(((IUniqueKeyColumn)c).ColumnKey));

			AssertEquals("These columns are unsortable", string.Empty, string.Join(", ", unsortableColumns.Select(c => c.HeaderText)));
		}

		#region Implementation

		protected virtual void AssertColumn(DataGridColumn expectedColumn, DataGridColumn actualColumn)
		{
			AssertNotNull(expectedColumn);
			AssertEquals(expectedColumn.GetType(), actualColumn.GetType());
			AssertEquals(expectedColumn.HeaderText, actualColumn.HeaderText);
			if (actualColumn is ZGroupColumn)
			{
				AssertEquals(((ZGroupColumn)expectedColumn).GroupMembers.Length, ((ZGroupColumn)actualColumn).GroupMembers.Length);
				for (int i = 0; i < ((ZGroupColumn)actualColumn).GroupMembers.Length; i++)
				{
					AssertColumn(((ZGroupColumn)expectedColumn).GroupMembers[i], ((ZGroupColumn)actualColumn).GroupMembers[i]);
				}
			}
			else
			{
				if (expectedColumn is IBindTo || actualColumn is IBindTo)
				{
					AssertEquals(((IBindTo)expectedColumn).BindTo, ((IBindTo)actualColumn).BindTo);
				}
			}
		}

		protected List<DataGridColumn> ExpectedRequiredColumns
		{
			get { return expectedRequiredColumns ?? (expectedRequiredColumns = new List<DataGridColumn>()); }
		}
		List<DataGridColumn> expectedRequiredColumns;

		protected List<DataGridColumn> ExpectedColumns
		{
			get { return expectedColumns ?? (expectedColumns = new List<DataGridColumn>()); }
		}
		List<DataGridColumn> expectedColumns;

		protected List<DataGridColumn> ExpectedDefaultColumns
		{
			get { return expectedDefaultColumns ?? (expectedDefaultColumns = new List<DataGridColumn>()); }
		}
		List<DataGridColumn> expectedDefaultColumns;

		protected List<int> ExpectedRestrictedColumns
		{
			get { return expectedRestrictedColumns ?? (expectedRestrictedColumns = new List<int>()); }
		}
		List<int> expectedRestrictedColumns;

		protected List<object> ExpectedUnsortableColumnKeys => expectedUnsortableColumnKeys ?? (expectedUnsortableColumnKeys = GetUnsortableColumnKeys());
		List<object> expectedUnsortableColumnKeys;

		protected virtual List<object> GetUnsortableColumnKeys() => new List<object>();

		protected override void SetupNewProvider()
		{
			base.SetupNewProvider();
			SetupColumns();
		}

		protected void SetupColumns()
		{
			ExpectedDefaultColumns.Clear();
			ExpectedRequiredColumns.Clear();
			ExpectedColumns.Clear();
			SetupColumnsCore();
		}

		protected virtual void SetupColumnsCore()
		{
		}

		protected virtual void AddDefaultsColumn(DataGridColumn column)
		{
			ExpectedDefaultColumns.Add(column);
			AddColumn(column);
		}

		protected virtual void AddRequiredColumn(DataGridColumn column)
		{
			ExpectedRequiredColumns.Add(column);
			AddColumn(column);
		}

		protected virtual void AddColumn(DataGridColumn column)
		{
			((IUniqueKeyColumn)column).ColumnIndex = ExpectedColumns.Count;
			ExpectedColumns.Add(column);
		}

		#endregion
	}
}
