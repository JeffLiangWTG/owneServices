using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class DataExportWizardForm : ZChildForm
	{
		static readonly string contextPrefix = "DEW:"; // Programmatic constant

		public DataExportWizardForm()
		{
		}

		public DataExportWizardForm(IExportCollectionInfo collectionInfo, string contextKey)
			: base(new ExportWizard(collectionInfo, new StmModuleFilterSettingsStorage(contextPrefix, contextKey), new FileMapper()))
		{
			ToGrid.AfterBind += ToGrid_AfterBind;
			ToGrid.ContextMenu.Popup += ContextMenu_Popup;
			SetButtons();
			Wizard.FileNameExpressionObjectValueChanged += new ExportWizard.FileNameExpressionObjectValueChangedEventHandler(Wizard_FileNameExpressionObjectValueChanged);

			this.columnHeader1.Text = Res.GetString("a5fbc147-cef2-4002-8b0e-4d3e3c7cc517", "Column");
		}

		ExportWizard Wizard
		{
			get { return (ExportWizard)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void DisposeWizard()
		{
			var disposable = BusinessEntity as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}

		#region Navigation

		void PreviousButton_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedIndex > 0)
			{
				MainTabControl.SelectedIndex--;
			}
		}

		void NextButton_Click(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedIndex < MainTabControl.TabCount - 1)
			{
				MainTabControl.SelectedIndex++;
			}
			else if (MainTabControl.SelectedIndex == MainTabControl.TabCount - 1)
			{
				if (ValidateWizard())
				{
					if (DoExport())
					{
						DialogResult = isCancelled ? DialogResult.Cancel : DialogResult.OK;
						Close();
					}
				}
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void MainTabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (MainTabControl.TabPages[MainTabControl.SelectedIndex] == PreviewTabPage)
			{
				LoadPreview();
			}
			else if (MainTabControl.TabPages[MainTabControl.SelectedIndex] == MappingTabPage)
			{
				if (toGridIsBound)
				{
					UpdateToGridColumnVisibility();
				}
			}

			SetButtons();
		}

		void SetButtons()
		{
			var firstButton = MainTabControl.SelectedIndex == 0;
			var lastButton = MainTabControl.SelectedIndex == MainTabControl.TabCount - 1;

			PreviousButton.Enabled = !firstButton;
			PreviousButton.Text = Res.GetString("e6195005-c7f1-4d99-957e-93cef9742b1e", "Previous");
			NextButton.Text = lastButton ? Res.GetString("e41b4c5c-780d-49a0-8315-589d32db10cc", "Finish") : Res.GetString("cca25294-b7be-4103-8ad3-acf64c0b8bfd", "Next");
		}

		void ToGrid_AfterBind(object sender, EventArgs e)
		{
			UpdateToGridColumnVisibility();
			toGridIsBound = true;
		}
		bool toGridIsBound;

		void UpdateToGridColumnVisibility()
		{
			var refresh = false;

			if (ToGrid.Columns["Width"].IsVisible != Wizard.FixedWidth || ToGrid.Columns["AlignmentString"].IsVisible != Wizard.FixedWidth)
			{
				ToGrid.Columns["Width"].IsVisible = Wizard.FixedWidth;
				ToGrid.Columns["AlignmentString"].IsVisible = Wizard.FixedWidth;
				refresh = true;
			}

			var hasExpressions = (from m in Wizard.Mapping.Cast<ExportWizardMapping>() where String.IsNullOrEmpty(m.MappingName) select m).Any();
			if (ToGrid.Columns["Expression"].IsVisible != hasExpressions)
			{
				ToGrid.Columns["Expression"].IsVisible = hasExpressions;
				refresh = true;
			}

			if (refresh)
			{
				ToGrid.RefreshTableStyles();
			}
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var wizard = (ExportWizard)dataSource;
			if (wizard != null)
			{
				FromListView.VirtualListSize = wizard.FilteredProperties.Count;

				if (wizard.CollectionInfo.RowTypes.Count() < 2)
				{
					rowTypeDropEdit.Enabled = false;
					rowTypeDropEdit.Visible = false;
				}

				DataBindings.RemoveBinding(nameof(RowTypeNameFilter));

				if (wizard.FileNameExpressionObject == null)
				{
					FileNameExpressionTextBox.Enabled = false;
					FileNameExpressionTextBox.Visible = false;
				}
			}

			base.SetDataBinding(dataSource, dataMember);

			if (wizard != null)
			{
				DataBindings.Add(new KBinding(nameof(RowTypeNameFilter), wizard, "RowTypeNameFilter"));
			}
		}

		#endregion

		#region BrowseButton

		void BrowseButton_Click(object sender, EventArgs e)
		{
			var dialog = new ZSaveFileDialog();
			dialog.FileName = Wizard.FileName;
			dialog.Filter = FileFilter;
			dialog.FilterIndex = Wizard.FilterIndex;

			var dr = dialog.ShowDialog(this);
			if (dr == DialogResult.OK)
			{
				Wizard.FileName = dialog.UnmappedFileName;
				Wizard.FilterIndex = dialog.FilterIndex;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension Filter")]
		const string FileFilter = "Text Files (*.csv;*.txt)|*.csv;*.txt|Excel Files (*.xls)|*.xls|All files (*.*)|*.*";

		#endregion

		#region FromListView

		void FromListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			ListViewItem result;
			if (e.ItemIndex < Wizard.FilteredProperties.Count)
			{
				var importPropertyInfo = Wizard.FilteredProperties[e.ItemIndex];
				result = new ListViewItem(importPropertyInfo.HeaderText);
				result.SubItems.Add(importPropertyInfo.MappingName);
			}
			else
			{
				result = new ListViewItem();
				result.SubItems.Add("");
			}
			e.Item = result;
		}

		public ZString RowTypeNameFilter
		{
			get { return rowTypeNameFilter; }
			set
			{
				rowTypeNameFilter = value;
				FromListView.VirtualListSize = Wizard.FilteredProperties.Count;
				FromListView.Refresh();
			}
		}
		ZString rowTypeNameFilter;

		#endregion

		#region CustomMapButton

		void CustomMapButton_Click(object sender, EventArgs e)
		{
			using (Form customMapListsForm = new CustomMapListsForm(Wizard))
			{
				customMapListsForm.ShowDialog(this);
			}
		}

		#endregion

		#region ClearMapButton

		void ClearMapButton_Click(object sender, EventArgs e)
		{
			foreach (ExportWizardMapping m in ToGrid.SelectedElements)
			{
				Wizard.Mapping.RemoveAndDelete(m);
			}
		}

		#endregion

		#region Move Up/Down

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			MoveSelectedElementsUpDown(ExportWizardMappingCollectionView.Direction.Up);
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			MoveSelectedElementsUpDown(ExportWizardMappingCollectionView.Direction.Down);
		}

		void MoveSelectedElementsUpDown(ExportWizardMappingCollectionView.Direction direction)
		{
			var selectedElements = Array.ConvertAll(ToGrid.SelectedElements, se => (ExportWizardMapping)se);

			Wizard.MappingView.MoveSelectedElementsUpDown(selectedElements, direction);

			for (var i = 0; i < Wizard.MappingView.Count; i++)
			{
				if (Array.IndexOf(selectedElements, Wizard.MappingView[i]) >= 0)
				{
					ToGrid.Select(i);
				}
			}
		}

		#endregion

		#region Expressions

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (addExpressionMenuItem == null)
			{
				var contextMenu = (ContextMenu)sender;
				addExpressionMenuItem = new ZMenuItem(ResString.GetMultilingualString("20ecca01-4054-461b-ae9f-7e395e450019", "Add Expression"), ContextMenu_AddExpression);
				contextMenu.MenuItems.Add(0, new ZMenuItem("-"));
				contextMenu.MenuItems.Add(0, addExpressionMenuItem);
			}

			addExpressionMenuItem.Enabled = GetRowType() != null;
		}
		MenuItem addExpressionMenuItem;

		void ContextMenu_AddExpression(object sender, EventArgs e)
		{
			var rowType = GetRowType();
			if (rowType != null)
			{
				Wizard.Mapping.Add(new ExportWizardMapping(rowType, Wizard.Mapping));
				UpdateToGridColumnVisibility();
			}
		}

		RowType GetRowType()
		{
			return (from rowType in Wizard.CollectionInfo.RowTypes where rowType.Name == Wizard.RowTypeNameFilter select rowType).FirstOrDefault();
		}

		#endregion

		#region Drag-n-Drop

		void FromListView_ItemDrag(object sender, ItemDragEventArgs e)
		{
			DoDragDrop(e.Item, DragDropEffects.Move);
		}

		void FromListView_DragEnter(object sender, DragEventArgs e)
		{
			var arrayList = e.Data.GetData(typeof(ArrayList)) as ArrayList;
			if (arrayList != null && arrayList.Count > 0)
			{
				var m = arrayList[0] as ExportWizardMapping;
				if (m != null)
				{
					e.Effect = DragDropEffects.Move;
				}
			}
		}

		void FromListView_DragDrop(object sender, DragEventArgs e)
		{
			var arrayList = e.Data.GetData(typeof(ArrayList)) as ArrayList;
			if (arrayList != null)
			{
				foreach (var o in arrayList)
				{
					var m = o as ExportWizardMapping;
					if (m != null)
					{
						Wizard.Mapping.RemoveAndDelete(m);
					}
				}
			}
		}

		void ToGrid_DragEnter(object sender, DragEventArgs e)
		{
			var listViewItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
			if (listViewItem != null && listViewItem.ListView == FromListView)
			{
				e.Effect = DragDropEffects.Move;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		void ToGrid_DragDrop(object sender, DragEventArgs e)
		{
			var listViewItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
			if (listViewItem != null && listViewItem.ListView == FromListView)
			{
				Wizard.Mapping.Add(new ExportWizardMapping(Wizard.FilteredProperties[listViewItem.Index], Wizard.Mapping));
			}
		}

		#endregion

		#region DoExport

		bool DoExport()
		{
			string errorMessage = null;
			var businessObjects = Wizard.CollectionInfo.BusinessObjects;
			if (businessObjects != null)
			{
				var saveSettings = !Wizard.IsSettingSystemSetting;

				if (saveSettings && Wizard.HasSettingsChanges())
				{
					var message = Res.GetString("2dca3339-4dea-4886-ab52-b9d97db6f8bc", "The settings '{0}' already exist.\r\n\r\nDo you want to overwrite the existing settings?", Wizard.Setting);
					var result = Globals.Message.Show(message, FormCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					if (result == DialogResult.Cancel)
					{
						return false;
					}
					else if (result == DialogResult.No)
					{
						saveSettings = false;
					}
				}

				var cursorBefore = Cursor;
				try
				{
					Cursor = Cursors.WaitCursor;
					Wizard.ProgressChanged += Wizard_ProgressChanged;
					if (Wizard.ExportCollection(businessObjects, out errorMessage))
					{
						if (saveSettings)
						{
							Wizard.SaveSettings();
						}
					}
				}
				finally
				{
					Wizard.ProgressChanged -= Wizard_ProgressChanged;
					if (exportProgressForm != null)
					{
						exportProgressForm.Cancelled -= ExportProgressForm_Cancelled;
						exportProgressForm.Dispose();
						exportProgressForm = null;
					}
					Cursor = cursorBefore;
				}
			}

			if (errorMessage != null)
			{
				Globals.Message.ShowError(errorMessage);
				return false;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region Validation

		bool ValidateWizard()
		{
			var validationFailed = false;

			Wizard.RunPreSaveValidation();
			if (Wizard.HasErrors)
			{
				validationFailed = true;
				ShowErrorsDialog();
			}

			return !validationFailed;
		}

		#endregion

		#region Progress Form

		bool Wizard_ProgressChanged(int percentComplete, string status)
		{
			if (exportProgressForm == null)
			{
				isCancelled = false;
				exportProgressForm = new ProgressForm();
				exportProgressForm.TopMost = true;
				exportProgressForm.Cancelled += ExportProgressForm_Cancelled;
				exportProgressForm.Show();
			}

			if (exportProgressForm.PercentComplete != percentComplete)
			{
				exportProgressForm.SetStatusAndPercentComplete(status, percentComplete);
			}

			return !isCancelled;
		}

		void ExportProgressForm_Cancelled(object sender, EventArgs e)
		{
			isCancelled = true;
		}

		bool isCancelled;
		ProgressForm exportProgressForm;

		#endregion

		#region Preview

		void LoadPreview()
		{
			Wizard.LoadPreview();

			PreviewListView.Items.Clear();
			PreviewListView.Columns.Clear();

			if (Wizard.Preview != null && Wizard.Preview.GetLength(0) > 0 && Wizard.Preview.GetLength(1) > 0)
			{
				using (var gr = CreateGraphics())
				{
					var columnWidths = new int[Wizard.Preview.GetLength(1)];
					for (var i = 0; i < Wizard.Preview.GetLength(0); i++)
					{
						for (var j = 0; j < Wizard.Preview.GetLength(1); j++)
						{
							columnWidths[j] = Math.Max(columnWidths[j], gr.MeasureString(Wizard.Preview[i, j], PreviewListView.Font).ToSize().Width);
						}
					}

					foreach (var columnWidth in columnWidths)
					{
						PreviewListView.Columns.Add("", columnWidth + 10);
					}
				}

				for (var i = 0; i < Wizard.Preview.GetLength(0); i++)
				{
					var item = PreviewListView.Items.Add(Wizard.Preview[i, 0]);
					for (var j = 1; j < Wizard.Preview.GetLength(1); j++)
					{
						item.SubItems.Add(Wizard.Preview[i, j]);
					}
				}
			}
		}

		#endregion

		#region PropertyDescriptors

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<DataExportWizardForm>()
				.Property("RowTypeNameFilter", ZString.Empty, false)
				.Result;
		}

		#endregion

		#region Handlers

		void Wizard_FileNameExpressionObjectValueChanged(object sender, EventArgs e)
		{
			FileNameExpressionTextBox.Enabled = Wizard.FileNameExpressionObject != null;
			FileNameExpressionTextBox.Visible = Wizard.FileNameExpressionObject != null;
		}

		#endregion
	}
}
