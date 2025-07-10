using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Serialises or deserialises a grid layout
	/// </summary>
	internal class DataGridLayoutDataSetSerialiser
	{
		internal DataSet SerialiseAndGetCurrentLayoutAsDataset(ZGrid grid)
		{
			var columnSettingsDataSet = new DataSet(); // This is only for grid layout details, not for real tables
			columnSettingsDataSet.Tables.Add(GetColumnSettingTable(grid));

			var sortSettingTable = GetSortSettingTable(grid);
			if (sortSettingTable != null)
			{
				columnSettingsDataSet.Tables.Add(sortSettingTable);
			}

			return columnSettingsDataSet;
		}

		internal DataSet GetColumnSettingDataSet(MemoryStream gridLayoutsStream)
		{
			var result = new DataSet();// This is only for grid layout details, not for real tables

			try
			{
				if (gridLayoutsStream.Length > 0)
				{
					gridLayoutsStream.Position = 0;
					result.ReadXml(gridLayoutsStream, XmlReadMode.Auto);
				}
			}
			catch (System.Xml.XmlException)
			{
				// Registry values in the DevExpressGrid format fail to load.
				// These values are replaced by new format ones when saved.
			}

			return result;
		}

		internal MemoryStream GetLayoutStreamFromDataSet(DataSet gridLayout)
		{
			var result = new MemoryStream();

			gridLayout.WriteXml(result, XmlWriteMode.IgnoreSchema);
			result.Position = 0;

			return result;
		}

		internal MemoryStream GetLayoutStream(ZGridColumns currentColumns)
		{
			var result = new MemoryStream();

			GetColumnSettingTable(currentColumns).WriteXml(result, XmlWriteMode.IgnoreSchema);
			result.Position = 0;

			return result;
		}

		internal MemoryStream GetLayoutStream(IEnumerable<ICustomizableColumn> currentColumns)
		{
			var result = new MemoryStream();

			GetColumnSettingTable(currentColumns).WriteXml(result, XmlWriteMode.IgnoreSchema);
			result.Position = 0;

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		internal IEnumerable<string> GetVisibleColumnNames(MemoryStream gridLayoutsStream)
		{
			var dataSet = GetColumnSettingDataSet(gridLayoutsStream);
			if (dataSet != null)
			{
				var layoutTable = dataSet.Tables[DataGridLayoutManager.GridLayoutTableName];
				if (layoutTable != null)
				{
					foreach (DataRow row in layoutTable.Rows)
					{
						if ((string)row["IsVisible"] == "true")
						{
							yield return (string)row["MappingName"];
						}
					}
				}
			}
		}

		#region Implementation

		DataTable GetColumnSettingTable(ZGrid grid)
		{
			return GetColumnSettingTable(grid.Columns);
		}

		DataTable GetColumnSettingTable(ZGridColumns currentColumns)
		{
			var result = GetColumnSettingTable();
			foreach (var col in currentColumns)
			{
				result.Rows.Add(new object[] { col.ColumnStyle.MappingName, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(col.ColumnStyle.Width), col.IsVisible });
			}
			return result;
		}

		DataTable GetColumnSettingTable(IEnumerable<ICustomizableColumn> currentColumns)
		{
			var result = GetColumnSettingTable();
			foreach (var col in currentColumns)
			{
				var columnName = !string.IsNullOrEmpty(col.ColumnName) ? col.ColumnName : col.ToString();
				result.Rows.Add(new object[] { columnName, 0, col.IsVisible });
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		DataTable GetColumnSettingTable()
		{
			var result = new DataTable(DataGridLayoutManager.GridLayoutTableName);
			result.Columns.Add("MappingName", typeof(string));
			result.Columns.Add("Width", typeof(int));
			result.Columns.Add("IsVisible", typeof(bool));
			return result;
		}

#if DEBUG
		public
#endif
		DataTable GetSortSettingTable(ZGrid grid)
		{
			DataTable result = null;

			var bindingList = grid.ListManager != null ? grid.ListManager.List as IBindingList : null;
			if (bindingList != null)
			{
				ListSortDescriptionCollection sorts = null;
				var bindingListView = bindingList as IBindingListView;
				if (bindingListView != null && bindingListView.SupportsAdvancedSorting)
				{
					sorts = bindingListView.SortDescriptions;
				}
				// Test for List.SupportsSorting is in ZGrid
				else if (bindingList.SupportsSorting && bindingList.SortProperty != null)
				{
					sorts = new ListSortDescriptionCollection(new[] { new ListSortDescription(bindingList.SortProperty, bindingList.SortDirection) });
				}

				if (sorts != null && sorts.Count > 0)
				{
					result = new DataTable(DataGridLayoutManager.GridSortSettingTableName);
					result.Columns.Add("SortPropertyName", typeof(string));
					result.Columns.Add("SortDirection", typeof(ListSortDirection));

					foreach (ListSortDescription sortDescription in sorts)
					{
						if (sortDescription.PropertyDescriptor is ZCustomPropertyDescriptor)
						{
							continue;
						}

						var sortSettingsRow = result.NewRow();
						sortSettingsRow["SortPropertyName"] = sortDescription.PropertyDescriptor.Name;
						sortSettingsRow["SortDirection"] = sortDescription.SortDirection;
						result.Rows.Add(sortSettingsRow);
					}
				}
			}

			return result;
		}

		#endregion
	}
}
