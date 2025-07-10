using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICusHAWBBase : ISACLiabilityQuestionProvider
	{
		ZString CS_MessageReference
		{
			get;
		}

		ZPropertyInfo CS_MessageReferenceInfo
		{
			get;
		}

		ZString CS_HAWB
		{
			get;
		}

		ZPropertyInfo CS_HAWBInfo
		{
			get;
		}

		ZString CS_RL_NKLoadPort
		{
			get;
		}

		ZPropertyInfo CS_RL_NKLoadPortInfo
		{
			get;
		}

		ZString CS_RL_NKOrigin
		{
			get;
		}

		ZPropertyInfo CS_RL_NKOriginInfo
		{
			get;
		}

		ZString CS_RL_NKDestination
		{
			get;
		}

		ZPropertyInfo CS_RL_NKDestinationInfo
		{
			get;
		}

		ZDecimal CS_Weight
		{
			get;
		}

		ZPropertyInfo CS_WeightInfo
		{
			get;
		}

		ZString CS_WeightUQ
		{
			get;
		}

		ZPropertyInfo CS_WeightUQInfo
		{
			get;
		}

		ZDecimal CS_GoodsValue
		{
			get;
		}

		ZPropertyInfo CS_GoodsValueInfo
		{
			get;
		}

		ZString CS_RX_NKGoodsCurrency
		{
			get;
		}

		ZPropertyInfo CS_RX_NKGoodsCurrencyInfo
		{
			get;
		}

		ZString CS_GoodsDescription
		{
			get;
		}

		ZPropertyInfo CS_GoodsDescriptionInfo
		{
			get;
		}

		ZString CS_CustomsStatus
		{
			get;
		}

		ZPropertyInfo CS_CustomsStatusInfo
		{
			get;
		}

		ZBool CS_IsMasterHouse
		{
			get;
		}

		ZPropertyInfo CS_IsMasterHouseInfo
		{
			get;
		}

		ZShort CS_PiecesManifested
		{
			get;
		}

		ZPropertyInfo CS_PiecesManifestedInfo
		{
			get;
		}

		ZString CS_FreightPrepaidCollect
		{
			get;
		}

		ZPropertyInfo CS_FreightPrepaidCollectInfo
		{
			get;
		}

		ZString CS_WarehouseLocation
		{
			get;
		}

		ZPropertyInfo CS_WarehouseLocationInfo
		{
			get;
		}

		ZString WarehouseLocationCaption
		{
			get;
		}

		ZPropertyInfo WarehouseLocationCaptionInfo
		{
			get;
		}

		ZString CS_FolioReference
		{
			get;
		}

		ZPropertyInfo CS_FolioReferenceInfo
		{
			get;
		}

		ZString CS_ShipmentType
		{
			get;
		}

		ZPropertyInfo CS_ShipmentTypeInfo
		{
			get;
		}

		CusHAWBLookups Lookups
		{
			get;
		}

		ZString CS_ConsigneeCity
		{
			get;
		}

		ZPropertyInfo CS_ConsigneeCityInfo
		{
			get;
		}

		ZString CS_ConsigneeName
		{
			get;
		}

		ZPropertyInfo CS_ConsigneeNameInfo
		{
			get;
		}

		ZString CS_ConsigneePhone
		{
			get;
		}

		ZPropertyInfo CS_ConsigneePhoneInfo
		{
			get;
		}

		ZString CS_ConsigneePostcode
		{
			get;
		}

		ZPropertyInfo CS_ConsigneePostcodeInfo
		{
			get;
		}

		ZString CS_ConsigneeState
		{
			get;
		}

		ZPropertyInfo CS_ConsigneeStateInfo
		{
			get;
		}

		ZString CS_ConsigneeStreet
		{
			get;
		}

		ZPropertyInfo CS_ConsigneeStreetInfo
		{
			get;
		}

		ZString CS_ConsigneeStreet2
		{
			get;
		}

		ZPropertyInfo CS_ConsigneeStreet2Info
		{
			get;
		}

		ZString CS_ConsignorName
		{
			get;
		}

		ZPropertyInfo CS_ConsignorNameInfo
		{
			get;
		}

		ZString CS_ConsignorPostcode
		{
			get;
		}

		ZPropertyInfo CS_ConsignorPostcodeInfo
		{
			get;
		}

		ZString CS_ConsignorState
		{
			get;
		}

		ZPropertyInfo CS_ConsignorStateInfo
		{
			get;
		}

		ZString CS_ConsignorCity
		{
			get;
		}

		ZPropertyInfo CS_ConsignorCityInfo
		{
			get;
		}

		ZString CS_ConsignorStreet
		{
			get;
		}

		ZPropertyInfo CS_ConsignorStreetInfo
		{
			get;
		}

		ZString CS_ConsignorStreet2
		{
			get;
		}

		ZPropertyInfo CS_ConsignorStreet2Info
		{
			get;
		}

		ZGuid CS_OH_Consignee
		{
			get;
		}

		ZPropertyInfo CS_OH_ConsigneeInfo
		{
			get;
		}

		ZGuid CS_OH_Consignor
		{
			get;
		}

		ZPropertyInfo CS_OH_ConsignorInfo
		{
			get;
		}

		ZString CS_RN_NKConsigneeCountry
		{
			get;
		}

		ZPropertyInfo CS_RN_NKConsigneeCountryInfo
		{
			get;
		}

		ZString CS_RN_NKConsignorCountry
		{
			get;
		}

		ZPropertyInfo CS_RN_NKConsignorCountryInfo
		{
			get;
		}

		ZDecimal CS_ChargableWeight
		{
			get;
		}

		ZPropertyInfo CS_ChargableWeightInfo
		{
			get;
		}

		ZString ChargableWeightCaption
		{
			get;
		}

		ZPropertyInfo ChargableWeightCaptionInfo
		{
			get;
		}

		ZString CS_RS_NK_ServiceLevel
		{
			get;
		}

		ZPropertyInfo CS_RS_NK_ServiceLevelInfo
		{
			get;
		}

		ZBool CS_IsSelfAssessedClearance
		{
			get;
		}

		ZPropertyInfo CS_IsSelfAssessedClearanceInfo
		{
			get;
		}

		ZBool CS_IsPersonalEffects
		{
			get;
		}

		ZPropertyInfo CS_IsPersonalEffectsInfo
		{
			get;
		}

		CusPartShipCollection PartShips
		{
			get;
		}

		EDIMessageCollection Messages
		{
			get;
		}

		bool CanDeleteFromICusHAWBCollection
		{
			get;
		}

		ZString CS_PaymentTypeCaption
		{
			get;
		}

		ZPropertyInfo CS_PaymentTypeCaptionInfo
		{
			get;
		}

		ZString CS_FreightPrepaidCollectForBinding
		{
			get;
		}

		ZPropertyInfo CS_FreightPrepaidCollectForBindingInfo
		{
			get;
		}

		ZString CS_ShipmentTypeForBinding
		{
			get;
		}

		ZPropertyInfo CS_ShipmentTypeForBindingInfo
		{
			get;
		}

		ZString CS_TranshipmentEntryNum
		{
			get;
		}

		ZPropertyInfo CS_TranshipmentEntryNumInfo
		{
			get;
		}

		MessageCusStatus CMRMessageStatus { get; }

		CargoCusStatus CMRCargoStatus { get; }
	}
}
