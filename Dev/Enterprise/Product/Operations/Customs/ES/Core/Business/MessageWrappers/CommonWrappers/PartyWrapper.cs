using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PartyWrapper : PartyNameWrapper, IPartyProvider
	{
		public new static PartyWrapper New(OrgAddress address) => address?.Header == null ? null : new PartyWrapper(address);

		public new static PartyWrapper New(OrgHeader header) => PartyWrapper.New(header?.MainAddress);

		public new static PartyWrapper New(JobDocAddress docAddress) => PartyWrapper.New(docAddress?.Address);

		protected PartyWrapper(OrgAddress address)
			: base(address.Header)
		{
			orgAddress = address;
		}
		protected readonly OrgAddress orgAddress;

		public ZString Address => AddressCore;
		protected virtual ZString AddressCore => orgAddress.OA_Address1;

		public ZString City => CityCore;
		protected virtual ZString CityCore => orgAddress.OA_City;

		public ZString PostCode => PostCodeCore;
		protected virtual ZString PostCodeCore => orgAddress.OA_PostCode;

		public ZString Country => CountryCore;
		protected virtual ZString CountryCore => orgAddress.OA_RN_NKCountryCode;
	}
}
