using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseStocktakeLineWrapper))]
	sealed class WarehouseStocktakeLineWrapperTest : WarehouseGenericLineWrapperTest
	{
		#region Properties

		#region TestLocationString

		public void TestLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var locations = Helper.CreateRowAndGenerateLocations(whs, "AA", 1, 1, 2).Locations;
			StocktakeLine.WU_WL = locations[0].PK;
			AssertEquals("DocWrapper LocationString is incorrect", "AA-1-1-1", StocktakeLineWrapper.LocationString);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var testDate = ZDate.Today;
			StocktakeLine.WU_PackingDate = testDate;
			AssertEquals("DocWrapper PackingDateWithLabel is incorrect", "Packing Date: " + testDate.ToShortDateString(), StocktakeLineWrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var testDate = ZDate.Today;
			StocktakeLine.WU_ExpiryDate = testDate;
			AssertEquals("DocWrapper ExpiryDateWithLabel is incorrect", "Expiry Date: " + testDate.ToShortDateString(), StocktakeLineWrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			StocktakeLine.WU_PartAttrib1 = "TEST";
			AssertEquals("TEST", StocktakeLineWrapper.PartAttribute1);
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			StocktakeLine.WU_SerialNumber = "TEST";
			AssertEquals("TEST", StocktakeLineWrapper.TrackedSerialNumber);
		}

		#endregion

		#region TestPartAttribute1Name

		public void TestPartAttribute1Name()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute1Name should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute1Name);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib1Name = "TEST";
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "TEST", StocktakeLineWrapper.PartAttribute1Name);
		}

		public void TestPartAttribute1Name_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;
			client.MiscServ.OM_IMPartAttrib1Name = "TEST";
			AssertEquals("PartAttribute1Name in English", "TEST", StocktakeLineWrapper.PartAttribute1Name);

			var resKey = client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "TEST").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("PartAttribute1Name in Chinese", "测试", StocktakeLineWrapper.PartAttribute1Name);
			}
		}

		#endregion

		#region TestPartAttribute1WithLabel

		public void TestPartAttribute1WithLabel()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute1WithLabel);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.WU_PartAttrib1 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute1WithLabel);

			StocktakeLine.WU_PartAttrib1 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "Attribute 1: TEST", StocktakeLineWrapper.PartAttribute1WithLabel);

			StocktakeLine.WU_PartAttrib1 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "LABEL: TEST", StocktakeLineWrapper.PartAttribute1WithLabel);
		}

		public void TestPartAttribute1WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			StocktakeLine.WU_PartAttrib1 = "TEST";
			miscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("PartAttribute1WithLabel in English", "LABEL: TEST", StocktakeLineWrapper.PartAttribute1WithLabel);

			var resKey = miscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute1WithLabel in Chinese", "标签: TEST", StocktakeLineWrapper.PartAttribute1WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute2WithLabel

		public void TestPartAttribute2WithLabel()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute2WithLabel);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.WU_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute2WithLabel);

			StocktakeLine.WU_PartAttrib2 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", StocktakeLineWrapper.PartAttribute2WithLabel);

			StocktakeLine.WU_PartAttrib2 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "LABEL: TEST", StocktakeLineWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			StocktakeLine.WU_PartAttrib2 = "TEST";
			miscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("PartAttribute2WithLabel in English", "LABEL: TEST", StocktakeLineWrapper.PartAttribute2WithLabel);

			var resKey = miscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute2WithLabel in Chinese", "标签: TEST", StocktakeLineWrapper.PartAttribute2WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute3WithLabel

		public void TestPartAttribute3WithLabel()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute3WithLabel);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.WU_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, StocktakeLineWrapper.PartAttribute3WithLabel);

			StocktakeLine.WU_PartAttrib3 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", StocktakeLineWrapper.PartAttribute3WithLabel);

			StocktakeLine.WU_PartAttrib3 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "LABEL: TEST", StocktakeLineWrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;

			var miscServ = client.MiscServ;
			StocktakeLine.WU_PartAttrib3 = "TEST";
			miscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("PartAttribute3WithLabel in English", "LABEL: TEST", StocktakeLineWrapper.PartAttribute3WithLabel);

			var resKey = miscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(miscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("PartAttribute3WithLabel in Chinese", "标签: TEST", StocktakeLineWrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			StocktakeLine.WU_SerialNumber = ZString.Empty;
			AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, StocktakeLineWrapper.TrackedSerialWithLabel);

			StocktakeLine.WU_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel is incorrect", "Tracked Serial Number: TEST", StocktakeLineWrapper.TrackedSerialWithLabel);
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			StocktakeLine.WU_Status = Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Closed;
			AssertEquals("Status must be CLOSED", Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Closed, StocktakeLineWrapper.Status);

			StocktakeLine.WU_Status = Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Open;
			AssertEquals("Status must be OPEN", Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Open, StocktakeLineWrapper.Status);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			StocktakeLine.WU_PalletID = "P_ID_001";
			AssertEquals("P_ID_001", StocktakeLineWrapper.PalletID);
		}

		#endregion

		#region TestProduct

		public void TestProduct()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "ABC123";
			StocktakeLine.WU_OP = part.PK;
			AssertEquals(part, StocktakeLineWrapper.Product.WrappedObject);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			AssertEquals("DocWrapper ProductDesc should be empty", ZString.Empty, StocktakeLineWrapper.ProductDescription);
			StocktakeLine.WU_OP = Factory.New<OrgSupplierPart>().PK;
			StocktakeLine.SupplierPart.OP_Desc = "TEST DESC";
			AssertEquals("DocWrapper Product Desc is incorrect", "TEST DESC", StocktakeLineWrapper.ProductDescription);
		}

		#endregion

		#region TestClientCode

		public void TestClientCode()
		{
			AssertEquals("DocWrapper client code should be empty", ZString.Empty, StocktakeLineWrapper.ClientCode);
			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.Client.OH_Code = "TEST CLIENT";
			AssertEquals("DocWrapper ClientCode is incorrect", "TEST CLIENT", StocktakeLineWrapper.ClientCode);
		}

		#endregion

		#region TestInventoryStatus

		public void TestInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			StocktakeLine.WU_InventoryStatus = InventoryStatus.Codes.Available;

			AssertEquals(InventoryStatus.Codes.Available, StocktakeLineWrapper.InventoryStatus);
			AssertEquals(InventoryStatus.Descriptions.Available, StocktakeLineWrapper.InventoryStatusDescription);
		}

		#endregion

		#region TestLastCount

		public void TestLastCount()
		{
			StocktakeLine.WU_LastCount = 1;
			StocktakeLine.WU_Count2 = 2;
			StocktakeLine.WU_Count3 = 3;

			StocktakeLine.WU_TotalCounts = 1;
			AssertEquals(1m, StocktakeLineWrapper.LastCount);

			StocktakeLine.WU_TotalCounts = 2;
			AssertEquals(2m, StocktakeLineWrapper.LastCount);

			StocktakeLine.WU_TotalCounts = 3;
			AssertEquals(3m, StocktakeLineWrapper.LastCount);
		}

		#endregion

		#region TestVariance

		protected override void TestVarianceCore()
		{
			StocktakeLine.WU_SystemUnits = 10;
			StocktakeLine.WU_LastCount = 1;
			StocktakeLine.WU_Count2 = 2;
			StocktakeLine.WU_Count3 = 3;

			StocktakeLine.WU_TotalCounts = 1;
			AssertEquals(-9m, StocktakeLineWrapper.Variance);

			StocktakeLine.WU_TotalCounts = 2;
			AssertEquals(-8m, StocktakeLineWrapper.Variance);

			StocktakeLine.WU_TotalCounts = 3;
			AssertEquals(-7m, StocktakeLineWrapper.Variance);
		}

		#endregion

		#region TestIsEmptyLocationCore

		protected override void TestIsEmptyLocationCore()
		{
			AssertEquals(false, StocktakeLineWrapper.IsEmptyLocation);

			StocktakeLine.WU_Status = "EMP";
			AssertEquals(true, StocktakeLineWrapper.IsEmptyLocation);
		}

		#endregion

		#region TestNoOfAttribsAndAttributePrintSizeFactor

		protected override bool CanSetProductAttributesInWrapperCore => false;

		#endregion

		#endregion

		#region Implementation

		#region Abstract members Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return StocktakeLineWrapper;
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
			return new WarehouseStocktakeLineWrapper(null, Factory);
		}

		#endregion

		protected override WarehouseGenericLineWrapper GetNewWarehouseLineWrapper(BusinessObject whsLineBO)
		{
			return new WarehouseStocktakeLineWrapper((WhsStocktakeLine)WhsLineBO, Factory);
		}

		WhsStocktakeLine stocktakeLine;

		WhsStocktakeLine StocktakeLine => stocktakeLine ?? (stocktakeLine = Factory.New<WhsStocktake>().Lines.AddNew());

		WarehouseStocktakeLineWrapper stocktakeLineWrapper;

		WarehouseStocktakeLineWrapper StocktakeLineWrapper => stocktakeLineWrapper ?? (stocktakeLineWrapper = new WarehouseStocktakeLineWrapper(StocktakeLine, Factory));

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsStocktakeLine>();
		}

		#endregion
	}
}
