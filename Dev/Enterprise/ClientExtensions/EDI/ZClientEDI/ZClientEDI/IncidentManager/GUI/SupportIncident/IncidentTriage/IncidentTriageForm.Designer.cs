using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentTriageForm
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

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.triageSettingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.showMenuItemsLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.setProductAreaByMenuItemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.moduleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.nodeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.nodeDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.detailsTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.supportDescriptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.supportDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.detailOptionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NodeLevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isPublishedToERequestCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isInternalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.publishedDescriptionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.publishedDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.supportNotesRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.checklistGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.checklistModuleButtonGrid = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentTriageModuleButtonGrid();
			this.tagsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tagsGrid = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentTriageDiagnosticCriteriaModuleButtonGrid();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.tableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.rightSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.upDownSplitterContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.supportNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WorkflowTabPage = new Enterprise.Client.EDI.IncidentManager.GUI.EDIWorkflowTabPage(this.components);
			this.isPublishedToAssistCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.triageSettingsGroupBox.SuspendLayout();
			this.moduleDropEdit.SuspendLayout();
			this.productAreaDropEdit.SuspendLayout();
			this.productDropEdit.SuspendLayout();
			this.nodeTypeDropEdit.SuspendLayout();
			this.nodeDetailsGroupBox.SuspendLayout();
			this.detailsTableLayout.SuspendLayout();
			this.supportDescriptionPanel.SuspendLayout();
			this.detailOptionsPanel.SuspendLayout();
			this.NodeLevelDropEdit.SuspendLayout();
			this.publishedDescriptionPanel.SuspendLayout();
			this.supportNotesRichTextBox.SuspendLayout();
			this.checklistGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.checklistModuleButtonGrid.InnerGrid)).BeginInit();
			this.checklistModuleButtonGrid.SuspendLayout();
			this.tagsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tagsGrid.InnerGrid)).BeginInit();
			this.tagsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).BeginInit();
			this.rightSplitContainer.Panel1.SuspendLayout();
			this.rightSplitContainer.Panel2.SuspendLayout();
			this.rightSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.upDownSplitterContainer)).BeginInit();
			this.upDownSplitterContainer.Panel1.SuspendLayout();
			this.upDownSplitterContainer.Panel2.SuspendLayout();
			this.upDownSplitterContainer.SuspendLayout();
			this.supportNotesGroupBox.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 729, true);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1129, 702, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.upDownSplitterContainer);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1129, 702, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1129, 702, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1129, 702, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 729, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveButtonUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(835, 0, true);
			this.SaveButtonUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 32, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(411);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(412);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage);
			// 
			// triageSettingsGroupBox
			// 
			this.triageSettingsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("d79efc4a-d7bc-46db-9556-2e806b04d8ca", "Triage Settings");
			this.triageSettingsGroupBox.Controls.Add(this.showMenuItemsLinkLabel);
			this.triageSettingsGroupBox.Controls.Add(this.setProductAreaByMenuItemCheckBox);
			this.triageSettingsGroupBox.Controls.Add(this.moduleDropEdit);
			this.triageSettingsGroupBox.Controls.Add(this.productAreaDropEdit);
			this.triageSettingsGroupBox.Controls.Add(this.productDropEdit);
			this.triageSettingsGroupBox.Controls.Add(this.nodeTypeDropEdit);
			this.triageSettingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.triageSettingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 349, true);
			this.triageSettingsGroupBox.Name = "triageSettingsGroupBox";
			this.triageSettingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 149, true);
			this.triageSettingsGroupBox.TabIndex = 1;
			this.triageSettingsGroupBox.TabStop = false;
			// 
			// showMenuItemsLinkLabel
			// 
			this.showMenuItemsLinkLabel.AutoSize = true;
			this.showMenuItemsLinkLabel.CaptionResourceString = ZClientEDI.Res.GetData("299ff30e-b2b8-4e58-a12e-1ad1002f44ae", "Show Menu Items");
			this.showMenuItemsLinkLabel.IsFontBold = false;
			this.showMenuItemsLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(234, 74, true);
			this.showMenuItemsLinkLabel.Name = "showMenuItemsLinkLabel";
			this.showMenuItemsLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 13, true);
			this.showMenuItemsLinkLabel.TabIndex = 10;
			this.showMenuItemsLinkLabel.UseMnemonic = false;
			this.showMenuItemsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ShowMenuItemsLinkLabel_LinkClicked);
			// 
			// setProductAreaByMenuItemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.setProductAreaByMenuItemCheckBox, "IMT_SetProductAreaByMenuItem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_SetProductAreaByMenuItem)));
			this.setProductAreaByMenuItemCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("c139bd28-724e-4903-beee-f66040dd6d66", "Set product area by Menu Item");
			this.setProductAreaByMenuItemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.setProductAreaByMenuItemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 73, true);
			this.setProductAreaByMenuItemCheckBox.Name = "setProductAreaByMenuItemCheckBox";
			this.setProductAreaByMenuItemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 17, true);
			this.setProductAreaByMenuItemCheckBox.TabIndex = 7;
			this.setProductAreaByMenuItemCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.setProductAreaByMenuItemCheckBox.UseVisualStyleBackColor = true;
			// 
			// moduleDropEdit
			// 
			this.moduleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.moduleDropEdit, "IMT_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Module)));
			this.moduleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 122, true);
			this.moduleDropEdit.Name = "moduleDropEdit";
			this.moduleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.moduleDropEdit.TabIndex = 9;
			// 
			// productAreaDropEdit
			// 
			this.productAreaDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productAreaDropEdit, "IMT_ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_ProductArea)));
			this.productAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 96, true);
			this.productAreaDropEdit.Name = "productAreaDropEdit";
			this.productAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.productAreaDropEdit.TabIndex = 8;
			// 
			// productDropEdit
			// 
			this.productDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productDropEdit, "IMT_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Product)));
			this.productDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 45, true);
			this.productDropEdit.Name = "productDropEdit";
			this.productDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.productDropEdit.TabIndex = 6;
			// 
			// nodeTypeDropEdit
			// 
			this.nodeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nodeTypeDropEdit, "IMT_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Type)));
			this.nodeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 19, true);
			this.nodeTypeDropEdit.Name = "nodeTypeDropEdit";
			this.nodeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.nodeTypeDropEdit.TabIndex = 5;
			// 
			// nodeDetailsGroupBox
			// 
			this.nodeDetailsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("0cec705a-d02e-41f1-9f3b-d2eb0b787cb3", "Details");
			this.nodeDetailsGroupBox.Controls.Add(this.detailsTableLayout);
			this.nodeDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nodeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.nodeDetailsGroupBox.Name = "nodeDetailsGroupBox";
			this.nodeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 340, true);
			this.nodeDetailsGroupBox.TabIndex = 0;
			this.nodeDetailsGroupBox.TabStop = false;
			// 
			// detailsTableLayout
			// 
			this.detailsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(10)));
			this.detailsTableLayout.Controls.Add(this.supportDescriptionPanel, 0, 0);
			this.detailsTableLayout.Controls.Add(this.detailOptionsPanel, 0, 2);
			this.detailsTableLayout.Controls.Add(this.publishedDescriptionPanel, 0, 1);
			this.detailsTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.detailsTableLayout.Name = "detailsTableLayout";
			this.detailsTableLayout.RowCount = 3;
			this.detailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.detailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.detailsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(113)));
			this.detailsTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 328, true);
			this.detailsTableLayout.TabIndex = 5;
			// 
			// supportDescriptionPanel
			// 
			this.supportDescriptionPanel.Controls.Add(this.supportDescriptionTextBox);
			this.supportDescriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supportDescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.supportDescriptionPanel.Name = "supportDescriptionPanel";
			this.supportDescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 103, true);
			this.supportDescriptionPanel.TabIndex = 8;
			// 
			// supportDescriptionTextBox
			// 
			this.supportDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.supportDescriptionTextBox, "IMT_SupportDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_SupportDescription)));
			this.supportDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.supportDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.supportDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 20, true);
			this.supportDescriptionTextBox.Multiline = true;
			this.supportDescriptionTextBox.Name = "supportDescriptionTextBox";
			this.supportDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.supportDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 69, true);
			this.supportDescriptionTextBox.TabIndex = 1;
			// 
			// detailOptionsPanel
			// 
			this.detailOptionsPanel.Controls.Add(this.isPublishedToAssistCheckBox);
			this.detailOptionsPanel.Controls.Add(this.NodeLevelDropEdit);
			this.detailOptionsPanel.Controls.Add(this.isPublishedToERequestCheckBox);
			this.detailOptionsPanel.Controls.Add(this.isActiveCheckBox);
			this.detailOptionsPanel.Controls.Add(this.isInternalCheckBox);
			this.detailOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 217, true);
			this.detailOptionsPanel.Name = "detailOptionsPanel";
			this.detailOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 109, true);
			this.detailOptionsPanel.TabIndex = 6;
			// 
			// NodeLevelDropEdit
			// 
			this.NodeLevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NodeLevelDropEdit, "IMT_Level");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_Level)));
			this.NodeLevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 9, true);
			this.NodeLevelDropEdit.Name = "NodeLevelDropEdit";
			this.NodeLevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(319, 17, true);
			this.NodeLevelDropEdit.TabIndex = 1;
			// 
			// isPublishedToERequestCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isPublishedToERequestCheckBox, "IMT_IsPublished");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_IsPublished)));
			this.isPublishedToERequestCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isPublishedToERequestCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 68, true);
			this.isPublishedToERequestCheckBox.Name = "isPublishedCheckBox";
			this.isPublishedToERequestCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 15, true);
			this.isPublishedToERequestCheckBox.TabIndex = 4;
			this.isPublishedToERequestCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.isPublishedToERequestCheckBox.UseVisualStyleBackColor = true;
			// 
			// isActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "IMT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_IsActive)));
			this.isActiveCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("cd236914-a79a-4dea-8ddb-762182a88e6f", "Active");
			this.isActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 31, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 15, true);
			this.isActiveCheckBox.TabIndex = 2;
			this.isActiveCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.isActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// isInternalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isInternalCheckBox, "IMT_IsInternal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_IsInternal)));
			this.isInternalCheckBox.CaptionResourceString = ZClientEDI.Res.GetData("73689ba6-73a2-40a9-8622-e725bfe30d86", "WiseTech Internal");
			this.isInternalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isInternalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 49, true);
			this.isInternalCheckBox.Name = "isInternalCheckBox";
			this.isInternalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 15, true);
			this.isInternalCheckBox.TabIndex = 3;
			this.isInternalCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.isInternalCheckBox.UseVisualStyleBackColor = true;
			// 
			// publishedDescriptionPanel
			// 
			this.publishedDescriptionPanel.Controls.Add(this.publishedDescriptionTextBox);
			this.publishedDescriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.publishedDescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 109, true);
			this.publishedDescriptionPanel.Name = "publishedDescriptionPanel";
			this.publishedDescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 103, true);
			this.publishedDescriptionPanel.TabIndex = 7;
			// 
			// publishedDescriptionTextBox
			// 
			this.publishedDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.publishedDescriptionTextBox, "PublishedDescriptionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).PublishedDescriptionText)));
			this.publishedDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.publishedDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.publishedDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(57, 18, true);
			this.publishedDescriptionTextBox.Multiline = true;
			this.publishedDescriptionTextBox.Name = "publishedDescriptionTextBox";
			this.publishedDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.publishedDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 71, true);
			this.publishedDescriptionTextBox.TabIndex = 0;
			// 
			// supportNotesRichTextBox
			// 
			this.BindingSource.SetBindingMember(this.supportNotesRichTextBox, "SupportNotesAsBlob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBlob)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).SupportNotesAsBlob)));
			this.supportNotesRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supportNotesRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.supportNotesRichTextBox.MaxLength = 10000000;
			this.supportNotesRichTextBox.Name = "supportNotesRichTextBox";
			this.supportNotesRichTextBox.ParentZForm = this;
			this.supportNotesRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1123, 174, true);
			this.supportNotesRichTextBox.TabIndex = 5;
			// 
			// checklistGroupBox
			// 
			this.checklistGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("939c78a5-3538-4f0a-b64c-cffcbc42b173", "Checklist");
			this.checklistGroupBox.Controls.Add(this.checklistModuleButtonGrid);
			this.checklistGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.checklistGroupBox.Name = "checklistGroupBox";
			this.checklistGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 275, true);
			this.checklistGroupBox.TabIndex = 0;
			this.checklistGroupBox.TabStop = false;
			// 
			// checklistModuleButtonGrid
			// 
			this.checklistModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.checklistModuleButtonGrid, "ChecklistPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).ChecklistPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).Lookups.IncidentsNotLinked)));
			this.checklistModuleButtonGrid.BindToFindBoxList = "Lookups+IncidentsNotLinked";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "IMP_Sequence";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "ChecklistItem+IMC_SupportDescription";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("e69b5834-de4e-4f72-b42d-bb597cb96606", "Publish to Portal");
			zCheckBoxColumnStyleInfo1.ColumnName = "ChecklistItem+IMC_IsPublished";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "ChecklistItem+IMC_Category";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "ChecklistItem+IMC_ResponseType";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.checklistModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.checklistModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.checklistModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.checklistModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.checklistModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.checklistModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistModuleButtonGrid.GridId = "f2006414-43ce-4a08-a153-947130de8f7d";
			// 
			// 
			// 
			this.checklistModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.checklistModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.checklistModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.checklistModuleButtonGrid.InnerGrid.GridId = "f2006414-43ce-4a08-a153-947130de8f7d";
			this.checklistModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.checklistModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.checklistModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.checklistModuleButtonGrid.InnerGrid.Name = "Grid";
			this.checklistModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 224, true);
			this.checklistModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.checklistModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.checklistModuleButtonGrid.Name = "checklistModuleButtonGrid";
			this.checklistModuleButtonGrid.ReadOnly = false;
			this.checklistModuleButtonGrid.ShowNewButton = false;
			this.checklistModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 260, true);
			this.checklistModuleButtonGrid.TabIndex = 1;
			// 
			// tagsGroupBox
			// 
			this.tagsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("fde24fe9-c27d-48b5-9045-fef1d84bcf98", "Criteria");
			this.tagsGroupBox.Controls.Add(this.tagsGrid);
			this.tagsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tagsGroupBox.Name = "tagsGroupBox";
			this.tagsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 219, true);
			this.tagsGroupBox.TabIndex = 1;
			this.tagsGroupBox.TabStop = false;
			// 
			// tagsGrid
			// 
			this.tagsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tagsGrid, "DiagnosticCriteriaPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).DiagnosticCriteriaPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).Lookups.DiagnosticCriteriaNotLinked)));
			this.tagsGrid.BindToFindBoxList = "Lookups+DiagnosticCriteriaNotLinked";
			zTextBoxColumnStyleInfo1.ColumnName = "DiagnosticCriteria+IMD_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "DiagnosticCriteria+IMD_Type";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "DiagnosticCriteria+IMD_Question";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "DiagnosticCriteria+IMD_Keywords";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.tagsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tagsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tagsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.tagsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.tagsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagsGrid.GridId = "15432b46-d024-45e9-97c5-5ed52604f0cd";
			// 
			// 
			// 
			this.tagsGrid.InnerGrid.AllowNavigation = false;
			this.tagsGrid.InnerGrid.CaptionVisible = false;
			this.tagsGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagsGrid.InnerGrid.GridId = "15432b46-d024-45e9-97c5-5ed52604f0cd";
			this.tagsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tagsGrid.InnerGrid.LayoutKey = "Grid";
			this.tagsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.tagsGrid.InnerGrid.Name = "Grid";
			this.tagsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 166, true);
			this.tagsGrid.InnerGrid.TabIndex = 0;
			this.tagsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.tagsGrid.Name = "tagsGrid";
			this.tagsGrid.ReadOnly = false;
			this.tagsGrid.ShowNewButton = false;
			this.tagsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 202, true);
			this.tagsGrid.TabIndex = 0;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.tableLayoutPanel);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1129, 501, true);
			this.mainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(450);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.rightSplitContainer);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.mainSplitContainer.SplitterWidth = 8;
			this.mainSplitContainer.TabIndex = 2;
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(10)));
			this.tableLayoutPanel.Controls.Add(this.nodeDetailsGroupBox, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.triageSettingsGroupBox, 0, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 600, true);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 2;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(155)));
			this.tableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(551, 503, true);
			this.tableLayoutPanel.TabIndex = 4;
			// 
			// rightSplitContainer
			// 
			this.rightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rightSplitContainer.Name = "rightSplitContainer";
			this.rightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// rightSplitContainer.Panel1
			// 
			this.rightSplitContainer.Panel1.Controls.Add(this.tagsGroupBox);
			this.rightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 503, true);
			this.rightSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(106);
			// 
			// rightSplitContainer.Panel2
			// 
			this.rightSplitContainer.Panel2.Controls.Add(this.checklistGroupBox);
			this.rightSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.rightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(218);
			this.rightSplitContainer.SplitterWidth = 8;
			this.rightSplitContainer.TabIndex = 3;
			// 
			// upDownSplitterContainer
			// 
			this.upDownSplitterContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.upDownSplitterContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.upDownSplitterContainer.Name = "upDownSplitterContainer";
			this.upDownSplitterContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// upDownSplitterContainer.Panel1
			// 
			this.upDownSplitterContainer.Panel1.Controls.Add(this.mainSplitContainer);
			this.upDownSplitterContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1131, 707, true);
			this.upDownSplitterContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(450);
			// 
			// upDownSplitterContainer.Panel2
			// 
			this.upDownSplitterContainer.Panel2.Controls.Add(this.supportNotesGroupBox);
			this.upDownSplitterContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(175);
			this.upDownSplitterContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(501);
			this.upDownSplitterContainer.SplitterWidth = 8;
			this.upDownSplitterContainer.TabIndex = 0;
			// 
			// supportNotesGroupBox
			// 
			this.supportNotesGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("5cd08775-72e0-4fb1-85a9-9f7d59f17f5c", "Support Notes");
			this.supportNotesGroupBox.Controls.Add(this.supportNotesRichTextBox);
			this.supportNotesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supportNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supportNotesGroupBox.Name = "supportNotesGroupBox";
			this.supportNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1131, 195, true);
			this.supportNotesGroupBox.TabIndex = 6;
			this.supportNotesGroupBox.TabStop = false;
			// 
			// isPublishedToAssistCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isPublishedToAssistCheckBox, "IMT_IsPublishedToAssist");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage)(null)).IMT_IsPublishedToAssist)));
			this.isPublishedToAssistCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isPublishedToAssistCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 86, true);
			this.isPublishedToAssistCheckBox.Name = "isPublishedToAssistCheckBox";
			this.isPublishedToAssistCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 16, true);
			this.isPublishedToAssistCheckBox.TabIndex = 5;
			this.isPublishedToAssistCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.isPublishedToAssistCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncidentTriageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1137, 785, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IncidentManager.Business.IncidentTriage";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1066, 625, true);
			this.Name = "IncidentTriageForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Triage Engine Node";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.triageSettingsGroupBox.ResumeLayout(false);
			this.triageSettingsGroupBox.PerformLayout();
			this.moduleDropEdit.ResumeLayout(true);
			this.moduleDropEdit.PerformLayout();
			this.productAreaDropEdit.ResumeLayout(true);
			this.productAreaDropEdit.PerformLayout();
			this.productDropEdit.ResumeLayout(true);
			this.productDropEdit.PerformLayout();
			this.nodeTypeDropEdit.ResumeLayout(true);
			this.nodeTypeDropEdit.PerformLayout();
			this.nodeDetailsGroupBox.ResumeLayout(false);
			this.nodeDetailsGroupBox.PerformLayout();
			this.detailsTableLayout.ResumeLayout(false);
			this.detailsTableLayout.PerformLayout();
			this.supportDescriptionPanel.ResumeLayout(false);
			this.supportDescriptionPanel.PerformLayout();
			this.detailOptionsPanel.ResumeLayout(false);
			this.detailOptionsPanel.PerformLayout();
			this.NodeLevelDropEdit.ResumeLayout(true);
			this.NodeLevelDropEdit.PerformLayout();
			this.publishedDescriptionPanel.ResumeLayout(false);
			this.publishedDescriptionPanel.PerformLayout();
			this.supportNotesRichTextBox.ResumeLayout(true);
			this.supportNotesRichTextBox.PerformLayout();
			this.checklistGroupBox.ResumeLayout(false);
			this.checklistGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.checklistModuleButtonGrid.InnerGrid)).EndInit();
			this.checklistModuleButtonGrid.ResumeLayout(true);
			this.checklistModuleButtonGrid.PerformLayout();
			this.tagsGroupBox.ResumeLayout(false);
			this.tagsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tagsGrid.InnerGrid)).EndInit();
			this.tagsGrid.ResumeLayout(true);
			this.tagsGrid.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.rightSplitContainer.Panel1.ResumeLayout(false);
			this.rightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rightSplitContainer)).EndInit();
			this.rightSplitContainer.ResumeLayout(false);
			this.rightSplitContainer.PerformLayout();
			this.upDownSplitterContainer.Panel1.ResumeLayout(false);
			this.upDownSplitterContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.upDownSplitterContainer)).EndInit();
			this.upDownSplitterContainer.ResumeLayout(false);
			this.upDownSplitterContainer.PerformLayout();
			this.supportNotesGroupBox.ResumeLayout(false);
			this.supportNotesGroupBox.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZGroupBox triageSettingsGroupBox;
		private ZGroupBox checklistGroupBox;

		#endregion
		private ZGroupBox nodeDetailsGroupBox;
		private ZDropEdit nodeTypeDropEdit;
		private ZArchitecture.ZTextBox supportDescriptionTextBox;
		private ZCheckBox isActiveCheckBox;
		private ZCheckBox isPublishedToERequestCheckBox;
		private ZCheckBox isInternalCheckBox;
		private ZDropEdit productDropEdit;
		private ZDropEdit productAreaDropEdit;
		private ZDropEdit moduleDropEdit;
		private ZArchitecture.ZTextBox publishedDescriptionTextBox;
		private IncidentTriageModuleButtonGrid checklistModuleButtonGrid;
		private ZGroupBox tagsGroupBox;
		private ZGroupBox supportNotesGroupBox;
		private KSplitContainer mainSplitContainer;
		private KSplitContainer rightSplitContainer;
		private KTableLayoutPanel tableLayoutPanel;
		private KTableLayoutPanel detailsTableLayout;
		private KSplitContainer upDownSplitterContainer;
		private ZRichTextBox supportNotesRichTextBox;
		private ZPanel detailOptionsPanel;
		private IncidentTriageDiagnosticCriteriaModuleButtonGrid tagsGrid;
		private ZPanel supportDescriptionPanel;
		private ZPanel publishedDescriptionPanel;
		private ZCheckBox setProductAreaByMenuItemCheckBox;
		private ZLinkLabel showMenuItemsLinkLabel;
		private ZDropEdit NodeLevelDropEdit;
		private EDIWorkflowTabPage WorkflowTabPage;
		private ZCheckBox isPublishedToAssistCheckBox;
	}
}
