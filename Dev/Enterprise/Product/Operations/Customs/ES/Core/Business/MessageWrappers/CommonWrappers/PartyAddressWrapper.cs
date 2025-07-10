using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PartyAddressWrapper : IPartyAddressProvider
	{
		public static PartyAddressWrapper New(OrgAddress address) => address?.Header == null ? null : new PartyAddressWrapper(address.OA_Address1, address.OA_City, address.OA_PostCode, address.OA_RN_NKCountryCode);

		public static PartyAddressWrapper New(ZString address, ZString city, ZString postCode, ZString country) => new PartyAddressWrapper(address, city, postCode, country);

		protected PartyAddressWrapper(ZString address, ZString city, ZString postCode, ZString country)
		{
			Address = address;
			City = city;
			PostCode = postCode;
			Country = country;
		}

		public ZString Address { get; }

		public ZString City { get; }

		public ZString PostCode { get; }

		public ZString Country { get; }
	}
}
