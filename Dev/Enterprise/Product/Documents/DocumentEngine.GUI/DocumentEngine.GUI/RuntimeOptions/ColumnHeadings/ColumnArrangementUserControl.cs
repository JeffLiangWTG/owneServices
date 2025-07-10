using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class ColumnArrangementUserControl : ZUserControl
	{
		protected readonly ColumnConfigurationsManager headingManager;
		protected readonly Report report;

		public ColumnArrangementUserControl(Report report)
		{
			isInitializing = true;
			this.report = report;
			headingManager = report.ColumnHeadingManager;
			InitializeComponent();
			UpdateListOfSavedNames();
			NotificationExOnTextBox.Initialize(ColumnDisplayNameTextBox);
			NotificationExOnTextBox.Notifications1 = ColumnHeadingNotificationsOnTextBox;
			NotificationExOnLabel.Initialize(ZLabel1);
			NotificationExOnLabel.Notifications1 = ColumnHeadingNotificationsOnLabel;
			NotificationExOnTagNameTextBox.Initialize(ColumnTagNameTextBox);
			NotificationExOnTagNameTextBox.Notifications1 = ColumnHeadingNotificationsOnTagNameTextBox;
			headingManager.CurrentConfigurationRefreshed += headingManager_ColumnHeadingManagerLoaded;

			SettingsComboBox.SelectedIndexChanged += SettingsComboBox_SelectedIndexChanged;
			SetSetting();

			if (headingManager.CurrentConfiguration.Worksheets.IsEmpty)
			{
				ColumnConfigurationPanel.Hide();
				ControlDpiScalingHelper.SetWidth(this, SettingsGroupBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
				ControlDpiScalingHelper.SetHeight(this, SettingsGroupBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(3), false);
			}
			else
			{
				comboBoxConfigWorksheet.DisplayMember = "NameLocalized";
				FillWorkSheetsDropDowns();
				comboBoxConfigWorksheet.SelectedIndex = 0;
			}

			if (!headingManager.IsReportTitleChangeable)
			{
				tabColumnConfig.TabPages.Remove(OptionTabPage);
			}
			isInitializing = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (headingManager != null)
				{
					headingManager.CurrentConfigurationRefreshed -= headingManager_ColumnHeadingManagerLoaded;
				}
				if (SettingsComboBox != null)
				{
					SettingsComboBox.SelectedIndexChanged -= SettingsComboBox_SelectedIndexChanged;
				}
				if (NotificationExOnTextBox != null)
				{
					NotificationExOnTextBox.Dispose();
				}
				if (NotificationExOnLabel != null)
				{
					NotificationExOnLabel.Dispose();
				}
				if (NotificationExOnTagNameTextBox != null)
				{
					NotificationExOnTagNameTextBox.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		NotificationExtension notificationExOnLabel;
		internal NotificationExtension NotificationExOnLabel
		{
			get
			{
				return notificationExOnLabel = notificationExOnLabel ?? new NotificationExtension();
			}
		}

		List<PropertyNotification> columnHeadingNotificationsOnLabel;
		List<PropertyNotification> ColumnHeadingNotificationsOnLabel
		{
			get
			{
				return columnHeadingNotificationsOnLabel = columnHeadingNotificationsOnLabel ?? new List<PropertyNotification>();
			}
		}

		NotificationExtension notificationExOnTextBox;
		internal NotificationExtension NotificationExOnTextBox
		{
			get
			{
				return notificationExOnTextBox = notificationExOnTextBox ?? new NotificationExtension();
			}
		}

		List<PropertyNotification> columnHeadingNotificationsOnTextBox;
		List<PropertyNotification> ColumnHeadingNotificationsOnTextBox
		{
			get
			{
				return columnHeadingNotificationsOnTextBox = columnHeadingNotificationsOnTextBox ?? new List<PropertyNotification>();
			}
		}

		List<PropertyNotification> columnHeadingNotificationsOnTagNameTextBox;
		List<PropertyNotification> ColumnHeadingNotificationsOnTagNameTextBox
		{
			get
			{
				return columnHeadingNotificationsOnTagNameTextBox = columnHeadingNotificationsOnTagNameTextBox ?? new List<PropertyNotification>();
			}
		}

		NotificationExtension notificationExOnTagNameTextBox;
		internal NotificationExtension NotificationExOnTagNameTextBox
		{
			get
			{
				return notificationExOnTagNameTextBox = notificationExOnTagNameTextBox ?? new NotificationExtension();
			}
		}

		ColumnHeading SelectedColumnHeading
		{
			get { return SelectedColumnslistBox.SelectedItem as ColumnHeading; }
		}

		void AddButton_Click(object sender, EventArgs e)
		{
			AddColumns();
		}

		internal void ColumnDisplayNameTextBox_Leave(object sender, EventArgs e)
		{
			UpdateSelectedColumnHeadingText();
		}

		internal void ColumnTagNameTextBox_Leave(object sender, EventArgs e)
		{
			UpdateSelectedColumnTagName();
		}

		void ColumnDisplayWidthCalcEdit_Leave(object sender, EventArgs e)
		{
			UpdateSelectedColumnHeadingWidth();
		}

		void DeleteButton_Click(object sender, EventArgs e)
		{
			DeleteSetting();
		}

		void DeleteSetting()
		{
			string caption = Res.GetString("955f927d-2fa3-4672-9f86-790a1279ca98", "Delete Setting");
			ColumnConfigurationManager setting = SettingsComboBox.SelectedItem as ColumnConfigurationManager;
			if (setting != null)
			{
				if (Globals.Message.Show(Res.GetString("85bc6a21-9697-4200-a3d2-2cf410d7e300", "Do you want to delete the current report settings under \"{0}\"?", setting), caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					setting.Delete();
					UpdateListOfSavedNames();
					SettingsComboBox.SelectedIndex = 0;
				}
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("5dbb893d-9ff8-4d7f-b4d7-68796be4689b", "Please select a setting to delete."), caption);
			}
		}

		void FillWorkSheetsDropDowns()
		{
			comboBoxConfigWorksheet.Items.Clear();

			foreach (Worksheet worksheet in headingManager.CurrentConfiguration.Worksheets)
			{
				var sheetName = report.SheetNames.FirstOrDefault(x => x.StrictName == worksheet.Name);
				if (sheetName != null)
				{
					worksheet.NameLocalized = sheetName.EntireName;
				}
				comboBoxConfigWorksheet.Items.Add(worksheet);
			}
		}

		void FillListboxes()
		{
			AvailableColumnsListBox.Items.Clear();
			IncludedColumnsListBox.Items.Clear();
			SelectedColumnslistBox.Items.Clear();

			bool hasPerformanceWarnings = false;
			foreach (ColumnHeading heading in GetSortedHeadings())
			{
				if (heading.ShowPerformanceWarning)
				{
					hasPerformanceWarnings = true;
				}
				if (!heading.DisplayLabel.IsEmpty)
				{
					if (heading.Hidden)
					{
						AvailableColumnsListBox.Items.Add(heading);
					}
					else
					{
						heading.CurrentPosition = IncludedColumnsListBox.Items.Count;
						IncludedColumnsListBox.Items.Add(heading);
						SelectedColumnslistBox.Items.Add(heading);
					}
				}
			}
			IncludedColumnsListBox.SelectedIndex = IncludedColumnsListBox.Items.Count > 0 ? 0 : -1;
			SelectedColumnslistBox.SelectedIndex = SelectedColumnslistBox.Items.Count > 0 ? 0 : -1;

			if (hasPerformanceWarnings)
			{
				ControlDpiScalingHelper.SetHeight(ref AvailableColumnsListBox, 183, true);
				ControlDpiScalingHelper.SetHeight(ref IncludedColumnsListBox, 183, true);
				ControlDpiScalingHelper.SetTop(ref MoveUpButton, 144, true);
				ControlDpiScalingHelper.SetTop(ref MoveDownButton, 173, true);
			}
			else
			{
				ControlDpiScalingHelper.SetHeight(ref AvailableColumnsListBox, 186, true);
				ControlDpiScalingHelper.SetHeight(ref IncludedColumnsListBox, 186, true);
				ControlDpiScalingHelper.SetTop(ref MoveUpButton, 156, true);
				ControlDpiScalingHelper.SetTop(ref MoveDownButton, 185, true);
			}
			PerformanceWarningLabel.Visible = hasPerformanceWarnings;
		}

		List<ColumnHeading> GetSortedHeadings()
		{
			var result = new List<ColumnHeading>();
			result.AddRange(headingManager.CurrentConfiguration.Worksheets[((Worksheet)comboBoxConfigWorksheet.SelectedItem).Name].ColumnHeadings.ToArray());
			result.Sort((x, y) => x.CurrentPosition.CompareTo(y.CurrentPosition));
			return result;
		}

		void headingManager_ColumnHeadingManagerLoaded(object sender, EventArgs e)
		{
			FillWorkSheetsDropDowns();
			SetSetting();
			if (!headingManager.CurrentConfiguration.Worksheets.IsEmpty)
			{
				comboBoxConfigWorksheet.SelectedIndex = 0;
			}
		}

		void SetSetting()
		{
			if (SettingsComboBox.SelectedItem != headingManager.CurrentColumnConfigurationManager)
			{
				SettingsComboBox.SelectedItem = headingManager.CurrentColumnConfigurationManager;
			}
			if (SettingsComboBox.SelectedItem == null)
			{
				SettingsComboBox.SelectedItem = headingManager.DefaultTemplateConfigurationManager;
			}
		}

		void SelectedColumnslistBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateColumnDisplayControls();
		}

		void comboBoxConfigWorksheet_SelectedIndexChanged(object sender, EventArgs e)
		{
			FillListboxes();
			if (headingManager.IsReportTitleChangeable)
			{
				FillReportTitle();
			}
		}

		#region Edit Columns

		void MoveColumnsAcrossListBoxes(ZListBox source, ZListBox destination, bool hideColumn)
		{
			if (source.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("93e943b1-ef1a-4a82-ac87-f469dee14793", "There are no columns to move from the columns list."));
			}
			else if (source.SelectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("064d97f9-c3e4-41e9-9911-dcf5c0efa2e0", "No columns are selected to move from the columns list."));
			}
			else
			{
				destination.ClearSelected();

				int originalIndex = 0;
				for (var i = 0; i < source.Items.Count; i++)
				{
					if (source.SelectedIndices.Contains(i))
					{
						originalIndex = i;
						var column = (ColumnHeading)source.Items[i];
						column.Hidden = hideColumn;
						destination.Items.Add(column);
						destination.SetSelected(destination.Items.IndexOf(column), true);
						source.Items.Remove(column);
						foreach (ColumnHeading sourceHeading in source.Items)
						{
							if (sourceHeading.CurrentPosition > column.CurrentPosition)
							{
								sourceHeading.CurrentPosition--;
							}
						}
						column.CurrentPosition = destination.Items.Count - 1;
						i--;
					}
				}
				if (source.Items.Count > 0)
				{
					source.SelectedIndex = (originalIndex >= source.Items.Count) ? source.Items.Count - 1 : originalIndex;
				}

				InvalidateCurrentColumnLayoutNameIfRequired();
			}
		}

		void AddColumns()
		{
			MoveColumnsAcrossListBoxes(AvailableColumnsListBox, IncludedColumnsListBox, false);
		}

		void RemoveColumns()
		{
			MoveColumnsAcrossListBoxes(IncludedColumnsListBox, AvailableColumnsListBox, true);
		}

		void MoveColumnPosition(int positionDifference)
		{
			if (IncludedColumnsListBox.Items.Count == 0)
			{
				Globals.Message.ShowError(Res.GetString("fd73b6bb-e03d-46d1-b6c1-89a19c9a14ea", "There are no columns to move in the current columns list."));
			}
			else if (IncludedColumnsListBox.SelectedIndex == -1)
			{
				Globals.Message.ShowError(Res.GetString("ee250c4d-1e11-41be-94d5-11500d067e32", "No columns are selected to move in the current columns list."));
			}
			else
			{
				var selectedIndices = IncludedColumnsListBox.SelectedIndices.Cast<int>().ToList();
				if (positionDifference > 0)
				{
					selectedIndices.Reverse();
				}

				IncludedColumnsListBox.ClearSelected();
				selectedIndices.ForEach(i =>
				{
					var column = (ColumnHeading)IncludedColumnsListBox.Items[i];
					column.CurrentPosition += positionDifference;
					((ColumnHeading)IncludedColumnsListBox.Items[i + positionDifference]).CurrentPosition -= positionDifference;

					IncludedColumnsListBox.Items.Remove(column);
					IncludedColumnsListBox.Items.Insert(i + positionDifference, column);
					IncludedColumnsListBox.SelectedIndices.Add(i + positionDifference);
				});
				InvalidateCurrentColumnLayoutNameIfRequired();
			}
		}

		void MoveColumnUp()
		{
			if (IncludedColumnsListBox.SelectedIndices.Cast<int>().All(i => i > 0))
			{
				MoveColumnPosition(-1);
			}
		}

		void MoveColumnDown()
		{
			if (IncludedColumnsListBox.SelectedIndices.Cast<int>().All(i => i < IncludedColumnsListBox.Items.Count - 1))
			{
				MoveColumnPosition(1);
			}
		}

		protected virtual void InvalidateCurrentColumnLayoutNameIfRequired()
		{
			if (CustomiseBizObj != null)
			{
				CustomiseBizObj.CurrentLayout = null;
			}
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
			IncludedColumnsListBox.Refresh();
		}

		void AvailableColumnsListBox_DragDrop(object sender, DragEventArgs e)
		{
			if (IncludedColumnsListBox.IsDragging)
			{
				RemoveColumns();
			}
		}

		void IncludedColumnsListBox_DragAndDropItemChanged(object sender, EventArgs e)
		{
			InvalidateCurrentColumnLayoutNameIfRequired();

			for (int i = 0; i < IncludedColumnsListBox.Items.Count; i++)
			{
				var column = (ColumnHeading)IncludedColumnsListBox.Items[i];
				column.CurrentPosition = i;
			}
		}

		void IncludedColumnsListBox_PreviewDropItem(object sender, EventArgs e)
		{
			if (isDragging && AvailableColumnsListBox.SelectedIndices.Count > 0)
			{
				AddColumns();
			}
		}

		void IncludedColumnsListBox_DragLeave(object sender, EventArgs e)
		{
			IncludedColumnsListBox.Refresh();
		}

		internal void IncludedColumnsListBox_MouseDown(object sender, MouseEventArgs e)
		{
			isDragging = false;
		}

		protected ZGridCustomiseBizObj CustomiseBizObj { get; private set; }
		internal bool isDragging;

		#endregion

		void RefreshSelectedColumnsListBox()
		{
			SelectedColumnslistBox.Items.Clear();
			foreach (ColumnHeading heading in IncludedColumnsListBox.Items)
			{
				SelectedColumnslistBox.Items.Add(heading);
			}
			SelectedColumnslistBox.SelectedIndex = IncludedColumnsListBox.SelectedIndex;
			if (SelectedColumnslistBox.SelectedIndex == -1)
			{
				UpdateColumnDisplayControls();
			}
		}

		void tabPage2_Enter(object sender, EventArgs e)
		{
			RefreshSelectedColumnsListBox();
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			MoveColumnDown();
		}

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			MoveColumnUp();
		}

		void NewButton_Click(object sender, EventArgs e)
		{
			CreateNewSetting();
		}

		void CreateNewSetting()
		{
			var newConfigName = new NewConfiguration(headingManager);
			DialogResult result = ZFormModaliser.ShowDialogAndDispose(new NewReportConfigurationForm(newConfigName));
			if (result == DialogResult.OK)
			{
				AddNewConfig(newConfigName);
			}
		}

		protected void AddNewConfig(NewConfiguration newConfigName)
		{
			CombinedConfigurationManager manager = newConfigName.NewManager;
			manager.Save(report.FilterCollection, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.Orientation, report.Language);
			//manager.Load(report.FilterCollection, report.GroupByCollection, report.SortOrderCollection);
			UpdateListOfSavedNames();
			SettingsComboBox.SelectedItem = manager;
		}

		void RemoveButton_Click(object sender, EventArgs e)
		{
			RemoveColumns();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			SaveSetting();
		}

		void SaveSetting()
		{
			string caption = Res.GetString("a0473ff4-8cbe-47bb-bc52-d35ceca45a7e", "Save Setting");
			var setting = SettingsComboBox.SelectedItem as ColumnConfigurationManager;

			if (setting != null)
			{
				if (Globals.Message.Show(Res.GetString("cabcd7a3-bf1f-48b9-8e1d-d6886aee1be2", "Do you want to save the current report settings for \"{0}\"?", setting), caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					setting.Save(report.FilterCollection, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.Orientation, report.Language);
				}
			}
			else
			{
				string settingName = SettingsComboBox.Text.Trim();
				if (settingName.Length > 0)
				{
					if (Globals.Message.Show(Res.GetString("a9e477b1-a566-4ab4-892c-d5df5f6bafb0", "Do you want to save the current report settings under \"{0}\"?", settingName), caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
					{
						setting = new CombinedConfigurationManager(headingManager, settingName);
						setting.Save(report.FilterCollection, report.GroupByCollection.SelectedGroupBy.DisplayName, report.SortOrderCollection.SelectedOrder.DisplayName, report.Orientation, report.Language);
					}
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("0f2f2826-8546-4920-aed2-898e02860ec2", "Please provide a name for this setting in order to save it."), caption);
				}
			}
			UpdateListOfSavedNames();
			SettingsComboBox.SelectedItem = setting;
		}

		readonly bool isInitializing;

		void SettingsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ColumnConfigurationManager setting = (ColumnConfigurationManager)SettingsComboBox.SelectedItem;
			if (setting != null)
			{
				DeleteButton.Enabled = setting.CanSaveAndDelete;
				SaveButton.Enabled = setting.CanSaveAndDelete;
				if (!isInitializing)
				{
					setting.Load(report);
				}
			}
		}

		void UpdateColumnDisplayControls()
		{
			ColumnHeading columnHeading = SelectedColumnHeading;
			if (columnHeading == null)
			{
				ColumnDisplayNameTextBox.Text = "";
				ColumnTagNameTextBox.Text = "";
				ColumnDisplayWidthCalcEdit.Text = "";
			}
			else
			{
				ColumnDisplayNameTextBox.Text = columnHeading.HeadingText;
				ColumnTagNameTextBox.Text = columnHeading.TagName;
				ColumnDisplayWidthCalcEdit.Value = columnHeading.WidthInPixels;
			}
			ValidateColumnHeading();
		}

		protected void UpdateListOfSavedNames()
		{
			string selectedItemText = SettingsComboBox.Text;
			SettingsComboBox.Items.Clear();
			foreach (ColumnConfigurationManager manager in headingManager.ConfigurationManagersForAllSavedConfigurations)
			{
				SettingsComboBox.Items.Add(manager);
			}
			SettingsComboBox.Text = selectedItemText;
		}

		void UpdateSelectedColumnHeadingText()
		{
			ColumnHeading columnHeading = SelectedColumnHeading;
			if (columnHeading != null)
			{
				columnHeading.HeadingText = ColumnDisplayNameTextBox.Text;
			}
			ValidateColumnHeading();
		}

		internal void UpdateSelectedColumnTagName()
		{
			ColumnHeading columnHeading = SelectedColumnHeading;
			var hasBeenFormatted = false;
			if (columnHeading != null)
			{
				var tagNameTextXMLFormat = ((ZString)ColumnTagNameTextBox.Text).KeepAlphanumericCharactersXMLFormatting();
				hasBeenFormatted = (ZString)ColumnTagNameTextBox?.Text != tagNameTextXMLFormat;
				columnHeading.TagName = ColumnTagNameTextBox.Text = tagNameTextXMLFormat;
			}
			ValidateColumnHeading(hasBeenFormatted);
		}

		void ValidateColumnHeading(bool hasBeenFormatted = false)
		{
			ColumnHeadingNotificationsOnTextBox.Clear();
			ColumnHeadingNotificationsOnTagNameTextBox.Clear();
			ColumnHeadingNotificationsOnLabel.Clear();

			var columnHeading = SelectedColumnHeading;
			var duplicatedWarningElementName = Res.GetString("7D693935-B094-4AE3-B6A7-B61AAFA47D49", "One or more columns cannot be properly exported to CSV or XML files. Their display names are not unique once invalid XML characters have been removed.");
			var nonExportWarningMessageOnTagNameLabel = Res.GetString("4C0381C0-9871-4F36-AC35-DDF510249DD2", "Element name is subject to the following rules:\r\n\tCase-sensitive.\r\n\tMust start with a letter or underscore.\r\n\tCan contain letters, digits, hyphens, underscores, and periods.\r\n\tCannot contain spaces.");
			var nonExportWarningMessageOnLabel = Res.GetString("829a78bf-eef3-48db-bb13-704c717fff36", "One or more columns cannot be exported to CSV or XML files because their display names don't contain any alphabetic character.");
			if (columnHeading != null)
			{
				if (isDuplicatedColumn(columnHeading))
				{
					ColumnHeadingNotificationsOnLabel.Add(new PropertyNotification("columnHeading", NotificationType.Warning, duplicatedWarningElementName));
				}
				if (!string.IsNullOrEmpty(columnHeading.TagName))
				{
					if (hasBeenFormatted)
					{
						ColumnHeadingNotificationsOnTagNameTextBox.Add(new PropertyNotification("columnHeading", NotificationType.Warning, nonExportWarningMessageOnTagNameLabel));
					}
				}
				else
				{
					if (string.IsNullOrEmpty(columnHeading.HeadingText.KeepAlphabeticCharacters()))
					{
						ColumnHeadingNotificationsOnTextBox.Add(new PropertyNotification("columnHeading", NotificationType.Warning, Res.GetString("607ceaf2-792d-4960-af9a-ce36812ce35b", "This column cannot be exported to CSV or XML files because its display name doesn't contain any alphabetic character.")));
						ColumnHeadingNotificationsOnLabel.Add(new PropertyNotification("columnHeading", NotificationType.Warning, nonExportWarningMessageOnLabel));
					}
					else
					{
						foreach (var item in SelectedColumnslistBox.Items)
						{
							ColumnHeading currentColumnHeading = item as ColumnHeading;
							if (currentColumnHeading != null && string.IsNullOrEmpty(currentColumnHeading.HeadingText.KeepAlphanumericCharactersXMLFormatting()))
							{
								ColumnHeadingNotificationsOnLabel.Add(new PropertyNotification("columnHeading", NotificationType.Warning, nonExportWarningMessageOnLabel));
								break;
							}
						}
					}
				}
				if (string.IsNullOrEmpty(columnHeading.HeadingText))
				{
					ColumnHeadingNotificationsOnTextBox.Add(new PropertyNotification("columnHeading", NotificationType.Warning, Res.GetString("0182C2FA-1813-43BA-A15D-35417B13C945", "This column cannot be exported to CSV or XML files because its display name doesn't contain any character.")));
				}
			}
			SelectedColumnslistBox.Refresh();

			NotificationExOnLabel.Notifications1 = ColumnHeadingNotificationsOnLabel;
			NotificationExOnTextBox.Notifications1 = ColumnHeadingNotificationsOnTextBox;
			NotificationExOnTagNameTextBox.Notifications1 = ColumnHeadingNotificationsOnTagNameTextBox;
		}

		bool isDuplicatedColumn(ColumnHeading columnHeading)
		{
			foreach (var item in SelectedColumnslistBox.Items)
			{
				ColumnHeading currentColumnHeading = item as ColumnHeading;
				var currentColumnHeadingTagNameWithDefault = currentColumnHeading.TagName.IsEmpty ? currentColumnHeading.HeadingText : currentColumnHeading.TagName;
				var columnHeadingTagNameWithDefault = columnHeading.TagName.IsEmpty ? columnHeading.HeadingText : columnHeading.TagName;
				if (currentColumnHeading != columnHeading && columnHeadingTagNameWithDefault.KeepCharsXMLFormatting() == currentColumnHeadingTagNameWithDefault.KeepCharsXMLFormatting())
				{
					return true;
				}
			}
			return false;
		}

		void UpdateSelectedColumnHeadingWidth()
		{
			ColumnHeading columnHeading = SelectedColumnHeading;
			if (columnHeading != null)
			{
				columnHeading.WidthInPixels = Convert.ToInt32(ColumnDisplayWidthCalcEdit.Value);
			}
		}

		void SetItemColor(ListBox listBox, DrawItemEventArgs e)
		{
			e.DrawBackground();

			Brush brush = ((ColumnHeading)listBox.Items[e.Index]).ShowPerformanceWarning ? Brushes.Red : Brushes.Black;

#if !WINZOR
			TextRendererHelper.DrawText(e.Graphics, listBox.Items[e.Index].ToString(), e.Font, e.Bounds, brush, StringFormat.GenericDefault);
			e.DrawFocusRectangle();
#else
			listBox.ListBoxItemsData[e.Index].TextColor = ((SolidBrush)brush).Color;
#endif
		}

		void SetSelectedItemColor(ListBox listBox, DrawItemEventArgs e)
		{
			e.DrawBackground();

			Brush brush;
			if (((ColumnHeading)listBox.Items[e.Index]).HeadingText.KeepAlphabeticCharacters().Length == 0
				|| isDuplicatedColumn((ColumnHeading)listBox.Items[e.Index]))
			{
				brush = Brushes.Red;
			}
			else
			{
				brush = new SolidBrush(e.ForeColor);
			}

#if !WINZOR
			TextRendererHelper.DrawText(e.Graphics, listBox.Items[e.Index].ToString(), e.Font, e.Bounds, brush, StringFormat.GenericDefault);
			e.DrawFocusRectangle();
#else
			listBox.ListBoxItemsData[e.Index].TextColor = ((SolidBrush)brush).Color;
#endif
		}

		void AvailableColumnsListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			SetItemColor(AvailableColumnsListBox, e);
		}

		void IncludedColumnsListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			SetItemColor(IncludedColumnsListBox, e);
		}

		void SelectedColumnsListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (IncludedColumnsListBox.Items.Count > 0)
			{
#if !WINZOR
				SetSelectedItemColor(IncludedColumnsListBox, e);
#else
				SetSelectedItemColor(SelectedColumnslistBox, e);
#endif
			}
		}

		void ReportTitleTextBox_Leave(object sender, EventArgs e)
		{
			UpdateReportTitle();
		}

		void UpdateReportTitle()
		{
			headingManager.CurrentConfiguration.Worksheets[((Worksheet)comboBoxConfigWorksheet.SelectedItem).Name].Title = ReportTitleTextBox.Text;
		}

		void FillReportTitle()
		{
			ReportTitleTextBox.Text = headingManager.CurrentConfiguration.Worksheets[((Worksheet)comboBoxConfigWorksheet.SelectedItem).Name].Title;
		}

		public override IBusiness DataSourceForBinding
		{
			get { return this.report; }
		}
	}
}
