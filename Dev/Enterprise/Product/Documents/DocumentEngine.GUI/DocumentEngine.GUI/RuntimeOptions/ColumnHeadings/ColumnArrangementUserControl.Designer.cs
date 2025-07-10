using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class ColumnArrangementUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
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
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AvailableColumnsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PerformanceWarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.tabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ColumnPropertiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PixelsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ColumnDisplayWidthCalcEdit = new CargoWise.Windows.UI.KNumericUpDown();
			this.ColumnDisplayNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ColumnTagNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ColumnDisplayWidthLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ZLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.OptionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ReportTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.comboBoxConfigWorksheet = new CargoWise.Windows.UI.KComboBox();
			this.IncludedColumnsListBox = new Enterprise.ZArchitecture.GUI.ZListBox();
			this.AvailableColumnsListBox = new Enterprise.ZArchitecture.GUI.ZListBox();
			this.SelectedColumnslistBox = new Enterprise.ZArchitecture.GUI.ZListBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SettingsGroupBox.SuspendLayout();
			this.ColumnConfigurationPanel.SuspendLayout();
			this.tabColumnConfig.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.ColumnPropertiesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnDisplayWidthCalcEdit)).BeginInit();
			this.OptionTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|4df7a0bc-4286-41b6-a4a5-3507b7e285d5", "Save");
			this.SaveButton.Enabled = false;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 46, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.TabStop = false;
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
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.NewButton.TabIndex = 3;
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
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 23, true);
			this.DeleteButton.TabIndex = 2;
			this.DeleteButton.TabStop = false;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// ColumnConfigurationPanel
			// 
			this.ColumnConfigurationPanel.Controls.Add(this.ZLabel3);
			this.ColumnConfigurationPanel.Controls.Add(this.tabColumnConfig);
			this.ColumnConfigurationPanel.Controls.Add(this.comboBoxConfigWorksheet);
			this.ColumnConfigurationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 86, true);
			this.ColumnConfigurationPanel.Name = "ColumnConfigurationPanel";
			this.ColumnConfigurationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 269, true);
			this.ColumnConfigurationPanel.TabIndex = 16;
			// 
			// ZLabel3
			// 
			this.ZLabel3.AutoSize = true;
			this.ZLabel3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|83ddf2e2-0303-4bbe-80f9-8badf55430e2", "Worksheet");
			this.ZLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.ZLabel3.Name = "ZLabel3";
			this.ZLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
			this.ZLabel3.TabIndex = 18;
			// 
			// tabColumnConfig
			// 
			this.tabColumnConfig.Controls.Add(this.tabPage1);
			this.tabColumnConfig.Controls.Add(this.tabPage2);
			this.tabColumnConfig.Controls.Add(this.OptionTabPage);
			this.tabColumnConfig.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.tabColumnConfig.Name = "tabColumnConfig";
			this.tabColumnConfig.SelectedIndex = 0;
			this.tabColumnConfig.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 244, true);
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
			this.tabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 217, true);
			this.tabPage1.TabIndex = 0;
			// 
			// ZLabel2
			// 
			this.ZLabel2.AutoSize = true;
			this.ZLabel2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|d1a6fd78-5fa2-4b1b-ac14-fbc6e48d2e23", "Show Columns in this order");
			this.ZLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 3, true);
			this.ZLabel2.Name = "ZLabel2";
			this.ZLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 13, true);
			this.ZLabel2.TabIndex = 10;
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|527c7a79-8a04-463e-94ab-0d139aec8f0e", "Move Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 156, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.MoveUpButton.TabIndex = 2;
			this.MoveUpButton.TabStop = false;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|c5670a88-27f3-48aa-a589-c69466a558b7", "Move Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 185, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.MoveDownButton.TabIndex = 3;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// RemoveButton
			// 
			this.RemoveButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|2e90d223-0407-4ac6-a6ca-0433ce0c9fcd", "<- Remove");
			this.RemoveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 50, true);
			this.RemoveButton.Name = "RemoveButton";
			this.RemoveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.RemoveButton.TabIndex = 5;
			this.RemoveButton.TabStop = false;
			this.RemoveButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// AddButton
			// 
			this.AddButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|bdbc02d8-0871-4534-b078-d8563c59c345", "Add ->");
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 21, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.AddButton.TabIndex = 4;
			this.AddButton.TabStop = false;
			this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// AvailableColumnsLabel
			// 
			this.AvailableColumnsLabel.AutoSize = true;
			this.AvailableColumnsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|06a168b2-c591-41c4-a2a6-aadc88dde536", "Available Columns");
			this.AvailableColumnsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AvailableColumnsLabel.Name = "AvailableColumnsLabel";
			this.AvailableColumnsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.AvailableColumnsLabel.TabIndex = 11;
			// 
			// PerformanceWarningLabel
			// 
			this.PerformanceWarningLabel.AutoSize = true;
			this.PerformanceWarningLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|e65657f8-cd79-471b-a3da-719ee10d1d7a", "* Fields marked in Red are a high cost to database performance when selected in a report.");
			this.PerformanceWarningLabel.ForeColor = System.Drawing.Color.Red;
			this.PerformanceWarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 202, true);
			this.PerformanceWarningLabel.Name = "PerformanceWarningLabel";
			this.PerformanceWarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 13, true);
			this.PerformanceWarningLabel.TabIndex = 12;
			this.PerformanceWarningLabel.Visible = false;
			// 
			// tabPage2
			// 
			this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
			this.tabPage2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|b56db572-917c-4924-81c4-188229e5ac7e", "Column Properties");
			this.tabPage2.Controls.Add(this.ColumnPropertiesGroupBox);
			this.tabPage2.Controls.Add(this.ZLabel1);
			this.tabPage2.Controls.Add(this.SelectedColumnslistBox);
			this.tabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 217, true);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Enter += new System.EventHandler(this.tabPage2_Enter);
			// 
			// ColumnPropertiesGroupBox
			// 
			this.ColumnPropertiesGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|17f298e3-e56a-4d0f-b14d-0673858fe614", "Column Properties");
			this.ColumnPropertiesGroupBox.Controls.Add(this.PixelsLabel);
			this.ColumnPropertiesGroupBox.Controls.Add(this.ColumnDisplayWidthCalcEdit);
			this.ColumnPropertiesGroupBox.Controls.Add(this.ColumnDisplayNameTextBox);
			this.ColumnPropertiesGroupBox.Controls.Add(this.ColumnTagNameTextBox);
			this.ColumnPropertiesGroupBox.Controls.Add(this.ColumnDisplayWidthLabel);
			this.ColumnPropertiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 8, true);
			this.ColumnPropertiesGroupBox.Name = "ColumnPropertiesGroupBox";
			this.ColumnPropertiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 204, true);
			this.ColumnPropertiesGroupBox.TabIndex = 10;
			this.ColumnPropertiesGroupBox.TabStop = false;
			// 
			// PixelsLabel
			// 
			this.PixelsLabel.AutoSize = true;
			this.PixelsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|69a63656-b4ef-4af9-8d81-3e18c2df7181", "pixels");
			this.PixelsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 92, true);
			this.PixelsLabel.Name = "PixelsLabel";
			this.PixelsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.PixelsLabel.TabIndex = 5;
			// 
			// ColumnDisplayWidthCalcEdit
			// 
			this.ColumnDisplayWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 89, true);
			this.ColumnDisplayWidthCalcEdit.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.ColumnDisplayWidthCalcEdit.Name = "ColumnDisplayWidthCalcEdit";
			this.ColumnDisplayWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 18, true);
			this.ColumnDisplayWidthCalcEdit.TabIndex = 18;
			this.ColumnDisplayWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ColumnDisplayWidthCalcEdit.Leave += new System.EventHandler(this.ColumnDisplayWidthCalcEdit_Leave);
			// 
			// ColumnDisplayNameTextBox
			// 
			this.ColumnDisplayNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ColumnDisplayNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|a8a45147-913e-4b56-be1e-c04b72049818", "Column Display Name");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ColumnDisplayNameTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ColumnDisplayNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 42, true);
			this.ColumnDisplayNameTextBox.Name = "ColumnDisplayNameTextBox";
			this.ColumnDisplayNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 18, true);
			this.ColumnDisplayNameTextBox.TabIndex = 17;
			this.ColumnDisplayNameTextBox.Leave += new System.EventHandler(this.ColumnDisplayNameTextBox_Leave);
			// 
			// ColumnTagNameTextBox
			// 
			this.ColumnTagNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|9097633D-1C5C-4C8F-8C0A-7F60930B1A03", "Element Name");
			this.ColumnTagNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ColumnTagNameTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ColumnTagNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 134, true);
			this.ColumnTagNameTextBox.Name = "ColumnTagNameTextBox";
			this.ColumnTagNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 18, true);
			this.ColumnTagNameTextBox.TabIndex = 20;
			this.ColumnTagNameTextBox.Leave += new System.EventHandler(this.ColumnTagNameTextBox_Leave);
			// 
			// ColumnDisplayWidthLabel
			// 
			this.ColumnDisplayWidthLabel.AutoSize = true;
			this.ColumnDisplayWidthLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|24ca5352-dbc0-45fc-bf3f-fb321840d8f4", "Column Display Width");
			this.ColumnDisplayWidthLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 73, true);
			this.ColumnDisplayWidthLabel.Name = "ColumnDisplayWidthLabel";
			this.ColumnDisplayWidthLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 14, true);
			this.ColumnDisplayWidthLabel.TabIndex = 3;
			// 
			// ZLabel1
			// 
			this.ZLabel1.AutoSize = true;
			this.ZLabel1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|92caacb4-df2b-4541-afb3-cb8874666fed", "Selected Columns");
			this.ZLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 8, true);
			this.ZLabel1.Name = "ZLabel1";
			this.ZLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 13, true);
			this.ZLabel1.TabIndex = 8;
			// 
			// OptionTabPage
			// 
			this.OptionTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.OptionTabPage.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|7269cf12-37d6-47a7-8bb0-f25f15d2673e", "Options");
			this.OptionTabPage.Controls.Add(this.ReportTitleTextBox);
			this.OptionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OptionTabPage.Name = "OptionTabPage";
			this.OptionTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OptionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 217, true);
			this.OptionTabPage.TabIndex = 2;
			// 
			// ReportTitleTextBox
			// 
			this.ReportTitleTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ColumnArrangementUserControl|3a2e2123-abaf-4e7b-9188-08fed5b42249", "Report Title");
			this.ReportTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 9, true);
			this.ReportTitleTextBox.Name = "ReportTitleTextBox";
			this.ReportTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.ReportTitleTextBox.TabIndex = 2;
			this.ReportTitleTextBox.Leave += new System.EventHandler(this.ReportTitleTextBox_Leave);
			// 
			// comboBoxConfigWorksheet
			// 
			this.comboBoxConfigWorksheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxConfigWorksheet.FormattingEnabled = true;
			this.comboBoxConfigWorksheet.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 0, true);
			this.comboBoxConfigWorksheet.Name = "comboBoxConfigWorksheet";
			this.comboBoxConfigWorksheet.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 21, true);
			this.comboBoxConfigWorksheet.TabIndex = 16;
			this.comboBoxConfigWorksheet.SelectedIndexChanged += new System.EventHandler(this.comboBoxConfigWorksheet_SelectedIndexChanged);
			// 
			// IncludedColumnsListBox
			// 
			this.IncludedColumnsListBox.HorizontalScrollbar = true;
			this.IncludedColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 21, true);
			this.IncludedColumnsListBox.Name = "IncludedColumnsListBox";
			this.IncludedColumnsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.IncludedColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 186, true);
			this.IncludedColumnsListBox.TabIndex = 1;
			this.IncludedColumnsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.IncludedColumnsListBox_DrawItem);
			this.IncludedColumnsListBox.EnableDragAndDrop(true);
			this.IncludedColumnsListBox.DragAndDropItemChanged += IncludedColumnsListBox_DragAndDropItemChanged;
			this.IncludedColumnsListBox.PreviewDropItem += IncludedColumnsListBox_PreviewDropItem;
			this.IncludedColumnsListBox.DragLeave += IncludedColumnsListBox_DragLeave;
			this.IncludedColumnsListBox.MouseDown += IncludedColumnsListBox_MouseDown;
			// 
			// AvailableColumnsListBox
			// 
			this.AvailableColumnsListBox.AllowDrop = true;
			this.AvailableColumnsListBox.HorizontalScrollbar = true;
			this.AvailableColumnsListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 21, true);
			this.AvailableColumnsListBox.Name = "AvailableColumnsListBox";
			this.AvailableColumnsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.AvailableColumnsListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 186, true);
			this.AvailableColumnsListBox.Sorted = true;
			this.AvailableColumnsListBox.TabIndex = 0;
			this.AvailableColumnsListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.AvailableColumnsListBox_DrawItem);
			this.AvailableColumnsListBox.EnableDragAndDrop(false);
			this.AvailableColumnsListBox.MouseDown += AvailableColumnsListBox_MouseDown;
			this.AvailableColumnsListBox.MouseUp += AvailableColumnsListBox_MouseUp;
			this.AvailableColumnsListBox.AfterClearSelected += AvailableColumnsListBox_ListBoxClearSelected;
			this.AvailableColumnsListBox.DragDrop += AvailableColumnsListBox_DragDrop;
			// 
			// SelectedColumnslistBox
			// 
			this.SelectedColumnslistBox.HorizontalScrollbar = true;
			this.SelectedColumnslistBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 28, true);
			this.SelectedColumnslistBox.Name = "SelectedColumnslistBox";
			this.SelectedColumnslistBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 186, true);
			this.SelectedColumnslistBox.TabIndex = 5;
			this.SelectedColumnslistBox.DrawMode = DrawMode.OwnerDrawFixed;
			this.SelectedColumnslistBox.SelectedIndexChanged += new System.EventHandler(this.SelectedColumnslistBox_SelectedIndexChanged);
			this.SelectedColumnslistBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.SelectedColumnsListBox_DrawItem);
			// 
			// ColumnArrangementUserControl
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SettingsGroupBox);
			this.Controls.Add(this.ColumnConfigurationPanel);
			this.Name = "ColumnArrangementUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 358, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SettingsGroupBox.ResumeLayout(false);
			this.ColumnConfigurationPanel.ResumeLayout(false);
			this.ColumnConfigurationPanel.PerformLayout();
			this.tabColumnConfig.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ColumnPropertiesGroupBox.ResumeLayout(false);
			this.ColumnPropertiesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ColumnDisplayWidthCalcEdit)).EndInit();
			this.OptionTabPage.ResumeLayout(false);
			this.OptionTabPage.PerformLayout();
			this.ResumeLayout(false);

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
		internal ZButton MoveUpButton;
		internal ZButton MoveDownButton;
		internal ZButton RemoveButton;
		internal ZButton AddButton;
		private ZLabel AvailableColumnsLabel;
		private ZLabel PerformanceWarningLabel;
		internal ZTabPage tabPage2;
		internal ZGroupBox ColumnPropertiesGroupBox;
		private ZLabel PixelsLabel;
		internal CargoWise.Windows.UI.KNumericUpDown ColumnDisplayWidthCalcEdit;
		internal ZTextBox ColumnDisplayNameTextBox;
		internal ZTextBox ColumnTagNameTextBox;
		private ZLabel ColumnDisplayWidthLabel;
		private ZLabel ZLabel1;
		private ZTabPage OptionTabPage;
		internal ZTextBox ReportTitleTextBox;
		internal CargoWise.Windows.UI.KComboBox comboBoxConfigWorksheet;
		internal Enterprise.ZArchitecture.GUI.ZListBox IncludedColumnsListBox;
		internal Enterprise.ZArchitecture.GUI.ZListBox AvailableColumnsListBox;
		internal Enterprise.ZArchitecture.GUI.ZListBox SelectedColumnslistBox;
	}
}
