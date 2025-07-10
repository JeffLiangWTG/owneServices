using System;
using System.Linq;
using CargoWise.Customs.IN.MessageContracts.AirCgm;
using CargoWise.Types;
using Enterprise.Customs.IN.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.AirCgm.Testing;

[TestedType(typeof(AirCgmCMCHI01DataProvider))]
sealed class ConsolHouseDataProviderTest : AirCgmIConsolHouseDataProviderBase
{
	public override void TestMessageType()
	{
		AdditionalDataProviderMock.Setup(x => x.GetMessageType(It.IsAny<CGMAsycudaBill>())).Returns("S");
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
			AssertEquals(nameof(IConsolHouseDataProvider.GrossWeight), 0m, dataProvider.GrossWeight);

			bill.ABL_GrossWeightUQ = ZString.Empty;
			bill.ABL_GrossWeight = 9.123m;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.GrossWeight), 0m, dataProvider.GrossWeight);

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.GrossWeight), 9.123m, dataProvider.GrossWeight);

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.GrossWeight), 0.009123m, dataProvider.GrossWeight);

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.GrossWeight), 9123m, dataProvider.GrossWeight);
		});
	}

	public override void TestHawbDate()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNull(nameof(IConsolHouseDataProvider.HawbDate), dataProvider.HawbDate);

			bill.ABL_BillIssueDate = ZDate.Empty;
			dataProvider = CreateDataProvider();
			AssertNull(nameof(IConsolHouseDataProvider.HawbDate), dataProvider.HawbDate);

			bill.ABL_BillIssueDate = new ZDate(2022, 3, 16);
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.HawbDate), new DateTime(2022, 3, 16), dataProvider.HawbDate);
		});
	}

	public override void TestHawbNo()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.HawbNo), string.Empty, dataProvider.HawbNo);

			bill.ABL_BillNumber = "123";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.HawbNo), "123", dataProvider.HawbNo);
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
			AssertEquals(nameof(IConsolHouseDataProvider.ItemDescription), string.Empty, dataProvider.ItemDescription);

			bill.ABL_GoodsDescription = "XYZ";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.ItemDescription), "XYZ", dataProvider.ItemDescription);
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
		RefDataSetupTestHelper.SetupUNLOCOData(Factory);
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfDestination), string.Empty, dataProvider.PortOfDestination);

			bill.ABL_RL_NKFinalDestination = "INABC";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfDestination), "XYZ", dataProvider.PortOfDestination);

			bill.ABL_RL_NKFinalDestination = "INDEL";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfDestination), "DEL", dataProvider.PortOfDestination);

			bill.ABL_RL_NKFinalDestination = "EL";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfDestination), "EL", dataProvider.PortOfDestination);
		});
	}

	public override void TestPortOfOrigin()
	{
		RefDataSetupTestHelper.SetupUNLOCOData(Factory);
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfOrigin), string.Empty, dataProvider.PortOfOrigin);

			bill.ABL_RL_NKOrigin = "INABC";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfOrigin), "XYZ", dataProvider.PortOfOrigin);

			bill.ABL_RL_NKOrigin = "INBLR";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfOrigin), "BLR", dataProvider.PortOfOrigin);

			bill.ABL_RL_NKOrigin = "LR";
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.PortOfOrigin), "LR", dataProvider.PortOfOrigin);
		});
	}

	public override void TestShipmentType()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.ShipmentType), ShipmentTypeList.Codes.Total, dataProvider.ShipmentType);

			bill.ABL_SpecialCargoCode = ShipmentTypeList.Codes.PartShipment;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.ShipmentType), ShipmentTypeList.Codes.PartShipment, dataProvider.ShipmentType);
		});
	}

	public override void TestTotalPackages()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.TotalPackages), 0, dataProvider.TotalPackages);

			bill.ABL_ManifestQty = 3;
			dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolHouseDataProvider.TotalPackages), 3, dataProvider.TotalPackages);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header.AMA_TransportMode = Core.Constants.TransportModes.Air;
		masterBill = header.MasterBill;
		bill = header.Bills.AddNew();
	}

	protected override IConsolHouseDataProvider CreateDataProvider()
		=> AirCgmCMCHI01DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Consoligm.HouseItems.First();

	ConsolDataProviderBaseTestHelper DataProviderTestHelper => dataProviderTestHelper ??= new();
	ConsolDataProviderBaseTestHelper dataProviderTestHelper;

	Mock<IAirCgmCMCHI01AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new();
	Mock<IAirCgmCMCHI01AdditionalDataProvider> additionalDataProviderMock;

	CGMAsycudaBill bill;
	CGMAsycudaBill masterBill;
}
