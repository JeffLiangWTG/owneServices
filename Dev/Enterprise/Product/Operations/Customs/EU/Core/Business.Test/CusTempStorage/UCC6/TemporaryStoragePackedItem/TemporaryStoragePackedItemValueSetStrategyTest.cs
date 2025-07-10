using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStoragePackedItemValueSetStrategy))]
sealed class TemporaryStoragePackedItemValueSetStrategyTest : TestCaseWithFactory
{
	public void TestValueSet() => CombineAssertions(() =>
	{
		TemporaryStorageTestHelper.SetUpTariffAllUnits(Factory, Core.Constants.CountryCodes.Latvia);

		var packedItem = GetTemporaryStoragePackedItem();
		AssertLiabilityAmountChanged("API_Tariff", () => packedItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode);

		packedItem = GetTemporaryStoragePackedItem();
		AssertLiabilityAmountChanged("API_GoodsValue", () => packedItem.API_GoodsValue += 1);
		AssertLiabilityAmountChanged("API_CustomsQty", () => packedItem.API_CustomsQty += 1);
		AssertLiabilityAmountChanged("API_CustomsUQ", () => packedItem.API_CustomsUQ = "XXX");

		packedItem = GetTemporaryStoragePackedItem();
		AssertLiabilityAmountChanged("API_CustomsQty2", () => packedItem.API_CustomsQty2 += 1);
		AssertLiabilityAmountChanged("API_CustomsUQ2", () => packedItem.API_CustomsUQ2 = "XXX");

		packedItem = GetTemporaryStoragePackedItem();
		AssertLiabilityAmountChanged("API_CustomsQty3", () => packedItem.API_CustomsQty3 += 1);
		AssertLiabilityAmountChanged("API_CustomsUQ3", () => packedItem.API_CustomsUQ3 = "XXX");

		packedItem = GetTemporaryStoragePackedItem();
		AssertLiabilityAmountChanged("API_RN_NKGoodsOrigin", () => packedItem.API_RN_NKGoodsOrigin = "AD");

		void AssertLiabilityAmountChanged(string propertyName, Action changeProperty)
		{
			packedItem.RefreshAllDutyAmountsFromTariffRates();
			var liabilityAmount = packedItem.LiabilityAmount;
			changeProperty();
			AssertNotEquals(propertyName, liabilityAmount, packedItem.LiabilityAmount);
		}
	});

	TemporaryStoragePackedItemForTest GetTemporaryStoragePackedItem()
	{
		var bill = Factory.New<TemporaryStorageHeader>().Bills.AddNew();
		var packedItem = Factory.New<TemporaryStoragePackedItemForTest>();
		packedItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;
		bill.PackedItems.Add(packedItem);
		packedItem.API_TaxAmount = 1m;
		packedItem.API_GoodsValue = 1_000m;
		packedItem.API_RX_NKGoodsValueCurrency = "EUR";
		packedItem.API_Tariff = "9111200000";
		packedItem.API_CustomsQty = 15m;
		packedItem.API_CustomsUQ = "KGM";
		packedItem.API_CustomsQty2 = 15m;
		packedItem.API_CustomsUQ2 = "DTN";
		packedItem.API_CustomsQty3 = 15m;
		packedItem.API_CustomsUQ3 = "NAR";
		return packedItem;
	}
}
