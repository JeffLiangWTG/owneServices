using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Attributes;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Organisations;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("CompanyNameAndAddress")]
	public class OrganisationWrapper : GenericWrapperWithNotes
	{
		public OrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, ContactType mainContactType, ZString transportMode, BusinessObjectFactory factory)
			: this(usageType, organisation?.MainAddress ?? factory.GetNull<OrgAddress>(), mainContactType, transportMode, factory)
		{
			if (organisation != null)
			{
				RefreshMainAddressWrapperWhenLocalizationChanged = () => new AddressWrapper(organisation.MainAddress, mainContactType, factory);
			}
		}

		public OrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, ContactType mainContactType, BusinessObjectFactory factory)
			: this(usageType, organisation, mainContactType, transportMode: ZString.Empty, factory)
		{
		}

		public OrganisationWrapper(OrganisationUsageType usageType, OrgHeader organisation, OrgContact contact, BusinessObjectFactory factory)
			: base(organisation ?? factory.GetNull<OrgHeader>(), factory)
		{
			this.usageType = usageType;
			OrganisationBO = organisation ?? factory.GetNull<OrgHeader>();
			WrappedAddress = new AddressWrapper(OrganisationBO.MainAddress, ContactType.NoContactType, factory);
			RefreshMainAddressWrapperWhenLocalizationChanged = () => new AddressWrapper(OrganisationBO.MainAddress, ContactType.NoContactType, factory);

			WrappedMainContact = new ContactWrapper(contact, factory);
			wrappedMainContactIsEmpty = false;
		}

		public OrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType mainContactType, BusinessObjectFactory factory)
			: this(usageType, address, mainContactType, ZString.Empty, factory)
		{
		}

		public OrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, ContactType mainContactType, ZString transportMode, BusinessObjectFactory factory)
			: base(address != null ? address.Header : factory.GetNull<OrgHeader>(), factory)
		{
			this.usageType = usageType;
			OrganisationBO = address != null ? address.Header : factory.GetNull<OrgHeader>();
			WrappedAddress = new AddressWrapper(address, mainContactType, factory);
			WrappedMainContact = new ContactWrapper(new DefaultContactFinder(OrganisationBO, true, factory).DefaultContact(mainContactType, transportMode), address, factory);
			wrappedMainContactIsEmpty = false;
		}

		public OrganisationWrapper(OrganisationUsageType usageType, OrgAddress address, OrgContact contact, BusinessObjectFactory factory)
			: base(address != null ? address.Header : factory.GetNull<OrgHeader>(), factory)
		{
			this.usageType = usageType;
			OrganisationBO = address != null ? address.Header : factory.GetNull<OrgHeader>();
			WrappedAddress = new AddressWrapper(address, ContactType.NoContactType, factory);
			WrappedMainContact = new ContactWrapper(contact, factory);
			wrappedMainContactIsEmpty = false;
		}

		public OrganisationWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, BusinessObjectFactory factory)
			: base(docAddress, factory)
		{
			this.usageType = usageType;
			docAddress = docAddress ?? Factory.GetNull<JobDocAddress>();
			OrganisationBO = docAddress.Organisation ?? Factory.GetNull<OrgHeader>();
			WrappedAddress = new AddressWrapper(docAddress, factory);
			WrappedMainContact = new ContactWrapper(docAddress, factory);
			wrappedMainContactIsEmpty = docAddress == null;
		}

		public OrganisationWrapper(OrganisationUsageType usageType, JobDocAddress docAddress, ZString country, MultilingualString countryName, ZString countryForFormatter, BusinessObjectFactory factory)
	: base(docAddress, factory)
		{
			this.usageType = usageType;
			docAddress = docAddress ?? Factory.GetNull<JobDocAddress>();
			OrganisationBO = docAddress.Organisation ?? Factory.GetNull<OrgHeader>();
			WrappedAddress = new AddressWrapper(docAddress, country, countryName, countryForFormatter, factory);
			WrappedMainContact = new ContactWrapper(docAddress, factory);
			wrappedMainContactIsEmpty = docAddress == null;
		}

		public OrganisationWrapper(OrganisationUsageType usageType, ZString companyNameAndAddress, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.usageType = usageType;
			OrganisationBO = Factory.GetNull<OrgHeader>();
			WrappedAddress = new AddressWrapper(companyNameAndAddress, factory);
			WrappedMainContact = new ContactWrapper(Factory.GetNull<JobDocAddress>(), factory);
			wrappedMainContactIsEmpty = true;
		}

		internal OrganisationWrapper(OrganisationUsageType usageType, OrganisationWrapper wrapperBeingCloned)
			: base(wrapperBeingCloned.WrappedBO, wrapperBeingCloned.Factory)
		{
			this.usageType = usageType;
			OrganisationBO = wrapperBeingCloned.OrganisationBO;
			WrappedAddress = wrapperBeingCloned.WrappedAddress;
			WrappedMainContact = wrapperBeingCloned.WrappedMainContact;
			wrappedMainContactIsEmpty = wrapperBeingCloned.wrappedMainContactIsEmpty;
			RefreshMainAddressWrapperWhenLocalizationChanged = wrapperBeingCloned.RefreshMainAddressWrapperWhenLocalizationChanged;
		}

		readonly OrganisationUsageType usageType;
		readonly protected OrgHeader OrganisationBO;
		readonly Func<AddressWrapper> RefreshMainAddressWrapperWhenLocalizationChanged;
