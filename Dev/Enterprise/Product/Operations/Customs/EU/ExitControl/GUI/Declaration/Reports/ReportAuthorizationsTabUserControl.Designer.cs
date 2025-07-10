namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportAuthorizationsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReportAuthorizationsGridUserControl = new Enterprise.Customs.EU.ExitControl.GUI.ReportAuthorizationsGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReportAuthorizationsGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.ICusAuthorizationUsageCollection<Enterprise.Customs.EU.Business.CusAuthorizationUsage, Enterprise.Customs.EU.ExitControl.Business.CusExitReport>);
			// 
			// ReportAuthorizationsGridUserControl
			// 
			this.ReportAuthorizationsGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReportAuthorizationsGridUserControl, ".");
			this.ReportAuthorizationsGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportAuthorizationsGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportAuthorizationsGridUserControl.Name = "ReportAuthorizationsGridUserControl";
			this.ReportAuthorizationsGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 365, true);
			this.ReportAuthorizationsGridUserControl.TabIndex = 0;
			// 
			// ReportAuthorizationsTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReportAuthorizationsGridUserControl);
			this.Name = "ReportAuthorizationsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReportAuthorizationsGridUserControl.ResumeLayout(true);
			this.ReportAuthorizationsGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ReportAuthorizationsGridUserControl ReportAuthorizationsGridUserControl;
	}
}
