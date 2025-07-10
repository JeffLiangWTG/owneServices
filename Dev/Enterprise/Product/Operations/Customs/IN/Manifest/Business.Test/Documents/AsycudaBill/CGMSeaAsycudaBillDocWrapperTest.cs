using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMSeaAsycudaBillDocWrapper))]
sealed class CGMSeaAsycudaBillDocWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("When bill is null", () => new CGMSeaAsycudaBillDocWrapper(null));
			AssertExceptionThrown<ArgumentNullException>("When Header is null", () => new CGMSeaAsycudaBillDocWrapper(Factory.New<CGMAsycudaBill>()));
			AssertNoExceptionThrown("When bill is not null", () => new CGMSeaAsycudaBillDocWrapper(Bill));
		});
	}

	public void TestSubLineNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.SubLineNumber);
			Bill.ABL_CarrierReference = "12";
			AssertEquals("Bill Assigned", "12", Wrapper.SubLineNumber);
		});
	}

	public void TestHBLNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.HBLNumber);
			Bill.ABL_BillNumber = "B123";
			AssertEquals("Bill Assigned", "B123", Wrapper.HBLNumber);
		});
	}

	public void TestHBLDate()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZDate.Empty, Wrapper.HBLDate);
			Bill.ABL_BillIssueDate = new ZDate(2024, 6, 13);
			AssertEquals("Bill Assigned", new ZDate(2024, 6, 13), Wrapper.HBLDate);
		});
	}

	public void TestPortOfShipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.PortOfShipment);
			Header.AMA_RL_NKOrigin = "INBLR";
			AssertEquals("Header Assigned", "INBLR", Wrapper.PortOfShipment);
		});
	}

	public void TestPortOfDestination()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.PortOfDestination);
			Bill.ABL_RL_NKFinalDestination = "INDEL";
			AssertEquals("MasterBill Assigned", "INDEL", Wrapper.PortOfDestination);
		});
	}

	public void TestImporterName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ImporterName);
			Bill.ABL_OA_Buyer = OrgAddress.PK;
			AssertEquals("Bill Importer Assigned", "Apple", Wrapper.ImporterName);

			Bill.ABL_OA_Buyer = ZGuid.Empty;
			Bill.ABL_BuyerName = "Wisetech";
			AssertEquals("Bill Importer Assigned", "Wisetech", Wrapper.ImporterName);
		});
	}

	public void TestImporterAddress()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ImporterAddress);
			Bill.ABL_OA_Buyer = OrgAddress.PK;
			AssertEquals("Bill Importer Assigned", "Brookefield, AECS Layout, Bengaluru, KA, 560037, IN", Wrapper.ImporterAddress);

			Bill.ABL_OA_Buyer = ZGuid.Empty;
			Bill.ABL_BuyerStreet1 = "Wall street";
			Bill.ABL_BuyerStreet2 = "C V Raman Nagar";
			Bill.ABL_BuyerCity = "Bengaluru";
			Bill.ABL_RN_NKBuyerCountry = "IN";
			Bill.ABL_BuyerState = "KA";
			Bill.ABL_BuyerPostcode = "560093";
			AssertEquals("Bill Importer Assigned from details", "Wall street, C V Raman Nagar, Bengaluru, KA, 560093, IN", Wrapper.ImporterAddress);
		});
	}

	public void TestConsigneename()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.Consigneename);
			Bill.ABL_OA_Consignee = OrgAddress.PK;
			AssertEquals("Bill Importer Assigned", "Apple", Wrapper.Consigneename);

			Bill.ABL_OA_Consignee = ZGuid.Empty;
			Bill.ABL_ConsigneeName = "Wisetech";
			AssertEquals("Bill Importer Assigned", "Wisetech", Wrapper.Consigneename);
		});
	}

	public void TestConsigneeAddress()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ConsigneeAddress);
			Bill.ABL_OA_Consignee = OrgAddress.PK;
			AssertEquals("Bill Consignee Assigned", "Brookefield, AECS Layout, Bengaluru, KA, 560037, IN", Wrapper.ConsigneeAddress);

			Bill.ABL_OA_Consignee = ZGuid.Empty;
			Bill.ABL_ConsigneeStreet1 = "Wall street";
			Bill.ABL_ConsigneeStreet2 = "C V Raman Nagar";
			Bill.ABL_ConsigneeCity = "Bengaluru";
			Bill.ABL_RN_NKConsigneeCountry = "IN";
			Bill.ABL_ConsigneeState = "KA";
			Bill.ABL_ConsigneePostcode = "560093";
			AssertEquals("Bill Consignee Assigned from details", "Wall street, C V Raman Nagar, Bengaluru, KA, 560093, IN", Wrapper.ConsigneeAddress);
		});
	}

	public void TestNatureOfCargo()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.NatureOfCargo);
			Bill.ABL_ContainerMode = "C";
			AssertEquals("Bill Assigned", "Containerized", Wrapper.NatureOfCargo);

			Bill.ABL_ContainerMode = "XX";
			AssertEquals("Unknown NatureOfCargo", ZString.Empty, Wrapper.NatureOfCargo);
		});
	}

	public void TestItemType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.ItemType);
			Bill.ABL_SpecialCargoCode = "GC";
			AssertEquals("Bill Assigned", "Govt. Cargo", Wrapper.ItemType);

			Bill.ABL_SpecialCargoCode = "XX";
			AssertEquals("Unknown SpecialCargoCode", ZString.Empty, Wrapper.ItemType);
		});
	}

	public void TestCargoMovement()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.CargoMovement);
			Bill.ABL_CargoStatus = "LC";
			AssertEquals("Bill Assigned", "Local Cargo", Wrapper.CargoMovement);

			Bill.ABL_CargoStatus = "XX";
			AssertEquals("Unknown CargoStatus", ZString.Empty, Wrapper.CargoMovement);
		});
	}

	public void TestDestinationCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.DestinationCode);
			Bill.ABL_CustomsFinalDestinationPort = "INVJA";
			AssertEquals("Bill Assigned", "INVJA", Wrapper.DestinationCode);
		});
	}

	public void TestNumberOfPackage()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", 0, Wrapper.NumberOfPackage);
			Bill.ABL_ManifestQty = 2;
			AssertEquals("Bill Assigned", 2, Wrapper.NumberOfPackage);
		});
	}

	public void TestNumberOfPackageUOM()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.NumberOfPackageUOM);
			Bill.ABL_ManifestUQ = "KGS";
			AssertEquals("Bill Assigned", "KGS", Wrapper.NumberOfPackageUOM);
		});
	}

	public void TestGrossWeight()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", 0m, Wrapper.GrossWeight);
			Bill.ABL_GrossWeight = 2.32m;
			AssertEquals("Bill Assigned", 2.32m, Wrapper.GrossWeight);
		});
	}

	public void TestGrossWeightUOM()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.GrossWeightUOM);
			Bill.ABL_GrossWeightUQ = "GM";
			AssertEquals("Bill Assigned", "GM", Wrapper.GrossWeightUOM);
		});
	}

	public void TestGrossVolume()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", 0m, Wrapper.GrossVolume);
			Bill.ABL_Volume = 4.18m;
			AssertEquals("Bill Assigned", 4.18m, Wrapper.GrossVolume);
		});
	}

	public void TestGrossVolumeUOM()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.GrossVolumeUOM);
			Bill.ABL_VolumeUQ = "LT";
			AssertEquals("Bill Assigned", "LT", Wrapper.GrossVolumeUOM);
		});
	}

	public void TestMarksAndNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.MarksAndNumber);
			Bill.ABL_MarksAndNumbers = "MN123";
			AssertEquals("Bill Assigned", "MN123", Wrapper.MarksAndNumber);
		});
	}

	public void TestGoodsDescription()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.GoodsDescription);
			Bill.ABL_GoodsDescription = "iPhone display";
			AssertEquals("Bill Assigned", "iPhone display", Wrapper.GoodsDescription);
		});
	}

	public void TestUNOCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.UNOCode);
			UNDGSubstance.DG_UNNO = "D911";
			AssertEquals("Bill Assigned", "D911", Wrapper.UNOCode);
		});
	}

	public void TestIMCOCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.IMCOCode);
			UNDGSubstance.DG_Class = "DC12";
			AssertEquals("Bill Assigned", "DC12", Wrapper.IMCOCode);
		});
	}

	public void TestBondNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.BondNumber);
			Bill.BondNumber = "B1123";
			AssertEquals("Bill Assigned", "B1123", Wrapper.BondNumber);
		});
	}

	public void TestCarrierCode()
	{
		AssertEquals("Default", "TBA", Wrapper.CarrierCode);
	}

	public void TestTransportMode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Default", ZString.Empty, Wrapper.TransportMode);
			Bill.ABL_InlandTransportMode = "ROA";
			AssertEquals("Bill Assigned", "ROA", Wrapper.TransportMode);
		});
	}

	public void TestMLOCode()
	{
		AssertEquals("Default", "TBA", Wrapper.MLOCode);
	}

	CGMSeaAsycudaBillDocWrapper Wrapper => wrapper ??= new CGMSeaAsycudaBillDocWrapper(Bill);
	CGMSeaAsycudaBillDocWrapper wrapper;

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= GetHeader();
	CGMAsycudaManifestHeader header;

	CGMAsycudaManifestHeader GetHeader()
	{
		var manifestHeader = Factory.New<CGMAsycudaManifestHeader>();
		manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
		return manifestHeader;
	}

	OrgAddress OrgAddress => orgAddress ??= GetOrgAddress();
	OrgAddress orgAddress;

	OrgAddress GetOrgAddress()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Apple";
		var mainAddress = orgHeader.MainAddress;
		mainAddress.OA_Address1 = "Brookefield";
		mainAddress.OA_Address2 = "AECS Layout";
		mainAddress.OA_City = "Bengaluru";
		mainAddress.OA_RN_NKCountryCode = "IN";
		mainAddress.OA_State = "KA";
		mainAddress.OA_PostCode = "560037";
		return mainAddress;
	}

	UNDGSubstance UNDGSubstance => uNDGSubstance ??= GetUNDGSubstance();
	UNDGSubstance uNDGSubstance;

	UNDGSubstance GetUNDGSubstance()
	{
		var uNDGDataItem = Bill.Packs.AddNew().UNDGs.AddNew();
		uNDGDataItem.DI_DG = Factory.New<UNDGSubstance>().PK;
		return uNDGDataItem.UNDGSubstance;
	}
}
