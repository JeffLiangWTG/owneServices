using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	partial class ErrorLogForm : ZForm
	{
		System.ComponentModel.IContainer components = null;
		internal ErrorLogRelatedWorkItemsModuleButtonGrid RelatedWorkItemGrid;
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl TabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage MainTab;
		Enterprise.ZArchitecture.ZTextBox ExceptionSourceTextBox;
		Enterprise.ZArchitecture.ZTextBox ExceptionTypeTextBox;
		Enterprise.ZArchitecture.ZTextBox ExceptionMessageTextBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit FirstReportedDateEdit;
		Enterprise.ZArchitecture.GUI.ZDateEdit FixedDateEdit;
		internal Enterprise.Client.EDI.IssueManager.GUI.OccurrencesControl OccurrencesControl;
		Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		ZPanel SaveButtonPanel;
		ZPanel oPanel1;
		internal CargoWise.Windows.UI.KButton WorkItemButton;
		Enterprise.ZArchitecture.GUI.ZTabPage RelatedWorkItemsTabPage;
		ZTabPage relatedIncidentsTabPage;
		Enterprise.ZArchitecture.ZGrid relatedIncidentsGrid;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		CargoWise.Windows.UI.KButton loadKeysButton;
		ZCheckBox clientVisibleCheckBox;
		ZGroupBox zGroupBox1;
		Enterprise.ZArchitecture.ZTextBox IssueNumberTextBox;
		ZDateEdit LastReportedDateEdit;
		ZCheckBox HasWorkItemCheckBox;
		ZDateEdit LatestExeDateEdit;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zEventTabPage1;

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SaveButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OccurrencesControl = new Enterprise.Client.EDI.IssueManager.GUI.OccurrencesControl();
			this.oPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HasWorkItemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LatestExeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LastReportedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.IssueNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.clientVisibleCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.loadKeysButton = new CargoWise.Windows.UI.KButton();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.WorkItemButton = new CargoWise.Windows.UI.KButton();
			this.ExceptionMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FirstReportedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExceptionTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExceptionSourceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FixedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RelatedWorkItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RelatedWorkItemGrid = new Enterprise.Client.EDI.IssueManager.GUI.ErrorLogRelatedWorkItemsModuleButtonGrid();
			this.relatedIncidentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.relatedIncidentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zEventTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SaveButtonPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.MainTab.SuspendLayout();
			this.OccurrencesControl.SuspendLayout();
			this.oPanel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.LatestExeDateEdit.SuspendLayout();
			this.LastReportedDateEdit.SuspendLayout();
			this.FirstReportedDateEdit.SuspendLayout();
			this.FixedDateEdit.SuspendLayout();
			this.RelatedWorkItemsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedWorkItemGrid.InnerGrid)).BeginInit();
			this.RelatedWorkItemGrid.SuspendLayout();
			this.relatedIncidentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.relatedIncidentsGrid)).BeginInit();
			this.relatedIncidentsGrid.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 490, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 23, true);
			this.MainStatusBar.TabIndex = 16;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(929);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog);
			// 
			// SaveButtonPanel
			// 
			this.SaveButtonPanel.Controls.Add(this.PostingButtonsUserControl);
			this.SaveButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SaveButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 461, true);
			this.SaveButtonPanel.Name = "SaveButtonPanel";
			this.SaveButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 29, true);
			this.SaveButtonPanel.TabIndex = 1;
			this.SaveButtonPanel.Text = "Status";
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(973, 4, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtonsUserControl.TabIndex = 0;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.MainTab);
			this.TabControl.Controls.Add(this.RelatedWorkItemsTabPage);
			this.TabControl.Controls.Add(this.relatedIncidentsTabPage);
			this.TabControl.Controls.Add(this.zStmNoteTabPage1);
			this.TabControl.Controls.Add(this.zEventTabPage1);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 461, true);
			this.TabControl.TabIndex = 0;
			// 
			// MainTab
			// 
			this.MainTab.Controls.Add(this.OccurrencesControl);
			this.MainTab.Controls.Add(this.oPanel1);
			this.MainTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTab.Name = "MainTab";
			this.MainTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.MainTab.TabIndex = 0;
			this.MainTab.Text = "Issue";
			// 
			// OccurrencesControl
			// 
			this.OccurrencesControl.AllowDrop = true;
			this.OccurrencesControl.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
			this.BindingSource.SetBindingMember(this.OccurrencesControl, ".");
			this.OccurrencesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OccurrencesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 143, true);
			this.OccurrencesControl.Name = "OccurrencesControl";
			this.OccurrencesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 291, true);
			this.OccurrencesControl.TabIndex = 1;
			// 
			// oPanel1
			// 
			this.oPanel1.Controls.Add(this.zGroupBox1);
			this.oPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.oPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.oPanel1.Name = "oPanel1";
			this.oPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 143, true);
			this.oPanel1.TabIndex = 0;
			this.oPanel1.Text = "Status";
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.HasWorkItemCheckBox);
			this.zGroupBox1.Controls.Add(this.LatestExeDateEdit);
			this.zGroupBox1.Controls.Add(this.LastReportedDateEdit);
			this.zGroupBox1.Controls.Add(this.IssueNumberTextBox);
			this.zGroupBox1.Controls.Add(this.clientVisibleCheckBox);
			this.zGroupBox1.Controls.Add(this.loadKeysButton);
			this.zGroupBox1.Controls.Add(this.zTextBox1);
			this.zGroupBox1.Controls.Add(this.WorkItemButton);
			this.zGroupBox1.Controls.Add(this.ExceptionMessageTextBox);
			this.zGroupBox1.Controls.Add(this.FirstReportedDateEdit);
			this.zGroupBox1.Controls.Add(this.ExceptionTypeTextBox);
			this.zGroupBox1.Controls.Add(this.ExceptionSourceTextBox);
			this.zGroupBox1.Controls.Add(this.FixedDateEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1267, 137, true);
			this.zGroupBox1.TabIndex = 22;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "Issue Details";
			// 
			// HasWorkItemCheckBox
			// 
			this.HasWorkItemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasWorkItemCheckBox, "HasWorkItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HasWorkItems)));
			this.HasWorkItemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasWorkItemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.HasWorkItemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 103, true);
			this.HasWorkItemCheckBox.Name = "HasWorkItemCheckBox";
			this.HasWorkItemCheckBox.ReadOnly = true;
			this.HasWorkItemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.HasWorkItemCheckBox.TabIndex = 14;
			this.HasWorkItemCheckBox.Text = "Has Work Items";
			this.HasWorkItemCheckBox.UseVisualStyleBackColor = true;
			// 
			// LatestExeDateEdit
			// 
			this.LatestExeDateEdit.AllowDrop = true;
			this.LatestExeDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestExeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestExeDateEdit, "HE_LastEXEVersionDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_LastEXEVersionDateLocal)));
			this.LatestExeDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("9f341ec0-a42b-4cd7-b058-0a952b15e876", "Latest EXE Date");
			this.LatestExeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestExeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 77, true);
			this.LatestExeDateEdit.Name = "LatestExeDateEdit";
			this.LatestExeDateEdit.TabIndex = 6;
			// 
			// LastReportedDateEdit
			// 
			this.LastReportedDateEdit.AllowDrop = true;
			this.LastReportedDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastReportedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastReportedDateEdit, "HE_LastReportedLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_LastReportedLocal)));
			this.LastReportedDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("b95194a6-0f32-453c-94fc-0b58f8cc0e12", "Last Reported");
			this.LastReportedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LastReportedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 77, true);
			this.LastReportedDateEdit.Name = "LastReportedDateEdit";
			this.LastReportedDateEdit.TabIndex = 5;
			// 
			// IssueNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.IssueNumberTextBox, "HE_IssueNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_IssueNumber)));
			this.IssueNumberTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.IssueNumberTextBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.IssueNumberTextBox.ForeColor = System.Drawing.Color.MediumBlue;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.IssueNumberTextBox, false);
			this.IssueNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
			this.IssueNumberTextBox.Name = "IssueNumberTextBox";
			this.IssueNumberTextBox.ReadOnly = true;
			this.IssueNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.IssueNumberTextBox.TabIndex = 0;
			this.IssueNumberTextBox.Text = "<ISSUE NUMBER>";
			// 
			// clientVisibleCheckBox
			// 
			this.clientVisibleCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.clientVisibleCheckBox, "HE_IsClientVisible");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_IsClientVisible)));
			this.clientVisibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.clientVisibleCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.clientVisibleCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 102, true);
			this.clientVisibleCheckBox.Name = "clientVisibleCheckBox";
			this.clientVisibleCheckBox.ReadOnly = true;
			this.clientVisibleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.clientVisibleCheckBox.TabIndex = 13;
			this.clientVisibleCheckBox.Text = "Is Client Visible";
			this.clientVisibleCheckBox.UseVisualStyleBackColor = true;
			// 
			// loadKeysButton
			// 
			this.loadKeysButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.loadKeysButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(834, 100, true);
			this.loadKeysButton.Name = "loadKeysButton";
			this.loadKeysButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 22, true);
			this.loadKeysButton.TabIndex = 16;
			this.loadKeysButton.Text = "Load Final Keys";
			this.loadKeysButton.ToolTipCaption = null;
			this.loadKeysButton.Click += new System.EventHandler(this.loadKeysButton_Click);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "HE_IssueNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_IssueNumber)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 77, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.zTextBox1.TabIndex = 7;
			// 
			// WorkItemButton
			// 
			this.WorkItemButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WorkItemButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 100, true);
			this.WorkItemButton.Name = "WorkItemButton";
			this.WorkItemButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 22, true);
			this.WorkItemButton.TabIndex = 15;
			this.WorkItemButton.Text = "Make Work &Item";
			this.WorkItemButton.ToolTipCaption = null;
			this.WorkItemButton.Click += new System.EventHandler(this.WorkItemButton_Click);
			// 
			// ExceptionMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExceptionMessageTextBox, "HE_ExceptionMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_ExceptionMessage)));
			this.ExceptionMessageTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExceptionMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 25, true);
			this.ExceptionMessageTextBox.Name = "ExceptionMessageTextBox";
			this.ExceptionMessageTextBox.ReadOnly = true;
			this.ExceptionMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 20, true);
			this.ExceptionMessageTextBox.TabIndex = 1;
			// 
			// FirstReportedDateEdit
			// 
			this.FirstReportedDateEdit.AllowDrop = true;
			this.FirstReportedDateEdit.AutoCompleteMonthThreshold = 1;
			this.FirstReportedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FirstReportedDateEdit, "HE_FirstReportedLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_FirstReportedLocal)));
			this.FirstReportedDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("6fd38bb2-84aa-44ab-929b-7747df0e0735", "First Reported");
			this.FirstReportedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FirstReportedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 51, true);
			this.FirstReportedDateEdit.Name = "FirstReportedDateEdit";
			this.FirstReportedDateEdit.TabIndex = 2;
			// 
			// ExceptionTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExceptionTypeTextBox, "HE_ExceptionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_ExceptionType)));
			this.ExceptionTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExceptionTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 51, true);
			this.ExceptionTypeTextBox.Name = "ExceptionTypeTextBox";
			this.ExceptionTypeTextBox.ReadOnly = true;
			this.ExceptionTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ExceptionTypeTextBox.TabIndex = 3;
			// 
			// ExceptionSourceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExceptionSourceTextBox, "HE_ExceptionSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_ExceptionSource)));
			this.ExceptionSourceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ExceptionSourceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 51, true);
			this.ExceptionSourceTextBox.Name = "ExceptionSourceTextBox";
			this.ExceptionSourceTextBox.ReadOnly = true;
			this.ExceptionSourceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.ExceptionSourceTextBox.TabIndex = 4;
			// 
			// FixedDateEdit
			// 
			this.FixedDateEdit.AllowDrop = true;
			this.FixedDateEdit.AutoCompleteMonthThreshold = 1;
			this.FixedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FixedDateEdit, "HE_FixedDateLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).HE_FixedDateLocal)));
			this.FixedDateEdit.CaptionResourceString = ZClientEDI.Res.GetData("4848b3a3-9bdf-4523-93ec-b947df4204f4", "Fixed Date");
			this.FixedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FixedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 103, true);
			this.FixedDateEdit.Name = "FixedDateEdit";
			this.FixedDateEdit.TabIndex = 12;
			// 
			// RelatedWorkItemsTabPage
			// 
			this.RelatedWorkItemsTabPage.Controls.Add(this.RelatedWorkItemGrid);
			this.RelatedWorkItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedWorkItemsTabPage.Name = "RelatedWorkItemsTabPage";
			this.RelatedWorkItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.RelatedWorkItemsTabPage.TabIndex = 3;
			this.RelatedWorkItemsTabPage.Text = "Related Work Items";
			// 
			// RelatedWorkItemGrid
			// 
			this.RelatedWorkItemGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedWorkItemGrid, "RelatedWorkItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).RelatedWorkItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).Lookups.WorkItems)));
			this.RelatedWorkItemGrid.BindToFindBoxList = "Lookups+WorkItems";
			zTextBoxColumnStyleInfo5.Caption = "Number";
			zTextBoxColumnStyleInfo5.ColumnName = "WKI_WorkItemNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "Description";
			zTextBoxColumnStyleInfo6.ColumnName = "WKI_Summary";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zTextBoxColumnStyleInfo7.Caption = "Status";
			zTextBoxColumnStyleInfo7.ColumnName = "WKI_Status";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "Disposition";
			zTextBoxColumnStyleInfo8.ColumnName = "DispositionDescription";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo9.Caption = "Assigned";
			zTextBoxColumnStyleInfo9.ColumnName = "AssignedToStaff+GS_Code";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedWorkItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RelatedWorkItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RelatedWorkItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.RelatedWorkItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.RelatedWorkItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.RelatedWorkItemGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedWorkItemGrid.GridId = "0eb99854-e6a7-4375-ad12-645d240eb4cc";
			// 
			// 
			// 
			this.RelatedWorkItemGrid.InnerGrid.AllowNavigation = false;
			this.RelatedWorkItemGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RelatedWorkItemGrid.InnerGrid.CaptionVisible = false;
			this.RelatedWorkItemGrid.InnerGrid.GridId = null;
			this.RelatedWorkItemGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedWorkItemGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.RelatedWorkItemGrid.InnerGrid.LayoutKey = "Grid";
			this.RelatedWorkItemGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RelatedWorkItemGrid.InnerGrid.Name = "Grid";
			this.RelatedWorkItemGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1267, 396, true);
			this.RelatedWorkItemGrid.InnerGrid.TabIndex = 0;
			this.RelatedWorkItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedWorkItemGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WorkItem;
			this.RelatedWorkItemGrid.Name = "RelatedWorkItemGrid";
			this.RelatedWorkItemGrid.NameOfAGridElement = ZClientEDI.Res.GetData("A18974E3-20A1-44DC-9995-428FABEB517C", "Work Item");
			this.RelatedWorkItemGrid.ReadOnly = false;
			this.RelatedWorkItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.RelatedWorkItemGrid.TabIndex = 1;
			// 
			// relatedIncidentsTabPage
			// 
			this.relatedIncidentsTabPage.Controls.Add(this.relatedIncidentsGrid);
			this.relatedIncidentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.relatedIncidentsTabPage.Name = "relatedIncidentsTabPage";
			this.relatedIncidentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.relatedIncidentsTabPage.TabIndex = 4;
			this.relatedIncidentsTabPage.Text = "Related Incidents";
			// 
			// relatedIncidentsGrid
			// 
			this.relatedIncidentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.relatedIncidentsGrid, "RelatedIncidents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).RelatedIncidents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).RelatedIncidents)).SyncRoot)).IM_IncidentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).RelatedIncidents)).SyncRoot)).IM_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).RelatedIncidents)).SyncRoot)).ClientCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.SupportIncident)(((System.Collections.IList)(((Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog)(null)).RelatedIncidents)).SyncRoot)).ClientName)));
			this.relatedIncidentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.Caption = "Incident Number";
			zTextBoxColumnStyleInfo10.ColumnName = "IM_IncidentNumber";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "Description";
			zTextBoxColumnStyleInfo11.ColumnName = "IM_Description";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(270);
			zTextBoxColumnStyleInfo12.Caption = "Client Code";
			zTextBoxColumnStyleInfo12.ColumnName = "ClientCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.Caption = "Client Name";
			zTextBoxColumnStyleInfo13.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.relatedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.relatedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.relatedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.relatedIncidentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.relatedIncidentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedIncidentsGrid.GridId = "d9a6f6c2-c66e-4d58-84d0-0879c351c4b9";
			this.relatedIncidentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.relatedIncidentsGrid.IsWholeRowSelectedOnClick = true;
			this.relatedIncidentsGrid.LayoutKey = "relatedIncidentsGrid";
			this.relatedIncidentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.relatedIncidentsGrid.Name = "relatedIncidentsGrid";
			this.relatedIncidentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.relatedIncidentsGrid.TabIndex = 2;
			this.relatedIncidentsGrid.DoubleClick += new System.EventHandler(this.RelatedIncidentsGrid_DoubleClick);
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1273, 434, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// ErrorLogForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1281, 513, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.SaveButtonPanel);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog);
			this.DataSourceTypeName = "Enterprise.Client.EDI.IssueManager.Business.EdiHelpErrorLog";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 393, true);
			this.Name = "ErrorLogForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveButtonPanel, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SaveButtonPanel.ResumeLayout(false);
			this.SaveButtonPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.MainTab.ResumeLayout(false);
			this.MainTab.PerformLayout();
			this.OccurrencesControl.ResumeLayout(true);
			this.OccurrencesControl.PerformLayout();
			this.oPanel1.ResumeLayout(false);
			this.oPanel1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.LatestExeDateEdit.ResumeLayout(true);
			this.LatestExeDateEdit.PerformLayout();
			this.LastReportedDateEdit.ResumeLayout(true);
			this.LastReportedDateEdit.PerformLayout();
			this.FirstReportedDateEdit.ResumeLayout(true);
			this.FirstReportedDateEdit.PerformLayout();
			this.FixedDateEdit.ResumeLayout(true);
			this.FixedDateEdit.PerformLayout();
			this.RelatedWorkItemsTabPage.ResumeLayout(false);
			this.RelatedWorkItemsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RelatedWorkItemGrid.InnerGrid)).EndInit();
			this.RelatedWorkItemGrid.ResumeLayout(true);
			this.RelatedWorkItemGrid.PerformLayout();
			this.relatedIncidentsTabPage.ResumeLayout(false);
			this.relatedIncidentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.relatedIncidentsGrid)).EndInit();
			this.relatedIncidentsGrid.ResumeLayout(false);
			this.relatedIncidentsGrid.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
