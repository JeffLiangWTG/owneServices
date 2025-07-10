namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class eAdaptorNextLogs
	{
		public static string NoTokenInCache()
		{
			return noTokenInCache;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service Task Logs should be English")]
		const string noTokenInCache = "No cached token found, claiming new token.";

		public static string NewToken(string dateTimeOffset)
		{
			return string.Format(newToken, dateTimeOffset);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service Task Logs should be English")]
		const string newToken = "New Token claimed. Token expires at {0}";

		public static string TokenCacheCleared()
		{
			return tokenCacheCleared;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service Task Logs should be English")]
		const string tokenCacheCleared = "Token cache cleared for message retry.";

		public static string ErrorOccurredWhenClaimingToken(string errorResponse)
		{
			return string.Format(errorOccurredWhenClaimingToken, errorResponse);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service Task Logs should be English")]
		const string errorOccurredWhenClaimingToken = "An error occurred when claiming a token:\r\n{0}";
	}
}
