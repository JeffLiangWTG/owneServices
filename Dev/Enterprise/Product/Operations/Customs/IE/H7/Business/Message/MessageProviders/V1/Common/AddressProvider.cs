using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class AddressProvider : IAddress
	{
		public AddressProvider(IDocAddress address)
		{
			this.address = address;
		}

		protected readonly IDocAddress address;

		public string City => address?.E2_City;

		public string Country => Core.Constants.CountryCodes.Ireland;

		public string StreetAndNumber
		{
			get
			{
				return IE.Business.MessageProviderHelper.GetDocAddressLine(address);
			}
		}

		public string Postcode => address?.E2_Postcode;
	}
}
