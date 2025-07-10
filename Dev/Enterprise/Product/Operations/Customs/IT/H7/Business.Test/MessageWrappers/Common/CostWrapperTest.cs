using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(CostWrapper))]
public sealed class CostWrapperTest : DataProviderTestCase<CostWrapper>
{
	public void TestAmount()
	{
		AssertEquals("Amount should equal Constructor Amount.", 129384723.12m, Provider.Amount);
	}

	public void TestCurrency()
	{
		AssertEquals("Currency should equal Constructor Currency.", "EUR", Provider.Currency);
	}

	protected override CostWrapper GetProvider()
	{
		var amount = 129384723.12m;
		var currency = "EUR";

		return new CostWrapper(amount, currency);
	}
}
