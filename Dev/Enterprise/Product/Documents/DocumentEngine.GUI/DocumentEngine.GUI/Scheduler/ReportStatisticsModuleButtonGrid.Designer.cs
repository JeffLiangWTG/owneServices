using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class ReportStatisticsModuleButtonGrid : ZModuleButtonGrid
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Scheduler.Business.StmReportRunCollection);
			// 
			// ForwardingShipmentModuleButtonGrid
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ReportStatisticsModuleButtonGrid";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
