namespace Enterprise.Integration;

public static partial class Customs
{
	public static partial class EUH7
	{
		public interface IH7FeatureControlProvider
		{
			bool IsAuthorized(string countryCode, string companyCode);
		}
	}
}
