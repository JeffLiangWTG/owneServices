namespace Enterprise.BufferManagement.GUI
{
	partial class BMBoardForm
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
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.BoardConfigGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ViewVisualBoardButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExperimentalSettingsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GlobalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OwnerFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReleaseGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SystemDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SectionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SectionConfigPreviewSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.BoardSectionConfigControl = new Enterprise.BufferManagement.GUI.BoardSectionConfigControl();
			this.PreviewTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SectionPreviewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BoardSectionPreviewControl = new Enterprise.BufferManagement.GUI.VisualBoardPreviewUserControl();
			this.BoardPreviewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BoardPreviewControl = new Enterprise.BufferManagement.GUI.VisualBoardPreviewUserControl();
			this.customisedLayoutLinksControl1 = new Enterprise.BufferManagement.GUI.CustomisedLayoutLinksControl();
			this.CustomisedLayoutsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BoardConfigGroupBox.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.OwnerFindBox.SuspendLayout();
			this.ReleaseGroupFindBox.SuspendLayout();
			this.SystemDropEdit.SuspendLayout();
			this.SectionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SectionConfigPreviewSplitContainer)).BeginInit();
			this.SectionConfigPreviewSplitContainer.Panel1.SuspendLayout();
			this.SectionConfigPreviewSplitContainer.Panel2.SuspendLayout();
			this.SectionConfigPreviewSplitContainer.SuspendLayout();
			this.BoardSectionConfigControl.SuspendLayout();
			this.PreviewTabControl.SuspendLayout();
			this.SectionPreviewTabPage.SuspendLayout();
			this.BoardPreviewTabPage.SuspendLayout();
			this.customisedLayoutLinksControl1.SuspendLayout();
			this.CustomisedLayoutsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 705, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.CustomisedLayoutsGroupBox);
			this.MainTabPage.Controls.Add(this.SectionsGroupBox);
			this.MainTabPage.Controls.Add(this.BoardConfigGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 678, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1176, 678, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 705, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoard);
			// 
			// BoardConfigGroupBox
			// 
			this.BoardConfigGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("f0f8a581-5284-4dd0-9144-27b0b88cac81", "Board Configuration");
			this.BoardConfigGroupBox.Controls.Add(this.DepartmentFindBox);
			this.BoardConfigGroupBox.Controls.Add(this.BranchFindBox);
			this.BoardConfigGroupBox.Controls.Add(this.ViewVisualBoardButton);
			this.BoardConfigGroupBox.Controls.Add(this.ExperimentalSettingsButton);
			this.BoardConfigGroupBox.Controls.Add(this.PublishedCheckBox);
			this.BoardConfigGroupBox.Controls.Add(this.GlobalCheckBox);
			this.BoardConfigGroupBox.Controls.Add(this.OwnerFindBox);
			this.BoardConfigGroupBox.Controls.Add(this.ReleaseGroupFindBox);
			this.BoardConfigGroupBox.Controls.Add(this.SystemDropEdit);
			this.BoardConfigGroupBox.Controls.Add(this.DescriptionTextBox);
			this.BoardConfigGroupBox.Controls.Add(this.NameTextBox);
			this.BoardConfigGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BoardConfigGroupBox.Name = "BoardConfigGroupBox";
			this.BoardConfigGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 155, true);
			this.BoardConfigGroupBox.TabIndex = 0;
			this.BoardConfigGroupBox.TabStop = false;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "MB_GE_AgingDepartment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_GE_AgingDepartment)));
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 123, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.DepartmentFindBox.TabIndex = 11;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "MB_GB_AgingBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_GB_AgingBranch)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 97, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.BranchFindBox.TabIndex = 10;
			// 
			// ExperimentalSettingsButton
			// 
			this.ExperimentalSettingsButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("F1029141-3B34-4B4C-8DEE-5AFA960E5CBD", "Experimental Settings");
			this.ExperimentalSettingsButton.EditableInViewMode = true;
			this.ExperimentalSettingsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 17, true);
			this.ExperimentalSettingsButton.Name = "ExperimentalSettingsButton";
			this.ExperimentalSettingsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 23, true);
			this.ExperimentalSettingsButton.TabIndex = 6;
			this.ExperimentalSettingsButton.UseVisualStyleBackColor = false;
			this.ExperimentalSettingsButton.Visible = Business.BMSRegistry.Instance.EnablePaveExperimentalFeatures.Value && Environment.Env.Security.BMSystemsEdit.IsAllowed;
			this.ExperimentalSettingsButton.Click += new System.EventHandler(this.ExperimentalSettingsButton_Click);
			// 
			// ViewVisualBoardButton
			// 
			this.ViewVisualBoardButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("c5898793-bb3d-44ff-a928-81525b3807bc", "View Board");
			this.ViewVisualBoardButton.EditableInViewMode = true;
			this.ViewVisualBoardButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 17, true);
			this.ViewVisualBoardButton.Name = "ViewVisualBoardButton";
			this.ViewVisualBoardButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.ViewVisualBoardButton.TabIndex = 7;
			this.ViewVisualBoardButton.UseVisualStyleBackColor = false;
			this.ViewVisualBoardButton.Click += new System.EventHandler(this.ViewVisualBoardButton_Click);
			// 
			// PublishedCheckBox
			// 
			this.PublishedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PublishedCheckBox, "MB_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_IsPublished)));
			this.PublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 21, true);
			this.PublishedCheckBox.Name = "PublishedCheckBox";
			this.PublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.PublishedCheckBox.TabIndex = 5;
			this.PublishedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GlobalCheckBox
			// 
			this.GlobalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GlobalCheckBox, "IsGlobal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMBoard)(null)).IsGlobal)));
			this.GlobalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GlobalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(321, 45, true);
			this.GlobalCheckBox.Name = "GlobalCheckBox";
			this.GlobalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.GlobalCheckBox.TabIndex = 8;
			this.GlobalCheckBox.UseVisualStyleBackColor = true;
			// 
			// OwnerFindBox
			// 
			this.OwnerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerFindBox, "MB_GS_NKStaffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_GS_NKStaffCode)));
			this.OwnerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 97, true);
			this.OwnerFindBox.Name = "OwnerFindBox";
			this.OwnerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.OwnerFindBox.TabIndex = 4;
			// 
			// ReleaseGroupFindBox
			// 
			this.ReleaseGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseGroupFindBox, "MB_GG_ReleaseGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_GG_ReleaseGroup)));
			this.ReleaseGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 71, true);
			this.ReleaseGroupFindBox.Name = "ReleaseGroupFindBox";
			this.ReleaseGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.ReleaseGroupFindBox.TabIndex = 9;
			// 
			// SystemDropEdit
			// 
			this.SystemDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SystemDropEdit, "MB_FS_System");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_FS_System)));
			this.SystemDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 71, true);
			this.SystemDropEdit.Name = "SystemDropEdit";
			this.SystemDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.SystemDropEdit.TabIndex = 3;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "MB_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 45, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "MB_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMBoard)(null)).MB_Name)));
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 19, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.NameTextBox.TabIndex = 1;
			// 
			// SectionsGroupBox
			// 
			this.SectionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SectionsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("851cddc2-5449-45d3-afd9-6882edd78688", "Sections");
			this.SectionsGroupBox.Controls.Add(this.SectionConfigPreviewSplitContainer);
			this.SectionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 164, true);
			this.SectionsGroupBox.Name = "SectionsGroupBox";
			this.SectionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1170, 511, true);
			this.SectionsGroupBox.TabIndex = 1;
			this.SectionsGroupBox.TabStop = false;
			// 
			// SectionConfigPreviewSplitContainer
			// 
			this.SectionConfigPreviewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SectionConfigPreviewSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SectionConfigPreviewSplitContainer.Name = "SectionConfigPreviewSplitContainer";
			// 
			// SectionConfigPreviewSplitContainer.Panel1
			// 
			this.SectionConfigPreviewSplitContainer.Panel1.Controls.Add(this.BoardSectionConfigControl);
			// 
			// SectionConfigPreviewSplitContainer.Panel2
			// 
			this.SectionConfigPreviewSplitContainer.Panel2.Controls.Add(this.PreviewTabControl);
			this.SectionConfigPreviewSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 518, true);
			this.SectionConfigPreviewSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(765);
			this.SectionConfigPreviewSplitContainer.TabIndex = 2;
			// 
			// BoardSectionConfigControl
			// 
			this.BoardSectionConfigControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BoardSectionConfigControl, ".");
			this.BoardSectionConfigControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BoardSectionConfigControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BoardSectionConfigControl.Name = "BoardSectionConfigControl";
			this.BoardSectionConfigControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 518, true);
			this.BoardSectionConfigControl.TabIndex = 1;
			// 
			// PreviewTabControl
			// 
			this.PreviewTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PreviewTabControl.Controls.Add(this.SectionPreviewTabPage);
			this.PreviewTabControl.Controls.Add(this.BoardPreviewTabPage);
			this.PreviewTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviewTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviewTabControl.Name = "PreviewTabControl";
			this.PreviewTabControl.SelectedIndex = 0;
			this.PreviewTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 518, true);
			this.PreviewTabControl.TabIndex = 0;
			this.PreviewTabControl.SelectedIndexChanged += new System.EventHandler(this.PreviewTabControl_SelectedIndexChanged);
			// 
			// SectionPreviewTabPage
			// 
			this.SectionPreviewTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("175bef10-ab06-4388-a444-1d211132e366", "Section Preview");
			this.SectionPreviewTabPage.Controls.Add(this.BoardSectionPreviewControl);
			this.SectionPreviewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SectionPreviewTabPage.Name = "SectionPreviewTabPage";
			this.SectionPreviewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SectionPreviewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 491, true);
			this.SectionPreviewTabPage.TabIndex = 0;
			this.SectionPreviewTabPage.UseVisualStyleBackColor = true;
			// 
			// BoardSectionPreviewControl
			// 
			this.BoardSectionPreviewControl.AllowDrop = true;
			this.BoardSectionPreviewControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BoardSectionPreviewControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BoardSectionPreviewControl.Name = "BoardSectionPreviewControl";
			this.BoardSectionPreviewControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 485, true);
			this.BoardSectionPreviewControl.TabIndex = 0;
			// 
			// BoardPreviewTabPage
			// 
			this.BoardPreviewTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d498f980-fcac-4aae-96d3-c4f21bda0aa1", "Board Preview");
			this.BoardPreviewTabPage.Controls.Add(this.BoardPreviewControl);
			this.BoardPreviewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BoardPreviewTabPage.Name = "BoardPreviewTabPage";
			this.BoardPreviewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BoardPreviewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 491, true);
			this.BoardPreviewTabPage.TabIndex = 1;
			this.BoardPreviewTabPage.UseVisualStyleBackColor = true;
			// 
			// BoardPreviewControl
			// 
			this.BoardPreviewControl.AllowDrop = true;
			this.BoardPreviewControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BoardPreviewControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BoardPreviewControl.Name = "BoardPreviewControl";
			this.BoardPreviewControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 485, true);
			this.BoardPreviewControl.TabIndex = 0;
			// 
			// customisedLayoutLinksControl1
			// 
			this.customisedLayoutLinksControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customisedLayoutLinksControl1, "CustomisedLayoutLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Business.BMControlCustomisationLinkCollection)(((Enterprise.BufferManagement.Business.BMBoard)(null)).CustomisedLayoutLinks)));
			this.customisedLayoutLinksControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customisedLayoutLinksControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.customisedLayoutLinksControl1.Name = "customisedLayoutLinksControl1";
			this.customisedLayoutLinksControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 108, true);
			this.customisedLayoutLinksControl1.TabIndex = 13;
			// 
			// CustomisedLayoutsGroupBox
			// 
			this.CustomisedLayoutsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CustomisedLayoutsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ec337cb8-d7c4-4d37-9a51-59271b4275a3", "Customized Visual Layouts");
			this.CustomisedLayoutsGroupBox.Controls.Add(this.customisedLayoutLinksControl1);
			this.CustomisedLayoutsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(670, 5, true);
			this.CustomisedLayoutsGroupBox.Name = "CustomisedLayoutsGroupBox";
			this.CustomisedLayoutsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(503, 153, true);
			this.CustomisedLayoutsGroupBox.TabIndex = 14;
			this.CustomisedLayoutsGroupBox.TabStop = false;
			// 
			// BMBoardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("51f80adb-7cf8-4ccf-a5e9-6f15306a3d93", "Visual Board");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 761, true);
			this.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMBoard);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 720, true);
			this.Name = "BMBoardForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BoardConfigGroupBox.ResumeLayout(false);
			this.BoardConfigGroupBox.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.OwnerFindBox.ResumeLayout(true);
			this.OwnerFindBox.PerformLayout();
			this.ReleaseGroupFindBox.ResumeLayout(true);
			this.ReleaseGroupFindBox.PerformLayout();
			this.SystemDropEdit.ResumeLayout(true);
			this.SystemDropEdit.PerformLayout();
			this.SectionsGroupBox.ResumeLayout(false);
			this.SectionsGroupBox.PerformLayout();
			this.SectionConfigPreviewSplitContainer.Panel1.ResumeLayout(false);
			this.SectionConfigPreviewSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SectionConfigPreviewSplitContainer)).EndInit();
			this.SectionConfigPreviewSplitContainer.ResumeLayout(false);
			this.SectionConfigPreviewSplitContainer.PerformLayout();
			this.BoardSectionConfigControl.ResumeLayout(true);
			this.BoardSectionConfigControl.PerformLayout();
			this.PreviewTabControl.ResumeLayout(false);
			this.PreviewTabControl.PerformLayout();
			this.SectionPreviewTabPage.ResumeLayout(false);
			this.SectionPreviewTabPage.PerformLayout();
			this.BoardPreviewTabPage.ResumeLayout(false);
			this.BoardPreviewTabPage.PerformLayout();
			this.customisedLayoutLinksControl1.ResumeLayout(true);
			this.customisedLayoutLinksControl1.PerformLayout();
			this.CustomisedLayoutsGroupBox.ResumeLayout(false);
			this.CustomisedLayoutsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox BoardConfigGroupBox;
		private ZArchitecture.ZTextBox NameTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZGuidDropEdit SystemDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox ReleaseGroupFindBox;
		private ZArchitecture.GUI.ZCodeFindBox OwnerFindBox;
		private ZArchitecture.GUI.ZCheckBox PublishedCheckBox;
		private ZArchitecture.GUI.ZCheckBox GlobalCheckBox;
		private ZArchitecture.GUI.ZGroupBox SectionsGroupBox;
		private ZArchitecture.GUI.ZTabControl PreviewTabControl;
		private ZArchitecture.GUI.ZTabPage SectionPreviewTabPage;
		private ZArchitecture.GUI.ZTabPage BoardPreviewTabPage;
		private CargoWise.Windows.UI.KSplitContainer SectionConfigPreviewSplitContainer;
		private BoardSectionConfigControl BoardSectionConfigControl;
		private VisualBoardPreviewUserControl BoardSectionPreviewControl;
		private VisualBoardPreviewUserControl BoardPreviewControl;
		private ZArchitecture.GUI.ZButton ViewVisualBoardButton;
		private ZArchitecture.GUI.ZButton ExperimentalSettingsButton;
		private ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
		private ZArchitecture.GUI.ZGuidFindBox DepartmentFindBox;
		private ZArchitecture.GUI.ZGroupBox CustomisedLayoutsGroupBox;
		private CustomisedLayoutLinksControl customisedLayoutLinksControl1;
	}
}
