namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePackedItemGridControl
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
			this.PackedItemGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackedItemGrid)).BeginInit();
			this.PackedItemGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// PackedItemGrid
			// 
			this.PackedItemGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackedItemGrid, "Bills.PackedItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).PackedItems)));
			this.PackedItemGrid.CaptionVisible = false;
			this.PackedItemGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackedItemGrid.GridId = "27113bdb-f200-4db4-8aae-97ffd253ae06";
			this.PackedItemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackedItemGrid.LayoutKey = "zGrid1";
			this.PackedItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackedItemGrid.Name = "PackedItemGrid";
			this.PackedItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 305, true);
			this.PackedItemGrid.TabIndex = 0;
			// 
			// UCC6TemporaryStoragePackedItemGridControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackedItemGrid);
			this.Name = "UCC6TemporaryStoragePackedItemGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 305, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackedItemGrid)).EndInit();
			this.PackedItemGrid.ResumeLayout(false);
			this.PackedItemGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZGrid PackedItemGrid;
	}
}
