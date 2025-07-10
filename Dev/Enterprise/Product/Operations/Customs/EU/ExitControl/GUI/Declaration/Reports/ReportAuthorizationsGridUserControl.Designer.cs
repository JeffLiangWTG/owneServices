namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportAuthorizationsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AuthorizationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorizationsGrid)).BeginInit();
			this.AuthorizationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.ICusAuthorizationUsageCollection<Enterprise.Customs.EU.ExitControl.Business.CusAuthorizationUsage, Enterprise.Customs.EU.ExitControl.Business.CusExitReport>);
			// 
			// AuthorizationsGrid
			// 
			this.AuthorizationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorizationsGrid, ".");
			this.AuthorizationsGrid.CaptionVisible = false;
			this.AuthorizationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorizationsGrid.GridId = "6aa6f6a3-af6e-4d5e-991f-7a9d09eab8c9";
			this.AuthorizationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorizationsGrid.LayoutKey = "AuthorizationsGrid";
			this.AuthorizationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AuthorizationsGrid.Name = "AuthorizationsGrid";
			this.AuthorizationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 90, true);
			this.AuthorizationsGrid.TabIndex = 0;
			// 
			// ReportAuthorizationsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AuthorizationsGrid);
			this.Name = "ReportAuthorizationsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 90, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AuthorizationsGrid)).EndInit();
			this.AuthorizationsGrid.ResumeLayout(false);
			this.AuthorizationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid AuthorizationsGrid;
	}
}

