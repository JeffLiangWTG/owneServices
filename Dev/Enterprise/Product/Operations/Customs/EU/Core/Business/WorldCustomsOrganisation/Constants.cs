namespace Enterprise.Customs.EU.WorldCustomsOrganisation
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class Constants
	{
		public static class AmendmentType
		{
			public const string Change = "CHG";
			public const string Addition = "ADD";
			public const string Deletion = "DEL";
		}

		public static class EDIMessageApplicationReferencesForAmendment
		{
			public const string Current = "Current";
		}
	}
}
