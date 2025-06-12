namespace eServices.eHubAdmin.Helpers
{
	public static class StringExtension
	{
		public static string GetValueOrDefault(this string instance, string defaultValue = "")
		{
			return instance ?? defaultValue;
		}

		public static string ShortName(this string longName)
		{
			return (longName != null && longName.Length >= 25) ? $"{longName.Substring(0, 22)}..." : longName;
		}
	}
}