using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStoragePackedItemControl : ZUserControl
	{
		public UCC6TemporaryStoragePackedItemControl()
		{
			InitializeComponent();
			PackedItemDetailsTabPage.RunWhenBindingOrFirstShown((s, args) => InitializePackedItemGridAndPackedItemDetailTabPage());
			InitializePackPackedItemPivotTabPage();
			InitializeSupplyChainActorTabPage();
			SupportingDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitializeSupportingDocumentsTabPage());
			PreviousDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitPreviousDocumentsUserControl());
			AdditionalInfoTabPage.RunWhenBindingOrFirstShown((s, args) => InitializeAdditionalInformationTabPage());
		}

		void InitializePackedItemGridAndPackedItemDetailTabPage()
		{
			var panelLayoutWithGrid = LayoutProvider?.GetTemporaryStoragePackedItemWithGridLayout();
			PackedItemDetailsLayoutPanel.UpdateLayout(panelLayoutWithGrid);
			var packedItemGridControl = (ZUserControl)Activator.CreateInstance(panelLayoutWithGrid.GridUserControlType);
			packedItemGridControl.Dock = System.Windows.Forms.DockStyle.Fill;
			BindingSource.SetBindingMember(packedItemGridControl, ".");
			this.TopPanel.Controls.Add(packedItemGridControl);
		}

		ITemporaryStorageLayoutProvider fLayoutProvider;
		ITemporaryStorageLayoutProvider LayoutProvider => fLayoutProvider ?? (fLayoutProvider = TemporaryStorageLayoutProviderHelper.GetLayoutProvider(Header));

		void InitializeSupplyChainActorTabPage()
		{
			BindingSource.SetBindingMember(this.SupplyChainActorTabUserControl, "Bills.PackedItems.SupplyChainActors");
			SupplyChainActorTabUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		void InitializeSupportingDocumentsTabPage()
		{
			var supportingDocumentsUserControl = new UCC6TemporaryStorageSupportingDocumentsUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(supportingDocumentsUserControl, "Bills.PackedItems", SupportingInfoColumnLayoutContext);
			SupportingDocumentsLayoutPanel.Controls.Add(supportingDocumentsUserControl);
		}

		void InitPreviousDocumentsUserControl()
		{
			var previousDocumentsUserControl = new UCC6TemporaryStoragePreviousDocumentsUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(previousDocumentsUserControl, "Bills.PackedItems", SupportingInfoColumnLayoutContext);
			PreviousDocumentsLayoutPanel.Controls.Add(previousDocumentsUserControl);
		}

		void InitializeAdditionalInformationTabPage()
		{
			var additionalInfoUserControl = new UCC6TemporaryStorageAdditionalInfosUserControlWithGrid();
			SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalInfoUserControl, "Bills.PackedItems", SupportingInfoColumnLayoutContext);
			AdditionalInformationLayoutPanel.Controls.Add(additionalInfoUserControl);
		}

		const string SupportingInfoColumnLayoutContext = "STO";

		void InitializePackPackedItemPivotTabPage()
		{
			var packPackedItemPivotControl = (ZUserControl)Activator.CreateInstance<UCC6BillsPackedItemPackingPivotControl>();
			PackPackedItemPivotTabPage.Controls.Add(packPackedItemPivotControl);
			BindingSource.SetBindingMember(packPackedItemPivotControl, ".");
			packPackedItemPivotControl.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		TemporaryStorageHeader Header => CurrentDataItem as TemporaryStorageHeader;
	}
}
