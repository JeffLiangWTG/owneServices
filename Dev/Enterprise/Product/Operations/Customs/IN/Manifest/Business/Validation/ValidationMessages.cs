namespace Enterprise.Customs.IN.Manifest.Business;

public static class ValidationMessages
{
	public static class Shared
	{
		public static string GetFieldIsNotWithinRangeMessage(string fieldName, int minimumAcceptableValue, int maximumAcceptableValue) => Res.GetString("52A3A67B-2B89-45BD-A146-61842E481B40", "You have entered an invalid {0}. Expected numerical value ranging from {1} - {2}.", fieldName, minimumAcceptableValue, maximumAcceptableValue);
	}
}
