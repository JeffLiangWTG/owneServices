namespace Enterprise.Customs.BR.Business
{
	public static class MessageConstants
	{
		public const string BRCustoms = "BRCustoms";

		public static class ResponseTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Response text")]
			public const string Successful = "Successful";
		}

		public static class MessageType
		{
			public const string BER = "BER";
			public const string RES = "RES";
		}
	}
}
