using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Freight;

namespace Enterprise.Integration
{
	public static partial class Forwarding
	{
		public interface IForwardingShipment
		{
			ZGuid PK { get; }
			ZString JS_UniqueConsignRef { get; set; }

			ZDateTime JS_A_BKD { get; set; }
			ZDateTime JS_A_RCV { get; set; }
			ZDecimal JS_ActualChargeable { get; set; }
			ZDecimal JS_ActualVolume { get; set; }
			ZDecimal JS_ActualWeight { get; set; }
			ZString JS_AdditionalTerms { get; set; }
			ZString JS_AWBServiceLevel { get; set; }
			ZString JS_BookingReference { get; set; }
			ZString JS_CartageWaybill { get; set; }
			ZString JS_CFSReference { get; set; }
			ZDateTime JS_ClientRequestedETA { get; set; }
			ZString JS_ConsolReference { get; set; }
			ZDecimal JS_DocumentedChargeable { get; set; }
			ZDecimal JS_DocumentedLoadingMeters { get; set; }
			ZDecimal JS_DocumentedVolume { get; set; }
			ZDecimal JS_DocumentedWeight { get; set; }
			ZDateTime JS_E_ARV { get; set; }
			ZDateTime JS_E_DEP { get; set; }
			ZString JS_InspectionTypeCode { get; set; }
			ZString JS_F3_NKPackType { get; set; }
			ZString JS_F3_NKTotalCountPackType { get; set; }
			ZString JS_GoodsDescription { get; set; }
			ZDecimal JS_GoodsValue { get; set; }
			ZString JS_HBLAWBChargesDisplay { get; set; }
			ZString JS_HBLContainerPackModeOverride { get; set; }
			ZString JS_HouseBill { get; set; }
			ZDateTime JS_HouseBillIssueDate { get; set; }
			ZString JS_HouseBillOfLadingType { get; set; }
			ZString JS_INCO { get; set; }
			ZDecimal JS_InsuranceValue { get; set; }
			ZString JS_InterimReceipt { get; set; }
			ZString JS_InvisibleTabsXML { get; set; }
			ZBool JS_IsBooking { get; set; }
			ZBool JS_IsCancelled { get; set; }
			ZBool JS_IsCFSRegistered { get; set; }
			ZBool JS_IsDirectBooking { get; set; }
			ZBool JS_IsForwardRegistered { get; set; }
			ZBool JS_IsNeutralMaster { get; set; }
			ZBool JS_IsShipping { get; set; }
			ZBool JS_IsSplitShipment { get; set; }
			ZGuid JS_JS_ColoadMasterShipment { get; set; }
			ZGuid JS_JS_SplitSwitchShipment { get; set; }
			ZGuid JS_JX { get; set; }
			ZDecimal JS_LoadingMeters { get; set; }
			ZDecimal JS_ManifestedChargeable { get; set; }
			ZDecimal JS_ManifestedLoadingMeters { get; set; }
			ZDecimal JS_ManifestedVolume { get; set; }
			ZDecimal JS_ManifestedWeight { get; set; }
			ZByte JS_NoCopyBills { get; set; }
			ZByte JS_NoOriginalBills { get; set; }
			ZGuid JS_OA_ExportReceivingDepot { get; set; }
			ZGuid JS_OA_ImportReleaseDepot { get; set; }
			ZGuid JS_OA_BookedShippingLineAddress { get; set; }
			ZGuid JS_OH_DeliveryAgent { get; set; }
			ZGuid JS_OH_ExportBroker { get; set; }
			ZGuid JS_OH_HandledOnBehalfOfForwarder { get; set; }
			ZGuid JS_OH_ImportBroker { get; set; }
			ZGuid JS_OH_TranshipAgent { get; set; }
			ZInt JS_OuterPacks { get; set; }
			ZBool JS_OverrideWaybillDefaults { get; set; }
			ZString JS_PackingMode { get; set; }
			ZInt JS_PackingOrder { get; set; }
			ZString JS_Phase { get; set; }
			ZString JS_ReleaseType { get; set; }
			ZString JS_RL_NKDestination { get; set; }
			ZString JS_RL_NKOrigin { get; set; }
			ZString JS_RS_NKServiceLevel { get; set; }
			ZString JS_RX_NKFrtRateCurrency { get; set; }
			ZString JS_RX_NKGoodsValueCurr { get; set; }
			ZString JS_RX_NKInsuranceCurrency { get; set; }
			ZString JS_ScreeningStatus { get; set; }
			ZString JS_ShipmentStatus { get; set; }
			ZString JS_ShipmentType { get; set; }
			ZString JS_ShippedOnBoard { get; set; }
			ZDateTime JS_ShippedOnBoardDate { get; set; }
			ZDecimal JS_ShipperCODAmount { get; set; }
			ZString JS_ShipperCODPayMethod { get; set; }
			ZDateTime JS_SystemCreateTimeUtc { get; set; }
			ZString JS_SystemCreateUser { get; set; }
			ZDateTime JS_SystemLastEditTimeUtc { get; set; }
			ZString JS_SystemLastEditUser { get; set; }
			ZInt JS_TotalPackageCount { get; set; }
			ZBool JS_TranshipToOtherCFS { get; set; }
			ZString JS_TransportMode { get; set; }
			ZDecimal JS_UnitFreightRate { get; set; }
			ZString JS_UnitOfVolume { get; set; }
			ZString JS_UnitOfWeight { get; set; }
			ZInt JS_VisibleTabs { get; set; }
			ZString JS_WarehouseLocation { get; set; }
			ZGuid JS_WL { get; set; }
			IEnumerable<IForwardingShipment> CoLoadShipments { get; }
			ZBool IsMasterShipmentRepresentingAllChildShipments { get; }

			IJobDocAddress ConsignorDocumentaryAddress { get; }
			IJobDocAddress ConsigneeDocumentaryAddress { get; }

			ITransport Transports_AddNew();

			BusinessObject GetDeclarationFor(ZGuid companyPK);

			Customs.IBaseJobDeclaration[] Declarations { get; }

			IJobDocsAndCartage DocsAndCartage { get; }
		}
	}
}
