using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public static class OrganizationDocWrapperDecorator
	{
		public static void Decorate(this OrganizationDocWrapper wrapper, IOrganization organization)
		{
			wrapper.CompanyName = organization.CompanyName;
			wrapper.RepresentativeName = organization.RepresentativeName;
			wrapper.IsIndividual = organization.IsIndividual;
			wrapper.Postcode = organization.Postcode;
			wrapper.FormattedPostcode = MessageFunctions.GetFormattedNumber(organization.Postcode, new int[] { 0, 3 });
			wrapper.AddressLine1 = organization.AddressLine1;
			wrapper.AddressLine2 = organization.AddressLine2;
			wrapper.AddressDetails = organization.GetAddressDetails();
			wrapper.RoadNameCode = organization.RoadNameCode;
			wrapper.BuildingNumber = organization.BuildingNumber;
			wrapper.CountryCode = organization.CountryCode;
			wrapper.PhoneNumber = organization.PhoneNumber;
			wrapper.ExtensionNumber = organization.ExtensionNumber;
			wrapper.Email = organization.Email;
			wrapper.MobileNumber = organization.MobileNumber;
			wrapper.FaxNumber = organization.FaxNumber;
			wrapper.BusinessRegNo = organization.BusinessRegNo;
			wrapper.KoreanRegNoForResident = organization.KoreanRegNoForResident;
			wrapper.BuyerID = organization.ForeignCompanyID;
			wrapper.OfficeID = organization.OfficeID;
			wrapper.BusinessRegNoOrIndividualID = organization.GetBusinessOrIndividualRegistrationNumber()?.Number ?? ZString.Empty;
			wrapper.KoreanRegNoForForeigner = organization.KoreanRegNoForForeigner;
			wrapper.PassportNo = organization.PassportNo;
			wrapper.UnipassIDForIndividual = organization.UnipassIDForIndividual;
			wrapper.UnipassIDForOrganization = organization.UnipassIDForOrganization;
			wrapper.CorporationCode = organization.CorporationCode;
			wrapper.CertificateOfOriginExporterNumber = organization.CertificateOfOriginExporterNumber;
		}

		public static void Decorate(this OrganizationDocWrapper wrapper, OrgHeader orgHeader)
		{
			DecorateCoreWithOrgHeader(wrapper, orgHeader);
			DecorateCoreWithAddress(wrapper, orgHeader.MainAddress);
		}

		public static void Decorate(this OrganizationDocWrapper wrapper, OrgAddress address)
		{
			DecorateCoreWithOrgHeader(wrapper, address.Header);
			DecorateCoreWithAddress(wrapper, address);
		}

		static void DecorateCoreWithOrgHeader(this OrganizationDocWrapper wrapper, OrgHeader orgHeader)
		{
			wrapper.CompanyName = orgHeader.OH_FullName;
			wrapper.RepresentativeName = orgHeader.GetRepresentativeName();
			wrapper.IsIndividual = orgHeader.GetIsIndividual();
			wrapper.ExtensionNumber = orgHeader.GetExtensionNumber();
			wrapper.BusinessRegNo = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.BusinessRegNo, CountryCodes.KoreaSouth);
			wrapper.KoreanRegNoForResident = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.KoreanRegNoForResident, CountryCodes.KoreaSouth);
			wrapper.BuyerID = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.ForeignCompanyID, CountryCodes.KoreaSouth);
			wrapper.BusinessRegNoOrIndividualID = orgHeader.GetRegistrationFirstMatchedBusinessOrIndividualID();
			wrapper.KoreanRegNoForForeigner = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.KoreanRegNoForForeigner, CountryCodes.KoreaSouth);
			wrapper.PassportNo = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.PassportNo, CountryCodes.KoreaSouth);
			wrapper.UnipassIDForIndividual = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.UnipassIDForIndividual, CountryCodes.KoreaSouth);
			wrapper.UnipassIDForOrganization = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.UnipassIDForOrganization, CountryCodes.KoreaSouth);
			wrapper.CorporationCode = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.CorporationCode, CountryCodes.KoreaSouth);
			wrapper.CertificateOfOriginExporterNumber = orgHeader.CustomsCodes.GetCustomsRegNo(IdentificationType.CertificateOfOriginExporterNumber, CountryCodes.KoreaSouth);
		}

		static void DecorateCoreWithAddress(this OrganizationDocWrapper wrapper, OrgAddress address)
		{
			wrapper.Postcode = address.Postcode;
			wrapper.FormattedPostcode = address.GetFormattedPostcode();
			wrapper.AddressLine1 = address.Address1;
			wrapper.AddressLine2 = address.Address2;
			wrapper.AddressDetails = address.GetAddressDetails();
			wrapper.CountryCode = address.OA_RN_NKCountryCode;
			wrapper.PhoneNumber = address.OA_Phone;
			wrapper.Email = address.OA_Email;
			wrapper.MobileNumber = address.OA_Mobile;
			wrapper.FaxNumber = address.OA_Fax;
			wrapper.RoadNameCode = address.CustomsCodes.GetCustomsRegNo(IdentificationType.RoadNameCode, CountryCodes.KoreaSouth);
			wrapper.BuildingNumber = address.CustomsCodes.GetCustomsRegNo(IdentificationType.BuildingNumber, CountryCodes.KoreaSouth);
			wrapper.OfficeID = address.CustomsCodes.GetCustomsRegNo(IdentificationType.OfficeID, CountryCodes.KoreaSouth);
		}
	}
}
