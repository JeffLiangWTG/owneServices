using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICountryCodeProvider
			{
				ZString CountryCode { get; }
			}
		}
	}
}
