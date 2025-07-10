using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.JAS.GUI
{
	public partial class JASConsolForm : ConsolForm, IJXCExportForm
	{
		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ConsolControl
			// 
			this.ConsolControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 611, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 611, true);
			// 
			// AWBTabPage
			// 
			this.AWBTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 611, true);
			// 
			// ElectronicMessagingTabControl
			// 
			this.ElectronicMessagingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 605, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 638, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 611, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 604, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 638, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 24, true);
			// 
			// AccountingTabPage
			// 
			this.AccountingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1092, 611, true);
			// 
			// JASConsolForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 694, true);
			this.Name = "JASConsolForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
