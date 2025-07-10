using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business
{
	public class AddressProvider : IAddress
	{
		public static AddressProvider New(IDocAddress address) => address == null ? null : new AddressProvider(address);

		protected AddressProvider(IDocAddress address)
		{
			this.address = address;
		}
		protected readonly IDocAddress address;

		public string City => address.E2_City;

		public string Country => address.E2_RN_NKCountryCode;

		public string StreetAndNumber => MessageProviderHelper.GetDocAddressLine(address);

		public virtual string Postcode => address.E2_Postcode;
	}
}
