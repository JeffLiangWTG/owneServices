using Enterprise.Accounting.CountryCompliance.Implementation.Germany;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class GermanyCountryFactory :
		IInstanceProvider<IPreferredPaymentMethod>
	{
		IPreferredPaymentMethod IInstanceProvider<IPreferredPaymentMethod>.Get() => new GermanyPreferredPaymentMethod();
	}
}
