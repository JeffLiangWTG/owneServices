namespace Enterprise.Customs.EU.Intrastat.GUI
{
	partial class TransactionLinesGridUserControl
	{
		private void InitializeComponent()
		{
			this.TransactionLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionLinesGrid)).BeginInit();
			this.TransactionLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.ICusIntrastatLineCollection<Enterprise.Customs.EU.Intrastat.Business.CusIntrastatLine>);
			// 
			// TransactionLinesGrid
			// 
			this.TransactionLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransactionLinesGrid, ".");
			this.TransactionLinesGrid.CaptionVisible = false;
			this.TransactionLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionLinesGrid.GridId = "030D711A-C2A6-4E7C-9129-50EB5A4FA7E3";
			this.TransactionLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionLinesGrid.LayoutKey = "TransactionLinesGrid";
			this.TransactionLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionLinesGrid.Name = "TransactionLinesGrid";
			this.TransactionLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 175, true);
			this.TransactionLinesGrid.TabIndex = 0;
			// 
			// Phase5DepartureTransactionLinesGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransactionLinesGrid);
			this.Name = "Phase5DepartureTransactionLinesGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 175, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionLinesGrid)).EndInit();
			this.TransactionLinesGrid.ResumeLayout(false);
			this.TransactionLinesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZGrid TransactionLinesGrid;
	}
}
