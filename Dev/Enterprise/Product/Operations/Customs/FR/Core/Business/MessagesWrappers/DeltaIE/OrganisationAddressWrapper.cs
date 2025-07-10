using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class OrganisationAddressWrapper : IAddress
	{
		OrganisationAddressWrapper(OrgAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly OrgAddress address;

		public string City => city ?? (city = address.City);
		string city;

		public string Country => country ?? (country = address.OA_RN_NKCountryCode);
		string country;

		public string Postcode => postcode ?? (postcode = address.OA_PostCode);
		string postcode;

		public string StreetAndNumber => streetAndNumber ?? (streetAndNumber = $"{address.OA_Address1}, {address.OA_Address2}");
		string streetAndNumber;

		public static OrganisationAddressWrapper New(OrgAddress address) => address == null ? null : new OrganisationAddressWrapper(address);
	}
}
