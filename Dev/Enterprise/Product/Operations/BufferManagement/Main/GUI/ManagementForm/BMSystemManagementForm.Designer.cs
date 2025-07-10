using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class BMSystemManagementForm
	{
		new void InitializeComponent()
		{
			this.ConfigTabPage = new ZTabPage();
			this.ConfigurationUserControl = new ConfigurationUserControl();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DetailsGroupBox = new ZGroupBox();
			this.SystemIsLiveToggleButton = new ZButton();
			this.ExperimentalSettingsButton = new ZButton();
			this.UpdateRelatedWorkflowsButton = new ZButton();
			this.IsLiveTextBox = new ZTextBox();
			this.ResourceCountdownTimeEdit = new ZTimeEditEx();
			this.NameTextBox = new ZTextBox();
			this.DescriptionTextBox = new ZTextBox();
			this.ComponentsUserControl = new BMComponentsUserControl();
			this.CustomisedVisualLayoutsTabPage = new ZTabPage();
			this.CustomisedVisualLayoutsControl = new CustomisedVisualLayoutsUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigTabPage.SuspendLayout();
			this.ConfigurationUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ComponentsUserControl.SuspendLayout();
			this.CustomisedVisualLayoutsTabPage.SuspendLayout();
			this.CustomisedVisualLayoutsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.ConfigTabPage);
			this.MainTabControl.Controls.Add(this.CustomisedVisualLayoutsTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 563, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomisedVisualLayoutsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ConfigTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.BackColor = System.Drawing.Color.White;
			this.MainTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ZTemplateForm|2c2a9dbb-297a-4aa3-9ef1-5e9d82b282ff", "System Schematic");
			this.MainTabPage.Controls.Add(this.ComponentsUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MainSplitContainer);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 631, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(768);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BMSystem);
			// 
			// ConfigTabPage
			// 
			this.ConfigTabPage.BackColor = System.Drawing.Color.White;
			this.ConfigTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("a8319ea2-568e-4533-b2ed-73c81c86038d", "Configuration");
			this.ConfigTabPage.Controls.Add(this.ConfigurationUserControl);
			this.ConfigTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ConfigTabPage.Name = "ConfigTabPage";
			this.ConfigTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ConfigTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			this.ConfigTabPage.TabIndex = 3;
			// 
			// ConfigurationUserControl
			// 
			this.ConfigurationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConfigurationUserControl, ".");
			this.ConfigurationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConfigurationUserControl.Name = "ConfigurationUserControl";
			this.ConfigurationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 530, true);
			this.ConfigurationUserControl.TabIndex = 0;
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.IsSplitterFixed = true;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.DetailsGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 631, true);
			this.MainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(66);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.MainTabControl);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(66);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("4b91a6e1-fb45-404d-9d53-cb3d35217111", "Details");
			this.DetailsGroupBox.Controls.Add(this.SystemIsLiveToggleButton);
			this.DetailsGroupBox.Controls.Add(this.ExperimentalSettingsButton);
			this.DetailsGroupBox.Controls.Add(this.UpdateRelatedWorkflowsButton);
			this.DetailsGroupBox.Controls.Add(this.IsLiveTextBox);
			this.DetailsGroupBox.Controls.Add(this.ResourceCountdownTimeEdit);
			this.DetailsGroupBox.Controls.Add(this.NameTextBox);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 66, true);
			this.DetailsGroupBox.TabIndex = 2;
			this.DetailsGroupBox.TabStop = false;
			this.ExperimentalSettingsButton.Click += new EventHandler(this.ExperimentalSettingsButton_Click);
			this.UpdateRelatedWorkflowsButton.Click += new EventHandler(this.UpdateRelatedWorkflowsButton_Click);
			//
			// SystemIsLiveToggleButton
			// 
			this.SystemIsLiveToggleButton.AutoSize = true;
			this.SystemIsLiveToggleButton.IsCaptionOverridden = false;
			this.SystemIsLiveToggleButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 12, true);
			this.SystemIsLiveToggleButton.Name = "SystemIsLiveToggleButton";
			this.SystemIsLiveToggleButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SystemIsLiveToggleButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 26, true);
			this.SystemIsLiveToggleButton.TabIndex = 4;
			this.SystemIsLiveToggleButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SystemIsLiveToggleButton.ToolTipCaption = null;
			this.SystemIsLiveToggleButton.UseVisualStyleBackColor = true;
			// 
			// ExperimentalSettingsButton
			// 
			this.ExperimentalSettingsButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("368EAF84-2099-475B-92BF-F862D672ADCD", "Experimental Settings");
			this.ExperimentalSettingsButton.AutoSize = true;
			this.ExperimentalSettingsButton.IsCaptionOverridden = false;
			this.ExperimentalSettingsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(695, 12, true);
			this.ExperimentalSettingsButton.Name = "ExperimentalSettingsButton";
			this.ExperimentalSettingsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ExperimentalSettingsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 26, true);
			this.ExperimentalSettingsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ExperimentalSettingsButton.ToolTipCaption = null;
			this.ExperimentalSettingsButton.UseVisualStyleBackColor = true;
			this.ExperimentalSettingsButton.Visible = BMSRegistry.Instance.EnablePaveExperimentalFeatures.Value && Environment.Env.Security.BMBoardEdit.IsAllowed;
			// 
			// UpdateRelatedWorkflowsButton
			// 
			this.UpdateRelatedWorkflowsButton.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("0ED3CA92-EEFF-44AC-AE68-5B7DF5E82EED", "Force Update Related Workflows");
			this.UpdateRelatedWorkflowsButton.AutoSize = true;
			this.UpdateRelatedWorkflowsButton.IsCaptionOverridden = false;
			this.UpdateRelatedWorkflowsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(828, 12, true);
			this.UpdateRelatedWorkflowsButton.Name = "UpdateRelatedWorkflowsButton";
			this.UpdateRelatedWorkflowsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateRelatedWorkflowsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 26, true);
			this.UpdateRelatedWorkflowsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UpdateRelatedWorkflowsButton.ToolTipCaption = null;
			this.UpdateRelatedWorkflowsButton.UseVisualStyleBackColor = true;
			this.UpdateRelatedWorkflowsButton.Visible = BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value && Environment.Env.Security.BMBoardEdit.IsAllowed;
			// 
			// 
			// IsLiveTextBox
			// 
			this.BindingSource.SetBindingMember(this.IsLiveTextBox, "LivelinessDescriptionText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BMSystem)(null)).LivelinessDescriptionText)));
			this.IsLiveTextBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("e2f03a65-aa60-40fb-9217-9b33ad364811", "System is:");
			this.IsLiveTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 15, true);
			this.IsLiveTextBox.Name = "IsLiveTextBox";
			this.IsLiveTextBox.ReadOnly = true;
			this.IsLiveTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 20, true);
			this.IsLiveTextBox.TabIndex = 3;
			// 
			// ResourceCountdownTimeEdit
			// 
			this.ResourceCountdownTimeEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ResourceCountdownTimeEdit, "FS_ResourceCountdownHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((BMSystem)(null)).FS_ResourceCountdownHours)));
			this.ResourceCountdownTimeEdit.CaptionResourceString = null;
			this.ResourceCountdownTimeEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 41, true);
			this.ResourceCountdownTimeEdit.Name = "ResourceCountdownTimeEdit";
			this.ResourceCountdownTimeEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
			this.ResourceCountdownTimeEdit.TabIndex = 2;
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "FS_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BMSystem)(null)).FS_Name)));
			this.NameTextBox.CaptionResourceString = null;
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 15, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left));
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "FS_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((BMSystem)(null)).FS_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 41, true);
			this.DescriptionTextBox.Multiline = false;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.DescriptionTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(212, 20, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// ComponentsUserControl
			// 
			this.ComponentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComponentsUserControl, ".");
			this.ComponentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComponentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComponentsUserControl.Name = "ComponentsUserControl";
			this.ComponentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			this.ComponentsUserControl.TabIndex = 0;
			// 
			// CustomisedVisualLayoutsTabPage
			// 
			this.CustomisedVisualLayoutsTabPage.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("93e883e9-7ee1-470b-b6f0-65995fe72175", "Customized Visual Layouts");
			this.CustomisedVisualLayoutsTabPage.Controls.Add(this.CustomisedVisualLayoutsControl);
			this.CustomisedVisualLayoutsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomisedVisualLayoutsTabPage.Name = "CustomisedVisualLayoutsTabPage";
			this.CustomisedVisualLayoutsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			this.CustomisedVisualLayoutsTabPage.TabIndex = 5;
			// 
			// CustomisedVisualLayoutsControl
			// 
			this.CustomisedVisualLayoutsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomisedVisualLayoutsControl, ".");
			this.CustomisedVisualLayoutsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomisedVisualLayoutsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomisedVisualLayoutsControl.Name = "CustomisedVisualLayoutsControl";
			this.CustomisedVisualLayoutsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 536, true);
			this.CustomisedVisualLayoutsControl.TabIndex = 0;
			// 
			// BMSystemManagementForm
			// 
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("ba300b5c-3381-41ad-ac98-226f95623b0b", "Buffer Management System");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 687, true);
			this.DataSourceType = typeof(BMSystem);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "BMSystemManagementForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigTabPage.ResumeLayout(false);
			this.ConfigTabPage.PerformLayout();
			this.ConfigurationUserControl.ResumeLayout(true);
			this.ConfigurationUserControl.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ComponentsUserControl.ResumeLayout(true);
			this.ComponentsUserControl.PerformLayout();
			this.CustomisedVisualLayoutsTabPage.ResumeLayout(false);
			this.CustomisedVisualLayoutsTabPage.PerformLayout();
			this.CustomisedVisualLayoutsControl.ResumeLayout(true);
			this.CustomisedVisualLayoutsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZTabPage ConfigTabPage;
		ZGroupBox DetailsGroupBox;
		ZTextBox NameTextBox;
		ZTextBox DescriptionTextBox;
		private ZTimeEditEx ResourceCountdownTimeEdit;
		private ConfigurationUserControl ConfigurationUserControl;
		public BMComponentsUserControl ComponentsUserControl;
		private ZTabPage CustomisedVisualLayoutsTabPage;
		private CustomisedVisualLayoutsUserControl CustomisedVisualLayoutsControl;
		CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private ZButton SystemIsLiveToggleButton;
		private ZButton ExperimentalSettingsButton;
		private ZButton UpdateRelatedWorkflowsButton;
		private ZTextBox IsLiveTextBox;
	}
}
