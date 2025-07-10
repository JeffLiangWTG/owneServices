using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging
{
	public partial class PriceDeclarationItemCodeList : CodeDescriptionPairList
	{
		public static class StartingDigits
		{
			public const string ProvisionalPricingReasonCode = "1";
			public const string ValuationDeclarationTemplateQuestionCode = "2";
			public const string UseCode = "3";
			public const string GoodsPricingBasisCode = "4";
		}
		public static IEnumerable<string> MandatoryQuestionsForMethodTwoToSix()
		{
			yield return PriceDeclarationItemCodeList.Codes._301;
			yield return PriceDeclarationItemCodeList.Codes._302;
			yield return PriceDeclarationItemCodeList.Codes._303;
			yield return PriceDeclarationItemCodeList.Codes._304;
			yield return PriceDeclarationItemCodeList.Codes._305;
			yield return PriceDeclarationItemCodeList.Codes._306;
			yield return PriceDeclarationItemCodeList.Codes._307;
			yield return PriceDeclarationItemCodeList.Codes._401;
			yield return PriceDeclarationItemCodeList.Codes._402;
			yield return PriceDeclarationItemCodeList.Codes._403;
			yield return PriceDeclarationItemCodeList.Codes._404;
			yield return PriceDeclarationItemCodeList.Codes._405;
		}
	}
}
