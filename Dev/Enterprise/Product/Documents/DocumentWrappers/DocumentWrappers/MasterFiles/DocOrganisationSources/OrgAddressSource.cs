using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class OrgAddressSource : DocBaseWrapper, IOrganisationDetails
	{
		OrgAddressSource(JobDocAddress jobDocAddress, BusinessObjectFactory factory)
			: base(jobDocAddress, factory)
		{
		}

		public static OrgAddressSource New(JobDocAddress jobDocAddress, BusinessObjectFactory factoryForWrapper)
		{
			return (jobDocAddress != null) ? new OrgAddressSource(jobDocAddress, factoryForWrapper) : null;
		}

		DocCountry fCountry;
		protected JobDocAddress JobDocAddress
		{
			get { return (JobDocAddress)WrappedObject; }
		}

		#region Part Attributes

		public ZString PartAttrib1Name
		{
			get { return ""; }
		}

		public ZString PartAttrib2Name
		{
			get { return ""; }
		}

		public ZString PartAttrib3Name
		{
			get { return ""; }
		}

		public ZBool HasPartAttrib1
		{
			get { return false; }
		}

		public ZBool HasPartAttrib2
		{
			get { return false; }
		}

		public ZBool HasPartAttrib3
		{
			get { return false; }
		}

		public ZBool HasSerialNumber
		{
			get { return false; }
		}

		#endregion

		#region Contacts

		public DocContactsCollection Contacts
		{
			get
			{
				DocContactsCollection result = new DocContactsCollection(Factory);
				DocContacts docContact = DocContacts.New(JobDocAddress, Factory);
				if (docContact != null)
				{
					result.Add(docContact);
				}
				return result;
			}
		}

		public DocContacts SalesContact
		{
			get { return DocContacts.New(JobDocAddress, Factory); }
		}

		public DocContacts ExportFowardingContact
		{
			get { return DocContacts.New(JobDocAddress, Factory); }
		}

		public DocContacts ImportFowardingContact
		{
			get { return DocContacts.New(JobDocAddress, Factory); }
		}

		public DocContacts DefaultContact(ContactType defaultContactType)
		{
			return DocContacts.New(JobDocAddress, Factory);
		}

		public DocContacts DefaultContact(Guid menuItem, ContactType defaultContactType)
		{
			OrgContact contact = new DefaultContactFinder(JobDocAddress.Organisation).DefaultContact(menuItem, defaultContactType);
			return DocContacts.New(contact, Factory);
		}

		public DocContacts ARContact
		{
			get
			{
				if (fARContacts == null)
				{
					DefaultContactFinder finder = new DefaultContactFinder(this.JobDocAddress.Organisation);
					fARContacts = DocContacts.New(finder.DefaultContact(ContactType.Receivables), Factory);
				}
				return fARContacts;
			}
		}
		DocContacts fARContacts;

		#endregion

		#region Addresses

		public DocAddressCollection Addresses
		{
			get { return new DocAddressCollection(Factory); }
		}

		public DocDocAddressCollection DocAddresses
		{
			get
			{
				DocDocAddressCollection docDocAddressCollection = new DocDocAddressCollection(Factory);
				docDocAddressCollection.Add(DocDocAddress.New(JobDocAddress, Factory));
				return docDocAddressCollection;
			}
		}

		public DocAddress MainAddress
		{
			get { return null; }
		}

		public DocAddress OrganisationPADPostalAddress
		{
			get { return null; }
		}

		#endregion

		#region Cus Code Collection
		public DocCusCodeCollection CustomCodes
		{
			get { return new DocCusCodeCollection(new OrgCusCodeCollection(null, Factory), Factory); }
		}
		#endregion

		#region AppointedAgentPorts

		public DocAppointedAgentPortsCollection AppointedAgentPorts
		{
			get { return new DocAppointedAgentPortsCollection(Factory); }
		}
		#endregion

		#region Basic Organisation Details
		public ZString OrgType
		{
			get { return ""; }
		}

		public ZString[] AllNotes
		{
			get { return Array.Empty<ZString>(); }
		}

		public ZDateTime CurrentDate
		{
			get { return ZDateTime.Today; }
		}

		public ZString AdditionalAddressInformation
		{
			get { return string.Empty; }
		}

		public ZString Address1
		{
			get { return JobDocAddress.E2_Address1; }
		}

		public ZString Address2
		{
			get { return JobDocAddress.E2_Address2; }
		}

		public ZString City
		{
			get { return JobDocAddress.E2_City; }
		}

		public ZString Code
		{
			get { return ""; }
		}

		public ZString Email
		{
			get { return JobDocAddress.E2_Email; }
		}

		public ZString Fax
		{
			get { return JobDocAddress.E2_Fax_Formatted; }
		}

		public ZString BusinessRegNo
		{
			get { return ""; }
		}

		public ZString BusinessRegType
		{
			get { return ""; }
		}

		public ZString Name
		{
			get { return JobDocAddress.E2_CompanyName; }
		}

		public DocBranch Branch
		{
			get { return null; }
		}

		public ZBool IsActive
		{
			get { return false; }
		}

		public ZBool IsAirCTO
		{
			get { return false; }
		}

		public ZBool IsAirLine
		{
			get { return false; }
		}

		public ZBool IsAirWholesaler
		{
			get { return false; }
		}

		public ZBool IsBroker
		{
			get { return false; }
		}

		public ZBool IsCompetitor
		{
			get { return false; }
		}

		public ZBool IsConsignee
		{
			get { return false; }
		}

		public ZBool IsConsignor
		{
			get { return false; }
		}

		public ZBool IsContainerPark
		{
			get { return false; }
		}

		public ZBool IsCreditor
		{
			get { return false; }
		}

		public ZBool IsDebtor
		{
			get { return false; }
		}

		public ZBool IsForwarder
		{
			get { return false; }
		}

		public ZBool IsInlandWaterwayProvider
		{
			get { return false; }
		}

		public ZBool IsLineHaulProvider
		{
			get { return false; }
		}

		public ZBool IsLocalTransport
		{
			get { return false; }
		}

		public ZBool IsMiscFreightServices
		{
			get { return false; }
		}

		public ZBool IsPackDepot
		{
			get { return false; }
		}

		public ZBool IsRailProvider
		{
			get { return false; }
		}

		public ZBool IsSalesLead
		{
			get { return false; }
		}

		public ZBool IsSeaCTO
		{
			get { return false; }
		}

		public ZBool IsSeaWholesaler
		{
			get { return false; }
		}

		public ZBool IsShippingLine
		{
			get { return false; }
		}

		public ZBool IsShippingProvider
		{
			get { return false; }
		}

		public ZBool IsTempAccount
		{
			get { return false; }
		}

		public ZBool IsTransportClient
		{
			get { return false; }
		}

		public ZBool IsUnpackDepot
		{
			get { return false; }
		}

		public ZBool IsWarehouseClient
		{
			get { return false; }
		}

		public ZString Mobile
		{
			get { return ""; }
		}

		public ZString Phone
		{
			get { return JobDocAddress.E2_Phone_Formatted; }
		}

		public ZString PostCode
		{
			get { return JobDocAddress.E2_Postcode; }
		}

		public DocUNLOCO Loco
		{
			get { return null; }
		}

		public ZString State
		{
			get { return JobDocAddress.E2_State; }
		}

		public ZString Web
		{
			get { return ""; }
		}

		public ZString HandlingInstructions
		{
			get { return ""; }
		}

		public ZString GetHandlingInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return "";
		}

		public ZString GetHandlingInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return "";
		}

		public ZString CartageInstructions
		{
			get { return ""; }
		}

		public ZString GetCartageInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return "";
		}

		public ZString GetCartageInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return "";
		}

		public ZString LocalCustomsClientCode
		{
			get { return ""; }
		}

		public ZString LocalCustomsCarrierCode
		{
			get { return ""; }
		}

		public ZString LocalBusinessRegNo
		{
			get { return ""; }
		}

		public ZString LocalCustomsSupplierCode
		{
			get { return ""; }
		}

		public ZString LocalRebateUserCode
		{
			get { return ""; }
		}

		public ZString LocalVATCode
		{
			get { return ""; }
		}

		public ZString GetSpecialInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return "";
		}

		public ZString GetSpecialInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return "";
		}

		public DocOrgStaffAssignmentsCollection StaffAssignments
		{
			get
			{
				return JobDocAddress.Organisation == null ? new DocOrgStaffAssignmentsCollection(Factory) : new DocOrgStaffAssignmentsCollection(JobDocAddress.Organisation.StaffAssignments, Factory);
			}
		}

		#endregion

		#region Custom Labels

		public ZString CustomAttrib1
		{
			get { return ""; }
		}

		public ZString CustomAttrib2
		{
			get { return ""; }
		}

		public ZString CustomAttrib3
		{
			get { return ""; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return 0; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return 0; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return 0; }
		}

		public ZDateTime CustomDate1
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CustomDate2
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CustomDate3
		{
			get { return ZDateTime.Empty; }
		}

		public ZBool CustomFlag1
		{
			get { return false; }
		}

		public ZBool CustomFlag2
		{
			get { return false; }
		}

		public ZBool CustomFlag3
		{
			get { return false; }
		}

		#endregion

		#region  Marketing Flags

		public ZBool IsUserFlag1
		{
			get { return false; }
		}

		public ZBool IsUserFlag2
		{
			get { return false; }
		}

		public ZBool IsUserFlag3
		{
			get { return false; }
		}

		public ZBool IsUserFlag4
		{
			get { return false; }
		}

		public ZBool IsUserFlag5
		{
			get { return false; }
		}

		public ZBool IsUserFlag6
		{
			get { return false; }
		}

		public ZBool IsUserFlag7
		{
			get { return false; }
		}

		public ZBool IsUserFlag8
		{
			get { return false; }
		}

		public ZBool IsUserFlag9
		{
			get { return false; }
		}

		public ZBool IsUserFlag10
		{
			get { return false; }
		}

		public ZBool IsUserFlag11
		{
			get { return false; }
		}

		public ZBool IsUserFlag12
		{
			get { return false; }
		}

		public ZBool IsUserFlag13
		{
			get { return false; }
		}

		public ZBool IsUserFlag14
		{
			get { return false; }
		}

		public ZBool IsUserFlag15
		{
			get { return false; }
		}

		public ZBool IsUserFlag16
		{
			get { return false; }
		}

		public ZBool IsUserFlag17
		{
			get { return false; }
		}

		public ZBool IsUserFlag18
		{
			get { return false; }
		}

		public ZBool IsUserFlag19
		{
			get { return false; }
		}

		public ZBool IsUserFlag20
		{
			get { return false; }
		}

		public ZBool IsUserFlag21
		{
			get { return false; }
		}

		public ZBool IsUserFlag22
		{
			get { return false; }
		}

		public ZBool IsUserFlag23
		{
			get { return false; }
		}

		public ZBool IsUserFlag24
		{
			get { return false; }
		}

		public ZBool IsUserFlag25
		{
			get { return false; }
		}

		public ZBool IsUserFlag26
		{
			get { return false; }
		}

		public ZBool IsUserFlag27
		{
			get { return false; }
		}

		public ZBool IsUserFlag28
		{
			get { return false; }
		}

		public ZBool IsUserFlag29
		{
			get { return false; }
		}

		public ZBool IsUserFlag30
		{
			get { return false; }
		}

		public ZBool IsUserFlag31
		{
			get { return false; }
		}

		public ZBool IsUserFlag32
		{
			get { return false; }
		}

		#endregion

		#region Custom Fields

		public ZString ApprovedExporterCode
		{
			get { return ""; }
		}

		public ZString CarrierMasterBillPrefix
		{
			get { return ""; }
		}

		public ZString CarrierAirlinePrefix
		{
			get { return ""; }
		}

		public ZString PostalAddress
		{
			get { return GetPostalAddress(); }
		}

		public ZString PostalAddressInEnglish
		{
			get { return GetPostalAddress(true); }
		}

		ZString GetPostalAddress(bool inEnglish = false)
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var docCompany = DocCompany.New(currentCompany, Factory);

			var formatter = new WrapperPostalAddressFormatter(DocDocAddress.New(JobDocAddress, Factory), docCompany);
			formatter.EnglishOnly = inEnglish;

			return formatter.PostalAddress();
		}

		public ZString PostalAddressExcludeCountryIfSame
		{
			get
			{
				var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				DocCompany docCompany = DocCompany.New(currentCompany, Factory);
				return new WrapperPostalAddressFormatter(DocDocAddress.New(JobDocAddress, Factory), docCompany, false).PostalAddress();
			}
		}

		public ZString PostalAddressExcludeName
		{
			get
			{
				if (PostalAddress.ToString().StartsWith(Name, true, null))
				{
					return PostalAddress.SubstringSafe(Name.Length).TrimStart('\n');
				}
				else
				{
					return PostalAddress;
				}
			}
		}

		public DocCountry Country
		{
			get
			{
				var country = JobDocAddress.Country ?? emptyCountry ?? (emptyCountry = new BusinessObjectFactory { NameForDebugging = "DocWrappersReadOnlyFactory" }.New<RefCountry>());
				if (fCountry == null || fCountry.Code != country.Code)
				{
					fCountry = DocCountry.New(country, Factory);
				}
				return fCountry;
			}
		}
		RefCountry emptyCountry;

		public DocMiscServ MiscServ
		{
			get
			{
				if (miscServ == null && JobDocAddress != null)
				{
					miscServ = DocMiscServ.New(JobDocAddress.Organisation.MiscServ, Factory);
				}
				return miscServ;
			}
		}
		DocMiscServ miscServ;

		public DocCountryData CountryData
		{
			get { return null; }
		}

		public ZString SCAC
		{
			get { return ""; }
		}

		public ZString ABN
		{
			get { return ""; }
		}

		public ZString GST
		{
			get { return ""; }
		}

		public ZString CCC
		{
			get { return ""; }
		}

		public ZString CBR
		{
			get { return ZString.Empty; }
		}

		public ZString CSC
		{
			get { return ""; }
		}

		public ZString CCD
		{
			get { return ""; }
		}

		public ZString CID
		{
			get { return ""; }
		}

		public ZString LSC
		{
			get { return ""; }
		}

		public ZString PAN
		{
			get { return ""; }
		}

		public ZString Kennitala
		{
			get { return ""; }
		}

		public ZString DisbursmentTerms
		{
			get { return ""; }
		}

		public ZString ShortDisbursementTerms
		{
			get { return ""; }
		}

		public ZString ShortInvoiceTerms
		{
			get { return ""; }
		}
		#endregion

		#region Address Rules

		public DocAddress DeliverAddress
		{
			get { return null; }
		}

		public DocDocAddress DeliverDocAddress
		{
			get { return DocDocAddress.New(JobDocAddress, Factory); }
		}

		public DocAddress PickUpAddress
		{
			get { return null; }
		}

		public DocDocAddress PickUpDocAddress
		{
			get { return DocDocAddress.New(JobDocAddress, Factory); }
		}

		public DocAddress PostalAddressMain
		{
			get { return null; }
		}

		public DocAddress ARAddress
		{
			get { return null; }
		}

		#endregion

		#region Spliting of Name

		public ZString SplitFullName1
		{
			get
			{
				SplitFullName();
				return fSplitFullName1;
			}
		}

		public ZString SplitFullName2
		{
			get
			{
				SplitFullName();
				return fSplitFullName2;
			}
		}

		protected ZString fSplitFullName1;
		protected ZString fSplitFullName2;

		protected void SplitFullName()
		{
			if (fSplitFullName1.IsEmpty && fSplitFullName2.IsEmpty)
			{
				ZString[] splitFormattedName = WrapTextForAColumn(Name, 25).Split('\n');

				if (splitFormattedName.Length > 0)
				{
					fSplitFullName1 = splitFormattedName[0];
				}

				if (splitFormattedName.Length > 1)
				{
					fSplitFullName2 = splitFormattedName[1];
				}
			}
		}

		#endregion
	}
}
