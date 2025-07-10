namespace Enterprise.Customs.CH.Business.Testing;

public static class PassarMessagingConstants
{
	public static class DecisionCodes
	{
		public const string Accepted = "ACCEPTED";
		public const string Rejected = "REJECTED";
		public const string Received = "RECEIVED";
	}

	public static class SelectionStatusCodes
	{
		public const string Preliminary = "PRELIMINARY";
		public const string Final = "FINAL";
	}

	public static class InspectionDecisionCodes
	{
		public const string Clear = "CLEAR";
		public const string Intervention = "INTERVENTION";
	}
}
