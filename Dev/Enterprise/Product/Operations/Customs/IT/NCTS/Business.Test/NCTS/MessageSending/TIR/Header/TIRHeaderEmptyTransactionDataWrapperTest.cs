using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIRHeaderEmptyTransactionDataWrapperTest : TestCaseWithFactory
{
	public void TestPropertiesAreEmpty()
	{
		var emptyTransactionDataWrapper = new TIRHeaderEmptyTransactionDataWrapper();

		CombineAssertions("Assert properties are empty", () =>
		{
			AssertEquals("CurrencyCode", ZString.Empty, emptyTransactionDataWrapper.CurrencyCode);
			AssertNull("TotalAmountInvoiced", emptyTransactionDataWrapper.TotalAmountInvoiced);
			AssertNull("ExchangeRate", emptyTransactionDataWrapper.ExchangeRate);
			AssertEquals("NatureOfTransactionCode", ZString.Empty, emptyTransactionDataWrapper.NatureOfTransactionCode);
		});
	}
}
