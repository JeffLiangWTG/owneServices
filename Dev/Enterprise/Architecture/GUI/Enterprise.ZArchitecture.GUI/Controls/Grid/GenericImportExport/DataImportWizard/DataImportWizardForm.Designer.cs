
namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class DataImportWizardForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DisposeWizard();
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/// 
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ColumnHeader columnHeader1;
			System.Windows.Forms.ColumnHeader columnHeader5;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.FileTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.StartingRowNumericUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DelimiterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UseCurrentCountryNumberFormattingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SettingsControl = new Enterprise.ZArchitecture.GUI.DataMapping.SettingsControl();
			this.PreviewListView = new CargoWise.Windows.UI.KListView();
			this.MappingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClearMapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProperCaseExcludeListButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CustomMapListsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromNextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FromPrevButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FromListView = new CargoWise.Windows.UI.KListView();
			this.FromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PreviewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviewGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidateAndSaveTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.validateAndSaveControl = new Enterprise.ZArchitecture.GUI.DataMapping.DataImportWizardValidateAndSaveControl();
			columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.FileTabPage.SuspendLayout();
			this.zPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StartingRowNumericUpDown)).BeginInit();
			this.zPanel2.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.MappingTabPage.SuspendLayout();
			this.zPanel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ToGrid)).BeginInit();
			this.PreviewTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviewGrid)).BeginInit();
			this.ValidateAndSaveTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 282, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ImportWizard);
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Field";
			// 
			// columnHeader5
			// 
			columnHeader5.Text = "Value";
			columnHeader5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.FileTabPage);
			this.MainTabControl.Controls.Add(this.MappingTabPage);
			this.MainTabControl.Controls.Add(this.PreviewTabPage);
			this.MainTabControl.Controls.Add(this.ValidateAndSaveTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 226, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.SelectedIndexChanging += new System.EventHandler(this.MainTabControl_SelectedIndexChanging);
			// 
			// FileTabPage
			// 
			this.FileTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|165d1501-c0c3-4468-a9b1-4470fc565a77", "File");
			this.FileTabPage.Controls.Add(this.zPanel3);
			this.FileTabPage.Controls.Add(this.zPanel2);
			this.FileTabPage.Controls.Add(this.zPanel1);
			this.FileTabPage.Controls.Add(this.SettingsControl);
			this.FileTabPage.Controls.Add(this.PreviewListView);
			this.FileTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FileTabPage.Name = "FileTabPage";
			this.FileTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FileTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 199, true);
			this.FileTabPage.TabIndex = 0;
			// 
			// SettingsControl
			// 
			this.SettingsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SettingsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.DataMapping.ImportExportWizard)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)))));
			this.SettingsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 7, true);
			this.SettingsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 49, true);
			this.SettingsControl.Name = "SettingsControl";
			this.SettingsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 49, true);
			this.SettingsControl.TabIndex = 0;
			// 
			// zPanel3
			// 
			this.zPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel3.Controls.Add(this.StartingRowNumericUpDown);
			this.zPanel3.Controls.Add(this.zLabel2);
			this.zPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 121, true);
			this.zPanel3.Name = "zPanel3";
			this.zPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 24, true);
			this.zPanel3.TabIndex = 11;

			// 
			// StartingRowNumericUpDown
			// 
			this.BindingSource.SetBindingMember(this.StartingRowNumericUpDown, "StartingRow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).StartingRow)));
			this.StartingRowNumericUpDown.BindTo = "StartingRow";
			this.StartingRowNumericUpDown.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StartingRowNumericUpDown, false);
			this.StartingRowNumericUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 0, true);
			this.StartingRowNumericUpDown.Maximum = new decimal(new int[] {
			99,
			0,
			0,
			0});
			this.StartingRowNumericUpDown.Minimum = new decimal(new int[] {
			1,
			0,
			0,
			0});
			this.StartingRowNumericUpDown.Name = "StartingRowNumericUpDown";
			this.StartingRowNumericUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.StartingRowNumericUpDown.TabIndex = 8;
			this.StartingRowNumericUpDown.Value = new decimal(new int[] {
			1,
			0,
			0,
			0});
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.Dock = System.Windows.Forms.DockStyle.Left;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 14, true);
			this.zLabel2.TabIndex = 8;
			this.zLabel2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|a02d1bef-5d71-4e35-a8eb-0fc5aa3ecec2", "Start import at row: ");
			// 
			// zPanel2
			// 
			this.zPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel2.Controls.Add(this.DelimiterDropEdit);
			this.zPanel2.Controls.Add(this.zLabel1);
			this.zPanel2.Controls.Add(this.UseCurrentCountryNumberFormattingCheckBox);
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 91, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);
			this.zPanel2.TabIndex = 9;
			// 
			// DelimiterDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DelimiterDropEdit, "Delimiter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Delimiter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Delimiters)));
			this.DelimiterDropEdit.BindToList = "Delimiters";
			this.DelimiterDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DelimiterDropEdit.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DelimiterDropEdit, false);
			this.DelimiterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(51, 0, true);
			this.DelimiterDropEdit.Name = "DelimiterDropEdit";
			this.DelimiterDropEdit.PreBoundMaxLength = 5;
			this.DelimiterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.DelimiterDropEdit.TabIndex = 5;
			// 
			// UseCurrentCountryNumberFormattingCheckBox
			// 
			this.UseCurrentCountryNumberFormattingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseCurrentCountryNumberFormattingCheckBox, "UseCurrentCountryNumberFormatting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).UseCurrentCountryNumberFormatting)));
			this.UseCurrentCountryNumberFormattingCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|09d3531c-4f44-4b39-a377-03c9db345f9e", "Use current country/region number formatting");
			this.UseCurrentCountryNumberFormattingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseCurrentCountryNumberFormattingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 0, true);
			this.UseCurrentCountryNumberFormattingCheckBox.Name = "UseCurrentCountryNumberFormattingCheckBox";
			this.UseCurrentCountryNumberFormattingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 17, true);
			this.UseCurrentCountryNumberFormattingCheckBox.TabIndex = 6;
			this.UseCurrentCountryNumberFormattingCheckBox.UseVisualStyleBackColor = true;
			// 
			// PreviewListView
			// 
			this.PreviewListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewListView.GridLines = true;
			this.PreviewListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.PreviewListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 151, true);
			this.PreviewListView.Name = "PreviewListView";
			this.PreviewListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 47, true);
			this.PreviewListView.TabIndex = 8;
			this.PreviewListView.UseCompatibleStateImageBehavior = false;
			this.PreviewListView.View = System.Windows.Forms.View.Details;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 14, true);
			this.zLabel1.TabIndex = 6;
			this.zLabel1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|15b347c8-10a4-4810-a41f-7cf2bb7e0a9f", "Delimiter: ");
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right))));
			this.zPanel1.Controls.Add(this.BrowseButton);
			this.zPanel1.Controls.Add(this.FileTextBox);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 61, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 24, true);
			this.zPanel1.TabIndex = 8;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseButton.AutoSize = true;
			this.BrowseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|8b2a1d1e-b10c-4b1e-aa04-95ec48087cb2", "Browse...");
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 0, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 3;
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// FileTextBox
			// 
			this.FileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FileTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FileTextBox, "FileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).FileName)));
			this.FileTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|29941d85-9b53-4fbd-9e9b-73cfc373bb47", "File to import");
			this.FileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 3, true);
			this.FileTextBox.Name = "FileTextBox";
			this.FileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 20, true);
			this.FileTextBox.TabIndex = 2;
			// 
			// MappingTabPage
			// 
			this.MappingTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|36d8fe5a-681b-4c37-8915-79abcda1a720", "Mapping");
			this.MappingTabPage.Controls.Add(this.zPanel4);
			this.MappingTabPage.Controls.Add(this.ToGrid);
			this.MappingTabPage.Controls.Add(this.ToLabel);
			this.MappingTabPage.Controls.Add(this.FromNextButton);
			this.MappingTabPage.Controls.Add(this.FromPrevButton);
			this.MappingTabPage.Controls.Add(this.FromListView);
			this.MappingTabPage.Controls.Add(this.FromLabel);
			this.MappingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MappingTabPage.Name = "MappingTabPage";
			this.MappingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MappingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 202, true);
			this.MappingTabPage.TabIndex = 1;
			// 
			// zPanel4
			// 
			this.zPanel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left)));
			this.zPanel4.Controls.Add(this.ClearMapButton);
			this.zPanel4.Controls.Add(this.ProperCaseExcludeListButton);
			this.zPanel4.Controls.Add(this.CustomMapListsButton);
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 173, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 24, true);
			this.zPanel4.TabIndex = 9;

			// 
			// ProperCaseExcludeListButton
			// 
			this.ProperCaseExcludeListButton.AutoSize = true;
			this.ProperCaseExcludeListButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|8979b8ea-0144-4afe-a59b-36d3cd065033", "Proper Case Exclude List");
			this.ProperCaseExcludeListButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.ProperCaseExcludeListButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 0, true);
			this.ProperCaseExcludeListButton.Name = "ProperCaseExcludeListButton";
			this.ProperCaseExcludeListButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.ProperCaseExcludeListButton.TabIndex = 7;
			this.ProperCaseExcludeListButton.UseVisualStyleBackColor = true;
			this.ProperCaseExcludeListButton.Click += new System.EventHandler(this.ProperCaseExcludeListButton_Click);
			// 
			// CustomMapListsButton
			// 
			this.CustomMapListsButton.AutoSize = true;
			this.CustomMapListsButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|5e483d97-6b14-4e06-9bbd-a85ce4b6a4d6", "Custom Map Lists ...");
			this.CustomMapListsButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.CustomMapListsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 0, true);
			this.CustomMapListsButton.Name = "CustomMapListsButton";
			this.CustomMapListsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.CustomMapListsButton.TabIndex = 8;
			this.CustomMapListsButton.UseVisualStyleBackColor = true;
			this.CustomMapListsButton.Click += new System.EventHandler(this.CustomMapListsButton_Click);
			// 
			// ToGrid
			// 
			this.ToGrid.AllowDrop = true;
			this.ToGrid.AllowNavigation = false;
			this.ToGrid.AllowSorting = false;
			this.ToGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ToGrid.DisableImportDataMenuItem = true;
			this.BindingSource.SetBindingMember(this.ToGrid, "Mapping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).Text)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).MappedFrom)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).MappedField)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).MapAs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).Delimiter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).MapAsLookup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).CustomMapList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).CustomMapLists)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).DefaultValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).Expression)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).ProperCase)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.DataMapping.ImportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Mapping)).SyncRoot)).WesternCharactersOnly)));
			this.ToGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|3bf40f40-77f7-4743-8008-15b76b33958e", "Column");
			zTextBoxColumnStyleInfo1.ColumnName = "Text";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|1E3D5330-B077-4889-AD49-3E2F9CF5C470", "Mapped field");
			zTextBoxColumnStyleInfo5.ColumnName = "MappedField";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|d0332367-a9c0-44e2-9f1c-187a47bd9bc6", "Mapped from");
			zTextBoxColumnStyleInfo2.ColumnName = "MappedFrom";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.BindToList = "MapAsLookup";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|6c6ecf9c-8329-4a11-a5c8-cda08de93f48", "Map As");
			zDropEditColumnStyleInfo1.ColumnName = "MapAs";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.BindToList = "Delimiters";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|CD763AF4-90FD-4887-BC2C-B858FBB95DF7", "Delimiter");
			zDropEditColumnStyleInfo3.ColumnName = "Delimiter";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "CustomMapLists";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|987a5f2a-f0b9-4034-a6a3-8157300b4036", "Custom Map List");
			zDropEditColumnStyleInfo2.ColumnName = "CustomMapList";
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|aef4d5cd-6dfe-4d81-8cd1-184229b3d05b", "Default");
			zTextBoxColumnStyleInfo3.ColumnName = "DefaultValue";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|c7cf19f4-357a-4efe-8bdc-2f6a61258c46", "Expression");
			zTextBoxColumnStyleInfo4.ColumnName = "Expression";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|cab6ef8f-a559-41af-88b9-d1ebef001ae5", "Proper Case");
			zCheckBoxColumnStyleInfo1.ColumnName = "ProperCase";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|7783a244-3f19-4733-a43b-3587e1ac87535", "Match for Update");
			zCheckBoxColumnStyleInfo2.ColumnName = "UpdateExisting";
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|5f11135e-8abe-4a3d-b10d-6aaca1cf3709", "Western characters only");
			zCheckBoxColumnStyleInfo3.ColumnName = "WesternCharactersOnly";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ToGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ToGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ToGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ToGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ToGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ToGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ToGrid.CopySelectedRowsAllowed = true;
			this.ToGrid.GridId = "ce4a7608-a7b4-4a5f-b37d-6a1a47891f4f";
			this.ToGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ToGrid.LayoutKey = "zGrid1";
			this.ToGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 37, true);
			this.ToGrid.Name = "ToGrid";
			this.ToGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ToGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 135, true);
			this.ToGrid.TabIndex = 3;
			this.ToGrid.DragEnter += new System.Windows.Forms.DragEventHandler(this.ToGrid_DragEnter_DragOver);
			this.ToGrid.DragOver += new System.Windows.Forms.DragEventHandler(this.ToGrid_DragEnter_DragOver);
			this.ToGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.ToGrid_DragDrop);
			// 
			// ClearMapButton
			// 
			this.ClearMapButton.AutoSize = true;
			this.ClearMapButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|2b23b1a2-de23-4d06-b903-8b0c0d33d913", "Clear Map");
			this.ClearMapButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.ClearMapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClearMapButton.Name = "ClearMapButton";
			this.ClearMapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.ClearMapButton.TabIndex = 6;
			this.ClearMapButton.UseVisualStyleBackColor = true;
			this.ClearMapButton.Click += new System.EventHandler(this.ClearMapButton_Click);
			// 
			// ToLabel
			// 
			this.ToLabel.AutoSize = true;
			this.ToLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|ff212426-3121-469f-b546-160fd6bea68c", "To");
			this.ToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 21, true);
			this.ToLabel.Name = "ToLabel";
			this.ToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.ToLabel.TabIndex = 2;
			// 
			// FromNextButton
			// 
			this.FromNextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FromNextButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|bed76fb1-a17b-4d33-a36a-4b4bcf28d079", "Next >");
			this.FromNextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 173, true);
			this.FromNextButton.Name = "FromNextButton";
			this.FromNextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FromNextButton.TabIndex = 5;
			this.FromNextButton.UseVisualStyleBackColor = true;
			this.FromNextButton.Click += new System.EventHandler(this.FromNextButton_Click);
			// 
			// FromPrevButton
			// 
			this.FromPrevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.FromPrevButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|e537fbea-64da-4d82-9f5c-338212309ba7", "< Previous");
			this.FromPrevButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 173, true);
			this.FromPrevButton.Name = "FromPrevButton";
			this.FromPrevButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.FromPrevButton.TabIndex = 4;
			this.FromPrevButton.UseVisualStyleBackColor = true;
			this.FromPrevButton.Click += new System.EventHandler(this.FromPrevButton_Click);
			// 
			// FromListView
			// 
			this.FromListView.AllowDrop = true;
			this.FromListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.FromListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			columnHeader1,
			columnHeader5});
			this.FromListView.FullRowSelect = true;
			this.FromListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 37, true);
			this.FromListView.MultiSelect = false;
			this.FromListView.Name = "FromListView";
			this.FromListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 135, true);
			this.FromListView.TabIndex = 1;
			this.FromListView.UseCompatibleStateImageBehavior = false;
			this.FromListView.View = System.Windows.Forms.View.Details;
			this.FromListView.VirtualMode = true;
			this.FromListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.FromListView_DragDrop);
			this.FromListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.FromListView_RetrieveVirtualItem);
			this.FromListView.DragEnter += new System.Windows.Forms.DragEventHandler(this.FromListView_DragEnter);
			this.FromListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.FromListView_ItemDrag);
			// 
			// FromLabel
			// 
			this.FromLabel.AutoSize = true;
			this.FromLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|c1b922f9-defb-4edc-a254-037ca3a4e543", "From");
			this.FromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 21, true);
			this.FromLabel.Name = "FromLabel";
			this.FromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 13, true);
			this.FromLabel.TabIndex = 0;
			// 
			// PreviewTabPage
			// 
			this.PreviewTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|9e03b8f5-37fd-429f-8c3f-477d699fa566", "Preview");
			this.PreviewTabPage.Controls.Add(this.PreviewGrid);
			this.PreviewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviewTabPage.Name = "PreviewTabPage";
			this.PreviewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 202, true);
			this.PreviewTabPage.TabIndex = 2;
			// 
			// PreviewGrid
			// 
			this.PreviewGrid.AllowNavigation = false;
			this.PreviewGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PreviewGrid, "Preview");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).Preview)));
			this.PreviewGrid.CaptionVisible = false;
			this.PreviewGrid.CopySelectedRowsAllowed = true;
			this.PreviewGrid.GridId = "9b7212f0-8fac-454b-aea2-b0ed421e570e";
			this.PreviewGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviewGrid.LayoutKey = "DataImportWizardPreviewGrid";
			this.PreviewGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.PreviewGrid.Name = "PreviewGrid";
			this.PreviewGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PreviewGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 187, true);
			this.PreviewGrid.TabIndex = 0;
			// 
			// ValidateAndSaveTabPage
			// 
			this.ValidateAndSaveTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|0fba768b-534d-4895-ab10-5ece4e8d6912", "Validate && Save");
			this.ValidateAndSaveTabPage.Controls.Add(this.validateAndSaveControl);
			this.ValidateAndSaveTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ValidateAndSaveTabPage.Name = "ValidateAndSaveTabPage";
			this.ValidateAndSaveTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ValidateAndSaveTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 199, true);
			this.ValidateAndSaveTabPage.TabIndex = 3;
			// 
			// validateAndSaveControl
			// 
			this.validateAndSaveControl.AllowDrop = true;
			this.validateAndSaveControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.validateAndSaveControl, "CollectionInfo.Collection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.IBusinessObjectCollection)(((Enterprise.ZArchitecture.DataMapping.ImportWizard)(null)).CollectionInfo.Collection)));
			this.validateAndSaveControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.validateAndSaveControl.Name = "validateAndSaveControl";
			this.validateAndSaveControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 190, true);
			this.validateAndSaveControl.TabIndex = 0;
			// 
			// PreviousButton
			// 
			this.PreviousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviousButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|93a4d774-272b-44ae-9ce2-8e9e369e13b6", "Previous");
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 244, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.PreviousButton.TabIndex = 1;
			this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NextButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|f715f6c2-748a-4f68-9898-88e890ca6fd4", "Next");
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 244, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.NextButton.TabIndex = 2;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|8dfd5c1b-e40d-4b06-9477-34d80ac61f3f", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 244, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.CancelButtonX.TabIndex = 3;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// DataImportWizardForm
			// 
			this.AcceptButton = this.NextButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataImportWizardForm|fdf46bef-1ba3-4691-9f93-eabff83ae15b", "Data Import Wizard");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 306, true);
			this.Controls.Add(this.PreviousButton);
			this.Controls.Add(this.NextButton);
			this.Controls.Add(this.CancelButtonX);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ImportWizard);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.DataMapping.ImportWizard";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 340, true);
			this.Name = "DataImportWizardForm";
			this.ShouldSerializeTabPageMethods = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.NextButton, 0);
			this.Controls.SetChildIndex(this.PreviousButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.FileTabPage.ResumeLayout(false);
			this.FileTabPage.PerformLayout();
			this.zPanel3.ResumeLayout(false);
			this.zPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StartingRowNumericUpDown)).EndInit();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.MappingTabPage.ResumeLayout(false);
			this.MappingTabPage.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ToGrid)).EndInit();
			this.PreviewTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PreviewGrid)).EndInit();
			this.ValidateAndSaveTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected ZTabControl MainTabControl;
		private ZTabPage FileTabPage;
		private ZTabPage MappingTabPage;
		private ZButton PreviousButton;
		private ZButton NextButton;
		private ZButton CancelButtonX;
		private ZButton BrowseButton;
		private ZTextBox FileTextBox;
		private CargoWise.Windows.UI.KListView PreviewListView;
		private ZDropEdit DelimiterDropEdit;
		private ZCheckBox UseCurrentCountryNumberFormattingCheckBox;
		private CargoWise.Windows.UI.KListView FromListView;
		private ZLabel FromLabel;
		private ZButton FromNextButton;
		private ZButton FromPrevButton;
		private ZLabel ToLabel;
		private ZButton ClearMapButton;
		private ZTabPage PreviewTabPage;
		private ZGrid PreviewGrid;
		private ZNumericUpDown StartingRowNumericUpDown;
		protected ZGrid ToGrid;
		private ZButton CustomMapListsButton;
		private SettingsControl SettingsControl;
		private ZTabPage ValidateAndSaveTabPage;
		private DataImportWizardValidateAndSaveControl validateAndSaveControl;
		private ZButton ProperCaseExcludeListButton;
		private ZPanel zPanel1;
		private ZPanel zPanel2;
		private ZLabel zLabel1;
		private ZPanel zPanel3;
		private ZLabel zLabel2;
		private ZPanel zPanel4;
	}
}
