using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitHeaderTransactionDataWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => new TransitHeaderTransactionDataWrapper());
	}

	public void TestCurrencyCode()
	{
		AssertEquals(nameof(wrapper.CurrencyCode), ZString.Empty, wrapper.CurrencyCode);
	}

	public void TestTotalAmountInvoiced()
	{
		AssertNull(nameof(wrapper.TotalAmountInvoiced), wrapper.TotalAmountInvoiced);
	}

	public void TestExchangeRate()
	{
		AssertNull(nameof(wrapper.ExchangeRate), wrapper.ExchangeRate);
	}

	public void TestNatureOfTransactionCode()
	{
		AssertEquals(nameof(wrapper.NatureOfTransactionCode), ZString.Empty, wrapper.NatureOfTransactionCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		wrapper = new TransitHeaderTransactionDataWrapper();
	}

	TransitHeaderTransactionDataWrapper wrapper;
}
