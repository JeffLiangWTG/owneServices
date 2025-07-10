namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.billDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.jpafrBillsUserControl = new Enterprise.Customs.JP.AFR.GUI.JPAFRBillsUserControl();
			this.messageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesUserControl = new Enterprise.Customs.JP.AFR.GUI.MessagesUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.JPAFRSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.sailingUserControl = new Enterprise.Customs.JP.AFR.GUI.SailingUserControl();
			this.jpafrMainUserControl = new Enterprise.Customs.JP.AFR.GUI.JPAFRMainUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.billDetailsTabPage.SuspendLayout();
			this.messageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JPAFRSplitContainer)).BeginInit();
			this.JPAFRSplitContainer.Panel1.SuspendLayout();
			this.JPAFRSplitContainer.Panel2.SuspendLayout();
			this.JPAFRSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.billDetailsTabPage);
			this.MainTabControl.Controls.Add(this.messageTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 605, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.messageTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.billDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.JPAFRSplitContainer);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 578, true);
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("09ed9327-6009-4572-b7e6-cfae7cb126ef", "Main");
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 578, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 605, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			// 
			// billDetailsTabPage
			// 
			this.billDetailsTabPage.Controls.Add(this.jpafrBillsUserControl);
			this.billDetailsTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c5d24bd1-2542-4d91-80b4-54caa2bfd545", "Bill Details");
			this.billDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.billDetailsTabPage.Name = "billDetailsTabPage";
			this.billDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.billDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 459, true);
			this.billDetailsTabPage.TabIndex = 3;
			// 
			// jpafrBillsUserControl
			// 
			this.jpafrBillsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jpafrBillsUserControl, ".");
			this.jpafrBillsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jpafrBillsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.jpafrBillsUserControl.Name = "jpafrBillsUserControl";
			this.jpafrBillsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 453, true);
			this.jpafrBillsUserControl.TabIndex = 0;
			// 
			// messageTabPage
			// 
			this.messageTabPage.Controls.Add(this.messagesUserControl);
			this.messageTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("daaf9421-cf41-45e1-9402-3e4d3511ab9b", "Messages");
			this.messageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messageTabPage.Name = "messageTabPage";
			this.messageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 459, true);
			this.messageTabPage.TabIndex = 6;
			// 
			// messagesUserControl
			// 
			this.messagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesUserControl, ".");
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagesUserControl.Name = "messagesUserControl";
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(933, 453, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 459, true);
			this.WorkflowTabPage.TabIndex = 9;
			// 
			// JPAFRSplitContainer
			// 
			this.JPAFRSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JPAFRSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.JPAFRSplitContainer.IsSplitterFixed = true;
			this.JPAFRSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JPAFRSplitContainer.Name = "JPAFRSplitContainer";
			this.JPAFRSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// JPAFRSplitContainer.Panel1
			// 
			this.JPAFRSplitContainer.Panel1.Controls.Add(this.sailingUserControl);
			// 
			// JPAFRSplitContainer.Panel2
			// 
			this.JPAFRSplitContainer.Panel2.Controls.Add(this.jpafrMainUserControl);
			this.JPAFRSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 578, true);
			this.JPAFRSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.JPAFRSplitContainer.TabIndex = 1;
			this.JPAFRSplitContainer.TabStop = false;
			// 
			// sailingUserControl
			// 
			this.sailingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sailingUserControl, ".");
			this.sailingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sailingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sailingUserControl.Name = "sailingUserControl";
			this.sailingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 80, true);
			this.sailingUserControl.TabIndex = 0;
			this.sailingUserControl.TabStop = false;
			// 
			// jpafrMainUserControl
			// 
			this.jpafrMainUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jpafrMainUserControl, ".");
			this.jpafrMainUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jpafrMainUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.jpafrMainUserControl.Name = "jpafrMainUserControl";
			this.jpafrMainUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(971, 494, true);
			this.jpafrMainUserControl.TabIndex = 0;
			// 
			// JPAFRForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 661, true);
			this.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1035, 700, true);
			this.Name = "JPAFRForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "JPAFRForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.billDetailsTabPage.ResumeLayout(false);
			this.messageTabPage.ResumeLayout(false);
			this.JPAFRSplitContainer.Panel1.ResumeLayout(false);
			this.JPAFRSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.JPAFRSplitContainer)).EndInit();
			this.JPAFRSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage billDetailsTabPage;
		private ZArchitecture.GUI.ZTabPage messageTabPage;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private CargoWise.Windows.UI.KSplitContainer JPAFRSplitContainer;
		private JPAFRBillsUserControl jpafrBillsUserControl;
		private MessagesUserControl messagesUserControl;
		private SailingUserControl sailingUserControl;
		private JPAFRMainUserControl jpafrMainUserControl;

	}
}
