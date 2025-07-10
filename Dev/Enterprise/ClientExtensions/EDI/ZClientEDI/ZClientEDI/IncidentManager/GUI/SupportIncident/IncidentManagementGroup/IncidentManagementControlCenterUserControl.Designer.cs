using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class IncidentManagementControlCenterUserControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			CommunicationArea?.Dispose();
			timer?.Dispose();
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo34 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.communicationSettings = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.autoCascadeWorkItems = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.workItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.autoReplyTriggerDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.communicationManagerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.communicationSettingsCheckBox4 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.communicationSettingsCheckBox3 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.communicationSettingsCheckBox2 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.communicationSettingsCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommunicationArea = new Enterprise.Client.EDI.IncidentManager.GUI.IncidentManagementGroupControlCenterCommunicationAreaUserControl();
			this.broadcastMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.broadcastMessageSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.communicationAndWorkItemSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.messageInventoryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageInventoryGrid = new Enterprise.Client.EDI.IncidentManager.GUI.MessageInventoryModuleButtonGrid();
			this.messageContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MessageContentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.messageContentButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CreateNewDraftButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RevertToDraftButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PublishButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.linkedIncidentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.linkedIncidentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LinkedIncidentsGrid = new Enterprise.Client.EDI.IncidentManager.GUI.LinkedIncidentsModuleButtonGrid();
			this.linkedIncidentsButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.refreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.countdownLabel = new ZArchitecture.ZLabel();
			this.timer = new Timer();
			this.assignResponderButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ManualBroadcastButtonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.broadcastInterimButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sendAutoReplyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IsControllableButtonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.allowControlButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.disallowControlButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.mainLeftSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.mainRightSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.incidentCommunicationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.communicationSettings.SuspendLayout();
			this.autoCascadeWorkItems.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.workItemsGrid)).BeginInit();
			this.workItemsGrid.SuspendLayout();
			this.DetachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AttachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.menuStripNew = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.toolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.menuStripAttach = new CargoWise.Windows.UI.KContextMenuStrip(this.components);
			this.toolStripMenuItem1 = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.autoReplyTriggerDropEdit.SuspendLayout();
			this.communicationManagerCodeFindBox.SuspendLayout();
			this.CommunicationArea.SuspendLayout();
			this.broadcastMessageGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.broadcastMessageSplitContainer)).BeginInit();
			this.broadcastMessageSplitContainer.Panel1.SuspendLayout();
			this.broadcastMessageSplitContainer.Panel2.SuspendLayout();
			this.broadcastMessageSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.communicationAndWorkItemSplitContainer)).BeginInit();
			this.communicationAndWorkItemSplitContainer.Panel1.SuspendLayout();
			this.communicationAndWorkItemSplitContainer.Panel2.SuspendLayout();
			this.communicationAndWorkItemSplitContainer.SuspendLayout();
			this.messageInventoryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageInventoryGrid.InnerGrid)).BeginInit();
			this.MessageInventoryGrid.SuspendLayout();
			this.messageContentPanel.SuspendLayout();
			this.messageContentButtonsPanel.SuspendLayout();
			this.linkedIncidentsGroupBox.SuspendLayout();
			this.linkedIncidentsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinkedIncidentsGrid.InnerGrid)).BeginInit();
			this.LinkedIncidentsGrid.SuspendLayout();
			this.linkedIncidentsButtonsPanel.SuspendLayout();
			this.ManualBroadcastButtonGroupBox.SuspendLayout();
			this.IsControllableButtonGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainLeftSplitContainer)).BeginInit();
			this.mainLeftSplitContainer.Panel1.SuspendLayout();
			this.mainLeftSplitContainer.Panel2.SuspendLayout();
			this.mainLeftSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainRightSplitContainer)).BeginInit();
			this.mainRightSplitContainer.Panel1.SuspendLayout();
			this.mainRightSplitContainer.Panel2.SuspendLayout();
			this.mainRightSplitContainer.SuspendLayout();
			this.incidentCommunicationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.menuStripNew.SuspendLayout();
			this.menuStripAttach.SuspendLayout();
			this.menuStripAttach.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup);
			// 
			// communicationSettings
			// 
			this.communicationSettings.CaptionResourceString = ZClientEDI.Res.GetData("126d81bb-4cbf-4572-8d17-d85cc7fea74f", "Communication Settings");
			this.communicationSettings.Controls.Add(this.autoReplyTriggerDropEdit);
			this.communicationSettings.Controls.Add(this.communicationManagerCodeFindBox);
			this.communicationSettings.Controls.Add(this.communicationSettingsCheckBox4);
			this.communicationSettings.Controls.Add(this.communicationSettingsCheckBox3);
			this.communicationSettings.Controls.Add(this.communicationSettingsCheckBox2);
			this.communicationSettings.Controls.Add(this.communicationSettingsCheckBox1);
			this.communicationSettings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.communicationSettings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.communicationSettings.Name = "communicationSettings";
			this.communicationSettings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 251, true);
			this.communicationSettings.TabIndex = 2;
			this.communicationSettings.TabStop = false;
			// 
			// autoCascadeWorkItems
			// 
			this.autoCascadeWorkItems.CaptionResourceString = ZClientEDI.Res.GetData("6c121860-804f-41b7-ad30-4ad38b3b5044", "Automatically Attach Work Items to Linked Incidents");
			this.autoCascadeWorkItems.Controls.Add(this.workItemsGrid);
			this.autoCascadeWorkItems.Controls.Add(this.DetachButton);
			this.autoCascadeWorkItems.Controls.Add(this.EditButton);
			this.autoCascadeWorkItems.Controls.Add(this.AttachButton);
			this.autoCascadeWorkItems.Controls.Add(this.NewButton);
			this.autoCascadeWorkItems.Dock = System.Windows.Forms.DockStyle.Fill;
			this.autoCascadeWorkItems.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 252, true);
			this.autoCascadeWorkItems.Name = "autoCascadeWorkItems";
			this.autoCascadeWorkItems.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 251, true);
			this.autoCascadeWorkItems.TabIndex = 3;
			this.autoCascadeWorkItems.TabStop = false;
			// 
			// workItemsGrid
			// 
			this.workItemsGrid.AllowNavigation = false;
			this.workItemsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.workItemsGrid, "AutoCascadeRelatedItems");
			this.workItemsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo23.Caption = "";
			zTextBoxColumnStyleInfo23.CaptionResourceString = ZClientEDI.Res.GetData("5c2aace3-49e5-49b3-959d-c1df5c5387fb", "Number");
			zTextBoxColumnStyleInfo23.ColumnName = "Number";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo24.Caption = "";
			zTextBoxColumnStyleInfo24.CaptionResourceString = ZClientEDI.Res.GetData("fb3e7d9c-a65b-42de-983f-57a4d1aa4546", "Description");
			zTextBoxColumnStyleInfo24.ColumnName = "ItemDescription";
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo30.Caption = "";
			zTextBoxColumnStyleInfo30.CaptionResourceString = ZClientEDI.Res.GetData("b818a059-71d4-4092-bd4f-6f2ccb7220a4", "Selection Criterion 1");
			zTextBoxColumnStyleInfo30.ColumnName = "SelectionCriterion1";
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo31.Caption = "";
			zTextBoxColumnStyleInfo31.CaptionResourceString = ZClientEDI.Res.GetData("eccca020-fe62-4d2a-8910-6266ce2bd361", "Selection Criterion 2");
			zTextBoxColumnStyleInfo31.ColumnName = "SelectionCriterion2";
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo32.Caption = "";
			zTextBoxColumnStyleInfo32.CaptionResourceString = ZClientEDI.Res.GetData("57592105-0347-44c1-a03c-69b33d6d82bb", "Selection Criterion 3");
			zTextBoxColumnStyleInfo32.ColumnName = "SelectionCriterion3";
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo33.Caption = "";
			zTextBoxColumnStyleInfo33.CaptionResourceString = ZClientEDI.Res.GetData("ed35c141-f6f2-49c6-a83c-05418fa93a54", "Selection Criterion 4");
			zTextBoxColumnStyleInfo33.ColumnName = "SelectionCriterion4";
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo34.Caption = "";
			zTextBoxColumnStyleInfo34.CaptionResourceString = ZClientEDI.Res.GetData("624246ab-3673-4f13-a799-5cd7b6e217f8", "Selection Criterion 5");
			zTextBoxColumnStyleInfo34.ColumnName = "SelectionCriterion5";
			zTextBoxColumnStyleInfo34.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo10.Caption = "";
			zCheckBoxColumnStyleInfo10.CaptionResourceString = ZClientEDI.Res.GetData("00dfb155-f432-4964-a251-6df0f1affde2", "Closed");
			zCheckBoxColumnStyleInfo10.ColumnName = "IsClosedOrCancelled";
			zCheckBoxColumnStyleInfo10.IsVisible = false;
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.workItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo34);
			this.workItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.workItemsGrid.GridId = "71e55504-328a-45c8-ad57-af49a62b66a2";
			this.workItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.workItemsGrid.LayoutKey = "workItemsGrid";
			this.workItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 15, true);
			this.workItemsGrid.Name = "workItemsGrid";
			this.workItemsGrid.ReadOnly = true;
			this.workItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 200, true);
			this.workItemsGrid.TabIndex = 0;
			this.workItemsGrid.DoubleClick += new System.EventHandler(this.WorkItemsGrid_DoubleClick);
			// 
			// DetachButton
			// 
			this.DetachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DetachButton.CaptionResourceString = ZClientEDI.Res.GetData("c6d84b6c-9dc8-4398-9951-6f7c9c1cda43", "Detach");
			this.DetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 220, true);
			this.DetachButton.Name = "DetachButton";
			this.DetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DetachButton.TabIndex = 14;
			this.DetachButton.ToolTipCaption = null;
			this.DetachButton.UseVisualStyleBackColor = true;
			this.DetachButton.Click += new System.EventHandler(this.DetachButton_Click);
			// 
			// EditButton
			// 
			this.EditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EditButton.CaptionResourceString = ZClientEDI.Res.GetData("18906695-eb44-4aef-a71a-280057b342ab", "Edit");
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 220, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.EditButton.TabIndex = 13;
			this.EditButton.ToolTipCaption = null;
			this.EditButton.UseVisualStyleBackColor = true;
			this.EditButton.Click += new System.EventHandler(this.EditButton_Click);
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.CaptionResourceString = ZClientEDI.Res.GetData("a08c00e5-ab40-4a2e-b022-7694c454eea1", "Attach");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 220, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AttachButton.TabIndex = 12;
			this.AttachButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.AttachButton.ToolTipCaption = null;
			this.AttachButton.UseVisualStyleBackColor = true;
			this.AttachButton.Click += new System.EventHandler(this.AttachButton_Click);
			// 
			// NewButton
			// 
			this.NewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewButton.CaptionResourceString = ZClientEDI.Res.GetData("39d2bed6-5f02-4d9f-8b48-c8aec554717c", "New");
			this.NewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 220, true);
			this.NewButton.Name = "NewButton";
			this.NewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NewButton.TabIndex = 11;
			this.NewButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.NewButton.ToolTipCaption = null;
			this.NewButton.UseVisualStyleBackColor = true;
			this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
			// 
			// autoReplyTriggerDropEdit
			// 
			this.autoReplyTriggerDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.autoReplyTriggerDropEdit, "AutoReplyDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).AutoReplyDescription)));
			this.autoReplyTriggerDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("cd9b4fc3-c251-4ee1-a5fc-56cf18da6064", "Auto-reply trigger");
			this.autoReplyTriggerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 40, true);
			this.autoReplyTriggerDropEdit.Name = "autoReplyTriggerDropEdit";
			this.autoReplyTriggerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 15, true);
			this.autoReplyTriggerDropEdit.TabIndex = 2;
			// 
			// communicationManagerCodeFindBox
			// 
			this.communicationManagerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.communicationManagerCodeFindBox, "ING_GS_NKGroupOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_GS_NKGroupOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Lookups.GroupOwners)));
			this.communicationManagerCodeFindBox.BindToList = "Lookups+GroupOwners";
			this.communicationManagerCodeFindBox.CaptionResourceString = ZClientEDI.Res.GetData("8d942a67-d139-4da8-a102-4e9881650e6d", "Communication Manager (Default responder)");
			this.communicationManagerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 20, true);
			this.communicationManagerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.communicationManagerCodeFindBox.Name = "communicationManagerCodeFindBox";
			this.communicationManagerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.communicationManagerCodeFindBox.ParentType = null;
			this.communicationManagerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 15, true);
			this.communicationManagerCodeFindBox.TabIndex = 1;
			// 
			// communicationSettingsCheckBox4
			// 
			this.communicationSettingsCheckBox4.AutoSize = true;
			this.BindingSource.SetBindingMember(this.communicationSettingsCheckBox4, "ING_IsKnownIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_IsKnownIssue)));
			this.communicationSettingsCheckBox4.CaptionResourceString = ZClientEDI.Res.GetData("60149fef-5129-4da6-999a-239949f3b6f1", "Publish this Incident Group as a Known Issue");
			this.communicationSettingsCheckBox4.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.communicationSettingsCheckBox4, false);
			this.communicationSettingsCheckBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 120, true);
			this.communicationSettingsCheckBox4.Name = "communicationSettingsCheckBox4";
			this.communicationSettingsCheckBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.communicationSettingsCheckBox4.TabIndex = 6;
			this.communicationSettingsCheckBox4.UseVisualStyleBackColor = true;
			// 
			// communicationSettingsCheckBox3
			// 
			this.communicationSettingsCheckBox3.AutoSize = true;
			this.BindingSource.SetBindingMember(this.communicationSettingsCheckBox3, "ING_IsInterimBroadcastUnflagsCommunication");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_IsInterimBroadcastUnflagsCommunication)));
			this.communicationSettingsCheckBox3.CaptionResourceString = ZClientEDI.Res.GetData("98e164ee-c1ec-44bb-bab9-1e83a1f6beb1", "Interim broadcast un-flags communication");
			this.communicationSettingsCheckBox3.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.communicationSettingsCheckBox3, false);
			this.communicationSettingsCheckBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 100, true);
			this.communicationSettingsCheckBox3.Name = "communicationSettingsCheckBox3";
			this.communicationSettingsCheckBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.communicationSettingsCheckBox3.TabIndex = 5;
			this.communicationSettingsCheckBox3.UseVisualStyleBackColor = true;
			// 
			// communicationSettingsCheckBox2
			// 
			this.communicationSettingsCheckBox2.AutoSize = true;
			this.BindingSource.SetBindingMember(this.communicationSettingsCheckBox2, "ING_IsAutoReplyUnflagsCommunication");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_IsAutoReplyUnflagsCommunication)));
			this.communicationSettingsCheckBox2.CaptionResourceString = ZClientEDI.Res.GetData("8b586884-ce2c-4292-aa0c-f67b116ce520", "Auto-reply un-flags communication");
			this.communicationSettingsCheckBox2.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.communicationSettingsCheckBox2, false);
			this.communicationSettingsCheckBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 80, true);
			this.communicationSettingsCheckBox2.Name = "communicationSettingsCheckBox2";
			this.communicationSettingsCheckBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.communicationSettingsCheckBox2.TabIndex = 4;
			this.communicationSettingsCheckBox2.UseVisualStyleBackColor = true;
			// 
			// communicationSettingsCheckBox1
			// 
			this.communicationSettingsCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.communicationSettingsCheckBox1, "ING_IsBroadcastToControlledOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).ING_IsBroadcastToControlledOnly)));
			this.communicationSettingsCheckBox1.CaptionResourceString = ZClientEDI.Res.GetData("737a09ab-6be5-4774-ad2f-9fcce205b3d4", "Only include controlled incidents in broadcasts");
			this.communicationSettingsCheckBox1.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.communicationSettingsCheckBox1, false);
			this.communicationSettingsCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 60, true);
			this.communicationSettingsCheckBox1.Name = "communicationSettingsCheckBox1";
			this.communicationSettingsCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.communicationSettingsCheckBox1.TabIndex = 3;
			this.communicationSettingsCheckBox1.UseVisualStyleBackColor = true;
			// 
			// CommunicationArea
			// 
			this.CommunicationArea.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommunicationArea, "");
			this.CommunicationArea.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommunicationArea.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.CommunicationArea.Name = "CommunicationArea";
			this.CommunicationArea.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 408, true);
			this.CommunicationArea.TabIndex = 0;
			this.CommunicationArea.Visible = false;
			// 
			// broadcastMessageGroupBox
			// 
			this.broadcastMessageGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("09d4946d-6048-455f-830f-60862705ff5e", "Broadcast Message");
			this.broadcastMessageGroupBox.Controls.Add(this.broadcastMessageSplitContainer);
			this.broadcastMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.broadcastMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.broadcastMessageGroupBox.Name = "broadcastMessageGroupBox";
			this.broadcastMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 373, true);
			this.broadcastMessageGroupBox.TabIndex = 0;
			this.broadcastMessageGroupBox.TabStop = false;
			// 
			// broadcastMessageSplitContainer
			// 
			this.broadcastMessageSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.broadcastMessageSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.broadcastMessageSplitContainer.Name = "broadcastMessageSplitContainer";
			this.broadcastMessageSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// broadcastMessageSplitContainer.Panel1
			// 
			this.broadcastMessageSplitContainer.Panel1.Controls.Add(this.messageInventoryPanel);
			// 
			// broadcastMessageSplitContainer.Panel2
			// 
			this.broadcastMessageSplitContainer.Panel2.Controls.Add(this.messageContentPanel);
			this.broadcastMessageSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 356, true);
			this.broadcastMessageSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			this.broadcastMessageSplitContainer.SplitterWidth = 7;
			this.broadcastMessageSplitContainer.TabIndex = 23;
			// 
			// communicationAndWorkItemSplitContainer
			// 
			this.communicationAndWorkItemSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.communicationAndWorkItemSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.communicationAndWorkItemSplitContainer.Name = "communicationAndWorkItemSplitContainer";
			this.communicationAndWorkItemSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// communicationAndWorkItemSplitContainer.Panel1
			// 
			this.communicationAndWorkItemSplitContainer.Panel1.Controls.Add(this.communicationSettings);
			// 
			// communicationAndWorkItemSplitContainer.Panel2
			// 
			this.communicationAndWorkItemSplitContainer.Panel2.Controls.Add(this.autoCascadeWorkItems);
			this.communicationAndWorkItemSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 390, true);
			this.communicationAndWorkItemSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(160);
			this.communicationAndWorkItemSplitContainer.SplitterWidth = 7;
			this.communicationAndWorkItemSplitContainer.TabIndex = 23;
			// 
			// messageInventoryPanel
			// 
			this.messageInventoryPanel.Controls.Add(this.MessageInventoryGrid);
			this.messageInventoryPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageInventoryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageInventoryPanel.Name = "messageInventoryPanel";
			this.messageInventoryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 160, true);
			this.messageInventoryPanel.TabIndex = 0;
			// 
			// MessageInventoryGrid
			// 
			this.MessageInventoryGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageInventoryGrid, "IncidentManagementGroupMessages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).IncidentManagementGroupMessages)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("6599adcc-9891-40fc-a1dc-f706f469cab1", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("633dae7d-e0d4-465e-9ed3-f9bb42750d62", "Version");
			zTextBoxColumnStyleInfo2.ColumnName = "IsPublishedDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("1a9a7611-2d05-44ae-9a05-81c5f3bf155b", "Last Update");
			zTextBoxColumnStyleInfo4.ColumnName = "SystemLastEditTimeLocal";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.MessageInventoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageInventoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessageInventoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessageInventoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInventoryGrid.GridId = "F7A4744D-B9BF-4453-B4C9-BFE952868A7F";
			// 
			// 
			// 
			this.MessageInventoryGrid.InnerGrid.AllowNavigation = false;
			this.MessageInventoryGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageInventoryGrid.InnerGrid.CaptionVisible = false;
			this.MessageInventoryGrid.InnerGrid.GridId = "F7A4744D-B9BF-4453-B4C9-BFE952868A7F";
			this.MessageInventoryGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageInventoryGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.MessageInventoryGrid.InnerGrid.LayoutKey = "messageInventoryInnerGrid";
			this.MessageInventoryGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.MessageInventoryGrid.InnerGrid.Name = "messageInventoryInnerGrid";
			this.MessageInventoryGrid.InnerGrid.ReadOnly = true;
			this.MessageInventoryGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(474, 123, true);
			this.MessageInventoryGrid.InnerGrid.TabIndex = 0;
			this.MessageInventoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageInventoryGrid.Name = "MessageInventoryGrid";
			this.MessageInventoryGrid.ReadOnly = true;
			this.MessageInventoryGrid.ShowAttachButton = false;
			this.MessageInventoryGrid.ShowDetachButton = false;
			this.MessageInventoryGrid.ShowEditButton = false;
			this.MessageInventoryGrid.ShowNewButton = false;
			this.MessageInventoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 160, true);
			this.MessageInventoryGrid.TabIndex = 13;
			// 
			// messageContentPanel
			// 
			this.messageContentPanel.Controls.Add(this.MessageContentTextBox);
			this.messageContentPanel.Controls.Add(this.messageContentButtonsPanel);
			this.messageContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageContentPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 40, true);
			this.messageContentPanel.Name = "messageContentPanel";
			this.messageContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 81, true);
			this.messageContentPanel.TabIndex = 4;
			// 
			// MessageContentTextBox
			// 
			this.MessageContentTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.MessageContentTextBox.CaptionResourceString = null;
			this.MessageContentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageContentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageContentTextBox, false);
			this.MessageContentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageContentTextBox.Multiline = true;
			this.MessageContentTextBox.Name = "MessageContentTextBox";
			this.MessageContentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageContentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 49, true);
			this.MessageContentTextBox.TabIndex = 2;
			this.MessageContentTextBox.TextChanged += new System.EventHandler(MessageContentTextBox_TextChanged);
			// 
			// messageContentButtonsPanel
			// 
			this.messageContentButtonsPanel.Controls.Add(this.CreateNewDraftButton);
			this.messageContentButtonsPanel.Controls.Add(this.RevertToDraftButton);
			this.messageContentButtonsPanel.Controls.Add(this.PublishButton);
			this.messageContentButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.messageContentButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 49, true);
			this.messageContentButtonsPanel.Name = "messageContentButtonsPanel";
			this.messageContentButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 32, true);
			this.messageContentButtonsPanel.TabIndex = 4;
			// 
			// CreateNewDraftButton
			// 
			this.CreateNewDraftButton.CaptionResourceString = ZClientEDI.Res.GetData("6394626d-1c46-48d0-8ba5-bb5626f67d05", "Create New Draft");
			this.CreateNewDraftButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.CreateNewDraftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 6, true);
			this.CreateNewDraftButton.Name = "CreateNewDraftButton";
			this.CreateNewDraftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CreateNewDraftButton.TabIndex = 0;
			this.CreateNewDraftButton.ToolTipCaption = null;
			this.CreateNewDraftButton.Click += new System.EventHandler(this.CreateNewDraftButton_Click);
			// 
			// RevertToDraftButton
			// 
			this.RevertToDraftButton.CaptionResourceString = ZClientEDI.Res.GetData("f2282c81-aeec-41da-96d8-7931776c93e9", "Revert To Draft");
			this.RevertToDraftButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.RevertToDraftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 6, true);
			this.RevertToDraftButton.Name = "RevertToDraftButton";
			this.RevertToDraftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.RevertToDraftButton.TabIndex = 0;
			this.RevertToDraftButton.ToolTipCaption = null;
			this.RevertToDraftButton.Click += new System.EventHandler(this.RevertToDraftButton_Click);
			// 
			// PublishButton
			// 
			this.PublishButton.CaptionResourceString = ZClientEDI.Res.GetData("30cebc94-7b15-47c3-bc88-a345e9837067", "Publish");
			this.PublishButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.PublishButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 6, true);
			this.PublishButton.Name = "PublishButton";
			this.PublishButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.PublishButton.TabIndex = 0;
			this.PublishButton.ToolTipCaption = null;
			this.PublishButton.Click += new System.EventHandler(this.PublishButton_Click);
			// 
			// linkedIncidentsGroupBox
			// 
			this.linkedIncidentsGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("fb65580e-c34f-41c4-bf10-26b9a3b1d0e7", "Linked Incidents");
			this.linkedIncidentsGroupBox.Controls.Add(this.linkedIncidentsPanel);
			this.linkedIncidentsGroupBox.Controls.Add(this.linkedIncidentsButtonsPanel);
			this.linkedIncidentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkedIncidentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.linkedIncidentsGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 0, true);
			this.linkedIncidentsGroupBox.Name = "linkedIncidentsGroupBox";
			this.linkedIncidentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 231, true);
			this.linkedIncidentsGroupBox.TabIndex = 0;
			this.linkedIncidentsGroupBox.TabStop = false;
			// 
			// linkedIncidentsPanel
			// 
			this.linkedIncidentsPanel.Controls.Add(this.LinkedIncidentsGrid);
			this.linkedIncidentsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkedIncidentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.linkedIncidentsPanel.Name = "linkedIncidentsPanel";
			this.linkedIncidentsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.linkedIncidentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 177, true);
			this.linkedIncidentsPanel.TabIndex = 0;
			// 
			// LinkedIncidentsGrid
			// 
			this.LinkedIncidentsGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LinkedIncidentsGrid, "LinkedIncidents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).LinkedIncidents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentManagementGroup)(null)).Lookups.IncidentsNotLinked)));
			this.LinkedIncidentsGrid.BindToFindBoxList = "Lookups+IncidentsNotLinked";
			this.LinkedIncidentsGrid.CaptionResourceString = ZClientEDI.Res.GetData("c2d3e004-1031-4321-abc7-8f2805c04d4f", "Linked Incidents");
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("40f70048-c763-4b01-9bb0-203246e8281e", "Incident");
			zTextBoxColumnStyleInfo5.ColumnName = "SupportIncident+IM_IncidentNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("dfd2ce65-18ca-4d61-9e87-13773dfbf6a4", "Customer");
			zTextBoxColumnStyleInfo6.ColumnName = "SupportIncident+Client+OH_FullName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("0070b3d1-7a83-473d-84ab-4de98f07ba03", "Country");
			zTextBoxColumnStyleInfo7.ColumnName = "SupportIncident+IM_RN_NKCountry";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("ffa31e8e-6eb5-4e3f-a491-5b68c4c523e9", "Client Time");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "SupportIncident+ClientLocalTime";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateTimeOffsetEditColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("5989cd15-a3f0-456c-a376-519a49f18d73", "Allow Control");
			zCheckBoxColumnStyleInfo1.ColumnName = "INL_IsGroupControlled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("b8541b4c-7746-451f-a157-85b211a15dd2", "Flagged");
			zCheckBoxColumnStyleInfo2.ColumnName = "Flagged";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.IsReadOnly = false;
			zCheckBoxColumnStyleInfo9.CaptionResourceString = ZClientEDI.Res.GetData("b6741dd0-dfa0-4b23-be0e-30377c35adea", "Is Controlled");
			zCheckBoxColumnStyleInfo9.ColumnName = "IsControlled";
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.CaptionResourceString = ZClientEDI.Res.GetData("52ada06c-c96c-4e35-bfb2-8f9f66edeb0f", "Responder");
			zTextBoxColumnStyleInfo10.ColumnName = "Responder+GS_FullName";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.CaptionResourceString = ZClientEDI.Res.GetData("11567e72-6235-4a97-ba76-fe429d4eeb29", "Work Item Number");
			zTextBoxColumnStyleInfo11.ColumnName = "SupportIncident+WorkItemNumber";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.CaptionResourceString = ZClientEDI.Res.GetData("92b33e7c-1f71-406e-b306-714f3010ac93", "Work Item Status");
			zTextBoxColumnStyleInfo12.ColumnName = "SupportIncident+WorkItemStatus";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.CaptionResourceString = ZClientEDI.Res.GetData("aa9d6610-935f-4f28-9592-4c485e12604f", "Disposition");
			zTextBoxColumnStyleInfo13.ColumnName = "SupportIncident+IM_ResolutionCodeDescription";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.CaptionResourceString = ZClientEDI.Res.GetData("0b2addae-3f6d-41d1-9d52-de651ecce587", "Stage");
			zTextBoxColumnStyleInfo14.ColumnName = "SupportIncident+Stage";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.CaptionResourceString = ZClientEDI.Res.GetData("efc25642-b98d-491d-a5ca-21f7e246e606", "Organization");
			zTextBoxColumnStyleInfo15.ColumnName = "SupportIncident+Client+OH_Code";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.CaptionResourceString = ZClientEDI.Res.GetData("4dbed4b2-7c75-4dc9-8bc0-18c9f650bbff", "Incident Status");
			zTextBoxColumnStyleInfo16.ColumnName = "SupportIncident+IM_StatusDescription";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.CaptionResourceString = ZClientEDI.Res.GetData("73a31c05-3797-431d-a5f9-081200e57395", "Waiting Time");
			zTextBoxColumnStyleInfo17.ColumnName = "CustomerWaitingTime";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zDateEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("b2f7b65b-acda-4487-bb78-8a2fc1dc0935", "Date Added");
			zDateEditColumnStyleInfo1.ColumnName = "SystemCreateTimeLocal";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo19.CaptionResourceString = ZClientEDI.Res.GetData("af3b5661-ed38-4eaf-aeda-000eed706db3", "Date Added (UTC)");
			zTextBoxColumnStyleInfo19.ColumnName = "INL_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.IsReadOnly = true;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zTextBoxColumnStyleInfo18.CaptionResourceString = ZClientEDI.Res.GetData("1f318202-d5d7-4237-bbfd-78570b386798", "Last Communication");
			zTextBoxColumnStyleInfo18.ColumnName = "LastCommunication";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.CaptionResourceString = ZClientEDI.Res.GetData("81667ea0-d4fb-4696-93ec-c2e997370ff6", "Description");
			zTextBoxColumnStyleInfo20.ColumnName = "SupportIncident+IM_Description";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo20.IsReadOnly = true;
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.LinkedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.LinkedIncidentsGrid.DetachMessage = ZClientEDI.Res.GetData("85B69025-AD4F-410A-8D92-9A91131F9207", "Are you sure you want to detach the selected incident?");
			this.LinkedIncidentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinkedIncidentsGrid.GridId = "E01D8950-E32C-45A7-BEA6-46DCB51A06C5";
			// 
			// 
			// 
			this.LinkedIncidentsGrid.InnerGrid.AllowNavigation = false;
			this.LinkedIncidentsGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.LinkedIncidentsGrid.InnerGrid.CaptionVisible = false;
			this.LinkedIncidentsGrid.InnerGrid.GridId = "E01D8950-E32C-45A7-BEA6-46DCB51A06C5";
			this.LinkedIncidentsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinkedIncidentsGrid.InnerGrid.LayoutKey = "linkedIncidentsInnerGrid";
			this.LinkedIncidentsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.LinkedIncidentsGrid.InnerGrid.Name = "linkedIncidentsInnerGrid";
			this.LinkedIncidentsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(791, 139, true);
			this.LinkedIncidentsGrid.InnerGrid.TabIndex = 0;
			this.LinkedIncidentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.LinkedIncidentsGrid.Name = "LinkedIncidentsGrid";
			this.LinkedIncidentsGrid.ReadOnly = false;
			this.LinkedIncidentsGrid.ShowEditButton = false;
			this.LinkedIncidentsGrid.ShowNewButton = false;
			this.LinkedIncidentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 177, true);
			this.LinkedIncidentsGrid.TabIndex = 12;
			// 
			// linkedIncidentsButtonsPanel
			//
			this.linkedIncidentsButtonsPanel.Controls.Add(this.countdownLabel);
			this.linkedIncidentsButtonsPanel.Controls.Add(this.refreshButton);
			this.linkedIncidentsButtonsPanel.Controls.Add(this.assignResponderButton);
			this.linkedIncidentsButtonsPanel.Controls.Add(this.ManualBroadcastButtonGroupBox);
			this.linkedIncidentsButtonsPanel.Controls.Add(this.IsControllableButtonGroupBox);
			this.linkedIncidentsButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.linkedIncidentsButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 159, true);
			this.linkedIncidentsButtonsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.linkedIncidentsButtonsPanel.Name = "linkedIncidentsButtonsPanel";
			this.linkedIncidentsButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 48, true);
			this.linkedIncidentsButtonsPanel.TabIndex = 2;
			// 
			// countdownLabel
			// 
			this.countdownLabel.CaptionResourceString = ZClientEDI.Res.GetData("759B0B22-B228-4B07-BF45-E8DD21EC45AA", "Time to board refresh");
			this.countdownLabel.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.countdownLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(595, 18, true);
			this.countdownLabel.Name = "countdownLabel";
			this.countdownLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.countdownLabel.TabIndex = 5;
			// 
			// refreshButton
			// 
			this.refreshButton.CaptionResourceString = ZClientEDI.Res.GetData("67B773D9-1718-4F0D-850C-C32FD11E554A", "Refresh");
			this.refreshButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.refreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 18, true);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.refreshButton.TabIndex = 4;
			this.refreshButton.ToolTipCaption = null;
			this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// assignResponderButton
			// 
			this.assignResponderButton.CaptionResourceString = ZClientEDI.Res.GetData("7faa18bf-b96b-4b20-b052-c6f92d9ce5bb", "Assign Responder");
			this.assignResponderButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.assignResponderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 18, true);
			this.assignResponderButton.Name = "assignResponderButton";
			this.assignResponderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.assignResponderButton.TabIndex = 3;
			this.assignResponderButton.ToolTipCaption = null;
			this.assignResponderButton.Click += new System.EventHandler(this.AssignResponderButton_Click);
			// 
			// ManualBroadcastButtonGroupBox
			// 
			this.ManualBroadcastButtonGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("a96abdbe-d53f-4237-a8c2-61a804fc04ce", "Manual Broadcast");
			this.ManualBroadcastButtonGroupBox.Controls.Add(this.broadcastInterimButton);
			this.ManualBroadcastButtonGroupBox.Controls.Add(this.sendAutoReplyButton);
			this.ManualBroadcastButtonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 3, true);
			this.ManualBroadcastButtonGroupBox.Name = "ManualBroadcastButtonGroupBox";
			this.ManualBroadcastButtonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 41, true);
			this.ManualBroadcastButtonGroupBox.TabIndex = 7;
			this.ManualBroadcastButtonGroupBox.TabStop = false;
			// 
			// broadcastInterimButton
			// 
			this.broadcastInterimButton.CaptionResourceString = ZClientEDI.Res.GetData("7a58b4e0-8244-4fa4-b428-2de399409b40", "Broadcast Interim");
			this.broadcastInterimButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.broadcastInterimButton.Name = "broadcastInterimButton";
			this.broadcastInterimButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.broadcastInterimButton.TabIndex = 0;
			this.broadcastInterimButton.ToolTipCaption = null;
			this.broadcastInterimButton.Click += new System.EventHandler(this.BroadcastInterimButton_Click);
			// 
			// sendAutoReplyButton
			// 
			this.sendAutoReplyButton.CaptionResourceString = ZClientEDI.Res.GetData("4cc4b1b1-7762-439d-9d66-eea7e04785ca", "Send Auto Reply");
			this.sendAutoReplyButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.sendAutoReplyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 15, true);
			this.sendAutoReplyButton.Name = "sendAutoReplyButton";
			this.sendAutoReplyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.sendAutoReplyButton.TabIndex = 1;
			this.sendAutoReplyButton.ToolTipCaption = null;
			this.sendAutoReplyButton.Click += new System.EventHandler(this.SendAutoReplyButton_Click);
			// 
			// IsControllableButtonGroupBox
			// 
			this.IsControllableButtonGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("a65dce9a-d72f-4382-8f11-355406a84e3d", "Is Controllable");
			this.IsControllableButtonGroupBox.Controls.Add(this.allowControlButton);
			this.IsControllableButtonGroupBox.Controls.Add(this.disallowControlButton);
			this.IsControllableButtonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.IsControllableButtonGroupBox.Name = "IsControllableButtonGroupBox";
			this.IsControllableButtonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 41, true);
			this.IsControllableButtonGroupBox.TabIndex = 6;
			this.IsControllableButtonGroupBox.TabStop = false;
			// 
			// controlButton
			// 
			this.allowControlButton.CaptionResourceString = ZClientEDI.Res.GetData("81b402d4-6661-4f57-af7b-0ffda4e0a5ee", "Allow");
			this.allowControlButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.allowControlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.allowControlButton.Name = "allowControlButton";
			this.allowControlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.allowControlButton.TabIndex = 4;
			this.allowControlButton.ToolTipCaption = null;
			this.allowControlButton.Click += new System.EventHandler(this.AllowControlButton_Click);
			// 
			// releaseButton
			// 
			this.disallowControlButton.CaptionResourceString = ZClientEDI.Res.GetData("4299a01d-61ef-47ba-9aa5-4d6225bab7d3", "Disallow");
			this.disallowControlButton.ImageAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.disallowControlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 15, true);
			this.disallowControlButton.Name = "disallowControlButton";
			this.disallowControlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.disallowControlButton.TabIndex = 5;
			this.disallowControlButton.ToolTipCaption = null;
			this.disallowControlButton.Click += new System.EventHandler(this.DisallowControlButton_Click);
			// 
			// mainLeftSplitContainer
			// 
			this.mainLeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainLeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainLeftSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 255, true);
			this.mainLeftSplitContainer.Name = "mainLeftSplitContainer";
			this.mainLeftSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainLeftSplitContainer.Panel1
			// 
			this.mainLeftSplitContainer.Panel1.Controls.Add(this.broadcastMessageGroupBox);
			this.mainLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 880, true);
			this.mainLeftSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(250);
			// 
			// mainLeftSplitContainer.Panel2
			// 
			this.mainLeftSplitContainer.Panel2.Controls.Add(this.communicationAndWorkItemSplitContainer);
			this.mainLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(373);
			this.mainLeftSplitContainer.SplitterWidth = 8;
			this.mainLeftSplitContainer.TabIndex = 14;
			// 
			// mainRightSplitContainer
			// 
			this.mainRightSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainRightSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainRightSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 215, true);
			this.mainRightSplitContainer.Name = "mainRightSplitContainer";
			this.mainRightSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainRightSplitContainer.Panel1
			// 
			this.mainRightSplitContainer.Panel1.AutoScroll = true;
			this.mainRightSplitContainer.Panel1.Controls.Add(this.linkedIncidentsGroupBox);
			this.mainRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 640, true);
			this.mainRightSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
			// 
			// mainRightSplitContainer.Panel2
			// 
			this.mainRightSplitContainer.Panel2.Controls.Add(this.incidentCommunicationGroupBox);
			this.mainRightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(209);
			this.mainRightSplitContainer.SplitterWidth = 8;
			this.mainRightSplitContainer.TabIndex = 15;
			// 
			// incidentCommunicationGroupBox
			// 
			this.incidentCommunicationGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("4a6231ad-881d-4fb2-b798-f78ce667bd23", "Incident Communication and Activity");
			this.incidentCommunicationGroupBox.Controls.Add(this.CommunicationArea);
			this.incidentCommunicationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.incidentCommunicationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.incidentCommunicationGroupBox.Name = "incidentCommunicationGroupBox";
			this.incidentCommunicationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 400, true);
			this.incidentCommunicationGroupBox.TabIndex = 0;
			this.incidentCommunicationGroupBox.TabStop = false;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 480, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.mainLeftSplitContainer);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 640, true);
			this.mainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.mainRightSplitContainer);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(143);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(481);
			this.mainSplitContainer.SplitterWidth = 8;
			this.mainSplitContainer.TabIndex = 16;
			// 
			// menuStripNew
			// 
			this.menuStripNew.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripMenuItem});
			this.menuStripNew.Name = "menuStripNewItem";
			this.menuStripNew.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			// 
			// toolStripMenuItem
			// 
			this.toolStripMenuItem.Name = "toolStripMenuItem";
			this.toolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 18, true);
			// 
			// menuStripAttach
			// 
			this.menuStripAttach.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.toolStripMenuItem1});
			this.menuStripAttach.Name = "menuStripAttachItem";
			this.menuStripAttach.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 21, true);
			// 
			// IncidentManagementControlCenterUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "IncidentManagementControlCenterUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 640, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.communicationSettings.ResumeLayout(false);
			this.communicationSettings.PerformLayout();
			this.autoCascadeWorkItems.ResumeLayout(false);
			this.autoCascadeWorkItems.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.workItemsGrid)).EndInit();
			this.workItemsGrid.ResumeLayout(false);
			this.workItemsGrid.PerformLayout();
			this.autoReplyTriggerDropEdit.ResumeLayout(true);
			this.autoReplyTriggerDropEdit.PerformLayout();
			this.communicationManagerCodeFindBox.ResumeLayout(true);
			this.communicationManagerCodeFindBox.PerformLayout();
			this.CommunicationArea.ResumeLayout(true);
			this.CommunicationArea.PerformLayout();
			this.broadcastMessageGroupBox.ResumeLayout(false);
			this.broadcastMessageGroupBox.PerformLayout();
			this.broadcastMessageSplitContainer.Panel1.ResumeLayout(false);
			this.broadcastMessageSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.broadcastMessageSplitContainer)).EndInit();
			this.broadcastMessageSplitContainer.ResumeLayout(false);
			this.broadcastMessageSplitContainer.PerformLayout();
			this.communicationAndWorkItemSplitContainer.Panel1.ResumeLayout(false);
			this.communicationAndWorkItemSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.communicationAndWorkItemSplitContainer)).EndInit();
			this.communicationAndWorkItemSplitContainer.ResumeLayout(false);
			this.communicationAndWorkItemSplitContainer.PerformLayout();
			this.messageInventoryPanel.ResumeLayout(false);
			this.messageInventoryPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageInventoryGrid.InnerGrid)).EndInit();
			this.MessageInventoryGrid.ResumeLayout(true);
			this.MessageInventoryGrid.PerformLayout();
			this.messageContentPanel.ResumeLayout(false);
			this.messageContentPanel.PerformLayout();
			this.messageContentButtonsPanel.ResumeLayout(false);
			this.messageContentButtonsPanel.PerformLayout();
			this.linkedIncidentsGroupBox.ResumeLayout(false);
			this.linkedIncidentsGroupBox.PerformLayout();
			this.linkedIncidentsPanel.ResumeLayout(false);
			this.linkedIncidentsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinkedIncidentsGrid.InnerGrid)).EndInit();
			this.LinkedIncidentsGrid.ResumeLayout(true);
			this.LinkedIncidentsGrid.PerformLayout();
			this.linkedIncidentsButtonsPanel.ResumeLayout(false);
			this.linkedIncidentsButtonsPanel.PerformLayout();
			this.ManualBroadcastButtonGroupBox.ResumeLayout(false);
			this.ManualBroadcastButtonGroupBox.PerformLayout();
			this.IsControllableButtonGroupBox.ResumeLayout(false);
			this.IsControllableButtonGroupBox.PerformLayout();
			this.mainLeftSplitContainer.Panel1.ResumeLayout(false);
			this.mainLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainLeftSplitContainer)).EndInit();
			this.mainLeftSplitContainer.ResumeLayout(false);
			this.mainLeftSplitContainer.PerformLayout();
			this.mainRightSplitContainer.Panel1.ResumeLayout(false);
			this.mainRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainRightSplitContainer)).EndInit();
			this.mainRightSplitContainer.ResumeLayout(false);
			this.mainRightSplitContainer.PerformLayout();
			this.incidentCommunicationGroupBox.ResumeLayout(false);
			this.incidentCommunicationGroupBox.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.menuStripNew.ResumeLayout(false);
			this.menuStripNew.PerformLayout();
			this.menuStripAttach.ResumeLayout(false);
			this.menuStripAttach.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		CargoWise.Windows.UI.KSplitContainer mainLeftSplitContainer;
		CargoWise.Windows.UI.KSplitContainer mainRightSplitContainer;

		Enterprise.ZArchitecture.GUI.ZGroupBox broadcastMessageGroupBox;
		CargoWise.Windows.UI.KSplitContainer broadcastMessageSplitContainer;
		CargoWise.Windows.UI.KSplitContainer communicationAndWorkItemSplitContainer;
		Enterprise.ZArchitecture.GUI.ZPanel messageInventoryPanel;
		protected Enterprise.Client.EDI.IncidentManager.GUI.MessageInventoryModuleButtonGrid MessageInventoryGrid;
		protected Enterprise.ZArchitecture.ZTextBox MessageContentTextBox;
		Enterprise.ZArchitecture.GUI.ZPanel messageContentPanel;
		Enterprise.ZArchitecture.GUI.ZPanel messageContentButtonsPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton CreateNewDraftButton;
		protected Enterprise.ZArchitecture.GUI.ZButton RevertToDraftButton;
		protected Enterprise.ZArchitecture.GUI.ZButton PublishButton;

		Enterprise.ZArchitecture.GUI.ZGroupBox communicationSettings;
		Enterprise.ZArchitecture.GUI.ZDropEdit autoReplyTriggerDropEdit;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox communicationManagerCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox communicationSettingsCheckBox1;
		Enterprise.ZArchitecture.GUI.ZCheckBox communicationSettingsCheckBox2;
		Enterprise.ZArchitecture.GUI.ZCheckBox communicationSettingsCheckBox3;
		Enterprise.ZArchitecture.GUI.ZCheckBox communicationSettingsCheckBox4;

		Enterprise.ZArchitecture.GUI.ZGroupBox autoCascadeWorkItems;
		public ZGrid workItemsGrid;

		Enterprise.ZArchitecture.GUI.ZGroupBox linkedIncidentsGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel linkedIncidentsPanel;
		protected Enterprise.Client.EDI.IncidentManager.GUI.LinkedIncidentsModuleButtonGrid LinkedIncidentsGrid;
		Enterprise.ZArchitecture.GUI.ZPanel linkedIncidentsButtonsPanel;
		protected Enterprise.ZArchitecture.GUI.ZButton broadcastInterimButton;
		protected Enterprise.ZArchitecture.GUI.ZButton sendAutoReplyButton;
		Enterprise.ZArchitecture.GUI.ZButton assignResponderButton;
		protected Enterprise.ZArchitecture.GUI.ZButton refreshButton;
		protected ZArchitecture.ZLabel countdownLabel;
		protected System.Windows.Forms.Timer timer;
		protected Enterprise.ZArchitecture.GUI.ZButton disallowControlButton;
		protected Enterprise.ZArchitecture.GUI.ZButton allowControlButton;
		protected Enterprise.Client.EDI.IncidentManager.GUI.IncidentManagementGroupControlCenterCommunicationAreaUserControl CommunicationArea;
		Enterprise.ZArchitecture.GUI.ZGroupBox incidentCommunicationGroupBox;
		//Enterprise.ZArchitecture.GUI.ZGroupBox incidentDetailsGroupBox;
		//Enterprise.ZArchitecture.GUI.ZGuidFindBox supportEnterpriseGuidFindBox;
		//Enterprise.ZArchitecture.GUI.ZCodeFindBox supportEnterpriseCodeFindBox;
		//Enterprise.ZArchitecture.GUI.ZGuidFindBox supportClientGuidFindBox;
		//Enterprise.ZArchitecture.ZLabel supportClientNameLabel;
		//Enterprise.ZArchitecture.GUI.ZGuidFindBox contactGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZButton DetachButton;
		protected Enterprise.ZArchitecture.GUI.ZButton EditButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NewButton;
		protected Enterprise.ZArchitecture.GUI.ZButton AttachButton;
		protected CargoWise.Windows.UI.KContextMenuStrip menuStripNew;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem toolStripMenuItem;
		protected CargoWise.Windows.UI.KContextMenuStrip menuStripAttach;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem toolStripMenuItem1;
		ZArchitecture.GUI.ZGroupBox ManualBroadcastButtonGroupBox;
		ZArchitecture.GUI.ZGroupBox IsControllableButtonGroupBox;

		#endregion
	}
}
