using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseInventoryWrapper))]
	sealed class WarehouseInventoryWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region TestCrossDockConsigneeAddress

		public void TestCrossDockConsigneeAddress()
		{
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();

			ReceiveLine.ConsigneeDocAddress.OrganisationPK = consignee1.PK;
			AssertEquals(consignee1.PK, new WarehouseInventoryWrapper(ReceiveLine, Factory).CrossDockConsigneeAddress.Organization.Organisation.PK);

			ReceiveLine.ConsigneeDocAddress.E2_AddressOverride = true;
			ReceiveLine.ConsigneeDocAddress.E2_CompanyName = "OVERRIDEN NAME";
			AssertEquals("OVERRIDEN NAME", new WarehouseInventoryWrapper(ReceiveLine, Factory).CrossDockConsigneeAddress.CompanyName);

			ReceiveLine.ConsigneeDocAddress.E2_AddressOverride = false; // clear up the data.
			ReceiveLine.ConsigneeNameOrPK = consignee2.PK.ToString();
			AssertEquals(consignee2.PK, new WarehouseInventoryWrapper(ReceiveLine, Factory).CrossDockConsigneeAddress.Organization.Organisation.PK);

			ReceiveLine.ConsigneeDocAddress.E2_AddressOverride = true;
			ReceiveLine.ConsigneeDocAddress.E2_Email = "a@a.com";
			AssertEquals("a@a.com", new WarehouseInventoryWrapper(ReceiveLine, Factory).CrossDockConsigneeAddress.Email);
		}

		#endregion

		#region TestCrossDockOrderNumber

		public void TestCrossDockOrderNumber()
		{
			AssertEquals("", new WarehouseInventoryWrapper(ReceiveLine, Factory).CrossDockOrderNumber);

			ReceiveLine.WE_ReceiveCrossDockOrderNo = "R12345";
			AssertEquals("R12345", new WarehouseInventoryWrapper(ReceiveLine, Factory).CrossDockOrderNumber);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			AssertNull(DocWrapper.Product.WrappedObject);
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "ABC123";
			ReceiveLine.WE_OP = part.PK;
			var docWrapper = new WarehouseInventoryWrapper(ReceiveLine, Factory);
			AssertEquals(part, docWrapper.Product.WrappedObject);
		}

		#endregion

		#region TestProductCodeBarcode

		public void TestProductCodeBarcode()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "ABC123";
			ReceiveLine.WE_OP = part.PK;
			var barcode = new TextBarcode(part.OP_PartNum);
			AssertEquals(barcode.TextAs128sFontString, DocWrapper.ProductCodeBarcode);
			AssertEquals("ABC123", DocWrapper.ProductCodeBarcodeNumber);

			var partBarcode1 = ReceiveLine.Product.Parent.PartBarcodes.AddNew();
			partBarcode1.PH_Barcode = "123456";
			partBarcode1.PH_F3_NKPackType = "BAG";
			AssertEquals(barcode.TextAs128sFontString, DocWrapper.ProductCodeBarcode);
			AssertEquals("ABC123", DocWrapper.ProductCodeBarcodeNumber);

			ReceiveLine.Product.Parent.OP_StockKeepingUnit = "BOX";
			AssertEquals(barcode.TextAs128sFontString, DocWrapper.ProductCodeBarcode);
			AssertEquals("ABC123", DocWrapper.ProductCodeBarcodeNumber);

			var partBarcode2 = ReceiveLine.Product.Parent.PartBarcodes.AddNew();
			partBarcode1.PH_Barcode = "654321";
			partBarcode1.PH_F3_NKPackType = "BOX";
			AssertEquals(new TextBarcode("654321").TextAs128sFontString, DocWrapper.ProductCodeBarcode);
			AssertEquals("654321", DocWrapper.ProductCodeBarcodeNumber);
		}

		#endregion

		#region TestPalletIDBarcode

		public void TestPalletIDBarcode()
		{
			ReceiveLine.WE_PalletID = "LP12345";
			var barcode = new TextBarcode(ReceiveLine.WE_PalletID);
			AssertEquals(barcode.TextAs128sFontString, DocWrapper.PalletIDBarcode);
		}

		#endregion

		#region TestLeftOverAttributes

		public void TestLeftOverAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			AssertEquals(ZString.Empty, wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_PartAttrib1 = "PartAttr1";
			AssertEquals(": PartAttr1", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_PartAttrib2 = "PartAttr2";
			AssertEquals(": PartAttr1, : PartAttr2", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_PartAttrib3 = "PartAttr3";
			AssertEquals(": PartAttr1, : PartAttr2, : PartAttr3", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_SerialNumber = "Serial Num";
			AssertEquals(": PartAttr1, : PartAttr2, : PartAttr3, Tracked Serial Number: Serial Num", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_PartAttrib1 = ZString.Empty;
			AssertEquals(": PartAttr2, : PartAttr3, Tracked Serial Number: Serial Num", wrapper.LeftOverAttributes);

			var client = Factory.New<OrgHeader>();
			var miscService = Factory.New<OrgMiscServ>();
			miscService.OM_OH = client.PK;
			miscService.OM_IMPartAttrib1Name = "P1";
			miscService.OM_IMPartAttrib2Name = "P2";
			miscService.OM_IMPartAttrib3Name = "P3";

			receive.WD_OH_Client = client.PK;

			inventoryLine.InDocketLine.WE_PartAttrib1 = "PartAttr1";
			AssertEquals("P1: PartAttr1, P2: PartAttr2, P3: PartAttr3, Tracked Serial Number: Serial Num", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_PartAttrib1 = ZString.Empty;
			AssertEquals("P2: PartAttr2, P3: PartAttr3, Tracked Serial Number: Serial Num", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_SerialNumber = ZString.Empty;
			AssertEquals("P2: PartAttr2, P3: PartAttr3", wrapper.LeftOverAttributes);

			inventoryLine.InDocketLine.WE_PartAttrib2 = ZString.Empty;
			AssertEquals("P3: PartAttr3", wrapper.LeftOverAttributes);
		}

		#endregion

		#region TestFirstDate

		public void TestFirstDate()
		{
			AssertEquals(ZString.Empty, DocWrapper.FirstDate);
			ReceiveLine.WE_PackingDate = new ZDate(1978, 10, 1);
			AssertEquals("Packing Date: 01-Oct-78", DocWrapper.FirstDate);
			ReceiveLine.WE_ExpiryDate = new ZDate(1978, 1, 10);
			AssertEquals("Expiry Date: 10-Jan-78", DocWrapper.FirstDate);
		}

		#endregion

		#region TestSecondDate

		public void TestSecondDateCore()
		{
			AssertEquals(ZString.Empty, DocWrapper.SecondDate);
			ReceiveLine.WE_PackingDate = new ZDate(2010, 05, 11);
			AssertEquals(ZString.Empty, DocWrapper.SecondDate);
			ReceiveLine.WE_ExpiryDate = ZDate.Today;
			AssertEquals("Packing Date: 11-May-10", DocWrapper.SecondDate);
		}

		#endregion

		#region TestArrivalDate

		public void TestArrivalDate()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.ArrivalDate);

			ReceiveLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(2010, 05, 11);
			AssertEquals(new ZDateTime(2010, 05, 11), DocWrapper.ArrivalDate);
		}

		#endregion

		#region TestHoldCode

		protected override void TestHoldCodeCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocWrapper.HoldCode);
			ReceiveLine.WE_WHC_NKCurrentInventoryHeldCode = "HELD";
			AssertEquals("HELD", DocWrapper.HoldCode);
		}

		#endregion

		#region TestHoldReason

		protected override void TestHoldReasonCore()
		{
			AssertEquals("Pre-condition", ZString.Empty, DocWrapper.HoldCode);
			ReceiveLine.WE_CurrentHoldReason = "ABC";
			AssertEquals("ABC", DocWrapper.HoldReason);
		}

		#endregion

		#region TestReceivedQty

		public void TestGroupedReceiveUnitsAndGroupedInventoryUnitsViaAddUnitValuesMethod()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m);
			Factory.Save();

			var docWrapper = new WarehouseInventoryWrapper(inventory1.InDocketLine, Factory);
			AssertEquals("Pre-condition", "0", docWrapper.ReceivedQty);

			inventory1.InDocketLine.WE_TransactionQuantity = 12m;
			inventory1.InDocketLine.WE_StockOnHand = 6m;
			docWrapper.IncrementUnitValues(inventory1.InDocketLine);

			AssertEquals("12", docWrapper.ReceivedQty);
			AssertEquals("6", docWrapper.InventoryQty);

			inventory2.InDocketLine.WE_TransactionQuantity = 0.337m;
			inventory2.InDocketLine.WE_StockOnHand = 0.563m;
			docWrapper.IncrementUnitValues(inventory2.InDocketLine);

			data.Part1.OP_CountDecimalPlaces = 2;
			AssertEquals("12.34", docWrapper.ReceivedQty);
			AssertEquals("6.56", docWrapper.InventoryQty);
		}

		#endregion

		#region TestGroupedReceivedWeight

		public void TestGroupedReceivedWeightCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "M3");
			var notify = new TestNotificationBuffer();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1, "123");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, locationA1, "123");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "AD1", notify);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -8m, "A-1", palletID: "123");
			Helper.CreateWhsPickLine(adjustmentLine, inventory1, 8m);
			adjustment.FinaliseDocket();
			AssertEquals("Precondition - ensure adjustment is finalised", true, adjustment.IsFinalised);
			AssertEquals("Precondition", 2m, inventory1.WI_TotalUnits);
			AssertEquals("Precondition", 8m, inventory2.WI_TotalUnits);
			Factory.Save();

			var inventoryWrapper = new WarehouseInventoryWrapper(inventory1.InDocketLine, Factory);
			AssertEquals("20.0 KG", inventoryWrapper.GroupedReceivedWeight); //2 KG per unit * 10 total units should be 20 KG
			inventoryWrapper.IncrementUnitValues(inventory2.InDocketLine);
			AssertEquals("36.0 KG", inventoryWrapper.GroupedReceivedWeight); //2 KG per unit * 8 total units = 16 KG + 20 KG(inventory1) = 36 KG
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			ReceiveLine.WE_CurrentInventoryStatus = "TST";
			AssertEquals("DocWrapper Status property is incorrect", "TST", DocWrapper.Status);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("DocWrapper UnitsUQ is incorrect", "TST", DocWrapper.UnitsUQ);
		}

		#endregion

		#region TestPackType

		public void TestPackType()
		{
			ReceiveLine.WE_F3_NKPackType = "TST";
			AssertEquals("DocWrapper PackType is incorrect", "TST", DocWrapper.PacksUQ);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			ReceiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper PartNumber should be empty", ZString.Empty, DocWrapper.ProductCode);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_PartNum = "PART";
			AssertEquals("DocWrapper PartNumber is incorrect", "PART", DocWrapper.ProductCode);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			ReceiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductDescription should be empty", ZString.Empty, DocWrapper.ProductDescription);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_Desc = "TEST";
			AssertEquals("DocWrapper ProductDescription is incorrect", "TEST", DocWrapper.ProductDescription);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			ReceiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductBrandName should be empty", ZString.Empty, DocWrapper.ProductBrandName);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_Brand = "TEST BRAND";
			AssertEquals("DocWrapper ProductBrandName is incorrect", "TEST BRAND", DocWrapper.ProductBrandName);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			ReceiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductModel should be empty", ZString.Empty, DocWrapper.ProductModel);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_Model = "TEST MODEL";
			AssertEquals("DocWrapper ProductModel is incorrect", "TEST MODEL", DocWrapper.ProductModel);
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			AssertEquals("DocWrapper PartAttribute1Barcode is incorrect", ZString.Empty, DocWrapper.PartAttribute1Barcode);

			ReceiveLine.WE_PartAttrib1 = "TEST";
			AssertEquals("DocWrapper PartAttribute1 is incorrect", "TEST", DocWrapper.PartAttribute1);
			AssertEquals("DocWrapper PartAttribute1Barcode is incorrect", "ÈTESTlÊ", DocWrapper.PartAttribute1Barcode);
		}

		public void TestPartAttribute1Name()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "TEST";
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute1Name should be empty", ZString.Empty, wrapper.PartAttribute1Name);

			receive.WD_OH_Client = data.Org1.PK;
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "TEST", wrapper.PartAttribute1Name);
		}

		public void TestPartAttribute1Name_Translatable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			data.Org1.MiscServ.OM_IMPartAttrib1Name = "TEST";
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			AssertEquals("PartAttribute1Name in English.", "TEST", wrapper.PartAttribute1Name);

			var resKey = data.Org1.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(data.Org1.MiscServ, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("PartAttribute1Name in Chinese.", "测试", wrapper.PartAttribute1Name);
			}
		}

		public void TestPartAttribute1WithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel should be empty", ZString.Empty, wrapper.PartAttribute1WithLabel);

			receive.WD_OH_Client = data.Org1.PK;
			inventoryLine.InDocketLine.WE_PartAttrib1 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel should be empty", ZString.Empty, wrapper.PartAttribute1WithLabel);

			inventoryLine.InDocketLine.WE_PartAttrib1 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "Attribute 1: TEST", wrapper.PartAttribute1WithLabel);

			inventoryLine.InDocketLine.WE_PartAttrib1 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "LABEL: TEST", wrapper.PartAttribute1WithLabel);
		}

		public void TestPartAttribute1WithLabel_Translatable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			inventoryLine.InDocketLine.WE_PartAttrib1 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("PartAttribute1WithLabel in English", "LABEL: TEST", wrapper.PartAttribute1WithLabel);

			var resKey = receive.Client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(receive.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute1WithLabel in Chinese", "标签: TEST", wrapper.PartAttribute1WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute2

		public void TestPartAttribute2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			AssertEquals("DocWrapper PartAttribute2 is incorrect", ZString.Empty, wrapper.PartAttribute2);
			AssertEquals("DocWrapper PartAttribute2Barcode is incorrect", ZString.Empty, wrapper.PartAttribute2Barcode);
			inventoryLine.InDocketLine.WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2 is incorrect", "TEST", wrapper.PartAttribute2);
			AssertEquals("DocWrapper PartAttribute2Barcode is incorrect", "ÈTESTlÊ", wrapper.PartAttribute2Barcode);
		}

		protected override void TestPartAttribute2NameCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2Name should be empty", ZString.Empty, wrapper.PartAttribute2Name);

			receive.WD_OH_Client = data.Org1.PK;
			receive.Client.MiscServ.OM_IMPartAttrib2Name = "TEST";
			AssertEquals("DocWrapper PartAttribute2Name is incorrect", "TEST", wrapper.PartAttribute2Name);
		}

		public void TestPartAttribute2Name_Translatable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			data.Org1.MiscServ.OM_IMPartAttrib2Name = "TEST";
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			AssertEquals("PartAttribute2Name in English.", "TEST", wrapper.PartAttribute2Name);

			var resKey = data.Org1.MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(data.Org1.MiscServ, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("PartAttribute2Name in Chinese.", "测试", wrapper.PartAttribute2Name);
			}
		}

		public void TestPartAttribute2WithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, wrapper.PartAttribute2WithLabel);

			receive.WD_OH_Client = data.Org1.PK;
			inventoryLine.InDocketLine.WE_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, wrapper.PartAttribute2WithLabel);

			inventoryLine.InDocketLine.WE_PartAttrib2 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", wrapper.PartAttribute2WithLabel);

			inventoryLine.InDocketLine.WE_PartAttrib2 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "LABEL: TEST", wrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			inventoryLine.InDocketLine.WE_PartAttrib2 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("PartAttribute2WithLabel in English", "LABEL: TEST", wrapper.PartAttribute2WithLabel);

			var resKey = receive.Client.MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(receive.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute2WithLabel in Chinese", "标签: TEST", wrapper.PartAttribute2WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute3

		public void TestPartAttribute3Name_Translatable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			data.Org1.MiscServ.OM_IMPartAttrib3Name = "TEST";
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			AssertEquals("PartAttribute3Name in English.", "TEST", wrapper.PartAttribute3Name);

			var resKey = data.Org1.MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(data.Org1.MiscServ, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("PartAttribute3Name in Chinese.", "测试", wrapper.PartAttribute3Name);
			}
		}

		public void TestPartAttribute3WithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			receive.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, wrapper.PartAttribute3WithLabel);
			AssertEquals("DocWrapper PartAttribute3Barcode is incorrect", ZString.Empty, wrapper.PartAttribute2Barcode);

			receive.WD_OH_Client = Factory.New<OrgHeader>().PK;
			inventoryLine.InDocketLine.WE_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, wrapper.PartAttribute3WithLabel);

			inventoryLine.InDocketLine.WE_PartAttrib3 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", wrapper.PartAttribute3WithLabel);
			AssertEquals("DocWrapper PartAttribute3Barcode is incorrect", "ÈTESTlÊ", wrapper.PartAttribute3Barcode);

			ReceiveLine.WE_PartAttrib3 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "LABEL: TEST", wrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 2m);
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);
			inventoryLine.InDocketLine.WE_PartAttrib3 = "TEST";
			receive.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("PartAttribute3WithLabel in English", "LABEL: TEST", wrapper.PartAttribute3WithLabel);

			var resKey = receive.Client.MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(receive.Client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute3WithLabel in Chinese", "标签: TEST", wrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine.WE_SerialNumber = "Hello";
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(receiveLine, Factory);
			AssertEquals("TrackedSerialNumber", "Hello", wrapper.TrackedSerialNumber);
		}

		public void TestTrackedSerialWithLabel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine.WE_SerialNumber = "from the";
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(receiveLine, Factory);
			AssertEquals("TrackedSerialWithLabel", "Tracked Serial Number: from the", wrapper.TrackedSerialWithLabel);
		}

		public void TrackedSerialBarcode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part2, 2m);
			receiveLine.WE_SerialNumber = "past";
			Factory.Save();

			var wrapper = new WarehouseInventoryWrapper(receiveLine, Factory);
			AssertEquals("TrackedSerialBarcode", "ÌpastkÎ", wrapper.TrackedSerialBarcode);
		}

		public void TestTrackedSerialNumber_RolledUpAttribute()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			var wrapper = new WarehouseInventoryWrapper(receiveLine, Factory);
			AssertEquals("Precondition", "", wrapper.TrackedSerialNumber);
			wrapper.MarkSerialRolledUp();
			AssertEquals("DocWrapper Serial Number is incorrectly rolled up.", "Multiple", wrapper.TrackedSerialNumber);
		}

		#endregion

		#region TestRFConfirm

		public void TestRFConfirm()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			Factory.Save();
			var wrapper = new WarehouseInventoryWrapper(inventoryLine.InDocketLine, Factory);

			var consignee = Factory.New<OrgHeader>();
			var orgPart = data.Part1.RelatedOrganisations.AddNew();
			orgPart.OU_OH = consignee.PK;
			receive.WD_OH_Client = consignee.PK;
			orgPart.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertEquals("", wrapper.RFConfirm);
			AssertEquals("", wrapper.RFConfirmBarcode);
			AssertEquals("", wrapper.RFConfirmName);

			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.None;
			AssertEquals("", wrapper.RFConfirm);
			AssertEquals("", wrapper.RFConfirmBarcode);
			AssertEquals("", wrapper.RFConfirmName);

			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertEquals("", wrapper.RFConfirm);
			AssertEquals("", wrapper.RFConfirmBarcode);
			AssertEquals("", wrapper.RFConfirmName);

			inventoryLine.InDocketLine.WE_PartAttrib1 = "P1";
			inventoryLine.InDocketLine.WE_PartAttrib2 = "P2";
			inventoryLine.InDocketLine.WE_PartAttrib3 = "P3";
			inventoryLine.InDocketLine.WE_SerialNumber = "Serial";
			receive.Client.MiscServ.OM_IMPartAttrib1Name = "P1Name";
			receive.Client.MiscServ.OM_IMPartAttrib2Name = "P2Name";
			receive.Client.MiscServ.OM_IMPartAttrib3Name = "P3Name";

			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute1;
			AssertEquals("P1", wrapper.RFConfirm);
			AssertEquals("ÈP1sÊ", wrapper.RFConfirmBarcode);
			AssertEquals("P1Name", wrapper.RFConfirmName);

			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute2;
			AssertEquals("P2", wrapper.RFConfirm);
			AssertEquals("ÈP2uÊ", wrapper.RFConfirmBarcode);
			AssertEquals("P2Name", wrapper.RFConfirmName);

			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.PartAttribute3;
			AssertEquals("P3", wrapper.RFConfirm);
			AssertEquals("ÈP3wÊ", wrapper.RFConfirmBarcode);
			AssertEquals("P3Name", wrapper.RFConfirmName);

			orgPart.OU_RFAttributeConfirm = RFAttributeConfirmCode.Codes.SerialNumber;
			AssertEquals("Serial", wrapper.RFConfirm);
			AssertEquals("ÈSerialcÊ", wrapper.RFConfirmBarcode);
			AssertEquals("Tracked Serial Number", wrapper.RFConfirmName);
		}

		#endregion

		#region  TestLocationString

		public void TestLocationString()
		{
			AssertEquals("DocWrapper LocationString is incorrect", ".................", DocWrapper.LocationString);

			var whs = Helper.CreateWarehouse("1");
			var locations = Helper.CreateRowAndGenerateLocations(whs, "AA", 4, 3, 2).Locations;
			ReceiveLine.WE_WL = locations[locations.Count - 1].PK;
			AssertEquals("DocWrapper LocationString is incorrect", "AA-4-3-2", DocWrapper.LocationString);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var testDate = ZDate.Today;
			ReceiveLine.WE_PackingDate = testDate;
			AssertEquals("DocWrapper PackingDateWithLabel is incorrect", "Packing Date: " + ReceiveLine.WE_PackingDate.ToShortDateString(), DocWrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var testDate = ZDate.Today;
			ReceiveLine.WE_ExpiryDate = testDate;
			AssertEquals("DocWrapper ExpiryDateWithLabel is incorrect", "Expiry Date: " + ReceiveLine.WE_ExpiryDate.ToShortDateString(), DocWrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestWeightUQ

		public void TestWeightUQ()
		{
			AssertEquals("DocWrapper Weight should be empty", ZString.Empty, DocWrapper.WeightUQ);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_WeightUQ = "KG";
			AssertEquals("DocWrapper Weight should be KG", "KG", DocWrapper.WeightUQ);
		}

		#endregion

		#region TestVolumeUQ

		public void TestVolumeUQ()
		{
			AssertEquals("DocWrapper VolumeUQ should be empty", ZString.Empty, DocWrapper.VolumeUQ);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_CubicUQ = "M3";
			ReceiveLine.WE_OP = product.PK;

			AssertEquals("DocWrapper VolumeUQ should be M3", "M3", DocWrapper.VolumeUQ);
		}

		#endregion

		#region TestVolume

		public void TestVolume()
		{
			AssertEquals("DocWrapper Volume should be 0m", 0m, DocWrapper.Volume);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_Cubic = 1.1m;

			ReceiveLine.WE_OP = product.PK;
			AssertEquals("DocWrapper Volume should be 0m", 0m, DocWrapper.Volume);

			ReceiveLine.WE_TransactionQuantity = 3m;
			AssertEquals("Pre-condition: SupplierPart.OP_Cubic", 1.1m, ReceiveLine.SupplierPart.OP_Cubic);
			AssertEquals("Pre-condition: WE_TransactionQuantity", 3m, ReceiveLine.WE_TransactionQuantity);
			AssertEquals("DocWrapper Volume should be 3.3m", 3.3m, DocWrapper.Volume);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			AssertEquals("DocWrapper Pallet ID should be empty", ZString.Empty, DocWrapper.PalletID);
			ReceiveLine.WE_PalletID = "P_ID_001";
			AssertEquals("DocWrapper Pallet ID is incorrect", "P_ID_001", DocWrapper.PalletID);
		}

		#endregion

		#region TestLocationColumn

		public void TestLocationColumn()
		{
			ReceiveLine.WE_WL = ZGuid.Empty;
			AssertEquals("DocWrapper LocationColumn should be zero", ZShort.Zero, DocWrapper.LocationColumn);

			ReceiveLine.WE_WL = Factory.New<WhsLocation>().PK;
			ReceiveLine.Location.WLV_Column = 10;
			AssertEquals("DocWrapper LocationColumn is incorrect", new ZShort(10), DocWrapper.LocationColumn);
		}

		#endregion

		#region TestLocationLevel

		public void TestLocationLevel()
		{
			ReceiveLine.WE_WL = ZGuid.Empty;
			AssertEquals("DocWrapper LocationLevel should be zero", ZShort.Zero, DocWrapper.LocationLevel);

			ReceiveLine.WE_WL = Factory.New<WhsLocation>().PK;
			ReceiveLine.Location.WLV_Level = 10;
			AssertEquals("DocWrapper LocationLevel is incorrect", new ZShort(10), DocWrapper.LocationLevel);
		}

		#endregion

		#region TestPackingDate

		public void TestPackingDate()
		{
			AssertEquals(ZDate.Empty, DocWrapper.PackingDate);
			AssertEquals("", DocWrapper.PackingDateBarcode);
			AssertEquals("", DocWrapper.PackingDateFormatted);

			var temporaryDate = new ZDate(2012, 4, 29);
			ReceiveLine.WE_PackingDate = temporaryDate;
			AssertEquals(temporaryDate, DocWrapper.PackingDate);
			AssertEquals("È29-Apr-12oÊ", DocWrapper.PackingDateBarcode);
			AssertEquals("29.04.2012", DocWrapper.PackingDateFormatted);

			PackingRegistry.Instance.ProductLabelDateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "yyMMdd");
			AssertEquals("120429", DocWrapper.PackingDateFormatted);
		}

		#endregion

		#region TestExpiryDate

		public void TestExpiryDate()
		{
			AssertEquals(ZDate.Empty, DocWrapper.ExpiryDate);
			AssertEquals("", DocWrapper.ExpiryDateBarcode);
			AssertEquals("", DocWrapper.ExpiryDateFormatted);

			var temporaryDate = new ZDate(2012, 4, 29);
			ReceiveLine.WE_ExpiryDate = temporaryDate;
			AssertEquals(temporaryDate, DocWrapper.ExpiryDate);
			AssertEquals("È29-Apr-12oÊ", DocWrapper.ExpiryDateBarcode);
			AssertEquals("29.04.2012", DocWrapper.ExpiryDateFormatted);

			PackingRegistry.Instance.ProductLabelDateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "yyMMdd");
			AssertEquals("120429", DocWrapper.ExpiryDateFormatted);
		}

		#endregion

		#region TestWeight

		public void TestWeight()
		{
			AssertEquals("DocWrapper Weight should be 0m", 0m, DocWrapper.Weight);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			AssertEquals("DocWrapper Weight should be 0m", 0m, DocWrapper.Weight);

			ReceiveLine.SupplierPart.OP_Weight = 12m;
			AssertEquals("DocWrapper Weight should be 0m", 0m, DocWrapper.Weight);

			ReceiveLine.WE_TransactionQuantity = 10m;
			AssertEquals("DocWrapper Weight should be 120m", 120m, DocWrapper.Weight);
		}

		#endregion

		#region TestPackQty

		public void TestPackQty()
		{
			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			OrgHeader org = helper.CreateClient();
			OrgSupplierPart part = helper.CreateProduct(org, "P1");

			ReceiveLine.WE_OP = part.PK;
			ReceiveLine.WE_F3_NKPackType = "CTN";
			AssertEquals("DocWrapper PackQty should be zero", ZDecimal.Zero, DocWrapper.PackQty);

			ReceiveLine.WE_TransactionQuantity = 120;
			AssertEquals("DocWrapper PackQty is incorrect", 10m, DocWrapper.PackQty);
		}

		#endregion

		#region TestVarianceCore

		protected override void TestVarianceCore()
		{
			ReceiveLine.WE_ClientOrderedUnits = 15;
			ReceiveLine.WE_TransactionQuantity = 10;
			AssertEquals("DocWrapper Variance should be -5", -5m, DocWrapper.Variance);

			ReceiveLine.WE_ClientOrderedUnits = 5;
			ReceiveLine.WE_TransactionQuantity = 10;
			AssertEquals("DocWrapper Variance should be 5", 5m, DocWrapper.Variance);

			ReceiveLine.WE_ClientOrderedUnits = 10;
			ReceiveLine.WE_TransactionQuantity = 10;
			AssertEquals("DocWrapper Variance should be 0", 0m, DocWrapper.Variance);
		}

		#endregion

		#region TestUnits

		public void TestUnits()
		{
			ReceiveLine.WE_TransactionQuantity = 10;
			AssertEquals("DocWrapper Units is incorrect", new ZDecimal(10), DocWrapper.Units);
		}

		#endregion

		#region TestExpectedReceiptQuantity

		public void TestExpectedReceiptQuantity()
		{
			ReceiveLine.WE_ClientOrderedUnits = 10m;
			AssertEquals("DocWrapper ExpectedReceiptQuantity is incorrect", new ZDecimal(10), DocWrapper.ExpectedReceiptQuantity);
		}

		#endregion

		#region TestPallets

		public void TestPallets()
		{
			ReceiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper Pallets should be zero", ZDecimal.Zero, DocWrapper.Pallets);

			WhsTestHelperFunctions helper = new WhsTestHelperFunctions(Factory);
			OrgHeader org = helper.CreateClient();
			OrgSupplierPart part = helper.CreateProduct(org, "P1");
			helper.CreateProductUnit(part, "PLT", 5);

			ReceiveLine.WE_OP = part.PK;
			AssertEquals("DocWrapper Pallets should be zero", ZDecimal.Zero, DocWrapper.Pallets);

			ReceiveLine.WE_TransactionQuantity = 10;
			AssertEquals("DocWrapper Pallets is incorrect", 2m, DocWrapper.Pallets);
		}

		#endregion

		#region TestGroupedUnits

		public void TestGroupedUnits()
		{
			AssertEquals("DocWrapper Grouped Units is incorrect", new ZDecimal(0), DocWrapper.GroupedUnits);
			DocWrapper.GroupedUnits = 10m;
			AssertEquals("DocWrapper Grouped Units is incorrect", new ZDecimal(10), DocWrapper.GroupedUnits);
			DocWrapper.GroupedUnits = 12m;
			AssertEquals("DocWrapper Grouped Units is incorrect", new ZDecimal(12), DocWrapper.GroupedUnits);
		}

		#endregion

		#region TestLineNo

		public void TestLineNo()
		{
			var receiveLine = Factory.New<WhsReceiveLine>();
			receiveLine.WE_LineNo = 2;

			var docWrapper = new WarehouseInventoryWrapper(receiveLine, Factory);
			AssertEquals(nameof(docWrapper.LineNo), "00002", docWrapper.LineNo);
		}

		#endregion

		#region Custom Attributes

		public void TestCustomAttrib1()
		{
			ReceiveLine.WE_CustomAttrib1 = "CA1";
			AssertEquals("CA1", DocWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			ReceiveLine.WE_CustomAttrib2 = "CA2";
			AssertEquals("CA2", DocWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			ReceiveLine.WE_CustomAttrib3 = "CA3";
			AssertEquals("CA3", DocWrapper.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			ReceiveLine.WE_CustomAttrib4 = "CA4";
			AssertEquals("CA4", DocWrapper.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			ReceiveLine.WE_CustomAttrib5 = "CA5";
			AssertEquals("CA5", DocWrapper.CustomAttrib5);
		}

		public void TestCustomAttrib6()
		{
			ReceiveLine.WE_CustomAttrib6 = "CA6";
			AssertEquals("CA6", DocWrapper.CustomAttrib6);
		}

		public void TestCustomDate1()
		{
			ReceiveLine.WE_CustomDate1 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			ReceiveLine.WE_CustomDate2 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			ReceiveLine.WE_CustomDate3 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.CustomDate3);
		}

		public void TestCustomDate4()
		{
			ReceiveLine.WE_CustomDate4 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.CustomDate4);
		}

		public void TestCustomDate5()
		{
			ReceiveLine.WE_CustomDate5 = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, DocWrapper.CustomDate5);
		}

		public void TestCustomDecimal1()
		{
			ReceiveLine.WE_CustomDecimal1 = 10m;
			AssertEquals(10m, DocWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			ReceiveLine.WE_CustomDecimal2 = 10m;
			AssertEquals(10m, DocWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			ReceiveLine.WE_CustomDecimal3 = 10m;
			AssertEquals(10m, DocWrapper.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			ReceiveLine.WE_CustomDecimal4 = 10m;
			AssertEquals(10m, DocWrapper.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			ReceiveLine.WE_CustomDecimal5 = 10m;
			AssertEquals(10m, DocWrapper.CustomDecimal5);
		}

		public void TestCustomFlag1()
		{
			ReceiveLine.WE_CustomFlag1 = true;
			AssertEquals(true, DocWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			ReceiveLine.WE_CustomFlag2 = true;
			AssertEquals(true, DocWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			ReceiveLine.WE_CustomFlag3 = true;
			AssertEquals(true, DocWrapper.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			ReceiveLine.WE_CustomFlag4 = true;
			AssertEquals(true, DocWrapper.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			ReceiveLine.WE_CustomFlag5 = true;
			AssertEquals(true, DocWrapper.CustomFlag5);
		}

		public void TestCustomTextBlob1()
		{
			ReceiveLine.WE_CustomTextBlob1 = "TEXT";
			AssertEquals("TEXT", DocWrapper.CustomTextBlob1);
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override void SetAttribute1Core() => ReceiveLine.WE_PartAttrib1 = "AT1";

		protected override void SetAttribute2Core() => ReceiveLine.WE_PartAttrib2 = "AT2";

		protected override void SetAttribute3Core() => ReceiveLine.WE_PartAttrib3 = "AT3";

		protected override void SetExpiryDateCore() => ReceiveLine.WE_ExpiryDate = ZDate.Today.AddDays(7);

		protected override void SetPackingDateCore() => ReceiveLine.WE_PackingDate = ZDate.Today;

		protected override void SetSerialNumberCore() => ReceiveLine.WE_SerialNumber = "SNX";

		protected override WarehouseGenericLineWrapper SetProductDGCore()
		{
			var part = Factory.New<OrgSupplierPart>();
			var undg = Factory.New<UNDGSubstance>();
			undg.DG_UNNO = "9001";
			undg.DG_Variant = "a";
			part.UNDGs.AddNew().DI_DG = undg.PK;
			ReceiveLine.WE_OP = part.PK;
			return new WarehouseInventoryWrapper(ReceiveLine, Factory);
		}

		#endregion

		#region Implementation

		WhsReceiveLine ReceiveLine => docketLine ?? (docketLine = Factory.New<WhsReceiveLine>());

		WhsReceiveLine docketLine;

		WarehouseInventoryWrapper DocWrapper => docWrapper ?? (docWrapper = new WarehouseInventoryWrapper(ReceiveLine, Factory));
		WarehouseInventoryWrapper docWrapper;

		#region Abstract members Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocWrapper;
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CrossDockConsigneeAddress : 
DangerousGoodsSubstance :  is null
ExtendedLinePrice :  is null
ManufacturerAddress : 
Product : (No Default Field Value Available on Product)
RecommendedUnitPrice : 
Registry : (No Default Field Value Available on Registry)
UnitDiscountAmount : 
UnitDiscountPercent : 
UnitPriceAfterDiscount : 
UnitsMet : 
UnitsOrdered : 
UnitsPicked : 
UnitsShort :
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new WarehouseInventoryWrapper(null, Factory);
		}

		#endregion

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject bizO)
		{
			return new WarehouseInventoryWrapper((WhsReceiveLine)bizO, Factory);
		}

		protected override BusinessObject GetNewBusinessObject() => ReceiveLine;

		#endregion
	}
}
