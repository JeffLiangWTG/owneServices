using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IAsycudaCustomsCountryProvider
			{
				ZString[] GetAsycudaCustomsCountryCodes();
				bool IsAsycudaCustomsCountry(ZString countryCode);
				bool IsSelfManagedTariffCountry(ZString countryCode);
				ZString[] GetSelfManagedTariffCountryCodes();
#if DEBUG
				void ResetCachingForTest();
#endif
			}
		}
	}
}
