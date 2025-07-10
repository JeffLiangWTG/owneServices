using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public class ApplicationGUIProvider : ASYCUDA.GUI.ApplicationGUIProvider
	{
		public override Type ApplicationBusinessProviderType => typeof(ApplicationBusinessProvider);

		public override ASYCUDA.GUI.MenuBuilder GetMenuBuilder(ZForm mainForm, ASYCUDA.Business.AsycudaManifestHeader header) => new MenuBuilder(header, mainForm);

		protected override IPanelLayoutProvider GetManifestLayoutCore() => new JPManifestLayouts();

		protected override IPanelLayoutProvider GetBillLayoutCore() => new JPBillLayouts();

		protected override AsycudaContainerUserControl GetAsycudaContainerUserControlCore()
		{
			return new JPAsycudaContainerUserControl();
		}

		protected override IEnumerable<ZGridColumnInfo> GetContainersGridExtraColumnInfosCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var containerTypeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			containerTypeColumnStyleInfo.ColumnName = AsycudaContainer.Schema.NACCSContainerType;
			containerTypeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			containerTypeColumnStyleInfo.GroupName = Res.GetData("591E9F35-0123-409E-8A5D-4A8FC9587354", "NACCS Container");
			yield return containerTypeColumnStyleInfo;

			var containerSizeColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			containerSizeColumnStyleInfo.ColumnName = AsycudaContainer.Schema.NACCSContainerSize;
			containerSizeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			containerSizeColumnStyleInfo.GroupName = Res.GetData("591E9F35-0123-409E-8A5D-4A8FC9587354", "NACCS Container");
			yield return containerSizeColumnStyleInfo;

			var customsTareWeightColumnStyleInfo = new ZCalcEditColumnStyleInfo();
			customsTareWeightColumnStyleInfo.ColumnName = AsycudaContainer.Schema.CustomsTareWeight;
			customsTareWeightColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			customsTareWeightColumnStyleInfo.GroupName = Res.GetData("84A78F67-F766-4D0E-A80E-7F22F81C7B29", "Customs Tare Weight");
			yield return customsTareWeightColumnStyleInfo;

			var customsWeightUQColumnStyleInfo = new ZDropEditColumnStyleInfo();
			customsWeightUQColumnStyleInfo.ColumnName = AsycudaContainer.Schema.CustomsWeightUQ;
			customsWeightUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			customsWeightUQColumnStyleInfo.GroupName = Res.GetData("84A78F67-F766-4D0E-A80E-7F22F81C7B29", "Customs Tare Weight");
			yield return customsWeightUQColumnStyleInfo;

			var moveOutDateEditColumnStyleInfo = new ZDateEditColumnStyleInfo();
			moveOutDateEditColumnStyleInfo.ColumnName = AsycudaContainer.Schema.ACN_MoveOutDate;
			moveOutDateEditColumnStyleInfo.DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Long;
			moveOutDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			yield return moveOutDateEditColumnStyleInfo;

			var vanningLocationCodeColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			vanningLocationCodeColumnStyleInfo.ColumnName = AsycudaContainer.Schema.VanningLocationCode;
			vanningLocationCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			yield return vanningLocationCodeColumnStyleInfo;
		}

		protected override IEnumerable<string> GetContainersGridColumnsOrderCore()
		{
			yield return AsycudaContainer.Schema.ACN_ContainerNumber;
			yield return AsycudaContainer.Schema.ACN_EmptyFullIndicator;
			yield return AsycudaContainer.Schema.ACN_RC_ContainerType;
			yield return AsycudaContainer.Schema.NACCSContainerSize;
			yield return AsycudaContainer.Schema.NACCSContainerType;
			yield return AsycudaContainer.Schema.ACN_Seal1;
			yield return AsycudaContainer.Schema.ACN_SealType1;
			yield return AsycudaContainer.Schema.ACN_SealingPartyType;
			yield return AsycudaContainer.Schema.ACN_SealingPartyName;
			yield return AsycudaContainer.Schema.ACN_NumberOfPackages;
			yield return AsycudaContainer.Schema.ACN_CommodityCode;
			yield return AsycudaContainer.Schema.ACN_GoodsWeight;
			yield return AsycudaContainer.Schema.ACN_GoodsWeightUQ;
			yield return AsycudaContainer.Schema.ACN_StowageLocation;
			yield return AsycudaContainer.Schema.CustomsTareWeight;
			yield return AsycudaContainer.Schema.CustomsWeightUQ;
			yield return AsycudaContainer.Schema.ACN_MoveOutDate;
		}

		protected override IEnumerable<ZGridColumnInfo> GetBillsGridExtraColumnInfosCore()
		{
			var weightInKGColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			weightInKGColumnStyleInfo.ColumnName = nameof(AsycudaBill.CustomsWeight);
			weightInKGColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			weightInKGColumnStyleInfo.GroupName = Res.GetData("61C081E4-D732-4E4D-B314-B83AFAE107A6", "Customs Weight");
			yield return weightInKGColumnStyleInfo;

			var grossWeightUQColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			grossWeightUQColumnStyleInfo.ColumnName = nameof(AsycudaBill.CustomsWeightUQ);
			grossWeightUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			grossWeightUQColumnStyleInfo.GroupName = Res.GetData("61C081E4-D732-4E4D-B314-B83AFAE107A6", "Customs Weight");
			yield return grossWeightUQColumnStyleInfo;

			var specialCargoCodeFindBoxColumnInfo = new ZCodeFindBoxColumnStyleInfo();
			specialCargoCodeFindBoxColumnInfo.ColumnName = AsycudaBill.Schema.ABL_SpecialCargoCode;
			specialCargoCodeFindBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return specialCargoCodeFindBoxColumnInfo;

			var specialCargoCodeDescriptionTextBoxColumnInfo = new ZTextBoxColumnStyleInfo();
			specialCargoCodeDescriptionTextBoxColumnInfo.ColumnName = nameof(AsycudaBill.SpecialCargoCodeDescription);
			specialCargoCodeDescriptionTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			specialCargoCodeDescriptionTextBoxColumnInfo.IsVisible = false;
			yield return specialCargoCodeDescriptionTextBoxColumnInfo;

			var cargoTypeDropEdit = new ZDropEditColumnStyleInfo();
			cargoTypeDropEdit.ColumnName = AsycudaBill.Schema.ABL_CargoType;
			cargoTypeDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return cargoTypeDropEdit;

			var billMessageStatusDropEdit = new ZDropEditColumnStyleInfo();
			billMessageStatusDropEdit.ColumnName = nameof(AsycudaBill.ABL_MessageStatus);
			billMessageStatusDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			billMessageStatusDropEdit.IsVisible = false;
			billMessageStatusDropEdit.GroupName = Res.GetData("1B299701-FA7E-4B92-A522-AA46454BB304", "Message Status");
			yield return billMessageStatusDropEdit;

			var billMessageStatusDescriptionTextBoxColumnInfo = new ZTextBoxColumnStyleInfo();
			billMessageStatusDescriptionTextBoxColumnInfo.ColumnName = nameof(AsycudaBill.MessageStatusDescription);
			billMessageStatusDescriptionTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			billMessageStatusDescriptionTextBoxColumnInfo.IsVisible = false;
			billMessageStatusDescriptionTextBoxColumnInfo.GroupName = Res.GetData("1B299701-FA7E-4B92-A522-AA46454BB304", "Message Status");
			yield return billMessageStatusDescriptionTextBoxColumnInfo;

			var billStatusDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			billStatusDropEditColumnStyleInfo.ColumnName = nameof(AsycudaBill.ABL_BillStatus);
			billStatusDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return billStatusDropEditColumnStyleInfo;

			var billStatusDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			billStatusDescriptionTextBoxColumnStyleInfo.ColumnName = nameof(AsycudaBill.ABL_BillStatusDescription);
			billStatusDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			billStatusDescriptionTextBoxColumnStyleInfo.IsVisible = false;
			yield return billStatusDescriptionTextBoxColumnStyleInfo;

			var dischargePortCodeFindBoxColumnStyleInfo = new ZCodeFindBoxColumnStyleInfo();
			dischargePortCodeFindBoxColumnStyleInfo.ColumnName = nameof(AsycudaBill.ABL_RL_NKPortOfDischarge);
			dischargePortCodeFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return dischargePortCodeFindBoxColumnStyleInfo;

			var tariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
			tariffColumnStyleInfo.ColumnName = nameof(AsycudaBill.ABL_Tariff);
			tariffColumnStyleInfo.DefaultCollectionIndex = 0;
			tariffColumnStyleInfo.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			tariffColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			tariffColumnStyleInfo.GroupName = Res.GetData("83C34999-8E48-47A2-87D4-D82B623CA709", "Tariff");
			yield return tariffColumnStyleInfo;

			var tariffDescriptionColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			tariffDescriptionColumnStyleInfo.ColumnName = nameof(AsycudaBill.TariffDescription);
			tariffDescriptionColumnStyleInfo.DefaultCollectionIndex = 0;
			tariffDescriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			tariffDescriptionColumnStyleInfo.GroupName = Res.GetData("83C34999-8E48-47A2-87D4-D82B623CA709", "Tariff");
			yield return tariffDescriptionColumnStyleInfo;

			var netWeightColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			netWeightColumnStyleInfo.ColumnName = nameof(AsycudaBill.ABL_NetWeight);
			netWeightColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			netWeightColumnStyleInfo.GroupName = Res.GetData("9799BB01-4595-491A-BA8B-54D77FF62E1D", "Net Weight");
			yield return netWeightColumnStyleInfo;

			var netWeightUQColumnStyleInfo = new ZDropEditColumnStyleInfo();
			netWeightUQColumnStyleInfo.ColumnName = nameof(AsycudaBill.ABL_NetWeightUQ);
			netWeightUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			netWeightUQColumnStyleInfo.GroupName = Res.GetData("9799BB01-4595-491A-BA8B-54D77FF62E1D", "Net Weight");
			yield return netWeightUQColumnStyleInfo;

			var customsNetWeightColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			customsNetWeightColumnStyleInfo.ColumnName = nameof(AsycudaBill.CustomsNetWeight);
			customsNetWeightColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			customsNetWeightColumnStyleInfo.GroupName = Res.GetData("5EF60D65-3F82-4105-95BE-EC1256E1ADB3", "Customs Net Weight");
			yield return customsNetWeightColumnStyleInfo;

			var customsNetWeightUQColumnStyleInfo = new ZDropEditColumnStyleInfo();
			customsNetWeightUQColumnStyleInfo.ColumnName = nameof(AsycudaBill.CustomsNetWeightUQ);
			customsNetWeightUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			customsNetWeightUQColumnStyleInfo.GroupName = Res.GetData("5EF60D65-3F82-4105-95BE-EC1256E1ADB3", "Customs Net Weight");
			yield return customsNetWeightUQColumnStyleInfo;

			var customsVolumnColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			customsVolumnColumnStyleInfo.ColumnName = nameof(AsycudaBill.CustomsVolume);
			customsVolumnColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			customsVolumnColumnStyleInfo.GroupName = Res.GetData("E0820CE3-5770-4C51-926F-282748901975", "Customs Volume");
			yield return customsVolumnColumnStyleInfo;

			var customsVolumnUQColumnStyleInfo = new ZDropEditColumnStyleInfo();
			customsVolumnUQColumnStyleInfo.ColumnName = nameof(AsycudaBill.CustomsVolumeUQ);
			customsVolumnUQColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			customsVolumnUQColumnStyleInfo.GroupName = Res.GetData("E0820CE3-5770-4C51-926F-282748901975", "Customs Volume");
			yield return customsVolumnUQColumnStyleInfo;

			var goodsLocationCodeTextBoxColumnInfo = new ZTextBoxColumnStyleInfo();
			goodsLocationCodeTextBoxColumnInfo.ColumnName = nameof(AsycudaBill.ABL_GoodsLocation);
			goodsLocationCodeTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			yield return goodsLocationCodeTextBoxColumnInfo;

			var temporaryLandingNumberColumnInfo = new ZTextBoxColumnStyleInfo();
			temporaryLandingNumberColumnInfo.ColumnName = nameof(AsycudaBill.TemporaryLandingNumber);
			temporaryLandingNumberColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			temporaryLandingNumberColumnInfo.GroupName = Res.GetData("77AD1794-4B46-4F97-99B2-813E5B6FD7E7", "Temporary Landing");
			yield return temporaryLandingNumberColumnInfo;

			var temporaryLandingStatusColumnInfo = new ZTextBoxColumnStyleInfo();
			temporaryLandingStatusColumnInfo.ColumnName = nameof(AsycudaBill.TemporaryLandingStatus);
			temporaryLandingStatusColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			temporaryLandingStatusColumnInfo.GroupName = Res.GetData("77AD1794-4B46-4F97-99B2-813E5B6FD7E7", "Temporary Landing");
			yield return temporaryLandingStatusColumnInfo;

			var goodsOriginFindBoxColumnInfo = new ZCodeFindBoxColumnStyleInfo();
			goodsOriginFindBoxColumnInfo.ColumnName = nameof(AsycudaBill.ABL_CountryOfOrigin);
			goodsOriginFindBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			yield return goodsOriginFindBoxColumnInfo;

			var temporaryLandingStatusDescriptionTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			temporaryLandingStatusDescriptionTextBoxColumnStyleInfo.ColumnName = nameof(AsycudaBill.TemporaryLandingStatusDescription);
			temporaryLandingStatusDescriptionTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			temporaryLandingStatusDescriptionTextBoxColumnStyleInfo.IsVisible = false;
			yield return temporaryLandingStatusDescriptionTextBoxColumnStyleInfo;
		}

		protected override void CustomizeBillsGridCore(ZGridWithDynamicColumnHandler billsGrid)
		{
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_GrossWeightUQ, Res.GetData("D8EA2EBE-B5EA-4AB7-B899-72F6D398548E", "Gross Weight"));
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_GrossWeight, Res.GetData("D8EA2EBE-B5EA-4AB7-B899-72F6D398548E", "Gross Weight"));

			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_ManifestUQ, Res.GetData("98B1E5CB-C686-4CFA-8FD4-9DC00B879846", "Manifest Quantity"));
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_ManifestQty, Res.GetData("98B1E5CB-C686-4CFA-8FD4-9DC00B879846", "Manifest Quantity"));

			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_Volume, Res.GetData("00B0EB83-FE22-4F26-AE7E-0FD3645DFF12", "Volume"));
			billsGrid.SetColumnGroupName(AsycudaBill.Schema.ABL_VolumeUQ, Res.GetData("00B0EB83-FE22-4F26-AE7E-0FD3645DFF12", "Volume"));

			var bolTypeDropEditColumnStyleInfo = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_BolType);
			bolTypeDropEditColumnStyleInfo.IsMandatory = false;

			var nkOriginFindBoxColumnStyleInfo = billsGrid.GetColumnStyle(AsycudaBill.Schema.ABL_RL_NKOrigin);
			nkOriginFindBoxColumnStyleInfo.IsMandatory = false;
		}

		protected override IEnumerable<string> GetBillsGridColumnsOrderCore()
		{
			yield return AsycudaBill.Schema.ABL_SequenceNumber;
			yield return AsycudaBill.Schema.ABL_BolType;
			yield return AsycudaBill.Schema.ABL_BillNumber;
			yield return AsycudaBill.Schema.ABL_RL_NKFinalDestination;
			yield return AsycudaBill.Schema.ABL_RL_NKPortOfDischarge;
			yield return nameof(AsycudaBill.ABL_Tariff);
			yield return nameof(AsycudaBill.TariffDescription);
			yield return nameof(AsycudaBill.ABL_CountryOfOrigin);
			yield return AsycudaBill.Schema.ABL_ManifestQty;
			yield return AsycudaBill.Schema.ABL_ManifestUQ;
			yield return AsycudaBill.Schema.ABL_GrossWeight;
			yield return AsycudaBill.Schema.ABL_GrossWeightUQ;
			yield return nameof(AsycudaBill.CustomsWeight);
			yield return nameof(AsycudaBill.CustomsWeightUQ);
			yield return AsycudaBill.Schema.ABL_NetWeight;
			yield return AsycudaBill.Schema.ABL_NetWeightUQ;
			yield return nameof(AsycudaBill.CustomsNetWeight);
			yield return nameof(AsycudaBill.CustomsNetWeightUQ);
			yield return AsycudaBill.Schema.ABL_Volume;
			yield return AsycudaBill.Schema.ABL_VolumeUQ;
			yield return nameof(AsycudaBill.CustomsVolume);
			yield return nameof(AsycudaBill.CustomsVolumeUQ);
			yield return AsycudaBill.Schema.ABL_MarksAndNumbers;
			yield return AsycudaBill.Schema.ABL_UCRNumber;
			yield return AsycudaBill.Schema.ABL_SpecialCargoCode;
			yield return AsycudaBill.Schema.ABL_BillStatus;
		}

		protected override IReadOnlyDictionary<string, bool> GetBillsGridColumnVisiblilityOnValueChangedCore(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var result = base.GetBillsGridColumnVisiblilityOnValueChangedCore(header);
			if (header is AsycudaManifestHeader jpHeader)
			{
				var isHDF = jpHeader.IsHDF;
				var isHCH = jpHeader.IsHCH;
				var isVAN = jpHeader.IsVAN;
				var isNVC = jpHeader.IsNVC;
				var isNotHDFNorHCH = !isHDF && !isHCH;
				var isNotNVCNorHCH = !isNVC && !isHCH ;

				result = new Dictionary<string, bool>
				{
					{ AsycudaBill.Schema.ABL_BolType, isNotHDFNorHCH },
					{ AsycudaBill.Schema.ABL_RL_NKOrigin, false },
					{ AsycudaBill.Schema.ABL_Volume, isNotHDFNorHCH },
					{ nameof(AsycudaBill.CustomsVolume), isNotHDFNorHCH },
					{ AsycudaBill.Schema.ABL_MarksAndNumbers, isNotHDFNorHCH },
					{ AsycudaBill.Schema.ABL_Remarks, isVAN },
					{ AsycudaBill.Schema.ABL_CargoStatus, isVAN },
					{ AsycudaBill.Schema.ABL_UCRNumber, isNotHDFNorHCH },
					{ AsycudaBill.Schema.CustomsJobNumber, isVAN },
					{ AsycudaBill.Schema.ABL_SpecialCargoCode, !isHDF },
					{ AsycudaBill.Schema.ABL_RL_NKPortOfDischarge, isNotHDFNorHCH },
					{ nameof(AsycudaBill.ABL_Tariff), isNotHDFNorHCH },
					{ nameof(AsycudaBill.ABL_CountryOfOrigin), isNotHDFNorHCH },
					{ AsycudaBill.Schema.ABL_NetWeight, isNotHDFNorHCH },
					{ nameof(AsycudaBill.CustomsNetWeight), isNotHDFNorHCH },
					{ AsycudaBill.Schema.ABL_VolumeUQ, isNotHDFNorHCH },
					{ nameof(AsycudaBill.CustomsVolumeUQ), isNotHDFNorHCH },
					{ nameof(AsycudaBill.TariffDescription), isNotHDFNorHCH },
					{ AsycudaBill.Schema.ABL_NetWeightUQ, isNotHDFNorHCH },
					{ nameof(AsycudaBill.CustomsNetWeightUQ), isNotHDFNorHCH },
					{ nameof(AsycudaBill.ABL_CargoType), isHDF },
					{ nameof(AsycudaBill.ABL_BillStatusDescription), isNotNVCNorHCH },
					{ nameof(AsycudaBill.TemporaryLandingStatusDescription), isNVC },
					{ nameof(AsycudaBill.ABL_GoodsLocation), isHCH },
					{ nameof(AsycudaBill.ShipperOrgPK), false },
					{ nameof(AsycudaBill.ABL_OA_Shipper), false },
					{ nameof(AsycudaBill.ConsigneeOrgPK), false },
					{ nameof(AsycudaBill.ABL_OA_Consignee), false },
					{ nameof(AsycudaBill.TemporaryLandingNumber), isNVC },
					{ nameof(AsycudaBill.TemporaryLandingStatus), isNVC },
					{ AsycudaBill.Schema.ABL_CarrierReference, false },
					{ AsycudaBill.Schema.ABL_PrepaidCollect, false },
					{ AsycudaBill.Schema.ABL_InsuranceValue, false },
					{ AsycudaBill.Schema.ABL_RX_NKInsuranceValueCurrency, false },
					{ AsycudaBill.Schema.DiscountValue, false },
					{ AsycudaBill.Schema.DiscountValueCurrency, false },
					{ AsycudaBill.Schema.OtherChargesValue, false },
					{ AsycudaBill.Schema.OtherChargesValueCurrency, false },
					{ AsycudaBill.Schema.ABL_CustomsValue, false },
					{ AsycudaBill.Schema.ABL_RX_NKCustomsValueCurrency, false },
					{ AsycudaBill.Schema.ABL_GoodsDescription, !isNVC },
				};
			}
			return result;
		}

		protected override IPanelLayoutProvider GetBillPartiesLayoutCore() => new JPBillPartiesLayout();

		protected override IEnumerable<IAdditionalTabPage> GetBillAdditionalTabPageUserControlCore()
		{
			yield return new JPTemporaryLandingUserControl();
		}

		protected override ZBool GetBillPartiesTabPageVisibilityCore(ASYCUDA.Business.AsycudaManifestHeader header) => !(header.IsExport && header.IsAir);

		protected override IEnumerable<ZGridColumnInfo> GetMessagesGridExtraColumnInfosCore()
		{
			var procedureCodeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			procedureCodeTextBoxColumnStyleInfo.ColumnName = EDIMessage.Schema.EM_Calc_ProcedureCode;
			procedureCodeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			yield return procedureCodeTextBoxColumnStyleInfo;

			var procedureNameTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			procedureNameTextBoxColumnStyleInfo.ColumnName = EDIMessage.Schema.EM_Calc_ProcedureName;
			procedureNameTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			yield return procedureNameTextBoxColumnStyleInfo;

			var outputInformationCodeTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outputInformationCodeTextBoxColumnStyleInfo.ColumnName = EDIMessage.Schema.EM_Calc_OutputInformationCode;
			outputInformationCodeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			yield return outputInformationCodeTextBoxColumnStyleInfo;

			var outputInformationTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			outputInformationTextBoxColumnStyleInfo.ColumnName = EDIMessage.Schema.EM_Calc_OutputInformation;
			outputInformationTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			yield return outputInformationTextBoxColumnStyleInfo;
		}

		protected override IEnumerable<string> GetMessagesGridOrderCore()
		{
			yield return EDIMessage.Schema.EM_Calc_ProcedureCode;
			yield return EDIMessage.Schema.EM_Calc_ProcedureName;
			yield return EDIMessage.Schema.EM_Calc_OutputInformationCode;
			yield return EDIMessage.Schema.EM_Calc_OutputInformation;
			yield return EDIMessage.Schema.EM_MessageNum;
			yield return EDIMessage.Schema.EM_Status;
			yield return EDIMessage.Schema.EM_MessageDateTime;
			yield return EDIMessage.Schema.EM_SystemCreateTimeUtc;
			yield return EDIMessage.Schema.EM_SystemCreateUser;
			yield return EDIMessage.Schema.EM_ReceiveTransmit;
			yield return EDIMessage.Schema.EM_MessageType;
		}
	}
}
