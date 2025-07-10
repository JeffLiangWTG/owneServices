using System;
using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public sealed class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder((AsycudaManifestHeader)header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new ManifestLayouts();

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new ILBillPartiesLayouts();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new AsycudaPackUserControl();
			yield return new AsycudaPackedItemControl();
			yield return new AsycudaAdditionalInfoUserControl();
			yield return new AsycudaTransportDocumentsUserControl();
			yield return new AsycudaSupportingDocumentUserControl();
		}

		protected override ContainerCountrySpecificUserControl GetContainerCountrySpecificUserControlCore()
			=> new ContainerSpecificUserControl();

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var zCodeFindBoxColumnStyleInfoPortOfDischarge = new ZCodeFindBoxColumnStyleInfo();
			zCodeFindBoxColumnStyleInfoPortOfDischarge.ColumnName = nameof(AsycudaBill.ABL_RL_NKPortOfDischarge);
			zCodeFindBoxColumnStyleInfoPortOfDischarge.IsMandatory = true;
			zCodeFindBoxColumnStyleInfoPortOfDischarge.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			yield return zCodeFindBoxColumnStyleInfoPortOfDischarge;

			var billStatusGroupName = Res.GetData("CE88A206-EA9C-4F5E-8442-25DDFED6F7E3", "Bill Status");
			var zDropEditColumnStyleInfoBillStatus = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfoBillStatus.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfoBillStatus.ColumnName = nameof(AsycudaBill.ABL_BillStatus);
			zDropEditColumnStyleInfoBillStatus.IsMandatory = true;
			zDropEditColumnStyleInfoBillStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfoBillStatus.GroupName = billStatusGroupName;
			yield return zDropEditColumnStyleInfoBillStatus;

			var zTextBoxColumnStyleInfobillStatusDescription = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfobillStatusDescription.ColumnName = nameof(AsycudaBill.ABL_BillStatusDescription);
			zTextBoxColumnStyleInfobillStatusDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfobillStatusDescription.GroupName = billStatusGroupName;
			yield return zTextBoxColumnStyleInfobillStatusDescription;

			var zDropEditColumnStyleInfoCondition = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfoCondition.ColumnName = nameof(AsycudaBill.ABL_Condition);
			zDropEditColumnStyleInfoCondition.IsMandatory = false;
			zDropEditColumnStyleInfoCondition.IsVisible = false;
			zDropEditColumnStyleInfoCondition.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			yield return zDropEditColumnStyleInfoCondition;
		}

		protected override IReadOnlyDictionary<bool, string[]> GetContainersGridColumnVisibilityCore()
			=> new Dictionary<bool, string[]>()
			{
				{ true, new[]
					{
						AsycudaContainer.Schema.ACN_Seal2,
						AsycudaContainer.Schema.ACN_Seal3,
						AsycudaContainer.Schema.ACN_SealType1,
						AsycudaContainer.Schema.ACN_SealType2,
						AsycudaContainer.Schema.ACN_SealType3,
						AsycudaContainer.Schema.ACN_SealingPartyType,
						AsycudaContainer.Schema.ACN_SealingPartyType2,
						AsycudaContainer.Schema.ACN_SealingPartyType3,
						AsycudaContainer.Schema.ACN_Seal1UnloadingState,
						AsycudaContainer.Schema.ACN_Seal2UnloadingState,
						AsycudaContainer.Schema.ACN_Seal3UnloadingState,
					}
				},
				{ false, new[]
					{
						AsycudaContainer.Schema.ACN_SealingPartyName,
						AsycudaContainer.Schema.ACN_NumberOfPackages,
						AsycudaContainer.Schema.ACN_CommodityCode,
						AsycudaContainer.Schema.ACN_GoodsWeight,
						AsycudaContainer.Schema.ACN_GoodsWeightUQ,
						AsycudaContainer.Schema.ACN_StowageLocation,
					}
				},
			};

		protected override IEnumerable<string> GetContainersGridColumnsOrderCore()
			=> new[]
			{
				AsycudaContainer.Schema.ACN_ContainerNumber,
				AsycudaContainer.Schema.ACN_RC_ContainerType,
				AsycudaContainer.Schema.ACN_EmptyFullIndicator,
				AsycudaContainer.Schema.ACN_Seal1,
				AsycudaContainer.Schema.ACN_SealType1,
				AsycudaContainer.Schema.ACN_Seal1UnloadingState,
				AsycudaContainer.Schema.ACN_SealingPartyType,
				AsycudaContainer.Schema.ACN_Seal2,
				AsycudaContainer.Schema.ACN_SealType2,
				AsycudaContainer.Schema.ACN_Seal2UnloadingState,
				AsycudaContainer.Schema.ACN_SealingPartyType2,
				AsycudaContainer.Schema.ACN_Seal3,
				AsycudaContainer.Schema.ACN_SealType3,
				AsycudaContainer.Schema.ACN_Seal3UnloadingState,
				AsycudaContainer.Schema.ACN_SealingPartyType3,
			};

		protected override IPanelLayoutProvider GetBillLayoutCore() => new BillDetailsLayout();

		protected override IReadOnlyDictionary<bool, string[]> GetBillsGridColumnAvailabilityCore(ASYCUDA.Business.AsycudaManifestHeader header)
			=> new Dictionary<bool, string[]>()
			{
				{ true, new[]
					{
						AsycudaBill.Schema.ABL_SequenceNumber,
						AsycudaBill.Schema.ABL_RL_NKFinalDestination,
						AsycudaBill.Schema.ABL_ManifestQty,
						AsycudaBill.Schema.ABL_GrossWeight,
						AsycudaBill.Schema.ABL_GrossWeightUQ,
						AsycudaBill.Schema.ABL_Volume,
						AsycudaBill.Schema.ABL_VolumeUQ,
						AsycudaBill.Schema.ABL_BillStatus,
						AsycudaBill.Schema.ABL_BillStatusDescription,
						AsycudaBill.Schema.ABL_CustomsValue,
						AsycudaBill.Schema.ABL_BillNumber,
					}
				},
				{
					false, new []
					{
						AsycudaBill.Schema.ABL_CarrierReference,
						AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency,
						AsycudaBill.Schema.DiscountValue,
						AsycudaBill.Schema.DiscountValueCurrency,
						AsycudaBill.Schema.ABL_FreightValue,
						AsycudaBill.Schema.ABL_RX_NKFreightValueCurrency,
						AsycudaBill.Schema.ABL_GoodsValue,
						AsycudaBill.Schema.ABL_InsuranceValue,
						AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency,
						AsycudaBill.Schema.OtherChargesValue,
						AsycudaBill.Schema.OtherChargesValueCurrency,
						AsycudaBill.Schema.ABL_TransportValue,
						AsycudaBill.Schema.ABL_RX_NKTransportValueCurrency,
						AsycudaBill.Schema.ABL_PrepaidCollect,
						AsycudaBill.Schema.ABL_CargoStatus,
						AsycudaBill.Schema.ABL_BolType,
						AsycudaBill.Schema.ABL_GoodsDescription,
						AsycudaBill.Schema.ABL_MarksAndNumbers,
						AsycudaBill.Schema.ABL_Remarks,
						AsycudaBill.Schema.ABL_UCRNumber,
						AsycudaBill.Schema.CustomsJobNumber,
						AsycudaBill.Schema.ABL_RL_NKPortOfDischarge,
					}
				}
			};

		protected override IEnumerable<string> GetBillsGridColumnsOrderCore()
			=> new[]
				{
					AsycudaBill.Schema.ABL_SequenceNumber,
					AsycudaBill.Schema.ABL_RL_NKOrigin,
					AsycudaBill.Schema.ABL_BillNumber,
					AsycudaBill.Schema.ABL_RL_NKFinalDestination,
					AsycudaBill.Schema.ABL_ManifestQty,
					AsycudaBill.Schema.ABL_ManifestUQ,
					AsycudaBill.Schema.ABL_GrossWeight,
					AsycudaBill.Schema.ABL_GrossWeightUQ,
					AsycudaBill.Schema.ABL_Volume,
					AsycudaBill.Schema.ABL_VolumeUQ,
					AsycudaBill.Schema.ABL_BillStatus,
					AsycudaBill.Schema.ABL_BillStatusDescription,
				};

		protected override IReadOnlyDictionary<string, int> GetBillsGridColumnsWidthCore()
		{
			return new Dictionary<string, int>
			{
				{ AsycudaBill.Schema.ABL_RL_NKFinalDestination, 80 }
			};
		}

		protected override void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid)
		{
			base.CustomizeBillsGridCore(billsGrid);

			var consignorResourceStringData = Res.GetData("ECFF7AA6-D5BA-40C8-A9FE-525F1E13B901", "Consignor");
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_OA_Shipper, consignorResourceStringData);
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ShipperOrgPK, consignorResourceStringData);
			billsGrid.SetColumnCaption(AsycudaBill.Schema.ABL_OA_Shipper, ResString.GetMultilingualString("833C776E-64FD-494F-951B-E89C993578CC", "Address", "Consignor Address"));
			billsGrid.SetColumnCaption(AsycudaBill.Schema.ShipperOrgPK, ResString.GetMultilingualString("4221A041-E843-41E6-8066-5A1EB6768AF1", "Consignor"));
		}

		protected override ASYCUDA.GUI.AsycudaContainerUserControl GetAsycudaContainerUserControlCore() => new AsycudaContainerUserControl();

		protected override IAdditionalTabPage GetHeaderMessagesUserControlCore() => new CustomsMessagingControl();
	}
}
