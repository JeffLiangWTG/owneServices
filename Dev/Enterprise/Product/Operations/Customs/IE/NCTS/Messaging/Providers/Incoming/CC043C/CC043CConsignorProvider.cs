using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CConsignorProvider
	{
		public CC043CConsignorProvider(ConsignorType05 consignor)
		{
			this.consignor = consignor;
		}
		readonly ConsignorType05 consignor;

		public ZString IdentificationNumber => consignor.IdentificationNumber ?? ZString.Empty;

		public ZString Name => consignor.Name ?? ZString.Empty;

		public AddressProvider Address
		{
			get
			{
				var xmlAddress = consignor.Address;
				return xmlAddress == null ? null : addressCached ?? (addressCached = new AddressProvider
				{
					StreetAndNumber = xmlAddress.StreetAndNumber,
					Postcode = xmlAddress.Postcode,
					City = xmlAddress.City,
					Country = xmlAddress.Country,
				});
			}
		}
		AddressProvider addressCached;
	}
}
