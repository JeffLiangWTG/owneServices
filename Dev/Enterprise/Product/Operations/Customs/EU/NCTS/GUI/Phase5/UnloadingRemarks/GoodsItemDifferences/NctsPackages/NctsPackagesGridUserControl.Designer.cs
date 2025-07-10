namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class NctsPackagesGridUserControl
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
            this.NctsPackagesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NctsPackagesGrid)).BeginInit();
            this.NctsPackagesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPackage);
            // 
            // NctsPackagesGrid
            // 
            this.NctsPackagesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.NctsPackagesGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)))));
            this.NctsPackagesGrid.CaptionVisible = false;
            this.NctsPackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NctsPackagesGrid.GridId = "c88170de-168a-4e2d-a5f1-5ee1b084c477";
            this.NctsPackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.NctsPackagesGrid.LayoutKey = "zGrid1";
            this.NctsPackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.NctsPackagesGrid.Name = "NctsPackagesGrid";
            this.NctsPackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
            this.NctsPackagesGrid.TabIndex = 0;
            // 
            // NctsPackagesGridUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.NctsPackagesGrid);
            this.Name = "NctsPackagesGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NctsPackagesGrid)).EndInit();
            this.NctsPackagesGrid.ResumeLayout(false);
            this.NctsPackagesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid NctsPackagesGrid;
	}
}
