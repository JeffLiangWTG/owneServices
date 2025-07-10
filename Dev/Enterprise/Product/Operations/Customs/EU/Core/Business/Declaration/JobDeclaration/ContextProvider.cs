using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public interface IStatusChecker
	{
		string GetStatusCodeFromRouteOfEntry(string routeCode);
	}

	public interface IJobComInvoiceLineValueCalculator
	{
		ZString GetTariffDescription(ZString tariff);
	}
}
