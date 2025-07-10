namespace Enterprise.Customs.FR.GUI
{
	public partial class EntryInstructionGridUserControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntryInstructionsGrid
			// 
			this.EntryInstructionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 234, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// EntryInstructionGridUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "EntryInstructionGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(708, 234, true);
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
			this.EntryInstructionsGrid.ResumeLayout(false);
			this.EntryInstructionsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
