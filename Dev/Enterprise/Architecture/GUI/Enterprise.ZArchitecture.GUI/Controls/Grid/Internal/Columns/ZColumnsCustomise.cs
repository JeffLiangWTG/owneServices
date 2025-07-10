using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZColumnsCustomise : ZChildForm
	{
		public ZColumnsCustomise(IList<ICustomizableColumn> currentColumns, IList<ICustomizableColumn> defaultColumns = null, bool showLayoutsToolstrip = false, ZGridCustomiseBizObj customiseBizObj = null, ZGrid parentGrid = null)
			: base(customiseBizObj)
		{
			CustomiseBizObj = customiseBizObj;
			ParentGrid = parentGrid;
			CurrentColumns = currentColumns;
			DefaultColumns = defaultColumns;
			UpdateColumnsListBoxes();

			if (customiseBizObj != null)
			{
				if (!this.IsDesignMode())
				{
					SetDataBinding(customiseBizObj, "");
				}

				customiseBizObj.CurrentLayoutNameDisplayInfo.ValueChanged += CurrentLayoutNameInfo_ValueChanged;
				GridModuleFilterSavedLayoutUpdatedEvent.AddLayoutUpdatedEventHandler(customiseBizObj.Factory, UpdateFilterDescription);
				GridModuleFilterLayoutDeletedEvent.AddLayoutDeletedEventHandler(customiseBizObj.Factory, ClearCurrentLayoutIfItIsDeleted);
			}

			ResetButton.Visible = DefaultColumns != null;
			ShowLayoutsToolStrip(showLayoutsToolstrip, -35);

			SetupToolBarImages();
		}

		protected ZGrid ParentGrid { get; private set; }
		protected ZGridCustomiseBizObj CustomiseBizObj { get; private set; }
		protected IList<ICustomizableColumn> CurrentColumns { get; private set; }
		protected IList<ICustomizableColumn> DefaultColumns { get; private set; }
		public IList<ICustomizableColumn> Result { get; private set; }

		#region Overrides

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (CustomiseBizObj != null)
					{
						CustomiseBizObj.CurrentLayoutNameDisplayInfo.ValueChanged -= CurrentLayoutNameInfo_ValueChanged;
						GridModuleFilterSavedLayoutUpdatedEvent.RemoveLayoutUpdatedEventHandler(CustomiseBizObj.Factory, UpdateFilterDescription);
						GridModuleFilterLayoutDeletedEvent.RemoveLayoutDeletedEventHandler(CustomiseBizObj.Factory, ClearCurrentLayoutIfItIsDeleted);
						CustomiseBizObj = null;
					}

					AvailableColumnsListBox.MouseDown -= AvailableColumns_MouseDown;
					CurrentColumnsListBox.MouseDown -= CurrentColumns_MouseDown;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();
			MainStatusBar.Dispose();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Events

		private void AddButton_Click(object sender, EventArgs e)
		{
			AddColumns();
		}

		private void AvailableColumns_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2)
			{
				AddColumns();
			}
		}

		private void RemoveButton_Click(object sender, EventArgs e)
		{
			RemoveColumns();
		}

		private void CurrentColumns_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 2)
			{
				RemoveColumns();
			}
		}

#if DEBUG
		internal void FirePostButtonForTest()
		{
			PostButton_Click(this, EventArgs.Empty);
		}
