using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class DocAddressTypeXmlMappings : EnterpriseCodeExternalCodeMappings
	{
		DocAddressTypeXmlMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping("", nameof(Xsd.DocAddressAddressType.NONE));

			yield return new Mapping(DocAddressTypes.Codes.BuyerDocumentaryAddress, nameof(Xsd.DocAddressAddressType.BUY));

			yield return new Mapping(DocAddressTypes.Codes.ContainerLegPickupAddress, nameof(Xsd.DocAddressAddressType.CLP));
			yield return new Mapping(DocAddressTypes.Codes.ContainerLegDeliveryAddress, nameof(Xsd.DocAddressAddressType.CLD));
			yield return new Mapping(DocAddressTypes.Codes.ContainerLegWaitPointAddress, nameof(Xsd.DocAddressAddressType.CLW));

			yield return new Mapping(DocAddressTypes.Codes.ConsignorDocumentaryAddress, nameof(Xsd.DocAddressAddressType.CRD));
			yield return new Mapping(DocAddressTypes.Codes.ConsignorPickupDeliveryAddress, nameof(Xsd.DocAddressAddressType.CRG));

			yield return new Mapping(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, nameof(Xsd.DocAddressAddressType.CED));
			yield return new Mapping(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress, nameof(Xsd.DocAddressAddressType.CEG));
			yield return new Mapping(DocAddressTypes.Codes.ClientRequestedBillingParty, nameof(Xsd.DocAddressAddressType.CRB));

			yield return new Mapping(DocAddressTypes.Codes.SupplierDocumentaryAddress, nameof(Xsd.DocAddressAddressType.SUD));
			yield return new Mapping(DocAddressTypes.Codes.SupplierPickupDeliveryAddress, nameof(Xsd.DocAddressAddressType.SUG));
			yield return new Mapping(DocAddressTypes.Codes.SupplierTranslatedDocumentaryAddress, nameof(Xsd.DocAddressAddressType.STA));

			yield return new Mapping(DocAddressTypes.Codes.ImporterDocumentaryAddress, nameof(Xsd.DocAddressAddressType.IMD));
			yield return new Mapping(DocAddressTypes.Codes.ImporterPickupDeliveryAddress, nameof(Xsd.DocAddressAddressType.IMG));
			yield return new Mapping(DocAddressTypes.Codes.ImporterTranslatedDocumentaryAddress, nameof(Xsd.DocAddressAddressType.ITA));

			yield return new Mapping(DocAddressTypes.Codes.NotifyParty, nameof(Xsd.DocAddressAddressType.NPP));
			yield return new Mapping(DocAddressTypes.Codes.NotifyParty2, nameof(Xsd.DocAddressAddressType.N2D));
			yield return new Mapping(DocAddressTypes.Codes.NotifyParty3, nameof(Xsd.DocAddressAddressType.N3D));

			yield return new Mapping(DocAddressTypes.Codes.LocalCartagePickupFromAddress, nameof(Xsd.DocAddressAddressType.LCP));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageDeliverToAddress, nameof(Xsd.DocAddressAddressType.LCD));

			yield return new Mapping(DocAddressTypes.Codes.LocalCartageCTO, nameof(Xsd.DocAddressAddressType.LCT));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageCFS, nameof(Xsd.DocAddressAddressType.LCF));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageYard, nameof(Xsd.DocAddressAddressType.LCY));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageImporter, nameof(Xsd.DocAddressAddressType.LCI));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageExporter, nameof(Xsd.DocAddressAddressType.LCE));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageService, nameof(Xsd.DocAddressAddressType.LCS));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageMSC, nameof(Xsd.DocAddressAddressType.LCM));
			yield return new Mapping(DocAddressTypes.Codes.NonPersistent, nameof(Xsd.DocAddressAddressType.DUM));

			yield return new Mapping(DocAddressTypes.Codes.LocalCartageAddress1, nameof(Xsd.DocAddressAddressType.LC1));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageAddress2, nameof(Xsd.DocAddressAddressType.LC2));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageAddress3, nameof(Xsd.DocAddressAddressType.LC3));
			yield return new Mapping(DocAddressTypes.Codes.LocalCartageAddress4, nameof(Xsd.DocAddressAddressType.LC4));

			yield return new Mapping(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, nameof(Xsd.DocAddressAddressType.BKD));
			yield return new Mapping(DocAddressTypes.Codes.LoadListParty, nameof(Xsd.DocAddressAddressType.LLP));

			yield return new Mapping(DocAddressTypes.Codes.ConsigneeAddress, nameof(Xsd.DocAddressAddressType.CEA));
			yield return new Mapping(DocAddressTypes.Codes.GoodsBillToAddress, nameof(Xsd.DocAddressAddressType.GBA));
			yield return new Mapping(DocAddressTypes.Codes.PickUpAddress, nameof(Xsd.DocAddressAddressType.PUA));

			yield return new Mapping(DocAddressTypes.Codes.ForeignShipperDocumentaryAddress, nameof(Xsd.DocAddressAddressType.FSD));

			yield return new Mapping(DocAddressTypes.Codes.OneOffQuotePickupAddress, nameof(Xsd.DocAddressAddressType.OQP));
			yield return new Mapping(DocAddressTypes.Codes.OneOffQuoteDeliveryAddress, nameof(Xsd.DocAddressAddressType.OQD));

			yield return new Mapping(DocAddressTypes.Codes.ArrivalCFSAddress, nameof(Xsd.DocAddressAddressType.ACF));
			yield return new Mapping(DocAddressTypes.Codes.ArrivalCTOAddress, nameof(Xsd.DocAddressAddressType.ACT));
			yield return new Mapping(DocAddressTypes.Codes.ArrivalCYDAddress, nameof(Xsd.DocAddressAddressType.ACY));
			yield return new Mapping(DocAddressTypes.Codes.DepartureCFSAddress, nameof(Xsd.DocAddressAddressType.DCF));
			yield return new Mapping(DocAddressTypes.Codes.DepartureCTOAddress, nameof(Xsd.DocAddressAddressType.DCT));
			yield return new Mapping(DocAddressTypes.Codes.DepartureCYDAddress, nameof(Xsd.DocAddressAddressType.DCY));

			yield return new Mapping(DocAddressTypes.Codes.DropOffAddress, nameof(Xsd.DocAddressAddressType.DOA));

			yield return new Mapping(DocAddressTypes.Codes.InsuredByDocumentaryAddress, nameof(Xsd.DocAddressAddressType.INS));
			yield return new Mapping(DocAddressTypes.Codes.AssuredPartyDocumentaryAddress, nameof(Xsd.DocAddressAddressType.ASS));
			yield return new Mapping(DocAddressTypes.Codes.ClaimsPayableByDocumentaryAddress, nameof(Xsd.DocAddressAddressType.CPB));
			yield return new Mapping(DocAddressTypes.Codes.SurveyReportPartyDocumentaryAddress, nameof(Xsd.DocAddressAddressType.SRO));

			yield return new Mapping(DocAddressTypes.Codes.TransportBillToAddress, nameof(Xsd.DocAddressAddressType.TBT));
			yield return new Mapping(DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, nameof(Xsd.DocAddressAddressType.TRA));

			yield return new Mapping(DocAddressTypes.Codes.CustomsContainerYardAddress, nameof(Xsd.DocAddressAddressType.CCP));
			yield return new Mapping(DocAddressTypes.Codes.CustomsContainerTerminalOperatorAddress, nameof(Xsd.DocAddressAddressType.CTO));
			yield return new Mapping(DocAddressTypes.Codes.CustomsDepotAddress, nameof(Xsd.DocAddressAddressType.CDE));
			yield return new Mapping(DocAddressTypes.Codes.CustomsWarehouseAddress, nameof(Xsd.DocAddressAddressType.CWA));
			yield return new Mapping(DocAddressTypes.Codes.CustomsSupervisingOffice, nameof(Xsd.DocAddressAddressType.SOF));
			yield return new Mapping(DocAddressTypes.Codes.CustomsTreatmentProviderAddress, nameof(Xsd.DocAddressAddressType.CTP));
			yield return new Mapping(DocAddressTypes.Codes.GovernmentContractor, nameof(Xsd.DocAddressAddressType.GOV));
			yield return new Mapping(DocAddressTypes.Codes.ProtestantAddress, nameof(Xsd.DocAddressAddressType.PRO));
			yield return new Mapping(DocAddressTypes.Codes.QuotationClientAddress, nameof(Xsd.DocAddressAddressType.LCA));
			yield return new Mapping(DocAddressTypes.Codes.CustomsPlaceOfLoading, nameof(Xsd.DocAddressAddressType.CPL));

			yield return new Mapping(DocAddressTypes.Codes.ControllingCustomer, nameof(Xsd.DocAddressAddressType.SCP));
			yield return new Mapping(DocAddressTypes.Codes.Consolidator, nameof(Xsd.DocAddressAddressType.CON));
			yield return new Mapping(DocAddressTypes.Codes.Manufacturer, nameof(Xsd.DocAddressAddressType.MAN));
			yield return new Mapping(DocAddressTypes.Codes.ManufacturerTranslatedDocumentaryAddress, nameof(Xsd.DocAddressAddressType.MTA));
			yield return new Mapping(DocAddressTypes.Codes.SellingParty, nameof(Xsd.DocAddressAddressType.SEP));
			yield return new Mapping(DocAddressTypes.Codes.BuyingParty, nameof(Xsd.DocAddressAddressType.BYP));
			yield return new Mapping(DocAddressTypes.Codes.ShipToParty, nameof(Xsd.DocAddressAddressType.STP));
			yield return new Mapping(DocAddressTypes.Codes.ScheduledContainerStuffingLocation, nameof(Xsd.DocAddressAddressType.CSL));

			yield return new Mapping(DocAddressTypes.Codes.LocalCartageWarehouse, nameof(Xsd.DocAddressAddressType.LCW));

			yield return new Mapping(DocAddressTypes.Codes.ExternalBroker, nameof(Xsd.DocAddressAddressType.EXB));

			yield return new Mapping(DocAddressTypes.Codes.ArrivalCFSLocalTransportAddress, nameof(Xsd.DocAddressAddressType.ALT));
			yield return new Mapping(DocAddressTypes.Codes.Carrier, nameof(Xsd.DocAddressAddressType.CAR));
			yield return new Mapping(DocAddressTypes.Codes.ContainerYardEmptyPickupAddress, nameof(Xsd.DocAddressAddressType.CPP));
			yield return new Mapping(DocAddressTypes.Codes.ContainerYardEmptyReturnAddress, nameof(Xsd.DocAddressAddressType.CPR));
			yield return new Mapping(DocAddressTypes.Codes.Contractor, nameof(Xsd.DocAddressAddressType.CTR));
			yield return new Mapping(DocAddressTypes.Codes.Creditor, nameof(Xsd.DocAddressAddressType.CDT));
			yield return new Mapping(DocAddressTypes.Codes.DepartureCFSLocalTransportAddress, nameof(Xsd.DocAddressAddressType.DLT));
			yield return new Mapping(DocAddressTypes.Codes.LocalClient, nameof(Xsd.DocAddressAddressType.LCN));
			yield return new Mapping(DocAddressTypes.Codes.OverseasAgent, nameof(Xsd.DocAddressAddressType.OSA));
			yield return new Mapping(DocAddressTypes.Codes.Location, nameof(Xsd.DocAddressAddressType.LOC));
			yield return new Mapping(DocAddressTypes.Codes.ReceivingForwarderAddress, nameof(Xsd.DocAddressAddressType.RFA));
			yield return new Mapping(DocAddressTypes.Codes.SendingForwarderAddress, nameof(Xsd.DocAddressAddressType.SFA));
			yield return new Mapping(DocAddressTypes.Codes.ShippingLineAddress, nameof(Xsd.DocAddressAddressType.SLA));
			yield return new Mapping(DocAddressTypes.Codes.IntermediateConsigneeAddress, nameof(Xsd.DocAddressAddressType.ICA));
			yield return new Mapping(DocAddressTypes.Codes.PickupAgent, nameof(Xsd.DocAddressAddressType.PAG));
			yield return new Mapping(DocAddressTypes.Codes.DeliveryAgent, nameof(Xsd.DocAddressAddressType.DVA));
			yield return new Mapping(DocAddressTypes.Codes.ExportBroker, nameof(Xsd.DocAddressAddressType.EXR));
			yield return new Mapping(DocAddressTypes.Codes.ImportBroker, nameof(Xsd.DocAddressAddressType.IMB));

			yield return new Mapping(DocAddressTypes.Codes.DrawbackExporterOrDestroyer, nameof(Xsd.DocAddressAddressType.DED));
			yield return new Mapping(DocAddressTypes.Codes.DrawbackLocationOfDestruction, nameof(Xsd.DocAddressAddressType.DLD));
			yield return new Mapping(DocAddressTypes.Codes.DrawbackLocationOfMerchandise, nameof(Xsd.DocAddressAddressType.DLM));

			yield return new Mapping(DocAddressTypes.Codes.Principal, nameof(Xsd.DocAddressAddressType.PRC));

			yield return new Mapping(DocAddressTypes.Codes.AQISProcessingEstablishment, nameof(Xsd.DocAddressAddressType.APE));
			yield return new Mapping(DocAddressTypes.Codes.Warehouse, nameof(Xsd.DocAddressAddressType.WHS));

			yield return new Mapping(DocAddressTypes.Codes.PlaceOfConsolidation, nameof(Xsd.DocAddressAddressType.POC));
			yield return new Mapping(DocAddressTypes.Codes.CommercialInvoiceOriginator, nameof(Xsd.DocAddressAddressType.CIO));

			yield return new Mapping(DocAddressTypes.Codes.FinalConsigneeAddress, nameof(Xsd.DocAddressAddressType.FCA));
			yield return new Mapping(DocAddressTypes.Codes.OriginatingConsignorAddress, nameof(Xsd.DocAddressAddressType.OCA));

			yield return new Mapping(DocAddressTypes.Codes.DistributionCentreAddress, nameof(Xsd.DocAddressAddressType.DCA));

			yield return new Mapping(DocAddressTypes.Codes.AdditionalDeliveryAddress, nameof(Xsd.DocAddressAddressType.ADA));
			yield return new Mapping(DocAddressTypes.Codes.AdditionalConsignee, nameof(Xsd.DocAddressAddressType.ADC));
			yield return new Mapping(DocAddressTypes.Codes.OGDProcessInspectionLPCO, nameof(Xsd.DocAddressAddressType.OIL));
			yield return new Mapping(DocAddressTypes.Codes.CFIAPaymentParty, nameof(Xsd.DocAddressAddressType.CFP));
			yield return new Mapping(DocAddressTypes.Codes.ControllingAgent, nameof(Xsd.DocAddressAddressType.CAG));
			yield return new Mapping(DocAddressTypes.Codes.GrossWeightVerifiedBy, nameof(Xsd.DocAddressAddressType.VGM));
			yield return new Mapping(DocAddressTypes.Codes.ImporterOfRecord, nameof(Xsd.DocAddressAddressType.IMR));
			yield return new Mapping(DocAddressTypes.Codes.Exporter, nameof(Xsd.DocAddressAddressType.EXP));

			yield return new Mapping(DocAddressTypes.Codes.MasterBillIssuingParty, nameof(Xsd.DocAddressAddressType.MBI));
			yield return new Mapping(DocAddressTypes.Codes.ClaimantAddress, nameof(Xsd.DocAddressAddressType.CLA));
			yield return new Mapping(DocAddressTypes.Codes.CoLoadWith, nameof(Xsd.DocAddressAddressType.COL));
			yield return new Mapping(DocAddressTypes.Codes.HouseBillIssuingParty, nameof(Xsd.DocAddressAddressType.HBI));

			yield return new Mapping(DocAddressTypes.Codes.CarrierAgent, nameof(Xsd.DocAddressAddressType.AGT));
			yield return new Mapping(DocAddressTypes.Codes.DestinationWarehouse, nameof(Xsd.DocAddressAddressType.DES));
			yield return new Mapping(DocAddressTypes.Codes.DispatchWarehouse, nameof(Xsd.DocAddressAddressType.DIS));
			yield return new Mapping(DocAddressTypes.Codes.GoodsOwner, nameof(Xsd.DocAddressAddressType.OWN));
			yield return new Mapping(DocAddressTypes.Codes.Transporter, nameof(Xsd.DocAddressAddressType.TRP));

			yield return new Mapping(DocAddressTypes.Codes.Custodian, nameof(Xsd.DocAddressAddressType.CTN));
			yield return new Mapping(DocAddressTypes.Codes.DisposalEntitledTrader, nameof(Xsd.DocAddressAddressType.DET));

			yield return new Mapping(DocAddressTypes.Codes.WarehouseClient, nameof(Xsd.DocAddressAddressType.WHC));
			yield return new Mapping(DocAddressTypes.Codes.Representative, nameof(Xsd.DocAddressAddressType.REP));
			yield return new Mapping(DocAddressTypes.Codes.CarrierBookingAgent, nameof(Xsd.DocAddressAddressType.CBA));
			yield return new Mapping(DocAddressTypes.Codes.CarrierHandlingAgent, nameof(Xsd.DocAddressAddressType.CHA));
			yield return new Mapping(DocAddressTypes.Codes.Acquirer, nameof(Xsd.DocAddressAddressType.ACQ));
			yield return new Mapping(DocAddressTypes.Codes.Declarant, nameof(Xsd.DocAddressAddressType.DLR));
			yield return new Mapping(DocAddressTypes.Codes.OutwardCarrierAgent, nameof(Xsd.DocAddressAddressType.OCT));
			yield return new Mapping(DocAddressTypes.Codes.BondedFactory, nameof(Xsd.DocAddressAddressType.BOF));
			yield return new Mapping(DocAddressTypes.Codes.AQISResponsiblePerson, nameof(Xsd.DocAddressAddressType.ARP));
			yield return new Mapping(DocAddressTypes.Codes.AQISTransitDestination, nameof(Xsd.DocAddressAddressType.ATD));
			yield return new Mapping(DocAddressTypes.Codes.AQISEUContactPerson, nameof(Xsd.DocAddressAddressType.AEC));
			yield return new Mapping(DocAddressTypes.Codes.CBPBroker, nameof(Xsd.DocAddressAddressType.CBP));
			yield return new Mapping(DocAddressTypes.Codes.GoodsLocation, nameof(Xsd.DocAddressAddressType.GLC));
			yield return new Mapping(DocAddressTypes.Codes.LocationOfGoods, nameof(Xsd.DocAddressAddressType.LCG));
			yield return new Mapping(DocAddressTypes.Codes.ContainerPacking, nameof(Xsd.DocAddressAddressType.CPK));
			yield return new Mapping(DocAddressTypes.Codes.GoodsAvailableAt, nameof(Xsd.DocAddressAddressType.GAA));
			yield return new Mapping(DocAddressTypes.Codes.GoodsDeliveredTo, nameof(Xsd.DocAddressAddressType.GDT));
			yield return new Mapping(DocAddressTypes.Codes.RefundParty, nameof(Xsd.DocAddressAddressType.RFP));
			yield return new Mapping(DocAddressTypes.Codes.InwardCarrierAgent, nameof(Xsd.DocAddressAddressType.ICT));
			yield return new Mapping(DocAddressTypes.Codes.FDASubmitter, nameof(Xsd.DocAddressAddressType.FDA));
			yield return new Mapping(DocAddressTypes.Codes.UltimateConsignee, nameof(Xsd.DocAddressAddressType.UCE));
			yield return new Mapping(DocAddressTypes.Codes.IntermediateConsignee, nameof(Xsd.DocAddressAddressType.ICE));
			yield return new Mapping(DocAddressTypes.Codes.USPrincipalPartyInInterest, nameof(Xsd.DocAddressAddressType.PPI));
			yield return new Mapping(DocAddressTypes.Codes.DefermentParty, nameof(Xsd.DocAddressAddressType.DFP));
			yield return new Mapping(DocAddressTypes.Codes.InwardProcessingPlace, nameof(Xsd.DocAddressAddressType.IPP));
			yield return new Mapping(DocAddressTypes.Codes.LocalProcessorAddress, nameof(Xsd.DocAddressAddressType.LPA));
			yield return new Mapping(DocAddressTypes.Codes.MainAccountingAddress, nameof(Xsd.DocAddressAddressType.MAA));
			yield return new Mapping(DocAddressTypes.Codes.Forwarder, nameof(Xsd.DocAddressAddressType.FFW));
			yield return new Mapping(DocAddressTypes.Codes.JustificationContactDetailAddress, nameof(Xsd.DocAddressAddressType.JCD));
			yield return new Mapping(DocAddressTypes.Codes.AdministratorOfCustomsWork, nameof(Xsd.DocAddressAddressType.ACW));
			yield return new Mapping(DocAddressTypes.Codes.InspectionWitness, nameof(Xsd.DocAddressAddressType.IWS));
			yield return new Mapping(DocAddressTypes.Codes.ContractualPartner, nameof(Xsd.DocAddressAddressType.CAP));
			yield return new Mapping(DocAddressTypes.Codes.Payer, nameof(Xsd.DocAddressAddressType.PYR));
			yield return new Mapping(DocAddressTypes.Codes.BoardingLocalDocumentaryAddress, nameof(Xsd.DocAddressAddressType.BLD));
			yield return new Mapping(DocAddressTypes.Codes.Stevedore, nameof(Xsd.DocAddressAddressType.SVD));
			yield return new Mapping(DocAddressTypes.Codes.LPCOApplicant, nameof(Xsd.DocAddressAddressType.APP));
			yield return new Mapping(DocAddressTypes.Codes.LPCOHolder, nameof(Xsd.DocAddressAddressType.HOL));
			yield return new Mapping(DocAddressTypes.Codes.LocalProcessorTranslatedDocAddress, nameof(Xsd.DocAddressAddressType.LTA));
			yield return new Mapping(DocAddressTypes.Codes.Supplier, nameof(Xsd.DocAddressAddressType.SUP));
			yield return new Mapping(DocAddressTypes.Codes.SellerDocumentaryAddress, nameof(Xsd.DocAddressAddressType.SEL));
			yield return new Mapping(DocAddressTypes.Codes.AQISEUPlaceOfDestination, nameof(Xsd.DocAddressAddressType.APD));
			yield return new Mapping(DocAddressTypes.Codes.COLSDeliveryOrUnpack, nameof(Xsd.DocAddressAddressType.CDU));
			yield return new Mapping(DocAddressTypes.Codes.COLSResponsibleParty, nameof(Xsd.DocAddressAddressType.CRP));
			yield return new Mapping(DocAddressTypes.Codes.COLSDirectionAAAddress, nameof(Xsd.DocAddressAddressType.AAA));
			yield return new Mapping(DocAddressTypes.Codes.MasterBillConsigneeOverride, nameof(Xsd.DocAddressAddressType.MBC));
			yield return new Mapping(DocAddressTypes.Codes.MasterBillShipperOverride, nameof(Xsd.DocAddressAddressType.MBS));
			yield return new Mapping(DocAddressTypes.Codes.ClearanceLocalInvolvedParty, nameof(Xsd.DocAddressAddressType.CLR));
			yield return new Mapping(DocAddressTypes.Codes.ContainerOwnerAddress, nameof(Xsd.DocAddressAddressType.COA));
			yield return new Mapping(DocAddressTypes.Codes.BuyerTranslatedDocumentaryAddress, nameof(Xsd.DocAddressAddressType.BTA));
			yield return new Mapping(DocAddressTypes.Codes.CarrierExportCreditor, nameof(Xsd.DocAddressAddressType.CEC));
			yield return new Mapping(DocAddressTypes.Codes.CarrierImportCreditor, nameof(Xsd.DocAddressAddressType.CIC));
			yield return new Mapping(DocAddressTypes.Codes.ConsignorSecurityAddress, nameof(Xsd.DocAddressAddressType.COS));
			yield return new Mapping(DocAddressTypes.Codes.ConsigneeSecurityAddress, nameof(Xsd.DocAddressAddressType.CES));
			yield return new Mapping(DocAddressTypes.Codes.ICS2FacilityPlace, nameof(Xsd.DocAddressAddressType.IFP));
			yield return new Mapping(DocAddressTypes.Codes.ReturnAddress, nameof(Xsd.DocAddressAddressType.RET));
			yield return new Mapping(DocAddressTypes.Codes.Applicant, nameof(Xsd.DocAddressAddressType.APC));
			yield return new Mapping(DocAddressTypes.Codes.ApplicantTranslatedDocumentaryAddress, nameof(Xsd.DocAddressAddressType.ATA));
			yield return new Mapping(DocAddressTypes.Codes.FreightPayer, nameof(Xsd.DocAddressAddressType.FPY));
			yield return new Mapping(DocAddressTypes.Codes.OwnerOfGoods, nameof(Xsd.DocAddressAddressType.OOG));

			yield return new Mapping(DocAddressTypes.Codes.Shipper, nameof(Xsd.DocAddressAddressType.SHP));
			yield return new Mapping(DocAddressTypes.Codes.FSVPImporter, nameof(Xsd.DocAddressAddressType.FSI));
			yield return new Mapping(DocAddressTypes.Codes.InitialImporter, nameof(Xsd.DocAddressAddressType.ITI));
			yield return new Mapping(DocAddressTypes.Codes.Sponsor, nameof(Xsd.DocAddressAddressType.SPS));
			yield return new Mapping(DocAddressTypes.Codes.Grower, nameof(Xsd.DocAddressAddressType.GRO));
			yield return new Mapping(DocAddressTypes.Codes.Laboratory, nameof(Xsd.DocAddressAddressType.LAB));
			yield return new Mapping(DocAddressTypes.Codes.FDAShipperAddress, nameof(Xsd.DocAddressAddressType.FSP));
			yield return new Mapping(DocAddressTypes.Codes.InvoicerAddress, nameof(Xsd.DocAddressAddressType.IVC));
			yield return new Mapping(DocAddressTypes.Codes.PermitOwner, nameof(Xsd.DocAddressAddressType.PEO));
			yield return new Mapping(DocAddressTypes.Codes.ThirdPartyLaboratory, nameof(Xsd.DocAddressAddressType.TPL));
			yield return new Mapping(DocAddressTypes.Codes.ContainerAgentCodeAddress, nameof(Xsd.DocAddressAddressType.CAC));
			yield return new Mapping(DocAddressTypes.Codes.DutyPayer, nameof(Xsd.DocAddressAddressType.DYP));
			yield return new Mapping(DocAddressTypes.Codes.VanningLocationAddress, nameof(Xsd.DocAddressAddressType.VAN));
			yield return new Mapping(DocAddressTypes.Codes.Holder, nameof(Xsd.DocAddressAddressType.HLD));
			yield return new Mapping(DocAddressTypes.Codes.SurrenderParty, nameof(Xsd.DocAddressAddressType.SRP));
			yield return new Mapping(DocAddressTypes.Codes.ConsigneeElectronicBOLAddress, nameof(Xsd.DocAddressAddressType.CEB));
			yield return new Mapping(DocAddressTypes.Codes.ConsignorAddress, nameof(Xsd.DocAddressAddressType.CRA));
			yield return new Mapping(DocAddressTypes.Codes.AQISLoadingEstablishment, nameof(Xsd.DocAddressAddressType.ALE));
			yield return new Mapping(DocAddressTypes.Codes.SelfFiler, nameof(Xsd.DocAddressAddressType.ICS));
			yield return new Mapping(DocAddressTypes.Codes.CustomsExportOrientedUnitsAddress, nameof(Xsd.DocAddressAddressType.EOU));
			yield return new Mapping(DocAddressTypes.Codes.ToOrder, nameof(Xsd.DocAddressAddressType.TOR));
			yield return new Mapping(DocAddressTypes.Codes.AttorneyForCustomsProceduresAddress, nameof(Xsd.DocAddressAddressType.ACP));
			yield return new Mapping(DocAddressTypes.Codes.SupportingDocumentOrganizationAddress, nameof(Xsd.DocAddressAddressType.SDP));
			yield return new Mapping(DocAddressTypes.Codes.Transhipper, nameof(Xsd.DocAddressAddressType.TRS));
			yield return new Mapping(DocAddressTypes.Codes.AirCargoAgent, nameof(Xsd.DocAddressAddressType.ACA));
			yield return new Mapping(DocAddressTypes.Codes.AuthorizedEconomicOperatorAddress, nameof(Xsd.DocAddressAddressType.AEO));
		}

		public static readonly DocAddressTypeXmlMappings Instance = new DocAddressTypeXmlMappings();

		public Xsd.DocAddressAddressType GetExternalCode(JobDocAddress docAddress, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(docAddress.E2_AddressType, Xsd.DocAddressAddressType.NONE, errorContext, notifications);
		}

		protected override string GetExternalCodeCore(string enterpriseCode, string errorContext, INotifications notifications)
		{
			throw new NotSupportedException("Call the other overload");
		}

		protected override string Name
		{
			get { return Res.GetString("a65e471a-6f04-4096-bede-f22f3f9c24e9", "Address Type"); }
		}
	}
}
