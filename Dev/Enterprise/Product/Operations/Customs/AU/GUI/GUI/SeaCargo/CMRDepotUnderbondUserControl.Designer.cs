using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class CMRDepotUnderbondUserControl
	{
		private void InitializeComponent()
		{
			this.depotTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.cusUnderbondTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.cusUnderbondUserControl1 = new Enterprise.Customs.GUI.CusUnderbondUserControl();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messageUserControl1 = new Enterprise.Customs.GUI.MessageUserControl();
			this.depotTabControl.SuspendLayout();
			this.cusUnderbondTabPage.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// DepotTabControl
			// 
			this.depotTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.depotTabControl.Controls.Add(this.cusUnderbondTabPage);
			this.depotTabControl.Controls.Add(this.messagesTabPage);
			this.depotTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.depotTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.depotTabControl.Name = "DepotTabControl";
			this.depotTabControl.SelectedIndex = 0;
			this.depotTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 608, true);
			this.depotTabControl.TabIndex = 1;
			// 
			// CusUnderbondTabPage
			// 
			this.cusUnderbondTabPage.CheckForNotifications = true;
			this.cusUnderbondTabPage.Controls.Add(this.cusUnderbondUserControl1);
			this.cusUnderbondTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.cusUnderbondTabPage.Name = "CusUnderbondTabPage";
			this.cusUnderbondTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 581, true);
			this.cusUnderbondTabPage.TabIndex = 0;
			this.cusUnderbondTabPage.Text = "CMR";
			// 
			// cusUnderbondUserControl1
			// 
			this.cusUnderbondUserControl1.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.cusUnderbondUserControl1.DataSourceTypeName = "Enterprise.Customs.Business.ICusUnderbondUnionCollectionParent";
			this.cusUnderbondUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusUnderbondUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.cusUnderbondUserControl1.Name = "cusUnderbondUserControl1";
			this.cusUnderbondUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 581, true);
			this.cusUnderbondUserControl1.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.CheckForNotifications = true;
			this.messagesTabPage.Controls.Add(this.messageUserControl1);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 581, true);
			this.messagesTabPage.TabIndex = 1;
			this.messagesTabPage.Text = "Messages";
			// 
			// messageUserControl1
			// 
			this.messageUserControl1.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.messageUserControl1.DataSourceTypeName = "Enterprise.Customs.Business.BaseJobDeclaration";
			this.messageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageUserControl1.Name = "messageUserControl1";
			this.messageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 581, true);
			this.messageUserControl1.TabIndex = 0;
			// 
			// CMRDepotUnderbondUserControl
			//
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.depotTabControl);
			this.Name = "CMRDepotUnderbondUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 608, true);
			this.depotTabControl.ResumeLayout(false);
			this.cusUnderbondTabPage.ResumeLayout(false);
			this.messagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZTemplateTabControl depotTabControl;
		private ZTabPage cusUnderbondTabPage;
		private ZTabPage messagesTabPage;
		private Customs.GUI.MessageUserControl messageUserControl1;
		internal Customs.GUI.CusUnderbondUserControl cusUnderbondUserControl1;
	}
}
