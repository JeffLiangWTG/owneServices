using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class PostcodeAddressProvider : IPostcodeAddress
	{
		PostcodeAddressProvider(string houseNumber, string postcode, string country)
		{
			HouseNumber = houseNumber;
			Postcode = postcode;
			Country = country;
		}

		public static PostcodeAddressProvider New(string houseNumber, string postcode, string country)
		{
			return new PostcodeAddressProvider(houseNumber, postcode, country);
		}

		public string HouseNumber { get; }

		public string Postcode { get; }

		public string Country { get; }
	}
}
