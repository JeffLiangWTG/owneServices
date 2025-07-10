using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers
{
	[DefaultField("Name")]
	public class DocOrganisation : GenericWrapperWithNotes, Integration.DocumentWrappers.IDocOrganisation
	{
		protected DocOrganisation(IOrganisationDetails source, BusinessObjectFactory factoryForWrapper)
			: base((BusinessObject)source, factoryForWrapper)
		{
		}

		public static DocOrganisation New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<OrgHeader>(pK), factory);
		}

		public static DocOrganisation New(OrgHeader orgHeader, BusinessObjectFactory factoryForWrapper)
		{
			return (orgHeader != null) ? factoryForWrapper.GetCachedValue
				(
					orgHeader.PK.ToStringKey(),
					delegate
					{
						DocOrganisation result = new DocOrganisation(OrgHeaderSource.New(orgHeader, factoryForWrapper), factoryForWrapper);
						result.SelectedAddress = DocDocAddress.New(orgHeader.MainAddress, factoryForWrapper);
						return result;
					},
					CacheStalenessPolicy.StaleOnFactorySave
				) : null;
		}

		public static DocOrganisation New(OrgAddress orgAddress, BusinessObjectFactory factoryForWrapper)
		{
			DocOrganisation docOrganisation = null;
			if (orgAddress != null)
			{
				if (orgAddress.Header != null)
				{
					docOrganisation = factoryForWrapper.GetCachedValue
					(
							orgAddress.Header.PK.ToStringKey(),
							delegate
							{
								return new DocOrganisation(OrgHeaderSource.New(orgAddress.Header, factoryForWrapper), factoryForWrapper);
							},
							CacheStalenessPolicy.StaleOnFactorySave
					);
					docOrganisation.SelectedAddress = DocDocAddress.New(orgAddress, factoryForWrapper);
				}
			}
			return docOrganisation;
		}

		public static DocOrganisation New(JobDocAddress jobDocAddress, BusinessObjectFactory factoryForWrapper)
		{
			DocOrganisation docOrganisation = null;
			if (jobDocAddress != null)
			{
				if (jobDocAddress.Organisation != null && !jobDocAddress.E2_AddressOverride)
				{
					docOrganisation = factoryForWrapper.GetCachedValue
					(
						jobDocAddress.Organisation.PK.ToStringKey(),
						delegate
						{
							return new DocOrganisation(OrgHeaderSource.New(jobDocAddress.Organisation, factoryForWrapper), factoryForWrapper);
						},
						CacheStalenessPolicy.StaleOnFactorySave
					);
					docOrganisation.SelectedAddress = DocDocAddress.New(jobDocAddress, factoryForWrapper);
				}
				else if (jobDocAddress.E2_AddressOverride)
				{
					docOrganisation = factoryForWrapper.GetCachedValue
						(
							jobDocAddress.PK.ToStringKey(),
							delegate
							{
								return new DocOrganisation(OrgAddressSource.New(jobDocAddress, factoryForWrapper), factoryForWrapper);
							},
							CacheStalenessPolicy.StaleOnFactorySave
						);
					docOrganisation.SelectedAddress = DocDocAddress.New(jobDocAddress, factoryForWrapper);
				}
			}
			return docOrganisation;
		}

		public override string ToString()
		{
			return Name;
		}

		protected IOrganisationDetails Source
		{
			get { return (IOrganisationDetails)WrappedObject; }
		}

		public virtual OrgHeader OrgHeader
		{
			get { return ((DocBaseWrapper)WrappedObject).WrappedObject as OrgHeader; }
		}

		protected internal JobDocAddress JobDocAddress
		{
			get { return ((DocBaseWrapper)WrappedObject).WrappedObject as JobDocAddress; }
		}

		public ZString ECRCode
		{
			get
			{
				OrgCusCode eCRCode = OrgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				return eCRCode != null ? eCRCode.OK_CustomsRegNo : ZString.Empty;
			}
		}

		public DocOrganisation EXFreightBillTo
		{
			get { return DocOrganisation.New(OrgHeader.PickupFreightBillTo, Factory); }
		}

		public DocOrganisation IMFreightBillTo
		{
			get { return DocOrganisation.New(OrgHeader.DeliveryFreightBillTo, Factory); }
		}

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			return ((DocBaseWrapper)WrappedObject)?.WrappedObject as BusinessObject ?? base.GetParentBOForNoteStorageEDocsAndDocData();
		}

		#region Part Attributes

		public ZString PartAttrib1Name
		{
			get { return Source.PartAttrib1Name; }
		}

		public ZString PartAttrib2Name
		{
			get { return Source.PartAttrib2Name; }
		}

		public ZString PartAttrib3Name
		{
			get { return Source.PartAttrib3Name; }
		}

		public ZBool HasPartAttrib1
		{
			get { return Source.HasPartAttrib1; }
		}

		public ZBool HasPartAttrib2
		{
			get { return Source.HasPartAttrib2; }
		}

		public ZBool HasPartAttrib3
		{
			get { return Source.HasPartAttrib3; }
		}

		public ZBool HasSerialNumber
		{
			get { return Source.HasSerialNumber; }
		}

		#endregion

		#region Contacts

		public DocContactsCollection Contacts
		{
			get { return Source.Contacts; }
		}

		public DocContacts SalesContact
		{
			get { return Source.SalesContact; }
		}

		public DocContacts ExportFowardingContact
		{
			get { return Source.ExportFowardingContact; }
		}

		public DocContacts ImportFowardingContact
		{
			get { return Source.ImportFowardingContact; }
		}

		public DocContacts DefaultContact(ContactType defaultContactType)
		{
			return Source.DefaultContact(defaultContactType);
		}

		#endregion

		#region Addresses

		public DocAddressCollection Addresses
		{
			get { return Source.Addresses; }
		}

		public DocDocAddressCollection DocAddresses
		{
			get { return Source.DocAddresses; }
		}

		public DocAddress MainAddress
		{
			get { return Source.MainAddress; }
		}

		public DocAddress OrganisationPADPostalAddress
		{
			get { return Source.OrganisationPADPostalAddress; }
		}

		public DocDocAddress SelectedAddress
		{
			get { return fSelectedAddress; }
			private set { fSelectedAddress = value; }
		}
		DocDocAddress fSelectedAddress;

		#endregion

		#region Cus Code Collection
		public DocCusCodeCollection CustomCodes
		{
			get { return Source.CustomCodes; }
		}
		#endregion

		#region InvoiceOrders

		internal AccClientInvoiceOrderCollection InvoiceOrders
		{
			get { return OrgHeader.InvoiceOrders; }
		}

		#endregion

		public DocAppointedAgentPortsCollection AppAgPorts
		{
			get { return Source.AppointedAgentPorts; }
		}

		#region Basic Organisation Details
		public ZString OrgType
		{
			get { return Source.OrgType; }
		}

		public ZString Context
		{
			get { return "ORGANISATION"; }
		}

		public ZString[] AllNotes
		{
			get { return Source.AllNotes; }
		}

		public ZString ARCreditManagementNote
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.AccountsReceivableCreditManagementNote.Description, OrgHeader); }
		}

		public DocOrgStaffAssignmentsCollection StaffAssignments
		{
			get { return Source.StaffAssignments; }
		}

		public ZDateTime CurrentDate
		{
			get { return Source.CurrentDate; }
		}

		public ZString AdditionalAddressInformation
		{
			get { return Source.AdditionalAddressInformation; }
		}

		public ZString Address1
		{
			get { return Source.Address1; }
		}

		public ZString Address2
		{
			get { return Source.Address2; }
		}

		public ZString City
		{
			get { return Source.City; }
		}

		public ZString Code
		{
			get { return Source.Code; }
		}

		public ZString Email
		{
			get { return Source.Email; }
		}

		public ZString Fax
		{
			get { return Source.Fax; }
		}

		public ZString BusinessRegNo
		{
			get { return Source.BusinessRegNo; }
		}

		public ZString BusinessRegType
		{
			get { return Source.BusinessRegType; }
		}

		public ZString Name
		{
			get { return Source.Name; }
		}

		public DocBranch Branch
		{
			get { return Source.Branch; }
		}

		public ZBool IsActive
		{
			get { return Source.IsActive; }
		}

		public ZBool IsAirCTO
		{
			get { return Source.IsAirCTO; }
		}

		public ZBool IsAirLine
		{
			get { return Source.IsAirLine; }
		}

		public ZBool IsAirWholesaler
		{
			get { return Source.IsAirWholesaler; }
		}

		public ZBool IsBroker
		{
			get { return Source.IsBroker; }
		}

		public ZBool IsCompetitor
		{
			get { return Source.IsCompetitor; }
		}

		public ZBool IsConsignee
		{
			get { return Source.IsConsignee; }
		}

		public ZBool IsConsignor
		{
			get { return Source.IsConsignor; }
		}

		public ZBool IsContainerPark
		{
			get { return Source.IsContainerPark; }
		}

		public ZBool IsCreditor
		{
			get { return Source.IsCreditor; }
		}

		public ZBool IsDebtor
		{
			get { return Source.IsDebtor; }
		}

		public ZBool IsForwarder
		{
			get { return Source.IsForwarder; }
		}

		public ZBool IsInlandWaterwayProvider
		{
			get { return Source.IsInlandWaterwayProvider; }
		}

		public ZBool IsLineHaulProvider
		{
			get { return Source.IsLineHaulProvider; }
		}

		public ZBool IsLocalTransport
		{
			get { return Source.IsLocalTransport; }
		}

		public ZBool IsMiscFreightServices
		{
			get { return Source.IsMiscFreightServices; }
		}

		public ZBool IsPackDepot
		{
			get { return Source.IsPackDepot; }
		}

		public ZBool IsRailProvider
		{
			get { return Source.IsRailProvider; }
		}

		public ZBool IsSalesLead
		{
			get { return Source.IsSalesLead; }
		}

		public ZBool IsSeaCTO
		{
			get { return Source.IsSeaCTO; }
		}

		public ZBool IsSeaWholesaler
		{
			get { return Source.IsSeaWholesaler; }
		}

		public ZBool IsShippingLine
		{
			get { return Source.IsShippingLine; }
		}

		public ZBool IsShippingProvider
		{
			get { return Source.IsShippingProvider; }
		}

		public ZBool IsTempAccount
		{
			get { return Source.IsTempAccount; }
		}

		public ZBool IsTransportClient
		{
			get { return Source.IsTransportClient; }
		}

		public ZBool IsUnpackDepot
		{
			get { return Source.IsUnpackDepot; }
		}

		public ZBool IsWarehouseClient
		{
			get { return Source.IsWarehouseClient; }
		}

		public ZBool IsUserFlag1
		{
			get { return Source.IsUserFlag1; }
		}

		public ZBool IsUserFlag2
		{
			get { return Source.IsUserFlag2; }
		}

		public ZBool IsUserFlag3
		{
			get { return Source.IsUserFlag3; }
		}

		public ZBool IsUserFlag4
		{
			get { return Source.IsUserFlag4; }
		}

		public ZBool IsUserFlag5
		{
			get { return Source.IsUserFlag5; }
		}

		public ZBool IsUserFlag6
		{
			get { return Source.IsUserFlag6; }
		}

		public ZBool IsUserFlag7
		{
			get { return Source.IsUserFlag7; }
		}

		public ZBool IsUserFlag8
		{
			get { return Source.IsUserFlag8; }
		}

		public ZBool IsUserFlag9
		{
			get { return Source.IsUserFlag9; }
		}

		public ZBool IsUserFlag10
		{
			get { return Source.IsUserFlag10; }
		}

		public ZBool IsUserFlag11
		{
			get { return Source.IsUserFlag11; }
		}

		public ZBool IsUserFlag12
		{
			get { return Source.IsUserFlag12; }
		}

		public ZBool IsUserFlag13
		{
			get { return Source.IsUserFlag13; }
		}

		public ZBool IsUserFlag14
		{
			get { return Source.IsUserFlag14; }
		}

		public ZBool IsUserFlag15
		{
			get { return Source.IsUserFlag15; }
		}

		public ZBool IsUserFlag16
		{
			get { return Source.IsUserFlag16; }
		}

		public ZBool IsUserFlag17
		{
			get { return Source.IsUserFlag17; }
		}

		public ZBool IsUserFlag18
		{
			get { return Source.IsUserFlag18; }
		}

		public ZBool IsUserFlag19
		{
			get { return Source.IsUserFlag19; }
		}

		public ZBool IsUserFlag20
		{
			get { return Source.IsUserFlag20; }
		}

		public ZBool IsUserFlag21
		{
			get { return Source.IsUserFlag21; }
		}

		public ZBool IsUserFlag22
		{
			get { return Source.IsUserFlag22; }
		}

		public ZBool IsUserFlag23
		{
			get { return Source.IsUserFlag23; }
		}

		public ZBool IsUserFlag24
		{
			get { return Source.IsUserFlag24; }
		}

		public ZBool IsUserFlag25
		{
			get { return Source.IsUserFlag25; }
		}

		public ZBool IsUserFlag26
		{
			get { return Source.IsUserFlag26; }
		}

		public ZBool IsUserFlag27
		{
			get { return Source.IsUserFlag27; }
		}

		public ZBool IsUserFlag28
		{
			get { return Source.IsUserFlag28; }
		}

		public ZBool IsUserFlag29
		{
			get { return Source.IsUserFlag29; }
		}

		public ZBool IsUserFlag30
		{
			get { return Source.IsUserFlag30; }
		}

		public ZBool IsUserFlag31
		{
			get { return Source.IsUserFlag31; }
		}

		public ZBool IsUserFlag32
		{
			get { return Source.IsUserFlag32; }
		}

		public ZString Mobile
		{
			get { return Source.Mobile; }
		}

		public ZString Phone
		{
			get { return Source.Phone; }
		}

		public ZString PostCode
		{
			get { return Source.PostCode; }
		}

		public DocUNLOCO Loco
		{
			get { return Source.Loco; }
		}

		public ZString State
		{
			get { return Source.State; }
		}

		public ZString Web
		{
			get { return Source.Web; }
		}

		public ZString HandlingInstructions
		{
			get { return Source.HandlingInstructions; }
		}

		public ZString GetHandlingInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return Source.GetHandlingInstructionsByTransportOrContainerMode(transportMode, containerMode);
		}

		public ZString GetHandlingInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return Source.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(direction, transportMode, containerMode);
		}

		public ZString GetSpecialInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return Source.GetSpecialInstructionsByTransportOrContainerMode(transportMode, containerMode);
		}

		public ZString GetSpecialInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return Source.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(direction, transportMode, containerMode);
		}

		public ZString CartageInstructions
		{
			get { return Source.CartageInstructions; }
		}

		public ZString GetCartageInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return Source.GetCartageInstructionsByTransportOrContainerMode(transportMode, containerMode);
		}

		public ZString GetCartageInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return Source.GetCartageInstructionsByDirectionAndTransportOrContainerMode(direction, transportMode, containerMode);
		}

		public ZString LocalCustomsClientCode
		{
			get { return Source.LocalCustomsClientCode; }
		}

		public ZString LocalCustomsCarrierCode
		{
			get { return Source.LocalCustomsCarrierCode; }
		}

		public ZString LocalBusinessRegNo
		{
			get { return Source.LocalBusinessRegNo; }
		}

		public ZString LocalCustomsSupplierCode
		{
			get { return Source.LocalCustomsSupplierCode; }
		}

		public ZString LocalRebateUserCode
		{
			get { return Source.LocalRebateUserCode; }
		}

		public ZString LocalVATCode
		{
			get { return Source.LocalVATCode; }
		}

		public ZString EIN
		{
			get
			{
				return CustomCodes
						.GetCustomsRegNoForCodeAndCountry(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates))
						.Substring(0, 10);
			}
		}

		public ZString SSN
		{
			get { return CustomCodes.GetCustomsRegNoForCodeAndCountry(OrgCusCode.USACodeTypes.SocialSecurityNumber, RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedStates)); }
		}

		public ZString SSNorEIN
		{
			get { return SSN.IsEmpty ? EIN : SSN; }
		}

		public DocCompetitorsByTypeCollection CompetitorsByTypes
		{
			get
			{
				if (competitorsByTypes == null)
				{
					competitorsByTypes = new DocCompetitorsByTypeCollection(OrgHeader);
				}
				return competitorsByTypes;
			}
		}
		DocCompetitorsByTypeCollection competitorsByTypes;

		#endregion

		public DocOrgCompanyData CompanyData
		{
			get
			{
				if (fDocOrgCompanyData == null)
				{
					fDocOrgCompanyData = DocOrgCompanyData.New(OrgHeader.CompanyData, Factory);
				}

				return fDocOrgCompanyData;
			}
		}
		DocOrgCompanyData fDocOrgCompanyData;

		#region Custom Labels

		public ZString CustomAttrib1
		{
			get { return Source.CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return Source.CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return Source.CustomAttrib3; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return Source.CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return Source.CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return Source.CustomDecimal3; }
		}

		public ZDateTime CustomDate1
		{
			get { return Source.CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return Source.CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return Source.CustomDate3; }
		}

		public ZBool CustomFlag1
		{
			get { return Source.CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return Source.CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return Source.CustomFlag3; }
		}

		#endregion

		#region Custom Fields

		public ZString DGContactName
		{
			get
			{
				OrgContact contact = Factory.Load<OrgContact>(OrgHeader.MiscServ.OM_OC_EXDefaultDGContact);
				return (contact == null ? ZString.Empty : contact.OC_ContactName);
			}
		}

		public ZString DGContactPhone
		{
			get
			{
				return OrgHeader.MiscServ.DGPhoneNumber_Formatted;
			}
		}

		public ZString ApprovedExporterCode
		{
			get { return Source.ApprovedExporterCode; }
		}

		public ZString CarrierMasterBillPrefix
		{
			get { return Source.CarrierMasterBillPrefix; }
		}

		public ZString CarrierAirlinePrefix
		{
			get { return Source.CarrierAirlinePrefix; }
		}

		public ZString PostalAddress
		{
			get { return Source.PostalAddress; }
		}

		public ZString PostalAddressInEnglish
		{
			get { return Source.PostalAddressInEnglish; }
		}

		public ZString PostalAddress1
		{
			get { return Source.PostalAddressMain.Address1; }
		}

		public ZString PostalAddress2
		{
			get { return Source.PostalAddressMain.Address2; }
		}

		public ZString PostalAddressCity
		{
			get { return Source.PostalAddressMain.City; }
		}

		public ZString PostalAddressPostCode
		{
			get { return Source.PostalAddressMain.PostCode; }
		}

		public ZString PostalAddressExcludeCountryIfSame
		{
			get { return Source.PostalAddressExcludeCountryIfSame; }
		}

		public ZString PostalAddressExcludeName
		{
			get { return Source.PostalAddressExcludeName; }
		}

		public DocCountry Country
		{
			get { return Source.Country; }
		}

		public DocMiscServ MiscServ
		{
			get { return Source.MiscServ; }
		}

		public DocCountryData CountryData
		{
			get { return Source.CountryData; }
		}

		public ZString SCAC
		{
			get { return Source.SCAC; }
		}

		public ZString ABN
		{
			get { return Source.ABN; }
		}

		public ZString GST
		{
			get { return Source.GST; }
		}

		public ZString CCC
		{
			get { return Source.CCC; }
		}

		public ZString CBR
		{
			get { return Source.CBR; }
		}

		public ZString CSC
		{
			get { return Source.CSC; }
		}

		public ZString CCD
		{
			get { return Source.CCD; }
		}

		public ZString CID
		{
			get { return Source.CID; }
		}

		public ZString LSC
		{
			get { return Source.LSC; }
		}

		public ZString PAN
		{
			get { return Source.PAN; }
		}

		public ZString Kennitala
		{
			get { return Source.Kennitala; }
		}

		public ZString DisbursmentTerms
		{
			get { return Source.DisbursmentTerms; }
		}

		public ZString ShortDisbursementTerms
		{
			get { return Source.ShortDisbursementTerms; }
		}

		public ZString ShortInvoiceTerms
		{
			get { return Source.ShortInvoiceTerms; }
		}
		#endregion

		#region Address Rules

		public DocAddress DeliverAddress
		{
			get { return Source.DeliverAddress; }
		}

		public DocDocAddress DeliverDocAddress
		{
			get { return Source.DeliverDocAddress; }
		}

		public DocAddress PickUpAddress
		{
			get { return Source.PickUpAddress; }
		}

		public DocDocAddress PickUpDocAddress
		{
			get { return Source.PickUpDocAddress; }
		}

		public DocAddress ARAddress
		{
			get { return Source.ARAddress; }
		}

		public DocContacts ARDefaultContact
		{
			get { return Source.ARContact; }
		}

		public DocContacts ARDefaultContactForDocument
		{
			get
			{
				Guid menuItemPK = !DocumentMenuItemPK.IsEmpty ? DocumentMenuItemPK.ToGuid() : Guid.Empty;
				return Source.DefaultContact(menuItemPK, ContactType.Receivables);
			}
		}

		#endregion

		#region Spliting of Name

		public ZString SplitFullName1
		{
			get { return Source.SplitFullName1; }
		}

		public ZString SplitFullName2
		{
			get { return Source.SplitFullName2; }
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return Source.Code; }
		}

		#endregion

		#region IBODocDataProvider Members

		protected override BusinessObject BusinessObjectToLogAgainst
		{
			get { return OrgHeader; }
		}

		#endregion
	}
}
