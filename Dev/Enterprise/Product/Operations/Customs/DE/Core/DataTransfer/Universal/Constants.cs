namespace Enterprise.Customs.DE.DataTransfer.Universal;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
public static class Constants
{
	public static class AddInfoKeys
	{
		public static class InvoiceLine
		{
			public const string IsMainPack = "IsMainPack";
		}

		public static class PreviousProcedure
		{
			public const string UsualProcessingFlag = "UsualProcessingFlag";
		}
	}

	public static class ReferenceNumbers
	{
		public const string AuthorizationNumberCode = "AUT";
		public const string AuthorizationNumberDescription = "AuthorizationNumber";
	}
}
