namespace Enterprise.Customs.CA.DIF.Business
{
	public static class DIFConstants
	{
		public static class UniversalEventConstants
		{
			public const string CACustoms = "CACustoms";
			public const string MessageType = "DIF";
		}

		public static class UniversalEventFunctionCode
		{
			public const string Original = "09";
			public const string Change = "04";
			public const string Amendment = "05";
			public const string Cancel = "01";
		}

		public static class UniversalEventDepartment
		{
			public const string CBSA = "CBSA";
		}
	}
}
