using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
	public sealed partial class AsycudaPackUserControl : ManifestSpecificProviderUserControl, IAdditionalTabPage
	{
		public AsycudaPackUserControl()
		{
			InitializeComponent();
			new UNDGDataItemFormManager(PacksGrid, "", UNDGDataItemFormManagerConfig.IMOHideProperties()).Initialize();
		}

		public new AsycudaBill CurrentDataItem => (AsycudaBill)base.CurrentDataItem;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			AMA_TransportModeInfo_ValueChanged(null, null);
			SetMandatoryPacksGridColumns();

			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
			SetVisibility(provider);
		}

		protected override ZUserControl GetManifestSpecificUserControl(ApplicationGUIProvider provider) => (CurrentDataItem?.IsNonePackedItemRelationship ?? false) ? null : new AsycudaPackedItemsUserControl();
		protected override Control ControlToAddManifestSpecificUserControl => PackCountrySplitContainer.Panel2;
		protected override string ManifestSpecificUserControlDataMember => "Packs";

		protected override void UnHookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
			header.AMA_ManifestTypeInfo.ValueChanged -= AMA_ManifestTypeInfo_ValueChanged;
		}

		protected override void HookManifestHeaderEventsCore(AsycudaManifestHeader header)
		{
			header.AMA_ManifestTypeInfo.ValueChanged += AMA_ManifestTypeInfo_ValueChanged;
		}

		protected override void AMA_TransportModeInfo_ValueChangedCore(object sender, EventArgs e)
		{
			using (PacksGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var isAir = Header?.IsAir ?? false;
				PacksGrid.SetAvailability(!isAir, AsycudaPack.Schema.ContainerPK);
				PacksGrid.SetAvailability(Header?.ShowVINNumbers ?? false, AsycudaPack.Schema.APA_VINNumber);

				var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
				if (provider?.ShouldCheckPacksGridColumnAvailability() ?? false)
				{
					SetPacksGridColumnAvailability(provider, Header);
				}
			}
		}

		protected override void AMA_ManifestTypeInfo_ValueChangedCore(object sender, EventArgs e)
		{
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
			SetVisibility(provider);
		}

		protected override void SpecificCircumstanceIndicatorInfo_ValueChangedCore(object sender, EventArgs e)
		{
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(Header);
			SetVisibility(provider);
		}

		void SetVisibility(ApplicationGUIProvider provider)
		{
			var header = Header;
			var showPackedItems = header?.ShowPackedItems ?? true;
			PackCountrySplitContainer.Panel2Collapsed = !showPackedItems;

			using (PacksGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				SetPacksGridColumnVisibility(provider);
				SetPackGridExtraColumnInfosVisibility(provider);
				var packsGridColumnsOrder = provider?.GetPacksGridColumnsOrder();
				if (packsGridColumnsOrder != null && packsGridColumnsOrder.Length > 0)
				{
					var columnsToRemove = new List<string>();
					foreach (var column in PacksGrid.Columns)
					{
						if (!packsGridColumnsOrder.Contains(column.ColumnName))
						{
							columnsToRemove.Add(column.ColumnName);
						}
					}

					foreach (var item in columnsToRemove)
					{
						PacksGrid.Columns.Remove(item);
					}

					PacksGrid.ReOrderColumns(packsGridColumnsOrder);
				}

				if (header?.IsOnePackedItemRelationship ?? false)
				{
					var packedItemColumns = provider?.GetPackedItemColumns() ?? GetPackedItemColumns();
					var packItemStandAloneCountryProperty = nameof(AsycudaPack.PackedItem) + "+";
					PacksGrid.SetAvailability(showPackedItems, packedItemColumns.Select(x => packItemStandAloneCountryProperty + x).ToArray());

					if (showPackedItems)
					{
						SetPacksGridColumnAvailability(provider, header);
					}

					var tariffColumnStyle = PacksGrid.GetColumnStyle(packItemStandAloneCountryProperty + AsycudaPackedItem.Schema.API_FormattedTariff) as Universal.GUI.TariffColumnStyleInfo;
					if (tariffColumnStyle != null)
					{
						var applicationBusinessProvider = header.ApplicationBusinessProvider;
						tariffColumnStyle.GetCountryCode = () => header.AMA_RN_NKCountry;
						tariffColumnStyle.GetDataGrouping = () => applicationBusinessProvider.PackedItemTariffDataGrouping;
						tariffColumnStyle.TariffType = applicationBusinessProvider.PackedItemTariffType;
						tariffColumnStyle.SelectNomenclatureModes = applicationBusinessProvider.SelectNomenclatureModes;
						if (CurrentDataItem != null)
						{
							tariffColumnStyle.GetEffectiveDate = () => { return applicationBusinessProvider.GetEffectiveDateForDutyRate(Header); };
						}
					}
				}
			}
		}

		void SetPacksGridColumnAvailability(ApplicationGUIProvider provider, AsycudaManifestHeader header)
		{
			var packsGridColumnAvailability = provider?.GetPacksGridColumnAvailability(header);
			if (packsGridColumnAvailability != null)
			{
				foreach (var packsGridColumnAvailabilityData in packsGridColumnAvailability)
				{
					PacksGrid.SetAvailability(packsGridColumnAvailabilityData.Key, packsGridColumnAvailabilityData.Value);
				}
			}
		}

		void SetPackGridExtraColumnInfosVisibility(ApplicationGUIProvider provider)
		{
			if (provider != null)
			{
				if (packsGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in packsGridExtraColumnInfos)
					{
						PacksGrid.ColumnStyles.Remove(columnInfo);
					}
				}
				packsGridExtraColumnInfos = provider.GetPacksGridExtraColumnInfos().ToArray();
				if (packsGridExtraColumnInfos != null)
				{
					foreach (var columnInfo in packsGridExtraColumnInfos)
					{
						PacksGrid.ColumnStyles.Add(columnInfo);
					}
				}
			}
		}

		void SetPacksGridColumnVisibility(ApplicationGUIProvider provider)
		{
			if (provider != null)
			{
				var columns = provider.GetPacksGridColumnVisibility();
				if (columns != null)
				{
					foreach (var columnInfo in columns)
					{
						PacksGrid.SetColumnVisible(columnInfo.Key, columnInfo.Value);
					}
				}
			}
		}

		ZGridColumnInfo[] packsGridExtraColumnInfos;

		string[] GetPackedItemColumns()
		{
			return new string[]
			{
				AsycudaPackedItem.Schema.API_GoodsDescription,
				AsycudaPackedItem.Schema.API_CustomsQty,
				AsycudaPackedItem.Schema.API_CustomsUQ,
				AsycudaPackedItem.Schema.API_CustomsValue,
				AsycudaPackedItem.Schema.API_DutyAmount,
				AsycudaPackedItem.Schema.API_TaxAmount,
				AsycudaPackedItem.Schema.API_RN_NKGoodsOrigin,
				AsycudaPackedItem.Schema.API_MessageStatus,
				AsycudaPackedItem.Schema.API_PackStatus
			};
		}

		void SetMandatoryPacksGridColumns()
		{
			var header = Header ?? CurrentDataItem?.Header;
			var provider = ApplicationGUIProvider.GetApplicationGuiProvider(header);

			using (PacksGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				var packsGridMandatoryColumns = provider?.GetPacksGridMandatoryColumns();
				if (packsGridMandatoryColumns != null && packsGridMandatoryColumns.Length > 0)
				{
					foreach (var column in PacksGrid.Columns)
					{
						if (packsGridMandatoryColumns.Contains(column.ColumnName))
						{
							PacksGrid.SetColumnMandatory(column.ColumnName, true);
						}
					}
				}
			}
		}

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;
		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Enterprise.Customs.ASYCUDA.Gui.Res.GetData("e680c6bb-d6bb-4b1c-806c-5bdbf57124fe", "Packs");
		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);
		int IAdditionalTabPage.TabPageSequence => 0;
	}
}
