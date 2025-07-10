namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentsGridUserControl
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
			this.HouseConsignmentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentsGrid)).BeginInit();
			this.HouseConsignmentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsBillCollection<Enterprise.Customs.EU.NCTS.Business.NctsBill>);
			// 
			// HouseConsignmentsGrid
			// 
			this.HouseConsignmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseConsignmentsGrid, ".");
			this.HouseConsignmentsGrid.CaptionVisible = false;
			this.HouseConsignmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentsGrid.GridId = "C8C15F5D-7144-4065-8104-B91944C96DB9";
			this.HouseConsignmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseConsignmentsGrid.LayoutKey = "HouseConsignmentsGrid";
			this.HouseConsignmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentsGrid.Name = "HouseConsignmentsGrid";
			this.HouseConsignmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 250, true);
			this.HouseConsignmentsGrid.TabIndex = 0;
			// 
			// HouseConsignmentsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseConsignmentsGrid);
			this.Name = "HouseConsignmentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentsGrid)).EndInit();
			this.HouseConsignmentsGrid.ResumeLayout(false);
			this.HouseConsignmentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid HouseConsignmentsGrid;
	}
}

