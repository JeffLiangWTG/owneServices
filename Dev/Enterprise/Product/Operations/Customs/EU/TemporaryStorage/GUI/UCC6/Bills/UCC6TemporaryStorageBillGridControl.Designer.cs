using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI;

partial class UCC6TemporaryStorageBillGridControl
{
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.TemporaryStorageBillGrid = new Enterprise.ZArchitecture.ZGrid();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.TemporaryStorageBillGrid)).BeginInit();
		this.TemporaryStorageBillGrid.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
		// 
		// BillsGrid
		// 
		this.TemporaryStorageBillGrid.AllowNavigation = false;
		this.BindingSource.SetBindingMember(this.TemporaryStorageBillGrid, "Bills");
		this.TemporaryStorageBillGrid.CaptionVisible = false;
		this.TemporaryStorageBillGrid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.TemporaryStorageBillGrid.GridId = "59501349-2109-4434-a213-82e3e2b41a22";
		this.TemporaryStorageBillGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.TemporaryStorageBillGrid.LayoutKey = "BillsGrid";
		this.TemporaryStorageBillGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
		this.TemporaryStorageBillGrid.Name = "BillsGrid";
		this.TemporaryStorageBillGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
		this.TemporaryStorageBillGrid.TabIndex = 0;
		// 
		// UCC6TemporaryStorageBillGridControl
		// 
		this.CaptionRenderingEnabled = true;
		this.Controls.Add(this.TemporaryStorageBillGrid);
		this.Name = "UCC6TemporaryStorageBillGridControl";
		this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.TemporaryStorageBillGrid)).EndInit();
		this.TemporaryStorageBillGrid.ResumeLayout(false);
		this.TemporaryStorageBillGrid.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZGrid TemporaryStorageBillGrid;
}
