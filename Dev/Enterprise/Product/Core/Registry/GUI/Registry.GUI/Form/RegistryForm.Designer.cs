using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RegistryForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components;

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
			this.components = new System.ComponentModel.Container();
			this.RegistriesTreeView = new RegistryItemsTreeView();
			this.FallbackTreeView = new Enterprise.ZArchitecture.GUI.ZTreeView();
			this.RegistryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FallbackLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PluginGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tabControlRegistryItem = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.tabPageDetails = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PluginPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.registrySplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.FallbackMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SecurityDeniedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ValueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.HintUserControl = new Enterprise.ZArchitecture.GUI.ZUserControl();
			this.HintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ExtraPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.translateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NameOnDbTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NewFileMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.HideMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.findOverridesMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.importOverridesMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.exportListOfPreservedRegistryItemsMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.filterChangeLogsMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.CloseMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.NewHelpMenuItem = new Enterprise.ZArchitecture.GUI.ZMenuItem();
			this.NewMainMenu = new System.Windows.Forms.MainMenu(this.components);
			this.treeViewSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RegistriesTreeViewPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.FallbackTreeViewPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SupportLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.registrySplitContainer)).BeginInit();
			this.registrySplitContainer.Panel1.SuspendLayout();
			this.registrySplitContainer.Panel2.SuspendLayout();
			this.registrySplitContainer.SuspendLayout();
			this.PluginGroupBox.SuspendLayout();
			this.tabControlRegistryItem.SuspendLayout();
			this.tabPageDetails.SuspendLayout();
			this.PluginPanel.SuspendLayout();
			this.HintUserControl.SuspendLayout();
			this.ExtraPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.treeViewSplitContainer)).BeginInit();
			this.treeViewSplitContainer.Panel1.SuspendLayout();
			this.treeViewSplitContainer.Panel2.SuspendLayout();
			this.treeViewSplitContainer.SuspendLayout();
			this.RegistriesTreeViewPanel.SuspendLayout();
			this.FallbackTreeViewPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 625, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(496);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(497);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.RegistryFormManager);
			// 
			// RegistriesTreeView
			// 
			this.RegistriesTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.RegistriesTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.RegistriesTreeView.HideSelection = false;
			this.RegistriesTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.RegistriesTreeView.Name = "RegistriesTreeView";
			this.RegistriesTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 566, true);
			this.RegistriesTreeView.TabIndex = 0;
			this.RegistriesTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.RegistriesTreeView_BeforeSelect);
			this.RegistriesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.RegistriesTreeView_AfterSelect);
			// 
			// FallbackTreeView
			// 
			this.FallbackTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FallbackTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
			this.FallbackTreeView.HideSelection = false;
			this.FallbackTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 22, true);
			this.FallbackTreeView.Name = "FallbackTreeView";
			this.FallbackTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 566, true);
			this.FallbackTreeView.TabIndex = 1;
			this.FallbackTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.FallbackTreeView_BeforeSelect);
			this.FallbackTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FallbackTreeView_AfterSelect);
			// 
			// RegistryLabel
			// 
			this.RegistryLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|874ee00a-fc62-44c7-8a25-3c6e88f28290", "Select a Registry Item");
			this.RegistryLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.RegistryLabel.IsFontBold = true;
			this.RegistryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 0, true);
			this.RegistryLabel.Name = "RegistryLabel";
			this.RegistryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 21, true);
			this.RegistryLabel.TabIndex = 100;
			// 
			// FallbackLabel
			// 
			this.FallbackLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|e283a40b-1da0-4956-8836-573c3d49afd9", "Select a Fallback Level");
			this.FallbackLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FallbackLabel.IsFontBold = true;
			this.FallbackLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FallbackLabel.Name = "FallbackLabel";
			this.FallbackLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 21, true);
			this.FallbackLabel.TabIndex = 100;
			// 
			// PluginGroupBox
			//
			this.PluginGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PluginGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|a783d2b1-eb9d-4f67-9e2e-26bf976161ad", "Registry Item Editor");
            this.PluginGroupBox.Controls.Add(this.tabControlRegistryItem);
			this.PluginGroupBox.Name = "PluginGroupBox";
			this.PluginGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(466, 589, true);
			this.PluginGroupBox.TabIndex = 100;
			this.PluginGroupBox.TabStop = false;
			// 
			// tabControlRegistryItem
			// 
			this.tabControlRegistryItem.Controls.Add(this.tabPageDetails);
			this.tabControlRegistryItem.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.tabControlRegistryItem.Name = "tabControlRegistryItem";
			this.tabControlRegistryItem.SelectedIndex = 0;
			this.tabControlRegistryItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 567, true);
			this.tabControlRegistryItem.TabIndex = 108;
			// 
			// tabPageDetails
			// 
			this.tabPageDetails.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|22b42174-99c3-479f-9835-ddb2a6740144", "Details");
			this.tabPageDetails.Controls.Add(this.PluginPanel);
			this.tabPageDetails.Controls.Add(this.HintUserControl);
			this.tabPageDetails.Controls.Add(this.SupportLabel);
			this.tabPageDetails.Controls.Add(this.ExtraPanel);
			this.tabPageDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.tabPageDetails.Name = "tabPageDetails";
			this.tabPageDetails.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.tabPageDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 540, true);
			this.tabPageDetails.TabIndex = 0;
			this.tabPageDetails.UseVisualStyleBackColor = true;
			// 
			// PluginPanel
			//
			this.PluginPanel.Controls.Add(this.FallbackMessageLabel);
			this.PluginPanel.Controls.Add(this.SecurityDeniedLabel);
			this.PluginPanel.Controls.Add(this.OverrideCheckBox);
			this.PluginPanel.Controls.Add(this.ValueLabel);
			this.PluginPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PluginPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 18, true);
			this.PluginPanel.Name = "PluginPanel";
			this.PluginPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 484, true);
			this.PluginPanel.TabIndex = 100;
			// 
			// FallbackMessageLabel
			// 
			this.FallbackMessageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FallbackMessageLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|94348ea4-de9f-415b-a7c8-a2e0243fe558", "Fallback Message");
			this.FallbackMessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FallbackMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.FallbackMessageLabel.Name = "FallbackMessageLabel";
			this.FallbackMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 20, true);
			this.FallbackMessageLabel.TabIndex = 100;
			// 
			// SecurityDeniedLabel
			// 
			this.SecurityDeniedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SecurityDeniedLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|2b390c58-fcc7-4065-bdf4-680dbf15a5f3", "Security Denied");
			this.SecurityDeniedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SecurityDeniedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 30, true);
			this.SecurityDeniedLabel.Name = "SecurityDeniedLabel";
			this.SecurityDeniedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 20, true);
			this.SecurityDeniedLabel.TabIndex = 100;
			// 
			// OverrideCheckBox
			// 
			this.OverrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideCheckBox, "OverrideDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.GUI.RegistryFormManager)(null)).OverrideDefault)));
			this.OverrideCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|a4906f3e-7c7f-41cd-8bef-7e14f9139b02", "Override Default");
			this.OverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 50, true);
			this.OverrideCheckBox.Name = "OverrideCheckBox";
			this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17, true);
			this.OverrideCheckBox.TabIndex = 2;
			// 
			// ValueLabel
			// 
			this.ValueLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|5aab4b98-2c85-4096-b290-4feef0e085a8", "Value of Registry");
			this.ValueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ValueLabel.IsFontBold = true;
			this.ValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.ValueLabel.Name = "ValueLabel";
			this.ValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.ValueLabel.TabIndex = 100;
			// 
			// HintUserControl
			// 
			this.HintUserControl.CaptionRenderingEnabled = true;
			this.HintUserControl.AllowDrop = true;
			this.HintUserControl.AutoScroll = true;
			this.HintUserControl.AutoSize = true;
			this.HintUserControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.HintUserControl.Controls.Add(this.HintLabel);
			this.HintUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.HintUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HintUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 240, true);
			this.HintUserControl.Name = "HintUserControl";
			this.HintUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(446, 15, true);
			this.HintUserControl.TabIndex = 100;
			// 
			// HintLabel
			// 
			this.HintLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|824f496d-2e96-4c8a-a319-a5d1d690a29b", "Please select a Registry item.");
			this.HintLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HintLabel.Name = "HintLabel";
			this.HintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 15, true);
			this.HintLabel.TabIndex = 100;
			this.HintLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.HintLabel.UseMnemonic = false;
			// 
			// ExtraPanel
			// 
			this.ExtraPanel.Controls.Add(this.translateButton);
			this.ExtraPanel.Controls.Add(this.NameOnDbTextBox);
			this.ExtraPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ExtraPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 502, true);
			this.ExtraPanel.Name = "ExtraPanel";
			this.ExtraPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 35, true);
			this.ExtraPanel.TabIndex = 102;
			// 
			// translateButton
			// 
			this.translateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.translateButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d1cd356c-da76-4b2d-9780-0a2684b30205", "Edit Local Language Values");
			this.translateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.translateButton.Name = "translateButton";
			this.translateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.translateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 23, true);
			this.translateButton.TabIndex = 106;
			this.translateButton.ToolTipCaption = null;
			this.translateButton.UseVisualStyleBackColor = true;
			this.translateButton.Visible = false;
			this.translateButton.Click += new System.EventHandler(this.translateButton_Click);
			// 
			// NameOnDbTextBox
			// 
			this.NameOnDbTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.NameOnDbTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4a6fea29-4104-4a1c-9b0f-876412991a27", "Name");
			this.NameOnDbTextBox.CharacterCasing = CharacterCasing.Normal;
			this.NameOnDbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(271, 5, true);
			this.NameOnDbTextBox.Name = "NameOnDbTextBox";
			this.NameOnDbTextBox.ReadOnly = true;
			this.NameOnDbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.NameOnDbTextBox.TabIndex = 105;
			this.NameOnDbTextBox.Visible = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|db389214-4f6d-4734-ad83-87b887d10171", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(915, 598, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 107;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|f2bf41fa-119f-4b2c-9c08-9b9dc4bf2584", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(834, 598, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SaveButton.TabIndex = 106;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// NewFileMenuItem
			// 
			this.NewFileMenuItem.Caption = null;
			this.NewFileMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("28ae55fa-d39b-4adb-860f-b4bac55bec69", "&File");
			this.NewFileMenuItem.Index = 0;
			this.NewFileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.HideMenuItem,
			this.findOverridesMenuItem,
			this.importOverridesMenuItem,
			this.exportListOfPreservedRegistryItemsMenuItem,
			this.filterChangeLogsMenuItem,
			this.CloseMenuItem});
			// 
			// HideMenuItem
			// 
			this.HideMenuItem.Caption = null;
			this.HideMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7674cbd4-b5b3-431f-b616-57e11069e65a", "&Hide Inactive Fallbacks");
			this.HideMenuItem.Index = 0;
			this.HideMenuItem.Click += new System.EventHandler(this.HideMenuItem_Click);
			// 
			// findOverridesMenuItem
			// 
			this.findOverridesMenuItem.Caption = null;
			this.findOverridesMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7e77571b-ce78-486a-b518-ef3190aba056", "Find Overrides");
			this.findOverridesMenuItem.Index = 1;
			this.findOverridesMenuItem.Click += new System.EventHandler(this.findOverridesMenuItem_Click);
			// 
			// importOverridesMenuItem
			// 
			this.importOverridesMenuItem.Caption = null;
			this.importOverridesMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("12C5EF4C-AB4E-4030-A594-25A2A4FCF896", "Import Overrides");
			this.importOverridesMenuItem.Index = 2;
			this.importOverridesMenuItem.Click += new System.EventHandler(this.importOverridesMenuItem_Click);
			//
			// exportListOfPreservedRegistryItemsMenuItem
			// 
			this.exportListOfPreservedRegistryItemsMenuItem.Caption = null;
			this.exportListOfPreservedRegistryItemsMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3e45536b-fb63-4fda-9e6d-5a55e0f5c31e", "Export List Of Preserved Registry Items");
			this.exportListOfPreservedRegistryItemsMenuItem.Index = 3;
			this.exportListOfPreservedRegistryItemsMenuItem.Click += new System.EventHandler(this.exportListOfPreservedRegistryItemsMenuItem_Click);
			//
			// FilterChangeLogsMenuItem
			//
			this.filterChangeLogsMenuItem.Caption = null;
			this.filterChangeLogsMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0728AC28-D6D4-4F44-9FC5-999AA7BFF2E3", "Hide Items Without Change Logs");
			this.filterChangeLogsMenuItem.Index = 4;
			this.filterChangeLogsMenuItem.Click += new System.EventHandler(this.filterChangeLogsMenuItem_Click);
			// 
			// CloseMenuItem
			// 
			this.CloseMenuItem.Caption = null;
			this.CloseMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("35bae880-86ba-48a4-937a-e6ee9804eb42", "&Close");
			this.CloseMenuItem.Index = 5;
			this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
			// 
			// NewHelpMenuItem
			// 
			this.NewHelpMenuItem.Caption = null;
			this.NewHelpMenuItem.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4BEDA120-ED06-49AF-A1C9-CB9A9C05A20C", "&Help");
			this.NewHelpMenuItem.Index = 1;
			// 
			// NewMainMenu
			// 
			this.NewMainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.NewFileMenuItem,
			this.NewHelpMenuItem});
			// 
			// TreeViewSplitContainer
			// 
			this.treeViewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewSplitContainer.Panel2.Controls.Add(this.FallbackTreeViewPanel);
			this.treeViewSplitContainer.Panel1.Controls.Add(this.RegistriesTreeViewPanel);
			this.treeViewSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 592, true);
			this.treeViewSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.treeViewSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.treeViewSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.treeViewSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.treeViewSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.treeViewSplitContainer.Name = "TreeViewSplitContainer";
			this.treeViewSplitContainer.TabIndex = 101;
			// 
			// RegistriesTreeViewPanel
			// 
			this.RegistriesTreeViewPanel.Controls.Add(this.RegistriesTreeView);
			this.RegistriesTreeViewPanel.Controls.Add(this.RegistryLabel);
			this.RegistriesTreeViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RegistriesTreeViewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RegistriesTreeViewPanel.Name = "RegistriesTreeViewPanel";
			this.RegistriesTreeViewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 592, true);
			this.RegistriesTreeViewPanel.TabIndex = 0;
			// 
			// FallbackTreeViewPanel
			// 
			this.FallbackTreeViewPanel.Controls.Add(this.FallbackTreeView);
			this.FallbackTreeViewPanel.Controls.Add(this.FallbackLabel);
			this.FallbackTreeViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RegistriesTreeViewPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.FallbackTreeViewPanel.Name = "FallbackTreeViewPanel";
            this.RegistriesTreeViewPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 592, true);
            this.FallbackTreeViewPanel.TabIndex = 2;
			// 
			// SupportLabel
			// 
			this.SupportLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SupportLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SupportLabel.ForeColor = System.Drawing.Color.Red;
			this.SupportLabel.IsFontBold = true;
			this.SupportLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SupportLabel.Name = "SupportLabel";
			this.SupportLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 15, true);
			this.SupportLabel.TabIndex = 103;
			this.SupportLabel.Text = "Visible only to CWSupport";
			this.SupportLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.SupportLabel.UseMnemonic = false;
			this.SupportLabel.Visible = false;
			// 
			// RegistrySplitContainer
			// 
			this.registrySplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.registrySplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.registrySplitContainer.Name = "RegistrySplitContainer";
			this.registrySplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 592, true);
			this.registrySplitContainer.TabIndex = 0;
			this.registrySplitContainer.Panel2.Controls.Add(this.PluginGroupBox);
			this.registrySplitContainer.Panel1.Controls.Add(this.treeViewSplitContainer);
			this.registrySplitContainer.FixedPanel = FixedPanel.Panel1;
			this.registrySplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(384);
			this.registrySplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			this.registrySplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(530);
			// 
			// RegistryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|65b6d239-7d45-4e40-a904-b421bd2a142e", "Edit System Registry");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 648, true);
			this.Controls.Add(this.registrySplitContainer);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Registry.GUI";
			this.DataSourceType = typeof(Enterprise.Registry.GUI.RegistryFormManager);
			this.DataSourceTypeName = "Enterprise.Registry.GUI.RegistryFormManager";
			this.Menu = this.NewMainMenu;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 686, true);
			this.Name = "RegistryForm";
			this.Resize += new System.EventHandler(this.RegistryForm_Resize);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.registrySplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PluginGroupBox.ResumeLayout(false);
			this.PluginGroupBox.PerformLayout();
			this.tabControlRegistryItem.ResumeLayout(false);
			this.tabControlRegistryItem.PerformLayout();
			this.tabPageDetails.ResumeLayout(false);
			this.tabPageDetails.PerformLayout();
			this.PluginPanel.ResumeLayout(false);
			this.PluginPanel.PerformLayout();
			this.HintUserControl.ResumeLayout(true);
			this.HintUserControl.PerformLayout();
			this.ExtraPanel.ResumeLayout(false);
			this.ExtraPanel.PerformLayout();
			this.RegistriesTreeViewPanel.ResumeLayout(false);
			this.RegistriesTreeViewPanel.PerformLayout();
			this.treeViewSplitContainer.Panel1.ResumeLayout(false);
			this.treeViewSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.treeViewSplitContainer)).EndInit();
			this.treeViewSplitContainer.ResumeLayout(false);
			this.treeViewSplitContainer.PerformLayout();
			this.registrySplitContainer.Panel1.ResumeLayout(false);
			this.registrySplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.registrySplitContainer)).EndInit();
			this.registrySplitContainer.ResumeLayout(false);
			this.registrySplitContainer.PerformLayout();
			this.FallbackTreeViewPanel.ResumeLayout(false);
			this.FallbackTreeViewPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected LinkButton LinkButton;
		protected Control PluginControl;
		protected RegistryItemsTreeView RegistriesTreeView;
		protected Enterprise.ZArchitecture.GUI.ZTreeView FallbackTreeView;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox OverrideCheckBox;
		protected Enterprise.ZArchitecture.GUI.ZPanel PluginPanel;
		protected CargoWise.Windows.UI.KSplitContainer registrySplitContainer;
		protected Enterprise.ZArchitecture.ZLabel HintLabel;
		private Enterprise.ZArchitecture.ZLabel RegistryLabel;
		private Enterprise.ZArchitecture.ZLabel FallbackLabel;
		protected Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox PluginGroupBox;
		protected Enterprise.ZArchitecture.ZLabel ValueLabel;
		private Enterprise.ZArchitecture.GUI.ZMenuItem NewFileMenuItem;
		private Enterprise.ZArchitecture.GUI.ZMenuItem NewHelpMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZMenuItem HideMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZMenuItem CloseMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZMenuItem ERequestMenuItem;
		private System.Windows.Forms.MainMenu NewMainMenu;
		protected Enterprise.ZArchitecture.ZLabel FallbackMessageLabel;
		protected Enterprise.ZArchitecture.ZLabel SecurityDeniedLabel;
		protected CargoWise.Windows.UI.KSplitContainer treeViewSplitContainer;
		private Enterprise.ZArchitecture.GUI.ZPanel FallbackTreeViewPanel;
		protected ZTabControl tabControlRegistryItem;
		protected ZTabPage tabPageDetails;
		protected Enterprise.ZArchitecture.GUI.ZPanel ExtraPanel;
		private ZButton translateButton;
		private ZTextBox NameOnDbTextBox;
		private Enterprise.ZArchitecture.GUI.ZPanel RegistriesTreeViewPanel;
		internal ZMenuItem findOverridesMenuItem;
		internal ZMenuItem importOverridesMenuItem;
		internal ZMenuItem exportListOfPreservedRegistryItemsMenuItem;
		internal ZMenuItem filterChangeLogsMenuItem;
		protected ZUserControl HintUserControl;
		protected ZLabel SupportLabel;
	}
}
