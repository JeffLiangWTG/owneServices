using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ComplianceRisk.GUI
{
	partial class CommodityAssessmentPanelUserControl
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
			if (compliancePluginBizObj != null)
			{
				compliancePluginBizObj.ComplianceRiskStatus.CommodityDetailCollection.CountChanged -= CommodityDetailCollection_CountChanged;
				compliancePluginBizObj.ComplianceRiskStatus.CommodityDetailCollectionView.CountChanged -= CommodityDetailCollection_CountChanged;
			}

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
			this.CommodityAssessmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailGroupBoxTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.TopBannerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Toolstrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.FilterCountInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RemoveFiltersToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.RemoveFiltersButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CommodityAssessmentSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommodityDetailsGroupBox.SuspendLayout();
			this.DetailGroupBoxTableLayoutPanel.SuspendLayout();
			this.TopBannerPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityAssessmentSplitContainer)).BeginInit();
			this.CommodityAssessmentSplitContainer.Panel1.SuspendLayout();
			this.CommodityAssessmentSplitContainer.Panel2.SuspendLayout();
			this.CommodityAssessmentSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// CommodityAssessmentGroupBox
			// 
			this.CommodityAssessmentGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("f46960af-6027-42a1-8739-a106e9f63545", "Compliance Assessment");
			this.CommodityAssessmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityAssessmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityAssessmentGroupBox.Name = "CommodityAssessmentGroupBox";
			this.CommodityAssessmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 185, true);
			this.CommodityAssessmentGroupBox.TabIndex = 5;
			this.CommodityAssessmentGroupBox.TabStop = false;
			// 
			// CommodityDetailsGroupBox
			// 
			this.CommodityDetailsGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("8AAE7152-14C3-4BA7-837C-C5D1E72A30CD", "Commodity Details");
			this.CommodityDetailsGroupBox.Controls.Add(this.DetailGroupBoxTableLayoutPanel);
			this.CommodityDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityDetailsGroupBox.Name = "CommodityDetailsGroupBox";
			this.CommodityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 185, true);
			this.CommodityDetailsGroupBox.TabIndex = 6;
			this.CommodityDetailsGroupBox.TabStop = false;
			// 
			// DetailGroupBoxTableLayoutPanel
			// 
			this.DetailGroupBoxTableLayoutPanel.ColumnCount = 1;
			this.DetailGroupBoxTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DetailGroupBoxTableLayoutPanel.Controls.Add(this.TopBannerPanel, 0, 0);
			this.DetailGroupBoxTableLayoutPanel.Controls.Add(this.FilterPanel, 0, 1);
			this.DetailGroupBoxTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailGroupBoxTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.DetailGroupBoxTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.DetailGroupBoxTableLayoutPanel.Name = "DetailGroupBoxTableLayoutPanel";
			this.DetailGroupBoxTableLayoutPanel.RowCount = 2;
			this.DetailGroupBoxTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(24)));
			this.DetailGroupBoxTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.DetailGroupBoxTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(16)));
			this.DetailGroupBoxTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 168, true);
			this.DetailGroupBoxTableLayoutPanel.TabIndex = 1;
			// 
			// TopBannerPanel
			// 
			this.TopBannerPanel.AutoScroll = true;
			this.TopBannerPanel.Controls.Add(this.RemoveFiltersToolStrip);
			this.TopBannerPanel.Controls.Add(this.Toolstrip);
			this.TopBannerPanel.Controls.Add(this.FilterCountInfoLabel);
			this.TopBannerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopBannerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopBannerPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.TopBannerPanel.Name = "TopBannerPanel";
			this.TopBannerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 24, true);
			this.TopBannerPanel.TabIndex = 6;
			// 
			// Toolstrip
			// 
			this.Toolstrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(247)))));
			this.Toolstrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Toolstrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 0, true);
			this.Toolstrip.Name = "Toolstrip";
			this.Toolstrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 24, true);
			this.Toolstrip.TabIndex = 0;
			this.Toolstrip.GripStyle = ToolStripGripStyle.Hidden;
			//
			// RemoveFiltersToolStrip
			//
			this.RemoveFiltersToolStrip.AutoSize = true;
			this.RemoveFiltersToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(247)))));
			this.RemoveFiltersToolStrip.Dock = System.Windows.Forms.DockStyle.Right;
			this.RemoveFiltersToolStrip.Name = "RemoveFiltersToolStrip";
			this.RemoveFiltersToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 24);
			this.RemoveFiltersToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 0);
			this.RemoveFiltersToolStrip.Width = 200;
			this.RemoveFiltersToolStrip.TabIndex = 4;
			this.RemoveFiltersToolStrip.GripStyle = ToolStripGripStyle.Hidden;
			this.RemoveFiltersToolStrip.Padding = new Padding(0, 4, 0, 0);
			this.RemoveFiltersToolStrip.Items.Add(this.RemoveFiltersButton);
			// 
			// RemoveFiltersButton
			// 
			this.RemoveFiltersButton.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("333DCB47-D659-40C6-8AAC-3F1110118573", "Remove All Filters");
			this.RemoveFiltersButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RemoveFiltersButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.RemoveFiltersButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(246)))), ((int)(((byte)(247)))));
			this.RemoveFiltersButton.Name = "RemoveFiltersButton";
			this.RemoveFiltersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 0, true);
			this.RemoveFiltersButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.RemoveFiltersButton.Image = Icons.GetImage(IconTypes.MinusButtonActive);
			this.RemoveFiltersButton.ImageAlign = ContentAlignment.MiddleLeft;
			this.RemoveFiltersButton.TextImageRelation = TextImageRelation.ImageBeforeText;
			this.RemoveFiltersButton.ImageScaling = ToolStripItemImageScaling.SizeToFit;
			this.RemoveFiltersButton.Click += new System.EventHandler(this.RemoveFiltersButton_Click);
			// 
			// FilterCountInfoLabel
			// 
			this.FilterCountInfoLabel.Dock = System.Windows.Forms.DockStyle.Right;
			this.FilterCountInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FilterCountInfoLabel.ForeColor = System.Drawing.SystemColors.AppWorkspace;
			this.FilterCountInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 0, true);
			this.FilterCountInfoLabel.Name = "FilterCountInfoLabel";
			this.FilterCountInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 24, true);
			this.FilterCountInfoLabel.TabIndex = 3;
			this.FilterCountInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.FilterCountInfoLabel.UseMnemonic = false;
			// 
			// FilterPanel
			// 
			this.FilterPanel.AutoScroll = true;
			this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.FilterPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 144, true);
			this.FilterPanel.TabIndex = 1;
			// 
			// CommodityAssessmentSplitContainer
			// 
			this.CommodityAssessmentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityAssessmentSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommodityAssessmentSplitContainer.Name = "CommodityAssessmentSplitContainer";
			// 
			// CommodityAssessmentSplitContainer.Panel1
			// 
			this.CommodityAssessmentSplitContainer.Panel1.Controls.Add(this.CommodityDetailsGroupBox);
			// 
			// CommodityAssessmentSplitContainer.Panel2
			// 
			this.CommodityAssessmentSplitContainer.Panel2.Controls.Add(this.CommodityAssessmentGroupBox);
			this.CommodityAssessmentSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 185, true);
			this.CommodityAssessmentSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(493);
			this.CommodityAssessmentSplitContainer.SplitterWidth = 5;
			this.CommodityAssessmentSplitContainer.TabIndex = 4;
			// 
			// CommodityAssessmentPanelUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommodityAssessmentSplitContainer);
			this.Name = "CommodityAssessmentPanelUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 185, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommodityDetailsGroupBox.ResumeLayout(false);
			this.CommodityDetailsGroupBox.PerformLayout();
			this.DetailGroupBoxTableLayoutPanel.ResumeLayout(false);
			this.DetailGroupBoxTableLayoutPanel.PerformLayout();
			this.TopBannerPanel.ResumeLayout(false);
			this.TopBannerPanel.PerformLayout();
			this.CommodityAssessmentSplitContainer.Panel1.ResumeLayout(false);
			this.CommodityAssessmentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CommodityAssessmentSplitContainer)).EndInit();
			this.CommodityAssessmentSplitContainer.ResumeLayout(false);
			this.CommodityAssessmentSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CommodityAssessmentGroupBox;
		private ZArchitecture.GUI.ZGroupBox CommodityDetailsGroupBox;
		private CargoWise.Windows.UI.KSplitContainer CommodityAssessmentSplitContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel TopBannerTableLayoutPanel;
		private ZArchitecture.GUI.ZToolStripMenuItem HideShowMenuItem;
		private ZArchitecture.GUI.ZToolStrip ToolStripItem;
		internal ZArchitecture.GUI.ZFilterStripBaseControl CommodityFilterStrip;
		private CargoWise.Windows.UI.KTableLayoutPanel DetailGroupBoxTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel FilterPanel;
		private ZArchitecture.GUI.ZPanel TopBannerPanel;
		private ZArchitecture.ZLabel FilterCountInfoLabel;
		private ZArchitecture.ZLabel ActiveFilterCountInfoLabel;
		private ZArchitecture.GUI.ZToolStrip RemoveFiltersToolStrip;
		private ZArchitecture.GUI.ZToolStripButton RemoveFiltersButton;
		internal ToolStripItem HideShowToolStripItem;
		private ZToolStrip Toolstrip;
	}
}
