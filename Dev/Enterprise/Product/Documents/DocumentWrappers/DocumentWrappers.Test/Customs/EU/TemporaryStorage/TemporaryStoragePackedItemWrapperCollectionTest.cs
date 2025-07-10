using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
[TestedType(typeof(TemporaryStoragePackedItemWrapperCollection))]
sealed class TemporaryStoragePackedItemWrapperCollectionTest : DocumentWrapperCollectionTest<TemporaryStoragePackedItemWrapperCollection>
{
	public void TestConstruct()
	{
		CombineAssertions(() => {
			AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStoragePackedItemWrapperCollection(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => new TemporaryStoragePackedItemWrapperCollection(packedItems, null));
			AssertNoExceptionThrown(() => GetNewDocumentWrapperCollection());
			AssertEquals("Collection Count", 1, Collection.Count);
		});
	}

	public void TestPackedItems()
	{
		CombineAssertions(() =>
		{
			var packedItem = packedItems.First();
			packedItem.API_LineNo = 1;
			packedItem.API_FormattedTariff = "111100001111";
			packedItem.API_GoodsDescription = "Goods Description";
			packedItem.API_GrossWeight = 100m;
			packedItem.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			packedItem.API_NetWeight = 80m;
			packedItem.API_NetWeightUQ = Core.Constants.Weight.Kilograms;
			packedItem.API_PackStatus = "NO";
			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Spain;
			packedItem.API_GoodsValue = 1500m;
			packedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Spain;

			var wrapper = GetNewDocumentWrapperCollection()[0];
			AssertNotNull(wrapper);

			AssertEquals("LineNo", "1", wrapper.LineNo);
			AssertEquals("Tariff", "1111.00.00 1111", wrapper.Tariff);
			AssertEquals("GoodsDescription", "Goods Description", wrapper.GoodsDescription);
			AssertEquals("GrossWeight", "100", wrapper.GrossWeight);
			AssertEquals("GrossWeightUQ", Core.Constants.Weight.Kilograms, wrapper.GrossWeightUQ);
			AssertEquals("NetWeight", "80", wrapper.NetWeight);
			AssertEquals("NetWeightUQ", Core.Constants.Weight.Kilograms, wrapper.NetWeightUQ);
			AssertEquals("Missing", "NO", wrapper.Missing);
			AssertEquals("Origin", Core.Constants.CountryCodes.Spain, wrapper.Origin);
			AssertEquals("MonetaryValue", "1500", wrapper.MonetaryValue);
			AssertEquals("Currency", Core.Constants.CurrencyCodes.Spain, wrapper.Currency);
		});
	}

	protected override TemporaryStoragePackedItemWrapperCollection GetNewDocumentWrapperCollection() => new TemporaryStoragePackedItemWrapperCollection(packedItems, Factory);

	protected override object GetNewObjectToWrap() => null;

	protected override void SetUp()
	{
		base.SetUp();
		packedItems = new TemporaryStoragePackedItem[] { Factory.New<TemporaryStorageBill>().PackedItems.AddNew() };
	}

	IEnumerable<TemporaryStoragePackedItem> packedItems;
}
