using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAddress : DocBaseWrapper
	{
		DocAddress(OrgAddress orgAddress, BusinessObjectFactory factoryForWrapper)
			: base(orgAddress, factoryForWrapper)
		{
			this.OrgAddress = orgAddress;
		}

		public static DocAddress New(OrgAddress orgAddress, BusinessObjectFactory factoryForWrapper)
		{
			if (orgAddress == null)
			{
				return null;
			}
			else
			{
				return new DocAddress(orgAddress, factoryForWrapper);
			}
		}

		#region ZString fields

		public ZString CompanyNameAddress1Address2City
		{
			get { return Organisation.Name + " " + Address1 + " " + Address2 + " " + City; }
		}

		public ZString Code
		{
			get { return OrgAddress.OA_Code; }
		}

		public ZString Description
		{
			get { return OrgAddress.OA_Code; }
		}

		public ZString Address1
		{
			get { return OrgAddress.OA_Address1; }
		}

		public ZString Address2
		{
			get { return OrgAddress.OA_Address2; }
		}

		public ZString AddressType
		{
			get { return OrgAddress.AddressCapability.GetListOfCodes(); }
		}

		public ZString City
		{
			get { return OrgAddress.OA_City; }
		}

		public ZString CompanyName
		{
			get
			{
				ZString result = ZString.Empty;

				if (!CompanyNameOverride.IsEmpty)
				{
					result = CompanyNameOverride;
				}
				else if (Organisation != null)
				{
					result = Organisation.Name;
				}

				return result;
			}
		}

		public ZString CompanyNameOverride
		{
			get { return OrgAddress.OA_CompanyNameOverride; }
		}

		public ZString Phone
		{
			get { return OrgAddress.OA_Phone_Formatted; }
		}

		public ZString Fax
		{
			get { return OrgAddress.OA_Fax_Formatted; }
		}

		public ZString Language
		{
			get { return OrgAddress.OA_Language; }
		}

		public ZString PostCode
		{
			get { return OrgAddress.OA_PostCode; }
		}

		public ZString State
		{
			get { return OrgAddress.OA_State; }
		}

		public ZString UsageComment
		{
			get { return OrgAddress.OA_Code; }
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
			var result = ZString.Empty;

			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var docCompany = DocCompany.New(currentCompany, Factory);

			if (docCompany != null)
			{
				var addressFormatter = new WrapperPostalAddressFormatter(this, docCompany);
				if (addressFormatter != null)
				{
					addressFormatter.EnglishOnly = inEnglish;
					result = addressFormatter.PostalAddress();
				}
			}

			return result;
		}

		public ZString PostalAddressExcludeCountryIfSame
		{
			get
			{
				ZString result = ZString.Empty;

				var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				DocCompany docCompany = DocCompany.New(currentCompany, Factory);

				if (docCompany != null)
				{
					WrapperPostalAddressFormatter addressFormatter = new WrapperPostalAddressFormatter(this, docCompany, false);
					if (addressFormatter != null)
					{
						result = addressFormatter.PostalAddress();
					}
				}

				return result;
			}
		}

		public ZString PostalAddressExcludeName
		{
			get
			{
				string nameWithoutConsecutiveSpace = Regex.Replace(CompanyName.ToUpper(), @"(\s)\1+", "$1");
				if (PostalAddress.ToUpper().StartsWith(nameWithoutConsecutiveSpace))
				{
					return PostalAddress.SubstringSafe(nameWithoutConsecutiveSpace.Length).TrimStart('\n');
				}
				else
				{
					return PostalAddress;
				}
			}
		}
		public ZString LocalControlledPremisesID
		{
			get { return OrgAddress.LocalControlledPremisesID; }
		}

		public ZString WarehouseLocalControlledPremisesID
		{
			get { return OrgAddress.WarehouseLocalControlledPremisesID; }
		}

		public ZString DepotLocalControlledPremisesID
		{
			get { return OrgAddress.DepotLocalControlledPremisesID; }
		}
		#endregion

		#region ZDateTime fields

		public ZDateTime PickupFromTimeOnly
		{
			get { return OrgAddress.OA_PickupFromTimeOnly; }
		}

		public ZDateTime PickupToTimeOnly
		{
			get { return OrgAddress.OA_PickupToTimeOnly; }
		}

		public ZDateTime DeliverFromTimeOnly
		{
			get { return OrgAddress.OA_DeliverFromTimeOnly; }
		}

		public ZDateTime DeliverToTimeOnly
		{
			get { return OrgAddress.OA_DeliverToTimeOnly; }
		}

		public ZDateTime DoNotAttendFrom
		{
			get { return OrgAddress.OA_DoNotAttendFrom; }
		}

		public ZDateTime DoNotAttendTo
		{
			get { return OrgAddress.OA_DoNotAttendTo; }
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

			if (Organisation != null)
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

			return contact;
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
			get { return OrgAddress.OA_DockLeveler; }
		}

		public ZBool HasPalletJack
		{
			get { return OrgAddress.OA_PalletJack; }
		}

		public ZBool HasForkLift
		{
			get { return OrgAddress.OA_ForkLift; }
		}

		public ZString OtherWarehouseFacilities
		{
			get { return OrgAddress.OA_OtherWarehouseFacilities; }
		}

		public ZString AccessPoint
		{
			get { return OrgAddress.OA_AccessPoint_List.GetDescriptionFromCode(OrgAddress.OA_AccessPoint); }
		}

		public ZString ContainerHandling
		{
			get { return OrgAddress.OA_ContainerHandling_List.GetDescriptionFromCode(OrgAddress.OA_ContainerHandling); }
		}

		public ZString CommunicationRequired
		{
			get { return OrgAddress.OA_CommunicationRequired_List.GetDescriptionFromCode(OrgAddress.OA_CommunicationRequired); }
		}

		public ZString LabourRequired
		{
			get { return OrgAddress.OA_LabourRequired_List.GetDescriptionFromCode(OrgAddress.OA_LabourRequired); }
		}

		public ZString DockHeight
		{
			get { return OrgAddress.OA_Dock_Height_List.GetDescriptionFromCode(OrgAddress.OA_Dock_Height); }
		}

		public ZString FurtherConstraints
		{
			get { return OrgAddress.OA_LoadingUnloadingConstraints; }
		}
		#endregion

		public DocOrgAddressCapapabilityCollection AddressCapability
		{
			get
			{
				return new DocOrgAddressCapapabilityCollection(OrgAddress.AddressCapability, Factory);
			}
		}

		public DocOrganisation Organisation
		{
			get { return DocOrganisation.New(OrgAddress.Header, Factory); }
		}

		public DocCountry Country
		{
			get
			{
				DocCountry result = null;

				if (OrgAddress.RelatedCountry != null)
				{
					result = DocCountry.New(OrgAddress.RelatedCountry, Factory);
				}

				return result;
			}
		}

		public override string ToString()
		{
			return PostalAddress;
		}

		public ZBool IsActive
		{
			get { return OrgAddress.OA_IsActive; }
		}

		public DocCusCodeCollection CustomCodes
		{
			get { return (OrgAddress == null || OrgAddress.CustomsCodes == null) ? new DocCusCodeCollection(new OrgCusCodeCollection(null, Factory), Factory) : new DocCusCodeCollection(OrgAddress.CustomsCodes, Factory); }
		}

		#region Spilt Address

		public ZString SpiltAddress1
		{
			get
			{
				SpiltPostalAddress();
				return fSpiltAddress1;
			}
		}

		public ZString SpiltAddress2
		{
			get
			{
				SpiltPostalAddress();
				return fSpiltAddress2;
			}
		}

		protected ZString fSpiltAddress1;
		protected ZString fSpiltAddress2;

		protected void SpiltPostalAddress()
		{
			if (fSpiltAddress1.IsEmpty && fSpiltAddress2.IsEmpty)
			{
				ZString unformattedPostalAddress = PostalAddress.Replace("\n", " ");
				ZString[] splitFormattedAddress = WrapTextForAColumn(unformattedPostalAddress, 30).Split('\n');

				if (splitFormattedAddress.Length > 0)
				{
					fSpiltAddress1 = splitFormattedAddress[0];
				}

				if (splitFormattedAddress.Length > 1)
				{
					ZString[] spiltAddressElementTwo = WrapTextForAColumn(splitFormattedAddress[1], 25).Split('\n');
					if (spiltAddressElementTwo.Length > 0)
					{
						fSpiltAddress2 = spiltAddressElementTwo[0];
					}
				}
			}
		}

		#endregion

		#region Implementation

		bool IsOrgDepotOrContainerPark
		{
			get { return (Organisation != null && (Organisation.IsPackDepot || Organisation.IsUnpackDepot || Organisation.IsContainerPark)); }
		}

		#endregion

		public OrgAddress OrgAddress { get; private set; }
	}
}
