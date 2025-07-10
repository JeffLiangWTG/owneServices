using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface ICommodityCodeCommon
	{
		ZString TariffCode { get; }
		ZString TariffCodeCombined { get; }
	}
}
