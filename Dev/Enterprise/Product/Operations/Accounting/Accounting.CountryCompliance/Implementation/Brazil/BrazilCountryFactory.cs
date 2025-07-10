using Enterprise.Accounting.CountryCompliance.Implementation.Brazil;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class BrazilCountryFactory : IInstanceProvider<IFeatureConstants>
	{
		IFeatureConstants IInstanceProvider<IFeatureConstants>.Get() => new BrazilFeatureConstants();
	}
}
