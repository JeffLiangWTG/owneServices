using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5NctsUserControlForPlugin
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ArrivalNotificationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DepartureDeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UnloadingRemarksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.IncidentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ServicesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TransportAndPackagingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseConsignmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MiscOptionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("DE69337E-1756-4AA1-BC01-C4562097D0F0", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.MessagesTabPage.TabIndex = 6;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// ArrivalNotificationTabPage
			// 
			this.ArrivalNotificationTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ArrivalNotificationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("F6320E04-4EB3-471C-8384-D1B65B08E3EE", "Arrival Notification");
			this.ArrivalNotificationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ArrivalNotificationTabPage.Name = "ArrivalNotificationTabPage";
			this.ArrivalNotificationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ArrivalNotificationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.ArrivalNotificationTabPage.TabIndex = 9;
			// 
			// DepartureDeclarationTabPage
			// 
			this.DepartureDeclarationTabPage.AutoScroll = true;
			this.DepartureDeclarationTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 590, true);
			this.DepartureDeclarationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B9DB026F-1663-4C6B-9688-B38E8BA8225C", "Details");
			this.DepartureDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DepartureDeclarationTabPage.Name = "DepartureDeclarationTabPage";
			this.DepartureDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.DepartureDeclarationTabPage.TabIndex = 0;
			// 
			// UnloadingRemarksTabPage
			// 
			this.UnloadingRemarksTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UnloadingRemarksTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E351CE7E-D2D3-4AFB-B50E-1F54923076A3", "Unloading Remarks");
			this.UnloadingRemarksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnloadingRemarksTabPage.Name = "UnloadingRemarksTabPage";
			this.UnloadingRemarksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnloadingRemarksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.UnloadingRemarksTabPage.TabIndex = 10;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DepartureDeclarationTabPage);
			this.MainTabControl.Controls.Add(this.ArrivalNotificationTabPage);
			this.MainTabControl.Controls.Add(this.IncidentsTabPage);
			this.MainTabControl.Controls.Add(this.ServicesTabPage);
			this.MainTabControl.Controls.Add(this.TransportAndPackagingTabPage);
			this.MainTabControl.Controls.Add(this.HouseConsignmentsTabPage);
			this.MainTabControl.Controls.Add(this.UnloadingRemarksTabPage);
			this.MainTabControl.Controls.Add(this.MiscOptionsTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 740, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// IncidentsTabPage
			// 
			this.IncidentsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.IncidentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("073DA5C7-307A-4F25-A5E2-DCABEBB0B4A7", "Incidents");
			this.IncidentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.IncidentsTabPage.Name = "IncidentsTabPage";
			this.IncidentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.IncidentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.IncidentsTabPage.TabIndex = 2;
			// 
			// ServicesTabPage
			// 
			this.ServicesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ServicesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C2F5C311-2746-41A6-9044-1907F1291B08", "Services");
			this.ServicesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ServicesTabPage.Name = "ServicesTabPage";
			this.ServicesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ServicesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.ServicesTabPage.TabIndex = 1;
			// 
			// TransportAndPackagingTabPage
			// 
			this.TransportAndPackagingTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.TransportAndPackagingTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("BFB995E0-DEAC-4D47-9E75-BB0BB2C6CF4B", "Transport && Containers");
			this.TransportAndPackagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TransportAndPackagingTabPage.Name = "TransportAndPackagingTabPage";
			this.TransportAndPackagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TransportAndPackagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.TransportAndPackagingTabPage.TabIndex = 5;
			// 
			// HouseConsignmentsTabPage
			// 
			this.HouseConsignmentsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.HouseConsignmentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8208FB3D-1554-4E88-838F-CF2BB5598257", "House Consignments");
			this.HouseConsignmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseConsignmentsTabPage.Name = "HouseConsignmentsTabPage";
			this.HouseConsignmentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HouseConsignmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.HouseConsignmentsTabPage.TabIndex = 3;
			// 
			// MiscOptionsTabPage
			// 
			this.MiscOptionsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MiscOptionsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("F1445D6B-3D6A-4648-9170-304AB6078759", "Misc.");
			this.MiscOptionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscOptionsTabPage.Name = "MiscOptionsTabPage";
			this.MiscOptionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MiscOptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.MiscOptionsTabPage.TabIndex = 11;
			// 
			// Phase5NctsUserControlForPlugin
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "Phase5NctsUserControlForPlugin";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 740, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage IncidentsTabPage;
		protected ZUserControl IncidentsUserControl;
		protected ZArchitecture.GUI.ZTabPage HouseConsignmentsTabPage;
		protected ZUserControl HouseConsignmentsUserControl;
		protected ZArchitecture.GUI.ZTabPage ServicesTabPage;
		protected ZUserControl ServicesUserControl;
		protected ZArchitecture.GUI.ZTabPage TransportAndPackagingTabPage;
		protected ZUserControl TransportAndPackagingUserControl;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private ZUserControl MessagesUserControl;
		protected ZUserControl DeclarationDetailsTabUserControl;
		private ZArchitecture.GUI.ZTabPage ArrivalNotificationTabPage;
		private ZArchitecture.GUI.ZTabPage UnloadingRemarksTabPage;
		protected ZUserControl NctsArrivalUserControl;
		protected ZUserControl UnloadingRemarksUserControl;
		protected Enterprise.ZArchitecture.GUI.ZTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DepartureDeclarationTabPage;
		protected ZArchitecture.GUI.ZTabPage MiscOptionsTabPage;
		protected ZUserControl MiscOptionsUserControl;
	}
}
