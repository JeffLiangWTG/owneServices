using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class LocalExportTransactionNatureCodeList :
		Integration.Customs.KR.IKRLocalExportTransactionNatureCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool IsSea(string code)
		{
			return (code == Codes._07) || (code == Codes._09) || (code == Codes._17);
		}

		public static bool IsAir(string code)
		{
			return code == Codes._08 || code == Codes._18;
		}

		public static bool Is5DP(string code)
		{
			return code == Codes._01
				|| code == Codes._02
				|| code == Codes._03
				|| code == Codes._04
				|| code == Codes._06;
		}

		public static bool Is5DQ(string code)
		{
			return code == Codes._07
				|| code == Codes._08
				|| code == Codes._09
				|| code == Codes._17
				|| code == Codes._18;
		}

		public static bool IsChangedTo5DPOr5DQ(string oldCode, string newCode)
		{
			return !Is5DP(oldCode) && Is5DP(newCode)
				|| !Is5DQ(oldCode) && Is5DQ(newCode);
		}

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList() => new LocalExportTransactionNatureCodeList();
	}
}