#endif

		private void PostButton_Click(object sender, EventArgs e)
		{
			if (CurrentColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("826123a7-c696-4996-aaf5-5e859d56b857", "At least one column must be shown in the current grid."));
				DialogResult = DialogResult.Cancel;
				return;
			}

			var allSubmissive = true;
			foreach (ICustomizableColumn column in CurrentColumnsListBox.Items)
			{
				var submissiveColumn = column as ISubmissiveColumn;
				allSubmissive = submissiveColumn != null && (submissiveColumn.IsSubmissive);
				if (!allSubmissive)
				{
					break;
				}
			}
			if (allSubmissive)
			{
				Globals.Message.ShowError(Res.GetString("f0d40a99-e739-4944-bc52-7c1d7ff146f3", "Selected columns cannot be shown by themselves. Use them with some other columns."));
				DialogResult = DialogResult.Cancel;
				return;
			}

			SaveColumns();
			Close();
		}

		private void ResetButton_Click(object sender, EventArgs e)
		{
			ResetColumns();
		}

		private void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void CustomColumnsCheckBox_Checked(object sender, EventArgs e)
		{
			UpdateCustomColumnsVisibility();
			ColumnSearchBox_TextChanged(null, null);
		}

		private void MoveUpButton_Click(object sender, EventArgs e)
		{
			MoveColumnUp();
		}

		private void MoveDownButton_Click(object sender, EventArgs e)
		{
			MoveColumnDown();
		}

		private void FormSizeChanged(object sender, EventArgs e)
		{
			var yLocationAlignment = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(this.PostButton.Location.X, this.CurrentColumnsListBox.Location.Y + this.CurrentColumnsListBox.Size.Height + yLocationAlignment, false);
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(this.Cancel_Button.Location.X, this.CurrentColumnsListBox.Location.Y + this.CurrentColumnsListBox.Size.Height + yLocationAlignment, false);
			this.ToolStripLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(this.ToolStripLayout.Location.X, this.CurrentColumnsListBox.Location.Y + this.CurrentColumnsListBox.Size.Height + yLocationAlignment, false);
			this.ResetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(this.ResetButton.Location.X, this.CurrentColumnsListBox.Location.Y + this.CurrentColumnsListBox.Size.Height - this.ResetButton.Size.Height, false);
			this.label4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(this.label4.Location.X, this.CurrentColumnsListBox.Location.Y + this.CurrentColumnsListBox.Size.Height + 10, false);
			this.label5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(this.label5.Location.X, this.CurrentColumnsListBox.Location.Y + this.CurrentColumnsListBox.Size.Height + 30, false);
		}
		#endregion

		#region Edit Columns

		protected void AddColumns()
		{
			if (AvailableColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("85d96660-269b-43c7-9c24-ce392fd33641", "There are no columns to add from the available columns list."));
			}
			else if (AvailableColumnsListBox.SelectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("dfbc2d20-b8d0-4653-b403-bd2eef4be773", "No columns are selected to add from the available columns list."));
			}
			else
			{
				CurrentColumnsListBox.ClearSelected();

				for (var i = 0; i < AvailableColumnsListBox.Items.Count; i++)
				{
					if (AvailableColumnsListBox.SelectedIndices.Contains(i))
					{
						var column = (ICustomizableColumn)AvailableColumnsListBox.Items[i];
						if (CanAddColumn(column))
						{
							CurrentColumnsListBox.Items.Add(column);
							CurrentColumnsListBox.SetSelected(CurrentColumnsListBox.Items.IndexOf(column), true);
							AvailableColumnsListBox.Items.Remove(column);
							i--;
						}
					}
				}

				InvalidateCurrentColumnLayoutNameIfRequired();
			}
		}

		protected virtual bool CanAddColumn(ICustomizableColumn column)
		{
			return true;
		}

		protected void RemoveColumns()
		{
			if (CurrentColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("5e2e4061-a0f1-4dba-9f75-0fea8791ade4", "There are no columns to remove from the current columns list."));
			}
			else if (CurrentColumnsListBox.SelectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("043425a8-d1dc-4f78-9e40-e7f8179f8dc7", "No columns are selected to remove from the current columns list."));
			}
			else
			{
				for (var i = 0; i < CurrentColumnsListBox.Items.Count; i++)
				{
					if (CurrentColumnsListBox.SelectedIndices.Contains(i))
					{
						var column = (ICustomizableColumn)CurrentColumnsListBox.Items[i];
						if (CanRemoveColumn(column))
						{
							AvailableColumnsListBox.Items.Add(column);
							AvailableColumnsListBox.SetSelected(AvailableColumnsListBox.Items.IndexOf(column), true);
							CurrentColumnsListBox.Items.Remove(column);
							i--;
						}
					}
				}

				InvalidateCurrentColumnLayoutNameIfRequired();
			}
		}

		protected virtual bool CanRemoveColumn(ICustomizableColumn column)
		{
			if (column.IsMandatory)
			{
				Globals.Message.ShowError(Res.GetString("c66b3855-3553-4f8f-81b0-b877842293bf", "This column is mandatory and cannot be removed: {0}.", column.ToString()));
				return false;
			}
			return true;
		}

		protected void ResetColumns()
		{
			var result = Globals.Message.Show(Res.GetString("1b9fd16b-5cdd-4e46-ac0b-1f4e3705b915", "This will reset the columns back to their default layout and widths. Do you wish to continue?"), Res.GetString("baaefa03-0296-4637-9ea9-8b9ab5e94086", "Column Reset"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No);
			if (result == DialogResult.Yes)
			{
				Result = ResetColumnsCore();

				InvalidateCurrentColumnLayoutNameIfRequired();
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		protected virtual IList<ICustomizableColumn> ResetColumnsCore()
		{
			var result = new List<ICustomizableColumn>();

			foreach (var defaultColumn in DefaultColumns)
			{
				var column = CurrentColumns.FirstOrDefault(element => element.ColumnName == defaultColumn.ColumnName);
				if (column != null)
				{
					column.IsVisible = defaultColumn.IsVisible;
					result.Add(column);
				}
			}

			return result;
		}

		protected void MoveColumnUp()
		{
			if (CurrentColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("32a890a0-a597-428e-8a44-9a1286c42871", "There are no columns to move in the current columns list."));
			}
			else if (CurrentColumnsListBox.SelectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("d9af7438-1288-4d21-beed-4f7f0dd0211c", "No columns are selected to move in the current columns list."));
			}
			else if (CurrentColumnsListBox.SelectedIndices.Count > 1)
			{
				Globals.Message.ShowError(Res.GetString("7dc5585c-c549-438a-b92d-afacbdf58558", "Please select only one column to move in the current columns list."));
			}
			else
			{
				if (CurrentColumnsListBox.SelectedIndex > 0)
				{
					var selectedItem = CurrentColumnsListBox.SelectedItem;
					var selectedItemIndex = CurrentColumnsListBox.SelectedIndex;

					CurrentColumnsListBox.Items.Remove(CurrentColumnsListBox.SelectedItem);
					CurrentColumnsListBox.Items.Insert(selectedItemIndex - 1, selectedItem);
					CurrentColumnsListBox.SelectedIndex = selectedItemIndex - 1;

					InvalidateCurrentColumnLayoutNameIfRequired();
				}
			}
		}

		protected void MoveColumnDown()
		{
			if (CurrentColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("32a890a0-a597-428e-8a44-9a1286c42871", "There are no columns to move in the current columns list."));
			}
			else if (CurrentColumnsListBox.SelectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("d9af7438-1288-4d21-beed-4f7f0dd0211c", "No columns are selected to move in the current columns list."));
			}
			else if (CurrentColumnsListBox.SelectedIndices.Count > 1)
			{
				Globals.Message.ShowError(Res.GetString("7dc5585c-c549-438a-b92d-afacbdf58558", "Please select only one column to move in the current columns list."));
			}
			else
			{
				if (CurrentColumnsListBox.SelectedIndex < CurrentColumnsListBox.Items.Count - 1)
				{
					var selectedItem = CurrentColumnsListBox.SelectedItem;
					var selectedItemIndex = CurrentColumnsListBox.SelectedIndex;

					CurrentColumnsListBox.Items.Remove(CurrentColumnsListBox.SelectedItem);
					CurrentColumnsListBox.Items.Insert(selectedItemIndex + 1, selectedItem);
					CurrentColumnsListBox.SelectedIndex = selectedItemIndex + 1;

					InvalidateCurrentColumnLayoutNameIfRequired();
				}
			}
		}

		#endregion

		#region Manage & Save Layouts Event Handlers

		public static IEnumerable<string> GetVisibleColumnNames(BusinessObjectFactory factory, IGridLayoutStorage filter)
		{
			if (filter != null && filter.ColumnLayoutData != null && filter.ColumnLayoutData.Length > 0)
			{
				using (var memoryStream = new MemoryStream(filter.ColumnLayoutData))
				{
					foreach (var columnName in new DataGridLayoutDataSetSerialiser().GetVisibleColumnNames(memoryStream))
					{
						yield return columnName;
					}
				}
			}
		}

		public static IEnumerable<ICustomizableColumn> GetVisibleColumns(BusinessObjectFactory factory, IGridLayoutStorage filter, IEnumerable<ICustomizableColumn> allColumns)
		{
			if (filter != null && filter.ColumnLayoutData != null && filter.ColumnLayoutData.Length > 0)
			{
				using (var memoryStream = new MemoryStream(filter.ColumnLayoutData))
				{
					foreach (var columnName in new DataGridLayoutDataSetSerialiser().GetVisibleColumnNames(memoryStream))
					{
						foreach (var defaultColumn in allColumns)
						{
							var gridColumn = defaultColumn as ZGridColumn;

							if ((gridColumn == null ? defaultColumn.ColumnName == columnName : gridColumn.ColumnComparer(columnName)) ||
								string.IsNullOrEmpty(defaultColumn.ColumnName) && defaultColumn.ToString() == columnName)
							{
								defaultColumn.IsVisible = true;
								yield return defaultColumn;
								break;
							}
						}
					}
				}
			}
		}

		void ToolStripManageLayoutsButton_Click(object sender, EventArgs e)
		{
			var gridLayoutManageable = GetGridLayoutManageable();
			if (gridLayoutManageable != null)
			{
				ZFormModaliser.Show(new ManageLayoutsForm(gridLayoutManageable, false, false, false), this);
			}
		}

