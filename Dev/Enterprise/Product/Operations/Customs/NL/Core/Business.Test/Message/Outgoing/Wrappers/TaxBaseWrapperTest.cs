using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class TaxBaseWrapperTest : DataProviderTestCase<TaxBaseWrapper>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestTaxAmount()
	{
		AssertEquals(0m, wrapper.TaxAmount);
	}

	public void TestAdValoremTaxBaseAmount() => CombineAssertions(() =>
	{
		cusEntryLineFee.CF_BaseValue = 10m;

		cusEntryLineFee.CF_MethodOfCalculation = Mathematics.Percentage;
		AssertEquals("%", 10m, wrapper.AdValoremTaxBaseAmount);

		cusEntryLineFee.CF_MethodOfCalculation = "X";
		AssertEquals("not %", 0m, wrapper.AdValoremTaxBaseAmount);
	});

	public void TestSpecificTaxBaseQuantity() => CombineAssertions(() =>
	{
		cusEntryLineFee.CF_BaseValue = 10m;

		cusEntryLineFee.CF_MethodOfCalculation = Mathematics.Percentage;
		AssertEquals("%", 0m, wrapper.SpecificTaxBaseQuantity);

		cusEntryLineFee.CF_MethodOfCalculation = "X";
		AssertEquals("not %", 10m, wrapper.SpecificTaxBaseQuantity);
	});

	public void TestUnitCode() => CombineAssertions(() =>
	{
		cusEntryLineFee.CF_MethodOfCalculation = Mathematics.Percentage;
		AssertEquals("%", string.Empty, wrapper.UnitCode);

		cusEntryLineFee.CF_MethodOfCalculation = "X";
		AssertEquals("not %", "X", wrapper.UnitCode);
	});

	public void TestTaxRateNumeric()
	{
		cusEntryLineFee.CF_Rate = 12;
		AssertEquals(12, wrapper.TaxRateNumeric);
	}

	protected override void SetUp()
	{
		base.SetUp();

		cusEntryLineFee = Factory.New<CusEntryLineFee>();
		wrapper = new TaxBaseWrapper(cusEntryLineFee, 1);
	}
	CusEntryLineFee cusEntryLineFee;
	TaxBaseWrapper wrapper;

	protected override TaxBaseWrapper GetProvider() => wrapper;
}
