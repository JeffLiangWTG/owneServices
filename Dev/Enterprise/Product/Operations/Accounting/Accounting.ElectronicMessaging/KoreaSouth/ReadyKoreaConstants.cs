namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public static class ReadyKoreaConstants
	{
		public const string BusinessRegistrationNumber = "1028142299";

		public const string KRC = "KRC";
		public const string KRS = "KRS";

		public const string SubmitID = "SubmitID";

		public static class InvoiceePartyBusinessTypeCode
		{
			public const string RegistrationNumberOfBusinessOperator = "01";
			public const string RegistrationNumberOfResident = "02";
			public const string Foreigner = "03";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public static class ValidationDocumentStatusCodes
		{
			public const string Success = "Success";
			public const string Fail = "Fail";
		}

		public const string ValidationDocumentStatusInfoSplitter = "-";

		public const int TimeSpanMinuteForQueryPivot = 30;

		public const int MaxQueryStatusTimes = 5;
	}
}
