using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage.Testing;

[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
[TestedType(typeof(TemporaryStoragePackedItemWrapper))]
sealed class TemporaryStoragePackedItemWrapperTest : DocBaseWrapperTest
{
	public void TestLineNo()
	{
		AssertEquals("LineNo", "1", Wrapper.LineNo);
	}

	public void TestTariff()
	{
		packedItem = Factory.New<TemporaryStorageHeader>().Bills.AddNew().PackedItems.AddNew();
		packedItem.API_Tariff = "11112222";
		var wrapper = TemporaryStoragePackedItemWrapper.New(packedItem, Factory);
		AssertEquals("Tariff", "1111.22.22", wrapper.Tariff);
	}

	public void TestGoodsDescription()
	{
		packedItem.API_GoodsDescription = "DESC";
		AssertEquals("GoodsDescription", "DESC", Wrapper.GoodsDescription);
	}

	public void TestGrossWeight()
	{
		packedItem.API_GrossWeight = 0.236m;
		AssertEquals("GrossWeight", "0.236", Wrapper.GrossWeight);
	}

	public void TestGrossWeightUQ()
	{
		packedItem.API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		AssertEquals("GrossWeightUQ", Core.Constants.Weight.Kilograms, Wrapper.GrossWeightUQ);
	}

	public void TestNetWeight()
	{
		packedItem.API_NetWeight = 15.67m;
		AssertEquals("NetWeight", "15.67", Wrapper.NetWeight);
	}

	public void TestNetWeightUQ()
	{
		packedItem.API_NetWeightUQ = Core.Constants.Weight.Decitons;
		AssertEquals("NetWeightUQ", Core.Constants.Weight.Decitons, Wrapper.NetWeightUQ);
	}

	public void TestMissing()
	{
		packedItem.API_PackStatus = "NO";
		AssertEquals("Missing", "NO", Wrapper.Missing);
	}

	public void TestOrigin()
	{
		packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Portugal;
		AssertEquals("Origin", Core.Constants.CountryCodes.Portugal, Wrapper.Origin);
	}

	public void TestMonetaryValue()
	{
		packedItem.API_GoodsValue = 123.456m;
		AssertEquals("MonetaryValue", "123.456", Wrapper.MonetaryValue);
	}

	public void TestCurrency()
	{
		packedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Latvia;
		AssertEquals("Currency", Core.Constants.CurrencyCodes.Latvia, Wrapper.Currency);
	}

	new TemporaryStoragePackedItemWrapper Wrapper => (TemporaryStoragePackedItemWrapper)base.Wrapper;

	protected override DocBaseWrapper GetNewDocumentWrapper() => TemporaryStoragePackedItemWrapper.New(packedItem, Factory);

	protected override void SetUp()
	{
		base.SetUp();
		packedItem = Factory.New<TemporaryStorageBill>().PackedItems.AddNew();
	}

	TemporaryStoragePackedItem packedItem;
}
