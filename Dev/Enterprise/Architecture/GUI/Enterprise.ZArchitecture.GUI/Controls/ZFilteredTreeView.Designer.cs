namespace Enterprise.ZArchitecture.GUI
{
	partial class ZFilteredTreeView
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
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.FilterTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DisplayTree = new CargoWise.Windows.UI.KTreeView();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.IsSplitterFixed = true;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.splitContainer1);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.DisplayTree);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 160, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(25);
			this.splitContainer.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.FilterTextBox);
			this.splitContainer1.Panel2Collapsed = true;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 25, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(308);
			this.splitContainer1.TabIndex = 1;
			// 
			// FilterTextBox
			// 
			this.FilterTextBox.CaptionResourceString = null;
			this.FilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterTextBox.Name = "FilterTextBox";
			this.FilterTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 20, true);
			this.FilterTextBox.TabIndex = 0;
			// 
			// DisplayTree
			// 
			this.DisplayTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DisplayTree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DisplayTree.Name = "DisplayTree";
			this.DisplayTree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 131, true);
			this.DisplayTree.TabIndex = 0;
			// 
			// ZFilteredTreeView
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Name = "ZFilteredTreeView";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer splitContainer;
		public ZTextBox FilterTextBox;
		public CargoWise.Windows.UI.KTreeView DisplayTree;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
	}
}
