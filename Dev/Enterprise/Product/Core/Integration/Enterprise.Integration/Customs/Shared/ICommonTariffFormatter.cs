namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICommonTariffFormatter
			{
				string DisplayFormat(string countryCode, string unformattedTariff);
			}
		}
	}
}
