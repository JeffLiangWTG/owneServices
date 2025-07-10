using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface ITrader
	{
		ZString TraderName { get; set; }

		ZString StreetName { get; set; }

		ZString StreetNumber { get; set; }

		ZString Postcode { get; set; }

		ZString City { get; set; }
	}
}
