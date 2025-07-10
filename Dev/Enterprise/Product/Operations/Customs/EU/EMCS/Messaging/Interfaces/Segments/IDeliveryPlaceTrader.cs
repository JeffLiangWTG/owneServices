using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IDeliveryPlaceTrader : ITrader
	{
		ZString TraderId { get; set; }
	}
}
