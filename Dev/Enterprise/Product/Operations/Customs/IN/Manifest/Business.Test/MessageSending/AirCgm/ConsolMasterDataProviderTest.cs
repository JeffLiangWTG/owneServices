using CargoWise.Customs.IN.MessageContracts.AirCgm;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01DataProvider))]
sealed class ConsolMasterDataProviderTest : AirCgmIConsolMasterDataProviderBase
{
	public override void TestMessageType()
	{
		AdditionalDataProviderMock.Setup(x => x.GetMessageType(It.IsAny<CGMAsycudaManifestHeader>())).Returns("S");
		AssertEquals("When message type is S", "S", CreateDataProvider().MessageType);
	}

	public override void TestConsolAgentId()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestConsolAgentId(() => dataProvider.ConsolAgentId);
	}

	public override void TestCustomsHouseCode()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestCustomsHouseCode(header, () => dataProvider.CustomsHouseCode);
	}

	public override void TestFlightNo()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestFlightNo(header, () => dataProvider.FlightNo);
	}

	public override void TestFlightOriginDate()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestFlightOriginDate(masterBill, () => dataProvider.FlightOriginDate);
	}

	public override void TestGrossWeight()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.GrossWeight), 0m, dataProvider.GrossWeight);

			masterBill.ABL_GrossWeightUQ = ZString.Empty;
			masterBill.ABL_GrossWeight = 9.123m;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.GrossWeight), 0m, dataProvider.GrossWeight);

			masterBill.ABL_GrossWeightUQ = "KG";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.GrossWeight), 9.123m, dataProvider.GrossWeight);

			masterBill.ABL_GrossWeightUQ = "G";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.GrossWeight), 0.009123m, dataProvider.GrossWeight);

			masterBill.ABL_GrossWeightUQ = "T";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.GrossWeight), 9123m, dataProvider.GrossWeight);
		});
	}

	public override void TestIgmDate()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestIgmDate(header, () => dataProvider.IgmDate);
	}

	public override void TestIgmNo()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestIgmNo(header, () => dataProvider.IgmNo);
	}

	public override void TestItemDescription()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.ItemDescription), string.Empty, dataProvider.ItemDescription);

			masterBill.ABL_GoodsDescription = "XYZ";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.ItemDescription), "XYZ", dataProvider.ItemDescription);
		});
	}

	public override void TestMawbDate()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestMawbDate(masterBill, () => dataProvider.MawbDate);
	}

	public override void TestMawbNo()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestMawbNo(masterBill, () => dataProvider.MawbNo);
	}

	public override void TestPortOfDestination()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.PortOfDestination), string.Empty, dataProvider.PortOfDestination);

			masterBill.ABL_RL_NKFinalDestination = "INDEL";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.PortOfDestination), "DEL", dataProvider.PortOfDestination);

			masterBill.ABL_RL_NKFinalDestination = "EL";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.PortOfDestination), "EL", dataProvider.PortOfDestination);
		});
	}

	public override void TestPortOfOrigin()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.PortOfOrigin), string.Empty, dataProvider.PortOfOrigin);

			masterBill.ABL_RL_NKOrigin = "INBLR";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.PortOfOrigin), "BLR", dataProvider.PortOfOrigin);

			masterBill.ABL_RL_NKOrigin = "LR";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.PortOfOrigin), "LR", dataProvider.PortOfOrigin);
		});
	}

	public override void TestShipmentType()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.ShipmentType), ShipmentTypeList.Codes.Total, dataProvider.ShipmentType);

			masterBill.ABL_SpecialCargoCode = ShipmentTypeList.Codes.PartShipment;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.ShipmentType), ShipmentTypeList.Codes.PartShipment, dataProvider.ShipmentType);
		});
	}

	public override void TestTotalPackages()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.TotalPackages), 0, dataProvider.TotalPackages);

			masterBill.ABL_ManifestQty = 3;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolMasterDataProvider.TotalPackages), 3, dataProvider.TotalPackages);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		masterBill = header.MasterBill;
	}

	protected override IConsolMasterDataProvider CreateDataProvider()
		=> AirCgmCMCHI01DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Consoligm.Master;

	ConsolDataProviderBaseTestHelper DataProviderTestHelper => dataProviderTestHelper ??= new();
	ConsolDataProviderBaseTestHelper dataProviderTestHelper;

	Mock<IAirCgmCMCHI01AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new();
	Mock<IAirCgmCMCHI01AdditionalDataProvider> additionalDataProviderMock;

	CGMAsycudaBill masterBill;
}
