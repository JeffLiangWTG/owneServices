using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class OrganizationDocWrapper : NonPersistentBusinessObject
	{
		public OrganizationDocWrapper(IOrganization organisation)
		{
			if (organisation != null)
			{
				this.Decorate(organisation);
			}
		}

		public OrganizationDocWrapper(OrgHeader orgHeader)
		{
			if (orgHeader != null)
			{
				this.Decorate(orgHeader);
			}
		}

		public OrganizationDocWrapper(OrgAddress orgAddress)
		{
			if (orgAddress != null)
			{
				this.Decorate(orgAddress);
			}
		}

		#region Organization Member
		public ZString CompanyName { get; set; }
		public ZString RepresentativeName { get; set; }
		public ZBool IsIndividual { get; set; }
		public ZString Postcode { get; set; }
		public ZString FormattedPostcode { get; set; }
		public ZString AddressLine1 { get; set; }
		public ZString AddressLine2 { get; set; }
		public ZString RoadNameCode { get; set; }
		public ZString BuildingNumber { get; set; }
		public ZString CountryCode { get; set; }
		public ZString PhoneNumber { get; set; }
		public ZString ExtensionNumber { get; set; }
		public ZString Email { get; set; }
		public ZString MobileNumber { get; set; }
		public ZString FaxNumber { get; set; }
		public ZString AddressDetails { get; set; }

		#endregion

		#region ID Numbers

		public ZString BusinessRegNo { get; set; }
		public ZString KoreanRegNoForResident { get; set; }
		public ZString UnipassIDForOrganization { get; set; }
		public ZString BuyerID { get; set; }
		public ZString OfficeID { get; set; }
		public ZString BusinessRegNoOrIndividualID { get; set; }
		public ZString KoreanRegNoForForeigner { get; set; }
		public ZString PassportNo { get; set; }
		public ZString UnipassIDForIndividual { get; set; }
		public ZString CertificateOfOriginExporterNumber { get; set; }
		public ZString CorporationCode { get; set; }
		public ZString FormattedBusinessRegNo => OrgHeaderExtensionMethods.FormatRegistrationIDNumber(BusinessRegNo, IdentificationType.BusinessRegNo);
		public ZString FormattedKoreanRegNoForResident => OrgHeaderExtensionMethods.FormatRegistrationIDNumber(KoreanRegNoForResident, IdentificationType.KoreanRegNoForResident);
		public ZString FormattedUnipassIDForOrganization => OrgHeaderExtensionMethods.FormatRegistrationIDNumber(UnipassIDForOrganization, IdentificationType.UnipassIDForOrganization);
		public ZString FormattedCorporationCode => OrgHeaderExtensionMethods.FormatRegistrationIDNumber(CorporationCode, IdentificationType.CorporationCode);

		#endregion
	}
}
