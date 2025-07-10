using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT
{
	public class TemporaryOrganisationCreator
	{
		public TemporaryOrganisationCreator(BusinessObjectFactory factory)
		{
			this.Factory = factory;
			Converter = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;
		}

		public OrgHeader Create(TNTOrganisation organisation, INotifications notify)
		{
			OrgHeader result = null;
			if (organisation.IsValid)
			{
				result = (OrgHeader)Factory.New(typeof(OrgHeader));
				Converter.SetPropertyInfoValue(result.OH_FullNameInfo, organisation.CompanyName, notify);
				result.OH_IsTempAccount = true;
				Converter.SetPropertyInfoValue(result.OH_RL_NKClosestPortInfo, GetPortCode(organisation.Country, organisation.State, organisation.City), notify);
				if (organisation.Address1.IsEmpty)
				{
					Converter.SetPropertyInfoValue(result.MainAddress.OA_Address1Info, GetDefaultAddressIfBlank(organisation.Address2), notify);
				}
				else
				{
					Converter.SetPropertyInfoValue(result.MainAddress.OA_Address1Info, organisation.Address1, notify);
					Converter.SetPropertyInfoValue(result.MainAddress.OA_Address2Info, organisation.Address2, notify);
				}
				Converter.SetPropertyInfoValue(result.MainAddress.OA_CityInfo, organisation.City, notify);
				if (result.MainAddress.OA_State.IsEmpty)
				{
					Converter.SetPropertyInfoValue(result.MainAddress.OA_StateInfo, organisation.State, notify);
				}
				Converter.SetPropertyInfoValue(result.MainAddress.OA_PostCodeInfo, organisation.PostCode, notify);
				Converter.SetPropertyInfoValue(result.MainAddress.OA_PhoneInfo, organisation.Phone, notify);
				Converter.SetPropertyInfoValue(result.MainAddress.OA_FaxInfo, organisation.Fax, notify);
				Converter.SetPropertyInfoValue(result.MainAddress.OA_EmailInfo, organisation.Email, notify);

				if (result.OH_Code.IsEmpty)
				{
					result.OH_Code = OrgHeaderMappingUtils.GetCompanyCode(result);
				}
			}

			return result;
		}

		public OrgContact AddContact(OrgHeader organisation, ZString contactName, ZString contactPhone, INotifications notify)
		{
			OrgContact newContact = null;
			if (organisation != null && !contactName.IsEmpty)
			{
				OrgContact[] contactsMatched = (OrgContact[])organisation.Contacts.Find(new ZQuery(OrgContactSchema.OC_ContactName, contactName));
				if (contactsMatched.Length == 0)
				{
					newContact = organisation.Contacts.AddNew();
					Converter.SetPropertyInfoValue(newContact.OC_ContactNameInfo, contactName, notify);
					newContact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Print;
				}
				else
				{
					newContact = contactsMatched[0];
				}

				if (!contactPhone.IsEmpty)
				{
					Converter.SetPropertyInfoValue(newContact.OC_PhoneInfo, contactPhone, notify);
				}
			}

			return newContact;
		}

		public OrgHeader CreateConsignor(TNTOrganisation organisation, INotifications notify)
		{
			OrgHeader result = Create(organisation, notify);
			if (result != null)
			{
				result.OH_IsConsignor = true;
				AddConsignorContact(result, organisation.ContactName, organisation.ContactPhone, notify);
			}
			return result;
		}

		public OrgContact AddConsignorContact(OrgHeader consignor, ZString contactName, ZString contactPhone, INotifications notify)
		{
			OrgContact newContact = AddContact(consignor, contactName, contactPhone, notify);
			AddDocumentGroup(newContact, ContactType.Consignor.Code);
			return newContact;
		}

		public OrgHeader CreateConsignee(TNTOrganisation organisation, INotifications notify)
		{
			OrgHeader result = Create(organisation, notify);
			if (result != null)
			{
				result.OH_IsConsignee = true;
				AddConsigneeContact(result, organisation.ContactName, organisation.ContactPhone, notify);
			}
			return result;
		}

		public OrgContact AddConsigneeContact(OrgHeader consignee, ZString contactName, ZString contactPhone, INotifications notify)
		{
			OrgContact newContact = AddContact(consignee, contactName, contactPhone, notify);
			AddDocumentGroup(newContact, ContactType.Consignee.Code);
			return newContact;
		}

		public void UpdateOrgAddress(OrgAddress address, TNTOrganisation organisation, INotifications notify)
		{
			if (organisation.IsValid)
			{
				Converter.SetPropertyInfoValue(address.OA_CompanyNameOverrideInfo, organisation.CompanyName, notify);
				Converter.SetPropertyInfoValue(address.OA_Address1Info, organisation.Address1, notify);
				Converter.SetPropertyInfoValue(address.OA_Address2Info, organisation.Address2, notify);
				Converter.SetPropertyInfoValue(address.OA_CityInfo, organisation.City, notify);
				Converter.SetPropertyInfoValue(address.OA_PostCodeInfo, organisation.PostCode, notify);
				address.OA_RL_NKRelatedPortCode = GetPortCode(organisation.Country, organisation.State, organisation.City);
				if (address.OA_State.IsEmpty)
				{
					Converter.SetPropertyInfoValue(address.OA_StateInfo, organisation.State, notify);
				}
				Converter.SetPropertyInfoValue(address.OA_PhoneInfo, organisation.Phone, notify);
				Converter.SetPropertyInfoValue(address.OA_FaxInfo, organisation.Fax, notify);
				Converter.SetPropertyInfoValue(address.OA_EmailInfo, organisation.Email, notify);
				Converter.SetPropertyInfoValue(address.OA_LanguageInfo, organisation.Language, notify);
			}
		}

		#region GetPortCode

		public ZString GetPortCode(ZString country, ZString state, ZString portName)
		{
			ZString result = DefaultEmptyPortCode;
			ZString trimmedCountryCode = country.Trim();

			if (trimmedCountryCode == Core.Constants.CountryCodes.Australia)
			{
				result = UnlocoMappingUtils.GetPortCodeFromAustralianState(state.Trim());
			}

			if (result == DefaultEmptyPortCode)
			{
				result = GetPortFromNameAndCountryCode(portName.Trim(), trimmedCountryCode);
			}

			if (result == DefaultEmptyPortCode)
			{
				result = UnlocoMappingUtils.GetPortCodeFromCountry(trimmedCountryCode);
			}

			return result.IsEmpty ? new ZString(DefaultEmptyPortCode) : result;
		}

		internal const string DefaultEmptyPortCode = "ZZZZZ";

		#endregion

		#region GetPortFromNameAndCountryCode

		public ZString GetPortFromNameAndCountryCode(ZString portName, ZString country)
		{
			ZString result = DefaultEmptyPortCode;

			RefUNLOCO locode = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, portName.Trim(), country.Trim());
			if (locode != null)
			{
				if (locode.Code.IsValid)
				{
					result = locode.Code;
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		ZString GetDefaultAddressIfBlank(ZString address)
		{
			return address.IsEmpty ? new ZString(DefaultEmptyAddress) : address;
		}

		internal const string DefaultEmptyAddress = "NOT SUPPLIED";

		void AddDocumentGroup(OrgContact contact, ZString groupCode)
		{
			if (contact != null)
			{
				if (contact.Documents.Count == 0 || contact.Documents.Find(new ZQuery(OrgDocumentSchema.OD_DocumentGroup, groupCode)).Length == 0)
				{
					contact.Documents.AddNew();
					contact.Documents[0].OD_DocumentGroup = groupCode;
				}
			}
		}

		public readonly BusinessObjectFactory Factory;
		protected readonly StringToBusinessObjectFieldConverter Converter;

		#endregion
	}
}
