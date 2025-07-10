using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	partial class GroupStripControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.FilterStripGroupBox = new Enterprise.ZArchitecture.GUI.GroupPanel();
			this.GroupColorLabel = new CargoWise.Windows.UI.KLabel();
			this.DeleteStripButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterCategoriesToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.FilterCategoriesToolStripDropDown = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripDropDownButton();
			this.filterCategoriesToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZToolStripMenuItem();
			this.separatorToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
			this.noCategoryToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.redToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.greenToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.blueToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.brownToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.greyToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.additionalCategoryToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			this.GroupNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AddStripButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterStripGroupBox.SuspendLayout();
			this.FilterCategoriesToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// FilterStripGroupBox
			// 
			this.FilterStripGroupBox.Controls.Add(this.GroupColorLabel);
			this.FilterStripGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 25, true);
			this.FilterStripGroupBox.Name = "FilterStripGroupBox";
			this.FilterStripGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 35, true);
			this.FilterStripGroupBox.TabIndex = 0;
			// 
			// GroupColorLabel
			// 
			this.GroupColorLabel.BackColor = System.Drawing.Color.Transparent;
			this.GroupColorLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.GroupColorLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.GroupColorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(678, 0, true);
			this.GroupColorLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GroupColorLabel.Name = "GroupColorLabel";
			this.GroupColorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 35, true);
			this.GroupColorLabel.TabIndex = 2;
			// 
			// DeleteStripButton
			// 
			this.DeleteStripButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.DeleteStripButton.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeleteStripButton, false);
			this.DeleteStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(740, 30, true);
			this.DeleteStripButton.Name = "DeleteStripButton";
			this.DeleteStripButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeleteStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.DeleteStripButton.TabIndex = 9998;
			this.DeleteStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DeleteStripButton.ToolTipCaption = ResString.GetMultilingualString("3b00e10c-bf39-444f-9dfe-590467f7e20c", "Remove Group");
			this.DeleteStripButton.Click += new System.EventHandler(this.DeleteStripButton_Click);
			// 
			// FilterCategoriesToolStrip
			// 
			this.FilterCategoriesToolStrip.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.FilterCategoriesToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.FilterCategoriesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.FilterCategoriesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.FilterCategoriesToolStripDropDown});
			this.FilterCategoriesToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.FilterCategoriesToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 30, true);
			this.FilterCategoriesToolStrip.Name = "FilterCategoriesToolStrip";
			this.FilterCategoriesToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 24, true);
			this.FilterCategoriesToolStrip.TabIndex = 9999;
			this.FilterCategoriesToolStrip.TextDirection = System.Windows.Forms.ToolStripTextDirection.Vertical90;
			// 
			// FilterCategoriesToolStripDropDown
			// 
			this.FilterCategoriesToolStripDropDown.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("4DE90F03-19CB-4C1A-8FE8-156F8D78484E", "▼", "Select which group to add this filter to. Any result matching one of the filters in the group will be returned.");
			this.FilterCategoriesToolStripDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.FilterCategoriesToolStripDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.filterCategoriesToolStripMenuItem,
			this.separatorToolStripMenuItem,
			this.noCategoryToolStripMenuItem,
			this.redToolStripMenuItem,
			this.greenToolStripMenuItem,
			this.blueToolStripMenuItem,
			this.brownToolStripMenuItem,
			this.greyToolStripMenuItem,
			this.additionalCategoryToolStripMenuItem});
			this.FilterCategoriesToolStripDropDown.Font = new System.Drawing.Font("Arial", 7F);
			this.FilterCategoriesToolStripDropDown.Name = "FilterCategoriesToolStripDropDown";
			this.FilterCategoriesToolStripDropDown.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 4, 0, 0, true);
			this.FilterCategoriesToolStripDropDown.ShowDropDownArrow = false;
			this.FilterCategoriesToolStripDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 21, true);
			this.FilterCategoriesToolStripDropDown.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			// 
			// filterCategoriesToolStripMenuItem
			// 
			this.filterCategoriesToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|OrFilterCategories", "\'Or\' Filter Categories");
			this.filterCategoriesToolStripMenuItem.Font = OFont.GetFont();
			this.filterCategoriesToolStripMenuItem.Name = "filterCategoriesToolStripMenuItem";
			this.filterCategoriesToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// separatorToolStripMenuItem
			// 
			this.separatorToolStripMenuItem.Name = "separatorToolStripMenuItem";
			this.separatorToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 6, true);
			// 
			// noCategoryToolStripMenuItem
			// 
			this.noCategoryToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|ClearCategory", "Clear Category");
			this.noCategoryToolStripMenuItem.Font = OFont.GetFont();
			this.noCategoryToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.noCategoryToolStripMenuItem.Name = "noCategoryToolStripMenuItem";
			this.noCategoryToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.noCategoryToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// redToolStripMenuItem
			// 
			this.redToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|RedCategory", "Red Category");
			this.redToolStripMenuItem.Font = OFont.GetFont();
			this.redToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.redToolStripMenuItem.Name = "redToolStripMenuItem";
			this.redToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.redToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// greenToolStripMenuItem
			// 
			this.greenToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|GreenCategory", "Green Category");
			this.greenToolStripMenuItem.Font = OFont.GetFont();
			this.greenToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.greenToolStripMenuItem.Name = "greenToolStripMenuItem";
			this.greenToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.greenToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// blueToolStripMenuItem
			// 
			this.blueToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|BlueCategory", "Blue Category");
			this.blueToolStripMenuItem.Font = OFont.GetFont();
			this.blueToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.blueToolStripMenuItem.Name = "blueToolStripMenuItem";
			this.blueToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.blueToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// brownToolStripMenuItem
			// 
			this.brownToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|BrownCategory", "Brown Category");
			this.brownToolStripMenuItem.Font = OFont.GetFont();
			this.brownToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.brownToolStripMenuItem.Name = "brownToolStripMenuItem";
			this.brownToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.brownToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// greyToolStripMenuItem
			// 
			this.greyToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|GreyCategory", "Grey Category");
			this.greyToolStripMenuItem.Font = OFont.GetFont();
			this.greyToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.greyToolStripMenuItem.Name = "greyToolStripMenuItem";
			this.greyToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.greyToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// additionalCategoryToolStripMenuItem
			// 
			this.additionalCategoryToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|AdditionalCategory", "Additional Color Palette");
			this.additionalCategoryToolStripMenuItem.Font = OFont.GetFont();
			this.additionalCategoryToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.additionalCategoryToolStripMenuItem.Name = "additionalCategoryToolStripMenuItem";
			this.additionalCategoryToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.additionalCategoryToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 22, true);
			// 
			// GroupNameLabel
			// 
			this.GroupNameLabel.IsFontBold = true;
			this.GroupNameLabel.AutoSize = true;
			this.GroupNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.GroupNameLabel.Name = "GroupNameLabel";
			this.GroupNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.GroupNameLabel.TabIndex = 10000;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.AddStripButton.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddStripButton, false);
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 30, true);
			this.AddStripButton.Name = "AddStripButton";
			this.AddStripButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.AddStripButton.TabIndex = 10001;
			this.AddStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.AddStripButton.ToolTipCaption = ResString.GetMultilingualString("db14fad6-a664-4fe8-bda5-153b56415b9a", "Add Filter");
			this.AddStripButton.Click += new System.EventHandler(this.AddStripButton_Click);
			// 
			// GroupStripControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.GroupNameLabel);
			this.Controls.Add(this.FilterCategoriesToolStrip);
			this.Controls.Add(this.DeleteStripButton);
			this.Controls.Add(this.FilterStripGroupBox);
			this.Controls.Add(this.AddStripButton);
			this.Name = "GroupStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(830, 62, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterStripGroupBox.ResumeLayout(false);
			this.FilterCategoriesToolStrip.ResumeLayout(false);
			this.FilterCategoriesToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal GroupPanel FilterStripGroupBox;
		private CargoWise.Windows.UI.KLabel GroupColorLabel;
		protected internal ZButton DeleteStripButton;
		protected internal ZFilterStripToolStripDropDownButton FilterCategoriesToolStripDropDown;
		private ZFilterStripToolStripMenuItem redToolStripMenuItem;
		private ZFilterStripToolStripMenuItem greenToolStripMenuItem;
		private ZFilterStripToolStripMenuItem blueToolStripMenuItem;
		private ZFilterStripToolStripMenuItem brownToolStripMenuItem;
		private ZFilterStripToolStripMenuItem greyToolStripMenuItem;
		private ZFilterStripToolStripMenuItem noCategoryToolStripMenuItem;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem filterCategoriesToolStripMenuItem;
		private ZFilterStripToolStripMenuItem additionalCategoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator separatorToolStripMenuItem;
		internal ZToolStrip FilterCategoriesToolStrip;
		internal ZLabel GroupNameLabel;
		protected internal ZButton AddStripButton;

	}
}
