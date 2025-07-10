using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class Organisation : IOrganization
	{
		[Obsolete("This constructor is to be used only when the object is deserialised from xml.", true)]
		public Organisation()
		{
			Role = RoleType.None;
		}

		public Organisation(RoleType role)
		{
			Role = role;
		}

		public RoleType Role { get; set; }
		public string CompanyName { get; set; }
		public string RepresentativeName { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string Postcode { get; set; }
		[OrganisationIdentificationType(IdentificationType.RoadNameCode)]
		public string RoadNameCode { get; set; }
		[OrganisationIdentificationType(IdentificationType.BuildingNumber)]
		public string BuildingNumber { get; set; }
		public string CountryCode { get; set; }
		public string PhoneNumber { get; set; }
		public string ExtensionNumber { get; set; }
		public string Email { get; set; }
		public string MobileNumber { get; set; }
		public string FaxNumber { get; set; }
		public bool IsIndividual { get; set; }
		[OrganisationIdentificationType(IdentificationType.BusinessRegNo)]
		public string BusinessRegNo { get; set; }
		[OrganisationIdentificationType(IdentificationType.UnipassIDForOrganization)]
		public string UnipassIDForOrganization { get; set; }
		[OrganisationIdentificationType(IdentificationType.OfficeID)]
		public string OfficeID { get; set; }
		[OrganisationIdentificationType(IdentificationType.ForeignCompanyID)]
		public string ForeignCompanyID { get; set; }
		[OrganisationIdentificationType(IdentificationType.PassportNo)]
		public string PassportNo { get; set; }
		[OrganisationIdentificationType(MasterFiles.Business.OrgCusCode.CodeTypes.ControlledPremisesID)]
		public string ControlledPremisesID { get; set; }
		[OrganisationIdentificationType(MasterFiles.Business.OrgCusCode.CodeTypes.CarrierCode)]
		public string CarrierCode { get; set; }
		[OrganisationIdentificationType(IdentificationType.KoreanRegNoForResident)]
		public string KoreanRegNoForResident { get; set; }
		[OrganisationIdentificationType(IdentificationType.KoreanRegNoForForeigner)]
		public string KoreanRegNoForForeigner { get; set; }
		[OrganisationIdentificationType(IdentificationType.UnipassIDForIndividual)]
		public string UnipassIDForIndividual { get; set; }
		[OrganisationIdentificationType(IdentificationType.CertificateOfOriginExporterNumber)]
		public string CertificateOfOriginExporterNumber { get; set; }
		[OrganisationIdentificationType(IdentificationType.CorporationCode)]
		public string CorporationCode { get; set; }
		[OrganisationIdentificationType(IdentificationType.ECommerceCompanyID)]
		public string ECommerceCompanyID { get; set; }

		RoleType IOrganization.Role => Role;
		ZString IOrganization.CompanyName => CompanyName;
		ZString IOrganization.RepresentativeName => RepresentativeName;
		ZString IOrganization.AddressLine1 => AddressLine1;
		ZString IOrganization.AddressLine2 => AddressLine2;
		ZString IOrganization.Postcode => Postcode;
		ZString IOrganization.RoadNameCode => RoadNameCode;
		ZString IOrganization.BuildingNumber => BuildingNumber;
		ZString IOrganization.CountryCode => CountryCode;
		ZString IOrganization.PhoneNumber => PhoneNumber;
		ZString IOrganization.ExtensionNumber => ExtensionNumber;
		ZString IOrganization.Email => Email;
		ZString IOrganization.MobileNumber => MobileNumber;
		ZString IOrganization.FaxNumber => FaxNumber;
		ZBool IOrganization.IsIndividual => IsIndividual;
		ZString IOrganization.BusinessRegNo => BusinessRegNo;
		ZString IOrganization.KoreanRegNoForResident => KoreanRegNoForResident;
		ZString IOrganization.UnipassIDForOrganization => UnipassIDForOrganization;
		ZString IOrganization.ForeignCompanyID => ForeignCompanyID;
		ZString IOrganization.OfficeID => OfficeID;
		ZString IOrganization.KoreanRegNoForForeigner => KoreanRegNoForForeigner;
		ZString IOrganization.PassportNo => PassportNo;
		ZString IOrganization.UnipassIDForIndividual => UnipassIDForIndividual;
		ZString IOrganization.CertificateOfOriginExporterNumber => CertificateOfOriginExporterNumber;
		ZString IOrganization.CorporationCode => CorporationCode;
		ZString IOrganization.CarrierCode => CarrierCode;
		ZString IOrganization.ECommerceCompanyID => ECommerceCompanyID;
	}
}
