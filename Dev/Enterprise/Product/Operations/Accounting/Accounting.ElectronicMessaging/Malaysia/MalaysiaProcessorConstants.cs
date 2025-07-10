namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public static class MalaysiaProcessorConstants
	{
		public static class DataContext
		{
			public const string MalaysiaSubmitResult = "EINV_MY_SubmitResult";
			public const string MalaysiaSubmissionStatus = "EINV_MY_SubmissionStatus";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public static class SubmitResult
		{
			public const string Accepted = "Accepted";
			public const string Rejected = "Rejected";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public static class SubmissionStatus
		{
			public const string Valid = "Valid";
			public const string Submitted = "Submitted";
			public const string Invalid = "Invalid";
		}
	}
}
