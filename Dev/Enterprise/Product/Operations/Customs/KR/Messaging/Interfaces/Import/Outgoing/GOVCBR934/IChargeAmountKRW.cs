using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IChargeAmountKRW
	{
		ZString Type { get; }
		ZDecimal Amount { get; }
	}
}
