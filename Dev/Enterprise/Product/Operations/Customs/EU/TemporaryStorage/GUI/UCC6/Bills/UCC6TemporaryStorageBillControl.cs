using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI.PlugIn;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStorageBillControl : ZUserControl
	{
		public UCC6TemporaryStorageBillControl()
		{
			InitializeComponent();
			BillDetailsTabPage.RunWhenBindingOrFirstShown((s, args) => InitializeBillGridAndBillDetailTabPage());
			InitializeBillPartiesTabPage();
			InitializeSupplyChainActorTabPage();
			PackedItemsTabPage.RunWhenBindingOrFirstShown((s, args) => InitPackedItemsUserControl());
			SupportingDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitializeSupportingDocumentsTabPage());
			PreviousDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitPreviousDocumentsUserControl());
			AdditionalInformationTabPage.RunWhenBindingOrFirstShown((s, args) => InitializeAdditionalInformationTabPage());
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (Header != null)
			{
				Header.AMA_MessageTypeInfo.ValueChanged += TemporaryStorageHeader_AMA_MessageTypeChanged;
				TemporaryStorageHeader_AMA_MessageTypeChanged(this, EventArgs.Empty);
			}
		}

		void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs e)
		{
			if (Header != null)
			{
				SetupTabPagesVisibility();
			}
		}

		void SetupTabPagesVisibility()
		{
			var temporaryStorageBillDetailTabLayout = LayoutProvider.GetTemporaryStorageBillDetailTabLayout();
			var isTransferHeaderFlag = Header.IsTransfer;

			PackedItemsTabPage.TabVisible = !isTransferHeaderFlag;
			BillPartiesTabPage.TabVisible = !isTransferHeaderFlag;
			SupportingDocumentsTabPage.TabVisible = !isTransferHeaderFlag && temporaryStorageBillDetailTabLayout.IsSupportingDocumentsTabVisible;
			PreviousDocumentsTabPage.TabVisible = !isTransferHeaderFlag;
			AdditionalInformationTabPage.TabVisible = !isTransferHeaderFlag && temporaryStorageBillDetailTabLayout.IsAdditionalInformationTabVisible;
			SupplyChainActorTabPage.TabVisible = !isTransferHeaderFlag;
		}

		TemporaryStorageHeader Header => CurrentDataItem as TemporaryStorageHeader;

		void InitializeBillGridAndBillDetailTabPage()
		{
			var panelLayoutWithGrid = LayoutProvider.GetTemporaryStorageBillWithGridLayout();
			BillDetailsLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			var billGridControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			SplitContainer.Panel1.Controls.Add(billGridControl);
			BindingSource.SetBindingMember(billGridControl, ".");
			billGridControl.Dock = DockStyle.Fill;
		}

		void InitializeBillPartiesTabPage() => BillPartiesLayoutPanel.UpdateLayout(new UCC6TemporaryStorageBillPartiesLayout());

		void InitPackedItemsUserControl()
		{
			var packedItemsUserControl = new UCC6TemporaryStoragePackedItemControl();
			packedItemsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			PackedItemsLayoutPanel.Controls.Add(packedItemsUserControl);
		}

		void InitializeSupplyChainActorTabPage()
		{
			BindingSource.SetBindingMember(this.SupplyChainActorTabUserControl, "Bills.SupplyChainActors");
			SupplyChainActorTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		void InitializeSupportingDocumentsTabPage()
		{
			var supportingDocumentsUserControl = new UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(supportingDocumentsUserControl, (NoResString)"Bills", SupportingInfoColumnLayoutContext);
			SupportingDocumentsLayoutPanel.Controls.Add(supportingDocumentsUserControl);
		}

		void InitPreviousDocumentsUserControl()
		{
			var previousDocumentsUserControl = new UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(previousDocumentsUserControl, (NoResString)"Bills", SupportingInfoColumnLayoutContext);
			PreviousDocumentsLayoutPanel.Controls.Add(previousDocumentsUserControl);
		}

		void InitializeAdditionalInformationTabPage()
		{
			var additionalInfoUserControl = UCC6TemporaryStorageBillControlHelper.GetUCC6TemporaryStorageAdditionalInfosUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalInfoUserControl, (NoResString)"Bills", SupportingInfoColumnLayoutContext);
			AdditionalInformationLayoutPanel.Controls.Add(additionalInfoUserControl);
		}

		ITemporaryStorageLayoutProvider LayoutProvider => layoutProvider ??= TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header);
		ITemporaryStorageLayoutProvider layoutProvider;

		const string SupportingInfoColumnLayoutContext = "STO";
	}
}
