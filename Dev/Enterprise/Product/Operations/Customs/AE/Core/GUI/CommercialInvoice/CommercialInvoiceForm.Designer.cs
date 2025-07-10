using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.GUI
{
	public partial class CommercialInvoiceForm
	{
		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 608, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 604, true);
			// 
			// HeaderTabPage
			// 
			this.HeaderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 577, true);
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 578, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 637, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 22, true);
			// 
			// CommercialInvoiceForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 659, true);
			this.Name = "CommercialInvoiceForm";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		readonly System.ComponentModel.Container components = null;
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
	}
}
