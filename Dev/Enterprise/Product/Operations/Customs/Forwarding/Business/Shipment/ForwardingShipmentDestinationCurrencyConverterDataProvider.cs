using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Forwarding.Business
{
	internal sealed class ForwardingShipmentDestinationCurrencyConverterDataProvider : RefCurrencyCurrencyConverter
	{
		public ForwardingShipmentDestinationCurrencyConverterDataProvider(BusinessObjectFactory factory, ZDateTime dateForDestinationExchangeRate, ZString localCurrencyCode)
			: base(factory, dateForDestinationExchangeRate, ZArchitecture.Core.ExchangeRateType.Customs, 365)
		{
			this.LocalCurrencyCode = localCurrencyCode; 
		}

		public ZString LocalCurrencyCode { get; set; }

		public override ZString LocalCurrencyCodeOverride => LocalCurrencyCode;
	}
}
