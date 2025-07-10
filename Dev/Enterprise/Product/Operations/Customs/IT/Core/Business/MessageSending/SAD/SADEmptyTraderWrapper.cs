using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADEmptyTraderWrapper : ITrader
{
	public ZString IdCountryCode => ZString.Empty;

	public ZString ID => ZString.Empty;

	public ZString Name => ZString.Empty;

	public ZString Address => ZString.Empty;

	public ZString Postcode => ZString.Empty;

	public ZString City => ZString.Empty;

	public ZString CountryCode => ZString.Empty;
}
