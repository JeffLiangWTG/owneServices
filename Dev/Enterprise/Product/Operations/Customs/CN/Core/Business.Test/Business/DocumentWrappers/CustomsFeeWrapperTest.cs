using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CustomsFeeWrapperTest : DocBaseWrapperTest
	{
		public void TestProperties()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "USD", "美元", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			Factory.Save();
			var wrapper = new CustomsFeeWrapper(CustomsFee.Empty, ZDateTime.Today, Factory);
			AssertEquals(0m, wrapper.Amount);
			AssertEquals("", wrapper.Currency.CodeAndDescription);
			AssertEquals("", wrapper.FeeMark.CodeAndDescription);
			AssertEquals("", wrapper.CurrencyAmountAndMarkCode);
			AssertEquals("", wrapper.CurrencyAmountAndMarkDesc);
			var fee = new CustomsFee(10);
			wrapper = new CustomsFeeWrapper(fee, ZDateTime.Today, Factory);
			AssertEquals(10m, wrapper.Amount);
			AssertEquals("", wrapper.Currency.CodeAndDescription);
			AssertEquals("1 - 费率", wrapper.FeeMark.CodeAndDescription);
			AssertEquals("/10/1", wrapper.CurrencyAmountAndMarkCode);
			AssertEquals("/10/费率", wrapper.CurrencyAmountAndMarkDesc);
			fee = new CustomsFee(new Money(10, RefCurrency.LoadFromCurrencyCode(Factory, "USD")));
			wrapper = new CustomsFeeWrapper(fee, ZDateTime.Today, Factory);
			AssertEquals(10m, wrapper.Amount);
			AssertEquals("USD - 美元", wrapper.Currency.CodeAndDescription);
			AssertEquals("3 - 总价", wrapper.FeeMark.CodeAndDescription);
			AssertEquals("USD/10/3", wrapper.CurrencyAmountAndMarkCode);
			AssertEquals("美元/10/总价", wrapper.CurrencyAmountAndMarkDesc);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper() => new CustomsFeeWrapper(CustomsFee.Empty, ZDateTime.Today, Factory);
	}
}
