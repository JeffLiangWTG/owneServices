using System;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public partial class UCC6TemporaryStorageAdditionalInfosUserControlWithGrid : ZUserControl, ISupportingInfoUserControls, Integration.Customs.EU.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid
	{
		public UCC6TemporaryStorageAdditionalInfosUserControlWithGrid()
		{
			InitializeComponent();
			AdditionalInfosGrid.AfterBind += AdditionalInfosGrid_AfterBind;
			DetailsLayoutControl.AllowOutsideOfParent();
		}

		void AdditionalInfosGrid_AfterBind(object sender, EventArgs e)
		{
			AdditionalInfosGridColumnsVisible();
			SetAdditionalInfosLayout();
			AdjustControlProperties();
		}

		protected virtual void AdditionalInfosGridColumnsVisible()
		{
			if (IsBoundToUCC6TemporaryStorageBill || IsBoundToUCC6TemporaryStorageBillPackedItem)
			{
				AdditionalInfosGrid.SetAvailability(false, [AdditionalInfo.Schema.CSI_RX_NKCurrency, AdditionalInfo.Schema.CSI_Value, AdditionalInfo.Schema.CSI_ReferenceNumber2]);
			}
		}

		void SetAdditionalInfosLayout()
		{
			DetailsLayoutControl.SetLayout(CreateNewUCC6TemporaryStorageAdditionalInformationDetailsLayout());
		}

		protected virtual void AdjustControlProperties()
		{
			AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("80835990-FF99-458F-AFB8-C2A94305BF29", "Additional Information");
			AdditionalInfosPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 50, true);
			AdditionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 50, true);
			this.Dock = System.Windows.Forms.DockStyle.Fill;
		}

		public string GridBindingMember => nameof(TemporaryStorageHeader.Bills);

		public ZGrid Grid => AdditionalInfosGrid;

		protected virtual IPanelLayoutProvider CreateNewUCC6TemporaryStorageAdditionalInformationDetailsLayout() => new UCC6TemporaryStorageAdditionalInformationDetailsLayoutWithGrid(AdditionalInfosGrid.DataMember);

		protected bool IsBoundToUCC6TemporaryStorageBill => AdditionalInfosGrid.DataMember == UCC6TemporaryStorageBillAdditionalInfoBingdingMemberName;
		protected bool IsBoundToUCC6TemporaryStorageBillPackedItem => AdditionalInfosGrid.DataMember == UCC6TemporaryStorageBillPackedItemAdditionalInfoBingdingMemberName;

		public const string UCC6TemporaryStorageBillAdditionalInfoBingdingMemberName = nameof(TemporaryStorageHeader.Bills) + "." + nameof(TemporaryStorageBill.AdditionalInfos);
		public const string UCC6TemporaryStorageBillPackedItemAdditionalInfoBingdingMemberName = nameof(TemporaryStorageHeader.Bills) + "." + nameof(TemporaryStorageBill.PackedItems) + "." + nameof(TemporaryStoragePackedItem.AdditionalInfos);
	}
}
