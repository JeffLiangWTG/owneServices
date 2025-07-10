using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class OrganisationValidation
	{
		public static OrgContactAllocation GetCAPContactAllocation(OrgHeader organization)
		{
			return organization != null ? organization.Contacts.Cast<OrgContact>().SelectMany(c => c.Allocations).Cast<OrgContactAllocation>().FirstOrDefault(a => a.Contact.OC_IsActive && a.PC_Type == OrgConstants.ContactAllocationType.CAPGA) : null;
		}

		public static void ValidateCity(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null && address.OA_City.IsEmpty)
			{
				propertyInfo.AddMessageError(AddressLackOfCity);
			}
		}

		public static void ValidateCountry(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null)
			{
				if (address.OA_RN_NKCountryCode.IsEmpty || address.OA_RN_NKCountryCode == "ZZ")
				{
					propertyInfo.AddMessageError(AddressHasInvalidCountryCode);
				}
			}
		}

		public static void ValidateNamesAndAddressesSpecial(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null)
			{
				var organisation = address.Header;
				if (organisation != null)
				{
					if (organisation.OH_FullName.HasCharactersNotSupportedByCAMessaging())
					{
						propertyInfo.AddWarning(NameHasCharactersNotSupportedByCAMessaging);
					}

					if (address.Address1.HasCharactersNotSupportedByCAMessaging() || address.Address2.HasCharactersNotSupportedByCAMessaging())
					{
						propertyInfo.AddWarning(AddressHasCharactersNotSupportedByCAMessaging);
					}
				}
			}
		}

		public static void ValidateNamesAndAddressesSpecial(ZPropertyInfo propertyInfo, OrgHeader organisation)
		{
			if (organisation != null && !organisation.IsDeleted)
			{
				if (organisation.OH_FullName.HasCharactersNotSupportedByCAMessaging())
				{
					propertyInfo.AddWarning(NameHasCharactersNotSupportedByCAMessaging);
				}

				if (organisation.MainAddress.Address1.HasCharactersNotSupportedByCAMessaging() || organisation.MainAddress.Address2.HasCharactersNotSupportedByCAMessaging())
				{
					propertyInfo.AddWarning(AddressHasCharactersNotSupportedByCAMessaging);
				}
			}
		}

		public static void ValidateCAPContactOrAddressPhone(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null)
			{
				var organisation = address.Header;
				if (organisation != null)
				{
					var capContactAllocation = GetCAPContactAllocation(organisation);
					if (capContactAllocation != null)
					{
						ValidateOrgContactPhone(capContactAllocation.Contact, propertyInfo, ContactPhoneNumberIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
					else
					{
						ValidateAddressPhone(address, propertyInfo, AddressWorkPhoneIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
				}
			}
		}

		public static void ValidateCAPContactOrAddressPhoneAndEmailRequired(ZPropertyInfo propertyInfo, JobDocAddress address)
		{
			if (address != null && address.Address != null)
			{
				ValidateCAPContactOrAddressPhoneAndEmailRequired(propertyInfo, address.Address);
			}
		}

		public static void ValidateCAPContactOrAddressPhoneAndEmailRequired(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null)
			{
				var organisation = address.Header;
				if (organisation != null)
				{
					var capContactAllocation = GetCAPContactAllocation(organisation);
					if (capContactAllocation != null)
					{
						ValidateOrgContactPhone(capContactAllocation.Contact, propertyInfo, ContactPhoneNumberIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
						ValidateOrgContactEmail(capContactAllocation.Contact, propertyInfo, ContactEmailIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
					else
					{
						ValidateAddressPhone(address, propertyInfo, AddressWorkPhoneIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
						ValidateAddressEmail(address, propertyInfo, AddressEmailIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
				}
			}
		}

		public static void ValidateCAPContactOrAddressPhoneAndEmailRecommened(ZPropertyInfo propertyInfo, JobDocAddress address)
		{
			if (address != null && address.Address != null)
			{
				ValidateCAPContactOrAddressPhoneAndEmailRecommened(propertyInfo, address.Address);
			}
		}

		public static void ValidateCAPContactOrAddressPhoneAndEmailRecommened(ZPropertyInfo propertyInfo, OrgAddress address)
		{
			if (address != null)
			{
				var organisation = address.Header;
				if (organisation != null)
				{
					var capContactAllocation = GetCAPContactAllocation(organisation);
					if (capContactAllocation != null)
					{
						ValidateOrgContactPhone(capContactAllocation.Contact, propertyInfo, ContactPhoneNumberIsRecommended, CargoWise.ComponentModel.NotificationType.Warning);
						ValidateOrgContactEmail(capContactAllocation.Contact, propertyInfo, ContactEmailIsRecommended, CargoWise.ComponentModel.NotificationType.Warning);
					}
					else
					{
						ValidateAddressPhone(address, propertyInfo, ContactPhoneNumberIsRecommended, CargoWise.EntityFramework.NotificationType.Warning);
						ValidateAddressEmail(address, propertyInfo, ContactEmailIsRecommended, CargoWise.EntityFramework.NotificationType.Warning);
					}
				}
			}
		}

		public static void ValidateOrgContactPhone(OrgHeader organisation, ZPropertyInfo propertyInfo)
		{
			var capAllocation = OrganisationValidation.GetCAPContactAllocation(organisation);
			if (capAllocation != null)
			{
				ValidateOrgContactPhone(capAllocation.Contact, propertyInfo, ContactPhoneNumberIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
			}
		}

		public static void ValidateOrgContactPhone(OrgContact contact, ZPropertyInfo propertyInfo, ZString message, INotificationType notificationType)
		{
			ValidateProperty(() => contact != null && contact.OC_Phone.IsEmpty && contact.OC_Mobile.IsEmpty, propertyInfo, message, notificationType);
		}

		public static void ValidateAddressPhone(OrgAddress address, ZPropertyInfo propertyInfo, ZString message, INotificationType notificationType)
		{
			ValidateProperty(() => address != null && address.OA_Phone.IsEmpty, propertyInfo, message, notificationType);
		}

		public static void ValidateAddressEmail(OrgAddress address, ZPropertyInfo propertyInfo, ZString message, INotificationType notificationType)
		{
			ValidateProperty(() => address != null && address.OA_Email.IsEmpty, propertyInfo, message, notificationType);
		}

		static void ValidateOrgContactEmail(OrgContact contact, ZPropertyInfo propertyInfo, string message, INotificationType notificationType)
		{
			ValidateProperty(() => contact != null && contact.OC_Email.IsEmpty, propertyInfo, message, notificationType);
		}

		static void ValidateProperty(Func<bool> condition, ZPropertyInfo propertyInfo, string message, INotificationType notificationType)
		{
			if (condition())
			{
				propertyInfo.AddNotification(notificationType, message);
			}
		}

		public static void ValidateCAPAllocatedContact(OrgHeader organization, ZPropertyInfo propertyInfo, string pgasRequiringContact, bool needCheckEmail = false, bool needCheckPhone = false)
		{
			if (organization != null)
			{
				var capAllocation = OrganisationValidation.GetCAPContactAllocation(organization);
				if (capAllocation == null)
				{
					propertyInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, CAPAllocationIsRequired, pgasRequiringContact));
				}
				else
				{
					if (needCheckEmail)
					{
						ValidateOrgContactEmail(capAllocation.Contact, propertyInfo, ContactEmailIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
					if (needCheckPhone)
					{
						ValidateOrgContactPhone(capAllocation.Contact, propertyInfo, ContactPhoneNumberIsRequired, CargoWise.EntityFramework.NotificationType.MessageError);
					}
				}
			}
		}

		internal static string AddressEmailIsRequired => Res.GetString("2599614F-1737-4CC7-BEE4-BCAFD90AFC41", "Email is required for the Organization Address.");
		internal static string AddressWorkPhoneIsRequired => Res.GetString("94674094-FE51-4EF4-8539-637CEF5CDF70", "Work Phone is required for the Organization Address.");
		internal static string ContactPhoneNumberIsRecommended => Res.GetString("05C3FCD1-70AA-4D7E-92ED-AB48C8112205", "No contact telephone number was found for the PGA contact. A contact telephone number is highly recommended to facilitate communications in case of a referral.");
		internal static string ContactEmailIsRecommended => Res.GetString("F75F4C51-A744-4E4D-AC94-A1AACCCCB4B2", "No email address was found for the PGA contact. An email address is highly recommended to facilitate communications in case of a referral.");
		internal static string CAPAllocationIsRequired => Res.GetString("6BBD0D64-50A2-4D9A-9A37-DB4637A642C3", "Declaration contains lines with PGA(s): {0}. There must be a PGA contact listed against the Importer. PGA contacts are added to organizations under Organization -> Contacts -> Allocated Contact.");
		internal static string ContactNameIsRequired => Res.GetString("ADDE1C51-A68E-4BB2-9A1E-2572E22A56F1", "Contact for Canada Customs/PGA’s name is required.");
		internal static string ContactPhoneNumberIsRequired => Res.GetString("AEFA7882-0635-4F5C-AED9-AF2F688DACC3", "Contact for Canada Customs/PGA’s Phone number is required.");
		internal static string ContactEmailIsRequired => Res.GetString("5998EC5D-7758-4C27-B22C-BC9DCE8519EC", "Contact for Canada Customs/PGA’s e-mail is required.");
		internal static string AuthorizedPartyContactNameIsRequired => Res.GetString("D9B244B4-D062-4143-AFEE-B53F340D4C62", "Contact for Authorized Party’s name is required.");
		internal static string AuthorizedPartyContactPhoneNumberIsRequired => Res.GetString("A9D403D9-DDD8-468B-83E0-67A55043130B", "Contact for Authorized Party’s Phone number is required.");
		internal static string AuthorizedPartyContactEmailIsRequired => Res.GetString("4DB4B198-CC18-4608-A194-60E4C83305DB", "Contact for Authorized Party's e-mail is required.");
		internal static string ApplicantContactNameIsRequired => Res.GetString("06375E56-E1C8-4A44-A0FC-0E895CB600E7", "Contact for Applicant’s name is required.");
		internal static string ApplicantContactPhoneNumberIsRequired => Res.GetString("56363DC4-C3E4-4F18-A4FE-6717CF1DCC53", "Contact for Applicant’s Phone number is required.");
		internal static string ApplicantContactEmailIsRequired => Res.GetString("D9D2F7F5-C113-46B6-B788-B5886928F2C3", "Contact for Applicant's e-mail is required.");
		internal static string NameHasCharactersNotSupportedByCAMessaging => Res.GetString("a82129ac-8ae3-4e43-83e5-61add843784f", "The Name of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		internal static string AddressHasCharactersNotSupportedByCAMessaging => Res.GetString("a12de80a-d67a-414c-956a-d0d33ad0a0e4", "The Address of the Organization has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		internal static string AddressLackOfCity => Res.GetString("1425E7ED-4040-44D8-9697-4DCBBD3A3FE6", "The address does not contain city.");
		internal static string AddressHasInvalidCountryCode => Res.GetString("1B186802-9ABE-4964-BA9A-BA2EF314434C", "The address does not have a valid country/region code.");

		public static ZString GetCABusinessNumber(this OrgHeader header)
		{
			return GetCustomsRegNo(header, OrgCusCode.CACodeTypes.BusinessNumberForImportExport);
		}

		public static ZString GetCustomsRegNo(this OrgHeader orgHeader, params ZString[] codeTypes)
		{
			return IB3HeaderHelper.GetCustomsRegNo(orgHeader, codeTypes);
		}

		public static ZString GetCABrokerBusinessNumber(this OrgHeader header)
		{
			return IB3HeaderHelper.GetBrokerBusinessNumber(header);
		}
	}
}
