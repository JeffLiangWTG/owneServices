namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GuaranteesUserControl
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

		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).BeginInit();
			this.GuaranteesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// GuaranteesGrid
			// 
			this.BindingSource.SetBindingMember(this.GuaranteesGrid, "Guarantees");
			this.GuaranteesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 218, true);
			this.GuaranteesGrid.AfterBind += new System.EventHandler(this.GuaranteesGrid_AfterBind);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// Phase5GuaranteesUserControl
			// 
			this.Name = "Phase5GuaranteesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 218, true);
			((System.ComponentModel.ISupportInitialize)(this.GuaranteesGrid)).EndInit();
			this.GuaranteesGrid.ResumeLayout(false);
			this.GuaranteesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
