using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TNT.NZ
{
	public partial class QuantumNZForm : Exit2ImportForm
	{
		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseConsolsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.QuantumMawbsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ProcessButton
			// 
			this.ProcessButton.ReadOnly = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 563, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 26, true);
			// 
			// QuantumNZForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 589, true);
			this.Name = "QuantumNZForm";
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseConsolsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.QuantumMawbsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
