using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	partial class TransactionTypeCodeList :
		Integration.Customs.KR.IKRTransactionTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool IsDateOfFinalPriceValidation(string code) => code == Codes._15 || code == Codes._31;

		ReadOnlyCodeDescriptionPairList DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider.GetCodeDescriptionPairList() => new TransactionTypeCodeList();
	}
}
