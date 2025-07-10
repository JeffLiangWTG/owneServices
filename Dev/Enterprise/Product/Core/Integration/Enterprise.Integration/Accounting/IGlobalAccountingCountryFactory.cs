using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface IGlobalAccountingCountryFactory
	{
		IAccountingCountryFactory GetCountryFactory(ZString countryCode);
	}
}
