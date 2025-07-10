using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public interface IOrganisationDetails
	{
		#region Part Attributes

		ZString PartAttrib1Name { get; }
		ZString PartAttrib2Name { get; }
		ZString PartAttrib3Name { get; }
		ZBool HasPartAttrib1 { get; }
		ZBool HasPartAttrib2 { get; }
		ZBool HasPartAttrib3 { get; }
		ZBool HasSerialNumber { get; }

		#endregion

		#region Contacts

		DocContactsCollection Contacts { get; }
		DocContacts SalesContact { get; }
		DocContacts ExportFowardingContact { get; }
		DocContacts ImportFowardingContact { get; }
		DocContacts ARContact { get; }
		DocContacts DefaultContact(ContactType defaultContactType);
		DocContacts DefaultContact(Guid menuItem, ContactType defaultContactType);

		#endregion

		#region Addresses

		DocAddressCollection Addresses { get; }
		DocDocAddressCollection DocAddresses { get; }
		DocAddress MainAddress { get; }
		DocAddress OrganisationPADPostalAddress { get; }

		#endregion

		#region Cus Code Collection
		DocCusCodeCollection CustomCodes { get; }
		#endregion

		#region AppointedAgentPorts
		DocAppointedAgentPortsCollection AppointedAgentPorts { get; }
		#endregion

		#region Basic Organisation Details

		ZString OrgType { get; }
		ZString[] AllNotes { get; }
		ZDateTime CurrentDate { get; }
		ZString AdditionalAddressInformation { get; }
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString City { get; }
		ZString Code { get; }
		ZString Email { get; }
		ZString Fax { get; }
		ZString BusinessRegNo { get; }
		ZString BusinessRegType { get; }
		ZString Name { get; }
		DocBranch Branch { get; }
		ZBool IsActive { get; }
		ZBool IsAirCTO { get; }
		ZBool IsAirLine { get; }
		ZBool IsAirWholesaler { get; }
		ZBool IsBroker { get; }
		ZBool IsCompetitor { get; }
		ZBool IsConsignee { get; }
		ZBool IsConsignor { get; }
		ZBool IsContainerPark { get; }
		ZBool IsCreditor { get; }
		ZBool IsDebtor { get; }
		ZBool IsForwarder { get; }
		ZBool IsInlandWaterwayProvider { get; }
		ZBool IsLineHaulProvider { get; }
		ZBool IsLocalTransport { get; }
		ZBool IsMiscFreightServices { get; }
		ZBool IsPackDepot { get; }
		ZBool IsRailProvider { get; }
		ZBool IsSalesLead { get; }
		ZBool IsSeaCTO { get; }
		ZBool IsSeaWholesaler { get; }
		ZBool IsShippingLine { get; }
		ZBool IsShippingProvider { get; }
		ZBool IsTempAccount { get; }
		ZBool IsTransportClient { get; }
		ZBool IsUnpackDepot { get; }
		ZBool IsWarehouseClient { get; }
		ZString Mobile { get; }
		ZString Phone { get; }
		ZString PostCode { get; }
		DocUNLOCO Loco { get; }
		ZString State { get; }
		ZString Web { get; }
		ZString HandlingInstructions { get; }
		ZString GetHandlingInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode);
		ZString GetHandlingInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode);
		ZString CartageInstructions { get; }
		ZString GetCartageInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode);
		ZString GetCartageInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode);
		ZString LocalCustomsClientCode { get; }
		ZString LocalCustomsCarrierCode { get; }
		ZString LocalBusinessRegNo { get; }
		ZString LocalCustomsSupplierCode { get; }
		ZString LocalRebateUserCode { get; }
		ZString LocalVATCode { get; }
		ZString GetSpecialInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode);
		ZString GetSpecialInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode);

		DocOrgStaffAssignmentsCollection StaffAssignments { get; }

		#endregion

		#region Marketing Flags

		ZBool IsUserFlag1 { get; }
		ZBool IsUserFlag2 { get; }
		ZBool IsUserFlag3 { get; }
		ZBool IsUserFlag4 { get; }
		ZBool IsUserFlag5 { get; }
		ZBool IsUserFlag6 { get; }
		ZBool IsUserFlag7 { get; }
		ZBool IsUserFlag8 { get; }
		ZBool IsUserFlag9 { get; }
		ZBool IsUserFlag10 { get; }
		ZBool IsUserFlag11 { get; }
		ZBool IsUserFlag12 { get; }
		ZBool IsUserFlag13 { get; }
		ZBool IsUserFlag14 { get; }
		ZBool IsUserFlag15 { get; }
		ZBool IsUserFlag16 { get; }
		ZBool IsUserFlag17 { get; }
		ZBool IsUserFlag18 { get; }
		ZBool IsUserFlag19 { get; }
		ZBool IsUserFlag20 { get; }
		ZBool IsUserFlag21 { get; }
		ZBool IsUserFlag22 { get; }
		ZBool IsUserFlag23 { get; }
		ZBool IsUserFlag24 { get; }
		ZBool IsUserFlag25 { get; }
		ZBool IsUserFlag26 { get; }
		ZBool IsUserFlag27 { get; }
		ZBool IsUserFlag28 { get; }
		ZBool IsUserFlag29 { get; }
		ZBool IsUserFlag30 { get; }
		ZBool IsUserFlag31 { get; }
		ZBool IsUserFlag32 { get; }

		#endregion

		#region Custom Labels

		ZString CustomAttrib1 { get; }
		ZString CustomAttrib2 { get; }
		ZString CustomAttrib3 { get; }
		ZDecimal CustomDecimal1 { get; }
		ZDecimal CustomDecimal2 { get; }
		ZDecimal CustomDecimal3 { get; }
		ZDateTime CustomDate1 { get; }
		ZDateTime CustomDate2 { get; }
		ZDateTime CustomDate3 { get; }
		ZBool CustomFlag1 { get; }
		ZBool CustomFlag2 { get; }
		ZBool CustomFlag3 { get; }

		#endregion

		#region Custom Fields

		ZString ApprovedExporterCode { get; }
		ZString CarrierMasterBillPrefix { get; }
		ZString CarrierAirlinePrefix { get; }
		ZString PostalAddress { get; }
		ZString PostalAddressInEnglish { get; }
		ZString PostalAddressExcludeCountryIfSame { get; }
		ZString PostalAddressExcludeName { get; }
		DocCountry Country { get; }
		DocCountryData CountryData { get; }
		DocMiscServ MiscServ { get; }
		ZString SCAC { get; }
		ZString ABN { get; }
		ZString GST { get; }
		ZString CCC { get; }
		ZString CBR { get; }
		ZString CSC { get; }
		ZString CCD { get; }
		ZString CID { get; }
		ZString LSC { get; }
		ZString PAN { get; }
		ZString Kennitala { get; }
		ZString DisbursmentTerms { get; }
		ZString ShortDisbursementTerms { get; }
		ZString ShortInvoiceTerms { get; }

		#endregion

		#region Address Rules

		DocAddress DeliverAddress { get; }
		DocDocAddress DeliverDocAddress { get; }
		DocAddress PickUpAddress { get; }
		DocDocAddress PickUpDocAddress { get; }
		DocAddress ARAddress { get; }
		DocAddress PostalAddressMain { get; }

		#endregion

		#region Spliting of Name

		ZString SplitFullName1 { get; }
		ZString SplitFullName2 { get; }

		#endregion
	}
}

