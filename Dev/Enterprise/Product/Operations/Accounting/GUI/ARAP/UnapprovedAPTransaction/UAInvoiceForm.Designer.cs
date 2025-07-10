using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class UAInvoiceForm
	{
new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 649, true);
			// 
			// InvoiceDetailsTabPage
			// 
			this.InvoiceDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 622, true);
			this.InvoiceDetailsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.InvoiceDetailsTabPage_InitializeTab));
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 649, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 627, true);
			// 
			// UAInvoiceForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 686, true);
			this.Name = "UAInvoiceForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}