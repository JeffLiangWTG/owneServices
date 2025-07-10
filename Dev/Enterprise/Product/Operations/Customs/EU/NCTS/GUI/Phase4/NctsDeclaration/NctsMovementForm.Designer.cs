namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class NctsMovementForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.GoodsItemsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.ArrivalNotificationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.UnloadingRemarksTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MiscOptionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.StatusTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MainTabControl.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.ArrivalNotificationTabPage);
            this.MainTabControl.Controls.Add(this.UnloadingRemarksTabPage);
            this.MainTabControl.Controls.Add(this.SecurityTabPage);
            this.MainTabControl.Controls.Add(this.GoodsItemsTabPage);
            this.MainTabControl.Controls.Add(this.MiscOptionsTabPage);
            this.MainTabControl.Controls.Add(this.MessagesTabPage);
            this.MainTabControl.Controls.Add(this.StatusTabPage);
            this.MainTabControl.Controls.Add(this.WorkflowTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 763, true);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.StatusTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MiscOptionsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.GoodsItemsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.SecurityTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.UnloadingRemarksTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.ArrivalNotificationTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			//
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ZTemplateForm|05B61FD8-5434-45F4-AB88-E940D9074252", "Departure Declaration");
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 763, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
            // 
            // GoodsItemsTabPage
            // 
			this.GoodsItemsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b133961b-1598-431f-a472-26016b9400ad", "Goods Items");
            this.GoodsItemsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.GoodsItemsTabPage.Name = "GoodsItemsTabPage";
            this.GoodsItemsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 736, true);
            this.GoodsItemsTabPage.TabIndex = 4;
            this.GoodsItemsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.GoodsItemsTabPage_InitializeTab));
            // 
            // MessagesTabPage
            // 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("db5d1b87-a9eb-4ec1-bcb3-bf1bf3867f5b", "Messages");
            this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.MessagesTabPage.Name = "MessagesTabPage";
            this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 569, true);
            this.MessagesTabPage.TabIndex = 6;
            this.MessagesTabPage.UseVisualStyleBackColor = true;
            this.MessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagesTabPage_InitializeTab));
            // 
            // SecurityTabPage
            // 
			this.SecurityTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("f77e0cc1-b63f-4761-b2f3-e279a8e67c4d", "Security");
            this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SecurityTabPage.Name = "SecurityTabPage";
            this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            this.SecurityTabPage.TabIndex = 7;
            this.SecurityTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.SecurityTabPage_InitializeTab));
            // 
            // WorkflowTabPage
            // 
            this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.WorkflowTabPage.Name = "WorkflowTabPage";
            this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
            this.WorkflowTabPage.TabIndex = 8;
            this.WorkflowTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.WorkflowTabPage_InitializeTab));
            // 
            // ArrivalNotificationTabPage
            // 
            this.ArrivalNotificationTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ArrivalNotificationTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d8dda886-7de9-4cfc-b1c2-869dbd3a2bda", "Arrival Notification");
            this.ArrivalNotificationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ArrivalNotificationTabPage.Name = "ArrivalNotificationTabPage";
            this.ArrivalNotificationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.ArrivalNotificationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            this.ArrivalNotificationTabPage.TabIndex = 9;
            this.ArrivalNotificationTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ArrivalNotificationTabPage_InitializeTab));
            // 
            // UnloadingRemarksTabPage
            // 
            this.UnloadingRemarksTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.UnloadingRemarksTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("86662e90-c7a9-4f48-a9f9-159ba38b5e36", "Unloading Remarks");
            this.UnloadingRemarksTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.UnloadingRemarksTabPage.Name = "UnloadingRemarksTabPage";
            this.UnloadingRemarksTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.UnloadingRemarksTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            this.UnloadingRemarksTabPage.TabIndex = 10;
            this.UnloadingRemarksTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.UnloadingRemarksTabPage_InitializeTab));
            // 
            // MiscOptionsTabPage
            // 
            this.MiscOptionsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MiscOptionsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("04c5667b-223f-43af-aa5c-2c91572dc2d6", "Misc.");
            this.MiscOptionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.MiscOptionsTabPage.Name = "MiscOptionsTabPage";
            this.MiscOptionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.MiscOptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 569, true);
            this.MiscOptionsTabPage.TabIndex = 11;
            this.MiscOptionsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MiscOptionsTabPage_InitializeTab));
            // 
            // StatusTabPage
            // 
			this.StatusTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("2c3f72f9-a22d-49a0-94fd-f91671868b26", "Status");
            this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.StatusTabPage.Name = "StatusTabPage";
            this.StatusTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1156, 712, true);
            this.StatusTabPage.TabIndex = 12;
            this.StatusTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.StatusTabPage_InitializeTab));
            // 
            // NctsMovementForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1164, 839, true);
            this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1180, 725, true);
            this.Name = "NctsMovementForm";
            this.ShouldSerializeTabPageMethods = true;
            this.Text = "NctsMovementForm";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage GoodsItemsTabPage;
		protected ZArchitecture.GUI.ZTabPage MessagesTabPage;
		protected ZArchitecture.GUI.ZTabPage SecurityTabPage;
		protected Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		protected ZArchitecture.GUI.ZTabPage ArrivalNotificationTabPage;
		protected ZArchitecture.GUI.ZTabPage UnloadingRemarksTabPage;
		protected ZArchitecture.GUI.ZTabPage MiscOptionsTabPage;
		protected ZArchitecture.GUI.ZTabPage StatusTabPage;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl MessagesTabDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl ArrivalTabDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl UnloadingRemarksDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DeclarationDetailsTabDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl SecurityTabDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl GoodsItemsTabDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl MiscOptionsTabDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl DeclarationStatusTabDynamicUserControl;
	}
}
