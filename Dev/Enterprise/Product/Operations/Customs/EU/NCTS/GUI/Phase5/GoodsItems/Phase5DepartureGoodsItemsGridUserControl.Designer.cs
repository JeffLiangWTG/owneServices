namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DepartureGoodsItemsGridUserControl
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
			this.GoodsItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsGrid)).BeginInit();
			this.GoodsItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsDepartureCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc>);
			// 
			// GoodsItemsGrid
			// 
			this.GoodsItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GoodsItemsGrid, ".");
			this.GoodsItemsGrid.CaptionVisible = false;
			this.GoodsItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsGrid.GridId = "030D711A-C2A6-4E7C-9129-50EB5A4FA7E3";
			this.GoodsItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GoodsItemsGrid.LayoutKey = "GoodsItemsGrid";
			this.GoodsItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsGrid.Name = "GoodsItemsGrid";
			this.GoodsItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
			this.GoodsItemsGrid.TabIndex = 0;
			// 
			// Phase5DepartureGoodsItemsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsItemsGrid);
			this.Name = "Phase5DepartureGoodsItemsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsGrid)).EndInit();
			this.GoodsItemsGrid.ResumeLayout(false);
			this.GoodsItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid GoodsItemsGrid;
	}
}

