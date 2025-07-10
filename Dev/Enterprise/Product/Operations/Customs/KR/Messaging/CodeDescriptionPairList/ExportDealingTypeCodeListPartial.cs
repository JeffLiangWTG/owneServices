namespace Enterprise.Customs.KR.Messaging
{
	partial class ExportDealingTypeCodeList
	{
		public static bool IsImportedToReExported(string code)
		{
			return code == Codes._72
				|| code == Codes._84
				|| code == Codes._86
				|| code == Codes._89
				|| code == Codes._93
				|| code == Codes._94;
		}

		public static bool IsImportDeclarationNumberOptional(string code)
		{
			return code == Codes._94;
		}
	}
}
