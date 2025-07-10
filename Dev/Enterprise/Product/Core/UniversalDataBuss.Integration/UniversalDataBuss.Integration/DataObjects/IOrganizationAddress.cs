using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IOrganizationAddress
	{
		ZString? AdditionalAddressInformation { get; set; }
		ZString? Address1 { get; set; }
		ZString? Address2 { get; set; }
		ZBool? AddressOverride { get; set; }
		ZString? AddressShortCode { get; set; }
		ZString? AddressType { get; set; }
		ZString? City { get; set; }
		ICodeNameDataObject Country { get; }
		ZString? CompanyName { get; set; }
		ZString? Contact { get; set; }
		ZString? Email { get; set; }
		ZString? Fax { get; set; }
		ZString? GovRegNum { get; set; }
		ZString? Mobile { get; set; }
		ZString? OrganizationCode { get; set; }
		ZString? OrganizationCategory { get; set; }
		ZString? Phone { get; set; }
		ZString? Postcode { get; set; }
		ZString? State { get; set; }
		ZString? UniversalNettingCode { get; set; }
		ZString? UniversalOfficeCode { get; set; }
	}
}
