namespace Enterprise.Customs.AU.AirCargo.GUI
{
	partial class AirCTOExportForm
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.HeaderMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.HeaderMessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 590, true);
			this.MainTabControl.Controls.SetChildIndex(this.HeaderMessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 563, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// HeaderMessagesTabPage
			// 
			this.HeaderMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HeaderMessagesTabPage.Name = "HeaderMessagesTabPage";
			this.HeaderMessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.HeaderMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 563, true);
			this.HeaderMessagesTabPage.TabIndex = 3;
			this.HeaderMessagesTabPage.Text = "Messages";
			this.HeaderMessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.HeaderMessagesTabPage_InitializeTab));
			// 
			// AirCTOExportForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 780, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(848, 725, true);
			this.Name = "AirCTOExportForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.exportManifestDetailsUserControl1 = new Enterprise.Customs.AU.ExportManifest.GUI.ExportManifestDetailsUserControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.exportManifestDetailsUserControl1);
			// 
			// exportManifestDetailsUserControl1
			// 
			this.exportManifestDetailsUserControl1.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.exportManifestDetailsUserControl1.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader";
			this.exportManifestDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exportManifestDetailsUserControl1.Header = null;
			this.exportManifestDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exportManifestDetailsUserControl1.Name = "exportManifestDetailsUserControl1";
			this.exportManifestDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 563, true);
			this.exportManifestDetailsUserControl1.TabIndex = 0;
			this.MainTabPage.ResumeLayout(true);
		}

		void HeaderMessagesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AirCTOExportForm));
			this.ediMessageUserControl1 = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.HeaderMessagesTabPage.SuspendLayout();
			this.HeaderMessagesTabPage.Controls.Add(this.ediMessageUserControl1);
			// 
			// ediMessageUserControl1
			//
			this.ediMessageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ediMessageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ediMessageUserControl1.Name = "ediMessageUserControl1";
			this.ediMessageUserControl1.ShowChangingBlueMessageHeading = false;
			this.ediMessageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 557, true);
			this.ediMessageUserControl1.TabIndex = 0;
			this.HeaderMessagesTabPage.ResumeLayout(true);
		}

		#endregion

		internal Enterprise.Customs.AU.ExportManifest.GUI.ExportManifestDetailsUserControl exportManifestDetailsUserControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage HeaderMessagesTabPage;
		private Enterprise.Messaging.GUI.EDIMessageUserControl ediMessageUserControl1;
	}
}
