using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	partial class StripControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!DesignModeFinder.IsDesigning)
				{
					UnhookEventsOnFilterBizO();
					UnhookEventsOnParentForm();
					RemoveRecentPanelControl();
				}

				if (components != null)
				{
					components.Dispose();
				}

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StripControl));
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToolStripHelp = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ToolStripHelpButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.ToolStripManageDropButton = new Enterprise.ZArchitecture.GUI.ZToolStripSplitButton();
			this.ToolStripManageLayoutsMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ToolStripManageUsedDefinedMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ToolStripSaveLayoutButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStripResetLayoutButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStripAddGroupButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStripPreviewDropButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			this.ToolStripRecordsFoundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ToolStripFindDropButton = new Enterprise.ZArchitecture.GUI.ZToolStripSplitButton();
			this.ToolStripNoLayoutsAddedMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.ToolStripClearButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.AddStripButton = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripAddButton();
			this.FilterStripsPanel = new CargoWise.Windows.UI.KPanel();
			this.HostControl = new CargoWise.Windows.UI.KElementHost();
			this.RecentItemsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RecentItemsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToolStripColourPicker = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.ToolStripBackColourButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.ToolStripForeColourButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToolStripHelp.SuspendLayout();
			this.ToolStrip.SuspendLayout();
			this.ToolStripColourPicker.SuspendLayout();
			this.SuspendLayout();
			// 
			// CoveringLabel
			// 
			this.CoveringLabel.BackColor = System.Drawing.Color.Transparent;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CoveringLabel, false);
			this.CoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 48, true);
			this.CoveringLabel.Name = "CoveringLabel";
			this.CoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 2, true);
			this.CoveringLabel.TabIndex = 6;
			// 
			// ToolStripHelp
			// 
			this.ToolStripHelp.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripHelp.Dock = System.Windows.Forms.DockStyle.None;
			this.ToolStripHelp.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStripHelp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ToolStripHelpButton,
			this.ToolStripSeparator,
			this.ToolStripManageDropButton,
			this.ToolStripSaveLayoutButton,
			this.ToolStripResetLayoutButton,
			this.ToolStripAddGroupButton});
			this.ToolStripHelp.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 25, true);
			this.ToolStripHelp.Name = "ToolStripHelp";
			this.ToolStripHelp.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 25, true);
			this.ToolStripHelp.TabIndex = 1;
			// 
			// ToolStripHelpButton
			// 
			this.ToolStripHelpButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonHelp", "Help", "Clicking Help will direct you to the Filters update note on the web.");
			this.ToolStripHelpButton.Name = "ToolStripHelpButton";
			this.ToolStripHelpButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ToolStripHelpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 22, true);
			this.ToolStripHelpButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ToolStripSeparator
			// 
			this.ToolStripSeparator.Name = "ToolStripSeparator";
			this.ToolStripSeparator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 25, true);
			// 
			// ToolStripManageDropButton
			// 
			this.ToolStripManageDropButton.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripManageDropButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonManage", "Manage");
			this.ToolStripManageDropButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ToolStripManageLayoutsMenuItem,ToolStripManageUsedDefinedMenuItem});
			this.ToolStripManageDropButton.Name = "ToolStripManageDropButton";
			this.ToolStripManageDropButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ToolStripManageDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 22, true);
			this.ToolStripManageDropButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ToolStripManageLayoutsButton
			// 
			this.ToolStripManageLayoutsMenuItem.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripManageLayoutsMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonManageLayouts", "Manage Layouts");
			this.ToolStripManageLayoutsMenuItem.Name = "ToolStripManageLayoutsMenuItem";
			this.ToolStripManageLayoutsMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 22, true);
			// 
			// ToolStripManageUserDefinedButton
			// 
			this.ToolStripManageUsedDefinedMenuItem.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripManageUsedDefinedMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonManageUserDefinedFilters", "Manage User Defined Filters");
			this.ToolStripManageUsedDefinedMenuItem.Name = "ToolStripManageUserDefinedMenuItem";
			this.ToolStripManageUsedDefinedMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 22, true);

			// 
			// ToolStripSaveLayoutButton
			// 
			this.ToolStripSaveLayoutButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonSave", "Save");
			this.ToolStripSaveLayoutButton.Name = "ToolStripSaveLayoutButton";
			this.ToolStripSaveLayoutButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ToolStripSaveLayoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 22, true);
			this.ToolStripSaveLayoutButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ToolStripResetLayoutButton
			// 
			this.ToolStripResetLayoutButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonReset", "Reset ");
			this.ToolStripResetLayoutButton.Name = "ToolStripResetLayoutButton";
			this.ToolStripResetLayoutButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 22, true);
			// 
			// ToolStripAddGroupButton
			// 
			this.ToolStripAddGroupButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonAddGroup", "Group");
			this.ToolStripAddGroupButton.Name = "ToolStripAddGroupButton";
			this.ToolStripAddGroupButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 22, true);
			// 
			// ToolStripRecordsFoundLabel
			// 
			this.ToolStripRecordsFoundLabel.AutoSize = true;
			this.ToolStripRecordsFoundLabel.ForeColor = System.Drawing.Color.Blue;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToolStripRecordsFoundLabel, false);
			this.ToolStripRecordsFoundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(651, 13, true);
			this.ToolStripRecordsFoundLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.ToolStripRecordsFoundLabel.Name = "ToolStripRecordsFoundLabel";
			this.ToolStripRecordsFoundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ToolStripRecordsFoundLabel.TabIndex = 5;
			// 
			// ToolStrip
			// 
			this.ToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.ToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ToolStripPreviewDropButton,
			this.ToolStripFindDropButton,
			this.ToolStripClearButton});
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 25, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 25, true);
			this.ToolStrip.TabIndex = 3;
			// 
			// ToolStripPreviewDropButton
			// 
			this.ToolStripPreviewDropButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("5cb9dee1-2868-4697-8c96-99b6549621d3", "Preview");
			this.ToolStripPreviewDropButton.Name = "ToolStripPreviewDropButton";
			this.ToolStripPreviewDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 24, true);
			this.ToolStripPreviewDropButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ToolStripPreviewDropButton.Visible = false;
			// 
			// ToolStripFindDropButton
			// 
			this.ToolStripFindDropButton.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripFindDropButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonFind", "Find");
			this.ToolStripFindDropButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ToolStripNoLayoutsAddedMenuItem});
			this.ToolStripFindDropButton.Name = "ToolStripFindDropButton";
			this.ToolStripFindDropButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 0, true);
			this.ToolStripFindDropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 22, true);
			this.ToolStripFindDropButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ToolStripNoLayoutsAddedMenuItem
			// 
			this.ToolStripNoLayoutsAddedMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|NoFiltersHaveBeenAdded", "<no filters have been added>");
			this.ToolStripNoLayoutsAddedMenuItem.Enabled = false;
			this.ToolStripNoLayoutsAddedMenuItem.Name = "ToolStripNoLayoutsAddedMenuItem";
			this.ToolStripNoLayoutsAddedMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 22, true);
			// 
			// ToolStripClearButton
			// 
			this.ToolStripClearButton.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripClearButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|ToolStrip|ButtonClear", "Clear");
			this.ToolStripClearButton.Name = "ToolStripClearButton";
			this.ToolStripClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(38, 22, true);
			// 
			// AddStripButton
			// 
			this.AddStripButton.BackColor = System.Drawing.Color.Transparent;
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 28, true);
			this.AddStripButton.Name = "AddStripButton";
			this.AddStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.AddStripButton.TabIndex = 4;
			this.AddStripButton.TabStop = false;
			// 
			// FilterStripsPanel
			// 
			this.FilterStripsPanel.AutoScroll = true;
			this.FilterStripsPanel.BackColor = System.Drawing.Color.Transparent;
			this.FilterStripsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterStripsPanel.Name = "FilterStripsPanel";
			this.FilterStripsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 41, true);
			this.FilterStripsPanel.TabIndex = 0;
			// 
			// HostControl
			// 
			this.HostControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HostControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
			this.HostControl.Name = "HostControl";
			this.HostControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 10, true);
			this.HostControl.TabIndex = 0;
			this.HostControl.Child = null;
			// 
			// RecentItemsPanel
			// 
			this.RecentItemsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left)));
			this.RecentItemsPanel.Controls.Add(this.HostControl);
			this.RecentItemsPanel.Controls.Add(this.RecentItemsLabel);
			this.RecentItemsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RecentItemsPanel.Name = "RecentItemsPanel";
			this.RecentItemsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 174, true);
			this.RecentItemsPanel.TabIndex = 0;
			// 
			// RecentItemsLabel
			// 
			this.RecentItemsLabel.BackColor = System.Drawing.Color.Transparent;
			this.RecentItemsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RecentItemsLabel.IsFontBold = true;
			this.RecentItemsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RecentItemsLabel.Name = "RecentItemsLabel";
			this.RecentItemsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 16, true);
			this.RecentItemsLabel.TabIndex = 1;
			// 
			// ToolStripColourPicker
			// 
			this.ToolStripColourPicker.BackColor = System.Drawing.Color.Transparent;
			this.ToolStripColourPicker.Dock = System.Windows.Forms.DockStyle.None;
			this.ToolStripColourPicker.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStripColourPicker.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ToolStripBackColourButton,
			this.ToolStripForeColourButton});
			this.ToolStripColourPicker.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 25, true);
			this.ToolStripColourPicker.Name = "ToolStripColourPicker";
			this.ToolStripColourPicker.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.ToolStripColourPicker.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 25, true);
			this.ToolStripColourPicker.TabIndex = 2;
			// 
			// ToolStripBackColourButton
			// 
			this.ToolStripBackColourButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("6F2523FC-3898-4406-8A66-899C68431EE8", "Row Color");
			this.ToolStripBackColourButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolStripBackColourButton.Image")));
			this.ToolStripBackColourButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ToolStripBackColourButton.Name = "ToolStripBackColourButton";
			this.ToolStripBackColourButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 22, true);
			this.ToolStripBackColourButton.Visible = false;
			// 
			// ToolStripForeColourButton
			// 
			this.ToolStripForeColourButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("68B1BC97-11CD-4335-8DC7-45076280108C", "Fore Color");
			this.ToolStripForeColourButton.Image = ((System.Drawing.Image)(resources.GetObject("ToolStripForeColourButton.Image")));
			this.ToolStripForeColourButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ToolStripForeColourButton.Name = "ToolStripForeColourButton";
			this.ToolStripForeColourButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 22, true);
			this.ToolStripForeColourButton.Visible = false;
			// 
			// StripControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Controls.Add(this.CoveringLabel);
			this.Controls.Add(this.ToolStrip);
			this.Controls.Add(this.ToolStripColourPicker);
			this.Controls.Add(this.ToolStripHelp);
			this.Controls.Add(this.ToolStripRecordsFoundLabel);
			this.Controls.Add(this.AddStripButton);
			this.Controls.Add(this.FilterStripsPanel);
			this.Name = "StripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 174, true);
			this.Resize += new System.EventHandler(this.StripControl_Resize);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToolStripHelp.ResumeLayout(false);
			this.ToolStripHelp.PerformLayout();
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.ToolStripColourPicker.ResumeLayout(false);
			this.ToolStripColourPicker.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripHelpButton;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator;
		protected Enterprise.ZArchitecture.GUI.ZToolStripSplitButton ToolStripManageDropButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripMenuItem ToolStripManageLayoutsMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZToolStripMenuItem ToolStripManageUsedDefinedMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripSaveLayoutButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripResetLayoutButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripAddGroupButton;
		protected ZLabel ToolStripRecordsFoundLabel;
		protected Enterprise.ZArchitecture.GUI.ZToolStripSplitButton ToolStripFindDropButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripMenuItem ToolStripNoLayoutsAddedMenuItem;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripClearButton;
		protected Enterprise.ZArchitecture.GUI.Internal.ZFilterStripAddButton AddStripButton;
		protected CargoWise.Windows.UI.KPanel FilterStripsPanel;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripBackColourButton;
		protected Enterprise.ZArchitecture.GUI.ZToolStripButton ToolStripForeColourButton;
		protected ZLabel CoveringLabel;
		private CargoWise.Windows.UI.KElementHost HostControl;
		protected CargoWise.GUI.TileBar.RecentItemsControl RecentItemsControl;
		protected ZToolStrip ToolStripHelp;
		protected ZToolStrip ToolStrip;
		protected ZToolStrip ToolStripColourPicker;
		protected internal ZPanel RecentItemsPanel;
		internal ZLabel RecentItemsLabel;
		protected ZToolStripDropDownButton ToolStripPreviewDropButton;
	}
}
