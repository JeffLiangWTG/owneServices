using CargoWise.Customs.Shared.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExchangeHedgeListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("ExchangeHedgeList is ", "ATE_180_DIAS", ExchangeHedgeList.MapToCustomsCode(ExchangeHedgeList.Codes._1));
				AssertEquals("ExchangeHedgeList is ", "DE_181_ATE_360", ExchangeHedgeList.MapToCustomsCode(ExchangeHedgeList.Codes._2));
				AssertEquals("ExchangeHedgeList is ", "ACIMA_360", ExchangeHedgeList.MapToCustomsCode(ExchangeHedgeList.Codes._3));
				AssertEquals("ExchangeHedgeList is ", "SEM_COBERTURA", ExchangeHedgeList.MapToCustomsCode(ExchangeHedgeList.Codes._4));
				Assert("When code not found, should be Empty", ExchangeHedgeList.MapToCustomsCode("X").IsEmpty());
			});
		}
	}
}
