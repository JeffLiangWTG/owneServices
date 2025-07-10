using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class ManifestBase
		{
			public partial interface IAsycudaBill
			{
				bool IsInDatabase { get; }
				ZGuid PK { get; }
				ZGuid ABL_OA_NotifyParty { get; set; }
				ZGuid ABL_OA_Shipper { get; set; }
				ZString ABL_PrepaidCollect { get; set; }
				ZString ABL_Remarks { get; set; }
				ZString ABL_RL_NKFinalDestination { get; set; }
				ZString ABL_RL_NKOrigin { get; set; }
				ZString ABL_RN_NKConsigneeCountry { get; set; }
				ZString ABL_RN_NKNotifyPartyCountry { get; set; }
				ZString ABL_RN_NKShipperCountry { get; set; }
				ZString ABL_RX_NKCustomsValueCurrency { get; set; }
				ZString ABL_RX_NKFreightValueCurrency { get; set; }
				ZString ABL_RX_NKInsuranceValueCurrency { get; set; }
				ZString ABL_RX_NKTransportValueCurrency { get; set; }
				ZString ABL_ShipperCity { get; set; }
				ZString ABL_ShipperName { get; set; }
				ZString ABL_ShipperPostcode { get; set; }
				ZString ABL_ShipperState { get; set; }
				ZString ABL_ShipperStreet1 { get; set; }
				ZString ABL_ShipperStreet2 { get; set; }
				ZString ABL_ShipperPhone { get; set; }
				ZDateTime ABL_SystemCreateTimeUtc { get; set; }
				ZString ABL_SystemCreateUser { get; set; }
				ZDateTime ABL_SystemLastEditTimeUtc { get; set; }
				ZString ABL_SystemLastEditUser { get; set; }
				ZDecimal ABL_TransportValue { get; set; }
				ZDecimal ABL_Volume { get; set; }
				ZString ABL_VolumeUQ { get; set; }
				ZGuid ABL_OA_Consignee { get; set; }
				ZString ABL_NotifyPartyStreet2 { get; set; }
				ZString ABL_NotifyPartyStreet1 { get; set; }
				ZDecimal ABL_FreightValue { get; set; }
				ZString ABL_BillNumber { get; set; }
				ZString ABL_BolType { get; set; }
				ZString ABL_CargoStatus { get; set; }
				ZString ABL_CarrierReference { get; set; }
				ZInt ABL_ClusterKey { get; set; }
				ZString ABL_ConsigneeCity { get; set; }
				ZString ABL_ConsigneeName { get; set; }
				ZString ABL_ConsigneePhone { get; set; }
				ZString ABL_ConsigneePostcode { get; set; }
				ZString ABL_ConsigneeState { get; set; }
				ZString ABL_ConsigneeStreet1 { get; set; }
				ZString ABL_ConsigneeStreet2 { get; set; }
				ZDecimal ABL_CustomsValue { get; set; }
				ZString ABL_NotifyPartyState { get; set; }
				ZString ABL_GoodsDescription { get; set; }
				ZDecimal ABL_GrossWeight { get; set; }
				ZString ABL_GrossWeightUQ { get; set; }
				ZDecimal ABL_InsuranceValue { get; set; }
				ZGuid ABL_JS_Shipment { get; set; }
				ZInt ABL_ManifestQty { get; set; }
				ZString ABL_ManifestUQ { get; set; }
				ZString ABL_MarksAndNumbers { get; set; }
				ZString ABL_NotifyPartyCity { get; set; }
				ZString ABL_NotifyPartyName { get; set; }
				ZString ABL_NotifyPartyPhone { get; set; }
				ZString ABL_NotifyPartyPostcode { get; set; }
				ZDate ABL_BillIssueDate { get; set; }
				ZGuid ABL_AMA { get; set; }
			}
		}
	}
}
