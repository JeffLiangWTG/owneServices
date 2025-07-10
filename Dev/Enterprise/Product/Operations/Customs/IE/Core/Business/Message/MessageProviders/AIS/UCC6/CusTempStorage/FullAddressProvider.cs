using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class FullAddressProvider : IFullAddress
	{
		public static FullAddressProvider New(IDocAddress address) => address == null ? null : new FullAddressProvider(address);

		FullAddressProvider(IDocAddress address)
		{
			this.address = address;
		}
		readonly IDocAddress address;

		public string StreetAndNumber => address.E2_Address1;

		public string Number => null;

		public string Postcode => address.E2_Postcode;

		public string City => address.E2_City;

		public string Country => address.E2_RN_NKCountryCode;

		public string SubDivision => null;

		public string StreetAdditionalLine => address.E2_Address2;

		public string PoBox => null;
	}
}
