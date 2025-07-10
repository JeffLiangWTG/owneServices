using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GoodsItemsTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.GoodsItemsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.GoodsItemTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.GoodsItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DynamicGoodsItemDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.GoodsItemPackagesAndContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.GoodsItemSupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemSupportingDocumentsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5GoodsItemSupportingDocumentsTabUserControl();
			this.GoodsItemAdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemPreviousDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemPreviousDocumentsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5GoodsItemPreviousDocumentsTabUserControl();
			this.GoodsItemSupplyChainActorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GoodsItemSupplyChainActorsTabUserControl = new Enterprise.Customs.EU.NCTS.GUI.Phase5GoodsItemSupplyChainActorsTabUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsSplitContainer)).BeginInit();
			this.GoodsItemsSplitContainer.Panel1.SuspendLayout();
			this.GoodsItemsSplitContainer.Panel2.SuspendLayout();
			this.GoodsItemsSplitContainer.SuspendLayout();
			this.GoodsItemTabControl.SuspendLayout();
			this.GoodsItemDetailsTabPage.SuspendLayout();
			this.GoodsItemPackagesAndContainersTabPage.SuspendLayout();
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.SuspendLayout();
			this.GoodsItemSupportingDocumentsTabPage.SuspendLayout();
			this.GoodsItemSupportingDocumentsTabUserControl.SuspendLayout();
			this.GoodsItemAdditionalDocumentsTabPage.SuspendLayout();
			this.GoodsItemPreviousDocumentsTabUserControl.SuspendLayout();
			this.GoodsItemSupplyChainActorsTabPage.SuspendLayout();
			this.GoodsItemSupplyChainActorsTabUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsDepartureCargoDescCollection<Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc>);
			// 
			// GoodsItemsSplitContainer
			// 
			this.GoodsItemsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemsSplitContainer.Name = "GoodsItemsSplitContainer";
			this.GoodsItemsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.GoodsItemsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 490, true);
			// 
			// GoodsItemsSplitContainer.Panel1
			// 
			this.GoodsItemsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(10);
			// 
			// GoodsItemsSplitContainer.Panel2
			// 
			this.GoodsItemsSplitContainer.Panel2.Controls.Add(this.GoodsItemTabControl);
			this.GoodsItemsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(310);
			this.GoodsItemsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(159);
			this.GoodsItemsSplitContainer.SplitterWidth = 5;
			this.GoodsItemsSplitContainer.TabIndex = 0;
			// 
			// GoodsItemTabControl
			// 
			this.GoodsItemTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemDetailsTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemPackagesAndContainersTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemSupportingDocumentsTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemAdditionalDocumentsTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemPreviousDocumentsTabPage);
			this.GoodsItemTabControl.Controls.Add(this.GoodsItemSupplyChainActorsTabPage);
			this.GoodsItemTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemTabControl.Name = "GoodsItemTabControl";
			this.GoodsItemTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 326, true);
			this.GoodsItemTabControl.TabIndex = 0;
			// 
			// GoodsItemDetailsTabPage
			// 
			this.GoodsItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E9035F87-271A-6B0E-EDC3-52FE496BF27E", "Item Details");
			this.GoodsItemDetailsTabPage.Controls.Add(this.DynamicGoodsItemDetailsPanel);
			this.GoodsItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemDetailsTabPage.Name = "GoodsItemDetailsTabPage";
			this.GoodsItemDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.GoodsItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemDetailsTabPage.TabIndex = 0;
			// 
			// DynamicGoodsItemDetailsPanel
			// 
			this.DynamicGoodsItemDetailsPanel.AllowDrop = true;
			this.DynamicGoodsItemDetailsPanel.AutoScroll = true;
			this.DynamicGoodsItemDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicGoodsItemDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicGoodsItemDetailsPanel.Name = "DynamicGoodsItemDetailsPanel";
			this.DynamicGoodsItemDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 286, true);
			this.DynamicGoodsItemDetailsPanel.TabIndex = 0;
			// 
			// GoodsItemPackagesAndContainersTabPage
			// 
			this.GoodsItemPackagesAndContainersTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("31DCBC28-0970-4CF5-A5D6-F12B4B646532", "Packages && Containers");
			this.GoodsItemPackagesAndContainersTabPage.Controls.Add(this.GoodsItemPackagesAndContainersDynamicCreationUserControl);
			this.GoodsItemPackagesAndContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemPackagesAndContainersTabPage.Name = "GoodsItemPackagesAndContainersTabPage";
			this.GoodsItemPackagesAndContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemPackagesAndContainersTabPage.TabIndex = 0;
			// 
			// GoodsItemPackagesAndContainersDynamicCreationUserControl
			// 
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.AllowDrop = true;
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.AutoScroll = true;
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.Name = "GoodsItemPackagesAndContainersDynamicCreationUserControl";
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.TabIndex = 0;
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.UserControlType = typeof(Enterprise.ZArchitecture.GUI.DynamicLayoutPanel);
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.HostedControlCreated += new System.EventHandler(this.GoodsItemPackagesAndContainersDynamicCreationUserControl_HostedControlCreated);
			// 
			// GoodsItemSupportingDocumentsTabPage
			// 
			this.GoodsItemSupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B42F30A8-E144-4C95-8AA0-0BF0C1569FC7", "Supporting Documents");
			this.GoodsItemSupportingDocumentsTabPage.Controls.Add(this.GoodsItemSupportingDocumentsTabUserControl);
			this.GoodsItemSupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemSupportingDocumentsTabPage.Name = "GoodsItemSupportingDocumentsTabPage";
			this.GoodsItemSupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemSupportingDocumentsTabPage.TabIndex = 0;
			// 
			// GoodsItemSupportingDocumentsTabUserControl
			// 
			this.GoodsItemSupportingDocumentsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemSupportingDocumentsTabUserControl, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).SupportingDocuments)));
			this.GoodsItemSupportingDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemSupportingDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemSupportingDocumentsTabUserControl.Name = "GoodsItemSupportingDocumentsTabUserControl";
			this.GoodsItemSupportingDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemSupportingDocumentsTabUserControl.TabIndex = 0;
			// 
			// GoodsItemAdditionalDocumentsTabPage
			// 
			this.GoodsItemAdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("cbc63fc3-67ab-446e-b075-51874cb0c888", "Additional Documents");
			this.GoodsItemAdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemAdditionalDocumentsTabPage.Name = "GoodsItemAdditionalDocumentsTabPage";
			this.GoodsItemAdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemAdditionalDocumentsTabPage.TabIndex = 1;
			// 
			// GoodsItemPreviousDocumentsTabPage
			// 
			this.GoodsItemPreviousDocumentsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("198C00B7-6B13-445B-BDEB-064C12CE8DC6", "Previous Documents");
			this.GoodsItemPreviousDocumentsTabPage.Controls.Add(this.GoodsItemPreviousDocumentsTabUserControl);
			this.GoodsItemPreviousDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemPreviousDocumentsTabPage.Name = "GoodsItemPreviousDocumentsTabPage";
			this.GoodsItemPreviousDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 299, true);
			this.GoodsItemPreviousDocumentsTabPage.TabIndex = 0;
			// 
			// GoodsItemPreviousDocumentsTabUserControl
			// 
			this.GoodsItemPreviousDocumentsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemPreviousDocumentsTabUserControl, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsPreviousDocument>)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).PreviousDocuments)));
			this.GoodsItemPreviousDocumentsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemPreviousDocumentsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemPreviousDocumentsTabUserControl.Name = "GoodsItemPreviousDocumentsTabUserControl";
			this.GoodsItemPreviousDocumentsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1336, 299, true);
			this.GoodsItemPreviousDocumentsTabUserControl.TabIndex = 0;
			// 
			// GoodsItemSupplyChainActorsTabPage
			// 
			this.GoodsItemSupplyChainActorsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E20537F7-1397-456B-8F1D-14B6BEB88C09", "Supply Chain Actors");
			this.GoodsItemSupplyChainActorsTabPage.Controls.Add(this.GoodsItemSupplyChainActorsTabUserControl);
			this.GoodsItemSupplyChainActorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.GoodsItemSupplyChainActorsTabPage.Name = "GoodsItemSupplyChainActorsTabPage";
			this.GoodsItemSupplyChainActorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemSupplyChainActorsTabPage.TabIndex = 0;
			// 
			// GoodsItemSupplyChainActorsTabUserControl
			// 
			this.GoodsItemSupplyChainActorsTabUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsItemSupplyChainActorsTabUserControl, "CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ICusSupplyChainActorReferenceCollection<Enterprise.Customs.EU.NCTS.Business.CusSupplyChainActorReference>)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureCargoDesc)(null)).CusSupplyChainActorReferences)));
			this.GoodsItemSupplyChainActorsTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GoodsItemSupplyChainActorsTabUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GoodsItemSupplyChainActorsTabUserControl.Name = "GoodsItemSupplyChainActorsTabUserControl";
			this.GoodsItemSupplyChainActorsTabUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 292, true);
			this.GoodsItemSupplyChainActorsTabUserControl.TabIndex = 0;
			// 
			// Phase5GoodsItemsTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsItemsSplitContainer);
			this.Name = "Phase5GoodsItemsTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 490, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsItemsSplitContainer.Panel1.ResumeLayout(false);
			this.GoodsItemsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GoodsItemsSplitContainer)).EndInit();
			this.GoodsItemsSplitContainer.ResumeLayout(false);
			this.GoodsItemsSplitContainer.PerformLayout();
			this.GoodsItemTabControl.ResumeLayout(false);
			this.GoodsItemTabControl.PerformLayout();
			this.GoodsItemDetailsTabPage.ResumeLayout(false);
			this.GoodsItemDetailsTabPage.PerformLayout();
			this.GoodsItemPackagesAndContainersTabPage.ResumeLayout(false);
			this.GoodsItemPackagesAndContainersTabPage.PerformLayout();
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.ResumeLayout(true);
			this.GoodsItemPackagesAndContainersDynamicCreationUserControl.PerformLayout();
			this.GoodsItemSupportingDocumentsTabPage.ResumeLayout(false);
			this.GoodsItemSupportingDocumentsTabPage.PerformLayout();
			this.GoodsItemSupportingDocumentsTabUserControl.ResumeLayout(true);
			this.GoodsItemSupportingDocumentsTabUserControl.PerformLayout();
			this.GoodsItemAdditionalDocumentsTabPage.ResumeLayout(false);
			this.GoodsItemAdditionalDocumentsTabPage.PerformLayout();
			this.GoodsItemPreviousDocumentsTabPage.ResumeLayout(false);
			this.GoodsItemPreviousDocumentsTabPage.PerformLayout();
			this.GoodsItemPreviousDocumentsTabUserControl.ResumeLayout(true);
			this.GoodsItemPreviousDocumentsTabUserControl.PerformLayout();
			this.GoodsItemSupplyChainActorsTabPage.ResumeLayout(false);
			this.GoodsItemSupplyChainActorsTabPage.PerformLayout();
			this.GoodsItemSupplyChainActorsTabUserControl.ResumeLayout(true);
			this.GoodsItemSupplyChainActorsTabUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer GoodsItemsSplitContainer;
		public ZArchitecture.GUI.ZTabControl GoodsItemTabControl;
		internal ZArchitecture.GUI.ZTabPage GoodsItemDetailsTabPage;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicGoodsItemDetailsPanel;
		protected internal ZArchitecture.GUI.ZTabPage GoodsItemPackagesAndContainersTabPage;
		internal Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl GoodsItemPackagesAndContainersDynamicCreationUserControl;
		internal ZArchitecture.GUI.ZTabPage GoodsItemPreviousDocumentsTabPage;
		internal Phase5GoodsItemPreviousDocumentsTabUserControl GoodsItemPreviousDocumentsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage GoodsItemSupportingDocumentsTabPage;
		internal Phase5GoodsItemSupportingDocumentsTabUserControl GoodsItemSupportingDocumentsTabUserControl;
		internal ZArchitecture.GUI.ZTabPage GoodsItemAdditionalDocumentsTabPage;
		protected internal ZArchitecture.GUI.ZTabPage GoodsItemSupplyChainActorsTabPage;
		internal Phase5GoodsItemSupplyChainActorsTabUserControl GoodsItemSupplyChainActorsTabUserControl;
		private System.ComponentModel.IContainer components;
	}
}

