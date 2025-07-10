using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationAddressProvider : IDeclarationAddress
	{
		public DeclarationAddressProvider(OrgAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}

		public static DeclarationAddressProvider New(OrgAddress address) => address == null ? null : new DeclarationAddressProvider(address);

		readonly OrgAddress address;

		public string Name => address.CompanyName;

		public string Address => address.StreetNumber.IsNullOrEmpty() ? address.Address1.ToString() : address.Street;

		public string AddressNumber => address.StreetNumber.IsNullOrEmpty() ? "0" : address.StreetNumber;

		public string AddressComplementary => address.OA_AdditionalAddressInformation;

		public string CityName => address.City;

		public string AddressStateCode => address.StateCode;

		public string AddressState => address.State;

		public string CountryCode => BRRefCusMapper.MapCW1CountryCodeToCustomsCode(address.Factory, address.OA_RN_NKCountryCode);
	}
}
