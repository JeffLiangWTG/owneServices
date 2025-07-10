using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CConsigneeProvider
	{
		public CC043CConsigneeProvider(ConsigneeType04 consignee)
		{
			this.consignee = consignee;
		}
		readonly ConsigneeType04 consignee;

		public ZString IdentificationNumber => consignee.IdentificationNumber ?? ZString.Empty;

		public ZString Name => consignee.Name ?? ZString.Empty;

		public AddressProvider Address
		{
			get
			{
				var xmlAddress = consignee.Address;
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
