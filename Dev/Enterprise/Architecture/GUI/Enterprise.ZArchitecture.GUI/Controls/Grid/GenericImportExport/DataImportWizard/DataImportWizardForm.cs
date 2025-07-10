using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider)), System.Runtime.InteropServices.Guid("970EF996-8A03-4DBF-8400-CC39C2E2E273")]
	public partial class DataImportWizardForm : ZChildForm
	{
		readonly ZString moduleName;
		public event EventHandler Imported;
		public event EventHandler Cancelled;

		public DataImportWizardForm()
		{
		}

		public DataImportWizardForm(ImportWizardFactory wizardFactory, IImportCollectionInfo collectionInfo, string contextKey, ZString moduleName)
			: base(wizardFactory.New(collectionInfo, contextKey))
		{
			if (!moduleName.IsEmpty)
			{
				this.moduleName = " - " + moduleName;
				this.ShowInTaskbar = true;
			}

			SetButtons();
			Wizard.GenerateReadOnlyWarnings();
			Wizard.UseCurrentCountryNumberFormatting = SystemDataRegistry.Instance.UseCurrentCountryNumberFormatting.Value;
		}

		public DataImportWizardForm(IImportCollectionInfo collectionInfo, string contextKey, ZString moduleName)
			: this(new ImportWizardFactory(), collectionInfo, contextKey, moduleName)
		{
		}

		public DataImportWizardForm(IImportCollectionInfo collectionInfo, string contextKey)
			: this(collectionInfo, contextKey, ZString.Empty)
		{
		}

		protected ImportWizard Wizard
		{
			get { return (ImportWizard)BusinessEntity; }
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

		public override string FormCaption
		{
			get
			{
				return Res.GetString("DataImportWizardForm|FormCaption", "Data Import Wizard") + moduleName;
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
			if (MainTabControl.TabPages[MainTabControl.SelectedIndex] == MappingTabPage)
			{
				if (ValidateWizard())
				{
					MainTabControl.SelectedIndex++;
				}
			}
			else if (MainTabControl.SelectedIndex < MainTabControl.TabCount - 1)
			{
				MainTabControl.SelectedIndex++;
			}
			else if (MainTabControl.SelectedIndex == MainTabControl.TabCount - 1)
			{
				if (Wizard.CollectionInfo.ValidateAndSave)
				{
					if (FireSaveButton() == ContinueWithSave.Yes)
					{
						DialogResult = DialogResult.OK;
						Close();
					}
				}
				else
				{
					if (ValidateWizard())
					{
						BeginInvoke(new Action(delegate
							{
								try
								{
									if (DoImport())
									{
										if (DoProcessing())
										{
											DialogResult = isCancelled ? DialogResult.Cancel : DialogResult.OK;
											Close();
											if (Imported != null)
											{
												Imported(this, new EventArgs());
											}
										}
									}
								}
								catch (System.Data.Common.DbException ex)
								{
									Globals.Message.ShowError(ex.Message);
								}
							}));
					}
				}
			}
		}

		protected virtual bool DoProcessing()
		{
			return true;
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
				Wizard.LoadPreview();
			}
			else if (MainTabControl.TabPages[MainTabControl.SelectedIndex] == ValidateAndSaveTabPage)
			{
				var collection = Wizard.CollectionInfo.Collection;
				for (var i = collection.Count - 1; i >= 0; --i)
				{
					var bizo = (BusinessObject)collection[i];
					if (!bizo.IsInDatabase)
					{
						collection.Delete(bizo);
					}
					else
					{
						((INeedRow)bizo).Row.RejectChanges();
					}
				}

				var result = ValidateWizard();
				if (result)
				{
					result = DoImport();
				}
				if (result)
				{
					collection.RunPreSaveValidation();
				}
				else
				{
					MainTabControl.SelectedIndex = 0;
				}
			}

			SetButtons();
		}

		void SetButtons()
		{
			var firstButton = MainTabControl.SelectedIndex == 0;
			var lastButton = MainTabControl.SelectedIndex == MainTabControl.TabCount - 1;

			PreviousButton.Enabled = !firstButton;
			PreviousButton.Text = Res.GetString("a48a1a15-2ac7-488a-99e3-d13014bc23f3", "Previous");
			NextButton.Text = lastButton ? Res.GetString("1287e400-bbd4-48eb-94cc-a0da2831e737", "Finish") : Res.GetString("b40f17b7-60c1-4762-9fd8-8956530b9f21", "Next");
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var wizard = (ImportWizard)dataSource;
			if (wizard != null)
			{
				wizard.CollectionInfo.OnImportStarted();
				if (wizard.CollectionInfo.ValidateAndSave)
				{
					PopulateGridColumns(wizard, validateAndSaveControl.Grid, "DataImportWizardValidateAndSaveGrid|");
				}
				else
				{
					MainTabControl.TabPages.Remove(ValidateAndSaveTabPage);
				}
				PopulateGridColumns(wizard, PreviewGrid, "DataImportWizardPreviewGrid|");
				DataBindings.RemoveBinding(nameof(FileContent));
			}
			else if (Wizard != null)
			{
				Wizard.CollectionInfo.OnImportCompleted(DialogResult == DialogResult.OK);
			}

			base.SetDataBinding(dataSource, dataMember);
			if (wizard != null)
			{
				DataBindings.Add(new KBinding(nameof(FileContent), wizard, "FileContent"));
			}
		}

		static void PopulateGridColumns(ImportWizard wizard, ZGrid grid, string columnIdPrefix)
		{
			foreach (var property in wizard.CollectionInfo.Properties)
			{
				var styleInfoType = GetColumnInfoType(wizard, property);
				var styleInfo = (ZGridColumnInfo)Activator.CreateInstance(styleInfoType);
				styleInfo.ColumnName = property.MappingName;
				styleInfo.Caption = property.HeaderText;
				ControlDpiScalingHelper.SetWidth(ref styleInfo, property.ColumnWidth, false);
#if DEBUG
				TypeDescriptor.AddAttributes(styleInfo, new SuppressFormsLocalizedTestAttribute());
#endif
				grid.ColumnStyles.Add(styleInfo);
			}
			grid.RefreshTableStyles();
		}

		internal static Type GetColumnInfoType(ImportWizard wizard, IImportPropertyInfo property)
		{
			IList list;
			wizard.BindToLists.TryGetValue(property.MappingName, out list);

			Type styleInfoType;
			if (ImportWizard.IsType(property.PropertyType, typeof(ZByte)) ||
				ImportWizard.IsType(property.PropertyType, typeof(ZShort)) ||
				ImportWizard.IsType(property.PropertyType, typeof(ZInt)) ||
				ImportWizard.IsType(property.PropertyType, typeof(ZDecimal)) ||
				ImportWizard.IsType(property.PropertyType, typeof(ZLong)))
			{
				styleInfoType = typeof(ZCalcEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZString))
				|| ImportWizard.IsType(property.PropertyType, typeof(MultilingualString)))
			{
				if (list != null)
				{
					styleInfoType = typeof(ZDropEditColumnStyleInfo);
				}
				else
				{
					styleInfoType = typeof(ZTextBoxColumnStyleInfo);
				}
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZDateTime)) ||
				ImportWizard.IsType(property.PropertyType, typeof(ZDate)))
			{
				styleInfoType = typeof(ZDateEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZDateTimeOffset)))
			{
				styleInfoType = typeof(ZDateTimeOffsetEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZTime)))
			{
				styleInfoType = typeof(ZTimeEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZGeography)))
			{
				styleInfoType = typeof(ZGeographyEditColumnStyleInfo);
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZGuid)))
			{
				if ((list as IBusinessObjectCollection) != null)
				{
					styleInfoType = typeof(ZGuidFindBoxColumnStyleInfo);
				}
				else
				{
					styleInfoType = typeof(ZGuidDropEditColumnStyleInfo);
				}
			}
			else if (ImportWizard.IsType(property.PropertyType, typeof(ZBool)))
			{
				styleInfoType = typeof(ZCheckBoxColumnStyleInfo);
			}
			else
			{
				throw new NotSupportedException(string.Format("Cannot determine column style info for property {0}.{1} ({2}).", property.ComponentType.FullName, property.MappingName, property.PropertyType.FullName));
			}

			return styleInfoType;
		}

		#endregion

		#region FileContent

		public List<string[]> FileContent
		{
			get { return fileContent; }
			set
			{
				var changed = fileContent != value;
				if (changed)
				{
					fileContent = value;

					PreviewListView.Items.Clear();
					PreviewListView.Columns.Clear();

					FromPrevButton.ReadOnly = true;
					FromNextButton.ReadOnly = true;
					if (fileContent.Count > 0)
					{
						Wizard.CurrentFileContentLine = Math.Min(Wizard.StartingRow, fileContent.Count - 1);
					}
					else
					{
						Wizard.CurrentFileContentLine = 0;
					}

					if (Wizard.FileColumns != null && Wizard.FileColumns.Count > 0)
					{
						using (var gr = CreateGraphics())
						{
							foreach (var columnWidth in Wizard.FileColumns)
							{
								var columnWidthG = gr.MeasureString(new string('A', columnWidth), PreviewListView.Font).ToSize().Width;
								PreviewListView.Columns.Add("", columnWidthG);
							}
						}

						foreach (var values in fileContent)
						{
							if (values.Length < 1)
							{
								continue;
							}

							var item = PreviewListView.Items.Add(values[0]);
							for (var i = 1; i < values.Length; i++)
							{
								item.SubItems.Add(values[i]);
							}
						}

						FromListView.VirtualListSize = Wizard.FileColumns.Count;
					}
					SetFromListView();
				}
			}
		}

		List<string[]> fileContent;

		#endregion

		#region BrowseButton

		void BrowseButton_Click(object sender, EventArgs e)
		{
			var dialog = new ZOpenFileDialog();
			dialog.FileName = Wizard.FileName;
			dialog.Filter = FileFilter;
			dialog.FilterIndex = Wizard.FilterIndex;
			dialog.CheckFileExists = true;

			var result = dialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				try
				{
					Wizard.FileName = dialog.UnmappedFileName;
				}
				catch (ExcelInterfaceExceptionBase ex)
				{
					var caption = Res.GetString("3afe57a1-aca4-4992-8298-9d29501094e6", "Error loading file");
					Globals.Message.ShowError(ex.Message, caption);
				}
				Wizard.FilterIndex = dialog.FilterIndex;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Extension Filter")]
		const string FileFilter = "Text Files (*.csv;*.txt)|*.csv;*.txt|Excel Files (*.xls)|*.xls|All files (*.*)|*.*";

#endregion

		#region FromListView

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
#if DEBUG
		internal
#endif
		void FromListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			var result = new ListViewItem(string.Format("Field[{0}]", e.ItemIndex));
			result.SubItems.Add(Wizard != null ? Wizard.GetFileContentValue(e.ItemIndex) : "");

			e.Item = result;
		}

		void FromPrevButton_Click(object sender, EventArgs e)
		{
			if (Wizard.CurrentFileContentLine > 0)
			{
				Wizard.CurrentFileContentLine--;
				SetFromListView();
			}
		}

		void FromNextButton_Click(object sender, EventArgs e)
		{
			if (Wizard.CurrentFileContentLine < FileContent.Count - 1)
			{
				Wizard.CurrentFileContentLine++;
				SetFromListView();
			}
		}

		void SetFromListView()
		{
			FromPrevButton.ReadOnly = Wizard.CurrentFileContentLine <= 0;
			FromNextButton.ReadOnly = Wizard.CurrentFileContentLine >= fileContent.Count - 1;

			if (FromListView.VirtualListSize > 0)
			{
				FromListView.RedrawItems(0, FromListView.VirtualListSize - 1, true);
			}

			Wizard.Mapping.RefreshBinding();
		}

		#endregion

		#region ClearMapButton

		void ClearMapButton_Click(object sender, EventArgs e)
		{
			foreach (ImportWizardMapping m in ToGrid.SelectedElements)
			{
				m.ClearAllFileColumnIndex();
				m.RefreshBinding();
			}
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
				var m = arrayList[0] as ImportWizardMapping;
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
					var m = o as ImportWizardMapping;
					if (m != null)
					{
						m.ClearAllFileColumnIndex();
						m.RefreshBinding();
					}
				}
			}
		}

		void ToGrid_DragEnter_DragOver(object sender, DragEventArgs e)
		{
			var listViewItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
			if (listViewItem != null && listViewItem.ListView == FromListView && IsOverRow(e) >= 0)
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
			int row, listIndex;
#if WINZOR
			(listIndex, row) = (Tuple<int, int>)e.Data.GetData(typeof(Tuple<int, int>));
#else
			var listViewItem = e.Data.GetData(typeof(ListViewItem)) as ListViewItem;
			if (listViewItem == null || listViewItem.ListView != FromListView)
			{
				return;
			}
			row = IsOverRow(e);
			listIndex = listViewItem.Index;
#endif
			if (row >= 0)
			{
				var m = Wizard.Mapping[row];
				m.AddFileColumnIndex(listIndex);
				m.RefreshBinding();
			}
		}

		int IsOverRow(DragEventArgs e)
		{
			var p = ToGrid.PointToClient(ControlDpiScalingHelper.NewScaledPoint(e.X, e.Y, false));
			var hitTestInfo = ToGrid.HitTest(p);
			if (hitTestInfo.Type == DataGrid.HitTestType.Cell || hitTestInfo.Type == DataGrid.HitTestType.RowHeader)
			{
				return hitTestInfo.Row;
			}

			return -1;
		}

