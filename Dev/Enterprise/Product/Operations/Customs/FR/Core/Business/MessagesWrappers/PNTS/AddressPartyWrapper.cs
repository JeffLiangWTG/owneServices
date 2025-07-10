using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class AddressPartyWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.IAddressParty
	{
		AddressPartyWrapper(OrgAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly OrgAddress address;

		public string City => city ?? (city = address.OA_City);
		string city;

		public string Country => country ?? (country = address.OA_RN_NKCountryCode);
		string country;

		public string Number => number ?? (number = address.StreetNumber);
		string number;

		public string PoBox => poBox ?? (poBox = string.Empty);
		string poBox;

		public string PostCode => postcode ?? (postcode = address.OA_PostCode);
		string postcode;

		public string Street => street ?? (street = string.IsNullOrEmpty(address.OA_AddressMap) ? address.OA_Address1 : address.Street);
		string street;

		public string StreetAdditionalLine => streetAdditionalLine ?? (streetAdditionalLine = address.OA_Address2);
		string streetAdditionalLine;

		//TODO: waiting to be done in the future
		public string SubDivision => subDivision ?? (subDivision = string.Empty);
		string subDivision;

		public static AddressPartyWrapper New(OrgAddress address) => address == null ? null : new AddressPartyWrapper(address);
	}
}
