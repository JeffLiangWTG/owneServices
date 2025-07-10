using System;
using Aga.Controls.Tree;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZTreeViewControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZTreeViewControl));
			this.mainPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.Tree = CreateNewTreeViewAdv();
			this.bottomToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.detachToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.attachToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.editToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.newToolStripButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.Panel2.SuspendLayout();
			this.bottomToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainPanel
			// 
			this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.mainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainPanel.Name = "mainPanel";
			this.mainPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainPanel.Panel1
			// 
			this.mainPanel.Panel1.Controls.Add(this.Tree);
			this.mainPanel.Panel2Collapsed = true;
			this.mainPanel.Panel2MinSize = 0;
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 370, true);
			this.mainPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(150);
			this.mainPanel.TabIndex = 2;
			// 
			// Tree
			// 
			this.Tree.AllowColumnReorder = true;
			this.Tree.AllowDrop = true;
			this.Tree.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Tree.AutoRowHeight = true;
			this.Tree.BackColor = System.Drawing.SystemColors.Window;
			this.Tree.DefaultToolTipProvider = null;
			this.Tree.DragDropMarkColor = System.Drawing.Color.Black;
			this.Tree.ElementType = null;
			this.Tree.LineColor = System.Drawing.SystemColors.ControlDark;
			this.Tree.LineDashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Tree.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 0, true);
			this.Tree.Model = null;
			this.Tree.Name = "Tree";
			this.Tree.SelectedNode = null;
			this.Tree.SelectionMode = Aga.Controls.Tree.TreeSelectionMode.MultiSameParent;
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 367, true);
			this.Tree.TabIndex = 3;
			this.Tree.UseColumns = true;
			this.Tree.NodeMouseClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this.TreeViewAdv_NodeMouseClick);
			this.Tree.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this.TreeViewAdv_NodeMouseDoubleClick);
			this.Tree.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Tree_KeyUp);
			// 
			// bottomToolStrip
			// 
			this.bottomToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bottomToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.bottomToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.bottomToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.bottomToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detachToolStripButton,
            this.attachToolStripButton,
            this.editToolStripButton,
            this.newToolStripButton});
			this.bottomToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 371, true);
			this.bottomToolStrip.Name = "bottomToolStrip";
			this.bottomToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 1, 3, true);
			this.bottomToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 29, true);
			this.bottomToolStrip.TabIndex = 7;
			this.bottomToolStrip.Text = "bottomToolStrip";
			// 
			// detachToolStripButton
			// 
			this.detachToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.detachToolStripButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("973a58df-8e0e-4178-89d6-dd6fad12b957", "Detach");
			this.detachToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("detachToolStripButton.Image")));
			this.detachToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.detachToolStripButton.Name = "detachToolStripButton";
			this.detachToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.detachToolStripButton.Click += new System.EventHandler(this.DetachToolStripButton_Click);
			// 
			// attachToolStripButton
			// 
			this.attachToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.attachToolStripButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("d1808983-3e87-4567-a384-93f6e4299583", "Attach");
			this.attachToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("attachToolStripButton.Image")));
			this.attachToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.attachToolStripButton.Name = "attachToolStripButton";
			this.attachToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.attachToolStripButton.Click += new System.EventHandler(this.AttachToolStripButton_Click);
			// 
			// editToolStripButton
			// 
			this.editToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.editToolStripButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("26741a03-d5e0-4ca0-be90-251c3bc3f655", "Edit");
			this.editToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.editToolStripButton.Name = "editToolStripButton";
			this.editToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.editToolStripButton.Click += new System.EventHandler(this.EditToolStripButton_Click);
			// 
			// newToolStripButton
			// 
			this.newToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.newToolStripButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("b683e382-9dee-489f-bdfe-1c23848947af", "New");
			this.newToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripButton.Image")));
			this.newToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newToolStripButton.Name = "newToolStripButton";
			this.newToolStripButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.newToolStripButton.Click += new System.EventHandler(this.NewToolStripButton_Click);
			// 
			// ZTreeViewControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainPanel);
			this.Controls.Add(this.bottomToolStrip);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.Name = "ZTreeViewControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 400, true);
			this.Controls.SetChildIndex(this.mainPanel, 0);
			this.Controls.SetChildIndex(this.bottomToolStrip, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Tree.ResumeLayout(false);
			this.Tree.PerformLayout();
			this.bottomToolStrip.ResumeLayout(false);
			this.bottomToolStrip.PerformLayout();
			this.mainPanel.Panel1.ResumeLayout(false);
			this.mainPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZTreeViewAdv Tree;
		protected CargoWise.Windows.UI.KSplitContainer mainPanel;
		protected ZArchitecture.GUI.ZToolStrip bottomToolStrip;
		ZArchitecture.GUI.ZToolStripButton detachToolStripButton;
		ZArchitecture.GUI.ZToolStripButton attachToolStripButton;
		ZArchitecture.GUI.ZToolStripButton editToolStripButton;
		ZArchitecture.GUI.ZToolStripButton newToolStripButton;
	}
}
