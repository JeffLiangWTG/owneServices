namespace Enterprise.Customs.GB.Business.NCTS
{
	public static class Constants
	{
		public static class AuthorisationTypes
		{
			public const string C520 = "C520";
			public const string C521 = "C521";
			public const string C522 = "C522";
			public const string C523 = "C523";
			public const string C524 = "C524";
		}

		public static class CusCodeDataTypes
		{
			public const string CountryOfRouting = "COR";
			public const string TransportInland = "TPI";
			public const string ITEM = "ITM";
		}

		public static class CusPermitHeaderTypes
		{
			public const string TransitOperation = "TRD";
			public const string ACR = "ACR";
			public const string SSE = "SSE";
		}

		public static class CusSupportingInfoSubTypes
		{
			public const string TransportDocument = "TRA";
			public const string AdditionalReference = "REF";
			public const string AdditionalInformation = "INF";
		}

		public static class CusSupportingInfoTypes
		{
			public const string Other = "OTH";
			public const string PreviousDocument = "PRE";
			public const string SupportingDocument = "SUP";
		}

		public static class MessageTypes
		{
			public const string CC007C = "Cc007C";
			public const string CC013C = "Cc013C";
			public const string CC014C = "Cc014C";
			public const string CC015C = "Cc015C";
			public const string CC044C = "Cc044C";
			public const string CC054C = "Cc054C";
			public const string CC141C = "Cc141C";
			public const string CC170C = "Cc170C";
		}

		public static class OrgCusCodeTypes
		{
			public const string TransitOperationHolder = "TIR";
		}
	}
}
