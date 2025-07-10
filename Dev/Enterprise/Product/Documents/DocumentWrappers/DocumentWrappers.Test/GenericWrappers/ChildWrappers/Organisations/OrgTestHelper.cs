using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public static class OrgTestHelper
	{
		public static OrgHeader GetNewOrganisation(ZString dataSeed, BusinessObjectFactory factory)
		{
			return GetNewOrganisation(dataSeed, dataSeed + "_NAME"
					, dataSeed + "_ADDRESS1", dataSeed + "_ADDRESS2", "ZZ", dataSeed + "_CITY"
					, dataSeed.Left(7) + "_PC", dataSeed + "_S", "ZZ" + dataSeed.Left(3)
					, dataSeed + "_P", dataSeed + "_F", dataSeed + "_E", dataSeed + "_M", "", factory);
		}

		public static OrgHeader GetNewOrganisation(string orgCode, string fullName,
				string address1, string address2, string countryCode, string city, string postCode, string state, string portCode,
				string phone, string fax, string email, string mobile, string addressCode, BusinessObjectFactory factory)
		{
			OrgHeader organisation = factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			organisation.OH_FullName = fullName;
			organisation.OH_RL_NKClosestPort = portCode;
			SetupNewAddress(organisation.MainAddress, address1, address2, countryCode, city, postCode, state, portCode, phone, fax, email, mobile, addressCode);
			organisation.OH_Code = orgCode;
			return organisation;
		}

		static void SetupNewAddress(OrgAddress address, string address1, string address2, string countryCode, string city, string postCode, string state, string portCode, string phone, string fax, string email, string mobile, string code)
		{
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_RN_NKCountryCode = countryCode;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_RL_NKRelatedPortCode = portCode;
			address.OA_State = state;
			address.OA_Phone = phone;
			address.OA_Fax = fax;
			address.OA_Email = email;
			address.OA_Mobile = mobile;
			address.OA_Code = code;
		}

		public static OrgAddress AddNewAddress(OrgHeader organisation, OrgAddressType type, ZString dataSeed)
		{
			return AddNewAddress(organisation, type, true, dataSeed + "_ADDRESS1", dataSeed + "_ADDRESS2", "ZZ", dataSeed + "_CITY"
					, dataSeed.Left(7) + "_PC", dataSeed + "_S", "ZZ" + dataSeed.Left(3)
					, dataSeed + "_P", dataSeed + "_F", dataSeed + "_E", dataSeed + "_M");
		}

		public static OrgAddress AddNewAddress(OrgHeader organisation, OrgAddressType type, bool isDefault, string address1, string address2, string countryCode, string city, string postCode, string state, string portCode, string phone, string fax, string email, string mobile)
		{
			OrgAddress address = organisation.Addresses.AddNew(type, isDefault);
			SetupNewAddress(address, address1, address2, countryCode, city, postCode, state, portCode, phone, fax, email, mobile, "");
			return address;
		}

		public static OrgContact AddNewContact(OrgHeader organisation, ContactType contactType, ZString dataSeed)
		{
			OrgContact contact = AddNewContact(organisation, dataSeed + "_NAME", dataSeed + "_P", dataSeed + "_F", dataSeed + "_E", dataSeed + "_M", dataSeed + "_H", dataSeed + "_S", dataSeed + "_T", dataSeed + "_X", dataSeed + "_J", dataSeed + "_O", dataSeed + "_G");
			AddNewDocumentType(contact, contactType, true);
			return contact;
		}

		public static OrgContact AddNewContact(OrgHeader organisation, string name, string phone, string fax, string email, string mobile, string homePhone
			, string salutation, string title, string extension, string jobCategory, string otherPhone, string pager)
		{
			OrgContact contact = organisation.Contacts.AddNew();

			contact.OC_ContactName = name;
			contact.OC_Phone = phone;
			contact.OC_Fax = fax;
			contact.OC_Email = email;
			contact.OC_Mobile = mobile;
			contact.OC_HomePhone = homePhone;
			contact.OC_PhoneExtension = extension;
			contact.OC_OtherPhone = otherPhone;
			contact.OC_Pager = pager;

			contact.OC_Salutation = salutation;
			contact.OC_Title = title;
			contact.OC_JobCategory = jobCategory;

			contact.OC_AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.OC_NotifyMode = Constants.ContactNotifyModes.Email;

			return contact;
		}

		public static OrgDocument AddNewDocumentType(OrgContact contact, ContactType documentType, bool isDefaultForType)
		{
			OrgDocument document = contact.Documents.AddNew();
			document.OD_DocumentGroup = documentType.ToString();
			document.OD_DefaultContact = isDefaultForType;
			document.OD_AttachmentType = contact.OC_AttachmentType;
			document.OD_DeliverBy = contact.OC_NotifyMode;
			return document;
		}
	}
}
