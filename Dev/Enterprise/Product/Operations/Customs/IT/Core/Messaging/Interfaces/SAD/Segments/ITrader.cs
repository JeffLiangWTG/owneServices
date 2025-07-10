using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface ITrader
{
	ZString IdCountryCode { get; }
	ZString ID { get; }
	ZString Name { get; }
	ZString Address { get; }
	ZString Postcode { get; }
	ZString City { get; }
	ZString CountryCode { get; }
}
