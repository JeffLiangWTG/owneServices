using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public partial class BIReportsColumnArrangementUserControl : ZUserControl
	{
		protected readonly ColumnConfigurationsManager headingManager;
		protected readonly Report report;

		public BIReportsColumnArrangementUserControl(Report report)
		{
			this.report = report;
			headingManager = report.ColumnHeadingManager;
			InitializeComponent();
			UpdateListOfSavedNames();
			NotificationExOnTextBox.Notifications1 = ColumnHeadingNotificationsOnTextBox;
			NotificationExOnLabel.Notifications1 = ColumnHeadingNotificationsOnLabel;
			NotificationExOnTagNameTextBox.Notifications1 = ColumnHeadingNotificationsOnTagNameTextBox;
			headingManager.CurrentConfigurationRefreshed += headingManager_ColumnHeadingManagerLoaded;

			SettingsComboBox.SelectedIndex = 0;
			// Event hooked manually to stop SettingsComboBox_SelectedIndexChanged firing and blasting loaded values from Desktop Shortcut.
			SettingsComboBox.SelectedIndexChanged += SettingsComboBox_SelectedIndexChanged;

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
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		NotificationExtension notificationExOnLabel;
		NotificationExtension NotificationExOnLabel
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
		NotificationExtension NotificationExOnTextBox
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
		NotificationExtension NotificationExOnTagNameTextBox
		{
			get
			{
				return notificationExOnTagNameTextBox = notificationExOnTagNameTextBox ?? new NotificationExtension();
			}
		}

		ColumnHeading IncludedColumnHeading
		{
			get { return IncludedColumnsListBox.SelectedItem as ColumnHeading; }
		}

		void AddButton_Click(object sender, EventArgs e)
		{
			MoveColumnsAcrossListBoxes(AvailableColumnsListBox, IncludedColumnsListBox, false);
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
				worksheet.NameLocalized = DocBuilderResourceStrings.GetReportString(report.Template.TemplateName, worksheet.Name);
				comboBoxConfigWorksheet.Items.Add(worksheet);
			}
		}

		void FillListboxes()
		{
			AvailableColumnsListBox.Items.Clear();
			IncludedColumnsListBox.Items.Clear();

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
					}
				}
			}
			IncludedColumnsListBox.SelectedIndex = IncludedColumnsListBox.Items.Count > 0 ? 0 : -1;

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
		}

		void comboBoxConfigWorksheet_SelectedIndexChanged(object sender, EventArgs e)
		{
			FillListboxes();
		}

		void MoveColumnsAcrossListBoxes(ListBox source, ListBox destination, bool hideColumn)
		{
			var heading = source.SelectedItem as ColumnHeading;
			if (heading != null)
			{
				heading.Hidden = hideColumn;
				int originalIndex = source.SelectedIndex;
				source.Items.Remove(heading);
				destination.Items.Add(heading);
				destination.SelectedItem = heading;
				foreach (ColumnHeading sourceHeading in source.Items)
				{
					if (sourceHeading.CurrentPosition > heading.CurrentPosition)
					{
						sourceHeading.CurrentPosition--;
					}
				}
				heading.CurrentPosition = destination.Items.Count - 1;
				if (source.Items.Count > 0)
				{
					source.SelectedIndex = (originalIndex >= source.Items.Count) ? source.Items.Count - 1 : originalIndex;
				}
			}
		}

		void MoveColumnPosition(ColumnHeading heading, int positionDifference)
		{
			heading.CurrentPosition += positionDifference;
			((ColumnHeading)IncludedColumnsListBox.Items[IncludedColumnsListBox.SelectedIndex + positionDifference]).CurrentPosition -= positionDifference;
			int originalPosition = IncludedColumnsListBox.Items.IndexOf(heading);
			IncludedColumnsListBox.Items.Remove(heading);
			IncludedColumnsListBox.Items.Insert(originalPosition + positionDifference, heading);
			IncludedColumnsListBox.SelectedItem = heading;
			IncludedColumnsListBox.Refresh();
			IncludedColumnsListBox.Invalidate();
		}

		void MoveDownButton_Click(object sender, EventArgs e)
		{
			ColumnHeading columnHeading = IncludedColumnHeading;
			if ((columnHeading != null) && (columnHeading.CurrentPosition < IncludedColumnsListBox.Items.Count - 1))
			{
				MoveColumnPosition(columnHeading, 1);
			}
		}

		void MoveUpButton_Click(object sender, EventArgs e)
		{
			ColumnHeading columnHeading = IncludedColumnHeading;
			if ((columnHeading != null) && (columnHeading.CurrentPosition > 0))
			{
				MoveColumnPosition(columnHeading, -1);
			}
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
			UpdateListOfSavedNames();
			SettingsComboBox.SelectedItem = manager;
		}

		void RemoveButton_Click(object sender, EventArgs e)
		{
			MoveColumnsAcrossListBoxes(IncludedColumnsListBox, AvailableColumnsListBox, true);
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

		void SettingsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ColumnConfigurationManager setting = (ColumnConfigurationManager)SettingsComboBox.SelectedItem;
			if (setting != null)
			{
				DeleteButton.Enabled = setting.CanSaveAndDelete;
				SaveButton.Enabled = setting.CanSaveAndDelete;
				setting.Load(report);
			}
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

#if !WINZOR

		void SetItemColor(ListBox listBox, DrawItemEventArgs e)
		{
			e.DrawBackground();

			Brush brush = ((ColumnHeading)listBox.Items[e.Index]).ShowPerformanceWarning ? Brushes.Red : Brushes.Black;

			TextRendererHelper.DrawText(e.Graphics, listBox.Items[e.Index].ToString(), e.Font, e.Bounds, brush, StringFormat.GenericDefault);
			e.DrawFocusRectangle();
		}

		void SetSelectedItemColor(ListBox listBox, DrawItemEventArgs e)
		{
			e.DrawBackground();

			Brush brush;
			if (((ColumnHeading)listBox.Items[e.Index]).HeadingText.KeepAlphabeticCharacters().Length == 0)
			{
				brush = Brushes.Red;
			}
			else
			{
				brush = new SolidBrush(e.ForeColor);
			}

			TextRendererHelper.DrawText(e.Graphics, listBox.Items[e.Index].ToString(), e.Font, e.Bounds, brush, StringFormat.GenericDefault);
			e.DrawFocusRectangle();
		}

#endif

		void AvailableColumnsListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
#if !WINZOR
			SetItemColor(AvailableColumnsListBox, e);
#endif
		}

		void IncludedColumnsListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
#if !WINZOR
			SetItemColor(IncludedColumnsListBox, e);
#endif
		}

		void SelectedColumnsListBox_DrawItem(object sender, DrawItemEventArgs e)
		{
#if !WINZOR
			if (IncludedColumnsListBox.Items.Count > 0)
			{
				SetSelectedItemColor(IncludedColumnsListBox, e);
			}
#endif
		}

		public override IBusiness DataSourceForBinding
		{
			get { return this.report; }
		}
	}
}
