namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class NctsUserControlForPlugin
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
			this.GoodsItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ArrivalNotificationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DepartureDeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UnloadingRemarksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MiscOptionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatusTabPage = new ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// GoodsItemsTabPage
			//
			this.GoodsItemsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("AEE13B39-E2B1-4ABA-9211-C0477A30A385", "Goods Items");
			this.GoodsItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemsTabPage.Name = "GoodsItemsTabPage";
			this.GoodsItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.GoodsItemsTabPage.TabIndex = 4;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.INctsCommonCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc>)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.GoodsItems)));
			// 
			// MessagesTabPage
			//
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("F0863017-8116-4C77-A500-E8F212B32C9D", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.MessagesTabPage.TabIndex = 6;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// SecurityTabPage
			//
			this.SecurityTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("89729E91-3283-4BA4-AA2D-CAE204DBD3AD", "Security");
			this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SecurityTabPage.Name = "SecurityTabPage";
			this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.SecurityTabPage.TabIndex = 7;
			// 
			// ArrivalNotificationTabPage
			//
			this.ArrivalNotificationTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ArrivalNotificationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d8dda886-7de9-4cfc-b1c2-869dbd3a2bda", "Arrival Notification");
			this.ArrivalNotificationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ArrivalNotificationTabPage.Name = "ArrivalNotificationTabPage";
			this.ArrivalNotificationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ArrivalNotificationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.ArrivalNotificationTabPage.TabIndex = 9;
			// 
			// DepartureDeclarationTabPage
			// 
			this.DepartureDeclarationTabPage.AutoScroll = true;
			this.DepartureDeclarationTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 590, true);
			this.DepartureDeclarationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ZTemplateForm|05B61FD8-5434-45F4-AB88-E940D9074252", "Departure Declaration");
			this.DepartureDeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DepartureDeclarationTabPage.Name = "DepartureDeclarationTabPage";
			this.DepartureDeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 713, true);
			this.DepartureDeclarationTabPage.TabIndex = 0;
			// 
			// UnloadingRemarksTabPage
			// 
			this.UnloadingRemarksTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UnloadingRemarksTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("86662e90-c7a9-4f48-a9f9-159ba38b5e36", "Unloading Remarks");
			this.UnloadingRemarksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.UnloadingRemarksTabPage.Name = "UnloadingRemarksTabPage";
			this.UnloadingRemarksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.UnloadingRemarksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.UnloadingRemarksTabPage.TabIndex = 10;
			// 
			// MiscOptionsTabPage
			// 
			this.MiscOptionsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MiscOptionsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("4B0314D1-7060-48FC-A0A8-48C3D2A87099", "Misc.");
			this.MiscOptionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MiscOptionsTabPage.Name = "MiscOptionsTabPage";
			this.MiscOptionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MiscOptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.MiscOptionsTabPage.TabIndex = 11;
			//
			// StatusTabPage
			//
			this.StatusTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.StatusTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E9D674F7-F31E-455F-BB88-6A34CFEE867E", "Status.");
			this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1105, 657, true);
			this.StatusTabPage.TabIndex = 12;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DepartureDeclarationTabPage);
			this.MainTabControl.Controls.Add(this.ArrivalNotificationTabPage);
			this.MainTabControl.Controls.Add(this.UnloadingRemarksTabPage);
			this.MainTabControl.Controls.Add(this.SecurityTabPage);
			this.MainTabControl.Controls.Add(this.GoodsItemsTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.MiscOptionsTabPage);
			this.MainTabControl.Controls.Add(this.StatusTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 740, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// NctsUserControlForPlugin
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "NctsUserControlForPlugin";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1113, 740, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZTabPage GoodsItemsTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private MessagesTabUserControl MessagesUserControl;
		protected ZArchitecture.GUI.ZTabPage SecurityTabPage;
		protected DeclarationDetailsTabUserControl DeclarationDetailsTabUserControl;
		protected SecurityTabUserControl SecurityTabUserControl;
		private ZArchitecture.GUI.ZTabPage ArrivalNotificationTabPage;
		private ZArchitecture.GUI.ZTabPage UnloadingRemarksTabPage;
		protected NctsArrivalUserControl NctsArrivalUserControl;
		protected UnloadingRemarksUserControl UnloadingRemarksUserControl;
		protected Enterprise.ZArchitecture.GUI.ZTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DepartureDeclarationTabPage;
		public NctsGoodsItemsUserControl NctsGoodsItemsUserControl;
		protected ZArchitecture.GUI.ZTabPage MiscOptionsTabPage;
		protected Enterprise.Customs.EU.NCTS.GUI.MiscOptionsUserControl MiscOptionsUserControl;
		protected ZArchitecture.GUI.ZTabPage StatusTabPage;
		protected Enterprise.Customs.EU.NCTS.GUI.DeclarationStatusTabUserControl DeclarationStatusUserControl;
	}
}
