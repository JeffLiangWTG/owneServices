namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemDifferencesGridUserControl
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
            this.GoodsItemDifferencesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GoodsItemDifferencesGrid)).BeginInit();
            this.GoodsItemDifferencesGrid.SuspendLayout();
            this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc);
			// 
			// GoodsItemDifferencesGrid
			// 
			this.GoodsItemDifferencesGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.GoodsItemDifferencesGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            this.GoodsItemDifferencesGrid.CaptionVisible = false;
            this.GoodsItemDifferencesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GoodsItemDifferencesGrid.GridId = "3956fbd5-eb79-4bb0-b9a4-8cd167743616";
            this.GoodsItemDifferencesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GoodsItemDifferencesGrid.LayoutKey = "GoodsItemDifferencesGrid";
            this.GoodsItemDifferencesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GoodsItemDifferencesGrid.Name = "GoodsItemDifferencesGrid";
            this.GoodsItemDifferencesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
            this.GoodsItemDifferencesGrid.TabIndex = 0;
            // 
            // Phase5GoodsItemDifferencesGridUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GoodsItemDifferencesGrid);
            this.Name = "Phase5GoodsItemDifferencesGridUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GoodsItemDifferencesGrid)).EndInit();
            this.GoodsItemDifferencesGrid.ResumeLayout(false);
            this.GoodsItemDifferencesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZArchitecture.ZGrid GoodsItemDifferencesGrid;

		#endregion
	}
}
