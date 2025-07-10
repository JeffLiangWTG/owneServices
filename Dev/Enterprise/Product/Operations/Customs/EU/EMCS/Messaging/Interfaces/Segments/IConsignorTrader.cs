using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IConsignorTrader : ITrader
	{
		ZString TraderExciseNumber { get; set; }
	}
}
