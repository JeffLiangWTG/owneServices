namespace Enterprise.Customs.JP.Manifest.Business;

public static class Constants
{
	public static class GenAddOnColumnFieldName
	{
		public const string ACN_MoveOutDate = "MoveOutDate";
		public const string ACN_CustomsTareWeight = "CustomsTareWeight";
		public const string ACN_CustomsWeightUQ = "CustomsWeightUQ";
		public const string AMA_ViaLocationCode = "Via";
		public const string AMA_CustomsAgentCredentialPK = "AMA_CustomsAgentCredentialPK";
		public const string ABL_CountryOfOrigin = "ABL_CountryOfOrigin";
	}

	public static class Message
	{
		public const int MaxNumberOfBillsForHCH01 = 20;
		public const int MaxNumberOfBillsForHDF01 = 30;
		public const int MaxNumberOfBillsForNVC01 = 20;
		public const int MaxNumberOfBillsForHDE = 20;

		public static class ManifestNatureCodes
		{
			public const string Export22 = "22";
			public const string Import23 = "23";
			public const string Transhipment28 = "28";
			public const string Transit24 = "24";
		}
	}
}
