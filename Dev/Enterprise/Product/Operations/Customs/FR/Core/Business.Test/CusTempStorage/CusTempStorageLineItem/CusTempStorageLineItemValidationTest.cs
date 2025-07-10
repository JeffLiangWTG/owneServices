using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	internal class CusTempStorageLineItemValidationTest : EU.Business.CusTempStorage.Testing.CusTempStorageLineItemValidationTest
	{
		public void TestCheckTSI_GuaranteedValue()
		{
			var storageItem = Factory.New<CusTempStorageLineItem>();
			storageItem.TSI_GuaranteedValue = ZDecimal.Zero;
			AssertHasMessageErrors(storageItem.TSI_GuaranteedValueInfo);

			storageItem.TSI_GuaranteedValue = 10.0;
			AssertNoMessageErrors(storageItem.TSI_GuaranteedValueInfo);
		}

		public void TestCheckTSI_RX_NKCurrency()
		{
			var storageItem = Factory.New<CusTempStorageLineItem>();
			storageItem.TSI_RX_NKCurrency = ZString.Empty;
			AssertHasMessageErrors(storageItem.TSI_RX_NKCurrencyInfo);

			storageItem.TSI_RX_NKCurrency = "EUR";
			AssertNoMessageErrors(storageItem.TSI_RX_NKCurrencyInfo);

			storageItem.TSI_RX_NKCurrency = "ABC";
			AssertHasMessageErrors(storageItem.TSI_RX_NKCurrencyInfo);
		}

		public void TestCheckTSI_GoodsOrigin()
		{
			var storageItem = Factory.New<CusTempStorageLineItem>();
			ValidationTestHelper.AssertInvalidCodeMessageError(storageItem.TSI_GoodsOriginInfo, "^^", Core.Constants.CountryCodes.Australia);
		}
	}
}
