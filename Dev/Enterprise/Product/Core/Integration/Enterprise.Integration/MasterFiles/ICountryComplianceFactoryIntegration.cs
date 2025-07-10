using CargoWise.Types;

namespace Enterprise.Integration.Compliance
{
	public interface ICountryComplianceFactoryIntegration
	{
		ICountryComplianceInfo GetICountryComplianceInfo(ZString countryCode);

		IComplianceSubTypeCodeProvider GetIComplianceSubTypeCodeProvider(ZString countryCode);
	}
}
