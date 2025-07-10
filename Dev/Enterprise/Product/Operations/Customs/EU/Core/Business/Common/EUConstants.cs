namespace Enterprise.Customs.EU.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class EUCommonConstants
	{
		public static class PreviousDocumentCodeList
		{
			public const string Cleared = "CLE";
		}

		public static class CertificateSource
		{
			public const string Windows = "Windows";
			public const string Token = "Token";
		}

		public static class ImportDeclarationTypeList
		{
			public const string H1 = "H1";
			public const string H2 = "H2";
			public const string H3 = "H3";
			public const string H4 = "H4";
			public const string H5 = "H5";
			public const string H6 = "H6";
			public const string H7 = "H7";
			public const string I1 = "I1";
		}

		public static class CusAuthorizationUsageType
		{
			public const string C019 = "C019";
			public const string C506 = "C506";
			public const string C512 = "C512";
			public const string C513 = "C513";
			public const string C514 = "C514";
			public const string C515 = "C515";
			public const string C626 = "C626";
			public const string C627 = "C627";
			public const string C601 = "C601";
			public const string D019 = "D019";
			public const string N990 = "N990";
		}

		public static class DeclarationRelationshipType
		{
			public const string SupplementaryDeclaration = "SUP";
		}

		public enum TransportModeSource
		{
			None,
			TransportModeAtBorder,
			InlandTransportMode,
		}
	}
}
