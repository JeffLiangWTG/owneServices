using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Messaging
{
	public interface IGuarantorTrader : ITrader
	{
		ZString TraderExciseNumber { get; set; }

		ZString VatNumber { get; set; }
	}
}
