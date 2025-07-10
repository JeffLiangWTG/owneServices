namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	partial class StmMenuDocumentConfigForm
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
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.systemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.templateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.excludedFromDocPackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.sectionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.sectionsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.previewSectionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.addButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sectionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.sectionsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.previewConfigItemButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.removeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.moveDownButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.moveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.configItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.configItemsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.titleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.menuItemNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.menuItemAndTemplateDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.companyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.clientFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.previewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.pageStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverrideDataContextDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverrideEmailSubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.sectionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sectionsSplitContainer)).BeginInit();
			this.sectionsSplitContainer.Panel1.SuspendLayout();
			this.sectionsSplitContainer.Panel2.SuspendLayout();
			this.sectionsSplitContainer.SuspendLayout();
			this.CategoryDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sectionsGrid)).BeginInit();
			this.sectionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.configItemsGrid)).BeginInit();
			this.configItemsGrid.SuspendLayout();
			this.menuItemAndTemplateDetailsGroupBox.SuspendLayout();
			this.companyFindBox.SuspendLayout();
			this.clientFindBox.SuspendLayout();
			this.pageStyleDropEdit.SuspendLayout();
			this.OverrideDataContextDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 541, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 24, true);
			this.MainStatusBar.TabIndex = 13;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig);
			// 
			// systemCheckBox
			// 
			this.systemCheckBox.AutoSize = true;
			this.systemCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.systemCheckBox, "S3_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_IsSystem)));
			this.systemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.systemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 11, true);
			this.systemCheckBox.Name = "systemCheckBox";
			this.systemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.systemCheckBox.TabIndex = 0;
			this.systemCheckBox.UseVisualStyleBackColor = false;
			// 
			// templateCheckBox
			// 
			this.templateCheckBox.AutoSize = true;
			this.templateCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.templateCheckBox, "S3_IsTemplate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_IsTemplate)));
			this.templateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.templateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 11, true);
			this.templateCheckBox.Name = "templateCheckBox";
			this.templateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.templateCheckBox.TabIndex = 1;
			this.templateCheckBox.UseVisualStyleBackColor = false;
			// 
			// excludedFromDocPackCheckBox
			// 
			this.excludedFromDocPackCheckBox.AutoSize = true;
			this.excludedFromDocPackCheckBox.BackColor = System.Drawing.SystemColors.Control;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_ExcludedFromDocPack)));
			this.excludedFromDocPackCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.excludedFromDocPackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 11, true);
			this.excludedFromDocPackCheckBox.Name = "excludedFromDocPackCheckBox";
			this.excludedFromDocPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.excludedFromDocPackCheckBox.TabIndex = 6;
			this.excludedFromDocPackCheckBox.UseVisualStyleBackColor = false;
			this.BindingSource.SetBindingMember(this.excludedFromDocPackCheckBox, "S3_ExcludedFromDocPack");
			// 
			// sectionsGroupBox
			// 
			this.sectionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.sectionsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|9903b7e1-4251-4c6b-b096-8502005ae384", "Sections");
			this.sectionsGroupBox.Controls.Add(this.sectionsSplitContainer);
			this.sectionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 145, true);
			this.sectionsGroupBox.Name = "sectionsGroupBox";
			this.sectionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 361, true);
			this.sectionsGroupBox.TabIndex = 9;
			this.sectionsGroupBox.TabStop = false;
			// 
			// sectionsSplitContainer
			// 
			this.sectionsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sectionsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.sectionsSplitContainer.Name = "sectionsSplitContainer";
			// 
			// sectionsSplitContainer.Panel1
			// 
			this.sectionsSplitContainer.Panel1.Controls.Add(this.previewSectionButton);
			this.sectionsSplitContainer.Panel1.Controls.Add(this.CategoryDropEdit);
			this.sectionsSplitContainer.Panel1.Controls.Add(this.addButton);
			this.sectionsSplitContainer.Panel1.Controls.Add(this.sectionsGrid);
			this.sectionsSplitContainer.Panel1.Controls.Add(this.sectionsLabel);
			// 
			// sectionsSplitContainer.Panel2
			// 
			this.sectionsSplitContainer.Panel2.Controls.Add(this.previewConfigItemButton);
			this.sectionsSplitContainer.Panel2.Controls.Add(this.removeButton);
			this.sectionsSplitContainer.Panel2.Controls.Add(this.moveDownButton);
			this.sectionsSplitContainer.Panel2.Controls.Add(this.moveUpButton);
			this.sectionsSplitContainer.Panel2.Controls.Add(this.configItemsGrid);
			this.sectionsSplitContainer.Panel2.Controls.Add(this.configItemsLabel);
			this.sectionsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 342, true);
			this.sectionsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(423);
			this.sectionsSplitContainer.TabIndex = 0;
			// 
			// previewSectionButton
			// 
			this.previewSectionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.previewSectionButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|4877a7f5-8019-47f1-a15a-a9075f2449dc", "Preview Section");
			this.previewSectionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 26, true);
			this.previewSectionButton.Name = "previewSectionButton";
			this.previewSectionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.previewSectionButton.TabIndex = 2;
			this.previewSectionButton.UseVisualStyleBackColor = true;
			// 
			// CategoryDropEdit
			// 
			this.CategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CategoryFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).CategoryFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).Categories)));
			this.CategoryDropEdit.BindToList = "Categories";
			this.CategoryDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|deb80fc9-163b-4372-86be-8c0b4db5d7d9", "Cat. Filter", "Filter by Category", "Select a Category you would like to filter the \'Sections\' grid by");
			this.CategoryDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 27, true);
			this.CategoryDropEdit.Name = "CategoryDropEdit";
			this.CategoryDropEdit.PreBoundMaxLength = 15;
			this.CategoryDropEdit.ShowDescriptionBox = false;
			this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.CategoryDropEdit.TabIndex = 1;
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|2aa20870-d1a6-4780-9224-32da2370917e", "Add ->");
			this.addButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 26, true);
			this.addButton.Name = "addButton";
			this.addButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.addButton.TabIndex = 3;
			this.addButton.UseVisualStyleBackColor = true;
			// 
			// sectionsGrid
			// 
			this.sectionsGrid.AllowNavigation = false;
			this.sectionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.sectionsGrid, "AvailableSections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).AvailableSections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).AvailableSections)).SyncRoot)).Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).AvailableSections)).SyncRoot)).TypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.DocBuilder.TemplateSection)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).AvailableSections)).SyncRoot)).SectionName)));
			this.sectionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|e7c0d85f-f863-490f-9ae6-9d8c162fd1ce", "Category", "Category used for grouping in the Available Sections Grid");
			zTextBoxColumnStyleInfo1.ColumnName = "Category";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|8ec4204b-35e4-48ab-afb8-bf7f666ba765", "Type");
			zTextBoxColumnStyleInfo2.ColumnName = "TypeCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|48492b03-2a24-4d66-ba41-eb4da900b6eb", "Name");
			zTextBoxColumnStyleInfo3.ColumnName = "SectionName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.sectionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.sectionsGrid.CopySelectedRowsAllowed = true;
			this.sectionsGrid.GridId = "e0159fae-d064-4af9-8c24-a5e7028cd7f5";
			this.sectionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sectionsGrid.IsWholeRowSelectedOnClick = true;
			this.sectionsGrid.LayoutKey = "availableSectionsGrid";
			this.sectionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 53, true);
			this.sectionsGrid.Name = "sectionsGrid";
			this.sectionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 286, true);
			this.sectionsGrid.TabIndex = 4;
			// 
			// sectionsLabel
			// 
			this.sectionsLabel.AutoSize = true;
			this.sectionsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|06a3e60a-4a71-414e-93e7-d36a2f5aebd1", "Available Sections");
			this.sectionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.sectionsLabel.Name = "sectionsLabel";
			this.sectionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.sectionsLabel.TabIndex = 0;
			// 
			// previewConfigItemButton
			// 
			this.previewConfigItemButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|4405d1b6-ad23-469e-b800-a721587e01e2", "Preview Section");
			this.previewConfigItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 25, true);
			this.previewConfigItemButton.Name = "previewConfigItemButton";
			this.previewConfigItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.previewConfigItemButton.TabIndex = 2;
			this.previewConfigItemButton.UseVisualStyleBackColor = true;
			// 
			// removeButton
			// 
			this.removeButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|e033de92-550a-4163-8632-63a8b24274cb", "<- Remove");
			this.removeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 25, true);
			this.removeButton.Name = "removeButton";
			this.removeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.removeButton.TabIndex = 1;
			this.removeButton.UseVisualStyleBackColor = true;
			// 
			// moveDownButton
			// 
			this.moveDownButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moveDownButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|a5001665-0744-4985-bf13-c8e99fbf4a17", "Move Down");
			this.moveDownButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(354, 25, true);
			this.moveDownButton.Name = "moveDownButton";
			this.moveDownButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.moveDownButton.TabIndex = 4;
			this.moveDownButton.UseVisualStyleBackColor = true;
			// 
			// moveUpButton
			// 
			this.moveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.moveUpButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|d2b05ebe-bd15-464d-a16d-05b2ead33210", "Move Up");
			this.moveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 25, true);
			this.moveUpButton.Name = "moveUpButton";
			this.moveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.moveUpButton.TabIndex = 3;
			this.moveUpButton.UseVisualStyleBackColor = true;
			// 
			// configItemsGrid
			// 
			this.configItemsGrid.AllowNavigation = false;
			this.configItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.configItemsGrid, "ConfigItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).ConfigItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Business.StmMenuDocumentConfigItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).ConfigItems)).SyncRoot)).S4_PrintOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.StmMenuDocumentConfigItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).ConfigItems)).SyncRoot)).S4_SectionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.StmMenuDocumentConfigItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).ConfigItems)).SyncRoot)).Lookups.GenericSectionTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.StmMenuDocumentConfigItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).ConfigItems)).SyncRoot)).S4_SectionItemName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.StmMenuDocumentConfigItem)(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).ConfigItems)).SyncRoot)).S4_FilterList)));
			this.configItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "S4_PrintOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.GenericSectionTypes";
			zDropEditColumnStyleInfo1.ColumnName = "S4_SectionType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.ColumnName = "S4_SectionItemName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo5.ColumnName = "S4_FilterList";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.configItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.configItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.configItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.configItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.configItemsGrid.CopySelectedRowsAllowed = true;
			this.configItemsGrid.GridId = "dbf61082-3d4b-4c93-8f4f-b531c27eafc5";
			this.configItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.configItemsGrid.LayoutKey = "configItemsGrid";
			this.configItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 53, true);
			this.configItemsGrid.Name = "configItemsGrid";
			this.configItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 286, true);
			this.configItemsGrid.TabIndex = 5;
			// 
			// configItemsLabel
			// 
			this.configItemsLabel.AutoSize = true;
			this.configItemsLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|819bfd81-33cd-4e58-9337-7a3cb8d3ee04", "Shown Sections (in order)");
			this.configItemsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.configItemsLabel.Name = "configItemsLabel";
			this.configItemsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 13, true);
			this.configItemsLabel.TabIndex = 0;
			// 
			// titleTextBox
			// 
			this.BindingSource.SetBindingMember(this.titleTextBox, "S3_Calc_DocumentTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_Calc_DocumentTitle)));
			this.titleTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|f64f55ea-8124-4a09-bf18-f9a30473e979", "Title", "The Document Title that this config relates to.");
			this.titleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.titleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 45, true);
			this.titleTextBox.Name = "titleTextBox";
			this.titleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.titleTextBox.TabIndex = 1;
			// 
			// menuItemNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.menuItemNameTextBox, "S3_Calc_MenuName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_Calc_MenuName)));
			this.menuItemNameTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|9311deb8-34c9-4e99-a323-4ea5dbec4320", "Menu Item Name", "The Menu Item that this config item relates to.");
			this.menuItemNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.menuItemNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 19, true);
			this.menuItemNameTextBox.Name = "menuItemNameTextBox";
			this.menuItemNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.menuItemNameTextBox.TabIndex = 0;
			// 
			// menuItemAndTemplateDetailsGroupBox
			// 
			this.menuItemAndTemplateDetailsGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|dda34c94-af85-46c7-a7d5-084eeafc0d3a", "Menu Item && Template Details");
			this.menuItemAndTemplateDetailsGroupBox.Controls.Add(this.titleTextBox);
			this.menuItemAndTemplateDetailsGroupBox.Controls.Add(this.menuItemNameTextBox);
			this.menuItemAndTemplateDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 12, true);
			this.menuItemAndTemplateDetailsGroupBox.Name = "menuItemAndTemplateDetailsGroupBox";
			this.menuItemAndTemplateDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 76, true);
			this.menuItemAndTemplateDetailsGroupBox.TabIndex = 6;
			this.menuItemAndTemplateDetailsGroupBox.TabStop = false;
			// 
			// companyFindBox
			// 
			this.companyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.companyFindBox, "S3_GC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_GC)));
			this.companyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 61, true);
			this.companyFindBox.Name = "companyFindBox";
			this.companyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.companyFindBox.TabIndex = 3;
			// 
			// clientFindBox
			// 
			this.clientFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.clientFindBox, "S3_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_OH)));
			this.clientFindBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|f8da5141-d6f7-481a-9732-7a726c23b85e", "Default Recipient");
			this.clientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 94, true);
			this.clientFindBox.Name = "clientFindBox";
			this.clientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.clientFindBox.TabIndex = 4;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|864c3576-2a57-40ed-9bd5-9d4509d48ec6", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(720, 512, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 11;
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|1f0ba9ad-be2a-4c4d-984a-038412d412ab", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(801, 512, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 12;
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// previewButton
			// 
			this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.previewButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|0165fb43-e7f8-45a6-9909-074a60e6677d", "Preview Document");
			this.previewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(594, 512, true);
			this.previewButton.Name = "previewButton";
			this.previewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.previewButton.TabIndex = 10;
			this.previewButton.UseVisualStyleBackColor = true;
			// 
			// pageStyleDropEdit
			// 
			this.pageStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.pageStyleDropEdit, "S3_PageStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_PageStyle)));
			this.pageStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 119, true);
			this.pageStyleDropEdit.Name = "pageStyleDropEdit";
			this.pageStyleDropEdit.PreBoundMaxLength = 3;
			this.pageStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.pageStyleDropEdit.TabIndex = 5;
			// 
			// OverrideDataContextDropEdit
			// 
			this.OverrideDataContextDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverrideDataContextDropEdit, "S3_OverrideDataContext");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_OverrideDataContext)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).OverrideDataContextsList)));
			this.OverrideDataContextDropEdit.BindToList = "OverrideDataContextsList";
			this.OverrideDataContextDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|cac5f380-1044-4085-91e2-1dc4a55a595a", "Override Data Context", "Select an Overriding Data Context you would like to use for the document configuration.");
			this.OverrideDataContextDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverrideDataContextDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 119, true);
			this.OverrideDataContextDropEdit.Name = "OverrideDataContextDropEdit";
			this.OverrideDataContextDropEdit.PreBoundMaxLength = 15;
			this.OverrideDataContextDropEdit.ShowDescriptionBox = false;
			this.OverrideDataContextDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 20, true);
			this.OverrideDataContextDropEdit.TabIndex = 8;
			// 
			// OverrideEmailSubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideEmailSubjectTextBox, "S3_OverrideEmailSubject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_OverrideEmailSubject)));
			this.OverrideEmailSubjectTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("a791fe7a-7008-4301-ad22-945878762af6", "Override Document Name", "This field overrides the template configuration \'EMAILSUBJECT\', which is used for both initial email subject and document name.\r\nThis field will be used only to override the document name.");
			this.OverrideEmailSubjectTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverrideEmailSubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 94, true);
			this.OverrideEmailSubjectTextBox.Name = "OverrideEmailSubjectTextBox";
			this.OverrideEmailSubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, true);
			this.OverrideEmailSubjectTextBox.TabIndex = 7;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "S3_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig)(null)).S3_Description)));
			this.descriptionTextBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("b240e5ca-3a12-468d-8aa2-ee9de64b59f3", "Description");
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 34, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.descriptionTextBox.TabIndex = 2;
			// 
			// StmMenuDocumentConfigForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("StmMenuDocumentConfigForm|a54c81f9-19e7-4b8d-a69d-6d7c5f0ffaab", "Customizable Document Configuration");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 565, true);
			this.Controls.Add(this.descriptionTextBox);
			this.Controls.Add(this.OverrideEmailSubjectTextBox);
			this.Controls.Add(this.OverrideDataContextDropEdit);
			this.Controls.Add(this.excludedFromDocPackCheckBox);
			this.Controls.Add(this.templateCheckBox);
			this.Controls.Add(this.systemCheckBox);
			this.Controls.Add(this.pageStyleDropEdit);
			this.Controls.Add(this.clientFindBox);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.companyFindBox);
			this.Controls.Add(this.previewButton);
			this.Controls.Add(this.sectionsGroupBox);
			this.Controls.Add(this.menuItemAndTemplateDetailsGroupBox);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.Business.TemporaryStmMenuDocumentConfig";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(904, 570, true);
			this.Name = "StmMenuDocumentConfigForm";
			this.Controls.SetChildIndex(this.menuItemAndTemplateDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.sectionsGroupBox, 0);
			this.Controls.SetChildIndex(this.previewButton, 0);
			this.Controls.SetChildIndex(this.companyFindBox, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.clientFindBox, 0);
			this.Controls.SetChildIndex(this.pageStyleDropEdit, 0);
			this.Controls.SetChildIndex(this.systemCheckBox, 0);
			this.Controls.SetChildIndex(this.templateCheckBox, 0);
			this.Controls.SetChildIndex(this.excludedFromDocPackCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OverrideDataContextDropEdit, 0);
			this.Controls.SetChildIndex(this.OverrideEmailSubjectTextBox, 0);
			this.Controls.SetChildIndex(this.descriptionTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sectionsGroupBox.ResumeLayout(false);
			this.sectionsGroupBox.PerformLayout();
			this.sectionsSplitContainer.Panel1.ResumeLayout(false);
			this.sectionsSplitContainer.Panel1.PerformLayout();
			this.sectionsSplitContainer.Panel2.ResumeLayout(false);
			this.sectionsSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sectionsSplitContainer)).EndInit();
			this.sectionsSplitContainer.ResumeLayout(false);
			this.sectionsSplitContainer.PerformLayout();
			this.CategoryDropEdit.ResumeLayout(true);
			this.CategoryDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.sectionsGrid)).EndInit();
			this.sectionsGrid.ResumeLayout(false);
			this.sectionsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.configItemsGrid)).EndInit();
			this.configItemsGrid.ResumeLayout(false);
			this.configItemsGrid.PerformLayout();
			this.menuItemAndTemplateDetailsGroupBox.ResumeLayout(false);
			this.menuItemAndTemplateDetailsGroupBox.PerformLayout();
			this.companyFindBox.ResumeLayout(true);
			this.companyFindBox.PerformLayout();
			this.clientFindBox.ResumeLayout(true);
			this.clientFindBox.PerformLayout();
			this.pageStyleDropEdit.ResumeLayout(true);
			this.pageStyleDropEdit.PerformLayout();
			this.OverrideDataContextDropEdit.ResumeLayout(true);
			this.OverrideDataContextDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox systemCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox templateCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox excludedFromDocPackCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox sectionsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton addButton;
		private Enterprise.ZArchitecture.GUI.ZButton removeButton;
		private Enterprise.ZArchitecture.ZLabel configItemsLabel;
		private Enterprise.ZArchitecture.ZLabel sectionsLabel;
		private Enterprise.ZArchitecture.ZTextBox titleTextBox;
		private Enterprise.ZArchitecture.ZTextBox menuItemNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox menuItemAndTemplateDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox clientFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox companyFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.ZGrid configItemsGrid;
		private Enterprise.ZArchitecture.ZGrid sectionsGrid;
		private Enterprise.ZArchitecture.GUI.ZButton moveDownButton;
		private Enterprise.ZArchitecture.GUI.ZButton moveUpButton;
		private CargoWise.Windows.UI.KSplitContainer sectionsSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZButton previewButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit pageStyleDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OverrideDataContextDropEdit;
		private ZArchitecture.GUI.ZButton previewConfigItemButton;
		private ZArchitecture.GUI.ZButton previewSectionButton;
		private ZArchitecture.ZTextBox OverrideEmailSubjectTextBox;
		private ZArchitecture.ZTextBox descriptionTextBox;
	}
}
