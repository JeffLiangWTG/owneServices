using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IRefCusTaxOrFeeProvider
			{
				ZDecimal LoadMostRecentEffectiveDeminimusOfCountry(string country);
			}
		}
	}
}
