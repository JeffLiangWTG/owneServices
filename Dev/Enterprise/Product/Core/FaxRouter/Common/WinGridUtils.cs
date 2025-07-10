using System;
using System.Collections;
using System.Data;
using System.Windows.Forms;

namespace Enterprise.FaxRouter
{
	public static class WinGridUtils
	{
		public static int GridColumnIndex(DataGrid aCustomDataGrid, String aGridName, String aColumnName)
		{
			for (int i = 0; i < aCustomDataGrid.TableStyles[aGridName].GridColumnStyles.Count; i++)
			{
				if (aCustomDataGrid.TableStyles[aGridName].GridColumnStyles[i].MappingName.Equals(aColumnName))
				{
					return i;
				}
			}
			throw new IndexOutOfRangeException("Column [" + aColumnName + "] not found on data grid [" + aGridName + "]");
		}

		public static ArrayList GetSelectedRowsId(DataGrid aDataGrid, int aColumnIndex)
		{
			ArrayList selectedGridRows = new ArrayList();

			DataTable aDataTable = (DataTable)aDataGrid.DataSource;

			int i = 0;
			foreach (DataRow aDataRow in aDataTable.Rows)
			{
				if (aDataGrid.IsSelected(i++))
				{
					selectedGridRows.Add(aDataRow[aColumnIndex]);
				}
			}
			return selectedGridRows;
		}

		public static ArrayList GetSelectedRowsId(DataGrid aDataGrid, String aColumnName)
		{
			ArrayList selectedGridRows = new ArrayList();

			DataTable aDataTable = (DataTable)aDataGrid.DataSource;
			if (aDataTable != null)
			{
				int i = 0;
				foreach (DataRow aDataRow in aDataTable.Rows)
				{
					if (aDataGrid.IsSelected(i++))
					{
						selectedGridRows.Add(aDataRow[aColumnName]);
					}
				}
			}
			return selectedGridRows;
		}
	}
}
