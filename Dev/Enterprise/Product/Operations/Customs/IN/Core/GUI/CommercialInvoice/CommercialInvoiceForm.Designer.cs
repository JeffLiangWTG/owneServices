namespace Enterprise.Customs.IN.GUI;

partial class CommercialInvoiceForm
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
		if (disposing)
		{
			components?.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private new void InitializeComponent()
	{
		this.PostingButtonsUserControl.SuspendLayout();
		this.MainTabControl.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SuspendLayout();
		// 
		// PostingButtonsUserControl
		// 
		this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 633, true);
		this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1109, 25, true);
		// 
		// MainTabControl
		// 
		this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 623, true);
		// 
		// HeaderTabPage
		// 
		this.HeaderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.HeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 601, true);
		// 
		// LinesTabPage
		// 
		this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
		this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1003, 601, true);
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 658, true);
		// 
		// CommercialInvoiceForm
		// 
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1108, 680, true);
		this.Name = "CommercialInvoiceForm";
		this.PostingButtonsUserControl.ResumeLayout(true);
		this.PostingButtonsUserControl.PerformLayout();
		this.MainTabControl.ResumeLayout(false);
		this.MainTabControl.PerformLayout();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion
}
