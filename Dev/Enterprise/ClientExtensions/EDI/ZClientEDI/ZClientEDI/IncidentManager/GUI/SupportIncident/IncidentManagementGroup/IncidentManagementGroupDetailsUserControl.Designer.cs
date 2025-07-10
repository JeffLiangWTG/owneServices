
using System;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentManagementGroupDetailsUserControl
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
			if (outageDurationRefreshTimer != null)
			{
				outageDurationRefreshTimer.Tick -= new EventHandler(OutageDurationRefreshTimer_Tick);
				outageDurationRefreshTimer.Stop();
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IncidentManagementGroupDetailsUserControl));
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.incidentManagementGroupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupOwnerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.outageDurationTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.outageDurationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.urgencyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.businessImpactDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.serviceOutageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.groupDescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupDescriptionTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.initialSymptomsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.rootCauseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.businessImpactDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.milestonesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.toolStripMilestoneButtons = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ServiceRestoredButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.ServiceOutageStartMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ServiceOutageDowngradedMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ServiceRestoredMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.milestonesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.statusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.stageAndFieldsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.StagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.currentGroupStatusTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.taskStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.currentGroupStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.updateStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NextButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PreviousButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.currentTaskTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.taskStatusTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.currentTaskLabel = new Enterprise.ZArchitecture.ZLabel();
			this.taskAssignedDescriptionTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.taskAssignedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.taskAssignedCodeTextLabel = new Enterprise.ZArchitecture.ZLabel();
			this.productDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.detailFieldsRowLayoutPanel = new CargoWise.Windows.UI.Layout.RowLayoutPanel();
			this.menuSectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.cr8ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.cr9ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.serviceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.countryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.criticalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.productAreaDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.menuItemCaption = new Enterprise.ZArchitecture.ZLabel();
			this.OverrideSourceModuleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TriageAssistButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.serviceTypeCaption = new Enterprise.ZArchitecture.ZLabel();
			this.sourceModuleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.activityFeedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.kTabControl1 = new CargoWise.Windows.UI.KTabControl();
			this.eConversationTabPage = new CargoWise.Windows.UI.KTabPage();
			this.eConversationMessageBoxsplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.kTableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.sendMessageButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.conversationMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eConversationMessageListUserControl1 = new Enterprise.EConversation.GUI.EConversationMessageListUserControl();
			this.incidentManagementGroupCustomFieldsControl = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentManagementGroupCustomFieldsControl();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.mainLeftSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.leftTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.groupAndDescriptionSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.centerSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.stageAndDetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TriageAssistLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TriageAssistCaption = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.incidentManagementGroupGroupBox.SuspendLayout();
			this.groupOwnerCodeFindBox.SuspendLayout();
			this.urgencyDropEdit.SuspendLayout();
			this.businessImpactDropEdit.SuspendLayout();
			this.serviceOutageDropEdit.SuspendLayout();
			this.typeDropEdit.SuspendLayout();
			this.groupDescriptionGroupBox.SuspendLayout();
			this.groupDescriptionTableLayout.SuspendLayout();
			this.milestonesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.milestonesGrid)).BeginInit();
			this.milestonesGrid.SuspendLayout();
			this.statusGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.stageAndFieldsSplitContainer)).BeginInit();
			this.stageAndFieldsSplitContainer.Panel1.SuspendLayout();
			this.stageAndFieldsSplitContainer.Panel2.SuspendLayout();
			this.stageAndFieldsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.StagesGrid)).BeginInit();
			this.StagesGrid.SuspendLayout();
			this.productDetailsGroupBox.SuspendLayout();
			this.detailFieldsRowLayoutPanel.SuspendLayout();
			this.menuSectionDropEdit.SuspendLayout();
			this.cr8ModuleDropEdit.SuspendLayout();
			this.productDropEdit.SuspendLayout();
			this.cr9ModuleDropEdit.SuspendLayout();
			this.serviceTypeDropEdit.SuspendLayout();
			this.countryDropEdit.SuspendLayout();
			this.criticalityDropEdit.SuspendLayout();
			this.productAreaDropEdit.SuspendLayout();
			this.activityFeedGroupBox.SuspendLayout();
			this.kTabControl1.SuspendLayout();
			this.eConversationTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.eConversationMessageBoxsplitContainer)).BeginInit();
			this.eConversationMessageBoxsplitContainer.Panel1.SuspendLayout();
			this.eConversationMessageBoxsplitContainer.Panel2.SuspendLayout();
			this.eConversationMessageBoxsplitContainer.SuspendLayout();
			this.kTableLayoutPanel1.SuspendLayout();
			this.eConversationMessageListUserControl1.SuspendLayout();
			this.incidentManagementGroupCustomFieldsControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainLeftSplitContainer)).BeginInit();
			this.mainLeftSplitContainer.Panel1.SuspendLayout();
			this.mainLeftSplitContainer.Panel2.SuspendLayout();
			this.mainLeftSplitContainer.SuspendLayout();
			this.leftTableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.groupAndDescriptionSplitContainer)).BeginInit();
			this.groupAndDescriptionSplitContainer.Panel1.SuspendLayout();
			this.groupAndDescriptionSplitContainer.Panel2.SuspendLayout();
			this.groupAndDescriptionSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.centerSplitContainer)).BeginInit();
			this.centerSplitContainer.Panel1.SuspendLayout();
			this.centerSplitContainer.Panel2.SuspendLayout();
			this.centerSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.stageAndDetailsSplitContainer)).BeginInit();
			this.stageAndDetailsSplitContainer.Panel1.SuspendLayout();
			this.stageAndDetailsSplitContainer.Panel2.SuspendLayout();
			this.stageAndDetailsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup);
			// 
			// incidentManagementGroupGroupBox
			// 
			this.incidentManagementGroupGroupBox.Controls.Add(this.groupOwnerCodeFindBox);
			this.incidentManagementGroupGroupBox.Controls.Add(this.outageDurationTimeLabel);
			this.incidentManagementGroupGroupBox.Controls.Add(this.outageDurationLabel);
			this.incidentManagementGroupGroupBox.Controls.Add(this.urgencyDropEdit);
			this.incidentManagementGroupGroupBox.Controls.Add(this.businessImpactDropEdit);
			this.incidentManagementGroupGroupBox.Controls.Add(this.serviceOutageDropEdit);
			this.incidentManagementGroupGroupBox.Controls.Add(this.typeDropEdit);
			this.incidentManagementGroupGroupBox.Controls.Add(this.descriptionTextBox);
			this.incidentManagementGroupGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentManagementGroupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.incidentManagementGroupGroupBox.Name = "incidentManagementGroupGroupBox";
			this.incidentManagementGroupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 174, true);
			this.incidentManagementGroupGroupBox.TabIndex = 1;
			this.incidentManagementGroupGroupBox.TabStop = false;
			this.incidentManagementGroupGroupBox.Text = "Incident Management Group";
			// 
			// groupOwnerCodeFindBox
			// 
			this.groupOwnerCodeFindBox.AllowDrop = true;
			this.groupOwnerCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.groupOwnerCodeFindBox, "ING_GS_NKGroupOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_GS_NKGroupOwner)));
			this.groupOwnerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 147, true);
			this.groupOwnerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.groupOwnerCodeFindBox.Name = "groupOwnerCodeFindBox";
			this.groupOwnerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.groupOwnerCodeFindBox.ParentType = null;
			this.groupOwnerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.groupOwnerCodeFindBox.TabIndex = 7;
			// 
			// outageDurationTimeLabel
			// 
			this.outageDurationTimeLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.outageDurationTimeLabel, "OutageDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).OutageDuration)));
			this.outageDurationTimeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.outageDurationTimeLabel.IsFontBold = true;
			this.outageDurationTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 84, true);
			this.outageDurationTimeLabel.Name = "outageDurationTimeLabel";
			this.outageDurationTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.outageDurationTimeLabel.TabIndex = 10;
			this.outageDurationTimeLabel.UseMnemonic = false;
			// 
			// outageDurationLabel
			// 
			this.outageDurationLabel.AutoSize = true;
			this.outageDurationLabel.CaptionResourceString = ZClientEDI.Res.GetData("87223137-189b-48a4-80b0-8d69ac1103b5", "Outage Duration");
			this.outageDurationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.outageDurationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 85, true);
			this.outageDurationLabel.Name = "outageDurationLabel";
			this.outageDurationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.outageDurationLabel.TabIndex = 12;
			this.outageDurationLabel.UseMnemonic = false;
			// 
			// urgencyDropEdit
			// 
			this.urgencyDropEdit.AllowDrop = true;
			this.urgencyDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.urgencyDropEdit, "ING_Urgency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Urgency)));
			this.urgencyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 126, true);
			this.urgencyDropEdit.Name = "urgencyDropEdit";
			this.urgencyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.urgencyDropEdit.TabIndex = 6;
			// 
			// businessImpactDropEdit
			// 
			this.businessImpactDropEdit.AllowDrop = true;
			this.businessImpactDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.businessImpactDropEdit, "ING_BusinessImpact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_BusinessImpact)));
			this.businessImpactDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 105, true);
			this.businessImpactDropEdit.Name = "businessImpactDropEdit";
			this.businessImpactDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.businessImpactDropEdit.TabIndex = 5;
			// 
			// serviceOutageDropEdit
			// 
			this.serviceOutageDropEdit.AllowDrop = true;
			this.serviceOutageDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.serviceOutageDropEdit, "ING_ServiceOutage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ServiceOutage)));
			this.serviceOutageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 63, true);
			this.serviceOutageDropEdit.Name = "serviceOutageDropEdit";
			this.serviceOutageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.serviceOutageDropEdit.TabIndex = 3;
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.typeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.typeDropEdit, "ING_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Type)));
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 21, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.typeDropEdit.TabIndex = 1;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.AllowDrop = true;
			this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "ING_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Description)));
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 42, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 15, true);
			this.descriptionTextBox.TabIndex = 2;
			// 
			// groupDescriptionGroupBox
			// 
			this.groupDescriptionGroupBox.Controls.Add(this.groupDescriptionTableLayout);
			this.groupDescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupDescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupDescriptionGroupBox.Name = "groupDescriptionGroupBox";
			this.groupDescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 267, true);
			this.groupDescriptionGroupBox.TabIndex = 6;
			this.groupDescriptionGroupBox.TabStop = false;
			this.groupDescriptionGroupBox.Text = "Group Description";
			// 
			// groupDescriptionTableLayout
			// 
			this.groupDescriptionTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.groupDescriptionTableLayout.Controls.Add(this.initialSymptomsTextBox, 0, 1);
			this.groupDescriptionTableLayout.Controls.Add(this.rootCauseTextBox, 0, 5);
			this.groupDescriptionTableLayout.Controls.Add(this.businessImpactDescriptionTextBox, 0, 3);
			this.groupDescriptionTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupDescriptionTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.groupDescriptionTableLayout.Name = "groupDescriptionTableLayout";
			this.groupDescriptionTableLayout.RowCount = 6;
			this.groupDescriptionTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(15)));
			this.groupDescriptionTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.groupDescriptionTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(15)));
			this.groupDescriptionTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33F));
			this.groupDescriptionTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(15)));
			this.groupDescriptionTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34F));
			this.groupDescriptionTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 252, true);
			this.groupDescriptionTableLayout.TabIndex = 15;
			// 
			// initialSymptomsTextBox
			// 
			this.initialSymptomsTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.initialSymptomsTextBox, "InitialSymptomsText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).InitialSymptomsText)));
			this.initialSymptomsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.initialSymptomsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.initialSymptomsTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.initialSymptomsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.initialSymptomsTextBox.Multiline = true;
			this.initialSymptomsTextBox.Name = "initialSymptomsTextBox";
			this.initialSymptomsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.initialSymptomsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 65, true);
			this.initialSymptomsTextBox.TabIndex = 2;
			// 
			// rootCauseTextBox
			// 
			this.rootCauseTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.rootCauseTextBox, "RootCauseText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).RootCauseText)));
			this.rootCauseTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.rootCauseTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.rootCauseTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.rootCauseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 182, true);
			this.rootCauseTextBox.Multiline = true;
			this.rootCauseTextBox.Name = "rootCauseTextBox";
			this.rootCauseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.rootCauseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 68, true);
			this.rootCauseTextBox.TabIndex = 4;
			// 
			// businessImpactDescriptionTextBox
			// 
			this.businessImpactDescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.businessImpactDescriptionTextBox, "BusinessImpactDescriptionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).BusinessImpactDescriptionText)));
			this.businessImpactDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.businessImpactDescriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.businessImpactDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.businessImpactDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 99, true);
			this.businessImpactDescriptionTextBox.Multiline = true;
			this.businessImpactDescriptionTextBox.Name = "businessImpactDescriptionTextBox";
			this.businessImpactDescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.businessImpactDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 65, true);
			this.businessImpactDescriptionTextBox.TabIndex = 3;
			// 
			// milestonesGroupBox
			// 
			this.milestonesGroupBox.Controls.Add(this.toolStripMilestoneButtons);
			this.milestonesGroupBox.Controls.Add(this.milestonesGrid);
			this.milestonesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.milestonesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.milestonesGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 5, true);
			this.milestonesGroupBox.Name = "milestonesGroupBox";
			this.milestonesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 190, true);
			this.milestonesGroupBox.TabIndex = 7;
			this.milestonesGroupBox.TabStop = false;
			this.milestonesGroupBox.Text = "Milestones";
			// 
			// toolStripMilestoneButtons
			// 
			this.toolStripMilestoneButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStripMilestoneButtons.BackColor = System.Drawing.Color.Transparent;
			this.toolStripMilestoneButtons.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStripMilestoneButtons.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.toolStripMilestoneButtons.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ServiceRestoredButton});
			this.toolStripMilestoneButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 162, true);
			this.toolStripMilestoneButtons.Name = "toolStripMilestoneButtons";
			this.toolStripMilestoneButtons.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.toolStripMilestoneButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 21, true);
			this.toolStripMilestoneButtons.TabIndex = 0;
			// 
			// ServiceRestoredButton
			// 
			this.ServiceRestoredButton.CaptionResourceString = ZClientEDI.Res.GetData("cb9c2250-0e16-426f-af86-a1a84c2d43e1", "Update Service Event");
			this.ServiceRestoredButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ServiceOutageStartMenuItem,
			this.ServiceOutageDowngradedMenuItem,
			this.ServiceRestoredMenuItem});
			this.ServiceRestoredButton.Name = "ServiceRestoredButton";
			this.ServiceRestoredButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 18, true);
			// 
			// ServiceOutageStartMenuItem
			// 
			this.ServiceOutageStartMenuItem.CaptionResourceString = ZClientEDI.Res.GetData("A7B11427-F756-41EE-8D4A-67FFBB2B616C", "Service Outage Start");
			this.ServiceOutageStartMenuItem.Name = "ServiceOutageStartMenuItem";
			this.ServiceOutageStartMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 22, true);
			this.ServiceOutageStartMenuItem.Click += new System.EventHandler(this.ServiceOutageStartMenuItem_Click);
			// 
			// ServiceOutageDowngradedMenuItem
			// 
			this.ServiceOutageDowngradedMenuItem.CaptionResourceString = ZClientEDI.Res.GetData("946AD45B-DAC9-4CDC-9276-16F158F4174E", "Service Outage Downgraded");
			this.ServiceOutageDowngradedMenuItem.Name = "ServiceOutageDowngradedMenuItem";
			this.ServiceOutageDowngradedMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 22, true);
			this.ServiceOutageDowngradedMenuItem.Click += new System.EventHandler(this.ServiceOutageDowngradedMenuItem_Click);
			// 
			// ServiceRestoredMenuItem
			// 
			this.ServiceRestoredMenuItem.CaptionResourceString = ZClientEDI.Res.GetData("038384B5-B57A-4559-88CE-74D779DD84DC", "Service Restored");
			this.ServiceRestoredMenuItem.Name = "ServiceRestoredMenuItem";
			this.ServiceRestoredMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 22, true);
			this.ServiceRestoredMenuItem.Click += new System.EventHandler(this.ServiceRestoredMenuItem_Click);
			// 
			// milestonesGrid
			// 
			this.milestonesGrid.AllowNavigation = false;
			this.milestonesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.milestonesGrid, "Milestones");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)).SyncRoot)).P9_Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)).SyncRoot)).TriggerConditions.TriggerEventCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)).SyncRoot)).P9_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)).SyncRoot)).P9_ScheduledDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)).SyncRoot)).P9_ActualDateForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTask)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Milestones)).SyncRoot)).P9_IsPublished)));
			this.milestonesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "P9_Sequence";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("c65a1807-09aa-4f07-b80f-b8ca5885f824", "Seq.", "Sequence", "");
			zDropEditColumnStyleInfo1.ColumnName = "TriggerConditions+TriggerEventCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
			zTextBoxColumnStyleInfo1.ColumnName = "P9_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "P9_ScheduledDateForBinding";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "P9_ActualDateForBinding";
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "P9_IsPublished";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.milestonesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.milestonesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.milestonesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.milestonesGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.milestonesGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.milestonesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.milestonesGrid.GridId = "d0ebb744-943d-4bea-a567-7584bcefc50a";
			this.milestonesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.milestonesGrid.LayoutKey = "milestonesGrid";
			this.milestonesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.milestonesGrid.Name = "milestonesGrid";
			this.milestonesGrid.ReadOnly = true;
			this.milestonesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 130, true);
			this.milestonesGrid.TabIndex = 0;
			// 
			// statusGroupBox
			// 
			this.statusGroupBox.Controls.Add(this.stageAndFieldsSplitContainer);
			this.statusGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.statusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.statusGroupBox.Name = "statusGroupBox";
			this.statusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 296, true);
			this.statusGroupBox.TabIndex = 8;
			this.statusGroupBox.TabStop = false;
			this.statusGroupBox.Text = "Group Stage";
			// 
			// stageAndFieldsSplitContainer
			// 
			this.stageAndFieldsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stageAndFieldsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.stageAndFieldsSplitContainer.Name = "stageAndFieldsSplitContainer";
			this.stageAndFieldsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// stageAndFieldsSplitContainer.Panel1
			// 
			this.stageAndFieldsSplitContainer.Panel1.Controls.Add(this.StagesGrid);
			// 
			// stageAndFieldsSplitContainer.Panel2
			// 
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.currentGroupStatusTextLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.taskStatusLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.currentGroupStatusLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.updateStatusLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.NextButton);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.PreviousButton);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.currentTaskTextLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.taskStatusTextLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.currentTaskLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.taskAssignedDescriptionTextLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.taskAssignedLabel);
			this.stageAndFieldsSplitContainer.Panel2.Controls.Add(this.taskAssignedCodeTextLabel);
			this.stageAndFieldsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 281, true);
			this.stageAndFieldsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60);
			this.stageAndFieldsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(210);
			this.stageAndFieldsSplitContainer.SplitterWidth = 8;
			this.stageAndFieldsSplitContainer.TabIndex = 15;
			// 
			// StagesGrid
			// 
			this.StagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.StagesGrid, "Stages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).DescriptionOnGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).ControlIncidents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).IncidentCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).GroupCompleted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).TriggerOn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).CascadeCriticality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Registry.Business.IncidentGroupStatusConfiguration)(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Stages)).SyncRoot)).CascadeProductDetails)));
			this.StagesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "Sequence";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "DescriptionOnGroup";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "ControlIncidents";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "IncidentCompleted";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.ColumnName = "GroupCompleted";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "TriggerOn";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.ColumnName = "CascadeCriticality";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.ColumnName = "CascadeProductDetails";
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.StagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.StagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.StagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.StagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.StagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.StagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.StagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.StagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.StagesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.StagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StagesGrid.GridId = "b90c2d7e-1059-4455-8895-ac11ff911607";
			this.StagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.StagesGrid.IsWholeRowSelectedOnClick = true;
			this.StagesGrid.LayoutKey = "StagesGrid";
			this.StagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StagesGrid.Name = "StagesGrid";
			this.StagesGrid.ReadOnly = true;
			this.StagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 210, true);
			this.StagesGrid.TabIndex = 0;
			// 
			// currentGroupStatusTextLabel
			// 
			this.currentGroupStatusTextLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.currentGroupStatusTextLabel, "StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).StatusDescription)));
			this.currentGroupStatusTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.currentGroupStatusTextLabel.IsFontBold = true;
			this.currentGroupStatusTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 40, true);
			this.currentGroupStatusTextLabel.Name = "currentGroupStatusTextLabel";
			this.currentGroupStatusTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.currentGroupStatusTextLabel.TabIndex = 10;
			// 
			// taskStatusLabel
			// 
			this.taskStatusLabel.CaptionResourceString = ZClientEDI.Res.GetData("688c8a80-433b-4a6b-b1e5-bc5846d59212", "Task Status:");
			this.taskStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.taskStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.taskStatusLabel.Name = "taskStatusLabel";
			this.taskStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.taskStatusLabel.TabIndex = 2;
			this.taskStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// currentGroupStatusLabel
			// 
			this.currentGroupStatusLabel.AutoSize = true;
			this.currentGroupStatusLabel.CaptionResourceString = ZClientEDI.Res.GetData("d7883361-0a02-4f8d-853b-a0a3d37b3eab", "Current Group Stage:");
			this.currentGroupStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.currentGroupStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 40, true);
			this.currentGroupStatusLabel.Name = "currentGroupStatusLabel";
			this.currentGroupStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 13, true);
			this.currentGroupStatusLabel.TabIndex = 9;
			this.currentGroupStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// updateStatusLabel
			// 
			this.updateStatusLabel.AutoSize = true;
			this.updateStatusLabel.CaptionResourceString = ZClientEDI.Res.GetData("2be8d888-98d0-4959-9d5a-a0c049ddcad7", "Change Stage");
			this.updateStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.updateStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 5, true);
			this.updateStatusLabel.Name = "updateStatusLabel";
			this.updateStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.updateStatusLabel.TabIndex = 3;
			this.updateStatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// NextButton
			// 
			this.NextButton.CaptionResourceString = ZClientEDI.Res.GetData("ae999c73-ea6f-4a59-be2c-1c649f541985", "Next");
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 0, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 23, true);
			this.NextButton.TabIndex = 5;
			this.NextButton.ToolTipCaption = null;
			this.NextButton.UseVisualStyleBackColor = true;
			this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
			// 
			// PreviousButton
			// 
			this.PreviousButton.CaptionResourceString = ZClientEDI.Res.GetData("9d0f3322-c5be-4df4-a359-af28bcc0428b", "Previous");
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 0, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.PreviousButton.TabIndex = 4;
			this.PreviousButton.ToolTipCaption = null;
			this.PreviousButton.UseVisualStyleBackColor = true;
			this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
			// 
			// currentTaskTextLabel
			// 
			this.currentTaskTextLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.currentTaskTextLabel, "CurrentTaskDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).CurrentTaskDescription)));
			this.currentTaskTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.currentTaskTextLabel.IsFontBold = true;
			this.currentTaskTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 41, true);
			this.currentTaskTextLabel.Name = "currentTaskTextLabel";
			this.currentTaskTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.currentTaskTextLabel.TabIndex = 12;
			// 
			// taskStatusTextLabel
			// 
			this.taskStatusTextLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.taskStatusTextLabel, "CurrentTaskStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).CurrentTaskStatus)));
			this.taskStatusTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.taskStatusTextLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.taskStatusTextLabel.IsFontBold = true;
			this.taskStatusTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 5, true);
			this.taskStatusTextLabel.Name = "taskStatusTextLabel";
			this.taskStatusTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.taskStatusTextLabel.TabIndex = 2;
			// 
			// currentTaskLabel
			// 
			this.currentTaskLabel.CaptionResourceString = ZClientEDI.Res.GetData("88573df6-c6ea-417b-95e1-1b88750d0b36", "Current Task:");
			this.currentTaskLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.currentTaskLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 41, true);
			this.currentTaskLabel.Name = "currentTaskLabel";
			this.currentTaskLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.currentTaskLabel.TabIndex = 9;
			this.currentTaskLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// taskAssignedDescriptionTextLabel
			// 
			this.BindingSource.SetBindingMember(this.taskAssignedDescriptionTextLabel, "OverallAssignedToDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).OverallAssignedToDescription)));
			this.taskAssignedDescriptionTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.taskAssignedDescriptionTextLabel.IsFontBold = true;
			this.taskAssignedDescriptionTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 23, true);
			this.taskAssignedDescriptionTextLabel.Name = "taskAssignedDescriptionTextLabel";
			this.taskAssignedDescriptionTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.taskAssignedDescriptionTextLabel.TabIndex = 8;
			// 
			// taskAssignedLabel
			// 
			this.BindingSource.SetBindingMember(this.taskAssignedLabel, "OverallAssignedToLabelText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).OverallAssignedToLabelText)));
			this.taskAssignedLabel.CaptionResourceString = ZClientEDI.Res.GetData("33fc0058-b980-428b-b423-04c7245c15ad", "Task Assigned:");
			this.taskAssignedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.taskAssignedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 23, true);
			this.taskAssignedLabel.Name = "taskAssignedLabel";
			this.taskAssignedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.taskAssignedLabel.TabIndex = 5;
			this.taskAssignedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.taskAssignedLabel.UseMnemonic = false;
			// 
			// taskAssignedCodeTextLabel
			// 
			this.BindingSource.SetBindingMember(this.taskAssignedCodeTextLabel, "OverallAssignedToCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).OverallAssignedToCode)));
			this.taskAssignedCodeTextLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.taskAssignedCodeTextLabel.IsFontBold = true;
			this.taskAssignedCodeTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 23, true);
			this.taskAssignedCodeTextLabel.Name = "taskAssignedCodeTextLabel";
			this.taskAssignedCodeTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 13, true);
			this.taskAssignedCodeTextLabel.TabIndex = 7;
			// 
			// productDetailsGroupBox
			// 
			this.productDetailsGroupBox.Controls.Add(this.TriageAssistCaption);
			this.productDetailsGroupBox.Controls.Add(this.TriageAssistLabel);
			this.productDetailsGroupBox.Controls.Add(this.detailFieldsRowLayoutPanel);
			this.productDetailsGroupBox.Controls.Add(this.menuItemCaption);
			this.productDetailsGroupBox.Controls.Add(this.OverrideSourceModuleButton);
			this.productDetailsGroupBox.Controls.Add(this.TriageAssistButton);
			this.productDetailsGroupBox.Controls.Add(this.serviceTypeCaption);
			this.productDetailsGroupBox.Controls.Add(this.sourceModuleLabel);
			this.productDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.productDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.productDetailsGroupBox.Name = "productDetailsGroupBox";
			this.productDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 147, true);
			this.productDetailsGroupBox.TabIndex = 9;
			this.productDetailsGroupBox.TabStop = false;
			this.productDetailsGroupBox.Text = "Product Details";
			// 
			// detailFieldsRowLayoutPanel
			// 
			this.detailFieldsRowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.detailFieldsRowLayoutPanel.Controls.Add(this.menuSectionDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.cr8ModuleDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.productDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.cr9ModuleDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.serviceTypeDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.countryDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.criticalityDropEdit);
			this.detailFieldsRowLayoutPanel.Controls.Add(this.productAreaDropEdit);
			this.detailFieldsRowLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 20, true);
			this.detailFieldsRowLayoutPanel.Name = "detailFieldsRowLayoutPanel";
			this.detailFieldsRowLayoutPanel.RowHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(23);
			this.detailFieldsRowLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 70, true);
			this.detailFieldsRowLayoutPanel.TabIndex = 2;
			// 
			// menuSectionDropEdit
			// 
			this.menuSectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.menuSectionDropEdit, "ING_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ModuleDescription)));
			this.menuSectionDropEdit.BindToForDescription = "ING_ModuleDescription";
			this.menuSectionDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("86f00bc6-8710-453d-a433-cf8c0a310ed4", "Section", "Menu Section");
			this.menuSectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 46, true);
			this.menuSectionDropEdit.MaxItemsToShowInDropDown = 20;
			this.menuSectionDropEdit.Name = "menuSectionDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.menuSectionDropEdit, 2);
			this.menuSectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.menuSectionDropEdit.TabIndex = 4;
			this.menuSectionDropEdit.Visible = false;
			// 
			// cr8ModuleDropEdit
			// 
			this.cr8ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cr8ModuleDropEdit, "ING_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ModuleDescription)));
			this.cr8ModuleDropEdit.BindToForDescription = "ING_ModuleDescription";
			this.cr8ModuleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("a4bc292e-3bcb-43b1-8338-768dfb2eb269", "Requirement");
			this.cr8ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 46, true);
			this.cr8ModuleDropEdit.MaxItemsToShowInDropDown = 20;
			this.cr8ModuleDropEdit.Name = "cr8ModuleDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.cr8ModuleDropEdit, 2);
			this.cr8ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 15, true);
			this.cr8ModuleDropEdit.TabIndex = 5;
			this.cr8ModuleDropEdit.Visible = false;
			// 
			// productDropEdit
			// 
			this.productDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.productDropEdit, "ING_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Product)));
			this.productDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.productDropEdit.Name = "productDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.productDropEdit, 0);
			this.productDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 15, true);
			this.productDropEdit.TabIndex = 1;
			// 
			// cr9ModuleDropEdit
			// 
			this.cr9ModuleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cr9ModuleDropEdit, "ING_Module");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ModuleDescription)));
			this.cr9ModuleDropEdit.BindToForDescription = "ING_ModuleDescription";
			this.cr9ModuleDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("e738fb96-691a-44ca-8c57-f4da9b6fb267", "Service");
			this.cr9ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 46, true);
			this.cr9ModuleDropEdit.MaxItemsToShowInDropDown = 20;
			this.cr9ModuleDropEdit.Name = "cr9ModuleDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.cr9ModuleDropEdit, 2);
			this.cr9ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 15, true);
			this.cr9ModuleDropEdit.TabIndex = 6;
			this.cr9ModuleDropEdit.Visible = false;
			// 
			// serviceTypeDropEdit
			// 
			this.serviceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.serviceTypeDropEdit, "ING_ServiceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ServiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ServiceTypeDescription)));
			this.serviceTypeDropEdit.BindToForDescription = "ING_ServiceTypeDescription";
			this.serviceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 69, true);
			this.serviceTypeDropEdit.MaxItemsToShowInDropDown = 20;
			this.serviceTypeDropEdit.Name = "serviceTypeDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.serviceTypeDropEdit, 3);
			this.serviceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 15, true);
			this.serviceTypeDropEdit.TabIndex = 8;
			// 
			// countryDropEdit
			// 
			this.countryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryDropEdit, "ING_RN_NKCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_RN_NKCountry)));
			this.countryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 46, true);
			this.countryDropEdit.MaxItemsToShowInDropDown = 20;
			this.countryDropEdit.Name = "countryDropEdit";
			this.countryDropEdit.PreBoundMaxLength = 2;
			this.detailFieldsRowLayoutPanel.SetRow(this.countryDropEdit, 2);
			this.countryDropEdit.ShowDescriptionBox = false;
			this.countryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 15, true);
			this.countryDropEdit.TabIndex = 7;
			// 
			// criticalityDropEdit
			// 
			this.criticalityDropEdit.AllowDrop = true;
			this.criticalityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.criticalityDropEdit, "ING_Priority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_Priority)));
			this.criticalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.criticalityDropEdit.Name = "criticalityDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.criticalityDropEdit, 1);
			this.criticalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 15, true);
			this.criticalityDropEdit.TabIndex = 3;
			// 
			// productAreaDropEdit
			// 
			this.productAreaDropEdit.AllowDrop = true;
			this.productAreaDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.productAreaDropEdit, "ING_ProductArea");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_ProductArea)));
			this.productAreaDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(292, 0, true);
			this.productAreaDropEdit.Name = "productAreaDropEdit";
			this.detailFieldsRowLayoutPanel.SetRow(this.productAreaDropEdit, 0);
			this.productAreaDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 15, true);
			this.productAreaDropEdit.TabIndex = 2;
			// 
			// menuItemCaption
			// 
			this.menuItemCaption.AutoSize = true;
			this.menuItemCaption.CaptionResourceString = ZClientEDI.Res.GetData("74795467-b26e-4a91-acf6-2f27bff6dc65", "Menu Item:");
			this.menuItemCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.menuItemCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 125, true);
			this.menuItemCaption.Name = "menuItemCaption";
			this.menuItemCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 12, true);
			this.menuItemCaption.TabIndex = 12;
			// 
			// OverrideSourceModuleButton
			// 
			this.OverrideSourceModuleButton.CaptionResourceString = ZClientEDI.Res.GetData("aab144b9-5e95-4b50-b6fe-463fcd5aec0a", "Override");
			this.OverrideSourceModuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 120, true);
			this.OverrideSourceModuleButton.Name = "OverrideSourceModuleButton";
			this.OverrideSourceModuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 22, true);
			this.OverrideSourceModuleButton.TabIndex = 11;
			this.OverrideSourceModuleButton.ToolTipCaption = null;
			this.OverrideSourceModuleButton.UseVisualStyleBackColor = true;
			this.OverrideSourceModuleButton.Click += new System.EventHandler(this.OverrideSourceModuleButton_Click);
			// 
			// TriageAssistButton
			// 
			this.TriageAssistButton.CaptionResourceString = ZClientEDI.Res.GetData("cf68cd53-ab17-448e-a9b3-70d305213553", "Triage Assist");
			this.TriageAssistButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 95, true);
			this.TriageAssistButton.Name = "TriageAssistButton";
			this.TriageAssistButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 22, true);
			this.TriageAssistButton.TabIndex = 10;
			this.TriageAssistButton.ToolTipCaption = null;
			this.TriageAssistButton.UseVisualStyleBackColor = true;
			this.TriageAssistButton.Click += new System.EventHandler(this.TriageAssistButton_Click);
			// 
			// serviceTypeCaption
			// 
			this.serviceTypeCaption.AutoSize = true;
			this.serviceTypeCaption.CaptionResourceString = ZClientEDI.Res.GetData("8e9f0bf5-3e8b-4b4d-b4ef-3b5adb405c86", "Service Type");
			this.serviceTypeCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.serviceTypeCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 90, true);
			this.serviceTypeCaption.Name = "serviceTypeCaption";
			this.serviceTypeCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.serviceTypeCaption.TabIndex = 2;
			this.serviceTypeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// sourceModuleLabel
			// 
			this.sourceModuleLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.sourceModuleLabel, "SourceModuleWithPath");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).SourceModuleWithPath)));
			this.sourceModuleLabel.CaptionResourceString = ZClientEDI.Res.GetData("667ef025-a9f6-45de-8419-5a734ddbcf11", "Menu Item");
			this.sourceModuleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.sourceModuleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 125, true);
			this.sourceModuleLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.sourceModuleLabel.Name = "sourceModuleLabel";
			this.sourceModuleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.sourceModuleLabel.TabIndex = 9;
			this.sourceModuleLabel.Text = "<Menu Item>";
			this.sourceModuleLabel.UseMnemonic = false;
			// 
			// activityFeedGroupBox
			// 
			this.activityFeedGroupBox.Controls.Add(this.kTabControl1);
			this.activityFeedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.activityFeedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.activityFeedGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 5, true);
			this.activityFeedGroupBox.Name = "activityFeedGroupBox";
			this.activityFeedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 645, true);
			this.activityFeedGroupBox.TabIndex = 10;
			this.activityFeedGroupBox.TabStop = false;
			this.activityFeedGroupBox.Text = "Activity Feed";
			// 
			// kTabControl1
			// 
			this.kTabControl1.Controls.Add(this.eConversationTabPage);
			this.kTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.kTabControl1.Name = "kTabControl1";
			this.kTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 630, true);
			this.kTabControl1.TabIndex = 0;
			// 
			// eConversationTabPage
			// 
			this.eConversationTabPage.Controls.Add(this.eConversationMessageBoxsplitContainer);
			this.eConversationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.eConversationTabPage.Name = "eConversationTabPage";
			this.eConversationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.eConversationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 606, true);
			this.eConversationTabPage.TabIndex = 0;
			this.eConversationTabPage.Text = "eConversation";
			// 
			// eConversationMessageBoxsplitContainer
			// 
			this.eConversationMessageBoxsplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationMessageBoxsplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.eConversationMessageBoxsplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.eConversationMessageBoxsplitContainer.Name = "eConversationMessageBoxsplitContainer";
			this.eConversationMessageBoxsplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// eConversationMessageBoxsplitContainer.Panel1
			// 
			this.eConversationMessageBoxsplitContainer.Panel1.Controls.Add(this.kTableLayoutPanel1);
			this.eConversationMessageBoxsplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 600, true);
			this.eConversationMessageBoxsplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			// 
			// eConversationMessageBoxsplitContainer.Panel2
			// 
			this.eConversationMessageBoxsplitContainer.Panel2.Controls.Add(this.eConversationMessageListUserControl1);
			this.eConversationMessageBoxsplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			this.eConversationMessageBoxsplitContainer.SplitterWidth = 8;
			this.eConversationMessageBoxsplitContainer.TabIndex = 4;
			// 
			// kTableLayoutPanel1
			// 
			this.kTableLayoutPanel1.ColumnCount = 2;
			this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
			this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.kTableLayoutPanel1.Controls.Add(this.sendMessageButton, 1, 0);
			this.kTableLayoutPanel1.Controls.Add(this.conversationMessageTextBox, 0, 0);
			this.kTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kTableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kTableLayoutPanel1.Name = "kTableLayoutPanel1";
			this.kTableLayoutPanel1.RowCount = 1;
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.kTableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 50, true);
			this.kTableLayoutPanel1.TabIndex = 0;
			// 
			// sendMessageButton
			// 
			this.sendMessageButton.CaptionResourceString = ZClientEDI.Res.GetData("496ccf09-63c3-4007-b9bb-ca3ff32b8bb8", "Send");
			this.sendMessageButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sendMessageButton.Enabled = false;
			this.sendMessageButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 1, true);
			this.sendMessageButton.Name = "sendMessageButton";
			this.sendMessageButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 47, true);
			this.sendMessageButton.TabIndex = 5;
			this.sendMessageButton.ToolTipCaption = null;
			this.sendMessageButton.UseVisualStyleBackColor = true;
			// 
			// conversationMessageTextBox
			// 
			this.conversationMessageTextBox.CaptionResourceString = ZClientEDI.Res.GetData("e283c4b1-97ee-4a4b-b99a-c12b16a58c37", "eConversation Message");
			this.conversationMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.conversationMessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.conversationMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.conversationMessageTextBox.Multiline = true;
			this.conversationMessageTextBox.Name = "conversationMessageTextBox";
			this.conversationMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.conversationMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 47, true);
			this.conversationMessageTextBox.TabIndex = 1;
			// 
			// eConversationMessageListUserControl1
			// 
			this.eConversationMessageListUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eConversationMessageListUserControl1, "EConversation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.EConversation.Business.IConversation)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).EConversation)));
			this.eConversationMessageListUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eConversationMessageListUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eConversationMessageListUserControl1.Name = "eConversationMessageListUserControl1";
			this.eConversationMessageListUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 542, true);
			this.eConversationMessageListUserControl1.TabIndex = 1;
			// 
			// incidentManagementGroupCustomFieldsControl
			// 
			this.incidentManagementGroupCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentManagementGroupCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.incidentManagementGroupCustomFieldsControl.Name = "incidentManagementGroupCustomFieldsControl";
			this.incidentManagementGroupCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 186, true);
			this.incidentManagementGroupCustomFieldsControl.TabIndex = 11;
			this.incidentManagementGroupCustomFieldsControl.TabStop = false;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.mainLeftSplitContainer);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.activityFeedGroupBox);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 700, true);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(405);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(889);
			this.mainSplitContainer.SplitterWidth = 8;
			this.mainSplitContainer.TabIndex = 0;
			// 
			// mainLeftSplitContainer
			// 
			this.mainLeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainLeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainLeftSplitContainer.Name = "mainLeftSplitContainer";
			// 
			// mainLeftSplitContainer.Panel1
			// 
			this.mainLeftSplitContainer.Panel1.Controls.Add(this.leftTableLayoutPanel);
			this.mainLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(889, 645, true);
			this.mainLeftSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			// 
			// mainLeftSplitContainer.Panel2
			// 
			this.mainLeftSplitContainer.Panel2.Controls.Add(this.centerSplitContainer);
			this.mainLeftSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(405);
			this.mainLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.mainLeftSplitContainer.SplitterWidth = 8;
			this.mainLeftSplitContainer.TabIndex = 15;
			// 
			// leftTableLayoutPanel
			// 
			this.leftTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this.leftTableLayoutPanel.Controls.Add(this.incidentManagementGroupGroupBox, 0, 0);
			this.leftTableLayoutPanel.Controls.Add(this.groupAndDescriptionSplitContainer, 0, 1);
			this.leftTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.leftTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.leftTableLayoutPanel.Name = "leftTableLayoutPanel";
			this.leftTableLayoutPanel.RowCount = 2;
			this.leftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(177)));
			this.leftTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(49)));
			this.leftTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 645, true);
			this.leftTableLayoutPanel.TabIndex = 15;
			// 
			// groupAndDescriptionSplitContainer
			// 
			this.groupAndDescriptionSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupAndDescriptionSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 178, true);
			this.groupAndDescriptionSplitContainer.Name = "groupAndDescriptionSplitContainer";
			this.groupAndDescriptionSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// groupAndDescriptionSplitContainer.Panel1
			// 
			this.groupAndDescriptionSplitContainer.Panel1.Controls.Add(this.groupDescriptionGroupBox);
			this.groupAndDescriptionSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 465, true);
			this.groupAndDescriptionSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(170);
			// 
			// groupAndDescriptionSplitContainer.Panel2
			// 
			this.groupAndDescriptionSplitContainer.Panel2.Controls.Add(this.milestonesGroupBox);
			this.groupAndDescriptionSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(267);
			this.groupAndDescriptionSplitContainer.SplitterWidth = 8;
			this.groupAndDescriptionSplitContainer.TabIndex = 12;
			// 
			// centerSplitContainer
			// 
			this.centerSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.centerSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.centerSplitContainer.Name = "centerSplitContainer";
			this.centerSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// centerSplitContainer.Panel1
			// 
			this.centerSplitContainer.Panel1.Controls.Add(this.stageAndDetailsSplitContainer);
			this.centerSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 645, true);
			this.centerSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			// 
			// centerSplitContainer.Panel2
			// 
			this.centerSplitContainer.Panel2.Controls.Add(this.incidentManagementGroupCustomFieldsControl);
			this.centerSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(451);
			this.centerSplitContainer.SplitterWidth = 8;
			this.centerSplitContainer.TabIndex = 1;
			// 
			// stageAndDetailsSplitContainer
			// 
			this.stageAndDetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.stageAndDetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.stageAndDetailsSplitContainer.Name = "stageAndDetailsSplitContainer";
			this.stageAndDetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// stageAndDetailsSplitContainer.Panel1
			// 
			this.stageAndDetailsSplitContainer.Panel1.Controls.Add(this.statusGroupBox);
			// 
			// stageAndDetailsSplitContainer.Panel2
			// 
			this.stageAndDetailsSplitContainer.Panel2.Controls.Add(this.productDetailsGroupBox);
			this.stageAndDetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 472, true);
			this.stageAndDetailsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(155);
			this.stageAndDetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(296);
			this.stageAndDetailsSplitContainer.SplitterWidth = 8;
			this.stageAndDetailsSplitContainer.TabIndex = 13;
			// 
			// TriageAssistLabel
			// 
			this.TriageAssistLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TriageAssistLabel, "TriageDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).TriageDescription)));
			this.TriageAssistLabel.CaptionResourceString = ZClientEDI.Res.GetData("a348fdcd-a9c6-4292-91d0-0d256b877d48", "Triage Node");
			this.TriageAssistLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TriageAssistLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 100, true);
			this.TriageAssistLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TriageAssistLabel.Name = "TriageAssistLabel";
			this.TriageAssistLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 12, true);
			this.TriageAssistLabel.TabIndex = 13;
			this.TriageAssistLabel.Text = "<Triage Node:>";
			this.TriageAssistLabel.UseMnemonic = false;
			// 
			// TriageAssistCaption
			// 
			this.TriageAssistCaption.AutoSize = true;
			this.TriageAssistCaption.CaptionResourceString = ZClientEDI.Res.GetData("227859d9-db1f-4017-956b-8c5bea0e41c9", "Triage Node:");
			this.TriageAssistCaption.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TriageAssistCaption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 100, true);
			this.TriageAssistCaption.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TriageAssistCaption.Name = "TriageAssistCaption";
			this.TriageAssistCaption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 12, true);
			this.TriageAssistCaption.TabIndex = 14;
			this.TriageAssistCaption.UseMnemonic = false;
			// 
			// IncidentManagementGroupDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "IncidentManagementGroupDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 645, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.incidentManagementGroupGroupBox.ResumeLayout(false);
			this.incidentManagementGroupGroupBox.PerformLayout();
			this.groupOwnerCodeFindBox.ResumeLayout(true);
			this.groupOwnerCodeFindBox.PerformLayout();
			this.urgencyDropEdit.ResumeLayout(true);
			this.urgencyDropEdit.PerformLayout();
			this.businessImpactDropEdit.ResumeLayout(true);
			this.businessImpactDropEdit.PerformLayout();
			this.serviceOutageDropEdit.ResumeLayout(true);
			this.serviceOutageDropEdit.PerformLayout();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.groupDescriptionGroupBox.ResumeLayout(false);
			this.groupDescriptionGroupBox.PerformLayout();
			this.groupDescriptionTableLayout.ResumeLayout(false);
			this.groupDescriptionTableLayout.PerformLayout();
			this.milestonesGroupBox.ResumeLayout(false);
			this.milestonesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.milestonesGrid)).EndInit();
			this.milestonesGrid.ResumeLayout(false);
			this.milestonesGrid.PerformLayout();
			this.statusGroupBox.ResumeLayout(false);
			this.statusGroupBox.PerformLayout();
			this.stageAndFieldsSplitContainer.Panel1.ResumeLayout(false);
			this.stageAndFieldsSplitContainer.Panel2.ResumeLayout(false);
			this.stageAndFieldsSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.stageAndFieldsSplitContainer)).EndInit();
			this.stageAndFieldsSplitContainer.ResumeLayout(false);
			this.stageAndFieldsSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.StagesGrid)).EndInit();
			this.StagesGrid.ResumeLayout(false);
			this.StagesGrid.PerformLayout();
			this.productDetailsGroupBox.ResumeLayout(false);
			this.productDetailsGroupBox.PerformLayout();
			this.detailFieldsRowLayoutPanel.ResumeLayout(false);
			this.detailFieldsRowLayoutPanel.PerformLayout();
			this.menuSectionDropEdit.ResumeLayout(true);
			this.menuSectionDropEdit.PerformLayout();
			this.cr8ModuleDropEdit.ResumeLayout(true);
			this.cr8ModuleDropEdit.PerformLayout();
			this.productDropEdit.ResumeLayout(true);
			this.productDropEdit.PerformLayout();
			this.cr9ModuleDropEdit.ResumeLayout(true);
			this.cr9ModuleDropEdit.PerformLayout();
			this.serviceTypeDropEdit.ResumeLayout(true);
			this.serviceTypeDropEdit.PerformLayout();
			this.countryDropEdit.ResumeLayout(true);
			this.countryDropEdit.PerformLayout();
			this.criticalityDropEdit.ResumeLayout(true);
			this.criticalityDropEdit.PerformLayout();
			this.productAreaDropEdit.ResumeLayout(true);
			this.productAreaDropEdit.PerformLayout();
			this.activityFeedGroupBox.ResumeLayout(false);
			this.activityFeedGroupBox.PerformLayout();
			this.kTabControl1.ResumeLayout(false);
			this.kTabControl1.PerformLayout();
			this.eConversationTabPage.ResumeLayout(false);
			this.eConversationTabPage.PerformLayout();
			this.eConversationMessageBoxsplitContainer.Panel1.ResumeLayout(false);
			this.eConversationMessageBoxsplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.eConversationMessageBoxsplitContainer)).EndInit();
			this.eConversationMessageBoxsplitContainer.ResumeLayout(false);
			this.eConversationMessageBoxsplitContainer.PerformLayout();
			this.kTableLayoutPanel1.ResumeLayout(false);
			this.kTableLayoutPanel1.PerformLayout();
			this.eConversationMessageListUserControl1.ResumeLayout(true);
			this.eConversationMessageListUserControl1.PerformLayout();
			this.incidentManagementGroupCustomFieldsControl.ResumeLayout(true);
			this.incidentManagementGroupCustomFieldsControl.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.mainLeftSplitContainer.Panel1.ResumeLayout(false);
			this.mainLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainLeftSplitContainer)).EndInit();
			this.mainLeftSplitContainer.ResumeLayout(false);
			this.mainLeftSplitContainer.PerformLayout();
			this.leftTableLayoutPanel.ResumeLayout(false);
			this.leftTableLayoutPanel.PerformLayout();
			this.groupAndDescriptionSplitContainer.Panel1.ResumeLayout(false);
			this.groupAndDescriptionSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.groupAndDescriptionSplitContainer)).EndInit();
			this.groupAndDescriptionSplitContainer.ResumeLayout(false);
			this.groupAndDescriptionSplitContainer.PerformLayout();
			this.centerSplitContainer.Panel1.ResumeLayout(false);
			this.centerSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.centerSplitContainer)).EndInit();
			this.centerSplitContainer.ResumeLayout(false);
			this.centerSplitContainer.PerformLayout();
			this.stageAndDetailsSplitContainer.Panel1.ResumeLayout(false);
			this.stageAndDetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.stageAndDetailsSplitContainer)).EndInit();
			this.stageAndDetailsSplitContainer.ResumeLayout(false);
			this.stageAndDetailsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZDropEdit typeDropEdit;
		private ZDropEdit urgencyDropEdit;
		private ZDropEdit serviceOutageDropEdit;
		private ZTextBox descriptionTextBox;
		private ZDropEdit businessImpactDropEdit;
		private ZCodeFindBox groupOwnerCodeFindBox;
		private ZGroupBox incidentManagementGroupGroupBox;
		private ZGroupBox groupDescriptionGroupBox;
		private ZGroupBox activityFeedGroupBox;
		private ZGroupBox productDetailsGroupBox;
		private ZGroupBox statusGroupBox;
		private ZGroupBox milestonesGroupBox;
		private ZTextBox initialSymptomsTextBox;
		private ZTextBox rootCauseTextBox;
		private ZTextBox businessImpactDescriptionTextBox;
		private ZLabel taskStatusTextLabel;
		private ZLabel currentGroupStatusLabel;
		private ZLabel currentTaskLabel;
		private ZLabel taskAssignedLabel;
		private ZLabel taskStatusLabel;
		private ZLabel updateStatusLabel;
		private ZLabel currentTaskTextLabel;
		private ZLabel taskAssignedCodeTextLabel;
		private ZLabel currentGroupStatusTextLabel;
		protected ZButton PreviousButton;
		protected ZButton NextButton;
		protected ZGrid StagesGrid;
		private ZGrid milestonesGrid;
		protected ZToolStripDropDownButton ServiceRestoredButton;
		protected ZArchitecture.GUI.ZToolStripMenuItem ServiceOutageStartMenuItem;
		protected ZArchitecture.GUI.ZToolStripMenuItem ServiceOutageDowngradedMenuItem;
		protected ZArchitecture.GUI.ZToolStripMenuItem ServiceRestoredMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZToolStrip toolStripMilestoneButtons;
		private ZLabel taskAssignedDescriptionTextLabel;
		private ZDropEdit productDropEdit;
		private ZDropEdit countryDropEdit;
		private ZDropEdit menuSectionDropEdit;
		private ZDropEdit cr8ModuleDropEdit;
		private ZDropEdit cr9ModuleDropEdit;
		private ZDropEdit criticalityDropEdit;
		private ZDropEdit productAreaDropEdit;
		protected ZButton OverrideSourceModuleButton;
		protected ZButton TriageAssistButton;
		private ZLabel sourceModuleLabel;
		private ZLabel menuItemCaption;
		private ZDropEdit serviceTypeDropEdit;
		private ZLabel outageDurationTimeLabel;
		private ZLabel outageDurationLabel;
		private CargoWise.Windows.UI.KTabControl kTabControl1;
		private CargoWise.Windows.UI.KTabPage eConversationTabPage;
		private CargoWise.Windows.UI.KSplitContainer eConversationMessageBoxsplitContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel kTableLayoutPanel1;
		private ZButton sendMessageButton;
		private ZTextBox conversationMessageTextBox;
		private EConversation.GUI.EConversationMessageListUserControl eConversationMessageListUserControl1;
		private IncidentManagementGroupCustomFieldsControl incidentManagementGroupCustomFieldsControl;
		private KSplitContainer centerSplitContainer;
		private KSplitContainer mainSplitContainer;
		private KSplitContainer groupAndDescriptionSplitContainer;
		private KSplitContainer stageAndDetailsSplitContainer;
		private KTableLayoutPanel stageGridAndFieldsTableLayout;
		private RowLayoutPanel detailFieldsRowLayoutPanel;
		private KSplitContainer stageAndFieldsSplitContainer;
		private KTableLayoutPanel groupDescriptionTableLayout;
		private KSplitContainer mainLeftSplitContainer;
		private ZLabel serviceTypeCaption;
		private KTableLayoutPanel leftTableLayoutPanel;
		private ZLabel TriageAssistLabel;
		private ZLabel TriageAssistCaption;
	}
}