#if DEBUG
		internal void FireSaveLayoutButtonForTest()
		{
			ToolStripSaveLayoutButton_Click(ToolStripSaveLayoutButton, EventArgs.Empty);
		}
#endif

		void ToolStripSaveLayoutButton_Click(object sender, EventArgs e)
		{
			if (CurrentColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("826123a7-c696-4996-aaf5-5e859d56b857", "At least one column must be shown in the current grid."));
				DialogResult = DialogResult.Cancel;
				return;
			}

			var gridLayoutManageable = GetGridLayoutManageable();
			if (gridLayoutManageable != null)
			{
				var saveLayoutBizO = GetSaveLayoutBizOAfterQueryingUser(gridLayoutManageable);
				if (saveLayoutBizO != null)
				{
					var layout = new DataGridLayoutManager().SavePreconfiguredLayout(
						gridLayoutManageable, saveLayoutBizO.LayoutNameMultilingual.GetUnresolvedString(), saveLayoutBizO.PublishLayout,
						saveLayoutBizO.PublishAcrossAllCompanies,
						saveLayoutBizO.SaveColumnLayout ? SaveColumnLayout.Yes : SaveColumnLayout.No);

					OnLayoutSaved(layout);
				}
			}
		}

		void OnLayoutSaved(StmModuleFilter layout)
		{
			CustomiseBizObj.CurrentLayout = layout;
			CustomiseBizObj.Lookups.ResetLayouts();
		}

		void ClearCurrentLayoutIfItIsDeleted(GridModuleFilterLayoutDeletedEventArgs args)
		{
			if (CustomiseBizObj.CurrentLayoutCached != null && args.layoutPk == CustomiseBizObj.CurrentLayoutCached.PK)
			{
				CustomiseBizObj.CurrentLayout = null;
			}

			if (ParentGrid != null)
			{
				ParentGrid.CurrentColumnLayout = null;
			}

			CustomiseBizObj.Lookups.ResetLayouts();
		}

		void UpdateFilterDescription(GridModuleFilterSavedLayoutUpdatedEventArgs args)
		{
			if (CustomiseBizObj.CurrentLayoutCached != null && args.filter.PK == CustomiseBizObj.CurrentLayoutCached.PK)
			{
				CustomiseBizObj.CurrentLayout = args.filter;
			}

			CustomiseBizObj.Lookups.ResetLayouts();
		}

		void CurrentLayoutNameInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!IsUpdateColumnViewsSuspended && CustomiseBizObj.CurrentLayout != null)
			{
				var layoutStorage = GetGridLayoutStorage();
				if (layoutStorage != null)
				{
					using (var layoutStream = new MemoryStream(layoutStorage.ColumnLayoutData))
					{
						UpdateCurrentColumnsVisiblity(new List<string>(new DataGridLayoutDataSetSerialiser().GetVisibleColumnNames(layoutStream)));
						UpdateColumnsListBoxes();
					}
				}
			}
		}

		protected virtual void UpdateCurrentColumnsVisiblity(IList<string> visibleColumnNames)
		{
			foreach (var column in CurrentColumns)
			{
				column.IsVisible = visibleColumnNames.Contains(column.ColumnName);
			}

			var index = 0;
			foreach (var columnName in visibleColumnNames)
			{
				var column = CurrentColumns.FirstOrDefault(item => item.ColumnName == columnName);
				if (column != null)
				{
					CurrentColumns.Remove(column);
					CurrentColumns.Insert(index++, column);
				}
			}

			SetNewCurrentColumns(CurrentColumns);
		}

		protected IModifyModuleAndGridLayout GetGridLayoutManageable()
		{
			SaveColumns();
			return GetGridLayoutManageableCore();
		}

		protected virtual IModifyModuleAndGridLayout GetGridLayoutManageableCore()
		{
			var layoutKey =
				CustomiseBizObj.GridIDsForStmModuleFilter != null && CustomiseBizObj.GridIDsForStmModuleFilter.Length > 0
					? CustomiseBizObj.GridIDsForStmModuleFilter[0]
					: string.Empty;
			return new ZColumnsLayoutModification(Result, CustomiseBizObj.Factory, CustomiseBizObj.CurrentLayout, layoutKey);
		}

		protected virtual SaveLayoutBizO GetSaveLayoutBizOAfterQueryingUser(IModifyModuleAndGridLayout gridLayoutManageable)
		{
			return new SaveLayoutUserQueryHandler().QueryUser(gridLayoutManageable, false, false);
		}

		protected virtual IGridLayoutStorage GetGridLayoutStorage()
		{
			return CustomiseBizObj.CurrentLayout;
		}

		protected virtual ZGuid LayoutContextPK
		{
			get { return CustomiseBizObj.LayoutContextPK; }
		}

		protected virtual void InvalidateCurrentColumnLayoutNameIfRequired()
		{
			if (CustomiseBizObj != null)
			{
				updateColumnViewsSuspenderIndex++;
				using (new DisposableAction(() => updateColumnViewsSuspenderIndex--))
				{
					CustomiseBizObj.CurrentLayout = null;
				}
			}
		}

