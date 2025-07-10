using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public partial class AirCTOUserControl
	{
		private void InitializeComponent()
		{
			this.airCTOMainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.flightDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.airCTOUserDetailsControl1 = new Enterprise.Customs.AU.AirCargo.GUI.AirCTOUserDetailsControl();
			this.flightOutturnTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.flightOutturnUserControl = new Enterprise.Customs.GUI.CusOutturnUserControl();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.airCTOMainTabControl.SuspendLayout();
			this.flightDetailsTabPage.SuspendLayout();
			this.flightOutturnTabPage.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// AirCTOMainTabControl
			// 
			this.airCTOMainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.airCTOMainTabControl.Controls.Add(this.flightDetailsTabPage);
			this.airCTOMainTabControl.Controls.Add(this.flightOutturnTabPage);
			this.airCTOMainTabControl.Controls.Add(this.messagesTabPage);
			this.airCTOMainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.airCTOMainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.airCTOMainTabControl.Name = "AirCTOMainTabControl";
			this.airCTOMainTabControl.SelectedIndex = 0;
			this.airCTOMainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 656, true);
			this.airCTOMainTabControl.TabIndex = 0;
			// 
			// FlightDetailsTabPage
			// 
			this.flightDetailsTabPage.CheckForNotifications = true;
			this.flightDetailsTabPage.Controls.Add(this.airCTOUserDetailsControl1);
			this.flightDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.flightDetailsTabPage.Name = "FlightDetailsTabPage";
			this.flightDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 629, true);
			this.flightDetailsTabPage.TabIndex = 0;
			this.flightDetailsTabPage.Text = "Flight Details";
			// 
			// airCTOUserDetailsControl1
			// 
			this.airCTOUserDetailsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.airCTOUserDetailsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.airCTOUserDetailsControl1.Name = "airCTOUserDetailsControl1";
			this.airCTOUserDetailsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 629, true);
			this.airCTOUserDetailsControl1.TabIndex = 0;
			// 
			// FlightOutturnTabPage
			// 
			this.flightOutturnTabPage.CheckForNotifications = true;
			this.flightOutturnTabPage.Controls.Add(this.flightOutturnUserControl);
			this.flightOutturnTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.flightOutturnTabPage.Name = "FlightOutturnTabPage";
			this.flightOutturnTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 629, true);
			this.flightOutturnTabPage.TabIndex = 1;
			this.flightOutturnTabPage.Text = "Outturn Report";
			// 
			// FlightOutturnUserControl
			// 
			this.flightOutturnUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flightOutturnUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.flightOutturnUserControl.Name = "FlightOutturnUserControl";
			this.flightOutturnUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 629, true);
			this.flightOutturnUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.CheckForNotifications = true;
			this.messagesTabPage.Controls.Add(this.messagesUserControl);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 629, true);
			this.messagesTabPage.TabIndex = 2;
			this.messagesTabPage.Text = "Outturn Messages";
			// 
			// MessagesUserControl
			// 
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesUserControl.Name = "MessagesUserControl";
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 629, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// AirCTOUserControl
			// 
			this.Controls.Add(this.airCTOMainTabControl);
			this.Name = "AirCTOUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 656, true);
			this.airCTOMainTabControl.ResumeLayout(false);
			this.flightDetailsTabPage.ResumeLayout(false);
			this.flightOutturnTabPage.ResumeLayout(false);
			this.messagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		internal ZTemplateTabControl airCTOMainTabControl;
		private ZTabPage flightDetailsTabPage;
		private AirCTOUserDetailsControl airCTOUserDetailsControl1;
		private Customs.GUI.CusOutturnUserControl flightOutturnUserControl;
		private ZTabPage messagesTabPage;
		internal Messaging.GUI.EDIMessageUserControl messagesUserControl;
		private ZTabPage flightOutturnTabPage;
	}
}
