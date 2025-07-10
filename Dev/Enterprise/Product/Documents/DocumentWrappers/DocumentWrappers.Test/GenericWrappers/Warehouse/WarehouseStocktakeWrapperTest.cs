using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WarehouseStocktakeWrapper))]
	sealed class WarehouseStocktakeWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestJobLinesCore

		protected override void TestJobLinesCore()
		{
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines.AddNew();
			Stocktake.Lines[1].WU_Status = StocktakeLineStatus.Codes.Closed;

			AssertEquals("Lines Collection should have count of 2 (1 of the 3 is closed)", 2, StocktakeWrapper.JobLines.Count);
		}

		#endregion

		#region TestJobLines_SortsByLocationThenByProduct

		public void TestJobLines_SortsByLocationThenByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("Warehous");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 12, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, whs);
			var listOfLines = new List<WhsStocktakeLine>();
			// Add in reverse order, ensure not sorted on the bizo
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-1-11-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-1-2-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-2-1-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-1-1-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-2-2-2"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-2-2-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-1-2-2"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-1-1-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("B-1-1-2"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("B-1-1-1"), StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available));

			// Reverse list
			listOfLines.Reverse();

			Factory.Save();

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			AssertEquals("Precondition", 10, stocktakeWrapper.JobLines.Count);
			AssertSequencesEqual("Should be sorted correctly.", listOfLines, stocktakeWrapper.JobLines.Cast<WarehouseStocktakeLineWrapper>().Select(jl => jl.WrappedObject));
		}

		#endregion

		#region TestVarianceLinesCollection

		#region TestVarianceLinesCollection

		public void TestVarianceLinesCollection()
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

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			AssertEquals("Should be 2 lines", 2, stocktakeWrapper.VarianceLines.Count);
			AssertEquals("Line 1 should have correct TrackedSerialNumber", "SER1", stocktakeWrapper.VarianceLines[0].TrackedSerialNumber);
			AssertEquals("Line 2 should have correct TrackedSerialNumber", "FFR", stocktakeWrapper.VarianceLines[1].TrackedSerialNumber);
		}

		#endregion

		#region TestVarianceLinesCollection_Count1

		public void TestVarianceLinesCollection_Count1()
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
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 2, 3, 4, 3, 2, 1, 0 }, dictionary, 1);
			dictionary.Clear();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Product");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 1, 1, 2, 2, 2, 1, 0 }, dictionary, 1);
		}

		#endregion

		#region TestVarianceLinesCollection_Count2

		public void TestVarianceLinesCollection_Count2()
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
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 2, 3, 4, 3, 2, 1, 0 }, dictionary, 2);
			dictionary.Clear();
			dictionary.Add(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Stocktake Variance By Product");
			TestVarianceLinesCollectionCore(data, new int[10] { 0, 0, 1, 1, 1, 2, 2, 2, 1, 0 }, dictionary, 2);
		}

		#endregion

		#region TestVarianceLinesCollection_Count3

		public void TestVarianceLinesCollection_Count3()
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

		#region TestVarianceLinesCollection_SortsByLocationThenByProduct

		public void TestVarianceLinesCollection_SortsByLocationThenByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateWarehouse("Warehous");
			var row1 = Helper.CreateRowAndGenerateLocations(whs, "B", 2, 2, 2, rowSequence: 1);
			var row2 = Helper.CreateRowAndGenerateLocations(whs, "C", 2, 2, 2, rowSequence: 2);
			var row3 = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 12, 2, rowSequence: 3);
			row3.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			row3.UpdatePathSequenceOnLocations();
			Factory.Save();

			var stocktake = Helper.CreateWhsStocktake(data.Org1, whs);
			var listOfLines = new List<WhsStocktakeLine>();
			// Add in reverse order, ensure not sorted on the bizo
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-1-11-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-1-2-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-2-1-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("A-1-1-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-2-2-2"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-2-2-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-1-2-2"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("C-1-1-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("B-1-1-2"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));
			listOfLines.Add(Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, whs.FindLocation("B-1-1-1"), StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available));

			foreach (var line in listOfLines)
			{
				line.WU_SystemUnits = 0m;
				line.WU_LastCount = 1m;
				line.WU_TotalCounts = 1;
			}

			// Reverse list
			listOfLines.Reverse();

			Factory.Save();

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			stocktakeWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "LOCATION" } });
			AssertEquals("Precondition", "LOCATION", StocktakeWrapper.DocumentTitle); // Wtf, why does the production code require this!?

			AssertEquals("Precondition", 10, stocktakeWrapper.VarianceLines.Count);
			AssertSequencesEqual("Should be sorted correctly.", listOfLines, stocktakeWrapper.VarianceLines.Cast<WarehouseStocktakeLineWrapper>().Select(jl => jl.WrappedObject));
		}

		#endregion

		void TestVarianceLinesCollectionCore(TestDataSimpleEnvironment data, int[] countStocktakeLines, Dictionary<string, object> dictionary, ZByte countColumnNumber)
		{
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[0].ToString(), countStocktakeLines[0], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[0].LocationString = "SSS-1-1-1";
			Stocktake.Lines[0].WU_OP = data.Part1.PK;
			Stocktake.Lines[0].WU_SystemUnits = 10;
			Stocktake.Lines[0].WU_TotalCounts = countColumnNumber;
			Stocktake.Lines[0].CurrentCount = 10;

			Stocktake.Lines[1].LocationString = "SSS-1-1-1";
			stocktake.Lines[1].WU_OP = data.Part1.PK;
			Stocktake.Lines[1].WU_SystemUnits = 99.99;
			Stocktake.Lines[1].WU_TotalCounts = countColumnNumber;
			Stocktake.Lines[1].CurrentCount = 99.99;

			Stocktake.Lines[2].LocationString = "SSS-1-1-2";
			Stocktake.Lines[2].WU_OP = data.Part1.PK;
			Stocktake.Lines[2].WU_SystemUnits = 500;
			Stocktake.Lines[2].WU_TotalCounts = countColumnNumber;
			Stocktake.Lines[2].CurrentCount = 500;

			Stocktake.Lines[3].LocationString = "SSS-1-1-2";
			Stocktake.Lines[3].WU_OP = data.Part1.PK;
			Stocktake.Lines[3].WU_SystemUnits = 100;
			SetCountValue(Stocktake.Lines[3], countColumnNumber, 100m);
			Stocktake.Lines[3].WU_Status = StocktakeLineStatus.Codes.Closed;

			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			StocktakeWrapper.SetTemplateConstants(dictionary);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[1].ToString(), countStocktakeLines[1], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[0].CurrentCount = 5;
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[2].ToString(), countStocktakeLines[2], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[1].CurrentCount = 99.98;
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[3].ToString(), countStocktakeLines[3], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[2].CurrentCount = 0;
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[4].ToString(), countStocktakeLines[4], StocktakeWrapper.VarianceLines.Count);

			SetCountValue(Stocktake.Lines[3], countColumnNumber, 95m);
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[5].ToString(), countStocktakeLines[5], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[0].CurrentCount = 10;
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[6].ToString(), countStocktakeLines[6], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[1].CurrentCount = 99.99;
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[7].ToString(), countStocktakeLines[7], StocktakeWrapper.VarianceLines.Count);

			Stocktake.Lines[2].CurrentCount = 500;
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[8].ToString(), countStocktakeLines[8], StocktakeWrapper.VarianceLines.Count);

			SetCountValue(Stocktake.Lines[3], countColumnNumber, 100m);
			StocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory);
			AssertEquals("Variance Lines Collection should have count of " + countStocktakeLines[9].ToString(), countStocktakeLines[9], StocktakeWrapper.VarianceLines.Count);
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

		#endregion

		#region TestStocktakeWrapperSave

		public void TestStocktakeWrapperSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stocktake = Helper.CreateWhsStocktake(data.Whs1, CountEmptyLocationCategory.Codes.IncludeEmptyLocations);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);

			var emptyLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);
			stocktake.Lines.Add(emptyLine);

			StocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);

			AssertEquals("Pre-condition", 3, StocktakeWrapper.StocktakeLines.Count);
			AssertNoExceptionThrown("No exception should be thrown.", () => Factory.Save());
		}

		#endregion

		#region TestStocktakeLines

		public void TestStocktakeLines()
		{
			//setup test data
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);

			//setup stocktake and lines

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var openedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Open, InventoryStatus.Codes.Available);
			var closedLine = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation, StocktakeLineStatus.Codes.Closed, InventoryStatus.Codes.Available);
			var emptyLine = Helper.CreateEmptyWhsStocktakeLine(stocktake, data.Whs1.DefaultLocation);

			StocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			AssertEquals("All stocktake lines should be returned.", 3, StocktakeWrapper.StocktakeLines.Count);
			AssertCollectionContains(openedLine, StocktakeWrapper.StocktakeLines);
			AssertCollectionContains(closedLine, StocktakeWrapper.StocktakeLines);
			AssertCollectionContains(emptyLine, StocktakeWrapper.StocktakeLines);
		}

		#endregion

		#region TestJobNumber

		protected override void TestJobNumberCore()
		{
			var stocktake = Factory.New<WhsStocktake>();
			stocktake.WS_StocktakeNumber = "WS1";

			var stocktakeWrapper = new WarehouseStocktakeWrapper(stocktake, Factory);
			AssertEquals("WS1", stocktakeWrapper.JobNumber);
		}

		#endregion

		#region Properties

		public void TestDocumentName()
		{
			StocktakeWrapper.SetTemplateConstants(new Dictionary<string, object> { { DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, "Some menu title" } });
			AssertEquals("DocumentName", "Some menu title", StocktakeWrapper.DocumentTitle);
		}

		protected override void TestStocktakeNumberCore()
		{
			Stocktake.WS_StocktakeNumber = "00000001";
			AssertEquals("00000001", StocktakeWrapper.StocktakeNumber);
		}

		protected override void TestWarehouseCore()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			Stocktake.WS_WW_Whs = whs.PK;
			AssertEquals("TEST WAREHOUSE", StocktakeWrapper.Warehouse.Name);
		}

		protected override void TestSelectedCycleCore()
		{
			Stocktake.WS_StocktakeCycle = "XXX";
			AssertEquals("XXX", StocktakeWrapper.SelectedCycle);
		}

		protected override void TestSelectedClientCore()
		{
			OrgHeader client = Helper.CreateClient("TEST CLIENT");
			Stocktake.WS_OH_Client = client.PK;
			AssertEquals("TEST CLIENT", StocktakeWrapper.SelectedClient);
		}

		protected override void TestSelectedSupplierPartCore()
		{
			var client = Helper.CreateClient("TEST CLIENT");
			var product1 = Helper.CreateProduct(client, "TEST PRODUCT");
			var product2 = Helper.CreateProduct(client, "ANOTHER PRODUCT");
			Helper.CreateWhsStocktakeProductFilter(Stocktake, product1);
			AssertEquals("TEST PRODUCT", StocktakeWrapper.SelectedSupplierPart);
			Helper.CreateWhsStocktakeProductFilter(Stocktake, product2);
			AssertEquals("Many", StocktakeWrapper.SelectedSupplierPart);
		}

		protected override void TestSelectedCommodityCodeCore()
		{
			Stocktake.WS_RH_NKCommodityCode = "COM1";
			AssertEquals("COM1", StocktakeWrapper.SelectedCommodityCode);
		}

		protected override void TestSelectedPickMethodCore()
		{
			Stocktake.WS_PickMethod = "AAA";
			AssertEquals("AAA", StocktakeWrapper.SelectedPickMethod);
		}

		protected override void TestSelectedRowCore()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			WhsRow row = Helper.CreateRow(whs, "ROW1");
			Stocktake.WS_WR_Row = row.PK;
			AssertEquals("ROW1", StocktakeWrapper.SelectedRow);
		}

		protected override void TestSecondaryHeadingCore()
		{
			AssertEquals("Stocktake Line Details", StocktakeWrapper.SecondaryHeading);
		}

		protected override void TestSelectedAreaCore()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			WhsArea area = Helper.CreateArea(whs, "AREA1", "FRE");
			Stocktake.WS_WA_Area = area.PK;
			AssertEquals("AREA1", StocktakeWrapper.SelectedArea);
		}

		public void TestSelectedArea_Translatable()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var area = Helper.CreateArea(whs, "AREA1", "FRE");
			Stocktake.WS_WA_Area = area.PK;
			AssertEquals("SelectedArea in English.", "AREA1", StocktakeWrapper.SelectedArea);

			var resKey = area.WA_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(area, "AREA1").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "区域1"));
				AssertEquals("SelectedArea in Chinese.", "区域1", StocktakeWrapper.SelectedArea);
			}
		}

		protected override void TestPrimaryBarcodeTextCore()
		{
			var barcode = new TextBarcode("SK000001");
			Stocktake.WS_StocktakeNumber = barcode.TextToEncode;
			AssertEquals(barcode.TextAs128sFontString, StocktakeWrapper.PrimaryBarcodeText);
		}

		protected override void TestSelectedABCCategoryCore()
		{
			Stocktake.WS_ABCAnalysisCategory = "A1";
			AssertEquals("A1", StocktakeWrapper.SelectedABCCategory);
		}

		protected override void TestSelectedLocationCore()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE", "A", 1, 2);
			Factory.Save();

			AssertEquals("", StocktakeWrapper.SelectedLocation);

			Stocktake.WS_WL_Location = whs.FindLocation("A-1-1").PK;
			AssertEquals("A-1-1", StocktakeWrapper.SelectedLocation);
		}

		protected override void TestSelectedStocktakeTypeCore()
		{
			Stocktake.WS_StocktakeType = "T1";
			AssertEquals("T1", StocktakeWrapper.SelectedStocktakeType);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		protected override void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
			((WhsStocktake)bizO).WS_WW_Whs = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsStocktake>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WarehouseStocktakeWrapper((WhsStocktake)bizO, factory);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsStocktake>();
		}

		WarehouseStocktakeWrapper StocktakeWrapper
		{
			get => stocktakeWrapper ?? (stocktakeWrapper = new WarehouseStocktakeWrapper(Stocktake, Factory));
			set => stocktakeWrapper = value;
		}
		WarehouseStocktakeWrapper stocktakeWrapper;

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WarehouseStocktakeWrapper((WhsStocktake)bizO, Factory);
		}

		WhsStocktake Stocktake => stocktake ?? (stocktake = (WhsStocktake)GetNewWhsBusinessObject());
		WhsStocktake stocktake;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var stocktake = Factory.NewWithValidTestData<WhsStocktake>();
			return new WarehouseStocktakeWrapper(stocktake, Factory);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = new WarehouseStocktakeWrapper(null, Factory);
			AssertEquals("SecondaryHeading", "Stocktake Line Details", emptyWrapper.SecondaryHeading);
			AssertEquals("BOLNumber", "", emptyWrapper.BOLNumber);
			AssertNull("CarrierServiceLevel", emptyWrapper.CarrierServiceLevel);
			AssertEquals("CartageAdviceClosingText", "", emptyWrapper.CartageAdviceClosingText);
			AssertEquals("CartageAdviceOpeningText", "", emptyWrapper.CartageAdviceOpeningText);
			AssertEquals("CartageDropMode", "", emptyWrapper.CartageDropMode);
			AssertNull("Client", emptyWrapper.Client);
			AssertNull("CODAmount", emptyWrapper.CODAmount);
			AssertNull("CODType", emptyWrapper.CODType);
			AssertNull("Consignee", emptyWrapper.Consignee);
			AssertNull("ConsigneeAddress", emptyWrapper.ConsigneeAddress);
			AssertEquals("ConsolidatedInvoiceRef", "", emptyWrapper.ConsolidatedInvoiceRef);
			AssertEquals("ContainerNumberAndTypeLine", "", emptyWrapper.ContainerNumberAndTypeLine);
			Assert("CustomerReference", emptyWrapper.CustomerReference.IsEmpty);
			AssertNull("Destination", emptyWrapper.Destination);
			AssertNull("DropMode", emptyWrapper.DropMode);
			Assert("FinalisedDate", emptyWrapper.FinalisedDate.IsEmpty);
			AssertNull("FulfillRule", emptyWrapper.FulfillRule);
			AssertNull("GoodsBillToAddress", emptyWrapper.GoodsBillToAddress);
			Assert("HandlingInstructions", emptyWrapper.HandlingInstructions.IsEmpty);
			AssertNull("IncoTerm", emptyWrapper.IncoTerm);
			AssertNull("Insurance", emptyWrapper.Insurance);
			AssertEquals("InvoiceNumber", "", emptyWrapper.InvoiceNumber);
			AssertNull("JobClient", emptyWrapper.JobClient);
			AssertEquals("JobNumber", "", emptyWrapper.JobNumber);
			Assert("PickingInstructions", emptyWrapper.PickingInstructions.IsEmpty);
			Assert("PickMethod", emptyWrapper.PickMethod.IsEmpty);
			Assert("PickNo", emptyWrapper.PickNo.IsEmpty);
			Assert("PickNumberReference", emptyWrapper.PickNumberReference.IsEmpty);
			AssertNull("", emptyWrapper.PickOption);
			Assert("PrimaryBarcode", emptyWrapper.PrimaryBarcode.IsEmpty);
			AssertEquals("References", "", emptyWrapper.References);
			Assert("RequiredDate", emptyWrapper.RequiredDate.IsEmpty);
			AssertEquals("SecondaryNumber", "", emptyWrapper.SecondaryNumber);
			Assert("SecondaryReference", emptyWrapper.SecondaryReference.IsEmpty);
			Assert("Status", emptyWrapper.Status.IsEmpty);
			AssertEquals("TotalNumberOfLabels", 0, emptyWrapper.TotalNumberOfLabels);
			AssertEquals("TotalNumberOfPackageLabels", 0, emptyWrapper.TotalNumberOfPackageLabels);
			AssertNull("TransportBillToAddress", emptyWrapper.TransportBillToAddress);
			AssertNull("TransportCoAddress", emptyWrapper.TransportCoAddress);
			AssertNull("TransportCompany", emptyWrapper.TransportCompany);
			Assert("TransportReference", emptyWrapper.TransportReference.IsEmpty);
			AssertNull("Warehouse", emptyWrapper.Warehouse);
			AssertEquals("WarehouseCartageCoordinatorName", "", emptyWrapper.WarehouseCartageCoordinatorName);
			AssertEquals("WarehouseCartageCoordinatorPhone", "", emptyWrapper.WarehouseCartageCoordinatorPhone);
			Assert("WhoCreated", emptyWrapper.WhoCreated.IsEmpty);
			Assert("WhoFinalised", emptyWrapper.WhoFinalised.IsEmpty);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor :  is null
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode :  is null
DropOffAddress :  is null
FinalisedDate : 
Forwarder :  is null
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient :  is null
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress :  is null
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel :  is null
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : 
Supplier :  is null
SupplierBuyerLink :  is null
SupplierDocAddress :  is null
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress :  is null
TransportCompany :  is null
TransportReference : 
VehicleReference : 
Warehouse : 
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
";
			}
		}

		#endregion
	}
}
