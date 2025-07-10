using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IConsigneeTrader : ITrader
	{
		ZString TraderId { get; set; }

		ZString EoriNumber { get; set; }
	}
}