#endregion

		#region DoImport
#if DEBUG
		internal
#endif
		bool DoImport()
		{
			var wizard = this.Wizard ?? throw new InvalidOperationException("Wizard should not be null in DoImport");

			if (wizard.CollectionInfo == null)
			{
				throw new InvalidOperationException("Wizard.CollectionInfo should not be null in DoImport");
			}

			var collection = wizard.CollectionInfo.Collection;
			if (collection != null)
			{
				var saveSettings = !wizard.IsSettingSystemSetting;

				if (saveSettings && wizard.HasSettingsChanges())
				{
					var message = Res.GetString("0b6ea9b0-b3a3-43c5-a3d6-c4db647bfeb3", "The settings '{0}' already exist.\r\n\r\nDo you want to overwrite the existing settings?", wizard.Setting);
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

				using (new ZWaitCursorChanger(this))
				{
					try
					{
						wizard.ProgressChanged += Wizard_ProgressChanged;
						wizard.Saving += Wizard_Saving;
						wizard.SavingComplete += Wizard_SavingComplete;
						if (!collection.AllowNew && !wizard.MatchForUpdateColumns.Any())
						{
							var caption = Res.GetString("9f29d6e2-f297-47fe-94f9-fe3a52a8b8e5", "Error during import");
							Globals.Message.ShowError(Res.GetString("bb268d38-7354-4011-8c6f-44cb6957bf88", "Adding new records is not supported by this collection. Please try to use 'Match for Update' functionality."), caption);

							return false;
						}
						wizard.ImportIntoCollection(collection);
						if (saveSettings)
						{
							wizard.SaveSettings();
						}
					}
					catch (ExcelInterfaceExceptionBase ex)
					{
						var caption = Res.GetString("9f29d6e2-f297-47fe-94f9-fe3a52a8b8e5", "Error during import");
						Globals.Message.ShowError(ex.Message, caption);

						return false;
					}
					finally
					{
						wizard.ProgressChanged -= Wizard_ProgressChanged;
						wizard.Saving -= Wizard_Saving;
						wizard.SavingComplete -= Wizard_SavingComplete;
						DisposeProgressForm();
					}
				}
			}

			return true;
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

		protected bool Wizard_ProgressChanged(int percentComplete, string status)
		{
			if (importProgressForm == null)
			{
				isCancelled = false;
				importProgressForm = CreateProgressForm();
				importProgressForm.TopMost = true;
				importProgressForm.Cancelled += ImportProgressForm_Cancelled;

				if (!importProgressForm.InvokeRequired)
				{
					ZFormModaliser.Show(importProgressForm, this);
				}
				else
				{
					BeginInvoke(new Action(delegate
					{
						ZFormModaliser.Show(importProgressForm, this);
					}));
				}
			}

			if (importProgressForm.PercentComplete != percentComplete)
			{
				importProgressForm.SetStatusAndPercentComplete(status, percentComplete);
			}

			return !isCancelled;
		}

		protected void Wizard_Saving(int percentage, string status)
		{
			if (importProgressForm != null)
			{
				importProgressForm.ShowCancelButton = false;
				importProgressForm.SetStatusAndPercentComplete(status, percentage);
			}
		}

		protected void Wizard_SavingComplete(int percentage, string status)
		{
			if (importProgressForm != null)
			{
				importProgressForm.SetStatusAndPercentComplete(status, percentage);
			}
		}

		protected virtual void DisposeProgressForm()
		{
			if (importProgressForm != null)
			{
				importProgressForm.Cancelled -= ImportProgressForm_Cancelled;
				importProgressForm.Dispose();
				importProgressForm = null;
			}
		}

		protected virtual ProgressForm CreateProgressForm()
		{
			return new ProgressForm();
		}

		protected void ImportProgressForm_Cancelled(object sender, EventArgs e)
		{
			isCancelled = true;
			if (Cancelled != null)
			{
				Cancelled(this, new EventArgs());
			}
		}

		bool isCancelled;
		ProgressForm importProgressForm;

		#endregion

		#region ProperCaseExcludeListButton

		void ProperCaseExcludeListButton_Click(object sender, EventArgs e)
		{
			using (var properCaseExcludeListForm = new ProperCaseExcludeListForm(Wizard))
			{
				properCaseExcludeListForm.ShowDialog(this);
			}
		}

		#endregion

		#region CustomMapListsButton

		void CustomMapListsButton_Click(object sender, EventArgs e)
		{
			using (Form customMapListsForm = new CustomMapListsForm(Wizard))
			{
				customMapListsForm.ShowDialog(this);
			}
		}

		#endregion

		#region PropertyDescriptors

		public new static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<DataImportWizardForm>()
				.Property("FileContent", new List<string[]>(), false)
				.Result;
		}

		#endregion
	}
}
