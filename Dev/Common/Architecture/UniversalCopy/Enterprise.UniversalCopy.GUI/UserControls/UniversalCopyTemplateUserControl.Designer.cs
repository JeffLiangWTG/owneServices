using System.Windows.Forms;

namespace Enterprise.UniversalCopy.GUI
{
	partial class UniversalCopyTemplateUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.panelPreview = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.linkLabelShowPreview = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.groupBoxTree = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.templateTreeView = new Enterprise.UniversalCopy.GUI.UniversalCopyTemplateTreeView();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.groupBoxTree.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.UniversalCopyTemplate);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel1MinSize = 500;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.groupBoxTree);
			this.splitContainer1.Panel2MinSize = 150;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 750, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(700);
			this.splitContainer1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.panelPreview);
			this.splitContainer2.Panel1.Controls.Add(this.linkLabelShowPreview);
			this.splitContainer2.Panel1MinSize = 20;
			this.splitContainer2.Panel2MinSize = 150;
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 750, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(24);
			this.splitContainer2.TabIndex = 0;
			// 
			// panelPreview
			// 
			this.panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelPreview.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.panelPreview.Name = "panelPreview";
			this.panelPreview.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 4, true);
			this.panelPreview.TabIndex = 1;
			this.panelPreview.Visible = false;
			// 
			// linkLabelShowPreview
			// 
			this.linkLabelShowPreview.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("0309a3b4-a6a2-4bda-9dc6-1376c07ba4b2", "Show/hide preview controls");
			this.linkLabelShowPreview.Dock = System.Windows.Forms.DockStyle.Top;
			this.linkLabelShowPreview.IsFontBold = false;
			this.linkLabelShowPreview.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.linkLabelShowPreview.Name = "linkLabelShowPreview";
			this.linkLabelShowPreview.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 20, true);
			this.linkLabelShowPreview.TabIndex = 0;
			this.linkLabelShowPreview.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.linkLabelShowPreview.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelShowPreviewClicked);
			// 
			// groupBoxTree
			// 
			this.groupBoxTree.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("fe49d806-6bf5-4e1b-baff-5f147a8fe5e4", "Related Records");
			this.groupBoxTree.Controls.Add(this.templateTreeView);
			this.groupBoxTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxTree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxTree.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.groupBoxTree.Name = "groupBoxTree";
			this.groupBoxTree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 750, true);
			this.groupBoxTree.TabIndex = 2;
			this.groupBoxTree.TabStop = false;
			// 
			// templateTreeView
			// 
			this.templateTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.templateTreeView.HideSelection = false;
			this.templateTreeView.ImageIndex = 0;
			this.templateTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.templateTreeView.Name = "templateTreeView";
			this.templateTreeView.SelectedImageIndex = 0;
			this.templateTreeView.ShowNodeToolTips = true;
			this.templateTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 731, true);
			this.templateTreeView.Sorted = true;
			this.templateTreeView.TabIndex = 0;
			this.templateTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.templateTreeView_AfterSelect);
			// 
			// UniversalCopyTemplateUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "UniversalCopyTemplateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 750, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.groupBoxTree.ResumeLayout(false);
			this.groupBoxTree.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZGroupBox groupBoxTree;
		internal UniversalCopyTemplateTreeView templateTreeView;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private ZArchitecture.GUI.ZLinkLabel linkLabelShowPreview;
		private Enterprise.ZArchitecture.GUI.ZPanel panelPreview;
	}
}
