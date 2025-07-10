namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDeclarationUserControl
	{
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
		private void InitializeComponent()
		{
			this.SeaCargoBoundTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.TabPageDeclaration = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SeaCargoShipmentUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoShipmentUserControl();
			this.TabPageMessages = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.seaCargoMessageUserControl1 = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoMessageUserControl();
			this.SeaCargoBoundTabControl.SuspendLayout();
			this.TabPageDeclaration.SuspendLayout();
			this.TabPageMessages.SuspendLayout();
			this.SuspendLayout();
			// 
			// SeaCargoBoundTabControl
			// 
			this.SeaCargoBoundTabControl.Controls.Add(this.TabPageDeclaration);
			this.SeaCargoBoundTabControl.Controls.Add(this.TabPageMessages);
			this.SeaCargoBoundTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SeaCargoBoundTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SeaCargoBoundTabControl.Name = "SeaCargoBoundTabControl";
			this.SeaCargoBoundTabControl.SelectedIndex = 0;
			this.SeaCargoBoundTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 608, true);
			this.SeaCargoBoundTabControl.TabIndex = 0;
			// 
			// TabPageDeclaration
			// 
			this.TabPageDeclaration.CheckForNotifications = true;
			this.TabPageDeclaration.Controls.Add(this.SeaCargoShipmentUserControl);
			this.TabPageDeclaration.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TabPageDeclaration.Name = "TabPageDeclaration";
			this.TabPageDeclaration.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 581, true);
			this.TabPageDeclaration.TabIndex = 0;
			this.TabPageDeclaration.Text = "Declaration";
			// 
			// SeaCargoShipmentUserControl
			// 
			this.SeaCargoShipmentUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SeaCargoShipmentUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SeaCargoShipmentUserControl.Name = "SeaCargoShipmentUserControl";
			this.SeaCargoShipmentUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 581, true);
			this.SeaCargoShipmentUserControl.TabIndex = 0;
			// 
			// TabPageMessages
			// 
			this.TabPageMessages.CheckForNotifications = true;
			this.TabPageMessages.Controls.Add(this.seaCargoMessageUserControl1);
			this.TabPageMessages.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TabPageMessages.Name = "TabPageMessages";
			this.TabPageMessages.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 461, true);
			this.TabPageMessages.TabIndex = 1;
			this.TabPageMessages.Text = "Messages";
			// 
			// seaCargoMessageUserControl1
			// 
			this.seaCargoMessageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seaCargoMessageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.seaCargoMessageUserControl1.Name = "seaCargoMessageUserControl1";
			this.seaCargoMessageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 461, true);
			this.seaCargoMessageUserControl1.TabIndex = 0;
			// 
			// SeaCargoDeclarationUserControl
			//
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.SeaCargoBoundTabControl);
			this.Name = "SeaCargoDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 608, true);
			this.SeaCargoBoundTabControl.ResumeLayout(false);
			this.TabPageDeclaration.ResumeLayout(false);
			this.TabPageMessages.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl SeaCargoBoundTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage TabPageDeclaration;
		private Enterprise.ZArchitecture.GUI.ZTabPage TabPageMessages;
		private Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoMessageUserControl seaCargoMessageUserControl1;
		private Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoShipmentUserControl SeaCargoShipmentUserControl;
		private System.ComponentModel.Container components = null;
	}
}
