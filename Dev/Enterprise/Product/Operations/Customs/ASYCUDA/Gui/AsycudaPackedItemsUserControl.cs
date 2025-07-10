using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaPackedItemsUserControl : ZUserControl, IResetMessageStatusSupporter
	{
		public AsycudaPackedItemsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			manifestHeader = dataSource as AsycudaManifestHeader;
			AddPackAdditionalTabPage(manifestHeader);

			if (manifestHeader != null)
			{
				dataMember = "Bills.Packs";
			}
			else
			{
				var bill = dataSource as AsycudaBill;
				if (bill != null)
				{
					manifestHeader = bill.Header;
					dataMember = "Packs";
				}
			}
			base.SetDataBinding(dataSource, dataMember);

			SetControlVisibility();
		}
		AsycudaManifestHeader manifestHeader;

		void SetControlVisibility()
		{
			if (manifestHeader != null)
			{
				ItemsGroupBox.Visible = manifestHeader.IsManyPackedItemRelationship;
				DynamicPackedItemDetailsPanel.Visible = manifestHeader.IsOnePackedItemRelationship;

				if (manifestHeader.IsOnePackedItemRelationship)
				{
					var provider = ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader);
					var layout = provider?.GetPackedItemDetailsLayout();
					DynamicPackedItemDetailsPanel.UpdateLayout(layout);
					CustomiseMenuItemOnPackItemMessageStatus();
				}
				else if (manifestHeader.IsManyPackedItemRelationship)
				{
					if (manifestHeader.SupportUNDGsOnPackedItemLevel)
					{
						new UNDGDataItemFormManager(PackedItemsGrid, "", UNDGDataItemFormManagerConfig.IMOHideProperties()).Initialize();
					}

					var packedItemColumns = ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader)?.GetPackedItemColumns();
					if (packedItemColumns != null)
					{
						using (PackedItemsGrid.SuspendRefreshTableStyles())
						{
							PackedItemsGrid.SetAllAvailability(false);
							PackedItemsGrid.SetAvailability(true, packedItemColumns);
							PackedItemsGrid.ReOrderColumns(packedItemColumns);
							PackedItemsGrid.SetColumnVisible(true, packedItemColumns);

							if (packedItemColumns.Contains(AsycudaPackedItem.Schema.API_FormattedTariff) || packedItemColumns.Contains(AsycudaPackedItem.Schema.API_Tariff))
							{
								var tariffColumnStyles = PackedItemsGrid.ColumnStyles.OfType<Universal.GUI.TariffColumnStyleInfo>().ToArray();
								if (tariffColumnStyles.Length > 0
									&& manifestHeader.ApplicationBusinessProvider is ApplicationBusinessProvider applicationBusinessProvider)
								{
									foreach (var tariffColumnStyle in tariffColumnStyles)
									{
										tariffColumnStyle.GetCountryCode = () => manifestHeader.AMA_RN_NKCountry;
										tariffColumnStyle.GetDataGrouping = () => applicationBusinessProvider.PackedItemTariffDataGrouping;
										tariffColumnStyle.TariffType = applicationBusinessProvider.PackedItemTariffType;
										tariffColumnStyle.SelectNomenclatureModes = applicationBusinessProvider.SelectNomenclatureModes;
										if (CurrentDataItem != null)
										{
											tariffColumnStyle.GetEffectiveDate = () => { return applicationBusinessProvider.GetEffectiveDateForDutyRate(manifestHeader); };
										}
									}
								}
							}
						}
					}
				}
			}
		}

		UserControlGenerateHelper additionalPackLevelUserControlsHelper;

		void AddPackAdditionalTabPage(AsycudaManifestHeader manifestHeader)
		{
			if (manifestHeader != null && additionalPackLevelUserControlsHelper == null)
			{
				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(manifestHeader);
				additionalPackLevelUserControlsHelper = new UserControlGenerateHelper(manifestHeader, provider.GetPackAdditionalTabPageUserControl());
				additionalPackLevelUserControlsHelper.AddAdditionalTabPages(PackedItemDetailsTabControl, this.BindingSource, "", () => new PackAdditionalTabPageUserControl(), 0);
			}
		}

		void CustomiseMenuItemOnPackItemMessageStatus()
		{
			var messageStatusTextBox = DynamicPackedItemDetailsPanel.Controls.Find(nameof(CommonPackedItemDetailsControlBag.MessageStatusTextBox), true).FirstOrDefault();
			if (messageStatusTextBox != null && messageStatusTextBox.ContextMenu == null)
			{
				resetMessageStatus = ASYCUDA.GUI.ResetMessageStatusHelper.CreateResetMessageStatusMenuItem(messageStatusTextBox, ResetMessageStatus_Click, ResetMessageStatus_Popup);
			}
		}

		void IResetMessageStatusSupporter.ResetMessageStatus_Popup() => ResetMessageStatus_Popup();
		void ResetMessageStatus_Popup() => ASYCUDA.GUI.ResetMessageStatusHelper.ResetMessageStatus_PopupPackLevel(CurrentDataItem.PackedItem, resetMessageStatus, false);

		void IResetMessageStatusSupporter.ResetMessageStatus_Click() => ResetMessageStatus_Click();
		void ResetMessageStatus_Click() => ASYCUDA.GUI.ResetMessageStatusHelper.ResetMessageStatus_ClickPackLevel(CurrentDataItem.PackedItem);

		public new AsycudaPack CurrentDataItem => (AsycudaPack)base.CurrentDataItem;

		ZMenuItem resetMessageStatus;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				additionalPackLevelUserControlsHelper?.DisposeTabPages();
			}
			base.Dispose(disposing);
		}
	}
}
