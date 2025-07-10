using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IJobDocAddress : IBusiness, IEquatable<IJobDocAddress>
	{
		ZGuid PK { get; }
		ZString E2_Address1 { get; set; }
		ZString E2_Address2 { get; set; }
		ZBool E2_AddressOverride { get; set; }
		ZByte E2_AddressSequence { get; set; }
		ZString E2_AddressType { get; set; }
		ZString E2_City { get; set; }
		ZString E2_CompanyName { get; set; }
		ZString E2_Contact { get; set; }
		ZString E2_Email { get; set; }
		ZString E2_Fax { get; set; }
		ZString E2_GovRegNum { get; set; }
		ZString E2_GovRegNumType { get; set; }
		ZBool E2_IsResidential { get; set; }
		ZString E2_Mobile { get; set; }
		ZGuid E2_OA_Address { get; set; }
		ZGuid E2_ParentID { get; set; }
		ZString E2_ParentTableCode { get; set; }
		ZString E2_Phone { get; set; }
		ZString E2_Postcode { get; set; }
		ZString E2_RN_NKCountryCode { get; set; }
		ZString E2_ScreeningStatus { get; set; }
		ZString E2_State { get; set; }
		ZString E2_ValidationStatus { get; set; }
		ZGuid OrganisationPK { get; set; }
		ZString AddressFull { get; }
		ZString E2_AddressMap { get; set; }
		ZGeography E2_GeoLocation { get; set; }
		string DocAddressCollectionInfo { get; }
	}
}
