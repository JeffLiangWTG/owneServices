using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentWrappers
{
	public interface IDocDocAddress
	{
		ZString AccessPoint { get; }
		ZString Address1 { get; }
		ZString Address2 { get; }
		ZString AddressType { get; }
		ZString City { get; }
		ZString Code { get; }
		ZString CommunicationRequired { get; }
		ZString CompanyName { get; }
		ZString CompanyNameAddress1Address2City { get; }
		ZString CompanyNameOverride { get; }
		ZString ContactName { get; }
		ZString ContainerHandling { get; }
		ZString DepotLocalControlledPremisesID { get; }
		ZString Description { get; }
		ZString DockHeight { get; }
		ZString Email { get; }
		ZString Fax { get; }
		ZString FurtherConstraints { get; }
		ZString LabourRequired { get; }
		ZString Language { get; }
		ZString LocalControlledPremisesID { get; }
		ZString OtherWarehouseFacilities { get; }
		ZString Phone { get; }
		ZString PostalAddress { get; }
		ZString PostalAddressExcludeName { get; }
		ZString PostCode { get; }
		ZString SplitAddress1 { get; }
		ZString SplitAddress2 { get; }
		ZString State { get; }
		ZString Type { get; }
		ZString UsageComment { get; }
		ZString WarehouseLocalControlledPremisesID { get; }

		ZDateTime DeliverFromTimeOnly { get; }
		ZDateTime DeliverToTimeOnly { get; }
		ZDateTime DoNotAttendFrom { get; }
		ZDateTime DoNotAttendFromForCartageAdvice { get; }
		ZDateTime DoNotAttendTo { get; }
		ZDateTime DoNotAttendToForCartageAdvice { get; }
		ZDateTime PickupFromTimeOnly { get; }
		ZDateTime PickupToTimeOnly { get; }

		ZBool HasDockLeveller { get; }
		ZBool HasForkLift { get; }
		ZBool HasLoadingUnloadingConstraints { get; }
		ZBool HasPalletJack { get; }
		ZBool HasWareHousing { get; }
		ZBool Overridden { get; }

		DocCountry Country { get; }
		DocAddressType DocAddressType { get; }
		DocOrganisation Organisation { get; }
		DocUNLOCO Port { get; }
	}

	public class DocDocAddress : DocBaseWrapper, IDocDocAddress
	{
		DocDocAddress(JobDocAddress docAddress, BusinessObjectFactory factoryForWrapper)
			: base(docAddress, factoryForWrapper)
		{
		}

		public static DocDocAddress New(JobDocAddress docAddress, BusinessObjectFactory factoryForWrapper)
		{
			if (docAddress == null)
			{
				return null;
			}
			else
			{
				return new DocDocAddress(docAddress, factoryForWrapper);
			}
		}

		public static bool IsNullOrEmpty(DocDocAddress docAddress)
		{
			bool result = true;

			if (docAddress != null)
			{
				return IsEmpty(docAddress.DocAddress);
			}

			return result;
		}

		static bool IsEmpty(JobDocAddress docAddress)
		{
			bool result = true;

			if (!docAddress.E2_AddressOverride)
			{
				result = !docAddress.E2_OA_Address.IsValid;
			}
			else
			{
				result = docAddress.E2_Address1.IsEmpty && docAddress.E2_CompanyName.IsEmpty;
			}

			return result;
		}

		public static DocDocAddress New(OrgAddress orgAddress, BusinessObjectFactory factoryForWrapper)
		{
			if (ReferenceEquals(orgAddress, null))
			{
				return null;
			}
			else
			{
				JobDocAddress docAddress = factoryForWrapper.New<JobDocAddress>();
				if (!orgAddress.OA_OH.IsValid)
				{
					OrgHeader fakeOrgHeader = factoryForWrapper.GetCachedReadOnlyFactory().New<OrgHeader>();
					orgAddress.OA_OH = fakeOrgHeader.PK;
				}

				docAddress.E2_OA_Address = orgAddress.PK;
				docAddress.MakeNonPersistent();

				return new DocDocAddress(docAddress, factoryForWrapper);
			}
		}

		public static bool AreTheseDocAddressesTheSameOrgAddresses(DocDocAddress docDocAddress1, DocDocAddress docDocAddress2)
		{
			bool result = false;
			if (docDocAddress1.IsPivotToActualAddress && docDocAddress2.IsPivotToActualAddress)
			{
				result = (docDocAddress1.PivotReferenceToActualAddress == docDocAddress2.PivotReferenceToActualAddress);
			}

			return result;
		}

		#region ZString fields

		public ZString CompanyNameAddress1Address2City
		{
			get { return CompanyName + " " + Address1 + " " + Address2 + " " + City; }
		}

		public ZString AddressSummaryExcudeAddress1Address2
		{
			get
			{
				StringBuilder result = new StringBuilder();
				if (!City.IsEmpty)
				{
					result.Append(City.ToUpper() + " ");
				}

				if (!State.IsEmpty)
				{
					result.Append(State.ToUpper() + " ");
				}

				if (!PostCode.IsEmpty)
				{
					result.Append(PostCode.ToUpper() + " ");
				}

				if (Country != null)
				{
					result.Append(Country.ToString().ToUpper(CultureInfo.CurrentCulture) + " ");
				}

				return result.ToString();
			}
		}

		public ZString Code
		{
			get { return (OrgAddress == null) ? DocAddress.E2_AddressType : OrgAddress.OA_Code; }
		}

		public ZString Description
		{
			get { return DocAddress.AddressDescription; }
		}

		public ZString Address1
		{
			get { return DocAddress.E2_Address1; }
		}

		public ZString Address2
		{
			get { return DocAddress.E2_Address2; }
		}

		public ZString Type
		{
			get { return DocAddress.E2_AddressType; }
		}

		public DocAddressType DocAddressType
		{
			get { return DocAddress.DocAddressType; }
		}

		public ZBool Overridden
		{
			get { return DocAddress.E2_AddressOverride; }
		}

		public ZString AddressType
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.AddressCapability.GetListOfCodes(); }
		}

		public ZString City
		{
			get { return DocAddress.E2_City; }
		}

		public ZString CompanyName
		{
			get { return DocAddress.E2_CompanyName; }
		}

		public ZString CompanyNameOverride
		{
			get { return DocAddress.E2_CompanyName; }
		}

		public ZString Phone
		{
			get { return DocAddress.E2_Phone_Formatted; }
		}

		public ZString GetContactPhone(ContactType fallbackToContactType)
		{
			ZString result = "";

			if (DocAddress.Contact != null)
			{
				result = DocAddress.Contact.OC_Phone_Formatted;

				if (result.IsEmpty)
				{
					result = Phone;
				}
			}

			if (result.IsEmpty)
			{
				DocContacts contact = GetContact(fallbackToContactType);
				if (contact != null)
				{
					result = contact.Phone;
				}
			}

			if (result.IsEmpty)
			{
				result = Phone;
			}

			return result;
		}

		public ZString Fax
		{
			get { return DocAddress.E2_Fax_Formatted; }
		}

		public ZString GetContactFax(ContactType fallbackToContactType)
		{
			ZString result = "";

			if (DocAddress.Contact != null)
			{
				result = DocAddress.Contact.OC_Fax_Formatted;

				if (result.IsEmpty)
				{
					result = Fax;
				}
			}

			if (result.IsEmpty)
			{
				DocContacts contact = GetContact(fallbackToContactType);
				if (contact != null)
				{
					result = contact.Fax;
				}
			}

			if (result.IsEmpty)
			{
				result = Fax;
			}

			return result;
		}

		public ZString Language
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.OA_Language; }
		}

		public ZString Email
		{
			get { return DocAddress.E2_Email; }
		}

		public ZString PostCode
		{
			get { return DocAddress.E2_Postcode; }
		}

		public ZString State
		{
			get { return DocAddress.E2_State; }
		}

		public ZString UsageComment
		{
			get { return (OrgAddress == null) ? DocAddress.E2_AddressType : OrgAddress.OA_Code; }
		}

		#region Postal Address

		public ZString PostalAddress
		{
			get { return GetPostalAddress(); }
		}

		public ZString PostalAddressInEnglish
		{
			get { return GetPostalAddress(true); }
		}

		public ZString PostalAddressExcludeName
		{
			get
			{
				if (PostalAddress.StartsWith(CompanyName.ToUpper()))
				{
					return PostalAddress.SubstringSafe(CompanyName.Length).TrimStart('\n');
				}
				else
				{
					return PostalAddress;
				}
			}
		}

		ZString GetPostalAddress(bool inEnglish = false, bool includeCountryEvenIfSame = true)
		{
			var result = ZString.Empty;

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var docCompany = DocCompany.New(currentCompany, Factory);

			if (docCompany != null)
			{
				var addressFormatter = includeCountryEvenIfSame ? new WrapperPostalAddressFormatter(this, docCompany) : new WrapperPostalAddressFormatter(this, docCompany, false);
				addressFormatter.EnglishOnly = inEnglish;
				result = addressFormatter.PostalAddress();
			}

			return result;
		}

		public ZString PostalAddressExcludeCountryIfSame
		{
			get { return GetPostalAddress(false, false); }
		}

		#endregion

		public ZString LocalControlledPremisesID
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.LocalControlledPremisesID; }
		}

		public ZString WarehouseLocalControlledPremisesID
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.WarehouseLocalControlledPremisesID; }
		}

		public ZString DepotLocalControlledPremisesID
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.DepotLocalControlledPremisesID; }
		}
		#endregion

		#region ZDateTime fields

		public ZDateTime PickupFromTimeOnly
		{
			get { return (OrgAddress == null) ? ZDateTime.Empty : OrgAddress.OA_PickupFromTimeOnly; }
		}

		public ZDateTime PickupToTimeOnly
		{
			get { return (OrgAddress == null) ? ZDateTime.Empty : OrgAddress.OA_PickupToTimeOnly; }
		}

		public ZDateTime DeliverFromTimeOnly
		{
			get { return (OrgAddress == null) ? ZDateTime.Empty : OrgAddress.OA_DeliverFromTimeOnly; }
		}

		public ZDateTime DeliverToTimeOnly
		{
			get { return (OrgAddress == null) ? ZDateTime.Empty : OrgAddress.OA_DeliverToTimeOnly; }
		}

		public ZDateTime DoNotAttendFrom
		{
			get { return (OrgAddress == null) ? ZDateTime.Empty : OrgAddress.OA_DoNotAttendFrom; }
		}

		public ZDateTime DoNotAttendTo
		{
			get { return (OrgAddress == null) ? ZDateTime.Empty : OrgAddress.OA_DoNotAttendTo; }
		}

		#region Cartage Advice Fields

		public ZDateTime DoNotAttendFromForCartageAdvice
		{
			get { return (IsOrgDepotOrContainerPark) ? OrgAddress.OA_DoNotAttendFrom : ZDateTime.Empty; }
		}

		public ZDateTime DoNotAttendToForCartageAdvice
		{
			get { return (IsOrgDepotOrContainerPark) ? OrgAddress.OA_DoNotAttendTo : ZDateTime.Empty; }
		}

		#endregion

		#endregion

		#region Contacts

		public DocContacts GetContact(ContactType contactType)
		{
			DocContacts contact = null;

			if (Organisation != null && !DocAddress.E2_AddressOverride)
			{
				contact = Organisation.DefaultContact(contactType);

				if (contact != null)
				{
					ContactOrgContactSource contactSource = contact.WrappedObject as ContactOrgContactSource;
					if (contactSource != null)
					{
						OrgContact orgContact = contactSource.WrappedObject as OrgContact;
						if (orgContact != null && !orgContact.IsInDatabase && orgContact.OrgAddress == null && OrgAddress != null)
						{
							orgContact.OC_OA_OrgAddress = OrgAddress.PK;
						}
					}
				}
			}
			else if (DocAddress.E2_AddressOverride)
			{
				contact = DocContacts.New(DocAddress, Factory);
			}

			return contact;
		}

		public ZString ContactName
		{
			get { return DocAddress.E2_Contact; }
		}

		public ZString GetContactName(ContactType fallbackToContactType)
		{
			ZString result = ContactName;

			if (result.IsEmpty)
			{
				DocContacts contact = GetContact(fallbackToContactType);
				if (contact != null)
				{
					result = contact.ContactName;
				}
			}

			return result;
		}

		#endregion

		#region Warehousing Facilities and Loading/Unloading Constraints

		public ZBool HasWareHousing
		{
			get
			{
				return (HasDockLeveller ||
					HasPalletJack ||
					HasForkLift ||
					!OtherWarehouseFacilities.IsEmpty);
			}
		}

		public ZBool HasLoadingUnloadingConstraints
		{
			get
			{
				return (!AccessPoint.IsEmpty ||
					!ContainerHandling.IsEmpty ||
					!CommunicationRequired.IsEmpty ||
					!LabourRequired.IsEmpty ||
					!DockHeight.IsEmpty ||
					!FurtherConstraints.IsEmpty);
			}
		}

		public ZBool HasDockLeveller
		{
			get { return (OrgAddress == null) ? ZBool.False : OrgAddress.OA_DockLeveler; }
		}

		public ZBool HasPalletJack
		{
			get { return (OrgAddress == null) ? ZBool.False : OrgAddress.OA_PalletJack; }
		}

		public ZBool HasForkLift
		{
			get { return (OrgAddress == null) ? ZBool.False : OrgAddress.OA_ForkLift; }
		}

		public ZString OtherWarehouseFacilities
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.OA_OtherWarehouseFacilities; }
		}

		public ZString AccessPoint
		{
			get { return (OrgAddress == null) ? "" : OrgAddress.OA_AccessPoint_List.GetDescriptionFromCode(OrgAddress.OA_AccessPoint); }
		}

		public ZString ContainerHandling
		{
			get { return (OrgAddress == null) ? "" : OrgAddress.OA_ContainerHandling_List.GetDescriptionFromCode(OrgAddress.OA_ContainerHandling); }
		}

		public ZString CommunicationRequired
		{
			get { return (OrgAddress == null) ? "" : OrgAddress.OA_CommunicationRequired_List.GetDescriptionFromCode(OrgAddress.OA_CommunicationRequired); }
		}

		public ZString LabourRequired
		{
			get { return (OrgAddress == null) ? "" : OrgAddress.OA_LabourRequired_List.GetDescriptionFromCode(OrgAddress.OA_LabourRequired); }
		}

		public ZString DockHeight
		{
			get { return (OrgAddress == null) ? "" : OrgAddress.OA_Dock_Height_List.GetDescriptionFromCode(OrgAddress.OA_Dock_Height); }
		}

		public ZString FurtherConstraints
		{
			get { return (OrgAddress == null) ? ZString.Empty : OrgAddress.OA_LoadingUnloadingConstraints; }
		}
		#endregion

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(DocAddress, Factory); }
		}

		public DocCountry Country
		{
			get { return (DocAddress.Country != null) ? DocCountry.New(DocAddress.Country, Factory) : null; }
		}

		public DocUNLOCO Port
		{
			get { return (OrgAddress == null) ? null : DocUNLOCO.New(Factory, OrgAddress.OA_RL_NKRelatedPortCode); }
		}

		public DocCusCodeCollection CustomCodes
		{
			get { return (OrgAddress == null || OrgAddress.CustomsCodes == null) ? new DocCusCodeCollection(new OrgCusCodeCollection(null, Factory), Factory) : new DocCusCodeCollection(OrgAddress.CustomsCodes, Factory); }
		}

		public override string ToString()
		{
			return PostalAddress;
		}

		#region Split Address

		public ZString SplitAddress1
		{
			get
			{
				SplitPostalAddress();
				return fSplitAddress1;
			}
		}

		public ZString SplitAddress2
		{
			get
			{
				SplitPostalAddress();
				return fSplitAddress2;
			}
		}

		protected void SplitPostalAddress()
		{
			if (fSplitAddress1.IsEmpty && fSplitAddress2.IsEmpty)
			{
				ZString unformattedPostalAddress = PostalAddress.Replace("\n", " ");
				ZString[] splitFormattedAddress = WrapTextForAColumn(unformattedPostalAddress, 30).Split('\n');

				if (splitFormattedAddress.Length > 0)
				{
					fSplitAddress1 = splitFormattedAddress[0];
				}

				if (splitFormattedAddress.Length > 1)
				{
					ZString[] splitAddressElementTwo = WrapTextForAColumn(splitFormattedAddress[1], 25).Split('\n');
					if (splitAddressElementTwo.Length > 0)
					{
						fSplitAddress2 = splitAddressElementTwo[0];
					}
				}
			}
		}

		protected ZString fSplitAddress1;
		protected ZString fSplitAddress2;

		#endregion

		#region Implementation

		JobDocAddress DocAddress
		{
			get { return (JobDocAddress)WrappedObject; }
		}

		OrgAddress OrgAddress
		{
			get { return DocAddress.Address; }
		}

		bool IsOrgDepotOrContainerPark
		{
			get { return (Organisation != null && (Organisation.IsPackDepot || Organisation.IsUnpackDepot || Organisation.IsContainerPark)); }
		}

		public ZBool IsPivotToActualAddress
		{
			get { return DocAddress.E2_OA_Address.IsValid; }
		}

		public ZString PivotReferenceToActualAddress
		{
			get { return DocAddress.E2_OA_Address.ToString(); }
		}
		#endregion
	}
}
