using System.Collections.Generic;
using System.Linq;

namespace Enterprise.UniversalDataBuss.Integration
{
	/// <summary>
	/// List of all possible Recipient Roles available to send Universal Data to.
	/// MUST keep this in sync with Enterprise.MasterFiles.Business.MessageRecipientPartyTypeList.AllPossiblePartyTypeList
	/// ALSO: Please keep it in alphabetical order. Thanks!
	/// </summary>
	public enum RecipientRoleType
	{
		AAD, // DeConsolidator
		ACF, // ArrivalCFS
		ACR, // ArrivalCarrier
		ACT, // ArrivalCTO
		ACY, // ArrivalContainerYard
		AFR, // JapanCustomsAFR
		ARP, // AirCargo Responsible Party
		ASY, // ASYCUDA manifest
		ATW, // Arrival Transit Warehouse
		BCO, // Bonded Warehouse Change of Ownership
		BCR, // Bonded Warehouse Change of Regime
		BKP, // Booking Party
		BOR, // Bolero
		BRO, // Broker
		BRI, // Import Broker
		BRE, // Export Broker
		BRX, // External Broker
		BTP, // BillToParty
		CAP, // Required Capability Members
		CAR, // Carrier
		CBA, // Carrier Booking Agent
		CCA, // Credit Controlled Document Approval
		CD4, // CA Customs IID/D4 Status Notice
		CLI, // Client
		CMD, // Carrier Messaging Debtor
		CNE, // Consignee
		CNR, // Consignor
		COA, // Customs Outturn Agent
		CTA, // ControllingAgent
		CTG, // Cartage Agent
		CTO, // CTO
		CTP, // ControllingCustomer
		CYD, // Container Yard
		DAG, // Delivery Agent
		DCA, // DeliveryCartage
		DCF, // DepartureCFS
		DCR, // DepartureCarrier
		DCT, // DepartureCTO
		DCY, // DepartureContainerYard
		DEF, // TransportJobRegisty
		DTP, // DeliveryToParty
		DTW, // Departure Transit Warehouse
		FOR, // Forwarder
		GDM, // Gate Management
		GRP, // Assigned Group Members
		GTM, // Global Trade Management
		HCA, // HVLV Air Clearance Agent
		HSA, // HVLV Sea Cargo Clearance Agent
		HVL, // HVLV Forwarder
		ICE, // Indian Customs EDI System
		IDB, // InvoiceDebtor
		JWG, // Job-level Workflow Group
		LCT, // Last Completed Task Resource
		NFP, // NotifyParty
		NGP, // Notification Group
		NVO, // NVOCC
		ORP, // OrgProxy
		PAG, // Pickup Agent
		PCA, // PickupCartage
		PEM, // PortForExportManifest
		PER, // PortForExportRelease
		PIM, // PortForImportManifest
		PIR, // PortForImportRelease
		PRC, // Principal
		PTM, // PortForTransitManifest
		PUP, // PickupParty
		RAG, // ReceivingAgent
		SAG, // SendingAgent
		SGA, // SG Access Manifest
		SPM, // ShippingManager
		SRP, // SeaCargo Responsible Party
		STF, // Assigned Staff
		TPC, // TransportCo
		UAM, // US Air AMS
		WAR, // Warehouse Out
		WIN, // Warehouse In
		WDO, // Warehouse Dynamic Work Order
		WWO, // Warehouse Work Order
		BWI, // Bonded Warehouse In
		BWR, // Bonded Warehouse Out
		WHS, // Warehouse
		WNS, // WiseNettingSystem
		YER, // YardForExportRelease
		YIA, // YardForImportPreArrival
	}

	public static class RecipientRoleTypeExtension
	{
		public static RecipientRoleDetail[] ToRecipientRoleDetails(this IEnumerable<RecipientRoleType> recipientRoleTypes)
		{
			return recipientRoleTypes == null ? null : recipientRoleTypes.Select(x => new RecipientRoleDetail() { Type = x }).ToArray();
		}

		public static RecipientRoleDetail[] ToRecipientRoleDetails(this RecipientRoleType recipientRoleType)
		{
			return new[] { new RecipientRoleDetail() { Type = recipientRoleType } };
		}
	}
}
