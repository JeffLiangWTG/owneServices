using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsStocktakeLine))]
	sealed class DocWhsStocktakeLineTest : DocumentWrapperTestCase
	{
		#region TestClientCode

		public void TestClientCode()
		{
			AssertEquals("DocWrapper client code should be empty", ZString.Empty, DocWrapper.ClientCode);
			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.Client.OH_Code = "TEST CLIENT";
			AssertEquals("DocWrapper ClientCode is incorrect", "TEST CLIENT", DocWrapper.ClientCode);
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			AssertEquals("DocWrapper ProductCode should be empty", ZString.Empty, DocWrapper.ProductCode);
			StocktakeLine.WU_OP = Factory.New<OrgSupplierPart>().PK;
			StocktakeLine.SupplierPart.OP_PartNum = "TEST CODE";
			AssertEquals("DocWrapper ProductCode is incorrect", "TEST CODE", DocWrapper.ProductCode);
		}

		#endregion

		#region TestProductDesc

		public void TestProductDesc()
		{
			AssertEquals("DocWrapper ProductDesc should be empty", ZString.Empty, DocWrapper.ProductDesc);
			StocktakeLine.WU_OP = Factory.New<OrgSupplierPart>().PK;
			StocktakeLine.SupplierPart.OP_Desc = "TEST DESC";
			AssertEquals("DocWrapper Product Desc is incorrect", "TEST DESC", DocWrapper.ProductDesc);
		}

		#endregion

		#region TestProductBrandName

		public void TestProductBrandName()
		{
			AssertEquals("DocWrapper ProductBrandName should be empty", ZString.Empty, DocWrapper.ProductBrandName);
			StocktakeLine.WU_OP = Factory.New<OrgSupplierPart>().PK;
			StocktakeLine.SupplierPart.OP_Brand = "TEST BRAND";
			AssertEquals("DocWrapper ProductBrandName is incorrect", "TEST BRAND", DocWrapper.ProductBrandName);
		}

		#endregion

		#region TestProductModel

		public void TestProductModel()
		{
			AssertEquals("DocWrapper ProductModel should be empty", ZString.Empty, DocWrapper.ProductModel);
			StocktakeLine.WU_OP = Factory.New<OrgSupplierPart>().PK;
			StocktakeLine.SupplierPart.OP_Model = "TEST MODEL";
			AssertEquals("DocWrapper ProductModel is incorrect", "TEST MODEL", DocWrapper.ProductModel);
		}

		#endregion

		#region TestLocationString

		public void TestLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var locations = Helper.CreateRowAndGenerateLocations(whs, "AA", 1, 1, 2).Locations;
			StocktakeLine.WU_WL = locations[0].PK;
			AssertEquals("DocWrapper LocationString is incorrect", "AA-1-1-1", DocWrapper.LocationString);
		}

		#endregion

		#region TestPackingDateWithLabel

		public void TestPackingDateWithLabel()
		{
			var testDate = ZDate.Today;
			StocktakeLine.WU_PackingDate = testDate;
			AssertEquals("DocWrapper PackingDateWithLabel is incorrect", "Packing Date: " + testDate.ToShortDateString(), DocWrapper.PackingDateWithLabel);
		}

		#endregion

		#region TestExpiryDateWithLabel

		public void TestExpiryDateWithLabel()
		{
			var testDate = ZDate.Today;
			StocktakeLine.WU_ExpiryDate = testDate;
			AssertEquals("DocWrapper ExpiryDateWithLabel is incorrect", "Expiry Date: " + testDate.ToShortDateString(), DocWrapper.ExpiryDateWithLabel);
		}

		#endregion

		#region TestPartAttribute1

		public void TestPartAttribute1()
		{
			StocktakeLine.WU_PartAttrib1 = "TEST";
			AssertEquals("TEST", DocWrapper.PartAttribute1);
		}

		#endregion

		#region TestPartAttribute1Name

		public void TestPartAttribute1Name()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute1Name should be empty", ZString.Empty, DocWrapper.PartAttribute1Name);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib1Name = "TEST";
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "TEST", DocWrapper.PartAttribute1Name);
		}

		public void TestPartAttribute1Name_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;
			client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1Name in English", "LABEL", DocWrapper.PartAttribute1Name);

			var resKey = client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute1Name in English", "标签", DocWrapper.PartAttribute1Name);
			}
		}

		#endregion

		#region TestPartAttribute1WithLabel

		public void TestPartAttribute1WithLabel()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute1WithLabel);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.WU_PartAttrib1 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute1WithLabel);

			StocktakeLine.WU_PartAttrib1 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "Attribute 1: TEST", DocWrapper.PartAttribute1WithLabel);

			StocktakeLine.WU_PartAttrib1 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute1WithLabel);
		}

		public void TestPartAttribute1WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;
			StocktakeLine.WU_PartAttrib1 = "TEST";
			client.MiscServ.OM_IMPartAttrib1Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute1WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute1WithLabel);

			var resKey = client.MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute1WithLabel in English", "标签: TEST", DocWrapper.PartAttribute1WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute2WithLabel

		public void TestPartAttribute2WithLabel()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.WU_PartAttrib2 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute2WithLabel);

			StocktakeLine.WU_PartAttrib2 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "Attribute 2: TEST", DocWrapper.PartAttribute2WithLabel);

			StocktakeLine.WU_PartAttrib2 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);
		}

		public void TestPartAttribute2WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;
			StocktakeLine.WU_PartAttrib2 = "TEST";
			client.MiscServ.OM_IMPartAttrib2Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute2WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute2WithLabel);

			var resKey = client.MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute2WithLabel in English", "标签: TEST", DocWrapper.PartAttribute2WithLabel);
			}
		}

		#endregion

		#region TestPartAttribute3WithLabel

		public void TestPartAttribute3WithLabel()
		{
			StocktakeLine.WU_OH_Client = ZGuid.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			StocktakeLine.WU_OH_Client = Factory.New<OrgHeader>().PK;
			StocktakeLine.WU_PartAttrib3 = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel should be empty", ZString.Empty, DocWrapper.PartAttribute3WithLabel);

			StocktakeLine.WU_PartAttrib3 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "Attribute 3: TEST", DocWrapper.PartAttribute3WithLabel);

			StocktakeLine.WU_PartAttrib3 = "TEST";
			StocktakeLine.Client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel is incorrect", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);
		}

		public void TestPartAttribute3WithLabel_Translatable()
		{
			var client = Factory.New<OrgHeader>();
			StocktakeLine.WU_OH_Client = client.PK;
			StocktakeLine.WU_PartAttrib3 = "TEST";
			client.MiscServ.OM_IMPartAttrib3Name = "LABEL";
			AssertEquals("DocWrapper PartAttribute3WithLabel in English", "LABEL: TEST", DocWrapper.PartAttribute3WithLabel);

			var resKey = client.MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(client.MiscServ, "LABEL").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "标签"));
				AssertEquals("DocWrapper PartAttribute3WithLabel in English", "标签: TEST", DocWrapper.PartAttribute3WithLabel);
			}
		}

		#endregion

		#region TestTrackedSerialNumber

		public void TestTrackedSerialNumber()
		{
			StocktakeLine.WU_SerialNumber = "TEST";
			AssertEquals("TEST", DocWrapper.TrackedSerialNumber);
		}

		public void TestTrackedSerialNumberName()
		{
			AssertEquals("DocWrapper PartAttribute1Name is incorrect", "Tracked Serial Number", DocWrapper.TrackedSerialNumberName);
		}

		#endregion

		#region TestTrackedSerialWithLabel

		public void TestTrackedSerialWithLabel()
		{
			AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

			StocktakeLine.WU_SerialNumber = ZString.Empty;
			AssertEquals("DocWrapper TrackedSerialWithLabel should be empty", ZString.Empty, DocWrapper.TrackedSerialWithLabel);

			StocktakeLine.WU_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel is incorrect", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);
		}

		public void TestTrackedSerialWithLabel_Translatable()
		{
			StocktakeLine.WU_SerialNumber = "TEST";
			AssertEquals("DocWrapper TrackedSerialWithLabel in English", "Tracked Serial Number: TEST", DocWrapper.TrackedSerialWithLabel);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", new ResourceStringData("73b09e17-dc63-4fbf-9eb9-80e72a2f7892", "跟踪序列号"));
				AssertEquals("DocWrapper TrackedSerialWithLabel in Chinese", "跟踪序列号: TEST", DocWrapper.TrackedSerialWithLabel);
			}
		}

		#endregion

		#region TestStatus

		public void TestStatus()
		{
			StocktakeLine.WU_Status = Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Closed;
			AssertEquals("Status must be CLOSED", Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Closed, DocWrapper.Status);

			StocktakeLine.WU_Status = Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Open;
			AssertEquals("Status must be OPEN", Enterprise.Warehouse.Transactions.CodeLists.StocktakeLineStatus.Codes.Open, DocWrapper.Status);
		}

		#endregion

		#region TestPalletID

		public void TestPalletID()
		{
			StocktakeLine.WU_PalletID = "P_ID_001";
			AssertEquals("P_ID_001", DocWrapper.PalletID);
		}

		#endregion

		#region TestLastCount

		public void TestLastCount()
		{
			StocktakeLine.WU_LastCount = 1;
			StocktakeLine.WU_Count2 = 2;
			StocktakeLine.WU_Count3 = 3;

			StocktakeLine.WU_TotalCounts = 1;
			AssertEquals(1m, DocWrapper.LastCount);

			StocktakeLine.WU_TotalCounts = 2;
			AssertEquals(2m, DocWrapper.LastCount);

			StocktakeLine.WU_TotalCounts = 3;
			AssertEquals(3m, DocWrapper.LastCount);
		}

		#endregion

		#region TestVariance

		public void TestVariance()
		{
			StocktakeLine.WU_SystemUnits = 10;
			StocktakeLine.WU_LastCount = 1;
			StocktakeLine.WU_Count2 = 2;
			StocktakeLine.WU_Count3 = 3;

			StocktakeLine.WU_TotalCounts = 1;
			AssertEquals(-9m, DocWrapper.Variance);

			StocktakeLine.WU_TotalCounts = 2;
			AssertEquals(-8m, DocWrapper.Variance);

			StocktakeLine.WU_TotalCounts = 3;
			AssertEquals(-7m, DocWrapper.Variance);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		protected override void SetUp()
		{
			var stocktake = Factory.New<WhsStocktake>();
			StocktakeLine = stocktake.Lines.AddNew();
			DocWrapper = DocWhsStocktakeLine.New(StocktakeLine, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsStocktakeLine StocktakeLine;
		DocWhsStocktakeLine DocWrapper;

		#endregion
	}
}
