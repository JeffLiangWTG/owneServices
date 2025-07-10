using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ChargeDeductionWrapperTest : DataProviderTestCase<ChargeDeductionWrapper>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, wrapper.SequenceNumeric);
	}

	public void TestChargesTypeCode()
	{
		AssertEquals("ABC", wrapper.ChargesTypeCode);
	}

	public void TestOtherChargeDeductionAmount()
	{
		AssertEquals(12.5m, wrapper.OtherChargeDeductionAmount);
	}

	protected override ChargeDeductionWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var invoiceLineCharge = Factory.New<InvoiceLineCharge>();
		invoiceLineCharge.J7_Amount = 12.5m;
		invoiceLineCharge.J7_ChargeType = "ABC";
		wrapper = new ChargeDeductionWrapper(invoiceLineCharge, 1);
	}
	ChargeDeductionWrapper wrapper;
}
