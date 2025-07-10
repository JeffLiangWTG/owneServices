namespace Enterprise.Customs.BE.Business.Testing;

sealed class AdditionsAndDeductionsProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionsAndDeductionsProvider>
{
	public void TestCode()
	{
		AssertEquals("Code", "CT1", Provider.Code);
	}

	public void TestAmount()
	{
		AssertEquals("Amount", 31m, Provider.Amount);
	}

	public void TestSequenceNumber()
	{
		AssertEquals("SequenceNumber", "1", Provider.SequenceNumber);
	}

	protected override AdditionsAndDeductionsProvider GetProvider() => new AdditionsAndDeductionsProvider(1, "CT1", 31m);
}
