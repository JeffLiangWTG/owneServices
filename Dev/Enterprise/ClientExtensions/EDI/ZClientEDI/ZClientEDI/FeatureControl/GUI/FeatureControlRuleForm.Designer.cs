using System;
using System.Drawing;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	partial class FeatureControlRuleForm
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RuleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParameterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PopupButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ParameterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UseGlobalConfigCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BasicInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GlobalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FeatureSetCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EnableUntilDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EnableFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FeatureSetDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.LicenseDatabasesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DatabaseModuleButtonGrid = new Enterprise.Client.EDI.FeatureControl.GUI.FeatureControlRuleLicenceDatabaseModuleButtonGrid();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.RuleDetailsGroupBox.SuspendLayout();
			this.ParameterGroupBox.SuspendLayout();
			this.BasicInfoGroupBox.SuspendLayout();
			this.EnableUntilDateEdit.SuspendLayout();
			this.EnableFromDateEdit.SuspendLayout();
			this.FeatureSetDropEdit.SuspendLayout();
			this.LicenseDatabasesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabaseModuleButtonGrid.InnerGrid)).BeginInit();
			this.DatabaseModuleButtonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 507, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.MainSplitContainer);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 485, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 485, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 485, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 507, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 24, true);
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.RuleDetailsGroupBox);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.LicenseDatabasesGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 485, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(395);
			this.MainSplitContainer.SplitterWidth = 42;
			this.MainSplitContainer.TabIndex = 2;
			// 
			// RuleDetailsGroupBox
			// 
			this.RuleDetailsGroupBox.Controls.Add(this.ParameterGroupBox);
			this.RuleDetailsGroupBox.Controls.Add(this.BasicInfoGroupBox);
			this.RuleDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RuleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RuleDetailsGroupBox.Name = "RuleDetailsGroupBox";
			this.RuleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 485, true);
			this.RuleDetailsGroupBox.TabIndex = 1;
			this.RuleDetailsGroupBox.TabStop = false;
			this.RuleDetailsGroupBox.Text = "Rule Details";
			// 
			// ParameterGroupBox
			// 
			this.ParameterGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ParameterGroupBox.Controls.Add(this.PopupButton);
			this.ParameterGroupBox.Controls.Add(this.ParameterTextBox);
			this.ParameterGroupBox.Controls.Add(this.UseGlobalConfigCheckBox);
			this.ParameterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 158, true);
			this.ParameterGroupBox.Name = "ParameterGroupBox";
			this.ParameterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 321, true);
			this.ParameterGroupBox.TabIndex = 20;
			this.ParameterGroupBox.TabStop = false;
			this.ParameterGroupBox.Text = "Parameters";
			// 
			// PopupButton
			// 
			this.PopupButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.PopupButton.IsCaptionOverridden = true;
			this.PopupButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 11, true);
			this.PopupButton.Name = "PopupButton";
			this.PopupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.PopupButton.TabIndex = 21;
			this.PopupButton.Text = "Pop up";
			this.PopupButton.ToolTipCaption = null;
			this.PopupButton.Click += new System.EventHandler(this.PopupButton_Click);
			// 
			// ParameterTextBox
			// 
			this.ParameterTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ParameterTextBox, "FCR_Parameters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_Parameters)));
			this.ParameterTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ParameterTextBox, false);
			this.ParameterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 35, true);
			this.ParameterTextBox.Multiline = true;
			this.ParameterTextBox.Name = "ParameterTextBox";
			this.ParameterTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ParameterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(365, 257, true);
			this.ParameterTextBox.TabIndex = 22;
			// 
			// UseGlobalConfigCheckBox
			// 
			this.UseGlobalConfigCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UseGlobalConfigCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseGlobalConfigCheckBox, "FCR_UseGlobalParameters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_UseGlobalParameters)));
			this.UseGlobalConfigCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UseGlobalConfigCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 299, true);
			this.UseGlobalConfigCheckBox.Name = "UseGlobalConfigCheckBox";
			this.UseGlobalConfigCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 16, true);
			this.UseGlobalConfigCheckBox.TabIndex = 23;
			this.UseGlobalConfigCheckBox.Text = "Use global parameters";
			this.UseGlobalConfigCheckBox.UseVisualStyleBackColor = true;
			// 
			// BasicInfoGroupBox
			// 
			this.BasicInfoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BasicInfoGroupBox.Controls.Add(this.GlobalCheckBox);
			this.BasicInfoGroupBox.Controls.Add(this.FeatureSetCheckBox);
			this.BasicInfoGroupBox.Controls.Add(this.DescriptionTextBox);
			this.BasicInfoGroupBox.Controls.Add(this.ActiveCheckBox);
			this.BasicInfoGroupBox.Controls.Add(this.EnableUntilDateEdit);
			this.BasicInfoGroupBox.Controls.Add(this.EnableFromDateEdit);
			this.BasicInfoGroupBox.Controls.Add(this.FeatureSetDropEdit);
			this.BasicInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.BasicInfoGroupBox.Name = "BasicInfoGroupBox";
			this.BasicInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 133, true);
			this.BasicInfoGroupBox.TabIndex = 10;
			this.BasicInfoGroupBox.TabStop = false;
			// 
			// GlobalCheckBox
			// 
			this.GlobalCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GlobalCheckBox, "IsGlobalRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).IsGlobalRule)));
			this.GlobalCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.GlobalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 49, true);
			this.GlobalCheckBox.Name = "GlobalCheckBox";
			this.GlobalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 16, true);
			this.GlobalCheckBox.TabIndex = 13;
			this.GlobalCheckBox.Text = "Global";
			this.GlobalCheckBox.UseVisualStyleBackColor = true;
			//this.GlobalCheckBox.CheckStateChanged += new System.EventHandler(this.GlobalCheckBox_CheckStateChanged);
			// 
			// FeatureSetCheckBox
			// 
			this.FeatureSetCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FeatureSetCheckBox, "IsFeatureSetRule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).IsFeatureSetRule)));
			this.FeatureSetCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FeatureSetCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 49, true);
			this.FeatureSetCheckBox.Name = "FeatureSetCheckBox";
			this.FeatureSetCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.FeatureSetCheckBox.TabIndex = 14;
			this.FeatureSetCheckBox.Text = "Feature Set";
			this.FeatureSetCheckBox.UseVisualStyleBackColor = true;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FCR_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 20, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 17, true);
			this.DescriptionTextBox.TabIndex = 11;
			// 
			// ActiveCheckBox
			// 
			this.ActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ActiveCheckBox, "FCR_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_IsActive)));
			this.ActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 49, true);
			this.ActiveCheckBox.Name = "ActiveCheckBox";
			this.ActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 16, true);
			this.ActiveCheckBox.TabIndex = 12;
			this.ActiveCheckBox.Text = "Active";
			this.ActiveCheckBox.UseVisualStyleBackColor = true;
			//this.ActiveCheckBox.CheckStateChanged += new System.EventHandler(this.ActiveCheckBox_CheckStateChanged);
			// 
			// EnableUntilDateEdit
			// 
			this.EnableUntilDateEdit.AllowDrop = true;
			this.EnableUntilDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EnableUntilDateEdit, "FCR_EndDateUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_EndDateUtc)));
			this.EnableUntilDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 97, true);
			this.EnableUntilDateEdit.Name = "EnableUntilDateEdit";
			this.EnableUntilDateEdit.TabIndex = 16;
			// 
			// EnableFromDateEdit
			// 
			this.EnableFromDateEdit.AllowDrop = true;
			this.EnableFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EnableFromDateEdit, "FCR_StartDateUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_StartDateUtc)));
			this.EnableFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 97, true);
			this.EnableFromDateEdit.Name = "EnableFromDateEdit";
			this.EnableFromDateEdit.TabIndex = 15;
			// 
			// FeatureSetDropEdit
			// 
			this.FeatureSetDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FeatureSetDropEdit, "FCR_FCS_FeatureSet");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).FCR_FCS_FeatureSet)));
			this.FeatureSetDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FeatureSetDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 71, true);
			this.FeatureSetDropEdit.Name = "FeatureSetDropEdit";
			this.FeatureSetDropEdit.PreBoundMaxLength = 40;
			this.FeatureSetDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FeatureSetDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 17, true);
			this.FeatureSetDropEdit.TabIndex = 14;
			// 
			// LicenseDatabasesGroupBox
			// 
			this.LicenseDatabasesGroupBox.Controls.Add(this.DatabaseModuleButtonGrid);
			this.LicenseDatabasesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicenseDatabasesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LicenseDatabasesGroupBox.Name = "LicenseDatabasesGroupBox";
			this.LicenseDatabasesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(357, 485, true);
			this.LicenseDatabasesGroupBox.TabIndex = 50;
			this.LicenseDatabasesGroupBox.TabStop = false;
			this.LicenseDatabasesGroupBox.Text = "License Databases";
			// 
			// DatabaseModuleButtonGrid
			// 
			this.DatabaseModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DatabaseModuleButtonGrid, "LicenceDatabasePivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).LicenceDatabasePivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(null)).Lookups.LicenceDatabaseNotLinked)));
			this.DatabaseModuleButtonGrid.BindToFindBoxList = "Lookups+LicenceDatabaseNotLinked";
			zTextBoxColumnStyleInfo1.ColumnName = "Database+LD_DatabaseNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Database+EDIWebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "Database+EDIWebAccessOrg+OH_FullName";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo4.ColumnName = "Database+LD_ReleaseRing";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "Database+LD_LicenceType";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "Database+LD_ServerCode";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DatabaseModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DatabaseModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DatabaseModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DatabaseModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DatabaseModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DatabaseModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DatabaseModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabaseModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.DatabaseModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.DatabaseModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.DatabaseModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DatabaseModuleButtonGrid.InnerGrid.GridId = null;
			this.DatabaseModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DatabaseModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.DatabaseModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DatabaseModuleButtonGrid.InnerGrid.Name = "Grid";
			this.DatabaseModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 432, true);
			this.DatabaseModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.DatabaseModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.DatabaseModuleButtonGrid.Name = "DatabaseModuleButtonGrid";
			this.DatabaseModuleButtonGrid.ReadOnly = false;
			this.DatabaseModuleButtonGrid.ShowNewButton = false;
			this.DatabaseModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 468, true);
			this.DatabaseModuleButtonGrid.TabIndex = 51;
			// 
			// FeatureControlRuleForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 563, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule);
			this.DataSourceTypeName = "Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			this.Name = "FeatureControlRuleForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "Control Rule";
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
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.RuleDetailsGroupBox.ResumeLayout(false);
			this.RuleDetailsGroupBox.PerformLayout();
			this.ParameterGroupBox.ResumeLayout(false);
			this.ParameterGroupBox.PerformLayout();
			this.BasicInfoGroupBox.ResumeLayout(false);
			this.BasicInfoGroupBox.PerformLayout();
			this.EnableUntilDateEdit.ResumeLayout(true);
			this.EnableUntilDateEdit.PerformLayout();
			this.EnableFromDateEdit.ResumeLayout(true);
			this.EnableFromDateEdit.PerformLayout();
			this.FeatureSetDropEdit.ResumeLayout(true);
			this.FeatureSetDropEdit.PerformLayout();
			this.LicenseDatabasesGroupBox.ResumeLayout(false);
			this.LicenseDatabasesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DatabaseModuleButtonGrid.InnerGrid)).EndInit();
			this.DatabaseModuleButtonGrid.ResumeLayout(true);
			this.DatabaseModuleButtonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox RuleDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox ParameterGroupBox;
		private ZArchitecture.GUI.ZButton PopupButton;
		internal ZArchitecture.ZTextBox ParameterTextBox;
		private ZArchitecture.GUI.ZCheckBox UseGlobalConfigCheckBox;
		private ZArchitecture.GUI.ZGroupBox BasicInfoGroupBox;
		private ZArchitecture.GUI.ZCheckBox GlobalCheckBox;
		private ZArchitecture.GUI.ZCheckBox FeatureSetCheckBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZArchitecture.GUI.ZCheckBox ActiveCheckBox;
		private ZArchitecture.GUI.ZDateEdit EnableUntilDateEdit;
		private ZArchitecture.GUI.ZDateEdit EnableFromDateEdit;
		private ZArchitecture.GUI.ZGroupBox LicenseDatabasesGroupBox;
		private ZArchitecture.GUI.ZGuidDropEdit FeatureSetDropEdit;
		private FeatureControlRuleLicenceDatabaseModuleButtonGrid DatabaseModuleButtonGrid;
	}
}
