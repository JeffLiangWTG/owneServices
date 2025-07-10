namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class NctsGoodsItemsUserControl
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
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.GoodsItemsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GoodsItemsGridDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.GoodsItemsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemLineDetailDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ItemPackagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NctsPackagesDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ItemContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemContainersDynamicUserControl = new ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SupportingDocumentsDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ItemAdditionalInfosTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalInfosDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ItemPreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.NctsPreviousDocumentsDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.ItemSecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ItemSecurityDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsSplitContainer)).BeginInit();
			this.GoodsItemsSplitContainer.Panel1.SuspendLayout();
			this.GoodsItemsSplitContainer.Panel2.SuspendLayout();
			this.GoodsItemsSplitContainer.SuspendLayout();
			this.GoodsItemsGridDynamicUserControl.SuspendLayout();
			this.GoodsItemsTabControl.SuspendLayout();
			this.ItemDetailsTabPage.SuspendLayout();
			this.GoodsItemLineDetailDynamicUserControl.SuspendLayout();
			this.ItemPackagesTabPage.SuspendLayout();
			this.NctsPackagesDynamicUserControl.SuspendLayout();
			this.ItemContainersTabPage.SuspendLayout();
			this.GoodsItemContainersDynamicUserControl.SuspendLayout();
			this.SupportingDocumentsTabPage.SuspendLayout();
			this.SupportingDocumentsDynamicUserControl.SuspendLayout();
			this.ItemAdditionalInfosTabPage.SuspendLayout();
			this.AdditionalInfosDynamicUserControl.SuspendLayout();
			this.ItemPreviousDocumentsTabPage.SuspendLayout();
			this.NctsPreviousDocumentsDynamicUserControl.SuspendLayout();
			this.ItemSecurityTabPage.SuspendLayout();
			this.ItemSecurityDynamicUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsCommonCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc>);
			// 
			// GoodsItemsSplitContainer
			// 
			this.GoodsItemsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsSplitContainer.Name = "GoodsItemsSplitContainer";
			this.GoodsItemsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// GoodsItemsSplitContainer.Panel1
			// 
			this.GoodsItemsSplitContainer.Panel1.Controls.Add(this.GoodsItemsGridDynamicUserControl);
			// 
			// GoodsItemsSplitContainer.Panel2
			// 
			this.GoodsItemsSplitContainer.Panel2.Controls.Add(this.GoodsItemsTabControl);
			this.GoodsItemsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 759, true);
			this.GoodsItemsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(198);
			this.GoodsItemsSplitContainer.TabIndex = 0;
			// 
			// GoodsItemsGridDynamicUserControl
			// 
			this.GoodsItemsGridDynamicUserControl.AllowDrop = true;
			this.GoodsItemsGridDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsGridDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsGridDynamicUserControl.Name = "GoodsItemsGridDynamicUserControl";
			this.GoodsItemsGridDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 198, true);
			this.GoodsItemsGridDynamicUserControl.TabIndex = 0;
			this.GoodsItemsGridDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.GoodsItemsGridUserControl);
			// 
			// GoodsItemsTabControl
			// 
			this.GoodsItemsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.GoodsItemsTabControl.Controls.Add(this.ItemDetailsTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.ItemPackagesTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.ItemContainersTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.SupportingDocumentsTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.ItemAdditionalInfosTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.ItemPreviousDocumentsTabPage);
			this.GoodsItemsTabControl.Controls.Add(this.ItemSecurityTabPage);
			this.GoodsItemsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsTabControl.Name = "GoodsItemsTabControl";
			this.GoodsItemsTabControl.SelectedIndex = 0;
			this.GoodsItemsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 557, true);
			this.GoodsItemsTabControl.TabIndex = 0;
			// 
			// ItemDetailsTabPage
			// 
			this.ItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E9035F87-271A-6B0E-EDC3-52FE496BF27E", "Item Details");
			this.ItemDetailsTabPage.Controls.Add(this.GoodsItemLineDetailDynamicUserControl);
			this.ItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemDetailsTabPage.Name = "ItemDetailsTabPage";
			this.ItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemDetailsTabPage.TabIndex = 0;
			// 
			// GoodsItemLineDetailDynamicUserControl
			// 
			this.GoodsItemLineDetailDynamicUserControl.AllowDrop = true;
			this.GoodsItemLineDetailDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemLineDetailDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemLineDetailDynamicUserControl.Name = "GoodsItemLineDetailDynamicUserControl";
			this.GoodsItemLineDetailDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.GoodsItemLineDetailDynamicUserControl.TabIndex = 0;
			this.GoodsItemLineDetailDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.ItemDetailsUserControl);
			// 
			// ItemPackagesTabPage
			// 
			this.ItemPackagesTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("A6F7CAB1-7AC0-8024-833B-7E8EA53EC03A", "[31] Packages");
			this.ItemPackagesTabPage.Controls.Add(this.NctsPackagesDynamicUserControl);
			this.ItemPackagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemPackagesTabPage.Name = "ItemPackagesTabPage";
			this.ItemPackagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemPackagesTabPage.TabIndex = 2;
			// 
			// NctsPackagesDynamicUserControl
			// 
			this.NctsPackagesDynamicUserControl.AllowDrop = true;
			this.NctsPackagesDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NctsPackagesDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NctsPackagesDynamicUserControl.Name = "NctsPackagesDynamicUserControl";
			this.NctsPackagesDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.NctsPackagesDynamicUserControl.TabIndex = 0;
			this.NctsPackagesDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.ItemPackagesTabUserControl);
			// 
			// ItemContainersTabPage
			// 
			this.ItemContainersTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C48A7187-D68F-A05D-58BD-73DE7C0AB047", "Containers");
			this.ItemContainersTabPage.Controls.Add(this.GoodsItemContainersDynamicUserControl);
			this.ItemContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemContainersTabPage.Name = "ItemContainersTabPage";
			this.ItemContainersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemContainersTabPage.TabIndex = 1;
			this.ItemContainersTabPage.UseVisualStyleBackColor = true;
			// 
			// GoodsItemContainersUserControl
			// 
			this.GoodsItemContainersDynamicUserControl.AllowDrop = true;
			this.GoodsItemContainersDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemContainersDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GoodsItemContainersDynamicUserControl.Name = "GoodsItemContainersDynamicUserControl";
			this.GoodsItemContainersDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1686, 524, true);
			this.GoodsItemContainersDynamicUserControl.TabIndex = 0;
			this.GoodsItemContainersDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.GoodsItemContainersUserControl);
			// 
			// SupportingDocumentsTabPage
			// 
			this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("DCA986F4-1A67-5FE1-2528-E079D7D3A308", "[44] Supporting Documents");
			this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsDynamicUserControl);
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.SupportingDocumentsTabPage.TabIndex = 4;
			// 
			// SupportingDocumentsDynamicUserControl
			// 
			this.SupportingDocumentsDynamicUserControl.AllowDrop = true;
			this.SupportingDocumentsDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsDynamicUserControl.Name = "SupportingDocumentsDynamicUserControl";
			this.SupportingDocumentsDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.SupportingDocumentsDynamicUserControl.TabIndex = 2;
			this.SupportingDocumentsDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.SupportingDocumentsUserControl);
			// 
			// ItemAdditionalInfosTabPage
			// 
			this.ItemAdditionalInfosTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("11cc3062-2c4d-437b-b49b-c19e142ae32d", "[44] Additional Infos");
			this.ItemAdditionalInfosTabPage.Controls.Add(this.AdditionalInfosDynamicUserControl);
			this.ItemAdditionalInfosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemAdditionalInfosTabPage.Name = "ItemAdditionalInfosTabPage";
			this.ItemAdditionalInfosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemAdditionalInfosTabPage.TabIndex = 5;
			// 
			// AdditionalInfosDynamicUserControl
			// 
			this.AdditionalInfosDynamicUserControl.AllowDrop = true;
			this.AdditionalInfosDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosDynamicUserControl.Name = "AdditionalInfosDynamicUserControl";
			this.AdditionalInfosDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.AdditionalInfosDynamicUserControl.TabIndex = 0;
			this.AdditionalInfosDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.AdditionalInfosUserControl);
			// 
			// ItemPreviousDocumentsTabPage
			// 
			this.ItemPreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B7A607D4-9D9C-2FC7-46D4-60594F5E6F42", "[40] Previous Documents");
			this.ItemPreviousDocumentsTabPage.Controls.Add(this.NctsPreviousDocumentsDynamicUserControl);
			this.ItemPreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemPreviousDocumentsTabPage.Name = "ItemPreviousDocumentsTabPage";
			this.ItemPreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemPreviousDocumentsTabPage.TabIndex = 3;
			// 
			// NctsPreviousDocumentsDynamicUserControl
			// 
			this.NctsPreviousDocumentsDynamicUserControl.AllowDrop = true;
			this.NctsPreviousDocumentsDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NctsPreviousDocumentsDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NctsPreviousDocumentsDynamicUserControl.Name = "NctsPreviousDocumentsDynamicUserControl";
			this.NctsPreviousDocumentsDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.NctsPreviousDocumentsDynamicUserControl.TabIndex = 0;
			this.NctsPreviousDocumentsDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.PreviousDocumentsUserControl);
			// 
			// ItemSecurityTabPage
			// 
			this.ItemSecurityTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0A3B1A4F-8A69-AC4A-C58D-B863F29ACD5A", "Security");
			this.ItemSecurityTabPage.Controls.Add(this.ItemSecurityDynamicUserControl);
			this.ItemSecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ItemSecurityTabPage.Name = "ItemSecurityTabPage";
			this.ItemSecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemSecurityTabPage.TabIndex = 6;
			// 
			// ItemSecurityDynamicUserControl
			// 
			this.ItemSecurityDynamicUserControl.AllowDrop = true;
			this.ItemSecurityDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemSecurityDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemSecurityDynamicUserControl.Name = "ItemSecurityDynamicUserControl";
			this.ItemSecurityDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1692, 530, true);
			this.ItemSecurityDynamicUserControl.TabIndex = 0;
			this.ItemSecurityDynamicUserControl.UserControlType = typeof(Enterprise.Customs.EU.NCTS.GUI.ItemSecurityTabUserControl);
			// 
			// NctsGoodsItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsItemsSplitContainer);
			this.Name = "NctsGoodsItemsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 759, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsItemsSplitContainer.Panel1.ResumeLayout(false);
			this.GoodsItemsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsSplitContainer)).EndInit();
			this.GoodsItemsSplitContainer.ResumeLayout(false);
			this.GoodsItemsSplitContainer.PerformLayout();
			this.GoodsItemsGridDynamicUserControl.ResumeLayout(true);
			this.GoodsItemsGridDynamicUserControl.PerformLayout();
			this.GoodsItemsTabControl.ResumeLayout(false);
			this.GoodsItemsTabControl.PerformLayout();
			this.ItemDetailsTabPage.ResumeLayout(false);
			this.ItemDetailsTabPage.PerformLayout();
			this.GoodsItemLineDetailDynamicUserControl.ResumeLayout(true);
			this.GoodsItemLineDetailDynamicUserControl.PerformLayout();
			this.ItemPackagesTabPage.ResumeLayout(false);
			this.ItemPackagesTabPage.PerformLayout();
			this.NctsPackagesDynamicUserControl.ResumeLayout(true);
			this.NctsPackagesDynamicUserControl.PerformLayout();
			this.ItemContainersTabPage.ResumeLayout(false);
			this.ItemContainersTabPage.PerformLayout();
			this.GoodsItemContainersDynamicUserControl.ResumeLayout(true);
			this.GoodsItemContainersDynamicUserControl.PerformLayout();
			this.SupportingDocumentsTabPage.ResumeLayout(false);
			this.SupportingDocumentsTabPage.PerformLayout();
			this.SupportingDocumentsDynamicUserControl.ResumeLayout(true);
			this.SupportingDocumentsDynamicUserControl.PerformLayout();
			this.ItemAdditionalInfosTabPage.ResumeLayout(false);
			this.ItemAdditionalInfosTabPage.PerformLayout();
			this.AdditionalInfosDynamicUserControl.ResumeLayout(true);
			this.AdditionalInfosDynamicUserControl.PerformLayout();
			this.ItemPreviousDocumentsTabPage.ResumeLayout(false);
			this.ItemPreviousDocumentsTabPage.PerformLayout();
			this.NctsPreviousDocumentsDynamicUserControl.ResumeLayout(true);
			this.NctsPreviousDocumentsDynamicUserControl.PerformLayout();
			this.ItemSecurityTabPage.ResumeLayout(false);
			this.ItemSecurityTabPage.PerformLayout();
			this.ItemSecurityDynamicUserControl.ResumeLayout(true);
			this.ItemSecurityDynamicUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KSplitContainer GoodsItemsSplitContainer;
		protected ZArchitecture.GUI.ZTabControl GoodsItemsTabControl;
		protected ZArchitecture.GUI.ZTabPage ItemDetailsTabPage;
		internal protected ZArchitecture.GUI.ZTabPage ItemContainersTabPage;
		public ZArchitecture.GUI.ZTabPage ItemPackagesTabPage;
		protected ZArchitecture.GUI.ZTabPage ItemPreviousDocumentsTabPage;
		protected ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		protected ZArchitecture.GUI.ZTabPage ItemAdditionalInfosTabPage;
		public ZArchitecture.GUI.ZTabPage ItemSecurityTabPage;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl GoodsItemContainersDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl GoodsItemLineDetailDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl GoodsItemsGridDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl NctsPreviousDocumentsDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl NctsPackagesDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl ItemSecurityDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl AdditionalInfosDynamicUserControl;
		protected ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentsDynamicUserControl;
	}
}
