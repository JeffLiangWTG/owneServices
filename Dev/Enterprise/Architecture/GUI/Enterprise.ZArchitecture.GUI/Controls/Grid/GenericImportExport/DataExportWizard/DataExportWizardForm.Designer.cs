namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class DataExportWizardForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ColumnHeader columnHeader2;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.PreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.FileTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AppendExtraNewLineToEndOfFileCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseTextQualifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FileNameExpressionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.fixedWidthCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IncludeHeadersCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DelimiterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EncodingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BrowseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FileTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SettingsControl = new Enterprise.ZArchitecture.GUI.DataMapping.SettingsControl();
			this.MappingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.FromLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromListView = new CargoWise.Windows.UI.KListView();
			this.ToLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomMapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearMapButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MoveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.rowTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PreviewListView = new CargoWise.Windows.UI.KListView();
			columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.FileTabPage.SuspendLayout();
			this.MappingTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ToGrid)).BeginInit();
			this.PreviewTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 285, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ExportWizard);
			// 
			// columnHeader1
			// 
			columnHeader1.Text = "Column";
			columnHeader1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			// 
			// columnHeader2
			// 
			columnHeader2.Text = "";
			columnHeader2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			// 
			// PreviousButton
			// 
			this.PreviousButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PreviousButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|feb02657-6c46-4935-9a12-aca412c666bc", "Previous");
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 255, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.PreviousButton.TabIndex = 1;
			this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
			// 
			// NextButton
			// 
			this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NextButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|dc6662b4-3699-4366-a49c-b2d0b92709d1", "Next");
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 255, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.NextButton.TabIndex = 2;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|de24811a-e3e4-4084-8cb4-c22ca855e556", "Cancel");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(717, 255, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 24, true);
			this.CancelButtonX.TabIndex = 3;
			this.CancelButtonX.Click += new System.EventHandler(this.CancelButtonX_Click);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.FileTabPage);
			this.MainTabControl.Controls.Add(this.MappingTabPage);
			this.MainTabControl.Controls.Add(this.PreviewTabPage);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 237, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.SelectedIndexChanging += new System.EventHandler(this.MainTabControl_SelectedIndexChanging);
			// 
			// FileTabPage
			// 
			this.FileTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|e789e466-b8f1-42b8-b279-1eb02ec00ca8", "File");
			this.FileTabPage.Controls.Add(this.AppendExtraNewLineToEndOfFileCheckBox);
			this.FileTabPage.Controls.Add(this.UseTextQualifierDropEdit);
			this.FileTabPage.Controls.Add(this.FileNameExpressionTextBox);
			this.FileTabPage.Controls.Add(this.fixedWidthCheckBox);
			this.FileTabPage.Controls.Add(this.IncludeHeadersCheckBox);
			this.FileTabPage.Controls.Add(this.DelimiterDropEdit);
			this.FileTabPage.Controls.Add(this.EncodingDropEdit);
			this.FileTabPage.Controls.Add(this.BrowseButton);
			this.FileTabPage.Controls.Add(this.FileTextBox);
			this.FileTabPage.Controls.Add(this.SettingsControl);
			this.FileTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.FileTabPage.Name = "FileTabPage";
			this.FileTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.FileTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 210, true);
			this.FileTabPage.TabIndex = 0;
			// 
			// AppendExtraNewLineToEndOfFileCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AppendExtraNewLineToEndOfFileCheckBox, "AppendExtraNewLineToEndOfFile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).AppendExtraNewLineToEndOfFile)));
			this.AppendExtraNewLineToEndOfFileCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|113cd81e-563c-4f6c-bca0-9470ac06afb8", "Add new line to end of file");
			this.AppendExtraNewLineToEndOfFileCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AppendExtraNewLineToEndOfFileCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 89, true);
			this.AppendExtraNewLineToEndOfFileCheckBox.Name = "AppendExtraNewLineToEndOfFileCheckBox";
			this.AppendExtraNewLineToEndOfFileCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 17, true);
			this.AppendExtraNewLineToEndOfFileCheckBox.TabIndex = 9;
			this.AppendExtraNewLineToEndOfFileCheckBox.UseVisualStyleBackColor = true;
			// 
			// UseTextQualifierDropEdit
			// 
			this.UseTextQualifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UseTextQualifierDropEdit, "UseTextQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).UseTextQualifier)));
			this.UseTextQualifierDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|6db8831f-6878-4f0e-a453-7361f407efc6", "Quotes", "Quotes", "Quotes", "");
			this.UseTextQualifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 167, true);
			this.UseTextQualifierDropEdit.Name = "UseTextQualifierDropEdit";
			this.UseTextQualifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 20, true);
			this.UseTextQualifierDropEdit.TabIndex = 7;
			// 
			// FileNameExpressionTextBox
			// 
			this.BindingSource.SetBindingMember(this.FileNameExpressionTextBox, "FileNameExpression");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).FileNameExpression)));
			this.FileNameExpressionTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|740b65e2-4661-4a78-880b-226b4be69f49", "Filename Expression");
			this.FileNameExpressionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileNameExpressionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 193, true);
			this.FileNameExpressionTextBox.Name = "FileNameExpressionTextBox";
			this.FileNameExpressionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 20, true);
			this.FileNameExpressionTextBox.TabIndex = 8;
			// 
			// fixedWidthCheckBox
			// 
			this.fixedWidthCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.fixedWidthCheckBox, "FixedWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).FixedWidth)));
			this.fixedWidthCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|14ad68b5-da56-4f11-acd8-45ed4f899c1c", "Fixed Width");
			this.fixedWidthCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fixedWidthCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 89, true);
			this.fixedWidthCheckBox.Name = "fixedWidthCheckBox";
			this.fixedWidthCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.fixedWidthCheckBox.TabIndex = 4;
			this.fixedWidthCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncludeHeadersCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IncludeHeadersCheckBox, "IncludeHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).IncludeHeaders)));
			this.IncludeHeadersCheckBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|4153ff4e-26fc-47d1-9e62-9be2284775fc", "Include Headers");
			this.IncludeHeadersCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeHeadersCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 85, true);
			this.IncludeHeadersCheckBox.Name = "IncludeHeadersCheckBox";
			this.IncludeHeadersCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IncludeHeadersCheckBox.TabIndex = 6;
			this.IncludeHeadersCheckBox.UseVisualStyleBackColor = true;
			// 
			// DelimiterDropEdit
			// 
			this.DelimiterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DelimiterDropEdit, "Delimiter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).Delimiter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).Delimiters)));
			this.DelimiterDropEdit.BindToList = "Delimiters";
			this.DelimiterDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|45c28799-d15c-4619-bff5-2f60db117a25", "Delimiter");
			this.DelimiterDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DelimiterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 115, true);
			this.DelimiterDropEdit.Name = "DelimiterDropEdit";
			this.DelimiterDropEdit.PreBoundMaxLength = 5;
			this.DelimiterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 20, true);
			this.DelimiterDropEdit.TabIndex = 3;
			// 
			// EncodingDropEdit
			// 
			this.EncodingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EncodingDropEdit, "EncodingType");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).EncodingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).EncodingTypes)));
			this.EncodingDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|9613E92E-6468-4A40-8CC0-4E233537400F", "Encoding");
			this.EncodingDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.EncodingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 141, true);
			this.EncodingDropEdit.Name = "EncodingDropEdit";
			this.EncodingDropEdit.PreBoundMaxLength = 5;
			this.EncodingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 20, true);
			this.EncodingDropEdit.TabIndex = 5;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|8b2a1d1e-b10c-4b1e-aa04-95ec48087cb2", "Browse...");
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 62, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.BrowseButton.TabIndex = 2;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).FileName)));
			this.FileTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|29941d85-9b53-4fbd-9e9b-73cfc373bb47", "File to export as");
			this.FileTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.FileTextBox.Name = "FileTextBox";
			this.FileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 20, true);
			this.FileTextBox.TabIndex = 1;
			// 
			// SettingsControl
			// 
			this.SettingsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SettingsControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.DataMapping.ImportExportWizard)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)))));
			this.SettingsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 6, true);
			this.SettingsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 49, true);
			this.SettingsControl.Name = "SettingsControl";
			this.SettingsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 49, true);
			this.SettingsControl.TabIndex = 0;
			// 
			// MappingTabPage
			// 
			this.MappingTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|016455f9-6774-498d-8d63-41a39aed3585", "Mapping");
			this.MappingTabPage.Controls.Add(this.splitContainer1);
			this.MappingTabPage.Controls.Add(this.rowTypeDropEdit);
			this.MappingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MappingTabPage.Name = "MappingTabPage";
			this.MappingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MappingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 210, true);
			this.MappingTabPage.TabIndex = 1;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 32, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.FromLabel);
			this.splitContainer1.Panel1.Controls.Add(this.FromListView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ToLabel);
			this.splitContainer1.Panel2.Controls.Add(this.CustomMapButton);
			this.splitContainer1.Panel2.Controls.Add(this.ClearMapButton);
			this.splitContainer1.Panel2.Controls.Add(this.MoveDownButton);
			this.splitContainer1.Panel2.Controls.Add(this.ToGrid);
			this.splitContainer1.Panel2.Controls.Add(this.MoveUpButton);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 172, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(263);
			this.splitContainer1.TabIndex = 8;
			// 
			// FromLabel
			// 
			this.FromLabel.AutoSize = true;
			this.FromLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|c1b922f9-defb-4edc-a254-037ca3a4e543", "From");
			this.FromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FromLabel.Name = "FromLabel";
			this.FromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.FromLabel.TabIndex = 1;
			// 
			// FromListView
			// 
			this.FromListView.AllowDrop = true;
			this.FromListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FromListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
						columnHeader1,
						columnHeader2});
			this.FromListView.FullRowSelect = true;
			this.FromListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.FromListView.MultiSelect = false;
			this.FromListView.Name = "FromListView";
			this.FromListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 153, true);
			this.FromListView.TabIndex = 3;
			this.FromListView.UseCompatibleStateImageBehavior = false;
			this.FromListView.View = System.Windows.Forms.View.Details;
			this.FromListView.VirtualMode = true;
			this.FromListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.FromListView_ItemDrag);
			this.FromListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.FromListView_RetrieveVirtualItem);
			this.FromListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.FromListView_DragDrop);
			this.FromListView.DragEnter += new System.Windows.Forms.DragEventHandler(this.FromListView_DragEnter);
			// 
			// ToLabel
			// 
			this.ToLabel.AutoSize = true;
			this.ToLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|ff212426-3121-469f-b546-160fd6bea68c", "To");
			this.ToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToLabel.Name = "ToLabel";
			this.ToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ToLabel.TabIndex = 2;
			// 
			// CustomMapButton
			// 
			this.CustomMapButton.AutoSize = true;
			this.CustomMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CustomMapButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|8B0C6FB4-7CFB-40DE-8777-184CC03C7288", "Custom Map List...");
			this.CustomMapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 146, true);
			this.CustomMapButton.Name = "CustomMapButton";
			this.CustomMapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 23, true);
			this.CustomMapButton.TabIndex = 8;
			this.CustomMapButton.UseVisualStyleBackColor = true;
			this.CustomMapButton.Click += new System.EventHandler(this.CustomMapButton_Click);
			// 
			// ClearMapButton
			// 
			this.ClearMapButton.AutoSize = true;
			this.ClearMapButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ClearMapButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|2b23b1a2-de23-4d06-b903-8b0c0d33d913", "Clear Map");
			this.ClearMapButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 146, true);
			this.ClearMapButton.Name = "ClearMapButton";
			this.ClearMapButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.ClearMapButton.TabIndex = 7;
			this.ClearMapButton.UseVisualStyleBackColor = true;
			this.ClearMapButton.Click += new System.EventHandler(this.ClearMapButton_Click);
			// 
			// MoveDownButton
			// 
			this.MoveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MoveDownButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|2d6feac9-79f5-4330-b745-77e59257f7e6", "Move &Down");
			this.MoveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 117, true);
			this.MoveDownButton.Name = "MoveDownButton";
			this.MoveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.MoveDownButton.TabIndex = 6;
			this.MoveDownButton.UseVisualStyleBackColor = true;
			this.MoveDownButton.Click += new System.EventHandler(this.MoveDownButton_Click);
			// 
			// ToGrid
			// 
			this.ToGrid.AllowDrop = true;
			this.ToGrid.AllowNavigation = false;
			this.ToGrid.AllowSorting = false;
			this.ToGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ToGrid, "MappingView");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).Text)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).Header)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).MapAs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).Expression)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).AlignmentString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).CustomMapList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).CustomMapLists)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.ExportWizardMapping)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).MappingView)).SyncRoot)).ConditionExpr)));
			this.ToGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|3bf40f40-77f7-4743-8008-15b76b33958e", "Column");
			zTextBoxColumnStyleInfo1.ColumnName = "Text";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|75967627-989b-4095-97d8-ecd89fb48b49", "Header");
			zTextBoxColumnStyleInfo2.ColumnName = "Header";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|6c6ecf9c-8329-4a11-a5c8-cda08de93f48", "Map As");
			zDropEditColumnStyleInfo1.ColumnName = "MapAs";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|2f9e5982-ece7-4297-88f7-059142b71547", "Expression");
			zMultiLineTextBoxColumnInfo1.ColumnName = "Expression";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|2ce16b32-4ff8-45a8-b79e-81c46a58b1b5", "Width");
			zCalcEditColumnStyleInfo1.ColumnName = "Width";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|352692a1-7ddf-4e60-bd6f-600b6ed6fe8e", "Alignment");
			zDropEditColumnStyleInfo2.ColumnName = "AlignmentString";
			zDropEditColumnStyleInfo3.BindToList = "CustomMapLists";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|987a5f2a-f0b9-4034-a6a3-8157300b4036", "Custom Map List");
			zDropEditColumnStyleInfo3.ColumnName = "CustomMapList";
			zDropEditColumnStyleInfo3.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|b8041ed4-ca35-41ac-ae1d-70803b9e5b61", "Condition");
			zMultiLineTextBoxColumnInfo2.ColumnName = "ConditionExpr";
			zMultiLineTextBoxColumnInfo2.IsVisible = false;
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ToGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ToGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ToGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ToGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ToGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ToGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ToGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.ToGrid.CopySelectedRowsAllowed = true;
			this.ToGrid.GridId = "ce4a7608-a7b4-4a5f-b37d-6a1a47891f4f";
			this.ToGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ToGrid.LayoutKey = "zGrid1";
			this.ToGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ToGrid.Name = "ToGrid";
			this.ToGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 124, true);
			this.ToGrid.TabIndex = 4;
			this.ToGrid.DragDrop += new System.Windows.Forms.DragEventHandler(this.ToGrid_DragDrop);
			this.ToGrid.DragEnter += new System.Windows.Forms.DragEventHandler(this.ToGrid_DragEnter);
			// 
			// MoveUpButton
			// 
			this.MoveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.MoveUpButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|ad60d35c-fb5d-4a1f-b686-ff53d92056ff", "Move &Up");
			this.MoveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 88, true);
			this.MoveUpButton.Name = "MoveUpButton";
			this.MoveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.MoveUpButton.TabIndex = 5;
			this.MoveUpButton.UseVisualStyleBackColor = true;
			this.MoveUpButton.Click += new System.EventHandler(this.MoveUpButton_Click);
			// 
			// rowTypeDropEdit
			// 
			this.rowTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rowTypeDropEdit, "RowTypeNameFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.ExportWizard)(null)).RowTypeNameFilter)));
			this.rowTypeDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|5c2f9cb7-c118-4dd5-8831-06383d55bc27", "Row Type");
			this.rowTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.rowTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 6, true);
			this.rowTypeDropEdit.Name = "rowTypeDropEdit";
			this.rowTypeDropEdit.PreBoundMaxLength = 30;
			this.rowTypeDropEdit.ShowDescriptionBox = false;
			this.rowTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.rowTypeDropEdit.TabIndex = 0;
			// 
			// PreviewTabPage
			// 
			this.PreviewTabPage.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|b8c7ce1b-028c-4e45-9f13-40fddbcd7b87", "Preview");
			this.PreviewTabPage.Controls.Add(this.PreviewListView);
			this.PreviewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PreviewTabPage.Name = "PreviewTabPage";
			this.PreviewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PreviewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 210, true);
			this.PreviewTabPage.TabIndex = 2;
			// 
			// PreviewListView
			// 
			this.PreviewListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PreviewListView.GridLines = true;
			this.PreviewListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.PreviewListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.PreviewListView.Name = "PreviewListView";
			this.PreviewListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 195, true);
			this.PreviewListView.TabIndex = 9;
			this.PreviewListView.UseCompatibleStateImageBehavior = false;
			this.PreviewListView.View = System.Windows.Forms.View.Details;
			// 
			// DataExportWizardForm
			// 
			this.AcceptButton = this.NextButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelButtonX;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("DataExportWizardForm|b378ede4-2f33-4bec-bf62-4bc640ad3af8", "Data Export Wizard");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 309, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.PreviousButton);
			this.Controls.Add(this.NextButton);
			this.Controls.Add(this.CancelButtonX);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.ExportWizard);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 343, true);
			this.Name = "DataExportWizardForm";
			this.ShouldSerializeTabPageMethods = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.NextButton, 0);
			this.Controls.SetChildIndex(this.PreviousButton, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.FileTabPage.ResumeLayout(false);
			this.FileTabPage.PerformLayout();
			this.MappingTabPage.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ToGrid)).EndInit();
			this.PreviewTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZButton PreviousButton;
		private ZButton NextButton;
		private ZButton CancelButtonX;
		private ZTabControl MainTabControl;
		private ZTabPage FileTabPage;
		private ZTabPage MappingTabPage;
		private ZTabPage PreviewTabPage;
		private SettingsControl SettingsControl;
		private ZDropEdit DelimiterDropEdit;
		private ZDropEdit EncodingDropEdit;
		private ZButton BrowseButton;
		private ZTextBox FileTextBox;
		private ZCheckBox IncludeHeadersCheckBox;
		private ZGrid ToGrid;
		private ZButton CustomMapButton;
		private ZButton ClearMapButton;
		private ZLabel ToLabel;
		private CargoWise.Windows.UI.KListView FromListView;
		private ZLabel FromLabel;
		private CargoWise.Windows.UI.KListView PreviewListView;
		private ZButton MoveDownButton;
		private ZButton MoveUpButton;
		private ZCheckBox fixedWidthCheckBox;
		private ZDropEdit rowTypeDropEdit;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZTextBox FileNameExpressionTextBox;
		private ZDropEdit UseTextQualifierDropEdit;
		private ZCheckBox AppendExtraNewLineToEndOfFileCheckBox;
		System.Windows.Forms.ColumnHeader columnHeader1;
	}
}
