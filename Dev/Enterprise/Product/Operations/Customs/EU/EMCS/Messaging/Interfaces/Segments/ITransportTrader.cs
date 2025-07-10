using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface ITransportTrader : ITrader
	{
		ZString VatNumber { get; set; }
	}
}
