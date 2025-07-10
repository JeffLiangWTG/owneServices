using System;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class G5V1TemporaryStorageForm : TemporaryStorageForm
	{
		public G5V1TemporaryStorageForm(TemporaryStorageHeader header)
			: base(header)
		{
			InitializeComponent();
			InitializeBillPartiesTabPage();
			InitPackedItemsUserControl();
			InitPackedUserControl();
			esHeader = header;
			ChangeTabs();
		}

		readonly TemporaryStorageHeader esHeader;

		void ChangeTabs()
		{
			if (!esHeader.IsMessageType_G5E_G5R)
			{
				var billsTabPage = MainTabControl.GetTabPageByNameOrText("BillsTabPage");
				MainTabControl.TabPages.Remove(billsTabPage);
			}

			var mainTabIndex = MainTabControl.TabPages.IndexOf(MainTabPage);
			MainTabControl.TabPages.Insert(BillPartiesTabPage, mainTabIndex + 1);

			var containerTabPage = MainTabControl.GetTabPageByNameOrText("ContainerTabPage");
			var containersIndex = MainTabControl.TabPages.IndexOf(containerTabPage);

			MainTabControl.TabPages.Insert(PacksTabPage, containersIndex + 1);
			MainTabControl.TabPages.Insert(PackedItemsTabPage, containersIndex + 2);
		}

		void InitializeBillPartiesTabPage() => BillPartiesLayoutPanel.UpdateLayout(new G5V1TemporaryStorageBillPartiesLayout());

		protected override void TemporaryStorageHeader_AMA_MessageTypeChanged(object sender, EventArgs e)
		{
			if (esHeader != null)
			{
				ContainerTabPage.TabVisible = !esHeader.IsMessageTypeManual;
				MessagesTabPage.TabVisible = !esHeader.IsMessageTypeLAM;
			}
		}

		void TemporaryStorageHeaderUnionGoods_ValueChanged(object sender, EventArgs e)
		{
			if (esHeader != null)
			{
				esHeader.Guarantee.ReadOnly = esHeader.UnionGoods;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (esHeader != null)
			{
				esHeader.AMA_MessageStatusInfo.ValueChanged += TemporaryStorageHeader_IsSent_ValueChanged;
				esHeader.CustomsStatusInfo.ValueChanged += TemporaryStorageHeader_IsSent_ValueChanged;
				esHeader.UnionGoodsInfo.ValueChanged += TemporaryStorageHeaderUnionGoods_ValueChanged;

				TemporaryStorageHeader_IsSent_ValueChanged(this, EventArgs.Empty);
				TemporaryStorageHeaderUnionGoods_ValueChanged(this, EventArgs.Empty);
			}
		}

		protected virtual void TemporaryStorageHeader_IsSent_ValueChanged(object sender, EventArgs e)
		{
			SetReadOnlyFieldsForSentDeclaration();
		}

		internal void SetReadOnlyFieldsForSentDeclaration()
		{
			var isSent = esHeader.IsSent;
			if (MainTabPage.TabVisible && isSent)
			{
				var mainPanel = MainDynamicLayoutPanel;

				var documentsTab = mainPanel.FindSingle<ZTemplateTabControl>("DocumentsTabControl");
				documentsTab?.SetReadOnlyIncludingChildren();
				var guaranteeTab = mainPanel.FindSingle<DynamicLayoutPanel>("DynamicGuaranteePanel");
				guaranteeTab?.SetReadOnlyIncludingChildren();
				var departureGoodsLocation = mainPanel.FindSingle<LocationOfGoodsUserControl>("LocationOfGoodsUserControl");
				departureGoodsLocation?.SetReadOnlyIncludingChildren();
				var destinationGoodsLocation = mainPanel.FindSingle<DestinationLocationOfGoodsUserControl>("DestinationLocationOfGoodsUserControl");
				destinationGoodsLocation?.SetReadOnlyIncludingChildren();
				var lamTransportDocumentType = mainPanel.FindSingle<ZDropEdit>("TransportDocumentTypeDropEdit");
				lamTransportDocumentType?.SetReadOnlyIncludingChildren();
				var lamTransportDocumentTextBox = mainPanel.FindSingle<ZTextBox>("TransportDocumentTextBox");
				lamTransportDocumentTextBox?.SetReadOnlyIncludingChildren();
			}

			if (ContainerTabPage.TabVisible && isSent)
			{
				ContainerTabPage.SetReadOnlyIncludingChildren();
			}

			if (PackedItemsTabPage.TabVisible && isSent)
			{
				PackedItemsTabPage.SetReadOnlyIncludingChildren();
			}

			if (PacksTabPage.TabVisible && isSent)
			{
				PacksTabPage.SetReadOnlyIncludingChildren();
			}

			if (BillPartiesTabPage.TabVisible && isSent)
			{
				BillPartiesTabPage.SetReadOnlyIncludingChildren();
			}
		}

		void InitPackedItemsUserControl()
		{
			var packedItemsUserControl = new G5V1TemporaryStoragePackedItemControl();
			packedItemsUserControl.Name = "G5V1TemporaryStoragePackedItemControl";
			packedItemsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			PackedItemsLayoutPanel.Controls.Add(packedItemsUserControl);
		}

		void InitPackedUserControl()
		{
			var packedUserControl = new UCC6TemporaryStoragePackagesControl();
			packedUserControl.Name = "UCC6TemporaryStoragePackagesControl";
			packedUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			PacksLayoutPanel.Controls.Add(packedUserControl);
		}

		protected override ZMenuItem GetNewMessagingMenu() => new G5V1TemporaryStorageMessagesMenu(this);
	}
}
