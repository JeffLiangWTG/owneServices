using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebOrgAddress : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static class Schema
		{
			public const string AddressPK = "AddressPK";
			public const string OrganisationPK = "OrganisationPK";
			public const string CompanyName = "CompanyName";
			public const string FormattedAddressSummary = "FormattedAddressSummary";
			public const string Phone = "Phone";
			public const string Fax = "Fax";
			public const string Email = "Email";
		}

		public WebOrgAddress(BusinessObjectFactory factory, OrgAddressType addressType = null)
			: this(null, factory, addressType)
		{
		}

		public WebOrgAddress(OrgAddress orgAddress)
			: this(orgAddress, orgAddress.Factory)
		{ }

		public WebOrgAddress(OrgAddress orgAddress, BusinessObjectFactory factory, OrgAddressType addressType = null)
		{
			this.factory = factory;
			if (orgAddress != null)
			{
				this.orgAddress = orgAddress;
				this.OrganisationPK = orgAddress == null ? ZGuid.Empty : orgAddress.OA_OH;
			}

			this.addressType = addressType;
		}
		readonly OrgAddressType addressType;

		readonly BusinessObjectFactory factory;

		#region Organisation

		public ZGuid OrganisationPK
		{
			get { return fOrganisationPK; }
			set
			{
				if (fOrganisationPK != value)
				{
					fOrganisationPK = value;
					UpdateOrganisation();
					OrganisationPKInfo.RefreshBinding();
				}
			}
		}
		ZGuid fOrganisationPK;

		void UpdateOrganisation()
		{
			if (OrganisationPK.IsValid && !OrganisationPK.IsEmpty)
			{
				fOrganisation = factory.Load<OrgHeader>(OrganisationPK);
				SetDefaultAddressPK();
			}
			else
			{
				fOrganisation = null;
			}
		}

		void SetDefaultAddressPK()
		{
			if (fOrganisation != null && (AddressPK.IsEmpty || !fOrganisation.Addresses.Contains(AddressPK)))
			{
				var result = fOrganisation.MainAddress.PK;

				if (addressType != null)
				{
					var defaultAddress = fOrganisation.Addresses.DefaultAddressOfType(addressType);
					if (defaultAddress != null)
					{
						result = defaultAddress.PK;
					}
					else
					{
						var anyAddress = fOrganisation.Addresses.AddressesOfType(addressType).Cast<OrgAddress>().FirstOrDefault();
						if (anyAddress != null)
						{
							result = anyAddress.PK;
						}
					}
				}

				AddressPK = result;
			}
		}

		public OrgHeader Organisation
		{
			get { return fOrganisation; }
		}
		OrgHeader fOrganisation;

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrganisationPK); }
		}

		public OrgAddressType OrgAddressType
		{
			get { return addressType; }
		}

		#endregion

		#region Address

		[List("Addresses")]
		public ZGuid AddressPK
		{
			get { return fAddressPK; }
			set
			{
				if (fAddressPK != value)
				{
					fAddressPK = value;
					UpdateAddress();
					if (AddressChanged != null)
					{
						AddressChanged(this, new EventArgs());
					}
					AddressPKInfo.RefreshBinding();
				}
			}
		}
		ZGuid fAddressPK;
		public event EventHandler AddressChanged;

		void UpdateAddress()
		{
			orgAddress = AddressPK.IsValid && !AddressPK.IsEmpty ?
				factory.Load<OrgAddress>(AddressPK) : factory.New<OrgAddress>();
			OrganisationPK = orgAddress == null ? ZGuid.Empty : orgAddress.OA_OH;
		}

		public ZPropertyInfo AddressPKInfo
		{
			get { return GetZPropertyInfo(Schema.AddressPK); }
		}

		public OrgAddressDependentCollection Addresses
		{
			get
			{
				return Organisation == null ? new OrgAddressDependentCollection(Factory) : Organisation.Addresses;
			}
		}

		OrgAddress orgAddress;
		public OrgAddress Address
		{
			get { return orgAddress; }
		}

		#endregion

		AddressFormatter Formatter
		{
			get
			{
				if (fFormatter == null)
				{
					fFormatter = new AddressFormatter(factory ?? new BusinessObjectFactory(), orgAddress, GlbCompany.CurrentCompany, true);
				}
				return fFormatter;
			}
		}
		AddressFormatter fFormatter;

		#region Properties

		#region FormattedAddressSummary

		[BusinessObjectTestExclude]
		public ZString FormattedAddressSummary
		{
			get
			{
				ZString result = Formatter.PostalAddress().Replace("\n", System.Environment.NewLine);
				if (result.IsEmpty)
				{ result = Res.GetString("07578776-6614-4a2e-a665-7940da9272d6", "No Address Selected"); }
				return result;
			}
		}

		#endregion

		#region Phone

		[MaxLength(OrgAddress.Schema.OA_PhoneMaxLength)]
		public ZString Phone
		{
			get { return Address.OA_Phone; }
		}

		public ZPropertyInfo PhoneInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Phone);
				if (Address != null)
				{
					result.AddRange(Address.OA_PhoneInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region Fax

		[MaxLength(OrgAddress.Schema.OA_FaxMaxLength)]
		public ZString Fax
		{
			get { return Address.OA_Fax; }
		}

		public ZPropertyInfo FaxInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Fax);
				if (Address != null)
				{
					result.AddRange(Address.OA_FaxInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#region Email

		[MaxLength(OrgAddress.Schema.OA_EmailMaxLength)]
		public ZString Email
		{
			get { return Address.OA_Email; }
		}

		public ZPropertyInfo EmailInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.Email);
				if (Address != null)
				{
					result.AddRange(Address.OA_EmailInfo.Notifications);
				}
				return result;
			}
		}

		#endregion

		#endregion

	}
}
