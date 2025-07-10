using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationImportOrganizationProvider : IDeclarationOrganization
	{
		public static DeclarationImportOrganizationProvider New(OrgHeader organization) => organization == null ? null : new DeclarationImportOrganizationProvider(organization);

		public DeclarationImportOrganizationProvider(OrgHeader organization)
		{
			this.organization = Argument.NotNull(organization, nameof(organization));
		}

		readonly OrgHeader organization;

		public string Name => organization.OH_FullName;

		public string ID => organization.GetCNPJOrCPF();

		public string ZipCode => organization.MainAddress.OA_PostCode.KeepAlphanumericCharacters();

		public string CountryCode => BRRefCusMapper.MapCW1CountryCodeToCustomsCode(organization.Factory, organization.MainAddress?.Country?.Code ?? string.Empty);

		public string AddressComplementary => organization.MainAddress.UnrestrictedAdditionalAddressInformation;

		public string Address => organization.MainAddress.OA_Address1;

		public string CityName => organization.MainAddress.OA_City;

		public string AddressNumber => string.Empty;

		public string AddressStateCode => organization.MainAddress.OA_State;

		public string PhoneNumber => organization.MainAddress.OA_Phone.KeepNumericCharacters();
	}
}

