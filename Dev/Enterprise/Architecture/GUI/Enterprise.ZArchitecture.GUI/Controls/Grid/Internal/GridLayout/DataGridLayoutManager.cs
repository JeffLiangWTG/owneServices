using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Responsible for loading & saving layouts for a grid. 
	/// ZGrid has an instance of this and interacts with this class.
	/// This class interacts with DataGridLayoutDataAccessor.
	/// </summary>
	public class DataGridLayoutManager : IDataGridLayoutManager
	{
		public DataGridLayoutManager(string nameForDebugging)
			: this()
		{
			this.nameForDebugging = nameForDebugging;
		}

		public DataGridLayoutManager()
		{
			dataAccessor = new DataGridLayoutDataAccessor();
		}

		readonly DataGridLayoutDataAccessor dataAccessor;

		#region Load Layouts

		public GridColourScheme GetLayoutGridColourScheme(ZGuid gridColourPk)
		{
			return dataAccessor.GetLastSavedGridColourStorage(Factory, gridColourPk);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void LoadUserLayoutSettings(ZGrid grid)
		{
			var gridLayouts = dataAccessor.GetUserLayoutSetting(grid);

			LoadLayoutCore(grid, gridLayouts);
		}

		protected void LoadLayoutCore(ZGrid grid, DataSet columnSettingsDataSet)
		{
			try
			{
				grid.SuspendLayout();

				if (columnSettingsDataSet != null)
				{
					SetColumnLayout(grid, columnSettingsDataSet);

					SetColumnSort(grid, columnSettingsDataSet);
				}

				EnsureVisibleColumnsFirst(grid.Columns);
			}
			finally
			{
				if (grid.Columns.Count > 0 && !grid.Columns.Any(x => x.IsVisible))
				{
					var columnLayout = "";
					if (grid.CurrentColumnLayout != null)
					{
						columnLayout = grid.CurrentColumnLayout.ColumnLayoutName;
					}

					Globals.Message.ShowError(Res.GetString("46484C02-3E79-4B42-A963-F6B25A40DC79", "Cannot use layout '{0}' because there is no any visible column selected.", columnLayout));
					grid.Columns.HasLayoutChanged = true;
					grid.ResetColumns();
				}
				grid.ResumeLayout();
				grid.RefreshTableStyles();
				grid.Columns.HasLayoutChanged = false;
			}
		}

		#endregion

		#region Serialise & Save Layouts

		/// <summary>
		/// If it is a module grid and current filter layout allows column setting to save automatically (S9_SaveColumnLayout), then it should save the current layout
		/// </summary>
		/// <param name="grid"></param>
		public void SaveDefaultLayoutIfRequired(ZGrid grid)
		{
			if (grid is ZDisplayGrid)
			{
				SaveDefaultLayout(grid);
			}
		}

		public void SaveDefaultLayout(ZGrid grid)
		{
			dataAccessor.SaveDataGridDefaultColumnLayout(grid);
		}

		public StmModuleFilter SavePreconfiguredLayout(IModifyModuleAndGridLayout layoutManageable, ZString layoutName, ZBool publish, ZBool publishGlobal, SaveColumnLayout saveColumnLayout, SaveGridColourLayout saveGridColourLayout = SaveGridColourLayout.No, bool isUserDefinedFilter = false)
		{
			return dataAccessor.SavePreconfiguredLayout(layoutManageable, layoutName, publish, publishGlobal, saveColumnLayout, saveGridColourLayout, isUserDefinedFilter);
		}

		internal static StmModuleFilter SaveLayout(SaveLayoutBizO saveLayoutBusinessObject, FilterStripBusinessObject filterBusinessObject, bool canSaveColumnLayouts)
		{
			return new DataGridLayoutManager().SavePreconfiguredLayout(
														filterBusinessObject,
														saveLayoutBusinessObject.LayoutNameMultilingual.GetUnresolvedString(),
														saveLayoutBusinessObject.PublishLayout,
														saveLayoutBusinessObject.PublishAcrossAllCompanies,
														(canSaveColumnLayouts && saveLayoutBusinessObject.SaveColumnLayout) ? SaveColumnLayout.Yes : SaveColumnLayout.No,
														saveLayoutBusinessObject.SaveGridColourLayout ? SaveGridColourLayout.Yes : SaveGridColourLayout.No,
														saveLayoutBusinessObject.IsUserDefinedFilter);
		}

		#endregion

		#region Get Layouts

		public IGridLayoutStorage GetLastSavedLayout(ZGrid grid)
		{
#if DEBUG
			if (DesignModeFinder.IsDesigning)
			{
				return null;
			}
#endif
			return dataAccessor.GetLastSavedLayoutStorage(Factory, grid);
		}

		public IGridLayoutStorage GetDefaultLayout(ZGrid grid)
		{
#if DEBUG
			if (DesignModeFinder.IsDesigning)
			{
				return null;
			}
#endif
			return dataAccessor.GetDefaultLayoutSettingsStorage(Factory, grid);
		}

		public IEnumerable<IGridLayoutStorage> GetSortedAllLayouts(ZGrid grid)
		{
			return dataAccessor.GetSortedAllLayoutsForFormGrid(Factory, grid);
		}

		#endregion

		public GridColourScheme GetGridColourLayout(ZGuid gridColourPK)
		{
			return dataAccessor.GetLastSavedGridColourStorage(Factory, gridColourPK);
		}

		#region Implementation

		internal const string GridLayoutTableName = "OGridColumnSettings";
		internal const string GridSortSettingTableName = "OGridSortSettings";

		void SetColumnLayout(ZGrid grid, DataSet columnSettingsDataSet) // This is legacy architecture that will be removed
		{
			if (columnSettingsDataSet.Tables.Contains(GridLayoutTableName))
			{
				var columnSettingsTable = columnSettingsDataSet.Tables[GridLayoutTableName];

				for (var i = 0; i < columnSettingsTable.Rows.Count; i++)
				{
					var row = columnSettingsTable.Rows[i];

					var col = grid.Columns[Convert.ToString(row[0])] ?? grid.Columns[Convert.ToString(row[0]), true];

					if (col != null)
					{
						grid.Columns.Move(col, i);
						var columnStyle = col.ColumnStyle;
						ControlDpiScalingHelper.SetWidth(ref columnStyle, Convert.ToInt32(row[1]), true);
						col.IsVisible = Convert.ToBoolean(row[2]) || col.IsMandatory;
					}
				}
			}
		}

		void SetColumnSort(ZGrid grid, DataSet columnSettingsDataSet) // This is legacy architecture that will be removed
		{
			if (columnSettingsDataSet.Tables.Contains(GridSortSettingTableName))
			{
				var sortSettingsTable = columnSettingsDataSet.Tables[GridSortSettingTableName];
				var props = grid.ListManager.GetItemProperties();
				if (sortSettingsTable.Rows.Count > 0)
				{
					var sortProperties = new List<SortProperty>(sortSettingsTable.Rows.Count);

					for (var i = 0; i < sortSettingsTable.Rows.Count; i++)
					{
						var propertyName = (string)sortSettingsTable.Rows[i]["SortPropertyName"];
						ZGridColumn column;
						ZGridColumnStyle columnStyle;
						var propertyDescriptor = props[propertyName];
						if ((column = grid.Columns[propertyName]) != null && (columnStyle = column.ColumnStyle as ZGridColumnStyle) != null
								&& columnStyle.IsSortable && propertyDescriptor != null && typeof(IComparable).IsAssignableFrom(propertyDescriptor.PropertyType))
						{
							var sortDirectionString = sortSettingsTable.Rows[i]["SortDirection"].ToString();
							var sortDirection = (ListSortDirection)Enum.Parse(typeof(ListSortDirection), sortDirectionString);
							sortProperties.Add(new SortProperty(propertyName, sortDirection));
						}
					}

					SetSort(grid, sortProperties.ToArray());
				}
			}
		}

		struct SortProperty
		{
			public SortProperty(string propertyName, ListSortDirection sortDirection)
			{
				PropertyName = propertyName;
				SortDirection = sortDirection;
			}

			public readonly string PropertyName;
			public readonly ListSortDirection SortDirection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant, Developer constant")]
		void SetSort(ZGrid grid, SortProperty[] sortProperties)
		{
			var bindingList = grid.ListManager != null ? grid.ListManager.List as IBindingList : null;
			if (sortProperties.Length > 0 && bindingList != null)
			{
				var sortPropertyName = "";
				try
				{
					var props = grid.ListManager.GetItemProperties();
					if (props != null)
					{
						var bindingListView = bindingList as IBindingListView;
						if (bindingListView != null && bindingListView.SupportsAdvancedSorting)
						{
							var sorts = new List<ListSortDescription>(sortProperties.Length);
							foreach (var sortProperty in sortProperties)
							{
								var propertyDescriptor = props[sortProperty.PropertyName];
								if (propertyDescriptor != null)
								{
									sorts.Add(new ListSortDescription(propertyDescriptor, sortProperty.SortDirection));
								}
							}
							bindingListView.ApplySort(new ListSortDescriptionCollection(sorts.ToArray()));
						}
						else if (bindingList.SupportsSorting)
						{
							sortPropertyName = sortProperties[sortProperties.Length - 1].PropertyName;
							var propertyDescriptor = props[sortPropertyName];
							if (propertyDescriptor != null)
							{
								bindingList.ApplySort(propertyDescriptor, sortProperties[sortProperties.Length - 1].SortDirection);
							}
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					var form = grid.FindForm();
					var formName = form != null ? form.Name : "Unknown";
					var message = "Exception in Grid column sort restore. Name: " + grid.Name + " List: " + bindingList.GetType().Name + " Sort: " + sortPropertyName + " Form: " + formName;
					ErrorReporter.ReportOnce(message, ex);
				}
			}
		}

		internal void SetupNewGroupColumnsToBeVisibleAndCorrectlyOrdered(ZGrid grid)
		{
			try
			{
				grid.SuspendLayout();

				var visibleGroupNames = new HashSet<string>(grid.Columns.Where(column => column.GroupName != null && column.GroupName.Caption != null && column.IsVisible).Select(column => column.GroupName.Caption));
				var matchingNewGroupColumns = grid.Columns.Where(column => !column.IsVisible && column.GroupName != null && visibleGroupNames.Contains(column.GroupName.Caption)).ToList();
				foreach (var newGroupColumn in matchingNewGroupColumns)
				{
					var newIndex = grid.Columns.Select((column, index) => new { column, index })
						.Last(item => item.column.IsVisible && item.column.GroupName.Caption == newGroupColumn.GroupName.Caption).index;
					grid.Columns.Move(newGroupColumn, newIndex + 1);
					newGroupColumn.IsVisible = true;
				}
			}
			finally
			{
				grid.ResumeLayout();
				grid.RefreshTableStyles();
				grid.Columns.HasLayoutChanged = false;
			}
		}

		public void EnsureVisibleColumnsFirst(ZGridColumns columns)
		{
			if (columns != null)
			{
				var visibleIndex = 0;
				for (var i = 0; i < columns.Count; ++i)
				{
					var column = columns[i];
					if (column.IsVisible)
					{
						if (i != visibleIndex)
						{
							columns.Move(column, visibleIndex);
						}
						visibleIndex++;
					}
				}
			}
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory() { NameForDebugging = nameForDebugging, RefreshEnabled = false });

		BusinessObjectFactory factory;
		readonly string nameForDebugging;

		#endregion
	}
}
