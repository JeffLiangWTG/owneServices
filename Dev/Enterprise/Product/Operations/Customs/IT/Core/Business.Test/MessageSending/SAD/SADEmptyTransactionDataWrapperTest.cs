using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADEmptyTransactionDataWrapperTest : TestCase
{
	public void TestWrapper()
	{
		var wrapper = new SADEmptyTransactionDataWrapper();
		AssertEquals(ZString.Empty, wrapper.CurrencyCode);
		AssertNull(wrapper.ExchangeRate);
		AssertEquals(ZString.Empty, wrapper.NatureOfTransactionCode);
		AssertNull(wrapper.TotalAmountInvoiced);
	}
}
