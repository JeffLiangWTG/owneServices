using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsInventory))]
	sealed class DocWhsInventoryTest : DocumentWrapperTestCase
	{
		#region ZString Fields

		public void TestStatus()
		{
			ReceiveLine.WE_CurrentInventoryStatus = "TST";
			AssertEquals("DocWrapper Status property is incorrect", "TST", DocWrapper.Status);
		}

		public void TestUnitsUQ()
		{
			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_StockKeepingUnit = "TST";
			AssertEquals("DocWrapper UnitsUQ is incorrect", "TST", DocWrapper.UnitsUQ);
		}

		public void TestPackType()
		{
			ReceiveLine.WE_F3_NKPackType = "TST";
			AssertEquals("DocWrapper PackType is incorrect", "TST", DocWrapper.PacksUQ);
		}

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

		#region TestProductDesc

		public void TestProductDesc()
		{
			ReceiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("DocWrapper ProductDescription should be empty", ZString.Empty, DocWrapper.ProductDesc);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_Desc = "TEST";
			AssertEquals("DocWrapper ProductDescription is incorrect", "TEST", DocWrapper.ProductDesc);
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

		public void TestPartAttribute1()
		{
			ReceiveLine.WE_PartAttrib1 = "TEST";
			AssertEquals("DocWrapper PartAttribute1 is incorrect", "TEST", DocWrapper.PartAttribute1);
		}

		public void TestPartAttribute1Name()
		{
			ReceiveLine.Docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute1Name should be empty", ZString.Empty, DocWrapper.PartAttribute1Name);

			ReceiveLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			ReceiveLine.Docket.Client.MiscServ.OM_IMPartAttrib1Name = "TEST";
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "TEST", DocWrapper.PartAttribute1Name);
		}

		public void TestPartAttribute1Name_Translatable()
		{
			ReceiveLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			var miscServ = ReceiveLine.Docket.Client.MiscServ;
			miscServ.OM_IMPartAttrib1Name = "Part Attribute 1";
			AssertEquals("DocWrapper PartAttribute1Name in English", "Part Attribute 1", DocWrapper.PartAttribute1Name);

			var multiLingualString = miscServ.OM_IMPartAttrib1NameMultilingual;
			var resKey = miscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "Part Attribute 1").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "属性1"));
				AssertEquals("DocWrapper PartAttribute1Name in Chinese", "属性1", DocWrapper.PartAttribute1Name);
			}
		}

		public void TestPartAttribute2()
		{
			AssertEquals("DocWrapper PartAttribute2 is incorrect", ZString.Empty, DocWrapper.PartAttribute2);
			ReceiveLine.WE_PartAttrib2 = "TEST";
			AssertEquals("DocWrapper PartAttribute2 is incorrect", "TEST", DocWrapper.PartAttribute2);
		}

		public void TestPartAttribute2Name()
		{
			ReceiveLine.Docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2Name should be empty", ZString.Empty, DocWrapper.PartAttribute2Name);

			ReceiveLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			ReceiveLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = "TEST";
			AssertEquals("DocWrapper PartAttribute2Name is incorrect", "TEST", DocWrapper.PartAttribute2Name);
		}

		public void TestPartAttribute2Name_Translatable()
		{
			ReceiveLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			var miscServ = ReceiveLine.Docket.Client.MiscServ;
			miscServ.OM_IMPartAttrib2Name = "Part Attribute 2";
			AssertEquals("DocWrapper PartAttribute2Name in English", "Part Attribute 2", DocWrapper.PartAttribute2Name);

			var multiLingualString = miscServ.OM_IMPartAttrib2NameMultilingual;
			var resKey = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "Part Attribute 2").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "属性2"));
				AssertEquals("DocWrapper PartAttribute2Name in Chinese", "属性2", DocWrapper.PartAttribute2Name);
			}
		}

		public void TestPartAttribute2WithLabel()
		{
			ReceiveLine.Docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			ReceiveLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			ReceiveLine.WE_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			ReceiveLine.WE_PartAttrib2 = "TEST";
			ReceiveLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);

			ReceiveLine.WE_PartAttrib2 = "TEST";
			ReceiveLine.Docket.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			ReceiveLine.Docket.WD_OH_Client = client.PK;
			ReceiveLine.WE_PartAttrib2 = "TEST";
			var miscServ = client.MiscServ;
			miscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);

			var multiLingualString = miscServ.OM_IMPartAttrib2NameMultilingual;
			var resKey = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute2WithLabel);
			}
		}

		public void TestPartAttribute3WithLabel()
		{
			ReceiveLine.Docket.WD_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			ReceiveLine.Docket.WD_OH_Client = Factory.New<OrgHeader>().PK;
			ReceiveLine.WE_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel shoule be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			ReceiveLine.WE_PartAttrib3 = "TEST";
			ReceiveLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);

			ReceiveLine.WE_PartAttrib3 = "TEST";
			ReceiveLine.Docket.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			ReceiveLine.Docket.WD_OH_Client = client.PK;
			ReceiveLine.WE_PartAttrib3 = "TEST";
			var miscServ = client.MiscServ;
			miscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);

			var multiLingualString = miscServ.OM_IMPartAttrib3NameMultilingual;
			var resKey = miscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute3WithLabel in Chinese", "标签: TEST", DocWrapper.PartAttribute3WithLabel);
			}
		}

		public void TestTrackedSerialNumber()
		{
			AssertEquals("DocWrapper TrackedSerialNumber is incorrect", ZString.Empty, DocWrapper.TrackedSerialNumber);
			ReceiveLine.WE_SerialNumber = "SER4";
			AssertEquals("DocWrapper TrackedSerialNumber is incorrect", "SER4", DocWrapper.TrackedSerialNumber);
		}

		public void TestTrackedSerialNumberName()
		{
			ReceiveLine.WE_SerialNumber = ZString.Empty;
			AssertEquals("DocWrapper TrackedSerialNumberName should be correct", "Tracked Serial Number", DocWrapper.TrackedSerialNumberName);
		}

		public void TestTrackedSerialWithLabel()
		{
			AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

			ReceiveLine.WE_SerialNumber = ZString.Empty;
			AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

			ReceiveLine.WE_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel is incorrect", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);
		}

		public void TestTrackedSerialWithLabel_Translatable()
		{
			ReceiveLine.WE_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel in English", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", new ResourceStringData("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "跟踪序列号"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in Chinese", "跟踪序列号: TEST", DocWrapper.TrackedSerialWithLabel);
			}
		}

		public void TestLocationString()
		{
			AssertEquals("DocWrapper LocationString is incorrect", ".................", DocWrapper.LocationString);

			var whs = Helper.CreateWarehouse("1");
			var locations = Helper.CreateRowAndGenerateLocations(whs, "AA", 4, 3, 2).Locations;
			ReceiveLine.WE_WL = locations[locations.Count - 1].PK;
			AssertEquals("DocWrapper LocationString is incorrect", "AA-4-3-2", DocWrapper.LocationString);
		}

		public void TestPackingDateWithLabel()
		{
			var testDate = ZDate.Today;
			ReceiveLine.WE_PackingDate = testDate;
			AssertEquals("DocWrapper PackingDateWithLabel is incorrect", "Packing Date: " + ReceiveLine.WE_PackingDate.ToShortDateString(), DocWrapper.PackingDateWithLabel);
		}

		public void TestExpiryDateWithLabel()
		{
			var testDate = ZDate.Today;
			ReceiveLine.WE_ExpiryDate = testDate;
			AssertEquals("DocWrapper ExpiryDateWithLabel is incorrect", "Expiry Date: " + ReceiveLine.WE_ExpiryDate.ToShortDateString(), DocWrapper.ExpiryDateWithLabel);
		}

		public void TestVolumeUQ()
		{
			AssertEquals("DocWrapper VolumeUQ should be empty", ZString.Empty, DocWrapper.VolumeUQ);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_CubicUQ = "M3";
			ReceiveLine.WE_OP = product.PK;

			AssertEquals("DocWrapper VolumeUQ should be M3", "M3", DocWrapper.VolumeUQ);
		}

		public void TestWeightUQ()
		{
			AssertEquals("DocWrapper Weight should be empty", ZString.Empty, DocWrapper.WeightUQ);

			ReceiveLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			ReceiveLine.SupplierPart.OP_WeightUQ = "KG";
			AssertEquals("DocWrapper Weight should be KG", "KG", DocWrapper.WeightUQ);
		}

		public void TestPalletID()
		{
			AssertEquals("DocWrapper Pallet ID should be empty", ZString.Empty, DocWrapper.PalletID);
			ReceiveLine.WE_PalletID = "P_ID_001";
			AssertEquals("DocWrapper Pallet ID is incorrect", "P_ID_001", DocWrapper.PalletID);
		}

		#endregion

		#region ZShort Fields

		public void TestLocationColumn()
		{
			ReceiveLine.WE_WL = ZGuid.Empty;
			AssertEquals("DocWrapper LocationColumn should be zero", ZShort.Zero, DocWrapper.LocationColumn);

			ReceiveLine.WE_WL = Factory.New<WhsLocation>().PK;
			ReceiveLine.Location.WLV_Column = 10;
			AssertEquals("DocWrapper LocationColumn is incorrect", new ZShort(10), DocWrapper.LocationColumn);
		}

		public void TestLocationLevel()
		{
			ReceiveLine.WE_WL = ZGuid.Empty;
			AssertEquals("DocWrapper LocationLevel should be zero", ZShort.Zero, DocWrapper.LocationLevel);

			ReceiveLine.WE_WL = Factory.New<WhsLocation>().PK;
			ReceiveLine.Location.WLV_Level = 10;
			AssertEquals("DocWrapper LocationLevel is incorrect", new ZShort(10), DocWrapper.LocationLevel);
		}

		#endregion

		#region ZDateTime Fields

		public void TestPackingDate()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.PackingDate);

			var temporaryDate = ZDate.Today;
			ReceiveLine.WE_PackingDate = temporaryDate;
			AssertEquals(temporaryDate, DocWrapper.PackingDate);
		}

		public void TestExpiryDate()
		{
			AssertEquals(ZDateTime.Empty, DocWrapper.ExpiryDate);

			var temporaryDate = ZDate.Today;
			ReceiveLine.WE_ExpiryDate = temporaryDate;
			AssertEquals(temporaryDate, DocWrapper.ExpiryDate);
		}

		#endregion

		#region ZDecimal Fields

		public void TestVolume()
		{
			AssertEquals("DocWrapper Volume should be 0m.", 0m, docWrapper.Volume);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_Cubic = 1.1m;

			ReceiveLine.WE_OP = product.PK;
			AssertEquals("DocWrapper Volume should be 0m", 0m, DocWrapper.Volume);

			ReceiveLine.WE_TransactionQuantity = 3m;
			AssertEquals("Pre-condition: SupplierPart.OP_Cubic", 1.1m, ReceiveLine.SupplierPart.OP_Cubic);
			AssertEquals("Pre-condition: WE_TransactionQuantity", 3m, ReceiveLine.WE_TransactionQuantity);
			AssertEquals("DocWrapper Volume should be 3.3m", 3.3m, DocWrapper.Volume);
		}

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

		public void TestUnits()
		{
			ReceiveLine.WE_TransactionQuantity = 10;
			AssertEquals("DocWrapper Units is incorrect", new ZDecimal(10), DocWrapper.Units);
		}

		public void TestExpectedReceiptQuantity()
		{
			ReceiveLine.WE_ClientOrderedUnits = 10m;
			AssertEquals("DocWrapper ExpectedReceiptQuantity is incorrect", new ZDecimal(10), DocWrapper.ExpectedReceiptQuantity);
		}

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

		public void TestGroupedReceiveUnits()
		{
			AssertEquals("DocWrapper Grouped Units is incorrect", 0m, DocWrapper.GroupedReceiveUnits);
			DocWrapper.GroupedReceiveUnits = 10m;
			AssertEquals("DocWrapper Grouped Units is incorrect", 10m, DocWrapper.GroupedReceiveUnits);
			DocWrapper.GroupedReceiveUnits = 12m;
			AssertEquals("DocWrapper Grouped Units is incorrect", 12m, DocWrapper.GroupedReceiveUnits);
		}

		public void TestGroupedInventoryUnits()
		{
			AssertEquals("Pre-Condition", 0m, DocWrapper.GroupedInventoryUnits);
			DocWrapper.GroupedInventoryUnits = 10m;
			AssertEquals(10m, DocWrapper.GroupedInventoryUnits);
			DocWrapper.GroupedInventoryUnits = 12m;
			AssertEquals(12m, DocWrapper.GroupedInventoryUnits);
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

		#region Implementation

		protected override void SetUp()
		{
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory)); }
		}
		TestDataSimpleEnvironment data;

		WhsReceive Receive
		{
			get { return receive ?? (receive = Helper.CreateWhsReceive(Data.Org1, Data.Whs1)); }
		}

		WhsReceive receive;

		WhsReceiveLine ReceiveLine
		{
			get { return receiveLine ?? (receiveLine = Receive.Lines.AddNew()); }
		}

		WhsReceiveLine receiveLine;

		DocWhsInventory DocWrapper
		{
			get { return docWrapper ?? (docWrapper = DocWhsInventory.New(ReceiveLine, Factory)); }
		}
		DocWhsInventory docWrapper;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion
	}
}
