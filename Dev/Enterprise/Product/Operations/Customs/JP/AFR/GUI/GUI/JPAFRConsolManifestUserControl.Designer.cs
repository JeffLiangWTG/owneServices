namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRConsolManifestUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.jpafrMainUserControl1 = new Enterprise.Customs.JP.AFR.GUI.JPAFRMainUserControl();
			this.BillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillsDetailsUserControl = new Enterprise.Customs.JP.AFR.GUI.JPAFRBillsUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.Customs.JP.AFR.GUI.MessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.BillsTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.JPAFRHeader);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.BillsTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(898, 620, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("JPAFRConsolManifestUserControl|39051fe5-d8c0-4dd0-992c-ebdde5617a1e", "Main");
			this.MainTabPage.Controls.Add(this.jpafrMainUserControl1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 593, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// jpafrMainUserControl1
			// 
			this.jpafrMainUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jpafrMainUserControl1, ".");
			this.jpafrMainUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jpafrMainUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.jpafrMainUserControl1.Name = "jpafrMainUserControl1";
			this.jpafrMainUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 587, true);
			this.jpafrMainUserControl1.TabIndex = 0;
			// 
			// BillsTabPage
			// 
			this.BillsTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("JPAFRConsolManifestUserControl|1F3581C3-6250-425A-B37E-C1B7D7E5F3BB", "Bills Details");
			this.BillsTabPage.Controls.Add(this.BillsDetailsUserControl);
			this.BillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillsTabPage.Name = "BillsTabPage";
			this.BillsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 593, true);
			this.BillsTabPage.TabIndex = 1;
			// 
			// BillsDetailsUserControl
			// 
			this.BillsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillsDetailsUserControl, ".");
			this.BillsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillsDetailsUserControl.Name = "BillsDetailsUserControl";
			this.BillsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 587, true);
			this.BillsDetailsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("JPAFRConsolManifestUserControl|20865C7B-D2B0-46B4-B0C3-CEE4CBD2E3E2", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 593, true);
			this.MessagesTabPage.TabIndex = 2;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 587, true);
			this.MessagesUserControl.TabIndex = 1;
			this.MessagesUserControl.TabStop = false;
			// 
			// JPAFRConsolManifestUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "JPAFRConsolManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(898, 620, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.BillsTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		private ZArchitecture.GUI.ZTabControl MainTabControl;
		private ZArchitecture.GUI.ZTabPage MainTabPage;
		private ZArchitecture.GUI.ZTabPage BillsTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private JPAFRBillsUserControl BillsDetailsUserControl;
		private MessagesUserControl MessagesUserControl;
		private JPAFRMainUserControl jpafrMainUserControl1;
		private System.ComponentModel.IContainer components;
	}
}
