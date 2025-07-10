using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCargoDeclarationUserControl
	{
		private void InitializeComponent()
		{
			this.airCargoDeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.airCargoMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.airCargoMessageUserControl = new Enterprise.Customs.AU.AirCargo.GUI.AirCargoMessageUserControl();
			this.airCargoTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.airCargoEventTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.airCargoDeclarationTabPage.SuspendLayout();
			this.airCargoMessagesTabPage.SuspendLayout();
			this.airCargoTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// AirCargoDeclarationTabPage
			// 
			this.airCargoDeclarationTabPage.CheckForNotifications = true;
			this.airCargoDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.airCargoDeclarationTabPage.Name = "AirCargoDeclarationTabPage";
			this.airCargoDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 533, true);
			this.airCargoDeclarationTabPage.TabIndex = 0;
			this.airCargoDeclarationTabPage.Text = "Declaration";
			// 
			// AirCargoMessagesTabPage
			// 
			this.airCargoMessagesTabPage.CheckForNotifications = true;
			this.airCargoMessagesTabPage.Controls.Add(this.airCargoMessageUserControl);
			this.airCargoMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.airCargoMessagesTabPage.Name = "AirCargoMessagesTabPage";
			this.airCargoMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 533, true);
			this.airCargoMessagesTabPage.TabIndex = 1;
			this.airCargoMessagesTabPage.Text = "Messages";
			// 
			// AirCargoMessageUserControl
			// 
			this.airCargoMessageUserControl.DataSourceAssemblyName = "";
			this.airCargoMessageUserControl.DataSourceTypeName = "";
			this.airCargoMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.airCargoMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.airCargoMessageUserControl.Name = "AirCargoMessageUserControl";
			this.airCargoMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 533, true);
			this.airCargoMessageUserControl.TabIndex = 0;
			// 
			// AirCargoTabControl
			// 
			this.airCargoTabControl.Controls.Add(this.airCargoDeclarationTabPage);
			this.airCargoTabControl.Controls.Add(this.airCargoMessagesTabPage);
			this.airCargoTabControl.Controls.Add(this.airCargoEventTabPage);
			this.airCargoTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.airCargoTabControl.ItemSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 19, true);
			this.airCargoTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.airCargoTabControl.Name = "AirCargoTabControl";
			this.airCargoTabControl.SelectedIndex = 0;
			this.airCargoTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 560, true);
			this.airCargoTabControl.TabIndex = 0;
			// 
			// AirCargoEventTabPage
			// 
			this.airCargoEventTabPage.CheckForNotifications = true;
			this.airCargoEventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.airCargoEventTabPage.Name = "AirCargoEventTabPage";
			this.airCargoEventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 533, true);
			this.airCargoEventTabPage.TabIndex = 2;
			// 
			// AirCargoDeclarationUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.airCargoTabControl);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusMAWB";
			this.Name = "AirCargoDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 560, true);
			this.airCargoDeclarationTabPage.ResumeLayout(false);
			this.airCargoMessagesTabPage.ResumeLayout(false);
			this.airCargoTabControl.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZTabPage airCargoDeclarationTabPage;
		private ZTabPage airCargoMessagesTabPage;
		private AirCargoMessageUserControl airCargoMessageUserControl;
		private ZTemplateTabControl airCargoTabControl;
		private ZLogsTabPage airCargoEventTabPage;
	}
}
