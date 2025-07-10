using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CurrencyExchangeWrapper : ICurrencyExchange
	{
		public string InternalCurrencyUnit => "EUR";
	}
}
