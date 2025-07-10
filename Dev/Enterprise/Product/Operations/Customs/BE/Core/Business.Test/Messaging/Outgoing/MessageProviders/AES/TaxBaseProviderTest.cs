using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Testing;

class TaxBaseProviderTest : Customs.Business.Testing.DataProviderTestCase<TaxBaseProvider>
{
	public void TestSequenceNumber()
	{
		AssertEquals("999", provider.SequenceNumber);
	}

	public void TestTaxRate()
	{
		entryLineFee.CF_Rate = 1614;
		AssertEquals(1614m, provider.TaxRate);
	}

	public void TestMeasurementUnitAndQualifier()
	{
		entryLineFee.CF_MethodOfCalculation = "CALC";
		AssertEquals("CALC", provider.MeasurementUnitAndQualifier);
	}

	public void TestQuantity()
	{
		entryLineFee.CF_BaseValue = 1615;
		AssertEquals(1615m, provider.Quantity);
	}

	public void TestAmount()
	{
		entryLineFee.CF_BaseValue = 1615;
		AssertEquals(1615m, provider.Amount);
	}

	public void TestTaxAmount()
	{
		entryLineFee.CF_ChargeAmount = 1616;
		AssertEquals(1616m, provider.TaxAmount);
	}

	protected override TaxBaseProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		entryLineFee = Factory.New<CusEntryLineFee>();
		provider = new TaxBaseProvider(entryLineFee, 999);
	}
	CusEntryLineFee entryLineFee;
	TaxBaseProvider provider;
}
