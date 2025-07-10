using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class G5V1TemporaryStoragePackedItemControl : UCC6TemporaryStoragePackedItemControl
	{
		public G5V1TemporaryStoragePackedItemControl() : base()
		{
			InitializeComponent();
			HideSupplyChainActorTab();
			SetNewCaptions();
		}

		void HideSupplyChainActorTab()
		{
			var packedItemTabControl = this.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
			var billsTabPage = packedItemTabControl.GetTabPageByNameOrText("SupplyChainActorTabPage");
			packedItemTabControl.TabPages.Remove(billsTabPage);
		}

		void SetNewCaptions()
		{
			var packedItemTabControl = this.FindSingle<ZTemplateTabControl>("PackedItemTabControl");
			var packedItemDetailsTabPage = packedItemTabControl.GetTabPageByNameOrText("PackedItemDetailsTabPage");
			packedItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("DB2CA195-6900-4E83-BD7D-B551F619E26D", "Details");
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem is TemporaryStorageHeader header)
			{
				header.AMA_MessageTypeInfo.ValueChanged -= TemporaryStorageHeader_AMA_MessageTypeChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (CurrentDataItem is TemporaryStorageHeader header)
			{
				header.AMA_MessageTypeInfo.ValueChanged += TemporaryStorageHeader_AMA_MessageTypeChanged;
				TemporaryStorageHeader_AMA_MessageTypeChanged(header, null);
			}
			base.OnCurrentDataItemChanged(e);
		}

		void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs e)
		{
			if (sender is TemporaryStorageHeader header)
			{
				HideTabsForLAME(header.IsMessageTypeLAM);
			}
		}
		void HideTabsForLAME(ZBool isLAME)
		{
			PreviousDocumentsTabPage.TabVisible = !isLAME;
			AdditionalInfoTabPage.TabVisible = !isLAME;
		}
	}
}