#if DEBUG
		protected void SetSearchText(string text)
		{
			columnSearchBox.Text = text;
		}
#endif

		protected void ColumnSearchBox_TextChanged(object sender, EventArgs e)
		{
			var searchResults = string.IsNullOrEmpty(columnSearchBox.Text) ? ColumnsNotInCurrentList : ColumnsNotInCurrentList.Where(column => column.ColumnName.IndexOf(columnSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0);

			AvailableColumnsListBox.Items.Clear();
			AvailableColumnsListBox.Items.AddRange(searchResults.ToArray());
		}

		IEnumerable<ICustomizableColumn> ColumnsNotInCurrentList
		{
			get
			{
				if (CustomColumnsCheckBox.Checked)
				{
					return CurrentColumns.Where(column => !CurrentColumnsListBox.Items.Contains(column));
				}
				else
				{
					return CurrentColumns.Where(column => !CurrentColumnsListBox.Items.Contains(column) && !column.IsCustomColumn);
				}
			}
		}
		#endregion

		#region Implementation

		protected void SetNewCurrentColumns(IList<ICustomizableColumn> columns)
		{
			CurrentColumns = columns;
		}

		protected void UpdateColumnsListBoxes()
		{
			AvailableColumnsListBox.Items.Clear();
			CurrentColumnsListBox.Items.Clear();

			foreach (var column in CurrentColumns)
			{
				if (column.IsVisible)
				{
					CurrentColumnsListBox.Items.Add(column);
				}
				else
				{
					AvailableColumnsListBox.Items.Add(column);
				}
			}
		}

		protected void UpdateCustomColumnsVisibility()
		{
			AvailableColumnsListBox.Items.Clear();

			foreach (var column in CurrentColumns)
			{
				if (!column.IsVisible)
				{
					if (column.IsCustomColumn && !CustomColumnsCheckBox.Checked)
					{
						continue;
					}
					AvailableColumnsListBox.Items.Add(column);
				}
			}
		}

		void ShowLayoutsToolStrip(bool showLayoutsToolstrip, int moveBy)
		{
			ToolStripLayout.Visible = showLayoutsToolstrip;
			CurrentLayoutNameDropEdit.Visible = showLayoutsToolstrip;
			label3.Visible = showLayoutsToolstrip;

			if (!showLayoutsToolstrip)
			{
				this.columnSearchBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(columnSearchBox.Location.X, columnSearchBox.Location.Y + moveBy, false);
				this.CustomColumnsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(CustomColumnsCheckBox.Location.X, CustomColumnsCheckBox.Location.Y + moveBy, false);
				this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(label1.Location.X, label1.Location.Y + moveBy, false);
				this.AvailableColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(AvailableColumnsListBox.Location.X, AvailableColumnsListBox.Location.Y + moveBy, false);
				this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(AddButton.Location.X, AddButton.Location.Y + moveBy, false);
				this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(RemoveButton.Location.X, RemoveButton.Location.Y + moveBy, false);
				this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(label2.Location.X, label2.Location.Y + moveBy, false);
				this.CurrentColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(CurrentColumnsListBox.Location.X, CurrentColumnsListBox.Location.Y + moveBy, false);
				this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(MoveUpButton.Location.X, MoveUpButton.Location.Y + moveBy, false);
				this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(MoveDownButton.Location.X, MoveDownButton.Location.Y + moveBy, false);

				this.AvailableColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(this.AvailableColumnsListBox.Width, this.AvailableColumnsListBox.Height - moveBy, false);
				this.CurrentColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(this.CurrentColumnsListBox.Width, this.CurrentColumnsListBox.Height - moveBy, false);
			}
		}

		void SetupToolBarImages()
		{
			SetupToolBarButton(ToolStripSaveLayoutButton, IconTypes.SaveButtonRest, IconTypes.SaveButtonActive);
			SetupToolBarButton(ToolStripManageLayoutsButton, IconTypes.ManageButtonRest, IconTypes.ManageButtonActive);
		}

		static void SetupToolBarButton(ToolStripItem button, IconTypes restImage, IconTypes activeImage)
		{
			button.Image = Icons.GetImage(restImage);
			button.MouseEnter += delegate
			{ button.Image = Icons.GetImage(activeImage); };
			button.MouseLeave += delegate
			{ button.Image = Icons.GetImage(restImage); };
		}

		protected void SaveColumns()
		{
			Result = new List<ICustomizableColumn>();

			CustomColumnsCheckBox.Checked = true;
			foreach (ICustomizableColumn column in CurrentColumnsListBox.Items)
			{
				column.IsVisible = true;
				Result.Add(column);
			}

			foreach (ICustomizableColumn column in ColumnsNotInCurrentList)
			{
				column.IsVisible = false;
				Result.Add(column);
			}
		}

		bool IsUpdateColumnViewsSuspended
		{
			get { return updateColumnViewsSuspenderIndex > 0; }
		}
		int updateColumnViewsSuspenderIndex;

		#endregion
	}
}
