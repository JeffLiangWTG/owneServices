namespace Enterprise.DocumentWrappers.Customs.EU
{
	public static class DocumentWrapperConstants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description Strings")]
		public static class Delimiters
		{
			public const string Comma = ",";
			public const string Space = " ";
			public const string CommaAndSpace = Comma + Space;
			public const string Semicolon = ";";
			public const string Dash = "-";
			public const string SemiColonAndspace = Semicolon + Space;
			public const string Colon = ":";
			public const string ColonAndSpace = Colon + Space;
			public const string CarriageReturn = "\r\n";
			public const string Hyphen = " - ";
			public const string UncheckedCheckBox = "\u2610";
			public const string CheckedCheckBox = "\u2611";
			public const string CrossedCheckBox = "\u2612";
		}
	}
}
