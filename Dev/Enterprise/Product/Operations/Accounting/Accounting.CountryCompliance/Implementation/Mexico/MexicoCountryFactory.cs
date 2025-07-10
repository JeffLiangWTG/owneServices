using Enterprise.Accounting.CountryCompliance.Implementation.Mexico;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class MexicoCountryFactory :
		IInstanceProvider<IDebtorTaxRegime>
	{
		IDebtorTaxRegime IInstanceProvider<IDebtorTaxRegime>.Get() => new DebtorTaxRegime();
	}
}
