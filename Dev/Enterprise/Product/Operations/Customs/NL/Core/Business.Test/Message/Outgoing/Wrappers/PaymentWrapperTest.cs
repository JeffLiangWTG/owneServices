using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class PaymentWrapperTest : DataProviderTestCase<PaymentWrapper>
{
	public void TestMethodCode()
	{
		AssertEquals("AB", wrapper.MethodCode);
	}
	public void TestPaymentAmount()
	{
		AssertEquals(0m, wrapper.PaymentAmount);
	}
	protected override void SetUp()
	{
		base.SetUp();
		wrapper = new PaymentWrapper("AB");
	}
	PaymentWrapper wrapper;

	protected override PaymentWrapper GetProvider() => wrapper;
}
