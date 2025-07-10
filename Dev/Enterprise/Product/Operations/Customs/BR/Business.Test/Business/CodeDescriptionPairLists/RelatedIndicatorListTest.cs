using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class RelatedIndicatorListTest : TestCase
	{
		public void TestMapToCustomsCodeISW()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NoBuyerSellerRelation", "1", RelatedIndicatorList.MapToCustomsCodeISW(RelatedIndicatorList.Codes.NoBuyerSellerRelation));
				AssertEquals("BuyerSellerRelationNoInfluence", "2", RelatedIndicatorList.MapToCustomsCodeISW(RelatedIndicatorList.Codes.BuyerSellerRelationNoInfluence));
				AssertEquals("BuyerSellerRelationWithInfluence", "3", RelatedIndicatorList.MapToCustomsCodeISW(RelatedIndicatorList.Codes.BuyerSellerRelationWithInfluence));
			});
		}

		public void TestMapToCustomsCodeDuimp()
		{
			CombineAssertions(() =>
			{
				AssertEquals("NoBuyerSellerRelation", "NAO_HA_VINCULACAO", RelatedIndicatorList.MapToCustomsCodeDuimp(RelatedIndicatorList.Codes.NoBuyerSellerRelation));
				AssertEquals("BuyerSellerRelationNoInfluence", "VINCULACAO_SEM_INFLUENCIA_PRECO", RelatedIndicatorList.MapToCustomsCodeDuimp(RelatedIndicatorList.Codes.BuyerSellerRelationNoInfluence));
				AssertEquals("BuyerSellerRelationWithInfluence", "VINCULACAO_COM_INFLUENCIA_PRECO", RelatedIndicatorList.MapToCustomsCodeDuimp(RelatedIndicatorList.Codes.BuyerSellerRelationWithInfluence));
			});
		}
	}
}
