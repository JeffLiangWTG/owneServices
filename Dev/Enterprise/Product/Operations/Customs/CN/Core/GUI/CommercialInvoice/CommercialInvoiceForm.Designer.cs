namespace Enterprise.Customs.CN.GUI.CommercialInvoice
{
	public partial class CommercialInvoiceForm
	{
		#region Designer Generated Code

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		System.ComponentModel.Container components = null;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Location =
				CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(861, 631, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1201, 625, true);
			// 
			// HeaderTabPage
			// 
			this.HeaderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 21, true);
			this.HeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 601, true);
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 21, true);
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 601, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			// 
			// CommercialInvoiceForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 681, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 681, true);
			this.Name = "CommercialInvoiceForm";
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
