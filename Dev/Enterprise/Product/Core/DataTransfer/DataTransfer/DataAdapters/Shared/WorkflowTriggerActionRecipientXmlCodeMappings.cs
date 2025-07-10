using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class WorkflowTriggerActionRecipientXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		WorkflowTriggerActionRecipientXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Bolero, nameof(Xsd.WorkflowTriggerActionRecipient.BOR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Broker, nameof(Xsd.WorkflowTriggerActionRecipient.BRO));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ImportBroker, nameof(Xsd.WorkflowTriggerActionRecipient.BRI));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ExportBroker, nameof(Xsd.WorkflowTriggerActionRecipient.BRE));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.BillToParty, nameof(Xsd.WorkflowTriggerActionRecipient.BTP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Client, nameof(Xsd.WorkflowTriggerActionRecipient.CLI));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent, nameof(Xsd.WorkflowTriggerActionRecipient.COA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Consignee, nameof(Xsd.WorkflowTriggerActionRecipient.CNE));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Consignor, nameof(Xsd.WorkflowTriggerActionRecipient.CNR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DeliveryCartage, nameof(Xsd.WorkflowTriggerActionRecipient.DCA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Email, nameof(Xsd.WorkflowTriggerActionRecipient.EML));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ControllingCustomer, nameof(Xsd.WorkflowTriggerActionRecipient.CTP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.OrgProxy, nameof(Xsd.WorkflowTriggerActionRecipient.ORP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PickupCartage, nameof(Xsd.WorkflowTriggerActionRecipient.PCA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ReceivingAgent, nameof(Xsd.WorkflowTriggerActionRecipient.RAG));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.SendingAgent, nameof(Xsd.WorkflowTriggerActionRecipient.SAG));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Print, nameof(Xsd.WorkflowTriggerActionRecipient.PRN));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.TransportCo, nameof(Xsd.WorkflowTriggerActionRecipient.TPC));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Carrier, nameof(Xsd.WorkflowTriggerActionRecipient.CAR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.NotifyParty, nameof(Xsd.WorkflowTriggerActionRecipient.NFP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DeliveryToParty, nameof(Xsd.WorkflowTriggerActionRecipient.DTP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PickupParty, nameof(Xsd.WorkflowTriggerActionRecipient.PUP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.EDICommunication, nameof(Xsd.WorkflowTriggerActionRecipient.EDI));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.InvoiceDebtor, nameof(Xsd.WorkflowTriggerActionRecipient.IDB));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DepartureCFS, nameof(Xsd.WorkflowTriggerActionRecipient.DCF));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ArrivalCFS, nameof(Xsd.WorkflowTriggerActionRecipient.ACF));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Forwarder, nameof(Xsd.WorkflowTriggerActionRecipient.FOR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ArrivalCarrier, nameof(Xsd.WorkflowTriggerActionRecipient.ACR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DepartureCarrier, nameof(Xsd.WorkflowTriggerActionRecipient.DCR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Principal, nameof(Xsd.WorkflowTriggerActionRecipient.PRC));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DepartureCTO, nameof(Xsd.WorkflowTriggerActionRecipient.DCT));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ArrivalCTO, nameof(Xsd.WorkflowTriggerActionRecipient.ACT));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DepartureContainerYard, nameof(Xsd.WorkflowTriggerActionRecipient.DCY));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ArrivalContainerYard, nameof(Xsd.WorkflowTriggerActionRecipient.ACY));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.WarehouseInwards, nameof(Xsd.WorkflowTriggerActionRecipient.WIN));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.WarehouseOutwards, nameof(Xsd.WorkflowTriggerActionRecipient.WAR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.WarehouseDynamicWorkOrder, nameof(Xsd.WorkflowTriggerActionRecipient.WDO));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.WarehouseWorkOrder, nameof(Xsd.WorkflowTriggerActionRecipient.WWO));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.BondedWarehouseInwards, nameof(Xsd.WorkflowTriggerActionRecipient.BWI));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.BondedWarehouseOutwards, nameof(Xsd.WorkflowTriggerActionRecipient.BWR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse, nameof(Xsd.WorkflowTriggerActionRecipient.DTW));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse, nameof(Xsd.WorkflowTriggerActionRecipient.ATW));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.HVLVAirClearanceAgent, nameof(Xsd.WorkflowTriggerActionRecipient.HCA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ShippingManager, nameof(Xsd.WorkflowTriggerActionRecipient.SPM));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.BookingParty, nameof(Xsd.WorkflowTriggerActionRecipient.BKP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DeConsolidator, nameof(Xsd.WorkflowTriggerActionRecipient.AAD));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty, nameof(Xsd.WorkflowTriggerActionRecipient.ARP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PickupAgent, nameof(Xsd.WorkflowTriggerActionRecipient.PAG));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.DeliveryAgent, nameof(Xsd.WorkflowTriggerActionRecipient.DAG));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CartageAgent, nameof(Xsd.WorkflowTriggerActionRecipient.CTG));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.HVLVSeaClearanceAgent, nameof(Xsd.WorkflowTriggerActionRecipient.HSA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.JapanCustomsAFR, nameof(Xsd.WorkflowTriggerActionRecipient.AFR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Warehouse, nameof(Xsd.WorkflowTriggerActionRecipient.WHS));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.SeaCargoResponsibleParty, nameof(Xsd.WorkflowTriggerActionRecipient.SRP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ASYCUDA, nameof(Xsd.WorkflowTriggerActionRecipient.ASY));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.IndianCustomsEDISystem, nameof(Xsd.WorkflowTriggerActionRecipient.ICE));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PortForExportManifest, nameof(Xsd.WorkflowTriggerActionRecipient.PEM));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PortForExportRelease, nameof(Xsd.WorkflowTriggerActionRecipient.PER));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PortForImportManifest, nameof(Xsd.WorkflowTriggerActionRecipient.PIM));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PortForImportRelease, nameof(Xsd.WorkflowTriggerActionRecipient.PIR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.YardForExportRelease, nameof(Xsd.WorkflowTriggerActionRecipient.YER));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.YardForImportPreArrival, nameof(Xsd.WorkflowTriggerActionRecipient.YIA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.WiseNettingSystem, nameof(Xsd.WorkflowTriggerActionRecipient.WNS));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ContainerYard, nameof(Xsd.WorkflowTriggerActionRecipient.CYD));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CTO, nameof(Xsd.WorkflowTriggerActionRecipient.CTO));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ControllingAgent, nameof(Xsd.WorkflowTriggerActionRecipient.CTA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CACustomsIIDD4StatusNotice, nameof(Xsd.WorkflowTriggerActionRecipient.CD4));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.BondedWhsChangeOfOwnership, nameof(Xsd.WorkflowTriggerActionRecipient.BCO));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.BondedWhsChangeOfRegime, nameof(Xsd.WorkflowTriggerActionRecipient.BCR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery, nameof(Xsd.WorkflowTriggerActionRecipient.ADV));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.SGAccess, nameof(Xsd.WorkflowTriggerActionRecipient.SGA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.USAirAMS, nameof(Xsd.WorkflowTriggerActionRecipient.UAM));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.LastCompletedTaskResource, nameof(Xsd.WorkflowTriggerActionRecipient.LCT));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup, nameof(Xsd.WorkflowTriggerActionRecipient.JWG));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.NotificationGroup, nameof(Xsd.WorkflowTriggerActionRecipient.NGP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CreditControlledDocumentApproval, nameof(Xsd.WorkflowTriggerActionRecipient.CCA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CarrierBookingAgent, nameof(Xsd.WorkflowTriggerActionRecipient.CBA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.AssignedStaff, nameof(Xsd.WorkflowTriggerActionRecipient.STF));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.Staff, nameof(Xsd.WorkflowTriggerActionRecipient.STA));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.RequiredCapabilityMembers, nameof(Xsd.WorkflowTriggerActionRecipient.CAP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.AssignedGroupMembers, nameof(Xsd.WorkflowTriggerActionRecipient.GRP));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PersonalEmail, nameof(Xsd.WorkflowTriggerActionRecipient.PSE));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail, nameof(Xsd.WorkflowTriggerActionRecipient.PWK));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail, nameof(Xsd.WorkflowTriggerActionRecipient.PPW));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.NVOCC, nameof(Xsd.WorkflowTriggerActionRecipient.NVO));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CurrentUser, nameof(Xsd.WorkflowTriggerActionRecipient.CUR));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.GroupOwners, nameof(Xsd.WorkflowTriggerActionRecipient.OWN));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.ExternalBroker, nameof(Xsd.WorkflowTriggerActionRecipient.BRX));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.HVLVForwarder, nameof(Xsd.WorkflowTriggerActionRecipient.HVL));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.CarrierMessagingDebtor, nameof(Xsd.WorkflowTriggerActionRecipient.CMD));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.FirstApprovalTask, nameof(Xsd.WorkflowTriggerActionRecipient.FAT));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.OnBoardingEmail, nameof(Xsd.WorkflowTriggerActionRecipient.OBE));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.TransportJobRegistry, nameof(Xsd.WorkflowTriggerActionRecipient.DEF));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.GlobalTradeManagement, nameof(Xsd.WorkflowTriggerActionRecipient.GTM));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.GateManagement, nameof(Xsd.WorkflowTriggerActionRecipient.GDM));
			yield return new Mapping(MessageRecipientPartyTypeList.Codes.PortForTransitManifest, nameof(Xsd.WorkflowTriggerActionRecipient.PTM));
		}

		public static readonly WorkflowTriggerActionRecipientXmlCodeMappings Instance = new WorkflowTriggerActionRecipientXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Workflow Trigger Action Recipient"; }
		}

		public new Xsd.WorkflowTriggerActionRecipient GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.WorkflowTriggerActionRecipient.ORP, errorContext, notify);
		}
	}
}
