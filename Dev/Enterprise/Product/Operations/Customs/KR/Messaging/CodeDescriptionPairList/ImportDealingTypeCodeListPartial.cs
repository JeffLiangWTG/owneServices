namespace Enterprise.Customs.KR.Messaging
{
	partial class ImportDealingTypeCodeList
	{
		public static bool IsTradeFree(string code)
		{
			return code == Codes._71
				|| code == Codes._80
				|| code == Codes._81
				|| code == Codes._82
				|| code == Codes._83
				|| code == Codes._84
				|| code == Codes._85
				|| code == Codes._86
				|| code == Codes._87
				|| code == Codes._88
				|| code == Codes._89
				|| code == Codes._90
				|| code == Codes._91
				|| code == Codes._92
				|| code == Codes._93
				|| code == Codes._94
				|| code == Codes._95
				|| code == Codes._96
				|| code == Codes._97
				|| code == Codes._100;
		}

		public static bool IsApplicableForDefaultSupplierID(string code)
		{
			return code == Codes._71 ||
						 code == Codes._72 ||
						 code == Codes._80 ||
						 code == Codes._85 ||
						 code == Codes._86 ||
						 code == Codes._87 ||
						 code == Codes._91 ||
						 code == Codes._94 ||
						 code == Codes._95 ||
						 code == Codes._96 ||
						 code == Codes._97 ||
						 code == Codes._100;
		}
	}
}