#if DEBUG
		internal
#endif
		readonly ContactWrapper WrappedMainContact;
		readonly ZBool wrappedMainContactIsEmpty;

		internal ZString ContactNameText { get; set; }

		public OrgHeader Organisation
		{
			get { return OrganisationBO; }
		}

		public ZString RANumber
		{
			get { return raNumber ?? (raNumber = GetRANumber()); }
		}
		string raNumber;

		ZString GetRANumber()
		{
			ZString loginCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ZString result = string.Join(", ", OrganisationBO.CustomsCodes.Cast<OrgCusCode>()
				.Where(c => c.OK_CodeType == OrgCusCode.CodeTypes.RegulatedAgentID && c.OK_RN_NKCodeCountry == loginCountry)
				.Select(c => c.OK_CustomsRegNo));

			if (result.IsEmpty)
			{
				if (loginCountry == Constants.CountryCodes.Australia)
				{
					result = new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(GlbStaff.CurrentUser, CertificateTypePairList.Codes.DT1);
				}
				else if (loginCountry == Constants.CountryCodes.UnitedKingdom)
				{
					result = OrganisationBO.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedKingdomCodeTypes.AirCargoAgentsListedNumber);
				}
			}

			return result;
		}

		#region Road Carrier Registration Number

		public ZString RoadCarrierRegistrationHeading
		{
			get { return ResString.GetMultilingualString("B57B2B51-3FF3-41C6-ACE1-3726B3FAB199", "Road Carrier Registration"); }
		}

		public ZString RoadCarrierRegistration
		{
			get { return OrganisationBO.RoadCarrierRegistrationNumber; }
		}

		#endregion

		protected AddressWrapper WrappedAddress
		{
			get
			{
				if (wrappedAddress.WrappedLanguage != Res.CurrentLanguage)
				{
					wrappedAddress = RefreshMainAddressWrapperWhenLocalizationChanged?.Invoke() ?? wrappedAddress;
				}

				return wrappedAddress;
			}

			private set { wrappedAddress = value; }
		}

		AddressWrapper wrappedAddress;

		public ZString SCAC
		{
			get { return OrganisationBO.SCACCode; }
		}

		public DGContactWrapper DGContact
		{
			get { return MiscServ.DGContact; }
		}

		public ZString CompanyCode
		{
			get { return OrganisationBO.OH_Code; }
		}

		public ZString CompanyName
		{
			get { return WrappedAddress.CompanyName; }
		}

		public ZString CompanyAddress
		{
			[MultiLineAddress]
			get { return WrappedAddress.Address; }
		}

		public ZString CompanyNameAndAddress
		{
			[MultiLineAddress]
			get { return WrappedAddress.CompanyNameAndAddress; }
		}

		public ZString FirstLineOfCompanyNameAndAddress
		{
			get
			{
				var lines = CompanyNameAndAddress.Trim().Split('\r', '\n');
				return lines.First();
			}
		}

		public CountryWrapper Country
		{
			get { return WrappedAddress.Country; }
		}

		public AddressWrapper MainAddress
		{
			get { return WrappedAddress; }
		}

		public ZString MainPhone
		{
			get { return WrappedAddress.Phone; }
		}

		public ZString MainFax
		{
			get { return WrappedAddress.Fax; }
		}

		public ZString MainEmail
		{
			get { return WrappedAddress.Email; }
		}

		public ZString ContactName
		{
			get { return !ContactNameText.IsEmpty ? ContactNameText : WrappedMainContact.FullName; }
		}

		public ZString ContactPhone
		{
			get { return GetBestValueWithFallback(WrappedMainContact.Phone, WrappedAddress.Phone); }
		}

		public ZString ContactFax
		{
			get { return GetBestValueWithFallback(WrappedMainContact.Fax, WrappedAddress.Fax); }
		}

		public ZString ContactEmail
		{
			get { return GetBestValueWithFallback(WrappedMainContact.Email, WrappedAddress.Email); }
		}

		public ZString LocalVATCode
		{
			get { return OrganisationBO.LocalVATCode; }
		}

		public ZString LocalBusinessRegNo
		{
			get { return OrganisationBO.LocalBusinessRegNo; }
		}

		public ZString TaxCode
		{
			get
			{
				var country = OrganisationBO.Country;
				return country != null && IsValidOrganisationForTaxCode(OrganisationBO) ? country.ConsumptionTaxDescription : string.Empty;
			}
		}

		bool IsValidOrganisationForTaxCode(OrgHeader orgHeader)
		{
			if (orgHeader == null || orgHeader.CompanyData == null)
			{
				return false;
			}

			return orgHeader.OH_IsDebtor && orgHeader.CompanyData.OB_ARVATConfig != AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
		}

		public LocationWrapper ClosestPort
		{
			get { return WrappedAddress.Location; }
		}

		public AddressWrapperCollection Addresses
		{
			get { return fAddresses ?? (fAddresses = new AddressWrapperCollection(OrganisationBO, Factory)); }
		}
		AddressWrapperCollection fAddresses;

		public ContactWrapperCollection Contacts
		{
			get { return fContacts ?? (fContacts = new ContactWrapperCollection(OrganisationBO, Factory)); }
		}
		ContactWrapperCollection fContacts;

		public ZString TypeDescription
		{
			get
			{
				switch (usageType)
				{
					case OrganisationUsageType.Airline:
						return CommonResourceStrings.Airline;
					case OrganisationUsageType.ArrivalCTO:
						return CommonResourceStrings.ArrivalCTO;
					case OrganisationUsageType.AssuredParty:
						return CommonResourceStrings.AssuredParty;
					case OrganisationUsageType.BillToParty:
						return Res.GetString("e0093203-c298-49f8-bee9-c726703861db", "Bill To Party");
					case OrganisationUsageType.BookingParty:
						return CommonResourceStrings.BookingParty;
					case OrganisationUsageType.Buyer:
						return CommonResourceStrings.Buyer;
					case OrganisationUsageType.Debtor:
						return ZString.Empty;
					case OrganisationUsageType.DeliveryAgent:
						return CommonResourceStrings.DeliveryAgent;
					case OrganisationUsageType.Carrier:
						return CommonResourceStrings.Carrier;
					case OrganisationUsageType.ClaimsPayableBy:
						return CommonResourceStrings.ClaimsPayableBy;
					case OrganisationUsageType.Client:
						return Res.GetString("b634877e-c94f-40ac-852f-e5ffc2e5ad66", "Client");
					case OrganisationUsageType.Consignee:
						return CommonResourceStrings.Consignee;
					case OrganisationUsageType.Consignor:
						return FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
					case OrganisationUsageType.ControllingAgent:
						return Res.GetString("46d3ef7c-abea-4aa1-8939-aa5f2d65e61e", "Controlling Agent");
					case OrganisationUsageType.ControllingCustomer:
						return Res.GetString("861bf3aa-25dc-4c31-8996-d374a593337f", "Controlling Customer");
					case OrganisationUsageType.ConsolCreditor:
						return CommonResourceStrings.ConsolCreditor;
					case OrganisationUsageType.Contractor:
						return Res.GetString("373e92cd-bdd5-4abc-83a7-8331b8df586c", "Contractor");
					case OrganisationUsageType.Creditor:
						return Res.GetString("729f622c-ee28-4de8-9462-d343df7bfff1", "Creditor");
					case OrganisationUsageType.CTOArrival:
						return Res.GetString("39482c93-604a-4b0f-b117-6e31a559f463", "CTO Arrival");
					case OrganisationUsageType.CTODeparture:
						return Res.GetString("7fe1e318-8e7f-4456-bb4b-4db82fdd3697", "CTO Departure");
					case OrganisationUsageType.Destination:
						return Res.GetString("3c31ac72-d2c2-4948-b36b-96583168554d", "Destination");
					case OrganisationUsageType.ExportAgent:
						return CommonResourceStrings.ExportAgent;
					case OrganisationUsageType.ExportBroker:
						return CommonResourceStrings.ExportBroker;
					case OrganisationUsageType.Forwarder:
						return Res.GetString("22122b01-a50b-4f4e-ad9b-4dacdcab2d5a", "Forwarder");
					case OrganisationUsageType.Importer:
						return Res.GetString("154cfd05-6b34-4338-a5a2-860307676918", "Importer");
					case OrganisationUsageType.ImportAgent:
						return CommonResourceStrings.ImportAgent;
					case OrganisationUsageType.ImportBroker:
						return CommonResourceStrings.ImportBroker;
					case OrganisationUsageType.InsuredBy:
						return CommonResourceStrings.InsuredBy;
					case OrganisationUsageType.LocalClient:
						return CommonResourceStrings.LocalClient;
					case OrganisationUsageType.LocalForwarder:
						return CommonResourceStrings.LocalForwarder;
					case OrganisationUsageType.NotifyParty:
						return CommonResourceStrings.NotifyParty;
					case OrganisationUsageType.Origin:
						return Res.GetString("2527fb3c-eec1-431e-81b8-333918e41e0b", "Origin");
					case OrganisationUsageType.PickupAgent:
						return CommonResourceStrings.PickupAgent;
					case OrganisationUsageType.Principal:
						return CommonResourceStrings.Principal;
					case OrganisationUsageType.PublishedAirFreightAgent:
						return Res.GetString("8239057b-1f79-4683-adff-96d8b1a918ac", "Published Air Freight Agent");
					case OrganisationUsageType.PublishedSeaFreightAgent:
						return Res.GetString("93139cf9-e64c-40fe-a239-5457ab441840", "Published Sea Freight Agent");
					case OrganisationUsageType.ReceivingForwarder:
						return Res.GetString("ad2b4376-016a-46bb-9225-bd76e9ffb026", "Receiving Forwarder");
					case OrganisationUsageType.SendingForwarder:
						return Res.GetString("4542260b-2dea-4d76-bc1b-01894cd9b8ca", "Sending Forwarder");
					case OrganisationUsageType.ServiceRequestedBy:
						return Res.GetString("7639be79-8854-4a25-9378-53649f9ba6d2", "Service Requested By");
					case OrganisationUsageType.ShippingLine:
						return CommonResourceStrings.ShippingLine;
					case OrganisationUsageType.SurveyReportParty:
						return CommonResourceStrings.SurveyReportParty;
					case OrganisationUsageType.Supplier:
						return Res.GetString("2d215137-d793-4e17-b424-f62ca3192110", "Supplier");
					case OrganisationUsageType.TransportCo:
						return Res.GetString("a57c9540-7f4c-45b2-840c-da49dc99faaf", "Transport Co");
					case OrganisationUsageType.TransportCompany:
						return Res.GetString("a74014a4-4c59-4801-88da-24cfedf7fc84", "Transport Company");
					case OrganisationUsageType.UltimateConsignee:
						return Res.GetString("5be75764-ba93-420e-bbb4-aa3dd954892a", "Ultimate Consignee");
					case OrganisationUsageType.UltimateConsignor:
						return Res.GetString("630ae7a2-2d0c-44de-bacc-e69ccc755ca4", "Ultimate {0}", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);
					case OrganisationUsageType.UnpackingLocation:
						return Res.GetString("3ee58e05-e12c-4a26-885b-629176463f96", "Unpacking Location");
					case OrganisationUsageType.PackingLocation:
						return Res.GetString("72c98b06-04d8-4da7-9699-63721b9050a1", "Packing Location");
					case OrganisationUsageType.ArrivalCFSTransport:
						return Res.GetString("ceef8c2d-6363-46d1-bf76-381544e3b0fd", "Arrival CFS Transport");
					case OrganisationUsageType.DepartureCFSTransport:
						return Res.GetString("11c26129-235b-4caf-8bb2-a3d7ac372a8b", "Departure CFS Transport");
					case OrganisationUsageType.CarrierBookingAgent:
						return Res.GetString("f5018209-401c-4325-aad0-3bac790252ee", "Carrier Booking Agent");
					case OrganisationUsageType.CarrierHandlingAgent:
						return Res.GetString("5ae2a030-43c7-4708-865b-6950b3708fe8", "Carrier Handling Agent");
#if DEBUG
					case OrganisationUsageType.Test:
						return "Test";
#endif

					default:
						return ZString.Empty;
				}
			}
		}

		#region Attribute Extensions

		public PartAttributeWrapper PartAttribute1
		{
			get { return MiscServ.PartAttribute1; }
		}

		public PartAttributeWrapper PartAttribute2
		{
			get { return MiscServ.PartAttribute2; }
		}

		public PartAttributeWrapper PartAttribute3
		{
			get { return MiscServ.PartAttribute3; }
		}

		#endregion

		#region ClientDocumentLogo

		public Image ClientDocumentLogo
		{
			get { return MiscServ.ClientDocumentLogo; }
		}

		#endregion

		internal MiscServWrapper MiscServ
		{
			get { return miscServ ?? (miscServ = new MiscServWrapper(OrganisationBO.MiscServ, Factory)); }
		}
		MiscServWrapper miscServ;

		public RegistrationNumberCodeWrapperCollection CustomsCodes
		{
			get { return fCustomsCodes ?? (fCustomsCodes = new RegistrationNumberCodeWrapperCollection(OrganisationBO, Factory)); }
		}
		RegistrationNumberCodeWrapperCollection fCustomsCodes;

		public AssignedStaffWrapperCollection AssignedStaff
		{
			get { return assignedStaff ?? (assignedStaff = new AssignedStaffWrapperCollection(OrganisationBO, Factory)); }
		}
		AssignedStaffWrapperCollection assignedStaff;

		public AviationSecurityWrapper AviationSecurity
		{
			get { return WrappedAddress.AviationSecurity; }
		}

		public eDocWrapperCollection EDocs
		{
			get { return eDocs ?? (eDocs = GetEDocs()); }
		}
		eDocWrapperCollection eDocs;

		eDocWrapperCollection GetEDocs()
		{
			return eDocWrapperCollection.New(GetParentBOForNoteStorageEDocsAndDocData() as IDocManagerSupport, Factory);
		}

		protected override BusinessObject GetParentBOForNoteStorageEDocsAndDocData()
		{
			return Organisation;
		}

		protected override BusinessObject BusinessObjectForCustomFields => Organisation;
	}
}
