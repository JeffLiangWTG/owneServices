using System;
using System.Linq;
using CargoWise.Customs.IN.MessageContracts.SeaCgm;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IN.Manifest.Business.MessageSending.SeaCgm.Testing;

[TestedType(typeof(SeaCgmCMCHI21DataProvider))]
sealed class ConsolCargoDataProviderTest : SeaCgmIConsolCargoDataProviderBase
{
	public override void TestBondNumber()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNullOrEmpty(nameof(IConsolCargoDataProvider.BondNumber), dataProvider.BondNumber);

			dataProvider = CreateDataProvider();
			bill.BondNumber = "ABC123";
			AssertEquals(nameof(IConsolCargoDataProvider.BondNumber), "ABC123", dataProvider.BondNumber);
		});
	}

	public override void TestCargoMovement()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.CargoMovement), string.Empty, dataProvider.CargoMovement);

			dataProvider = CreateDataProvider();
			bill.ABL_CargoStatus = "LC";
			AssertEquals(nameof(IConsolCargoDataProvider.CargoMovement), "LC", dataProvider.CargoMovement);
		});
	}

	public override void TestCarnNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestCarnNumber(() => dataProvider.CarnNumber);
	}

	public override void TestCarrierCode()
	{
		var carrier = Factory.New<OrgHeader>();
		carrier.OH_Code = "ABCD";
		carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", CountryCodes.India);
		bill.ABL_OH_LocalTransportCarrier = carrier.PK;
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.CarrierCode), string.Empty, dataProvider.CarrierCode);

			dataProvider = CreateDataProvider();
			bill.ABL_CargoStatus = CargoMovementList.Codes.LocalCargo;
			AssertEquals(nameof(IConsolCargoDataProvider.CarrierCode), string.Empty, dataProvider.CarrierCode);

			dataProvider = CreateDataProvider();
			bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentCargo;
			AssertEquals(nameof(IConsolCargoDataProvider.CarrierCode), "1234", dataProvider.CarrierCode);

			dataProvider = CreateDataProvider();
			bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentToICDSMTP;
			AssertEquals(nameof(IConsolCargoDataProvider.CarrierCode), "1234", dataProvider.CarrierCode);
		});
	}

	public override void TestConsigneeAddress1()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeAddress1), string.Empty, dataProvider.ConsigneeAddress1);

			dataProvider = CreateDataProvider();
			bill.ABL_ConsigneeStreet1 = "ACES";
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeAddress1), "ACES", dataProvider.ConsigneeAddress1);

			bill.ABL_OA_Consignee = GetSampleOrgAddress().PK;
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeAddress1), "Kundalahalli", dataProvider.ConsigneeAddress1);
		});
	}

	public override void TestConsigneeAddress2()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeAddress2), string.Empty, dataProvider.ConsigneeAddress2);

			dataProvider = CreateDataProvider();
			bill.ABL_ConsigneeStreet2 = "Whitefield";
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeAddress2), "Whitefield", dataProvider.ConsigneeAddress2);

			bill.ABL_OA_Consignee = GetSampleOrgAddress().PK;
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeAddress2), "Brookefield", dataProvider.ConsigneeAddress2);
		});
	}

	public override void TestConsigneeAddress3()
	{
		AdditionalDataProviderMock.Setup(x => x.GetConsigneeAddress3(It.IsAny<CGMAsycudaBill>())).Returns("Banglore IN KA 560037");
		AssertEquals("When ConsigneeAddress3 is set", "Banglore IN KA 560037", CreateDataProvider().ConsigneeAddress3);
	}

	public override void TestConsigneeName()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeName), string.Empty, dataProvider.ConsigneeName);

			dataProvider = CreateDataProvider();
			bill.ABL_ConsigneeName = "Wisetech";
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeName), "Wisetech", dataProvider.ConsigneeName);

			bill.ABL_OA_Consignee = GetSampleOrgAddress().PK;
			AssertEquals(nameof(IConsolCargoDataProvider.ConsigneeName), "Brigade", dataProvider.ConsigneeName);
		});
	}

	public override void TestCustomHouseCode()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestCustomHouseCode(header, () => dataProvider.CustomHouseCode);
	}

	public override void TestDestinationCode()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.DestinationCode), string.Empty, dataProvider.DestinationCode);

			dataProvider = CreateDataProvider();
			bill.ABL_CustomsFinalDestinationPort = "INBLR";
			AssertEquals(nameof(IConsolCargoDataProvider.DestinationCode), "INBLR", dataProvider.DestinationCode);
		});
	}

	public override void TestGoodsDescription()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.GoodsDescription), string.Empty, dataProvider.GoodsDescription);

			dataProvider = CreateDataProvider();
			bill.ABL_GoodsDescription = "Running mat";
			AssertEquals(nameof(IConsolCargoDataProvider.GoodsDescription), "Running mat", dataProvider.GoodsDescription);
		});
	}

	public override void TestGrossVolume()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.GrossVolume), 0m, dataProvider.GrossVolume);

			dataProvider = CreateDataProvider();
			bill.ABL_Volume = 0m;
			AssertEquals(nameof(IConsolCargoDataProvider.GrossVolume), 0m, dataProvider.GrossVolume);

			dataProvider = CreateDataProvider();
			bill.ABL_Volume = 121.82m;
			AssertEquals(nameof(IConsolCargoDataProvider.GrossVolume), 121.82m, dataProvider.GrossVolume);
		});
	}

	public override void TestGrossWeight()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.GrossWeight), 0m, dataProvider.GrossWeight);

			dataProvider = CreateDataProvider();
			bill.ABL_GrossWeight = 0m;
			AssertEquals(nameof(IConsolCargoDataProvider.GrossWeight), 0m, dataProvider.GrossWeight);

			dataProvider = CreateDataProvider();
			bill.ABL_GrossWeight = 1.23m;
			AssertEquals(nameof(IConsolCargoDataProvider.GrossWeight), 1.23m, dataProvider.GrossWeight);
		});
	}

	public override void TestHouseBillOfLadingDate()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNull(nameof(IConsolCargoDataProvider.HouseBillOfLadingDate), dataProvider.HouseBillOfLadingDate);

			dataProvider = CreateDataProvider();
			bill.ABL_BillIssueDate = ZDate.Empty;
			AssertNull(nameof(IConsolCargoDataProvider.HouseBillOfLadingDate), dataProvider.HouseBillOfLadingDate);

			dataProvider = CreateDataProvider();
			bill.ABL_BillIssueDate = new ZDate(2022, 3, 16);
			AssertEquals(nameof(IConsolCargoDataProvider.HouseBillOfLadingDate), new DateTime(2022, 3, 16), dataProvider.HouseBillOfLadingDate);
		});
	}

	public override void TestHouseBillOfLadingNo()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.HouseBillOfLadingNo), string.Empty, dataProvider.HouseBillOfLadingNo);

			dataProvider = CreateDataProvider();
			bill.ABL_BillNumber = "123";
			AssertEquals(nameof(IConsolCargoDataProvider.HouseBillOfLadingNo), "123", dataProvider.HouseBillOfLadingNo);
		});
	}

	public override void TestIgmDate()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestIgmDate(header, () => dataProvider.IgmDate);
	}

	public override void TestIgmNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestIgmNumber(header, () => dataProvider.IgmNumber);
	}

	public override void TestImcoCode()
	{
		AdditionalDataProviderMock.Setup(x => x.GetImcoCode(It.IsAny<CGMAsycudaBill>())).Returns("IMCO");
		AssertEquals("When ImcoCode is set", "IMCO", CreateDataProvider().ImcoCode);
	}

	public override void TestImoCodeOfVessel()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestImoCodeOfVessel(header, () => dataProvider.ImoCodeOfVessel);
	}

	public override void TestImporterAddress1()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterAddress1), string.Empty, dataProvider.ImporterAddress1);

			dataProvider = CreateDataProvider();
			bill.ABL_BuyerStreet1 = "ACES";
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterAddress1), "ACES", dataProvider.ImporterAddress1);

			bill.ABL_OA_Buyer = GetSampleOrgAddress().PK;
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterAddress1), "Kundalahalli", dataProvider.ImporterAddress1);
		});
	}

	public override void TestImporterAddress2()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterAddress2), string.Empty, dataProvider.ImporterAddress2);

			dataProvider = CreateDataProvider();
			bill.ABL_BuyerStreet2 = "Whitefield";
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterAddress2), "Whitefield", dataProvider.ImporterAddress2);

			bill.ABL_OA_Buyer = GetSampleOrgAddress().PK;
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterAddress2), "Brookefield", dataProvider.ImporterAddress2);
		});
	}

	public override void TestImporterAddress3()
	{
		AdditionalDataProviderMock.Setup(x => x.GetImporterAddress3(It.IsAny<CGMAsycudaBill>())).Returns("Banglore IN KA 560037");
		AssertEquals("When ImporterAddress3 is set", "Banglore IN KA 560037", CreateDataProvider().ImporterAddress3);
	}

	public override void TestImporterName()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterName), string.Empty, dataProvider.ImporterName);

			dataProvider = CreateDataProvider();
			bill.ABL_BuyerName = "Wisetech";
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterName), "Wisetech", dataProvider.ImporterName);

			bill.ABL_OA_Buyer = GetSampleOrgAddress().PK;
			AssertEquals(nameof(IConsolCargoDataProvider.ImporterName), "Brigade", dataProvider.ImporterName);
		});
	}

	public override void TestItemType()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ItemType), ZString.Empty, dataProvider.ItemType);

			dataProvider = CreateDataProvider();
			bill.ABL_SpecialCargoCode = ShipmentTypeList.Codes.PartShipment;
			AssertEquals(nameof(IConsolCargoDataProvider.ItemType), ShipmentTypeList.Codes.PartShipment, dataProvider.ItemType);
		});
	}

	public override void TestLineNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestLineNumber(header, () => dataProvider.LineNumber);
	}

	public override void TestMarksAndNumbers()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.MarksAndNumbers), string.Empty, dataProvider.MarksAndNumbers);

			dataProvider = CreateDataProvider();
			bill.ABL_MarksAndNumbers = "12";
			AssertEquals(nameof(IConsolCargoDataProvider.MarksAndNumbers), "12", dataProvider.MarksAndNumbers);
		});
	}

	public override void TestMasterBillOfLadingDate()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertNull(nameof(IConsolCargoDataProvider.MasterBillOfLadingDate), dataProvider.MasterBillOfLadingDate);

			dataProvider = CreateDataProvider();
			masterBill.ABL_BillIssueDate = ZDate.Empty;
			AssertNull(nameof(IConsolCargoDataProvider.MasterBillOfLadingDate), dataProvider.MasterBillOfLadingDate);

			dataProvider = CreateDataProvider();
			masterBill.ABL_BillIssueDate = new ZDate(2022, 3, 16);
			AssertEquals(nameof(IConsolCargoDataProvider.MasterBillOfLadingDate), new DateTime(2022, 3, 16), dataProvider.MasterBillOfLadingDate);
		});
	}

	public override void TestMasterBillOfLadingNo()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.MasterBillOfLadingNo), string.Empty, dataProvider.MasterBillOfLadingNo);

			dataProvider = CreateDataProvider();
			masterBill.ABL_BillNumber = "123";
			AssertEquals(nameof(IConsolCargoDataProvider.MasterBillOfLadingNo), "123", dataProvider.MasterBillOfLadingNo);
		});
	}

	public override void TestMessageType()
	{
		AdditionalDataProviderMock.Setup(x => x.GetMessageType(It.IsAny<CGMAsycudaBill>())).Returns("S");
		AssertEquals("When message type is S", "S", CreateDataProvider().MessageType);
	}
	public override void TestMloCode()
	{
		var dataProvider = CreateDataProvider();
		AssertEquals(nameof(IConsolCargoDataProvider.MloCode), string.Empty, dataProvider.MloCode);

		var holder = Factory.New<OrgHeader>();
		holder.OH_Code = "ABCD";
		holder.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", CountryCodes.India);
		bill.ABL_OH_BondHolder = holder.PK;

		dataProvider = CreateDataProvider();
		AssertEquals(nameof(IConsolCargoDataProvider.MloCode), "1234", dataProvider.MloCode);
	}

	public override void TestModeOfTransport()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.ModeOfTransport), string.Empty, dataProvider.ModeOfTransport);

			dataProvider = CreateDataProvider();
			bill.ABL_InlandTransportMode = "SEA";
			AssertEquals(nameof(IConsolCargoDataProvider.ModeOfTransport), "SEA", dataProvider.ModeOfTransport);
		});
	}

	public override void TestNatureOfCargo()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.NatureOfCargo), "OTH", dataProvider.NatureOfCargo);

			dataProvider = CreateDataProvider();
			header.AMA_ContainerMode = "CL";
			AssertEquals(nameof(IConsolCargoDataProvider.NatureOfCargo), "CL", dataProvider.NatureOfCargo);
		});
	}

	public override void TestPackageCode()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.PackageCode), string.Empty, dataProvider.PackageCode);

			dataProvider = CreateDataProvider();
			bill.ABL_ManifestUQ = "KG";
			AssertEquals(nameof(IConsolCargoDataProvider.PackageCode), "KG", dataProvider.PackageCode);
		});
	}

	public override void TestPortOfDestination()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.PortOfDestination), string.Empty, dataProvider.PortOfDestination);

			dataProvider = CreateDataProvider();
			masterBill.ABL_CustomsDischargePort = "XYZ";
			AssertEquals(nameof(IConsolCargoDataProvider.PortOfDestination), "XYZ", dataProvider.PortOfDestination);
		});
	}

	public override void TestPortOfShipment()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.PortOfShipment), string.Empty, dataProvider.PortOfShipment);

			dataProvider = CreateDataProvider();
			masterBill.ABL_RL_NKOrigin = "INBPL";
			AssertEquals(nameof(IConsolCargoDataProvider.PortOfShipment), "INBPL", dataProvider.PortOfShipment);
		});
	}

	public override void TestSubLineNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestSubLineNumber(bill, () => dataProvider.SubLineNumber);
	}

	public override void TestTotalNoOfPackages()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.TotalNoOfPackages), 0, dataProvider.TotalNoOfPackages);

			dataProvider = CreateDataProvider();
			bill.ABL_ManifestQty = 5;
			AssertEquals(nameof(IConsolCargoDataProvider.TotalNoOfPackages), 5, dataProvider.TotalNoOfPackages);
		});
	}

	public override void TestUnitOfVolume()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.UnitOfVolume), string.Empty, dataProvider.UnitOfVolume);

			dataProvider = CreateDataProvider();
			bill.ABL_VolumeUQ = "ml";
			AssertEquals(nameof(IConsolCargoDataProvider.UnitOfVolume), "ml", dataProvider.UnitOfVolume);
		});
	}

	public override void TestUnitOfWeight()
	{
		CombineAssertions(() =>
		{
			var dataProvider = CreateDataProvider();
			AssertEquals(nameof(IConsolCargoDataProvider.UnitOfWeight), string.Empty, dataProvider.UnitOfWeight);

			dataProvider = CreateDataProvider();
			bill.ABL_GrossWeightUQ = "GM";
			AssertEquals(nameof(IConsolCargoDataProvider.UnitOfWeight), "GM", dataProvider.UnitOfWeight);
		});
	}

	public override void TestUnoCode()
	{
		AdditionalDataProviderMock.Setup(x => x.GetUnoCode(It.IsAny<CGMAsycudaBill>())).Returns("UNO");
		AssertEquals("When UnoCode is set", "UNO", CreateDataProvider().UnoCode);
	}

	public override void TestVesselCode()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestVesselCode(header, () => dataProvider.VesselCode);
	}

	public override void TestVoyageNumber()
	{
		var dataProvider = CreateDataProvider();
		DataProviderTestHelper.TestVoyageNumber(header, () => dataProvider.VoyageNumber);
	}

	protected override IConsolCargoDataProvider CreateDataProvider() => SeaCgmCMCHI21DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Consoligm.Cargos.First();

	OrgAddress GetSampleOrgAddress()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Brigade";
		var orgAddress = orgHeader.MainAddress;
		orgAddress.OA_Address1 = "Kundalahalli";
		orgAddress.OA_Address2 = "Brookefield";
		orgAddress.OA_City = "Bengaluru";
		orgAddress.OA_RN_NKCountryCode = "IN";
		orgAddress.OA_State = "KA";
		orgAddress.OA_PostCode = "560036";

		return orgAddress;
	}

	protected override void SetUp()
	{
		base.SetUp();
		header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		bill = header.Bills.AddNew();
		masterBill = header.MasterBill;
	}

	ConsolDataProviderTestHelper DataProviderTestHelper => dataProviderTestHelper ??= new();
	ConsolDataProviderTestHelper dataProviderTestHelper;

	Mock<ISeaCgmCMCHI21AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new();
	Mock<ISeaCgmCMCHI21AdditionalDataProvider> additionalDataProviderMock;

	CGMAsycudaBill bill;
	CGMAsycudaBill masterBill;
}
