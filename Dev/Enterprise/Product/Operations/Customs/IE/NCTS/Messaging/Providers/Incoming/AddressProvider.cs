using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class AddressProvider
	{
		public ZString StreetAndNumber { get; set; }

		public ZString Postcode { get; set; }

		public ZString City { get; set; }

		public ZString Country { get; set; }
	}
}
