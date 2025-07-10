using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebJobDocAddress : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public WebJobDocAddress(JobDocAddress jobDocAddress)
			: this(jobDocAddress, false)
		{
		}

		public WebJobDocAddress(JobDocAddress jobDocAddress, bool residentialAddressVisible)
			: base(jobDocAddress.Factory)
		{
			this.jobDocAddress = jobDocAddress;

			if (jobDocAddress.Organisation != null)
			{
				Organisation = jobDocAddress.Organisation;
			}

			if (jobDocAddress.Address != null)
			{
				Address = jobDocAddress.Address;
			}

			if (jobDocAddress.Contact != null)
			{
				Contact = jobDocAddress.Contact;
			}

			this.residentialAddressVisible = residentialAddressVisible;

			if (!jobDocAddress.E2_AddressOverride)
			{
				if (residentialAddressVisible && jobDocAddress.Address != null)
				{
					jobDocAddress.E2_IsResidential = jobDocAddress.Address.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Residential).Enabled;
				}
			}

			jobDocAddress.GetDefaultCountryCodeIfEmpty = () => string.Empty;
		}

		#endregion

		#region Schema

		public abstract class Schema
		{
			public const string OrganisationPK = "OrganisationPK";
			public const string AddressPK = "AddressPK";
			public const string ContactPK = "ContactPK";

			public const string CompanyName = "CompanyName";

			public const string Address1 = "Address1";
			public const string Address2 = "Address2";
			public const string PostCode = "PostCode";
			public const string City = "City";
			public const string State = "State";
			public const string CountryCode = "CountryCode";

			public const string IsResidentialAddress = "IsResidentialAddress";

			public const string AddressAsASingleLine = "AddressAsASingleLine";

			public const string ContactName = "ContactName";
			public const string Phone = "Phone";
			public const string Fax = "Fax";
			public const string Email = "Email";

			public const string SaveAsNew = "SaveAsNew";
			public const string NoOrganizationSelected = "NoOrganizationSelected";

			public const string GovRegNum = "GovRegNum";
			public const string GovRegNumType = "GovRegNumType";
			public const string SocialSecurityNumber = "SocialSecurityNumber";
			public const string SocialSecurityNumberDateOfBirth = "SocialSecurityNumberDateOfBirth";
		}

		#endregion

		#region Methods

		public ZString GetPortCode()
		{
			ZString result = ZString.Empty;

			ZGuid orgPK = FindOrgHeaderPK();
			if (orgPK.IsValid)
			{
				OrgHeader orgHeader = Factory.Load<OrgHeader>(orgPK);
				result = orgHeader.OH_RL_NKClosestPort;
			}

			if (result.IsEmpty && AddressPK.IsValid)
			{
				OrgAddress orgAddress = Factory.Load<OrgAddress>(AddressPK);
				result = orgAddress.OA_RL_NKRelatedPortCode;
			}

			ZString portCode = RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, CountryCode, City, State);
			if (result.IsEmpty && !portCode.EndsWith("ZZZ"))
			{
				result = portCode;
			}

			if (result.IsEmpty && OrganisationPK.IsValid)
			{
				OrgHeader orgHeader = Factory.Load<OrgHeader>(OrganisationPK);
				result = orgHeader.OH_RL_NKClosestPort;
			}

			return result;
		}

		#endregion

		#region Properties

		#region SaveAsNew

		bool InternalsHaveChangesOrDontExist
		{
			get
			{
				bool result = true;
				if (Organisation != null && Address != null && jobDocAddress.HasRealAddress)
				{
					if (!(Organisation.OH_FullNameInfo.HasChanges ||
						Address.OA_Address1Info.HasChanges ||
						Address.OA_Address2Info.HasChanges ||
						Address.OA_PostCodeInfo.HasChanges ||
						Address.OA_CityInfo.HasChanges ||
						Address.OA_StateInfo.HasChanges ||
						Address.OA_PhoneInfo.HasChanges ||
						Address.OA_FaxInfo.HasChanges ||
						Address.OA_EmailInfo.HasChanges ||
						!Organisation.IsInDatabase ||
						!Address.IsInDatabase))
					{
						result = false;
					}
					if (!ContactName.IsEmpty &&
						ContactName != jobDocAddress.DefaultContactType)
					{
						if (Contact == null)
						{ result = false; }
						else
						{
							result = result ||
							(Contact.OC_ContactNameInfo.HasChanges ||
							Contact.OC_PhoneInfo.HasChanges ||
							Contact.OC_EmailInfo.HasChanges ||
							Contact.OC_FaxInfo.HasChanges ||
							!Contact.IsInDatabase);
						}
					}
				}

				return result;
			}
		}

		void UpdateOverride()
		{
			UpdateOverride(SaveAsNew);
		}

		void UpdateOverride(bool save)
		{
			if (!UpdatingOverride && !UpdatingCurrentState)
			{
				UpdatingOverride = true;
				if ((SaveAsNew == save && !SaveAsNew)
					|| (SaveAsNew != save && !save))
				{
					if (InternalsHaveChangesOrDontExist)
					{
						if (!jobDocAddress.E2_AddressOverride)
						{
							jobDocAddress.E2_AddressOverride = true;
							SetAddressProperties(Address);
							SetContactProperties(Contact);
						}
					}
					else
					{
						jobDocAddress.E2_AddressOverride = false;
					}
				}
				UpdatingOverride = false;
			}
		}

		public ZBool SaveAsNew
		{
			get
			{
				return saveAsNew;
			}
			set
			{
				if (saveAsNew && !value)
				{
					UpdateOverride(value);
				}
				saveAsNew = value;
				UpdateInternalsForSaving();
				if (value)
				{
					OrganisationPK = Organisation.PK;
					AddressPK = Address.PK;
					CompanyName = Address.OA_CompanyNameOverride.IsEmpty ? Organisation.OH_FullNameTruncated : Address.OA_CompanyNameOverrideTruncated;
					SetContactProperties(Contact);
				}

				SaveAsNewInfo.RefreshBinding();
			}
		}
		ZBool saveAsNew;

		public ZPropertyInfo SaveAsNewInfo
		{
			get { return GetZPropertyInfo(Schema.SaveAsNew); }
		}

		void ValidateSaveAsNew()
		{
			SaveAsNewInfo.ClearAllNotifications();
			if (SaveAsNew)
			{
				if (Address1.IsEmpty)
				{
					var errorMessage = Res.GetString("655F3BA3-6580-474E-9EA8-37B356B18C8D", "Organization {0} must have an address.  Please input an address on line 1 or uncheck the 'Save' checkbox.", CompanyName);
					SaveAsNewInfo.AddError(errorMessage);
				}
				if (!CanCreateNewOrgHeader)
				{
					var errorMessage = Res.GetString("26dce1f0-9ddb-43d2-b875-706df342a644", "Unable to locate closest port for organization {0}.  Please check City, State and Country/Region or uncheck the 'Save' checkbox.", CompanyName);
					SaveAsNewInfo.AddError(errorMessage);
				}
			}
		}

		#endregion

		#region ResidentialAddressVisible

		public bool ResidentialAddressVisible
		{
			get { return residentialAddressVisible; }
		}
		readonly bool residentialAddressVisible;

		#endregion

		#region JobDocAddressWillBeOverriden

		public bool JobDocAddressWillBeOverriden
		{
			get { return !OrgHeaderExists && (!SaveAsNew || !CanCreateNewOrgHeader); }
		}

		#endregion

		#region Inactive

		public bool Inactive
		{
			get { return inactive; }
			set { inactive = value; }
		}
		bool inactive;

		#endregion

		#region IsOptional

		public bool IsOptional
		{
			get { return isOptional; }
			set { isOptional = value; }
		}
		bool isOptional;

		#endregion

		#region AddressTypeDescription

		public ZString AddressTypeDescription
		{
			get
			{
				ZString result = ZString.Empty;
				if (Factory != null && jobDocAddress != null)
				{
					result = DocAddressTypes.GetDescription(Factory, jobDocAddress.DocAddressType);
				}
				return result;
			}
		}

		#endregion

		AddressFormatter Formatter
		{
			get
			{
				if (fFormatter == null && jobDocAddress != null)
				{
					fFormatter = new AddressFormatter(Factory, jobDocAddress, GlbCompany.CurrentCompany, true);
				}
				return fFormatter;
			}
		}
		AddressFormatter fFormatter;

		public ZString FormattedAddressSummary
		{
			get
			{
				ZString result = ZString.Empty;
				if (Formatter != null)
				{
					result = Formatter.PostalAddress().Replace("\n", System.Environment.NewLine);
				}
				if (result.IsEmpty)
				{ result = Res.GetString("05348d4c-e638-4f8a-9d5f-8c72f2059cfc", "No Address Selected"); }
				return result;
			}
		}

		#endregion

		#region Internals

		#region Organisation

		#region PK

		public ZGuid OrganisationPK
		{
			get
			{
				return jobDocAddress.OrganisationPK;
			}
			set
			{
#if DEBUG
				if (!Globals.IsTest)
#endif
				{
					jobDocAddress.ClearAllNotifications();
				}
				OrganisationPKInfo.ClearAllNotifications();
				AddressPKInfo.ClearAllNotifications();
				if (!(SaveAsNew && value.IsEmpty))
				{
					jobDocAddress.OrganisationPK = value;
					if (Organisation != null && Organisation.PK != value && !Organisation.IsInDatabase)
					{
						Organisation.Delete();
						Organisation = null;
					}
					if (Organisation == null)
					{
						Organisation = Factory.Load<OrgHeader>(value);
						if (Organisation != null)
						{
							UpdatingCurrentState = true;
							CompanyName = Organisation.OH_FullNameTruncated;
							UpdatingCurrentState = false;
						}

						if (jobDocAddress.HasRealAddress)
						{
							Address = Factory.Load<OrgAddress>(AddressPK);
						}
						Contact = Factory.Load<OrgContact>(ContactPK);
						SetAddressProperties(Address);
						SetContactProperties(Contact);
					}

					OrganisationPKInfo.RefreshBinding();
					AddressPKInfo.RefreshBinding();
					ContactPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo OrganisationPKInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.OrganisationPK);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.OrganisationNameOrPKInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region OrgBizO

#if DEBUG
		public
#endif
		OrgHeader Organisation
		{
			get
			{
				return fOrganisation;
			}
			set { fOrganisation = value; }
		}
		OrgHeader fOrganisation;

		void UpdateInternalsForSaving()
		{
			if (saveAsNew)
			{
				CreateOrgHeader();
				CreateOrUpdateAddress();
				CreateOrUpdateContact();
				if (jobDocAddress.E2_AddressOverride)
				{
					jobDocAddress.E2_AddressOverride = false;
				}
			}
			else
			{
				if (jobDocAddress.E2_AddressOverride)
				{
					if (Organisation != null && !Organisation.IsInDatabase)
					{
						bool deleteOrganisation = true;
						if (Organisation.Addresses.Count > 1)
						{
							foreach (OrgAddress orgAddress in Organisation.Addresses)
							{
								if ((orgAddress != Address) && (orgAddress.AddressDescription != OrgAddress.AddressNotOnFile))
								{
									deleteOrganisation = false;
									break;
								}
							}
						}

						if (deleteOrganisation)
						{
							Organisation.Delete();
						}
					}
					if (Address != null && !Address.IsInDatabase)
					{
						Address.Delete();
					}
					if (Contact != null && !Contact.IsInDatabase)
					{
						Contact.Delete();
					}
					Organisation = null;
					Address = null;
					Contact = null;
				}
			}
		}

		#region Business Objects Creation

		#region OrgHeader

		OrgHeader CreateOrgHeader()
		{
			Organisation = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, CompanyName));
			if (Organisation == null)
			{
				Organisation = Factory.New<OrgHeader>();
				Organisation.OH_FullName = CompanyName.Left(OrgHeader.Schema.OH_FullNameMaxLength);
				if (newOrgRelationType != NewOrgRelationTypes.Unknown)
				{
					CreateBuyerSupplierLink(Organisation.PK);
				}

				Organisation.OH_RL_NKClosestPort = PortCode;
				Organisation.OH_IsConsignee = isConsignee;
				Organisation.OH_IsConsignor = isConsignor;
			}
			return Organisation;
		}

		void CreateBuyerSupplierLink(ZGuid newOrgHeaderPK)
		{
			ZGuid currectUserOrgPK = ((OrgContact)WebEnv.CurrentUser).OC_OH;

			OrgSupplierBuyerLink link = Factory.New<OrgSupplierBuyerLink>();

			if (newOrgRelationType == NewOrgRelationTypes.Buyer)
			{
				link.OL_OH_Buyer = newOrgHeaderPK;
				link.OL_OH_Supplier = currectUserOrgPK;
			}

			if (newOrgRelationType == NewOrgRelationTypes.Supplier)
			{
				link.OL_OH_Buyer = currectUserOrgPK;
				link.OL_OH_Supplier = newOrgHeaderPK;
			}
		}

		#endregion

		#region OrgAddress

		OrgAddress CreateAddressIfNecessary()
		{
			if (Address == null)
			{
				if (Address == null)
				{
					Address = FindOrgAddress(Organisation.PK);
				}
				if (Address == null)
				{
					ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(OrgAddress));
					filter.AddToFilter(OrgAddressSchema.OA_OH, Organisation.PK);
					filter.AddToFilter(OrgAddressSchema.OA_Address1, Address1);
					Address = Factory.LoadTop1<OrgAddress>(filter);
				}
				if (Address == null)
				{
					Address = Organisation.MainAddress.OA_Address1.IsEmpty ? Organisation.MainAddress : Organisation.Addresses.AddNew();
				}
				if (!Address.IsInDatabase)
				{
					Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
				}
			}
			return Address;
		}

		OrgAddress CreateOrUpdateAddress()
		{
			CreateAddressIfNecessary();
			Address.OA_CompanyNameOverride = CompanyName.Left(OrgAddress.Schema.OA_CompanyNameOverrideMaxLength);
			Address.OA_Address1 = Address1;
			Address.OA_Address2 = Address2;
			Address.OA_City = City;
			Address.OA_PostCode = PostCode;
			Address.OA_State = State;
			Address.OA_RL_NKRelatedPortCode = PortCode;
			if (ContactName.IsEmpty && ContactName != (ZString)jobDocAddress.DefaultContactType.DefaultName)
			{
				Address.OA_Phone = Phone;
				Address.OA_Fax = Fax;
				Address.OA_Email = Email;
			}
			if (IsResidentialAddress)
			{
				Address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Residential);
			}

			return Address;
		}

		#endregion

		#region OrgContact

		void CreateContactIfNecessary(ZString contactName)
		{
			if (Contact == null && !contactName.IsEmpty && contactName != (ZString)jobDocAddress.DefaultContactType.DefaultName)
			{
				Contact = FindOrgContact(Organisation.PK, contactName);
				if (Contact == null)
				{
					Contact = Factory.New<OrgContact>();
					Contact.OC_ContactName = contactName;
					Contact.OC_OH = fOrganisation != null ? fOrganisation.PK : ZGuid.Empty;
				}
			}
		}

		OrgContact CreateOrUpdateContact(ZString contactName)
		{
			CreateContactIfNecessary(contactName);
			if (Contact != null)
			{
				Contact.OC_Phone = Phone;
				Contact.OC_Fax = Fax;
				Contact.OC_Email = Email;
				ContactPK = Contact.PK;
			}

			return Contact;
		}

		OrgContact CreateOrUpdateContact()
		{
			return CreateOrUpdateContact(ContactName);
		}

		#endregion

		#endregion

		#region FindExisting

		OrgAddress FindOrgAddress(ZGuid orgHeaderPK)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(OrgAddress));
			filter.AddToFilter(OrgAddressSchema.OA_OH, orgHeaderPK);
			filter.AddToFilter(OrgAddressSchema.OA_Address1, Address1);
			filter.AddToFilter(OrgAddressSchema.OA_Address2, Address2);
			return Factory.LoadTop1<OrgAddress>(filter);
		}

		OrgContact FindOrgContact(ZGuid orgHeaderPK, ZString contactName)
		{
			ZQuery filter = new ZQuery(OrgContactSchema.OC_OH, orgHeaderPK);
			filter.AddToFilter(OrgContactSchema.OC_ContactName, contactName);

			return Factory.LoadTop1<OrgContact>(filter);
		}

		#endregion

		#endregion

		#endregion

		#region Address

		#region AddressPK

		public ZGuid AddressPK
		{
			get
			{
				return jobDocAddress.E2_OA_Address;
			}
			set
			{
				AddressPKInfo.ClearAllNotifications();
				if (!(value.IsEmpty && SaveAsNew))
				{
					if (!SaveAsNew && value.IsEmpty && !jobDocAddress.E2_OA_Address.IsEmpty)
					{
						jobDocAddress.E2_AddressOverride = true;
					}
					if (!value.IsEmpty && jobDocAddress.HasRealAddress || !value.IsEmpty && jobDocAddress.E2_AddressOverride)
					{
						jobDocAddress.E2_OA_Address = value;
						if (jobDocAddress.HasRealOrganisation && (Organisation == null || Organisation.PK != OrganisationPK))
						{
							Organisation = Factory.Load<OrgHeader>(OrganisationPK);
						}
					}
					if (Address == null || (Address != null && Address.PK != value) || value.IsEmpty)
					{
						Address = Factory.Load<OrgAddress>(value);
					}
					if (Address != null)
					{
						SetAddressProperties(Address);
					}
					AddressPKInfo.ClearAllNotifications();
					UpdateOverride();
					AddressPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AddressPKInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.AddressPK);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_Address1Info.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region AddressBizO

#if DEBUG
		public
#endif
		OrgAddress Address
		{
			get
			{
				return fAddress;
			}
			set { fAddress = value; }
		}
		OrgAddress fAddress;

		#endregion AddressBizO

		#endregion

		#region Contact

		#region PK

		public ZGuid ContactPK
		{
			get { return jobDocAddress.ContactPK; }
			set
			{
				if (!(value.IsEmpty && SaveAsNew) || !SaveAsNew)
				{
					jobDocAddress.ContactPK = value;
					if (Contact == null || (Contact != null && Contact.PK != value))
					{
						Contact = Factory.Load<OrgContact>(value);
					}
					SetContactProperties(Contact);
					ContactPKInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ContactPKInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.ContactPK);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.ContactPKInfo.Notifications);
					result.AddRange(jobDocAddress.E2_ContactInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region ContactBizO

#if DEBUG
		public
#endif
		OrgContact Contact
		{
			get { return fContact; }
			set { fContact = value; }
		}
		OrgContact fContact;

		#endregion

		#endregion

		#endregion

		#region Wrapped JobDocAddress Properties

		[DefaultValue(false)]
		bool UpdatingCurrentState
		{ get; set; }

		[DefaultValue(false)]
		bool UpdatingOverride
		{ get; set; }

		void SetWrappedValue(ZPropertyInfo property, ZPropertyInfo wrappedProperty, IZType value, params ZPropertyInfo[] dependantProperties)
		{
			if (value == null)
			{
				return;
			}

			foreach (var dependantProperty in dependantProperties)
			{
				dependantProperty.ClearAllNotifications();
			}

			property.ClearAllNotifications();

			if (!wrappedProperty.Value.Equals(value) && !SaveAsNew)
			{
				if (!UpdatingCurrentState)
				{
					jobDocAddress.E2_AddressOverride = true;
					UpdateInternalsForSaving();
				}
				wrappedProperty.Value = value;
			}
			else if (SaveAsNew)
			{
				UpdateBizOs(property, value);
				wrappedProperty.Value = value;
			}

			UpdateOverride();

			property.RefreshBinding();

			foreach (var dependantProperty in dependantProperties)
			{
				dependantProperty.RefreshBinding();
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_CompanyNameMaxLength)]
		public ZString CompanyName
		{
			get { return jobDocAddress.E2_CompanyName; }
			set
			{
				if (jobDocAddress.E2_CompanyName != value)
				{
					if (!(Organisation != null && Address != null && Organisation.OH_FullName.Equals(value) && CompanyName.Equals(Address.OA_CompanyNameOverride)))
					{
						SetWrappedValue(CompanyNameInfo, jobDocAddress.E2_CompanyNameInfo, value);
					}
				}
			}
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.CompanyName);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_CompanyNameInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_Address1MaxLength)]
		public ZString Address1
		{
			get { return jobDocAddress.E2_Address1; }
			set
			{
				AddressPKInfo.ClearAllNotifications();
				SetWrappedValue(Address1Info, jobDocAddress.E2_Address1Info, value);
			}
		}

		public ZPropertyInfo Address1Info
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Address1);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_Address1Info.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_Address2MaxLength)]
		public ZString Address2
		{
			get { return jobDocAddress.E2_Address2; }
			set
			{
				SetWrappedValue(Address2Info, jobDocAddress.E2_Address2Info, value);
			}
		}

		public ZPropertyInfo Address2Info
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Address2);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_Address2Info.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_PostcodeMaxLength)]
		public ZString PostCode
		{
			get { return jobDocAddress.E2_Postcode; }
			set
			{
				SetWrappedValue(PostCodeInfo, jobDocAddress.E2_PostcodeInfo, value);
			}
		}

		public ZPropertyInfo PostCodeInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.PostCode);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_PostcodeInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_CityMaxLength)]
		public ZString City
		{
			get { return jobDocAddress.E2_City; }
			set
			{
				SetWrappedValue(CityInfo, jobDocAddress.E2_CityInfo, value);
			}
		}

		public ZPropertyInfo CityInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.City);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_CityInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_StateMaxLength)]
		public ZString State
		{
			get { return jobDocAddress.E2_State; }
			set
			{
				SetWrappedValue(StateInfo, jobDocAddress.E2_StateInfo, value);
			}
		}

		public ZPropertyInfo StateInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.State);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_StateInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_RN_NKCountryCodeMaxLength)]
		public ZString CountryCode
		{
			get { return jobDocAddress.E2_RN_NKCountryCode; }
			set
			{
				SetWrappedValue(
					CountryCodeInfo,
					jobDocAddress.E2_RN_NKCountryCodeInfo,
					value,
					StateInfo);
			}
		}

		public ZPropertyInfo CountryCodeInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.CountryCode);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_RN_NKCountryCodeInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_ContactMaxLength)]
		public ZString ContactName
		{
			get { return jobDocAddress.E2_Contact; }
			set
			{
				SetWrappedValue(ContactNameInfo, jobDocAddress.E2_ContactInfo, value);
			}
		}

		public ZPropertyInfo ContactNameInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.ContactName);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_ContactInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_PhoneMaxLength)]
		public ZString Phone
		{
			get { return jobDocAddress.E2_Phone; }
			set
			{
				SetWrappedValue(PhoneInfo, jobDocAddress.E2_PhoneInfo, value);
			}
		}

		public ZPropertyInfo PhoneInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Phone);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_PhoneInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_FaxMaxLength)]
		public ZString Fax
		{
			get { return jobDocAddress.E2_Fax; }
			set
			{
				SetWrappedValue(FaxInfo, jobDocAddress.E2_FaxInfo, value);
			}
		}

		public ZPropertyInfo FaxInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Fax);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_FaxInfo.Notifications);
				}
				return result;
			}
		}

		[MaxLength(JobDocAddress.Schema.E2_EmailMaxLength)]
		public ZString Email
		{
			get { return jobDocAddress.E2_Email; }
			set
			{
				SetWrappedValue(EmailInfo, jobDocAddress.E2_EmailInfo, value);
			}
		}

		public ZPropertyInfo EmailInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Email);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_EmailInfo.Notifications);
				}
				return result;
			}
		}

		public ZBool IsResidentialAddress
		{
			get { return jobDocAddress.E2_IsResidential; }
			set
			{
				SetWrappedValue(IsResidentialAddressInfo, jobDocAddress.E2_IsResidentialInfo, value);
			}
		}

		public ZPropertyInfo IsResidentialAddressInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.IsResidentialAddress);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_IsResidentialInfo.Notifications);
				}
				return result;
			}
		}

		public ZString AddressAsASingleLine
		{
			get { return jobDocAddress.AddressAsASingleLine; }
		}

		public ZPropertyInfo AddressAsASingleLineInfo
		{
			get { return GetZPropertyInfo(Schema.AddressAsASingleLine); }
		}

		#region GovRegNum

		[MaxLength(JobDocAddress.Schema.E2_GovRegNumMaxLength)]
		public ZString GovRegNum
		{
			get
			{
				return jobDocAddress.E2_GovRegNum;
			}
			set
			{
				jobDocAddress.E2_GovRegNum = value;
				GovRegNumInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo GovRegNumInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.GovRegNum);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_GovRegNumInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region SocialSecurityNumber

		[BusinessObjectTestExclude]
		public ZBool IsSocialSecurityNumberGovRegNumType
		{
			get
			{
				if (jobDocAddress is IWebISFDocAddress)
				{
					return ((IWebISFDocAddress)jobDocAddress).IsSocialSecurityNumberGovRegNumType;
				}
				return false;
			}
		}

		[BusinessObjectTestExclude]
		public ZString SocialSecurityNumber
		{
			get
			{
				if (jobDocAddress is IWebISFDocAddress)
				{
					return ((IWebISFDocAddress)jobDocAddress).E2_SocialSecurityNumber;
				}
				return ZString.Empty;
			}
			set
			{
				if (jobDocAddress is IWebISFDocAddress)
				{
					((IWebISFDocAddress)jobDocAddress).E2_SocialSecurityNumber = value;
				}
				SocialSecurityNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SocialSecurityNumberInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.SocialSecurityNumber);
				if (jobDocAddress != null && jobDocAddress is IWebISFDocAddress)
				{
					result.AddRange(((IWebISFDocAddress)jobDocAddress).E2_SocialSecurityNumberInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region SocialSecurityNumberDateOfBirth

		[BusinessObjectTestExclude]
		public ZDateTime SocialSecurityNumberDateOfBirth
		{
			get
			{
				if (jobDocAddress is IWebISFDocAddress)
				{
					return ((IWebISFDocAddress)jobDocAddress).E2_SocialSecurityNumberDateOfBirth;
				}
				return ZDateTime.Empty;
			}
			set
			{
				if (jobDocAddress is IWebISFDocAddress)
				{
					((IWebISFDocAddress)jobDocAddress).E2_SocialSecurityNumberDateOfBirth = value;
				}
				SocialSecurityNumberDateOfBirthInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SocialSecurityNumberDateOfBirthInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.SocialSecurityNumberDateOfBirth);
				if (jobDocAddress != null && jobDocAddress is IWebISFDocAddress)
				{
					result.AddRange(((IWebISFDocAddress)jobDocAddress).E2_SocialSecurityNumberDateOfBirthInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region GovRegNumType

		[MaxLength(JobDocAddress.Schema.E2_GovRegNumTypeMaxLength)]
		public ZString GovRegNumType
		{
			get
			{
				return jobDocAddress.E2_GovRegNumType;
			}
			set
			{
				jobDocAddress.E2_GovRegNumType = value;
				GovRegNumTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo GovRegNumTypeInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.GovRegNumType);
				if (jobDocAddress != null)
				{
					result.AddRange(jobDocAddress.E2_GovRegNumTypeInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Initialization

		public void InitBeforeBinding(bool isConsignor, bool isConsignee, NewOrgRelationTypes newOrgRelationType)
		{
			this.isConsignor = isConsignor;
			this.isConsignee = isConsignee;
			this.newOrgRelationType = newOrgRelationType;
		}

		public void InitBeforeBinding(OrgHeaderAutoCompleteHelper autoCompleteHelper)
		{
			InitBeforeBinding(autoCompleteHelper.IsConsignor, autoCompleteHelper.IsConsignee, autoCompleteHelper.NewOrgRelationType);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			this.ClearAllNotifications();
			jobDocAddress.Validation.ValidateAll();
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				if (IsOptional && IsEmpty)
				{
					jobDocAddress.E2_AddressOverride = false;
					jobDocAddress.OrganisationPK = ZGuid.Empty;
					jobDocAddress.E2_OA_Address = ZGuid.Empty;
					jobDocAddress.ContactPK = ZGuid.Empty;
				}

				ValidateAll();
			}
		}

		public void ValidateAll()
		{
			ValidateSaveAsNew();
		}

		#endregion

		#region Lookups

		public JobDocAddressLookups Lookups
		{
			get { return jobDocAddress.Lookups; }
		}

		#endregion

		#region UpdateInternals

		void SetInfo(ZPropertyInfo info, IZType value)
		{
			if (info.Value != value)
			{ info.Value = value; }
		}

		void UpdateBizOs(ZPropertyInfo info, IZType value)
		{
			if (Organisation != null && Address != null)
			{
				bool shouldUpdateContact = Contact != null;
				if (info.Name == Schema.ContactName)
				{
					shouldUpdateContact = !value.IsEmpty && value.ToString() != jobDocAddress.DefaultContactType.DefaultName;
				}
				switch (info.Name)
				{
					case Schema.CompanyName:
						SetInfo(Organisation.OH_FullName.IsEmpty ? Organisation.OH_FullNameInfo : Address.OA_CompanyNameOverrideInfo, value);
						break;
					case Schema.Address1:
						SetInfo(Address.OA_Address1Info, value);
						break;
					case Schema.Address2:
						SetInfo(Address.OA_Address2Info, value);
						break;
					case Schema.PostCode:
						SetInfo(Address.OA_PostCodeInfo, value);
						break;
					case Schema.City:
						SetInfo(Address.OA_CityInfo, value);
						break;
					case Schema.State:
						SetInfo(Address.OA_StateInfo, value);
						break;
					case Schema.CountryCode:
						SetInfo(Address.OA_RL_NKRelatedPortCodeInfo, (new PortLoader()).GetClosestPortCode(PostCode, City, State, value.ToString(), ZString.Empty, Factory, false));
						break;
					case Schema.ContactName:
						if (shouldUpdateContact)
						{
							if (Contact == null)
							{
								Contact = CreateOrUpdateContact((ZString)value);
							}
							SetInfo(Contact.OC_ContactNameInfo, value);
						}
						break;
					case Schema.Phone:
						SetInfo(shouldUpdateContact ? Contact.OC_PhoneInfo : Address.OA_PhoneInfo, value);
						break;
					case Schema.Fax:
						SetInfo(shouldUpdateContact ? Contact.OC_FaxInfo : Address.OA_FaxInfo, value);
						break;
					case Schema.Email:
						SetInfo(shouldUpdateContact ? Contact.OC_EmailInfo : Address.OA_EmailInfo, value);
						break;
				}
				string newPortCode = PortCode;
				if (Address.OA_RL_NKRelatedPortCode != newPortCode)
				{
					Address.OA_RL_NKRelatedPortCode = newPortCode;
				}
			}
		}

		void UpdateBizOs()
		{
			SetInfo(Organisation.OH_FullName.IsEmpty ? Organisation.OH_FullNameInfo : Address.OA_CompanyNameOverrideInfo, CompanyName);

			SetInfo(Address.OA_Address1Info, Address1);
			SetInfo(Address.OA_Address2Info, Address2);
			SetInfo(Address.OA_PostCodeInfo, PostCode);
			SetInfo(Address.OA_CityInfo, City);
			SetInfo(Address.OA_StateInfo, State);
			SetInfo(Address.OA_RL_NKRelatedPortCodeInfo, PortCode);
			bool shouldUpdateContact = !ContactName.IsEmpty && ContactName != (ZString)jobDocAddress.DefaultContactType.DefaultName;
			if (shouldUpdateContact)
			{
				if (Contact == null)
				{
					Contact = CreateOrUpdateContact();
				}
				SetInfo(Contact.OC_ContactNameInfo, ContactName);
			}
			SetInfo(shouldUpdateContact ? Contact.OC_PhoneInfo : Address.OA_PhoneInfo, Phone);
			SetInfo(shouldUpdateContact ? Contact.OC_FaxInfo : Address.OA_FaxInfo, Fax);
			SetInfo(shouldUpdateContact ? Contact.OC_EmailInfo : Address.OA_EmailInfo, Email);
		}

		void UpdateInternals()
		{
			if (SaveAsNew)
			{
				UpdateBizOs();
			}
		}

		#region Overrides

		bool CompanyNameOverriden
		{
			get
			{
				ZString name = ZString.Empty;
				if (Address != null)
				{
					name = Address.OA_CompanyNameOverrideTruncated;
				}
				if (name.IsEmpty && Organisation != null)
				{
					name = Organisation.OH_FullNameTruncated;
				}
				return CompanyName != name;
			}
		}

		bool AddressOverriden
		{
			get
			{
				bool result = false;
				if (Address != null)
				{
					result =
					Address1 != Address.OA_Address1 ||
					Address2 != Address.OA_Address2 ||
					PostCode != Address.OA_PostCode ||
					City != Address.OA_City ||
					State != Address.OA_State;
				}
				else
				{
					result =
						!Address1.IsEmpty ||
						!Address2.IsEmpty ||
						!PostCode.IsEmpty ||
						!City.IsEmpty ||
						!State.IsEmpty;
				}
				return result;
			}
		}

		bool ContactOverriden
		{
			get
			{
				bool result = false;
				if (Contact != null)
				{
					result =
					ContactName != Contact.OC_ContactName ||
					Fax != Contact.OC_Fax ||
					Email != Contact.OC_Email ||
					(Address != null && Phone != Contact.OC_Phone && Phone != Address.OA_Phone) ||
					(Address == null && Phone != Contact.OC_Phone);
				}
				else
				{
					result =
						(!ContactName.IsEmpty && ContactName != (ZString)jobDocAddress.DefaultContactType.DefaultName) ||
						!Fax.IsEmpty ||
						!Email.IsEmpty ||
						(!Phone.IsEmpty && Address == null) ||
						(!Phone.IsEmpty && Address != null && Phone != Address.OA_Phone);
				}
				return result;
			}
		}

		public bool HasOverrides
		{
			get
			{
				return
						!SaveAsNew &&
						(CompanyNameOverriden ||
						AddressOverriden ||
						ContactOverriden)
						|| jobDocAddress.E2_AddressOverride;
			}
		}

		#endregion

		#endregion

		#region Implementation

		readonly JobDocAddress jobDocAddress;
		bool isConsignor;
		bool isConsignee;
		NewOrgRelationTypes newOrgRelationType;

		OrgHeaderAutoCompleteHelper Helper
		{
			get { return fHelper ?? (fHelper = new OrgHeaderAutoCompleteHelper(Factory)); }
		}
		OrgHeaderAutoCompleteHelper fHelper;

		ZString PortCode
		{
			get
			{
				return (new PortLoader()).GetClosestPortCode(jobDocAddress, true, false);
			}
		}

		bool CanCreateNewOrgHeader
		{
			get
			{
				return !CompanyName.IsEmpty && !PortCode.EndsWith("ZZZ") && !PortCode.IsEmpty;
			}
		}

		#region IsEmpty

		bool IsEmpty
		{
			get
			{
				return CompanyName.IsEmpty &&
						 Address1.IsEmpty &&
						 Address2.IsEmpty &&
						 PostCode.IsEmpty &&
						 City.IsEmpty &&
						 State.IsEmpty &&
						 CountryCode.IsEmpty &&
						 ContactName.IsEmpty &&
						 Phone.IsEmpty &&
						 Fax.IsEmpty &&
						 Email.IsEmpty;
			}
		}

		#endregion

		#region Saving

		public override void Delete()
		{
			jobDocAddress.Delete();
			base.Delete();
		}

		protected override void OnFactorySaving()
		{
			if (Inactive)
			{
				Delete();
			}
			else
			{
				UpdateJobDocAddressProperties();
				if (!jobDocAddress.IsDeleted)
				{
					if (jobDocAddress.Organisation != null)
					{
						if (!jobDocAddress.Organisation.IsDeleted && !jobDocAddress.Organisation.IsInDatabase)
						{
							if (jobDocAddress.Organisation.CompanyData != null && jobDocAddress.Organisation.CompanyData.ControllingBranch == null)
							{
								if (WebEnv.CurrentUser != null)
								{
									OrgContact user = WebEnv.CurrentUser as OrgContact;
									if (user != null && user.ParentOrg != null)
									{
										GlbBranch controllingBranch = GlbBranch.FindControllingBranchWithFallBackToAnyCompany(user.ParentOrg);
										if (controllingBranch != null)
										{
											jobDocAddress.Organisation.CompanyData.OB_GB_ControllingBranch = controllingBranch.PK;
										}
									}
								}
							}
						}
					}
				}
			}

			base.OnFactorySaving();
		}

		public void UpdateJobDocAddressProperties()
		{
			if (!jobDocAddress.IsDeleted && !savingProcessed && (!IsOptional || !IsEmpty))
			{
				UpdateJobDocAddressPropertiesCore();
				savingProcessed = true;

#if DEBUG
				JobDocAddressPropertiesHaveBeenUpdated = true;
#endif
			}
		}

#if DEBUG
		public bool JobDocAddressPropertiesHaveBeenUpdated;
#endif

		bool savingProcessed;

		void UpdateJobDocAddressPropertiesCore()
		{
			UpdateInternals();
		}

		#endregion

		#region Set Related Properties

		ZString GetCountryCodeByPortCode(ZString portCode)
		{
			return !portCode.IsEmpty ? portCode.Substring(0, 2) : ZString.Empty;
		}

		void SetAddressProperties(OrgAddress orgAddress)
		{
			UpdatingCurrentState = true;
			if (orgAddress != null && orgAddress.PK != OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress)
			{
				if (!orgAddress.OA_CompanyNameOverride.IsEmpty)
				{
					CompanyName = orgAddress.OA_CompanyNameOverrideTruncated;
				}
				else if (Organisation != null)
				{
					CompanyName = Organisation.OH_FullNameTruncated;
				}
				Address1 = orgAddress.OA_Address1;
				Address2 = orgAddress.OA_Address2;
				City = orgAddress.OA_City;
				PostCode = orgAddress.OA_PostCode;
				State = orgAddress.OA_State;
				CountryCode = GetCountryCodeByPortCode(orgAddress.OA_RL_NKRelatedPortCode);
				IsResidentialAddress = orgAddress.AddressCapability.GetAddressCapabilityOnCode(OrgConstants.AddressType.Residential).Enabled;
			}
			else
			{
				Address1 = "";
				Address2 = "";
				City = "";
				PostCode = "";
				State = "";
				CountryCode = "";
				IsResidentialAddress = false;
			}

			UpdatingCurrentState = false;
		}

		void SetContactProperties(OrgContact orgContact)
		{
			UpdatingCurrentState = true;
			if (orgContact != null)
			{
				if (Address == null && Organisation == null && jobDocAddress.E2_AddressOverride)
				{
					TryToUndoOverride(orgContact);
				}
				else
				{
					ContactName = orgContact.OC_ContactName;
					Phone = orgContact.OC_Phone;
					Fax = orgContact.OC_Fax;
					Email = orgContact.OC_Email;
				}
			}
			else
			{
				if (Address != null)
				{
					Phone = Address.OA_Phone;
					Fax = Address.OA_Fax;
					Email = Address.OA_Email;
				}
				else
				{
					Phone = "";
					Fax = "";
					Email = "";
				}
				ContactName = "";
			}

			UpdatingCurrentState = false;
		}

		void TryToUndoOverride(OrgContact orgContact)
		{
			OrgHeader header = orgContact.ParentOrg;
			if (header != null)
			{
				var exactAddressQuery = new ZQuery(OrgAddressSchema.OA_Address1, Address1);
				exactAddressQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_Address2, Address2);
				exactAddressQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_City, City);
				exactAddressQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_PostCode, PostCode);
				exactAddressQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_State, State);
				exactAddressQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, CountryCode);
				var addresses = header.Addresses.Find(exactAddressQuery);
				var exactAddressMatch = addresses.Length > 0 ? addresses[0] : null;
				if (exactAddressMatch != null)
				{
					AddressPK = exactAddressMatch.PK;
					jobDocAddress.ContactPK = orgContact.PK;
				}
			}
		}

		#endregion

		#region Business Objects Search

		ZGuid FindOrgHeaderPK()
		{
			return (ZGuid)Helper.GetKey(CompanyName);
		}

		bool OrgHeaderExists
		{
			get { return !FindOrgHeaderPK().IsEmpty; }
		}

		#endregion

		#endregion
	}
}
