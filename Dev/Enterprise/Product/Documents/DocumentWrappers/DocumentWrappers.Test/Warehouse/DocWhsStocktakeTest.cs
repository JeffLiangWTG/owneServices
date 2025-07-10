using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Warehouse
{
	[TestedType(typeof(DocWhsStocktake))]
	sealed class DocWhsStocktakeTest : DocumentWrapperTestCase
	{
		#region TestLinesCollection

		public void TestLinesCollection()
		{
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines[1].WU_Status = StocktakeLineStatus.Codes.Closed;

			AssertEquals("Lines Collection should have count of 2 (1 of the 3 is closed)", 2, DocWrapper.Lines.Count);
		}

		#endregion

		#region TestVarianceLinesCollectionCore

		#region TestVarianceLinesCollectionCore_Count1

		public void TestVarianceLinesCollectionCore_Count1()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Helper.CreateRowAndGenerateLocations(data.Whs1, "SSS", 1, 1, 2);

			Factory.Save();

			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Location");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 2, 3, 4, 3, 2, 1, 0 }, dictionary, 1);
			dictionary.Clear();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Product");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 1, 1, 2, 2, 2, 1, 0 }, dictionary, 1);
		}

		#endregion

		#region TestVarianceLinesCollectionCore_Count2

		public void TestVarianceLinesCollectionCore_Count2()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Helper.CreateRowAndGenerateLocations(data.Whs1, "SSS", 1, 1, 2);

			Factory.Save();

			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Location");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 2, 3, 4, 3, 2, 1, 0 }, dictionary, 2);
			dictionary.Clear();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Product");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 1, 1, 2, 2, 2, 1, 0 }, dictionary, 2);
		}

		#endregion

		#region TestVarianceLinesCollectionCore_Count3

		public void TestVarianceLinesCollectionCore_Count3()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Stocktake.WS_OH_Client = data.Org1.PK;
			Stocktake.WS_WW_Whs = data.Whs1.PK;
			Helper.CreateRowAndGenerateLocations(data.Whs1, "SSS", 1, 1, 2);

			Factory.Save();

			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			var dictionary = new Dictionary<string, object>();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Location");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 2, 3, 4, 3, 2, 1, 0 }, dictionary, 3);
			dictionary.Clear();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Product");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 1, 1, 2, 2, 2, 1, 0 }, dictionary, 3);
		}

		#endregion

		void TestVarianceLinesCollectionCore(TestDataSimpleEnvironment data, int[] countStocktakeLines, Dictionary<string, object> dictionary, ZByte countColumnNumber)
		{
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[0].ToString(), countStocktakeLines[0], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[0].WU_OP = data.Part1.PK;
			Stocktake.Lines[0].LocationString = "SSS-1-1-1";
			Stocktake.Lines[0].WU_SystemUnits = 10;
			Stocktake.Lines[0].WU_TotalCounts = countColumnNumber;
			Stocktake.Lines[0].CurrentCount = 10;

			Stocktake.Lines[1].WU_OP = data.Part1.PK;
			Stocktake.Lines[1].LocationString = "SSS-1-1-1";
			Stocktake.Lines[1].WU_SystemUnits = 99.99;
			Stocktake.Lines[1].WU_TotalCounts = countColumnNumber;
			Stocktake.Lines[1].CurrentCount = 99.99;

			Stocktake.Lines[2].WU_OP = data.Part1.PK;
			Stocktake.Lines[2].LocationString = "SSS-1-1-2";
			Stocktake.Lines[2].WU_SystemUnits = 500;
			Stocktake.Lines[2].WU_TotalCounts = countColumnNumber;
			Stocktake.Lines[2].CurrentCount = 500;

			Stocktake.Lines[3].WU_OP = data.Part1.PK;
			Stocktake.Lines[3].LocationString = "SSS-1-1-2";
			Stocktake.Lines[3].WU_SystemUnits = 100;
			SetCountValue(Stocktake.Lines[3], countColumnNumber, 100m);
			Stocktake.Lines[3].WU_Status = StocktakeLineStatus.Codes.Closed;

			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			DocWrapper.SetTemplateConstants(dictionary);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[1].ToString(), countStocktakeLines[1], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[0].CurrentCount = 5;
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[2].ToString(), countStocktakeLines[2], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[1].CurrentCount = 99.98;
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[3].ToString(), countStocktakeLines[3], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[2].CurrentCount = 0;
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[4].ToString(), countStocktakeLines[4], DocWrapper.VarianceLines.Count);

			SetCountValue(Stocktake.Lines[3], countColumnNumber, 95m);
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[5].ToString(), countStocktakeLines[5], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[0].CurrentCount = 10;
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[6].ToString(), countStocktakeLines[6], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[1].CurrentCount = 99.99;
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[7].ToString(), countStocktakeLines[7], DocWrapper.VarianceLines.Count);

			Stocktake.Lines[2].CurrentCount = 500;
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[8].ToString(), countStocktakeLines[8], DocWrapper.VarianceLines.Count);

			SetCountValue(Stocktake.Lines[3], countColumnNumber, 100m);
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[9].ToString(), countStocktakeLines[9], DocWrapper.VarianceLines.Count);
		}

		void SetCountValue(WhsStocktakeLine line, ZByte countColumnNumber, ZDecimal countValue)
		{
			line.WU_TotalCounts = countColumnNumber;
			switch (countColumnNumber)
			{
				case 1:
					line.WU_LastCount = countValue;
					break;
				case 2:
					line.WU_Count2 = countValue;
					break;
				case 3:
					line.WU_Count3 = countValue;
					break;
				default:
					throw new NotImplementedException("Count column number not implemented.");
			}
		}

		#region TestVarianceLinesCollection_SerialNumber

		public void TestVarianceLinesCollection_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var line3 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);

			line1.WU_TotalCounts = 1;
			line1.WU_LastCount = 1;
			line1.WU_SerialNumber = "SER1";

			line2.WU_TotalCounts = 1;
			line2.WU_LastCount = 1;
			line2.WU_SerialNumber = "FFR";

			line3.WU_TotalCounts = 1;
			line3.WU_LastCount = 1;
			line3.WU_SerialNumber = "SER1";

			Factory.Save();

			var stocktakeWrapper = DocWhsStocktake.New(stocktake, Factory);
			AssertEquals("Should be 2 lines", 2, stocktakeWrapper.VarianceLines.Count);
			AssertEquals("Line 1 should have correct TrackedSerialNumber", "SER1", stocktakeWrapper.VarianceLines[0].TrackedSerialNumber);
			AssertEquals("Line 2 should have correct TrackedSerialNumber", "FFR", stocktakeWrapper.VarianceLines[1].TrackedSerialNumber);
		}

		#endregion

		#endregion

		#region TestWarehouseName

		public void TestWarehouseName()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			Stocktake.WS_WW_Whs = whs.PK;
			AssertEquals("TEST WAREHOUSE", DocWrapper.WarehouseName);
		}

		public void TestWarehouseName_Translatable()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			Stocktake.WS_WW_Whs = whs.PK;
			AssertEquals("WarehouseName in English", "TEST WAREHOUSE", DocWrapper.WarehouseName);

			var resKey = whs.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(whs, "TEST WAREHOUSE").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("WarehouseName in Chinese", "测试仓库", DocWrapper.WarehouseName);
			}
		}

		#endregion

		#region TestWarehouseNameAndAddress

		public void TestWarehouseNameAndAddress()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			whs.WW_OA_WarehouseAddress = address1.PK;
			Stocktake.WS_WW_Whs = whs.PK;
			AssertEquals("TEST WAREHOUSE", DocWrapper.WarehouseName);

			ZString expectedResult = "TEST WAREHOUSE\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";

			AssertEquals(expectedResult, DocWrapper.WarehouseNameAndAddress);
		}

		public void TestWarehouseNameAndAddress_Translatable()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "Address Line 1";
			address1.OA_Address2 = "Address Line 2";
			address1.OA_City = "SYDNEY";
			address1.OA_PostCode = "2000";

			whs.WW_OA_WarehouseAddress = address1.PK;
			Stocktake.WS_WW_Whs = whs.PK;
			AssertEquals("TEST WAREHOUSE", DocWrapper.WarehouseName);

			var expectedResult = "TEST WAREHOUSE\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";
			AssertEquals("WarehouesNameAndAddress in English", expectedResult, DocWrapper.WarehouseNameAndAddress);

			var resKey = whs.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(whs, "TEST WAREHOUSE").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				expectedResult = "测试仓库\r\nHEADER\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000";
				AssertEquals("WarehouesNameAndAddress in English", expectedResult, DocWrapper.WarehouseNameAndAddress);
			}
		}

		#endregion

		#region TestClientNameAndAddress

		public void TestClientNameAndAddress()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_FullName = "TEST CLIENT";
			client.OH_Code = "TESCLISYD";

			var address = client.MainAddress;
			address.OA_Address1 = "Address Line 1";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SYDNEY";
			address.OA_PostCode = "2000";

			Stocktake.WS_OH_Client = client.PK;

			ZString expectedResult = "TEST CLIENT\nADDRESS LINE 1\nADDRESS LINE 2\nSYDNEY 2000\n" + Env.CurrentCompany.Country.Description.ToUpper();

			AssertEquals(expectedResult, DocWrapper.ClientNameAndAddress);
		}

		#endregion

		#region TestStocktakeNumber

		public void TestStocktakeNumber()
		{
			Stocktake.WS_StocktakeNumber = "00000001";
			AssertEquals("00000001", DocWrapper.StocktakeNumber);
		}

		#endregion

		#region TestSelectedClient

		public void TestSelectedClient()
		{
			var client = Helper.CreateClient("TEST CLIENT");
			Stocktake.WS_OH_Client = client.PK;
			AssertEquals("TEST CLIENT", DocWrapper.SelectedClient);
		}

		#endregion

		#region TestSelectedCycle

		public void TestSelectedCycle()
		{
			Stocktake.WS_StocktakeCycle = "XXX";
			AssertEquals("XXX", DocWrapper.SelectedCycle);
		}

		#endregion

		#region TestSelectedSupplierPart

		public void TestSelectedSupplierPart()
		{
			var client = Helper.CreateClient("TEST CLIENT");
			var product1 = Helper.CreateProduct(client, "TEST PRODUCT");
			var product2 = Helper.CreateProduct(client, "ANOTHER PRODUCT");
			Helper.CreateWhsStocktakeProductFilter(Stocktake, product1);
			AssertEquals("TEST PRODUCT", DocWrapper.SelectedSupplierPart);
			Helper.CreateWhsStocktakeProductFilter(Stocktake, product2);
			AssertEquals("Many", DocWrapper.SelectedSupplierPart);
		}

		#endregion

		#region TestSelectedCommodityCode

		public void TestSelectedCommodityCode()
		{
			Stocktake.WS_RH_NKCommodityCode = "COM1";
			AssertEquals("COM1", DocWrapper.SelectedCommodityCode);
		}

		#endregion

		#region TestSelectedPickMethod

		public void TestSelectedPickMethod()
		{
			Stocktake.WS_PickMethod = "AAA";
			AssertEquals("AAA", DocWrapper.SelectedPickMethod);
		}

		#endregion

		#region TestSelectedRow

		public void TestSelectedRow()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var row = Helper.CreateRow(whs, "ROW1");
			Stocktake.WS_WR_Row = row.PK;
			AssertEquals("ROW1", DocWrapper.SelectedRow);
		}

		#endregion

		#region TestSelectedArea

		public void TestSelectedArea()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var area = Helper.CreateArea(whs, "AREA1", "FRE");
			Stocktake.WS_WA_Area = area.PK;
			AssertEquals("AREA1", DocWrapper.SelectedArea);
		}

		public void TestSelectedArea_Translatable()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var area = Helper.CreateArea(whs, "AREA1", "FRE");
			Stocktake.WS_WA_Area = area.PK;
			AssertEquals("SelectedArea in English", "AREA1", DocWrapper.SelectedArea);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "AREA1").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "库区1"));
				AssertEquals("SelectedArea in Chinese", "库区1", DocWrapper.SelectedArea);
			}
		}

		#endregion

		#region TestDocumentName

		public void TestDocumentName()
		{
			DocWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Some menu title" } });
			AssertEquals("DocumentName", "Some menu title", DocWrapper.DocumentName);
		}

		#endregion

		#region TestStocktakeWrapperSave

		public void TestStocktakeWrapperSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Whs1, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);

			var emptyLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);

			AssertEquals("Pre-condition", 3, stocktakeWrapper.StocktakeLines.Count);
			AssertNoExceptionThrown("No exception should be thrown.", () => Factory.Save());
		}

		#endregion

		#region TestStocktakeLines

		public void TestStocktakeLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var openedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var emptyLine1 = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.FindLocation("A-1-1"));
			var emptyLine2 = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.FindLocation("A-1-2"));

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			AssertEquals("All stocktake lines should be returned.", 4, stocktakeWrapper.StocktakeLines.Count);
			AssertCollectionContains(openedLine, stocktakeWrapper.StocktakeLines);
			AssertCollectionContains(closedLine, stocktakeWrapper.StocktakeLines);
			AssertCollectionContains(emptyLine1, stocktakeWrapper.StocktakeLines);
			AssertCollectionContains(emptyLine2, stocktakeWrapper.StocktakeLines);
		}

		#endregion

		#region TestVariainceLinesCollection

		public void TestVariainceLinesCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var line2 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var line3 = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			line1.WU_TotalCounts = 1;
			line1.WU_LastCount = 1;
			line2.WU_TotalCounts = 1;
			line2.WU_LastCount = 1;
			line3.WU_TotalCounts = 1;
			line3.WU_LastCount = 1;

			Factory.Save();

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			AssertEquals(2, stocktakeWrapper.VarianceLines.Count);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Helper = new WhsTestHelperFunctions(Factory);
			Stocktake = Factory.New<WhsStocktake>();
			DocWrapper = DocWhsStocktake.New(Stocktake, Factory);
			AssertNotNull("Wrapper not null", DocWrapper);
			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocWrapper };
		}

		WhsStocktake Stocktake;
		DocWhsStocktake DocWrapper;
		WhsTestHelperFunctions Helper;

		#endregion
	}
}
