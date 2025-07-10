using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZFilterStrip
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
			if (disposing)
			{
				UnHookValueChangedEvents();

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
			this.FilterDescriptionDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZFilterStripDropEdit();
			this.DeleteStripButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterPropertyLockButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GripHolderPanel = new CargoWise.Windows.UI.KPanel();
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
			this.additionalToolStripMenuItem = new Enterprise.ZArchitecture.GUI.ZFilterStripToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FilterCategoriesToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.FilterStrip);
			// 
			// FilterDescriptionDropEdit
			// 
			this.BindingSource.SetBindingMember(this.FilterDescriptionDropEdit, "FilterDescriptionLocalized");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ZArchitecture.Business.Internal.FilterStrip)(null)).FilterDescriptionLocalized)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ZArchitecture.Business.Internal.FilterStrip)(null)).FilterDescriptionLocalizedList)));
			this.FilterDescriptionDropEdit.BindToList = "FilterDescriptionLocalizedList";
			this.FilterDescriptionDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FilterDescriptionDropEdit, false);
			this.FilterDescriptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 1, true);
			this.FilterDescriptionDropEdit.MaxItemsToShowInDropDown = 35;
			this.FilterDescriptionDropEdit.Name = "FilterDescriptionDropEdit";
			this.FilterDescriptionDropEdit.PreBoundMaxLength = 30;
			this.FilterDescriptionDropEdit.ShowDescriptionBox = false;
			this.FilterDescriptionDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FilterDescriptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.FilterDescriptionDropEdit.TabIndex = 0;
			// 
			// DeleteStripButton
			// 
			this.DeleteStripButton.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeleteStripButton, false);
			this.DeleteStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(615, 1, true);
			this.DeleteStripButton.Name = "DeleteStripButton";
			this.DeleteStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.DeleteStripButton.TabIndex = 9997;
			this.DeleteStripButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.DeleteStripButton.ToolTipCaption = ResString.GetMultilingualString("b8e40e4a-c2a5-4f3e-8ec8-6851da83974f", "Remove Filter");
			this.DeleteStripButton.Click += new System.EventHandler(this.DeleteStripButton_Click);
			// 
			// FilterPropertyLockButton
			// 
			this.FilterPropertyLockButton.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FilterPropertyLockButton, false);
			this.FilterPropertyLockButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 1, true);
			this.FilterPropertyLockButton.Name = "FilterPropertyLockButton";
			this.FilterPropertyLockButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 18, true);
			this.FilterPropertyLockButton.TabIndex = 9998;
			this.FilterPropertyLockButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.FilterPropertyLockButton.ToolTipCaption = ResString.GetMultilingualString("760F1A2A-EDD3-49BB-BB4D-CAAF770D67A2", "Lock/Unlocked: A locked filter's properties will not be cleared when the Clear button is pressed.");
			FilterPropertyLockButton.Click += FilterPropertyLockButtonClick;
			// 
			// GripHolderPanel
			// 
			this.GripHolderPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.GripHolderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.GripHolderPanel.Name = "GripHolderPanel";
			this.GripHolderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 17, true);
			this.GripHolderPanel.TabIndex = 2000;
			// 
			// FilterCategoriesToolStrip
			// 
			this.FilterCategoriesToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.FilterCategoriesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.FilterCategoriesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.FilterCategoriesToolStripDropDown});
			this.FilterCategoriesToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.FilterCategoriesToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(658, 0, true);
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
			this.additionalToolStripMenuItem});
			this.FilterCategoriesToolStripDropDown.Font = new System.Drawing.Font("Arial", 7F);
			this.FilterCategoriesToolStripDropDown.Name = "FilterCategoriesToolStripDropDown";
			this.FilterCategoriesToolStripDropDown.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 4, 0, 0, true);
			this.FilterCategoriesToolStripDropDown.ShowDropDownArrow = false;
			this.FilterCategoriesToolStripDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 21, true);
			this.FilterCategoriesToolStripDropDown.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
			// 
			// filterCategoriesToolStripMenuItem
			// 
			this.filterCategoriesToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|OrFilterCategories", "'Or' Filter Categories");
			this.filterCategoriesToolStripMenuItem.Font = OFont.GetFont();
			this.filterCategoriesToolStripMenuItem.Name = "filterCategoriesToolStripMenuItem";
			this.filterCategoriesToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// separatorToolStripMenuItem
			// 
			this.separatorToolStripMenuItem.Name = "separatorToolStripMenuItem";
			this.separatorToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 6, true);
			// 
			// noCategoryToolStripMenuItem
			// 
			this.noCategoryToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|ClearCategory", "Clear Category");
			this.noCategoryToolStripMenuItem.Font = OFont.GetFont();
			this.noCategoryToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.noCategoryToolStripMenuItem.Name = "noCategoryToolStripMenuItem";
			this.noCategoryToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.noCategoryToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// redToolStripMenuItem
			// 
			this.redToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|RedCategory", "Red Category");
			this.redToolStripMenuItem.Font = OFont.GetFont();
			this.redToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.redToolStripMenuItem.Name = "redToolStripMenuItem";
			this.redToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.redToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// greenToolStripMenuItem
			// 
			this.greenToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|GreenCategory", "Green Category");
			this.greenToolStripMenuItem.Font = OFont.GetFont();
			this.greenToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.greenToolStripMenuItem.Name = "greenToolStripMenuItem";
			this.greenToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.greenToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// blueToolStripMenuItem
			// 
			this.blueToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|BlueCategory", "Blue Category");
			this.blueToolStripMenuItem.Font = OFont.GetFont();
			this.blueToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.blueToolStripMenuItem.Name = "blueToolStripMenuItem";
			this.blueToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.blueToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// brownToolStripMenuItem
			// 
			this.brownToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|BrownCategory", "Brown Category");
			this.brownToolStripMenuItem.Font = OFont.GetFont();
			this.brownToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.brownToolStripMenuItem.Name = "brownToolStripMenuItem";
			this.brownToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.brownToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// greyToolStripMenuItem
			// 
			this.greyToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|OrCategory|GreyCategory", "Grey Category");
			this.greyToolStripMenuItem.Font = OFont.GetFont();
			this.greyToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.greyToolStripMenuItem.Name = "greyToolStripMenuItem";
			this.greyToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.greyToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// additionalToolStripMenuItem
			// 
			this.additionalToolStripMenuItem.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterStrip|AdditionalOrCategory|AdditionalCategory", "Additional Color Palette");
			this.additionalToolStripMenuItem.Font = OFont.GetFont();
			this.additionalToolStripMenuItem.Color = System.Drawing.Color.Empty;
			this.additionalToolStripMenuItem.Name = "additionalToolStripMenuItem";
			this.additionalToolStripMenuItem.OrCategory = Enterprise.ZArchitecture.Business.FilterOrCategory.None;
			this.additionalToolStripMenuItem.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 22, true);
			// 
			// ZFilterStrip
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GripHolderPanel);
			this.Controls.Add(this.FilterCategoriesToolStrip);
			this.Controls.Add(this.FilterDescriptionDropEdit);
			this.Controls.Add(this.DeleteStripButton);
			this.Controls.Add(this.FilterPropertyLockButton);
			this.Font = new System.Drawing.Font(OFont.NormalFontName, 8.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ZFilterStrip";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FilterCategoriesToolStrip.ResumeLayout(false);
			this.FilterCategoriesToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected internal ZFilterStripDropEdit FilterDescriptionDropEdit;
		protected internal ZButton DeleteStripButton;
		protected internal ZButton FilterPropertyLockButton;
		protected internal CargoWise.Windows.UI.KPanel GripHolderPanel;
		internal CargoWise.Windows.UI.KToolStrip FilterCategoriesToolStrip;
		protected internal ZFilterStripToolStripDropDownButton FilterCategoriesToolStripDropDown;
		private ZFilterStripToolStripMenuItem redToolStripMenuItem;
		private ZFilterStripToolStripMenuItem greenToolStripMenuItem;
		private ZFilterStripToolStripMenuItem blueToolStripMenuItem;
		private ZFilterStripToolStripMenuItem brownToolStripMenuItem;
		private ZFilterStripToolStripMenuItem greyToolStripMenuItem;
		private ZFilterStripToolStripMenuItem additionalToolStripMenuItem;
		private ZFilterStripToolStripMenuItem noCategoryToolStripMenuItem;
		private Enterprise.ZArchitecture.GUI.ZToolStripMenuItem filterCategoriesToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator separatorToolStripMenuItem;
	}

	class CargowiseToolStripRenderer : ToolStripSystemRenderer
	{
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			// This gets rid of the little white line under the arrow
		}
	}
}
