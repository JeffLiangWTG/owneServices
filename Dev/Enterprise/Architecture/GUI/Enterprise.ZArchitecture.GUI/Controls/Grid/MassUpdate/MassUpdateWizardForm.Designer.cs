namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	partial class MassUpdateWizardForm
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
		/// 
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MatchingSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MatchingFiltersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MachingFiltersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MatchingBizObjGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BizObjsToUpdateGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UpdateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ModifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NewValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FieldValueToUpdateUserControl = new Enterprise.ZArchitecture.GUI.ZUserControl();
			this.FieldToUpdateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NoFieldAvailableLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MatchingSplitContainer)).BeginInit();
			this.MatchingSplitContainer.Panel1.SuspendLayout();
			this.MatchingSplitContainer.Panel2.SuspendLayout();
			this.MatchingSplitContainer.SuspendLayout();
			this.MatchingFiltersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MachingFiltersGrid)).BeginInit();
			this.MachingFiltersGrid.SuspendLayout();
			this.MatchingBizObjGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BizObjsToUpdateGrid)).BeginInit();
			this.BizObjsToUpdateGrid.SuspendLayout();
			this.UpdateGroupBox.SuspendLayout();
			this.ModifierDropEdit.SuspendLayout();
			this.FieldToUpdateDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 515, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 24, true);
			this.MainStatusBar.TabIndex = 6;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.MassUpdateWizard);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.FindButton);
			this.BottomPanel.Controls.Add(this.UpdateButton);
			this.BottomPanel.Controls.Add(this.CloseButtonX);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 480, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 35, true);
			this.BottomPanel.TabIndex = 9;
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|1f4e70ef-4909-4433-a989-52a1a591f38b", "&Show Matched");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 6, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 24, true);
			this.FindButton.TabIndex = 5;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// UpdateButton
			// 
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|2917af5d-d68b-4430-9bea-dfc9dcd7179f", "&Update", "&Update", "");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(616, 6, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.UpdateButton.TabIndex = 6;
			this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
			// 
			// CloseButtonX
			// 
			this.CloseButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButtonX.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|917744a8-ebd3-4110-869e-fd4510165cc5", "&Close", "&Close", "");
			this.CloseButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 6, true);
			this.CloseButtonX.Name = "CloseButtonX";
			this.CloseButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.CloseButtonX.TabIndex = 7;
			this.CloseButtonX.Click += new System.EventHandler(this.CloseButtonX_Click);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.MainPanel);
			this.TopPanel.Controls.Add(this.NoFieldAvailableLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 480, true);
			this.TopPanel.TabIndex = 10;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MatchingSplitContainer);
			this.MainPanel.Controls.Add(this.UpdateGroupBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 480, true);
			this.MainPanel.TabIndex = 8;
			// 
			// MatchingSplitContainer
			// 
			this.MatchingSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 71, true);
			this.MatchingSplitContainer.Name = "MatchingSplitContainer";
			this.MatchingSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MatchingSplitContainer.Panel1
			// 
			this.MatchingSplitContainer.Panel1.Controls.Add(this.MatchingFiltersGroupBox);
			this.MatchingSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 409, true);
			this.MatchingSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			// 
			// MatchingSplitContainer.Panel2
			// 
			this.MatchingSplitContainer.Panel2.Controls.Add(this.MatchingBizObjGroupBox);
			this.MatchingSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.MatchingSplitContainer.TabIndex = 3;
			// 
			// MatchingFiltersGroupBox
			// 
			this.MatchingFiltersGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|9d2741dd-8124-4c8d-8a14-7ce5683837c3", "Matching Filters");
			this.MatchingFiltersGroupBox.Controls.Add(this.MachingFiltersGrid);
			this.MatchingFiltersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingFiltersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchingFiltersGroupBox.Name = "MatchingFiltersGroupBox";
			this.MatchingFiltersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 200, true);
			this.MatchingFiltersGroupBox.TabIndex = 0;
			this.MatchingFiltersGroupBox.TabStop = false;
			// 
			// MachingFiltersGrid
			// 
			this.MachingFiltersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MachingFiltersGrid, "MatchingFilters");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).MatchingFilters)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.MassUpdateMatchingFilter)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).MatchingFilters)).SyncRoot)).Field)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.MassUpdateMatchingFilter)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).MatchingFilters)).SyncRoot)).Operator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.MassUpdateMatchingFilter)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).MatchingFilters)).SyncRoot)).FieldValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.DataMapping.MassUpdateMatchingFilter)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).MatchingFilters)).SyncRoot)).FieldValueType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateMatchingFilter)(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).MatchingFilters)).SyncRoot)).FieldValueBindingList)));
			this.MachingFiltersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|58029dbb-ceea-4af2-973c-fad742bd45ce", "Field");
			zDropEditColumnStyleInfo1.ColumnName = "Field";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|29f05d60-ad29-4f2f-aa09-7e2cb85f184f", "Operator");
			zDropEditColumnStyleInfo2.ColumnName = "Operator";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.BindToList = "FieldValueBindingList";
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|ac1a0135-62f8-4238-b0e3-93c42923980f", "Value");
			zMultiControlColumnStyleInfo1.ColumnName = "FieldValue";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "FieldValueType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.MachingFiltersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MachingFiltersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MachingFiltersGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.MachingFiltersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MachingFiltersGrid.GridId = "6f3e4259-0602-4f0e-9a2f-1644e00e48cd";
			this.MachingFiltersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MachingFiltersGrid.LayoutKey = "MachingFiltersGrid";
			this.MachingFiltersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MachingFiltersGrid.Name = "MachingFiltersGrid";
			this.MachingFiltersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 181, true);
			this.MachingFiltersGrid.TabIndex = 0;
			// 
			// MatchingBizObjGroupBox
			// 
			this.MatchingBizObjGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|76831169-bf0b-4261-a908-9aa9bc6cdcc8", "Matched Records");
			this.MatchingBizObjGroupBox.Controls.Add(this.BizObjsToUpdateGrid);
			this.MatchingBizObjGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingBizObjGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MatchingBizObjGroupBox.Name = "MatchingBizObjGroupBox";
			this.MatchingBizObjGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 205, true);
			this.MatchingBizObjGroupBox.TabIndex = 0;
			this.MatchingBizObjGroupBox.TabStop = false;
			// 
			// BizObjsToUpdateGrid
			// 
			this.BizObjsToUpdateGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BizObjsToUpdateGrid, "BizObjsToUpdate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).BizObjsToUpdate)));
			this.BizObjsToUpdateGrid.CaptionVisible = false;
			this.BizObjsToUpdateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BizObjsToUpdateGrid.GridId = "a758633a-436a-47cb-9c53-0482486c3471";
			this.BizObjsToUpdateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BizObjsToUpdateGrid.LayoutKey = "BizObjsToUpdateGrid";
			this.BizObjsToUpdateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BizObjsToUpdateGrid.Name = "BizObjsToUpdateGrid";
			this.BizObjsToUpdateGrid.ReadOnly = true;
			this.BizObjsToUpdateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(908, 186, true);
			this.BizObjsToUpdateGrid.TabIndex = 0;
			// 
			// UpdateGroupBox
			// 
			this.UpdateGroupBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|eff0a4c0-e87f-481b-bfb2-18757e7e2327", "Update");
			this.UpdateGroupBox.Controls.Add(this.ModifierDropEdit);
			this.UpdateGroupBox.Controls.Add(this.NewValueLabel);
			this.UpdateGroupBox.Controls.Add(this.FieldValueToUpdateUserControl);
			this.UpdateGroupBox.Controls.Add(this.FieldToUpdateDropEdit);
			this.UpdateGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.UpdateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UpdateGroupBox.Name = "UpdateGroupBox";
			this.UpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 71, true);
			this.UpdateGroupBox.TabIndex = 2;
			this.UpdateGroupBox.TabStop = false;
			// 
			// ModifierDropEdit
			// 
			this.ModifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModifierDropEdit, "Modifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).Modifier)));
			this.ModifierDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("bd142c09-c7ff-490a-9b14-f751536ab9bc", "Modifier");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ModifierDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ModifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 33, true);
			this.ModifierDropEdit.Name = "ModifierDropEdit";
			this.ModifierDropEdit.ShowDescriptionBox = false;
			this.ModifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ModifierDropEdit.TabIndex = 1;
			this.ModifierDropEdit.UseFullWidthForCodeBox = true;
			// 
			// NewValueLabel
			// 
			this.NewValueLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.NewValueLabel.AutoSize = true;
			this.NewValueLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|e1483654-980a-459a-9ff6-5fc19f831865", "Value");
			this.NewValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 16, true);
			this.NewValueLabel.Name = "NewValueLabel";
			this.NewValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 13, true);
			this.NewValueLabel.TabIndex = 2;
			// 
			// FieldValueToUpdateUserControl
			// 
			this.FieldValueToUpdateUserControl.AllowDrop = true;
			this.FieldValueToUpdateUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FieldValueToUpdateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 33, true);
			this.FieldValueToUpdateUserControl.Name = "FieldValueToUpdateUserControl";
			this.FieldValueToUpdateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.FieldValueToUpdateUserControl.TabIndex = 3;
			// 
			// FieldToUpdateDropEdit
			// 
			this.FieldToUpdateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FieldToUpdateDropEdit, "Field");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.DataMapping.MassUpdateWizard)(null)).Field)));
			this.FieldToUpdateDropEdit.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|35ae02d5-a662-46c1-819a-8cceec5fc34b", "Field", "Field", "");
			this.FieldToUpdateDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FieldToUpdateDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FieldToUpdateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 33, true);
			this.FieldToUpdateDropEdit.Name = "FieldToUpdateDropEdit";
			this.FieldToUpdateDropEdit.PreBoundMaxLength = 40;
			this.FieldToUpdateDropEdit.ShowDescriptionBox = false;
			this.FieldToUpdateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 20, true);
			this.FieldToUpdateDropEdit.TabIndex = 0;
			// 
			// NoFieldAvailableLabel
			// 
			this.NoFieldAvailableLabel.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|596f2d5d-dd13-4f75-bcef-ea334a62c680", "", "There are no field available for updating as all fields are read-only.");
			this.NoFieldAvailableLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NoFieldAvailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NoFieldAvailableLabel.Name = "NoFieldAvailableLabel";
			this.NoFieldAvailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 480, true);
			this.NoFieldAvailableLabel.TabIndex = 9;
			this.NoFieldAvailableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MassUpdateWizardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("MassUpdateWizardForm|5202f683-0772-40f5-b779-6cbf2cbb6a03", "Mass Update");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 539, true);
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.DataMapping.MassUpdateWizard);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.DataMapping.MassUpdateWizard";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(930, 573, true);
			this.Name = "MassUpdateWizardForm";
			this.ShouldSerializeTabPageMethods = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.MatchingSplitContainer.Panel1.ResumeLayout(false);
			this.MatchingSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MatchingSplitContainer)).EndInit();
			this.MatchingSplitContainer.ResumeLayout(false);
			this.MatchingSplitContainer.PerformLayout();
			this.MatchingFiltersGroupBox.ResumeLayout(false);
			this.MatchingFiltersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MachingFiltersGrid)).EndInit();
			this.MachingFiltersGrid.ResumeLayout(false);
			this.MachingFiltersGrid.PerformLayout();
			this.MatchingBizObjGroupBox.ResumeLayout(false);
			this.MatchingBizObjGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BizObjsToUpdateGrid)).EndInit();
			this.BizObjsToUpdateGrid.ResumeLayout(false);
			this.BizObjsToUpdateGrid.PerformLayout();
			this.UpdateGroupBox.ResumeLayout(false);
			this.UpdateGroupBox.PerformLayout();
			this.ModifierDropEdit.ResumeLayout(true);
			this.ModifierDropEdit.PerformLayout();
			this.FieldToUpdateDropEdit.ResumeLayout(true);
			this.FieldToUpdateDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel BottomPanel;
		private ZButton FindButton;
		private ZButton UpdateButton;
		private ZButton CloseButtonX;
		private ZPanel TopPanel;
		private ZLabel NoFieldAvailableLabel;
		private ZPanel MainPanel;
		private ZGroupBox UpdateGroupBox;
		private ZLabel NewValueLabel;
		private ZUserControl FieldValueToUpdateUserControl;
		private ZDropEdit FieldToUpdateDropEdit;
		private CargoWise.Windows.UI.KSplitContainer MatchingSplitContainer;
		private ZGroupBox MatchingFiltersGroupBox;
		private ZGrid MachingFiltersGrid;
		private ZGroupBox MatchingBizObjGroupBox;
		private ZGrid BizObjsToUpdateGrid;
		private ZDropEdit ModifierDropEdit;
	}
}
