namespace Enterprise.Customs.IT.TemporaryStorage.Module;

partial class UCC6TemporaryStorageFilterControl
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
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
		((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
		this.grid.SuspendLayout();
		this.AddStripButton.SuspendLayout();
		this.RecentItemsPanel.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// grid
		//
		zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("0905ddf1-71c4-4f67-9c8b-8eeb175c7416", "Transport Type");
		zTextBoxColumnStyleInfo1.ColumnName = "TransportType";
		zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

		zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("d8996229-6f78-4f9c-b122-802ac110fb1b", "Transport ID");
		zTextBoxColumnStyleInfo2.ColumnName = "ArrivalTransportMeansCode";
		zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
		this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

		// 
		// UCC6TemporaryStorageFilterControl
		// 
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
		this.Name = "UCC6TemporaryStorageFilterControl";
		((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
		this.grid.ResumeLayout(false);
		this.grid.PerformLayout();
		this.AddStripButton.ResumeLayout(true);
		this.AddStripButton.PerformLayout();
		this.RecentItemsPanel.ResumeLayout(false);
		this.RecentItemsPanel.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();
		}

	#endregion
}
