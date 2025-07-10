using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;


namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	partial class FeatureControlForm
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
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 536, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 509, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 509, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 509, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 536, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FCM_FeatureControlCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FCM_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FCM_GG_ReleaseGroup)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).Status)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FCM_WKI_ActiveWorkItem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FCM_WKI_DeactivateWorkItem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)).SyncRoot)).RuleTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)).SyncRoot)).FCR_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)).SyncRoot)).FCR_StartDateUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)).SyncRoot)).FCR_EndDateUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)).SyncRoot)).FCR_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlRule)(((System.Collections.IList)(((Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader)(null)).FeatureControlRules)).SyncRoot)).DatabaseCount)));
			// 
			// FeatureControlForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 592, true);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader);
			this.DataSourceTypeName = "Enterprise.Client.EDI.FeatureControl.Business.FeatureControlHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 458, true);
			this.Name = "FeatureControlForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Feature Control Centre";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TopSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.InfoGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.InfoGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ActiveWorkItemGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DeactivateWorkItemGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NewRuleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EditRuleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeleteRuleButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).BeginInit();
			this.TopSplitContainer.Panel1.SuspendLayout();
			this.TopSplitContainer.Panel2.SuspendLayout();
			this.TopSplitContainer.SuspendLayout();
			this.InfoGroupBox1.SuspendLayout();
			this.ReleaseGroupFindBox.SuspendLayout();
			this.InfoGroupBox2.SuspendLayout();
			this.ActiveWorkItemGuidFindBox.SuspendLayout();
			this.DeactivateWorkItemGuidFindBox.SuspendLayout();
			this.RulesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).BeginInit();
			this.RulesGrid.SuspendLayout();
			this.MainTabPage.Controls.Add(this.MainSplitContainer);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.IsSplitterFixed = true;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.TopSplitContainer);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.RulesGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 509, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(126);
			this.MainSplitContainer.SplitterWidth = 8;
			this.MainSplitContainer.TabIndex = 1;
			// 
			// TopSplitContainer
			// 
			this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopSplitContainer.Name = "TopSplitContainer";
			// 
			// TopSplitContainer.Panel1
			// 
			this.TopSplitContainer.Panel1.Controls.Add(this.InfoGroupBox1);
			// 
			// TopSplitContainer.Panel2
			// 
			this.TopSplitContainer.Panel2.Controls.Add(this.InfoGroupBox2);
			this.TopSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 126, true);
			this.TopSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(473);
			this.TopSplitContainer.SplitterWidth = 8;
			this.TopSplitContainer.TabIndex = 2;
			// 
			// InfoGroupBox1
			// 
			this.InfoGroupBox1.Controls.Add(this.ReleaseGroupFindBox);
			this.InfoGroupBox1.Controls.Add(this.DescriptionTextBox);
			this.InfoGroupBox1.Controls.Add(this.CodeTextBox);
			this.InfoGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InfoGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InfoGroupBox1.Name = "InfoGroupBox1";
			this.InfoGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 126, true);
			this.InfoGroupBox1.TabIndex = 10;
			this.InfoGroupBox1.TabStop = false;
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "FCM_FeatureControlCode");
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 20, true);
			this.CodeTextBox.TabIndex = 11;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FCM_Description");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 54, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 20, true);
			this.DescriptionTextBox.TabIndex = 12;
			// 
			// ReleaseGroupFindBox
			// 
			this.ReleaseGroupFindBox.AllowDrop = true;
			this.ReleaseGroupFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseGroupFindBox, "FCM_GG_ReleaseGroup");
			this.ReleaseGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 89, true);
			this.ReleaseGroupFindBox.Name = "ReleaseGroupFindBox";
			this.ReleaseGroupFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ReleaseGroupFindBox.ParentType = null;
			this.ReleaseGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 20, true);
			this.ReleaseGroupFindBox.TabIndex = 13;
			// 
			// InfoGroupBox2
			// 
			this.InfoGroupBox2.Controls.Add(this.DeactivateWorkItemGuidFindBox);
			this.InfoGroupBox2.Controls.Add(this.StatusTextBox);
			this.InfoGroupBox2.Controls.Add(this.ActiveWorkItemGuidFindBox);
			this.InfoGroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InfoGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InfoGroupBox2.Name = "InfoGroupBox2";
			this.InfoGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 126, true);
			this.InfoGroupBox2.TabIndex = 20;
			this.InfoGroupBox2.TabStop = false;
			// 
			// StatusTextBox
			// 
			this.StatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusTextBox, "Status");
			this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 19, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.StatusTextBox.TabIndex = 21;
			// 
			// ActiveWorkItemGuidFindBox
			// 
			this.ActiveWorkItemGuidFindBox.AllowDrop = true;
			this.ActiveWorkItemGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ActiveWorkItemGuidFindBox, "FCM_WKI_ActiveWorkItem");
			this.ActiveWorkItemGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 54, true);
			this.ActiveWorkItemGuidFindBox.Name = "ActiveWorkItemGuidFindBox";
			this.ActiveWorkItemGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ActiveWorkItemGuidFindBox.ParentType = null;
			this.ActiveWorkItemGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.ActiveWorkItemGuidFindBox.TabIndex = 22;
			// 
			// DeactivateWorkItemGuidFindBox
			// 
			this.DeactivateWorkItemGuidFindBox.AllowDrop = true;
			this.DeactivateWorkItemGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DeactivateWorkItemGuidFindBox, "FCM_WKI_DeactivateWorkItem");
			this.DeactivateWorkItemGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 89, true);
			this.DeactivateWorkItemGuidFindBox.Name = "DeactivateWorkItemGuidFindBox";
			this.DeactivateWorkItemGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DeactivateWorkItemGuidFindBox.ParentType = null;
			this.DeactivateWorkItemGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.DeactivateWorkItemGuidFindBox.TabIndex = 23;
			// 
			// RulesGroupBox
			//
			this.RulesGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("5f304023-743c-4c3e-bcf3-f1301b7e907c", "Control Rules");
			this.RulesGroupBox.Controls.Add(this.DeleteRuleButton);
			this.RulesGroupBox.Controls.Add(this.EditRuleButton);
			this.RulesGroupBox.Controls.Add(this.NewRuleButton);
			this.RulesGroupBox.Controls.Add(this.RulesGrid);
			this.RulesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RulesGroupBox.Name = "RulesGroupBox";
			this.RulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 354, true);
			this.RulesGroupBox.TabIndex = 21;
			this.RulesGroupBox.TabStop = false;
			// 
			// RulesGrid
			// 
			this.RulesGrid.AllowNavigation = false;
			this.RulesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RulesGrid, "FeatureControlRules");
			this.RulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "RuleTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "FCR_Description";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.ColumnName = "FeatureSet+FCS_ProductName";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.Caption = "Feature Set";
			zDateEditColumnStyleInfo1.ColumnName = "FCR_StartDateUtc";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "FCR_EndDateUtc";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "FCR_IsActive";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "DatabaseCount";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.RulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RulesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RulesGrid.GridId = "5d960fc9-5181-4dde-8ed9-0985c14ab218";
			this.RulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RulesGrid.LayoutKey = "RulesGrid";
			this.RulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RulesGrid.Name = "RulesGrid";
			this.RulesGrid.ReadOnly = true;
			this.RulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 323, true);
			this.RulesGrid.TabIndex = 22;
			this.RulesGrid.DoubleClick += new System.EventHandler(this.RulesGrid_DoubleClick);
			// 
			// NewRuleButton
			// 
			this.NewRuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.NewRuleButton.CaptionResourceString = ZClientEDI.Res.GetData("f3f478a3-51a3-4978-9d14-f854668f5244", "New");
			this.NewRuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(751, 345, true);
			this.NewRuleButton.Name = "NewRuleButton";
			this.NewRuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.NewRuleButton.TabIndex = 23;
			this.NewRuleButton.ToolTipCaption = null;
			this.NewRuleButton.Click += new System.EventHandler(this.NewRuleButton_Click);
			// 
			// EditRuleButton
			// 
			this.EditRuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.EditRuleButton.CaptionResourceString = ZClientEDI.Res.GetData("c74b0c3c-851d-42ae-bdec-37624b8cb708", "Edit");
			this.EditRuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(839, 345, true);
			this.EditRuleButton.Name = "EditRuleButton";
			this.EditRuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.EditRuleButton.TabIndex = 24;
			this.EditRuleButton.ToolTipCaption = null;
			this.EditRuleButton.Click += new System.EventHandler(this.EditRuleButton_Click);
			// 
			// DeleteRuleButton
			// 
			this.DeleteRuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteRuleButton.CaptionResourceString = ZClientEDI.Res.GetData("13130041-51e6-4345-9805-9aebea7b434f", "Delete");
			this.DeleteRuleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(927, 345, true);
			this.DeleteRuleButton.Name = "DeleteRuleButton";
			this.DeleteRuleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.DeleteRuleButton.TabIndex = 25;
			this.DeleteRuleButton.ToolTipCaption = null;
			this.DeleteRuleButton.Click += new System.EventHandler(this.DeleteRuleButton_Click);
			this.MainTabPage.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.TopSplitContainer.Panel1.ResumeLayout(false);
			this.TopSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).EndInit();
			this.TopSplitContainer.ResumeLayout(false);
			this.TopSplitContainer.PerformLayout();
			this.InfoGroupBox1.ResumeLayout(false);
			this.InfoGroupBox1.PerformLayout();
			this.ReleaseGroupFindBox.ResumeLayout(true);
			this.ReleaseGroupFindBox.PerformLayout();
			this.InfoGroupBox2.ResumeLayout(false);
			this.InfoGroupBox2.PerformLayout();
			this.ActiveWorkItemGuidFindBox.ResumeLayout(true);
			this.ActiveWorkItemGuidFindBox.PerformLayout();
			this.DeactivateWorkItemGuidFindBox.ResumeLayout(true);
			this.DeactivateWorkItemGuidFindBox.PerformLayout();
			this.RulesGroupBox.ResumeLayout(false);
			this.RulesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RulesGrid)).EndInit();
			this.RulesGrid.ResumeLayout(false);
			this.RulesGrid.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}
		void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		#endregion
		private KTableLayoutPanel tableLayoutPanel;
		private KSplitContainer MainSplitContainer;
		private KSplitContainer TopSplitContainer;
		private ZGroupBox InfoGroupBox1;
		private ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZGroupBox InfoGroupBox2;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZGuidFindBox ActiveWorkItemGuidFindBox;
		private ZGuidFindBox DeactivateWorkItemGuidFindBox;
		private ZGroupBox RulesGroupBox;
		private ZArchitecture.ZGrid RulesGrid;
		private ZButton DeleteRuleButton;
		private ZButton EditRuleButton;
		private ZButton NewRuleButton;
		private ZGuidFindBox ReleaseGroupFindBox;
	}
}
