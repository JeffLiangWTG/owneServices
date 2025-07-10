using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTSTGoodsItemCustodianAddressProvider : IUnderCustomsControlGoodsItemAddress
	{
		public CUSTSTGoodsItemCustodianAddressProvider(SCTSTJGoodsItemCustodianAddress address)
		{
			this.address = Argument.NotNull(address, nameof(address));
		}
		readonly SCTSTJGoodsItemCustodianAddress address;

		public ZString Line => address.Line;

		public ZString Country => address.Country;

		public ZString Postcode => address.Postcode;

		public ZString City => address.City;

		public ZString District => address.District;
	}
}
