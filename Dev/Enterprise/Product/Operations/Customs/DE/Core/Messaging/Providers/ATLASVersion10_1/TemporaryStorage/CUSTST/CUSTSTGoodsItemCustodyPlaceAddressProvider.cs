using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTSTGoodsItemCustodyPlaceAddressProvider : IUnderCustomsControlGoodsItemAddress
	{
		public CUSTSTGoodsItemCustodyPlaceAddressProvider(SCTSTJGoodsItemCustodyPlaceAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly SCTSTJGoodsItemCustodyPlaceAddress address;

		public ZString Line => address.Line;

		public ZString Country => ZString.Empty;

		public ZString Postcode => address.Postcode;

		public ZString City => address.City;

		public ZString District => address.District;
	}
}
