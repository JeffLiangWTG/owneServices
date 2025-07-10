using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ImportClassificationForm
	{
		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 328, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 301, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(281);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(281);
			// 
			// ImportClassificationForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(562, 384, true);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.Classification";
			this.MinimizeBox = false;
			this.Name = "ImportClassificationForm";
			this.Text = "Import Classification Lookup";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		}
	}
}
