using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class TurkeyDutyRateSelectionCriteriaTest : TestCaseWithFactory
{
	public void TestProperties()
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		var rateCriteria = new TurkeyDutyRateSelectionCriteria(invoiceLine);

		CombineAssertions(() =>
		{
			AssertEquals("Rate Type", "DTY", rateCriteria.RateType);
			AssertEquals("Trade Group Country", "TR", rateCriteria.TradeGroupCountry);
		});
	}
}
