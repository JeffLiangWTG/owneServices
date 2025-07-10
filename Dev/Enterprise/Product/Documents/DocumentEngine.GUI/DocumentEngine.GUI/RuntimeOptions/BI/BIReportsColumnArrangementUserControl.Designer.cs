using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	public partial class BIReportsColumnArrangementUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SettingsComboBox = new CargoWise.Windows.UI.KComboBox();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ColumnConfigurationPanel = new CargoWise.Windows.UI.KPanel();
			this.ZLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.tabColumnConfig = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.tabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ZLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.IncludedColumnsListBox = new CargoWise.Windows.UI.KListBox();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AvailableColumnsListBox = new CargoWise.Windows.UI.KListBox();
			this.AvailableColumnsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PerformanceWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.comboBoxConfigWorksheet = new CargoWise.Windows.UI.KComboBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SettingsGroupBox.SuspendLayout();
			this.ColumnConfigurationPanel.SuspendLayout();
			this.tabColumnConfig.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|4df7a0bc-4286-41b6-a4a5-3507b7e285d5", "Save");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 46, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.TabStop = false;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// SettingsGroupBox
			// 
			this.SettingsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|0c81a4bf-26ce-4056-88e5-34b3a13164cd", "Save or Load Settings by Name");
			this.SettingsGroupBox.Controls.Add(this.NewButton);
			this.SettingsGroupBox.Controls.Add(this.SettingsComboBox);
			this.SettingsGroupBox.Controls.Add(this.DeleteButton);
			this.SettingsGroupBox.Controls.Add(this.SaveButton);
			this.SettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SettingsGroupBox.Name = "SettingsGroupBox";
			this.SettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(602, 79, true);
			this.SettingsGroupBox.TabIndex = 1;
			this.SettingsGroupBox.TabStop = false;
			// 
			// NewButton
			// 
			this.NewButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|221c76a7-f03e-4775-9308-aba0006767b6", "New");
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 46, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.NewButton.TabIndex = 3;
			this.NewButton.ToolTipCaption = null;
			this.NewButton.UseVisualStyleBackColor = true;
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// SettingsComboBox
			// 
			this.SettingsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.SettingsComboBox.FormattingEnabled = true;
			this.SettingsComboBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.SettingsComboBox.Name = "SettingsComboBox";
			this.SettingsComboBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 21, true);
			this.SettingsComboBox.TabIndex = 0;
			// 
			// DeleteButton
			// 
			this.DeleteButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|17cc85d6-cd57-4a7f-aaed-5db598775b92", "Delete");
			this.DeleteButton.Enabled = false;
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 46, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.DeleteButton.TabIndex = 2;
			this.DeleteButton.TabStop = false;
			this.DeleteButton.ToolTipCaption = null;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// ColumnConfigurationPanel
			// 
			this.ColumnConfigurationPanel.Controls.Add(this.tabColumnConfig);
			this.ColumnConfigurationPanel.Controls.Add(this.comboBoxConfigWorksheet);
			this.ColumnConfigurationPanel.Controls.Add(this.ZLabel3);
			this.ColumnConfigurationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 86, true);
			this.ColumnConfigurationPanel.Name = "ColumnConfigurationPanel";
			this.ColumnConfigurationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 269, true);
			this.ColumnConfigurationPanel.TabIndex = 16;
			// 
			// ZLabel3
			// 
			this.ZLabel3.AutoSize = true;
			this.ZLabel3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|83ddf2e2-0303-4bbe-80f9-8badf55430e2", "Worksheet");
			this.ZLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ZLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.ZLabel3.Name = "ZLabel3";
			this.ZLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.ZLabel3.TabIndex = 18;
			this.ZLabel3.Visible = false;
			// 
			// tabColumnConfig
			// 
			this.tabColumnConfig.Controls.Add(this.tabPage1);
			this.tabColumnConfig.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tabColumnConfig.Name = "tabColumnConfig";
			this.tabColumnConfig.SelectedIndex = 0;
			this.tabColumnConfig.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 269, true);
			this.tabColumnConfig.TabIndex = 17;
			// 
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
			this.tabPage1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|26b12ae6-0702-432e-ab1f-62810c08e0a6", "Column Configuration");
			this.tabPage1.Controls.Add(this.ZLabel2);
			this.tabPage1.Controls.Add(this.IncludedColumnsListBox);
			this.tabPage1.Controls.Add(this.MoveUpButton);
			this.tabPage1.Controls.Add(this.MoveDownButton);
			this.tabPage1.Controls.Add(this.RemoveButton);
			this.tabPage1.Controls.Add(this.AddButton);
			this.tabPage1.Controls.Add(this.AvailableColumnsListBox);
			this.tabPage1.Controls.Add(this.AvailableColumnsLabel);
			this.tabPage1.Controls.Add(this.PerformanceWarningLabel);
			this.tabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 242, true);
			this.tabPage1.TabIndex = 0;
			// 
			// ZLabel2
			// 
			this.ZLabel2.AutoSize = true;
			this.ZLabel2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|d1a6fd78-5fa2-4b1b-ac14-fbc6e48d2e23", "Show Columns in this order");
			this.ZLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ZLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 3, true);
			this.ZLabel2.Name = "ZLabel2";
			this.ZLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 13, true);
			this.ZLabel2.TabIndex = 10;
			// 
			// IncludedColumnsListBox
			// 
			this.IncludedColumnsListBox.HorizontalScrollbar = true;
			this.IncludedColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 21, true);
			this.IncludedColumnsListBox.Name = "IncludedColumnsListBox";
			this.IncludedColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 186, true);
			this.IncludedColumnsListBox.TabIndex = 1;
			this.IncludedColumnsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.IncludedColumnsListBox_DrawItem);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|527c7a79-8a04-463e-94ab-0d139aec8f0e", "Move Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 156, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.MoveUpButton.TabIndex = 2;
			this.MoveUpButton.TabStop = false;
			this.MoveUpButton.ToolTipCaption = null;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|c5670a88-27f3-48aa-a589-c69466a558b7", "Move Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 185, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.MoveDownButton.TabIndex = 3;
			this.MoveDownButton.ToolTipCaption = null;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|2e90d223-0407-4ac6-a6ca-0433ce0c9fcd", "<- Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 50, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.RemoveButton.TabIndex = 5;
			this.RemoveButton.TabStop = false;
			this.RemoveButton.ToolTipCaption = null;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// AddButton
			// 
			this.AddButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|bdbc02d8-0871-4534-b078-d8563c59c345", "Add ->");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 21, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.AddButton.TabIndex = 4;
			this.AddButton.TabStop = false;
			this.AddButton.ToolTipCaption = null;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// AvailableColumnsListBox
			// 
			this.AvailableColumnsListBox.AllowDrop = true;
			this.AvailableColumnsListBox.HorizontalScrollbar = true;
			this.AvailableColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 21, true);
			this.AvailableColumnsListBox.Name = "AvailableColumnsListBox";
			this.AvailableColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 186, true);
			this.AvailableColumnsListBox.Sorted = true;
			this.AvailableColumnsListBox.TabIndex = 0;
			this.AvailableColumnsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.AvailableColumnsListBox_DrawItem);
			// 
			// AvailableColumnsLabel
			// 
			this.AvailableColumnsLabel.AutoSize = true;
			this.AvailableColumnsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|06a168b2-c591-41c4-a2a6-aadc88dde536", "Available Columns");
			this.AvailableColumnsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AvailableColumnsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AvailableColumnsLabel.Name = "AvailableColumnsLabel";
			this.AvailableColumnsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.AvailableColumnsLabel.TabIndex = 11;
			// 
			// PerformanceWarningLabel
			// 
			this.PerformanceWarningLabel.AutoSize = true;
			this.PerformanceWarningLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|e65657f8-cd79-471b-a3da-719ee10d1d7a", "* Fields marked in Red are a high cost to database performance when selected in a report.");
			this.PerformanceWarningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PerformanceWarningLabel.ForeColor = System.Drawing.Color.Red;
			this.PerformanceWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 202, true);
			this.PerformanceWarningLabel.Name = "PerformanceWarningLabel";
			this.PerformanceWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 13, true);
			this.PerformanceWarningLabel.TabIndex = 12;
			this.PerformanceWarningLabel.Visible = false;
			// 
			// comboBoxConfigWorksheet
			// 
			this.comboBoxConfigWorksheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxConfigWorksheet.FormattingEnabled = true;
			this.comboBoxConfigWorksheet.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 0, true);
			this.comboBoxConfigWorksheet.Name = "comboBoxConfigWorksheet";
			this.comboBoxConfigWorksheet.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 21, true);
			this.comboBoxConfigWorksheet.TabIndex = 16;
			this.comboBoxConfigWorksheet.Visible = false;
			this.comboBoxConfigWorksheet.SelectedIndexChanged += new System.EventHandler(this.comboBoxConfigWorksheet_SelectedIndexChanged);
			// 
			// BIReportsColumnArrangementUserControl
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SettingsGroupBox);
			this.Controls.Add(this.ColumnConfigurationPanel);
			this.Name = "BIReportsColumnArrangementUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 358, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SettingsGroupBox.ResumeLayout(false);
			this.SettingsGroupBox.PerformLayout();
			this.ColumnConfigurationPanel.ResumeLayout(false);
			this.ColumnConfigurationPanel.PerformLayout();
			this.tabColumnConfig.ResumeLayout(false);
			this.tabColumnConfig.PerformLayout();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZButton SaveButton;
		internal ZGroupBox SettingsGroupBox;
		internal ZButton DeleteButton;
		internal CargoWise.Windows.UI.KComboBox SettingsComboBox;
		private Enterprise.ZArchitecture.GUI.ZButton NewButton;
		internal CargoWise.Windows.UI.KPanel ColumnConfigurationPanel;
		private ZLabel ZLabel3;
		internal ZTabControl tabColumnConfig;
		private ZTabPage tabPage1;
		private ZLabel ZLabel2;
		public ZButton MoveUpButton;
		public ZButton MoveDownButton;
		public ZButton RemoveButton;
		internal ZButton AddButton;
		private ZLabel AvailableColumnsLabel;
		private ZLabel PerformanceWarningLabel;
		private CargoWise.Windows.UI.KComboBox comboBoxConfigWorksheet;
		internal CargoWise.Windows.UI.KListBox IncludedColumnsListBox;
		internal CargoWise.Windows.UI.KListBox AvailableColumnsListBox;
		private System.ComponentModel.IContainer components;
	}
}
