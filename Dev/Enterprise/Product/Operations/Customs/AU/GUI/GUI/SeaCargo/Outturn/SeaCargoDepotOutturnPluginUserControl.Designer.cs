using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class SeaCargoDepotOutturnPluginUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SeaCargoDepotOutturnUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoDepotOutturnUserControl();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.OutturnsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EDIMessageUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.TabControl.SuspendLayout();
			this.OutturnsTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SeaCargoDepotOutturnUserControl
			// 
			this.SeaCargoDepotOutturnUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SeaCargoDepotOutturnUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SeaCargoDepotOutturnUserControl.Name = "SeaCargoDepotOutturnUserControl";
			this.SeaCargoDepotOutturnUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 578, true);
			this.SeaCargoDepotOutturnUserControl.TabIndex = 0;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.OutturnsTabPage);
			this.TabControl.Controls.Add(this.MessagesTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 611, true);
			this.TabControl.TabIndex = 1;
			// 
			// OutturnsTabPage
			// 
			this.OutturnsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.OutturnsTabPage.CheckForNotifications = true;
			this.OutturnsTabPage.Controls.Add(this.SeaCargoDepotOutturnUserControl);
			this.OutturnsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OutturnsTabPage.Name = "OutturnsTabPage";
			this.OutturnsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OutturnsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 584, true);
			this.OutturnsTabPage.TabIndex = 0;
			this.OutturnsTabPage.Text = "Outturns";
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessagesTabPage.CheckForNotifications = true;
			this.MessagesTabPage.Controls.Add(this.EDIMessageUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 584, true);
			this.MessagesTabPage.TabIndex = 1;
			this.MessagesTabPage.Text = "Messages";
			// 
			// EDIMessageUserControl
			// 
			this.EDIMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDIMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EDIMessageUserControl.Name = "EDIMessageUserControl";
			this.EDIMessageUserControl.ShowChangingBlueMessageHeading = false;
			this.EDIMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(995, 578, true);
			this.EDIMessageUserControl.TabIndex = 0;
			// 
			// SeaCargoDepotOutturnPluginUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TabControl);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.Name = "SeaCargoDepotOutturnPluginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 611, true);
			this.TabControl.ResumeLayout(false);
			this.OutturnsTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SeaCargoDepotOutturnUserControl SeaCargoDepotOutturnUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl TabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage OutturnsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private System.ComponentModel.IContainer components;
		private Enterprise.Messaging.GUI.EDIMessageUserControl EDIMessageUserControl;
	}
}
