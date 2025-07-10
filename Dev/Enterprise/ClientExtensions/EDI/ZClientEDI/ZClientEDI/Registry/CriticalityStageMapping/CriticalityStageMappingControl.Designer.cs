namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class CriticalityStageMappingControl
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
			this.GroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Grid2 = new Enterprise.Client.EDI.Registry.GUI.CriticalityStageMappingGridControl();
			this.GroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.Grid1 = new Enterprise.Client.EDI.Registry.GUI.CriticalityStageMappingGridControl();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox2.SuspendLayout();
			this.GroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.CriticalityStageMappingCollection);
			// 
			// GroupBox2
			// 
			this.GroupBox2.Controls.Add(this.Grid2);
			this.GroupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox2.Name = "GroupBox2";
			this.GroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 255, true);
			this.GroupBox2.TabIndex = 0;
			this.GroupBox2.TabStop = false;
			// 
			// Grid2
			// 
			this.Grid2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Grid2, ".");
			this.Grid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.Grid2.Name = "Grid2";
			this.Grid2.ReadOnly = false;
			this.Grid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 236, true);
			this.Grid2.TabIndex = 0;
			// 
			// GroupBox1
			// 
			this.GroupBox1.Controls.Add(this.Grid1);
			this.GroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox1.Name = "GroupBox1";
			this.GroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 200, true);
			this.GroupBox1.TabIndex = 0;
			this.GroupBox1.TabStop = false;
			// 
			// Grid1
			// 
			this.Grid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Grid1, ".");
			this.Grid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.Grid1.Name = "Grid1";
			this.Grid1.ReadOnly = false;
			this.Grid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 181, true);
			this.Grid1.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.GroupBox1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.GroupBox2);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 459, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.splitContainer1.TabIndex = 5;
			// 
			// CriticalityStageMappingControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.splitContainer1);
			this.Name = "CriticalityStageMappingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 459, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox2.ResumeLayout(false);
			this.GroupBox1.ResumeLayout(false);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox GroupBox1;
		private ZArchitecture.GUI.ZGroupBox GroupBox2;
		private CriticalityStageMappingGridControl Grid1;
		private CriticalityStageMappingGridControl Grid2;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
	}
}
