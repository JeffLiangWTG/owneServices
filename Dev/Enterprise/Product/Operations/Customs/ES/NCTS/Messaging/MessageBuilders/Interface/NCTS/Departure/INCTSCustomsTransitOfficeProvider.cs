using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INctsCustomsTransitOfficeProvider
	{
		ZString CustomsTransitOfficeState { get; }
		ZString CustomsTransitOfficeCode { get; }
	}
}
