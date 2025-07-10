using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Freight;
namespace Enterprise.Integration
{
	public static partial class Forwarding
	{
		public interface IForwardingConsol
		{
			ZGuid PK { get; }
			string TablePrefix { get; }

			ZString JK_TransportMode { get; set; }
			ZString JK_UniqueConsignRef { get; set; }
			ZString JK_MasterBillNum { get; set; }

			ZString JK_AgentsReference { get; set; }
			ZString JK_AgentType { get; set; }
			ZString JK_AWBServiceLevel { get; set; }
			ZString JK_BookingReference { get; set; }
			ZString JK_CarrierContractNumber { get; set; }
			ZString JK_CoLoadBookingReference { get; set; }
			ZString JK_CoLoadMasterBill { get; set; }
			ZDecimal JK_ConsolChargeable { get; set; }
			ZDecimal JK_ConsolChargeableRate { get; set; }
			ZDateTime JK_ConsolCutOffDate { get; set; }
			ZString JK_ConsolMode { get; set; }
			ZString JK_ConsolStatus { get; set; }
			ZDecimal JK_CorrectedConsolVolume { get; set; }
			ZString JK_CorrectedConsolVolumeUnit { get; set; }
			ZDecimal JK_CorrectedConsolWeight { get; set; }
			ZString JK_CorrectedConsolWeightUnit { get; set; }
			ZDateTime JK_CustomDate1 { get; set; }
			ZDateTime JK_CustomDate2 { get; set; }
			ZBool JK_CustomFlag1 { get; set; }
			ZBool JK_CustomFlag2 { get; set; }
			ZString JK_CustomsReference { get; set; }
			ZDateTime JK_DateFirstForeignPort { get; set; }
			ZDateTime JK_DateLastForeignPort { get; set; }
			ZDateTime JK_DatePortOfFirstArrival { get; set; }
			ZBool JK_IsCancelled { get; set; }
			ZBool JK_IsCFS { get; set; }
			ZBool JK_IsForwarding { get; set; }
			ZBool JK_IsHazardous { get; set; }
			ZBool JK_IsNeutralMaster { get; set; }
			ZGuid JK_JK_MasterConsol { get; set; }
			ZDateTime JK_MasterBillIssueDate { get; set; }
			ZByte JK_NoCopyBills { get; set; }
			ZByte JK_NoOriginalBills { get; set; }
			ZGuid JK_OA_ArrivalCTOAddress { get; set; }
			ZGuid JK_OA_ArrivalUnpackCFSTransportAddress { get; set; }
			ZGuid JK_OA_CoLoadAddress { get; set; }
			ZGuid JK_OA_ContainerYardEmptyPickupAddress { get; set; }
			ZGuid JK_OA_ContainerYardEmptyReturnAddress { get; set; }
			ZGuid JK_OA_CreditorAddress { get; set; }
			ZGuid JK_OA_DepartureCTOAddress { get; set; }
			ZGuid JK_OA_DeparturePackCFSTransportAddress { get; set; }
			ZGuid JK_OA_PackDepotAddress { get; set; }
			ZGuid JK_OA_ReceivingForwarderAddress { get; set; }
			ZGuid JK_OA_SendingForwarderAddress { get; set; }
			ZGuid JK_OA_ShippingLineAddress { get; set; }
			ZGuid JK_OA_UnpackDepotAddress { get; set; }
			ZGuid JK_OH_ArrivalUnpackCFSTransport { get; set; }
			ZGuid JK_OH_Creditor { get; set; }
			ZGuid JK_OH_DeparturePackCFSTransport { get; set; }
			ZBool JK_OverrideConsolChargeable { get; set; }
			ZBool JK_OverrideWaybillDefaults { get; set; }
			ZString JK_Phase { get; set; }
			ZString JK_PrepaidCollect { get; set; }
			ZString JK_PrintOptionForColoadsOnManifest { get; set; }
			ZString JK_PrintOptionForColoadsOnOtherDocs { get; set; }
			ZString JK_PrintOptionForPackagesOnAWB { get; set; }
			ZGuid JK_RCA_AllocationLine { get; set; }
			ZString JK_ReleaseType { get; set; }
			ZString JK_RL_NKDischargePort { get; set; }
			ZString JK_RL_NKFirstForeignPort { get; set; }
			ZString JK_RL_NKLastForeignPort { get; set; }
			ZString JK_RL_NKLoadPort { get; set; }
			ZString JK_RL_NKMasterBillIssuePlace { get; set; }
			ZString JK_RL_NKPortOfFirstArrival { get; set; }
			ZString JK_ScreeningStatus { get; set; }
			ZDateTime JK_ShippedOnBoardDate { get; set; }
			ZDateTime JK_SystemCreateTimeUtc { get; set; }
			ZString JK_SystemCreateUser { get; set; }
			ZDateTime JK_SystemLastEditTimeUtc { get; set; }
			ZString JK_SystemLastEditUser { get; set; }
			ZString JK_TotalShipmentActOtherUnit { get; set; }
			ZDecimal JK_TotalShipmentActVolumeCheck { get; set; }
			ZDecimal JK_TotalShipmentActWeightCheck { get; set; }
			ZDecimal JK_TotalShipmentChargableCheck { get; set; }
			ZString JK_TotalShipmentChargeableUnit { get; set; }
			ZShort JK_TotalShipmentCountCheck { get; set; }
			IEnumerable<IForwardingShipment> Shipments { get; }

			ITransport Transports_AddNew();
			ITransport Transports_Get(int index);

			void AddShipment(IForwardingShipment shipment);

			void SetShippingLine(ZGuid shippingLineAddressPK, ZString reason);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045")]
			bool IsAllowedToChangeShippingLine(ref ZString message);

			IForwardingContainerCollection Containers { get; }

			ZString SecurityStatusCode { get; }
			ZString AWBAgentApprovalNumber { get; }
			ZString AWBAgentApprovalCountryCode { get; }
		}
	}
}
