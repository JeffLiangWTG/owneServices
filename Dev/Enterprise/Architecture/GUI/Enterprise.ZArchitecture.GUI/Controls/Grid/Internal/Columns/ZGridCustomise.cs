using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture
{
	public class ZGridCustomise : ZColumnsCustomise
	{
		public ZGridCustomise(ZGridColumns currentColumns, ZGridColumns defaultColumns, bool shouldToolStripBeAvailable, ZGridCustomiseBizObj customiseBizObj)
			: base(ConvertColumnsToGroups(currentColumns), ConvertColumnsToGroups(defaultColumns), shouldToolStripBeAvailable, customiseBizObj, currentColumns.Grid)
		{
			CurrentColumns = currentColumns;
			DefaultColumns = defaultColumns;

			PerformConversionFromLegacyKeyToControlIDKeyIfRequired();

			AvailableColumnsListBox.DrawMode = DrawMode.OwnerDrawFixed;
			AvailableColumnsListBox.DrawItem += ColumnsListBoxes_DrawItem;
			AvailableColumnsListBox.EnableDragAndDrop(false);
			AvailableColumnsListBox.MouseDown += AvailableColumnsListBox_MouseDown;
			AvailableColumnsListBox.MouseUp += AvailableColumnsListBox_MouseUp;
			AvailableColumnsListBox.AfterClearSelected += AvailableColumnsListBox_ListBoxClearSelected;
			AvailableColumnsListBox.DragDrop += AvailableColumnsListBox_DragDrop;

			CurrentColumnsListBox.DrawMode = DrawMode.OwnerDrawFixed;
			CurrentColumnsListBox.DrawItem += ColumnsListBoxes_DrawItem;
			CurrentColumnsListBox.EnableDragAndDrop(true);
			CurrentColumnsListBox.DragAndDropItemChanged += CurrentColumnsListBox_DragAndDropItemChanged;
			CurrentColumnsListBox.PreviewDropItem += CurrentColumnsListBox_PreviewDropItem;
			CurrentColumnsListBox.DragLeave += CurrentColumnsListBox_DragLeave;
			CurrentColumnsListBox.MouseDown += CurrentColumnsListBox_MouseDown;
		}

		internal void ParentGridDisposed(object sender, EventArgs e)
		{
			Close();
			Dispose();
		}

		readonly new ZGridColumns DefaultColumns;
		readonly new ZGridColumns CurrentColumns;
		internal bool isDragging;

		public new ZGridColumns Result
		{
			get { return result ?? ConvertGroupsToColumns(base.Result, CurrentColumns.Grid); }
			private set { result = value; }
		}
		ZGridColumns result;

		#region Overrides

		protected override bool CanAddColumn(ICustomizableColumn column)
		{
			var group = (ZGridColumnGroup)column;
			if (group.IsUnavailable)
			{
				Globals.Message.ShowError(group.ErrorMessageWhenUnavailable); // Contains strings which are already Res.GetStringLegacy'd.
				return false;
			}
			return true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				AvailableColumnsListBox.DrawItem -= ColumnsListBoxes_DrawItem;
				AvailableColumnsListBox.MouseDown -= AvailableColumnsListBox_MouseDown;
				AvailableColumnsListBox.MouseUp -= AvailableColumnsListBox_MouseUp;
				AvailableColumnsListBox.AfterClearSelected -= AvailableColumnsListBox_ListBoxClearSelected;
				AvailableColumnsListBox.DragDrop -= AvailableColumnsListBox_DragDrop;

				CurrentColumnsListBox.DrawItem -= ColumnsListBoxes_DrawItem;
				CurrentColumnsListBox.DragAndDropItemChanged -= CurrentColumnsListBox_DragAndDropItemChanged;
				CurrentColumnsListBox.PreviewDropItem -= CurrentColumnsListBox_PreviewDropItem;
				CurrentColumnsListBox.DragLeave -= CurrentColumnsListBox_DragLeave;
				CurrentColumnsListBox.MouseDown -= CurrentColumnsListBox_MouseDown;
			}

			base.Dispose(disposing);
		}
		#endregion

		#region Events

		void ColumnsListBoxes_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index >= 0)
			{
				var listbox = sender as ListBox;
				var col = listbox?.Items[e.Index] as ZGridColumnGroup;
				var textColor = e.ForeColor;
				var backColor = e.BackColor;

				if (e.State != DrawItemState.Selected && col.IsMandatory)
				{
#if !WINZOR
					textColor = Color.Gray;
#else
					listbox.ListBoxItemsData[e.Index].TextColor = Color.Gray;
#endif
				}

#if !WINZOR
				// In Windows 7 ListBox uses transparent colors and does not clean area for new item on scrolling, which leads to artefacts
				// Need to use both lines with generated colors with Alpha 0 then 255
				//e.Graphics.FillRectangle(BrushProvider.FromColor(e.BackColor), e.Bounds);
				e.Graphics.FillRectangle(BrushProvider.FromColor(Color.FromArgb(0, backColor.R, backColor.G, backColor.B)), e.Bounds);
				e.Graphics.FillRectangle(BrushProvider.FromColor(Color.FromArgb(255, backColor.R, backColor.G, backColor.B)), e.Bounds);
				TextRendererHelper.DrawText(e.Graphics, col.ToString(), e.Font, e.Bounds, BrushProvider.FromColor(textColor));
#endif
			}
		}

		void CurrentColumnsListBox_DragAndDropItemChanged(object sender, EventArgs e)
		{
			InvalidateCurrentColumnLayoutNameIfRequired();
		}

		void CurrentColumnsListBox_PreviewDropItem(object sender, EventArgs e)
		{
			if (isDragging && AvailableColumnsListBox.SelectedIndices.Count > 0)
			{
				AddColumns();
				isDragging = false;
			}
		}

		void CurrentColumnsListBox_DragLeave(object sender, EventArgs e)
		{
			CurrentColumnsListBox.Refresh();
		}

		internal void CurrentColumnsListBox_MouseDown(object sender, EventArgs e)
		{
			isDragging = false;
		}

		internal void AvailableColumnsListBox_MouseUp(object sender, MouseEventArgs e)
		{
			isDragging = false;
		}

		internal void AvailableColumnsListBox_MouseDown(object sender, MouseEventArgs e)
		{
			isDragging = true;
		}

		void AvailableColumnsListBox_ListBoxClearSelected(object sender, EventArgs e)
		{
			CurrentColumnsListBox.Refresh();
		}

		void AvailableColumnsListBox_DragDrop(object sender, DragEventArgs e)
		{
			if (CurrentColumnsListBox.IsDragging)
			{
				RemoveColumns();
			}
		}

		#endregion

		#region Column Editing

		protected override IList<ICustomizableColumn> ResetColumnsCore()
		{
			Result = CreateNewGridColumns(CurrentColumns.Grid);

			foreach (var defaultColumn in DefaultColumns)
			{
				var column = CurrentColumns[defaultColumn.ColumnStyle.MappingName];
				if (column != null)
				{
					var columnToAdd = defaultColumn.Clone();
					columnToAdd.IsVisible = defaultColumn.IsVisible || defaultColumn.IsMandatory;
					var columnStyle = columnToAdd.ColumnStyle;
					columnToAdd.IsCustomColumn = defaultColumn.IsCustomColumn;
					ControlDpiScalingHelper.SetWidth(ref columnStyle, columnToAdd.Width, false);
					Result.AddColumn(columnToAdd);
				}
			}

			return null;
		}

		#endregion

		#region Implementation

		static IList<ICustomizableColumn> ConvertColumnsToGroups(ZGridColumns columns)
		{
			return new ZGridColumnGroupCollection(columns).Cast<ICustomizableColumn>().ToList();
		}

		static ZGridColumns ConvertGroupsToColumns(IEnumerable<ICustomizableColumn> groups, ZGrid grid)
		{
			var result = CreateNewGridColumns(grid);

			foreach (var customizableColumn in groups)
			{
				var group = customizableColumn as ZGridColumnGroup;
				if (group != null)
				{
					foreach (var column in group.Columns)
					{
						column.IsVisible = group.IsVisible;
						result.AddColumn(column);
					}
				}
			}

			return result;
		}

		static ZGridColumns CreateNewGridColumns(ZGrid grid)
		{
			return new ZGridColumns(grid);
		}

		#endregion

		#region Manage & Save Layouts Event Handlers

		protected override IModifyModuleAndGridLayout GetGridLayoutManageableCore()
		{
			return new ZGridLayoutModification(CurrentColumns.Grid, Result, CustomiseBizObj.Factory, CustomiseBizObj.CurrentLayout);
		}

		protected override ZGuid LayoutContextPK
		{
			get { return CurrentColumns.Grid.LayoutCategoryPK; }
		}

		protected override void UpdateCurrentColumnsVisiblity(IList<string> visibleColumnNames)
		{
			foreach (var column in CurrentColumns)
			{
				column.IsVisible = visibleColumnNames.Any(deserialisedColumnName => column.ColumnComparer(deserialisedColumnName));
			}

			var index = 0;
			foreach (var columnName in visibleColumnNames)
			{
				var column = CurrentColumns[columnName];
				if (column != null)
				{
					CurrentColumns.Move(column, index++);
				}
			}

			SetNewCurrentColumns(ConvertColumnsToGroups(CurrentColumns));
		}

		#endregion

		#region PerformConversionFromLegacyKeyToControlIDKey

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "trying to fix a conflict and very rare")]
		void PerformConversionFromLegacyKeyToControlIDKeyIfRequired()
		{
			var grid = CurrentColumns.Grid;

			if (!string.IsNullOrEmpty(grid.GridId) && grid.IsGridLayoutConfigurable)
			{
				var legacyKey = new LegacyDataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter;

				var factory = CustomiseBizObj.Factory;
				var layouts = new StmModuleFilter.Loader(factory).FindByID(legacyKey);

				if (layouts.Any())
				{
					var controlIDKey = new DataGridLayoutContextKeyProvider(grid, false).ContextKeyForStmModuleFilter;
					var controlIDKeyWithCategoryPK = new DataGridLayoutContextKeyProvider(grid, true).ContextKeyForStmModuleFilter;
					foreach (var layout in layouts)
					{
						var visibleColumns = GetVisibleColumnNames(factory, layout).ToArray();

						if (grid.QueryHasNoOtherCustomisedColumns != null && grid.QueryHasNoOtherCustomisedColumns.Invoke(visibleColumns))
						{
							if (grid.QueryHasCustomisedColumns != null && grid.QueryHasCustomisedColumns.Invoke(visibleColumns))
							{
								//if the layout has the custom columns the grid has and no others, then we make the module id org specific
								layout.S9_ModuleID = controlIDKeyWithCategoryPK;
							}
							else
							{
								//if the layout has no custom columns, then we don't make the module id org specific
								layout.S9_ModuleID = controlIDKey;
							}
						}
						//if the layout has custom columns not in this grid, then we leave it for now
						//its module id can't be set until its grid is loaded
					}

					var max_retries = 10;
					for (var retries = 0; retries < max_retries; ++retries)
					{
						try
						{
							factory.Save();
							break;
						}
						catch (ZSaveException)
						{
							//(shouldn't ever happen except under extremely contrived circumstances)
							if (retries >= max_retries - 1)
							{
								throw;
							}
							//caused by legacy and new layout having identical names/companies - change name
							foreach (var layout in layouts)
							{
								layout.S9_FilterName += " (Legacy)";
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
